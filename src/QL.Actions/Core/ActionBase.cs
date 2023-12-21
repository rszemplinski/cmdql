using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using QL.Actions.Core.Attributes;
using Serilog;

namespace QL.Actions.Core;

public abstract partial class ActionBase<TArg, TReturnType> : IAction where TArg : class where TReturnType : class
{
    public Type ArgumentsType { get; } = typeof(TArg);
    public Type ReturnType { get; } = typeof(TReturnType);

    public string BuildCommand(IDictionary<string, object> arguments)
    {
        var convertedArguments = ConvertArguments<TArg>(arguments);
        if (convertedArguments is null)
        {
            throw new InvalidOperationException($"Could not convert arguments to {typeof(TArg).FullName}.");
        }

        var cmd = _BuildCommand(convertedArguments);
        return cmd;
    }

    protected virtual string _BuildCommand(TArg arguments)
    {
        var cmdTemplate = GetCommandTemplate();
        var cmd = ReplaceTemplates(cmdTemplate, arguments);
        return cmd;
    }

    public object ParseCommandResults(string commandResults)
    {
        var parsedResults = _ParseCommandResults(commandResults);
        if (parsedResults is null)
        {
            throw new InvalidOperationException($"Could not parse command results to {typeof(TReturnType).FullName}.");
        }

        return parsedResults;
    }

    protected virtual TReturnType? _ParseCommandResults(string commandResults)
    {
        var regex = GetRegex();
        var instanceType = typeof(TReturnType);

        if (!instanceType.IsGenericType || instanceType.GetGenericTypeDefinition() != typeof(List<>))
            return ProcessSingleReturnType(regex, commandResults, instanceType) as TReturnType;

        var singleInstanceType = instanceType.GetGenericArguments()[0];
        return ProcessListReturnType(regex, commandResults, singleInstanceType);
    }

    protected TReturnType? ProcessListReturnType(Regex regex, string commandResults, Type singleInstanceType)
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

        // Process each line
        var lines = commandResults.Split(Environment.NewLine);
        foreach (var line in lines)
        {
            var singleInstance = ProcessSingleReturnType(regex, line, singleInstanceType);
            if (singleInstance is null)
            {
                continue;
            }

            instanceAddMethod.Invoke(instance, [singleInstance]);
        }

        return instance as TReturnType;
    }

    protected object? ProcessSingleReturnType(Regex regex, string commandResults, Type instanceType)
    {
        var match = regex.Match(commandResults);
        if (!match.Success)
        {
            throw new InvalidOperationException($"The command results did not match the regex {regex}.");
        }

        var groupNameDictionary = match.Groups.Keys.ToDictionary(x => x.ToLower(), x => x);
        var instance = Activator.CreateInstance(instanceType);
        foreach (var property in instanceType.GetProperties())
        {
            // Attempt to match group name by property name (ignoring case)
            var groupName = groupNameDictionary
                .FirstOrDefault(x => x.Key.Equals(property.Name.ToLower())).Value;
            if (groupName is null)
            {
                Log.Warning($"Could not find a group name for property {property.Name}.");
                continue;
            }
            
            var group = match.Groups[groupName];
            if(!group.Success)
                continue;
            
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
                property.SetValue(instance, DateTime.Parse(propertyValue, CultureInfo.InvariantCulture));
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

        return instance;
    }

    private static T? HandleEmptyValue<T>(string value, T? defaultValue = default)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return (T) Convert.ChangeType(value, typeof(T));
    }

    protected string ReplaceTemplates(string cmdTemplate, TArg arguments)
    {
        var cmd = cmdTemplate;
        foreach (var property in typeof(TArg).GetProperties())
        {
            var propertyValue = GetPropertyValue(arguments, property.Name);

            if (propertyValue is bool condition)
            {
                var templateKey = $"?[{property.Name.ToLower()}]";
                cmd = condition
                    ? cmd.Replace(templateKey, "")
                    : Regex.Replace(cmd, $"{Regex.Escape(templateKey)}\\S*", "");
            }
            else if (propertyValue != null)
            {
                var placeholder = $"{{{property.Name.ToLower()}}}";
                cmd = cmd.Replace(placeholder, propertyValue.ToString());
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

    private static object? GetPropertyValue(IDictionary<string, object> obj, string propertyName)
    {
        return obj.TryGetValue(propertyName, out var value) ? value : null;
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

    private static T? ConvertArguments<T>(IDictionary<string, object> arguments) where T : class
    {
        var type = typeof(T);
        var instance = Activator.CreateInstance(type);
        foreach (var property in type.GetProperties())
        {
            var propertyValue = GetPropertyValue(arguments, property.Name);
            if (propertyValue is null)
            {
                continue;
            }

            if (property.PropertyType == typeof(bool))
            {
                property.SetValue(instance, propertyValue);
            }
            else if (property.PropertyType == typeof(string))
            {
                property.SetValue(instance, propertyValue);
            }
            else if (property.PropertyType == typeof(int))
            {
                property.SetValue(instance, propertyValue);
            }
            else if (property.PropertyType == typeof(long))
            {
                property.SetValue(instance, propertyValue);
            }
            else if (property.PropertyType == typeof(float))
            {
                property.SetValue(instance, propertyValue);
            }
            else if (property.PropertyType == typeof(double))
            {
                property.SetValue(instance, propertyValue);
            }
            else if (property.PropertyType == typeof(decimal))
            {
                property.SetValue(instance, propertyValue);
            }
            else if (property.PropertyType == typeof(DateTime))
            {
                property.SetValue(instance, propertyValue);
            }
            else if (property.PropertyType == typeof(DateTimeOffset))
            {
                property.SetValue(instance, propertyValue);
            }
            else if (property.PropertyType == typeof(TimeSpan))
            {
                property.SetValue(instance, propertyValue);
            }
            else if (property.PropertyType == typeof(Guid))
            {
                property.SetValue(instance, propertyValue);
            }
            else if (property.PropertyType == typeof(Uri))
            {
                property.SetValue(instance, propertyValue);
            }
            else if (property.PropertyType == typeof(Version))
            {
                property.SetValue(instance, propertyValue);
            }
            else if (property.PropertyType == typeof(Regex))
            {
                property.SetValue(instance, propertyValue);
            }
            else if (property.PropertyType == typeof(Type))
            {
                property.SetValue(instance, propertyValue);
            }
            else if (property.PropertyType == typeof(object))
            {
                property.SetValue(instance, propertyValue);
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