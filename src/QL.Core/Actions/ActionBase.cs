using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using QL.Core.Attributes;
using QL.Core.Exceptions;
using QL.Core.Extensions;
using Serilog;

namespace QL.Core.Actions;

public abstract partial class ActionBase<TArgs, TReturnType> : IAction
    where TArgs : class
{
    protected Platform Platform { get; private set; }

    private TArgs _arguments = default!;

    public void Initialize(Platform platform)
    {
        Platform = platform;
    }

    protected TArgs GetArguments()
    {
        return _arguments;
    }

    public async Task<object?> ExecuteCommandAsync(IClient client, IReadOnlyDictionary<string, object> arguments,
        IReadOnlyCollection<IField> fields,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        
        // Build Command
        var convertedArguments = _BuildCommand(arguments);
        _arguments = convertedArguments;

        await SetupAsync(client, cancellationToken);

        var cmd = BuildCommand(convertedArguments);
        cmd = ExtraSpaceRemoverRegex().Replace(cmd, " ").Trim();
        Log.Debug("[{0}] => Starting command: `{1}`", client.ToString(), cmd);

        // Execute
        var executeTask = await ExecuteAsync(convertedArguments, client, cmd, cancellationToken);
        ValidateResults(executeTask);

        await CleanupAsync(client, cancellationToken);

        // Parse
        var parsedResults = ParseCommandResults(executeTask);
        
        sw.Stop();
        Log.Debug("[{0}] => Executed command: `{1}`. Completed in {2}ms", client.ToString(), cmd, sw.ElapsedMilliseconds);
        
        return parsedResults!;
    }

    protected virtual Task<ICommandOutput> ExecuteAsync(
        TArgs arguments,
        IClient client,
        string command,
        CancellationToken cancellationToken = default)
    {
        return client.ExecuteCommandAsync(command, cancellationToken);
    }

    protected virtual void ValidateResults(ICommandOutput commandResults)
    {
        if (commandResults.ExitCode != 0)
        {
            throw new ActionException(commandResults.Error, commandResults.ExitCode);
        }
    }

    private static TArgs _BuildCommand(IReadOnlyDictionary<string, object> arguments)
    {
        var convertedArguments = Converter.ConvertArguments<TArgs>(arguments);
        if (convertedArguments is null)
        {
            throw new InvalidOperationException($"Could not convert arguments to {typeof(TArgs).FullName}.");
        }

        return convertedArguments;
    }

    protected virtual string BuildCommand(TArgs arguments)
    {
        var cmdTemplate = GetCommandTemplate();
        return ReplaceTemplates(cmdTemplate, arguments);
    }

    private static string ReplaceTemplates(string cmdTemplate, TArgs arguments)
    {
        var cmd = cmdTemplate;
        foreach (var property in typeof(TArgs).GetProperties())
        {
            var propertyType = property.PropertyType;
            var propertyValue = property.GetValue(arguments);

            if (propertyType == typeof(bool))
            {
                var condition = (bool)propertyValue!;
                var templateKey = $"?[{property.Name.ToCamelCase()}]";
                cmd = condition
                    ? cmd.Replace(templateKey, "")
                    : Regex.Replace(cmd, $"{Regex.Escape(templateKey)}\\S*", "");
            }
            else
            {
                var value = propertyValue!.ToString();
                var placeholder = $"{{{property.Name.ToCamelCase()}}}";
                cmd = cmd.Replace(placeholder, value);
            }
        }

        return cmd;
    }

    protected string GetCommandTemplate()
    {
        var type = GetType();
        var cmdAttribute = type.GetCustomAttribute<CmdAttribute>();
        if (cmdAttribute is null)
        {
            throw new InvalidOperationException($"The type {type.FullName} does not have a {nameof(CmdAttribute)}.");
        }

        return cmdAttribute.CmdTemplate;
    }

    protected Regex GetRegex()
    {
        var type = GetType();
        var regexAttribute = type.GetCustomAttribute<RegexAttribute>();
        if (regexAttribute is null)
        {
            throw new InvalidOperationException($"The type {type.FullName} does not have a {nameof(RegexAttribute)}.");
        }

        return regexAttribute.Regex;
    }
    
    protected string[] GetDeps()
    {
        var type = GetType();
        var depsAttribute = type.GetCustomAttribute<DepsAttribute>();
        return depsAttribute is null ? Array.Empty<string>() : depsAttribute.Deps;
    }

    protected virtual TReturnType? ParseCommandResults(ICommandOutput commandResults)
    {
        var parsedResults = Converter.ParseResults<TReturnType>(commandResults.Result, GetRegex());
        if (parsedResults is null)
        {
            throw new InvalidOperationException($"Could not parse command results to {typeof(TReturnType).FullName}.");
        }

        return parsedResults;
    }
    
    protected virtual Task SetupAsync(IClient client, CancellationToken cancellationToken = default)
    {
        return CheckDepsAsync(client, cancellationToken);
    }
    
    private async Task CheckDepsAsync(IClient client, CancellationToken cancellationToken = default)
    {
        var deps = GetDeps();
        if (deps.Length == 0)
            return;
        
        foreach (var dep in deps)
        {
            var depInstalled = await client.IsToolInstalledAsync(dep, cancellationToken);
            if (!depInstalled)
            {
                throw new InvalidOperationException($"The tool {dep} is not installed on the client.");
            }
        }
    }

    protected virtual Task CleanupAsync(IClient client, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    [GeneratedRegex("\\s+")]
    private static partial Regex ExtraSpaceRemoverRegex();

    private class Field : IField
    {
        public string Name { get; }
        public IField[] Fields { get; }

        public Field(PropertyInfo info)
        {
            Name = info.Name.ToCamelCase();
            Fields = info.PropertyType.GetProperties()
                .Select(x => new Field(x) as IField)
                .ToArray();
        }
    }
}