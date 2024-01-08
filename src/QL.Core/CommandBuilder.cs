using System.Text;
using System.Text.RegularExpressions;

namespace QL.Core;

public class CommandBuilder
{
    private readonly StringBuilder _scriptBuilder = new();
    private readonly List<string> _currentCommand = [];
    private bool _isConcatenatingCommands = false;
    private bool _isInForLoop = false;
    private bool _isInIfElseBlock = false;

    public CommandBuilder StartBlock(string blockStart)
    {
        FlushCurrentCommand();
        _scriptBuilder.AppendLine(blockStart);
        return this;
    }

    public CommandBuilder EndBlock(string blockEnd)
    {
        // Ensure there is a newline before ending the block
        if (_currentCommand.Count > 0)
        {
            _scriptBuilder.AppendLine(string.Join(" ", _currentCommand) + ";");
            _currentCommand.Clear();
        }
        _scriptBuilder.AppendLine(blockEnd);
        return this;
    }

    public CommandBuilder AddCommand(string command)
    {
        FlushCurrentCommand();
        _currentCommand.Add(command);
        return this;
    }
    
    public CommandBuilder AddConcatenatedCommand(string command)
    {
        if (_isConcatenatingCommands)
        {
            _currentCommand.Add("&&");
        }

        _currentCommand.Add(command);
        _isConcatenatingCommands = true;
        return this;
    }

    public CommandBuilder AddArgument(string argument)
    {
        _currentCommand.Add(WrapArgument(argument));
        return this;
    }

    public CommandBuilder AddVariable(string variableName, string value)
    {
        FlushCurrentCommand();
        _scriptBuilder.AppendLine($"{variableName}=\"{EscapeArgument(value)}\"");
        return this;
    }

    public CommandBuilder If(string condition)
    {
        _isInIfElseBlock = true; // Set the flag when starting an if block
        FlushCurrentCommand();
        _scriptBuilder.AppendLine($"if {condition}; then");
        return this;
    }

    public CommandBuilder Else()
    {
        FlushCurrentCommand();
        _scriptBuilder.AppendLine("else");
        return this;
    }
    
    public CommandBuilder Elif(string condition)
    {
        // Ensure the current command is flushed before starting the elif block
        FlushCurrentCommand();
        _scriptBuilder.AppendLine($"elif {condition}; then");
        return this;
    }

    public CommandBuilder EndIf()
    {
        FlushCurrentCommand(true); // Flush with a forced newline
        _scriptBuilder.AppendLine("fi;");
        _isInIfElseBlock = false; // Reset the flag when ending an if block
        return this;
    }

    public CommandBuilder ForLoop(string variable, string range)
    {
        _isInForLoop = true;
        return StartBlock($"for {variable} in {range}; do");
    }

    public CommandBuilder EndFor()
    {
        _isInForLoop = false;
        return EndBlock("done");
    }

    public string Build()
    {
        FlushCurrentCommand();
        return _scriptBuilder.ToString();
    }

    public CommandBuilder EndStatement()
    {
        if (_currentCommand.Count > 0)
        {
            _scriptBuilder.AppendLine(string.Join(" ", _currentCommand) + ";");
            _currentCommand.Clear();
        }
        else
        {
            _scriptBuilder.AppendLine(";");
        }
        return this;
    }

    private void FlushCurrentCommand(bool forceNewLine = false)
    {
        if (_currentCommand.Count <= 0) return;

        var command = string.Join(" ", _currentCommand);

        if (_isConcatenatingCommands)
        {
            _scriptBuilder.Append(command);
            _isConcatenatingCommands = false;
        }
        else
        {
            if (_isInIfElseBlock || _isInForLoop || forceNewLine)
            {
                _scriptBuilder.AppendLine(command + ";");
            }
            else
            {
                _scriptBuilder.Append(command);
            }
        }

        _currentCommand.Clear();
    }
    
    private static string WrapArgument(string argument)
    {
        // Check if the argument is a variable or needs special handling
        return IsVariable(argument) ?
            // Wrap variable in quotes
            $"\"{argument}\"" :
            // Use the existing EscapeArgument method for other cases
            EscapeArgument(argument);
    }
    
    private static bool IsVariable(string argument)
    {
        // A simple check for shell variables (starts with $)
        // This might need to be more sophisticated for complex cases
        return argument.StartsWith("$");
    }

    private static string EscapeArgument(string argument)
    {
        // Implement necessary escaping logic here
        return Regex.Replace(argument, @"([""\\])", "\\$1");
    }
}