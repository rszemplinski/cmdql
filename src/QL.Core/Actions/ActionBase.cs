using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using QL.Core.Attributes;
using QL.Parser.AST.Nodes;
using Serilog;

namespace QL.Core.Actions;

public abstract partial class ActionBase<TArg, TReturnType> : IAction
    where TArg : class
    where TReturnType : class
{
    public async Task<string> BuildCommandAsync(Dictionary<string, object> arguments)
    {
        var convertedArguments = ConvertArguments<TArg>(arguments);
        if (convertedArguments is null)
        {
            throw new InvalidOperationException($"Could not convert arguments to {typeof(TArg).FullName}.");
        }

        var cmd = await _BuildCommandAsync(convertedArguments);
        return cmd;
    }

    public virtual Task<string> PostExecutionAsync(string commandResults, ISshClient sshClient)
    {
        return Task.FromResult(commandResults);
    }

    protected virtual Task<string> _BuildCommandAsync(TArg arguments)
    {
        var cmdTemplate = GetCommandTemplate();
        var cmd = ReplaceTemplates(cmdTemplate, arguments);
        return Task.FromResult(cmd);
    }

    public async Task<object> ParseCommandResultsAsync(string commandResults, string[] fields)
    {
        var parsedResults = await _ParseCommandResultsAsync(commandResults);
        if (parsedResults is null)
        {
            throw new InvalidOperationException($"Could not parse command results to {typeof(TReturnType).FullName}.");
        }

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

    private static IReadOnlyDictionary<string, object?> ConvertInstanceToDictionary(object instance, string[] fields)
    {
        var instanceType = instance.GetType();
        var instanceDictionary = new Dictionary<string, object?>();
        foreach (var property in instanceType.GetProperties())
        {
            // Check equality by name (ignoring case)
            var field = fields.FirstOrDefault(x => x.Equals(property.Name, StringComparison.OrdinalIgnoreCase));
            if (field is null)
                continue;

            var propertyValue = property.GetValue(instance);
            instanceDictionary.Add(field, propertyValue);
        }

        return instanceDictionary;
    }

    protected virtual Task<TReturnType> _ParseCommandResultsAsync(string commandResults)
    {
        var regex = GetRegex();
        var instanceType = typeof(TReturnType);

        if (!instanceType.IsGenericType || instanceType.GetGenericTypeDefinition() != typeof(List<>))
        {
            var match = regex.Match(commandResults);
            if (!match.Success)
            {
                throw new InvalidOperationException($"The command results did not match the regex {regex}.");
            }

            var instance = ProcessSingleReturnType(match, instanceType);
            return Task.FromResult((instance as TReturnType)!);
        }

        var singleInstanceType = instanceType.GetGenericArguments()[0];
        return Task.FromResult(ProcessListReturnType(regex, commandResults, singleInstanceType));
    }

    protected TReturnType ProcessListReturnType(Regex regex, string commandResults, Type singleInstanceType)
    {
        var listType = typeof(List<>);
        var constructedListType = listType.MakeGenericType(singleInstanceType);
        var instance = Activator.CreateInstance(constructedListType);

        var instanceType = typeof(TReturnType);
        var instanceAddMethod = instanceType.GetMethod("Add");
        if (instanceAddMethod is null)
        {
            throw new InvalidOperationException($"The type {instanceType.FullName} does not have an Add method.");
        }

        if (!regex.Options.HasFlag(RegexOptions.Multiline))
        {
            // Process each line
            var lines = commandResults.Split(Environment.NewLine);
            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (!match.Success)
                {
                    throw new InvalidOperationException($"The command results did not match the regex {regex}.");
                }

                var singleInstance = ProcessSingleReturnType(match, singleInstanceType);
                instanceAddMethod.Invoke(instance, [singleInstance]);
            }
        }
        else
        {
            var matches = regex.Matches(commandResults);
            foreach (Match match in matches)
            {
                var singleInstance = ProcessSingleReturnType(match, singleInstanceType);
                instanceAddMethod.Invoke(instance, [singleInstance]);
            }
        }

        return (instance as TReturnType)!;
    }

    protected object ProcessSingleReturnType(Match match, Type instanceType)
    {
        var groupNameDictionary = match.Groups.Keys.ToDictionary(x => x.ToLowerInvariant(), x => x);
        
        var instance = Activator.CreateInstance(instanceType);
        var instanceProperties = instanceType.GetProperties();

        foreach (var property in instanceProperties)
        {
            // Attempt to match group name by property name (ignoring case)
            var (key, groupName) = groupNameDictionary
                .FirstOrDefault(x => x.Key.Equals(property.Name, StringComparison.OrdinalIgnoreCase));
            
            if (key is null)
                continue;

            var group = match.Groups[groupName];
            if (!group.Success)
            {
                Log.Warning("Group {0} was not found in the match.", groupName);
                continue;
            }

            var propertyValue = group.Value;

            if (property.PropertyType == typeof(string))
            {
                property.SetValue(instance, propertyValue);
            }
            else if (property.PropertyType == typeof(int))
            {
                property.SetValue(instance, HandleEmptyValue(propertyValue, 0));
            }
            else if (property.PropertyType == typeof(uint))
            {
                property.SetValue(instance, HandleEmptyValue(propertyValue, 0u));
            }
            else if (property.PropertyType == typeof(long))
            {
                property.SetValue(instance, HandleEmptyValue(propertyValue, 0L));
            }
            else if (property.PropertyType == typeof(ulong))
            {
                property.SetValue(instance, HandleEmptyValue(propertyValue, 0ul));
            }
            else if (property.PropertyType == typeof(float))
            {
                property.SetValue(instance, HandleEmptyValue(propertyValue, 0f));
            }
            else if (property.PropertyType == typeof(double))
            {
                property.SetValue(instance, HandleEmptyValue(propertyValue, 0d));
            }
            else if (property.PropertyType == typeof(decimal))
            {
                property.SetValue(instance, HandleEmptyValue(propertyValue, 0m));
            }
            else if (property.PropertyType == typeof(DateTime))
            {
                var success = DateTime.TryParseExact(propertyValue, Constants.DateFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var result);
                if (!success)
                {
                    Log.Warning("Could not parse {0} to a DateTime.", propertyValue);
                    continue;
                }

                property.SetValue(instance, result);
            }
            else if (property.PropertyType == typeof(DateTimeOffset))
            {
                property.SetValue(instance, DateTimeOffset.Parse(propertyValue));
            }
            else if (property.PropertyType == typeof(TimeSpan))
            {
                property.SetValue(instance, TimeSpan.Parse(propertyValue));
            }
            else if (property.PropertyType == typeof(Guid))
            {
                property.SetValue(instance, Guid.Parse(propertyValue));
            }
            else if (property.PropertyType == typeof(Uri))
            {
                property.SetValue(instance, new Uri(propertyValue));
            }
            else
            {
                throw new InvalidOperationException($"The type {property.PropertyType.FullName} is not supported.");
            }
        }

        return instance!;
    }

    private static T? HandleEmptyValue<T>(string value, T? defaultValue = default)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return (T)Convert.ChangeType(value, typeof(T));
    }

    protected string ReplaceTemplates(string cmdTemplate, TArg arguments)
    {
        var cmd = cmdTemplate;
        foreach (var property in typeof(TArg).GetProperties())
        {
            var propertyType = property.PropertyType;
            var propertyValue = GetPropertyValue(arguments, property.Name);

            if (propertyType == typeof(bool))
            {
                var condition = (bool)propertyValue!;
                var templateKey = $"?[{property.Name.ToCamelCase()}]";
                cmd = condition
                    ? cmd.Replace(templateKey, "")
                    : Regex.Replace(cmd, $"{Regex.Escape(templateKey)}\\S*", "");
            }
            else if (propertyType == typeof(string))
            {
                var value = (string)propertyValue!;
                var placeholder = $"{{{property.Name.ToCamelCase()}}}";
                cmd = cmd.Replace(placeholder, value);
            }
        }

        cmd = ExtraSpaceRemoverRegex().Replace(cmd, " ");

        return cmd;
    }

    private static object? GetPropertyValue(TArg obj, string propertyName)
    {
        var property = typeof(TArg).GetProperty(propertyName);
        return property?.GetValue(obj, null);
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

    private static T? ConvertArguments<T>(IReadOnlyDictionary<string, object> arguments) where T : class
    {
        var type = typeof(T);
        var instance = Activator.CreateInstance(type);
        foreach (var property in type.GetProperties())
        {
            var (key, propertyValue) = arguments
                .FirstOrDefault(x => x.Key.Equals(property.Name, StringComparison.OrdinalIgnoreCase));
            if (key is null)
                continue;

            if (propertyValue is null)
            {
                Log.Warning("Could not find a value for property {0}.", property.Name);
                continue;
            }

            if (property.PropertyType == typeof(bool) && propertyValue is BooleanValueNode boolValue)
            {
                property.SetValue(instance, boolValue.Value);
            }
            else if (property.PropertyType == typeof(string) && propertyValue is StringValueNode stringValue)
            {
                property.SetValue(instance, stringValue.Value);
            }
            else if (property.PropertyType == typeof(int) && propertyValue is IntValueNode intValue)
            {
                property.SetValue(instance, intValue.Value);
            }
            else if (property.PropertyType == typeof(long) && propertyValue is IntValueNode longValue)
            {
                property.SetValue(instance, longValue.Value);
            }
            else if (property.PropertyType == typeof(float) && propertyValue is DecimalValueNode floatValue)
            {
                property.SetValue(instance, floatValue.Value);
            }
            else if (property.PropertyType == typeof(double) && propertyValue is DecimalValueNode doubleValue)
            {
                property.SetValue(instance, doubleValue.Value);
            }
            else if (property.PropertyType == typeof(decimal) && propertyValue is DecimalValueNode decimalValue)
            {
                property.SetValue(instance, decimalValue.Value);
            }
            else
            {
                throw new InvalidOperationException($"The type {property.PropertyType.FullName} is not supported.");
            }
        }

        return instance as T;
    }

    [GeneratedRegex("\\s+")]
    private static partial Regex ExtraSpaceRemoverRegex();
}