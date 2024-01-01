using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using QL.Core.Attributes;
using Serilog;

namespace QL.Core.Actions;

public abstract partial class ActionBase<TArg, TReturnType> : IAction
    where TArg : class
    where TReturnType : class
{
    public async Task<object> ExecuteCommandAsync(IClient client, IReadOnlyDictionary<string, object> arguments,
        IReadOnlyCollection<IField> fields,
        CancellationToken cancellationToken = default)
    {
        // Build Command
        var convertedArguments = _BuildCommand(arguments);
        var cmd = BuildCommand(convertedArguments);
        cmd = ExtraSpaceRemoverRegex().Replace(cmd, " ").Trim();
        Log.Debug("[{0}] => Executing command: {1}", client, cmd);

        // Execute
        var executeTask = await ExecuteAsync(convertedArguments, client, cmd, cancellationToken);
        ValidateResults(executeTask);

        // Parse
        var parsedResults = _ParseCommandResults(executeTask, fields);
        return parsedResults;
    }

    protected virtual Task<ICommandOutput> ExecuteAsync(
        TArg arguments,
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
            throw new InvalidOperationException(
                $"Command exited with code {commandResults.ExitCode}.\nError: {commandResults.Error}");
        }
    }

    private static TArg _BuildCommand(IReadOnlyDictionary<string, object> arguments)
    {
        var convertedArguments = Converter.ConvertArguments<TArg>(arguments);
        if (convertedArguments is null)
        {
            throw new InvalidOperationException($"Could not convert arguments to {typeof(TArg).FullName}.");
        }

        return convertedArguments;
    }

    protected virtual string BuildCommand(TArg arguments)
    {
        var cmdTemplate = GetCommandTemplate();
        return ReplaceTemplates(cmdTemplate, arguments);
    }

    private static string ReplaceTemplates(string cmdTemplate, TArg arguments)
    {
        var cmd = cmdTemplate;
        foreach (var property in typeof(TArg).GetProperties())
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

    private object _ParseCommandResults(ICommandOutput commandResults, IReadOnlyCollection<IField> fields)
    {
        var parsedResults = ParseCommandResults(commandResults);

        var instanceType = typeof(TReturnType);
        if (!instanceType.IsGenericType || instanceType.GetGenericTypeDefinition() != typeof(List<>))
        {
            return ConvertInstanceToDictionary(parsedResults, fields);
        }

        if (parsedResults is not IEnumerable enumerable)
        {
            throw new InvalidOperationException($"Could not convert {typeof(TReturnType).FullName} to IEnumerable.");
        }

        var results = new List<IReadOnlyDictionary<string, object?>>();
        foreach (var item in enumerable)
        {
            var instanceDictionary = ConvertInstanceToDictionary(item, fields);
            results.Add(instanceDictionary);
        }

        return results;
    }

    protected virtual TReturnType ParseCommandResults(ICommandOutput commandResults)
    {
        var parsedResults = Converter.ParseResults<TReturnType>(commandResults.Result, GetRegex());
        if (parsedResults is null)
        {
            throw new InvalidOperationException($"Could not parse command results to {typeof(TReturnType).FullName}.");
        }

        return parsedResults;
    }

    private static IReadOnlyDictionary<string, object?> ConvertInstanceToDictionary(object instance,
        IReadOnlyCollection<IField> fields)
    {
        var instanceDictionary = new Dictionary<string, object?>();

        var instanceType = instance.GetType();
        var selectedFields = fields.Any()
            ? fields
            : instanceType.GetProperties().Select(x => new Field(x) as IField).ToArray();

        foreach (var property in instanceType.GetProperties())
        {
            var field = selectedFields
                .FirstOrDefault(x => x.Name.Equals(property.Name, StringComparison.OrdinalIgnoreCase));
            if (field is null)
                continue;

            if (property.PropertyType.IsGenericType &&
                property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                if (property.GetValue(instance) is not IEnumerable list)
                    continue;

                var listType = property.PropertyType.GetGenericArguments()[0];
                if (listType.IsPrimitive || listType == typeof(string) || listType.IsValueType)
                {
                    instanceDictionary.Add(field.Name, list);
                    continue;
                }

                var listDictionary = new List<IReadOnlyDictionary<string, object?>>();
                foreach (var item in list)
                {
                    var itemDictionary = ConvertInstanceToDictionary(item, field.Fields);
                    listDictionary.Add(itemDictionary);
                }

                instanceDictionary.Add(field.Name, listDictionary);
                continue;
            }

            var propertyValue = property.GetValue(instance);
            instanceDictionary.Add(field.Name, propertyValue);
        }

        return instanceDictionary;
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