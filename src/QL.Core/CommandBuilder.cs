using System.Text;
using System.Text.RegularExpressions;

namespace QL.Core;

public class CommandBuilder
{
    private readonly StringBuilder _scriptBuilder = new();
    private readonly List<string> _currentCommand = [];

    public CommandBuilder StartBlock(string blockStart)
    {
        FlushCurrentCommand();
        _scriptBuilder.AppendLine(blockStart);
        return this;
    }

    public CommandBuilder EndBlock(string blockEnd)
    {
        FlushCurrentCommand();
        _scriptBuilder.AppendLine(blockEnd);
        return this;
    }

    public CommandBuilder AddCommand(string command)
    {
        FlushCurrentCommand();
        _currentCommand.Add(command);
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

    public CommandBuilder EndIf()
    {
        FlushCurrentCommand();
        _scriptBuilder.AppendLine("fi;");
        return this;
    }

    public CommandBuilder ForLoop(string variable, string range)
    {
        return StartBlock($"for {variable} in {range}; do");
    }

    public CommandBuilder EndFor()
    {
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

    private void FlushCurrentCommand()
    {
        if (_currentCommand.Count <= 0) return;
        _scriptBuilder.AppendLine(string.Join(" ", _currentCommand));
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