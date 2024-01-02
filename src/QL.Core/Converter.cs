using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using QL.Parser.AST.Nodes;
using Serilog;

namespace QL.Core;

internal static class Converter
{
    internal static T? ConvertArguments<T>(IReadOnlyDictionary<string, object> arguments) where T : class
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
                Log.Warning("Could not find a value for property {0}", property.Name);
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
            else if (property.PropertyType == typeof(DateTime) && propertyValue is StringValueNode dateTimeValue)
            {
                var success = DateTime.TryParseExact(dateTimeValue.Value, Constants.DateFormats,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var result);
                if (!success)
                {
                    Log.Warning("Could not parse {0} to a DateTime", dateTimeValue.Value);
                    continue;
                }

                property.SetValue(instance, result);
            }
            else
            {
                throw new InvalidOperationException($"The type {property.PropertyType.FullName} is not supported.");
            }
        }

        return instance as T;
    }
    
    internal static TReturnType ParseResults<TReturnType>(string commandResults, Regex regex)
        where TReturnType : class
    {
        var instanceType = typeof(TReturnType);

        if (!instanceType.IsGenericType || instanceType.GetGenericTypeDefinition() != typeof(List<>))
        {
            var match = regex.Match(commandResults);
            if (!match.Success)
            {
                throw new InvalidOperationException($"The command results did not match the regex {regex}.");
            }

            var instance = ProcessSingleReturnType(match, instanceType);
            return (instance as TReturnType)!;
        }

        var singleInstanceType = instanceType.GetGenericArguments()[0];
        return ProcessListReturnType<TReturnType>(regex, commandResults, singleInstanceType);
    }

    private static TReturnType ProcessListReturnType<TReturnType>(Regex regex, string commandResults,
        Type singleInstanceType)
        where TReturnType : class
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

    private static object ProcessSingleReturnType(Match match, Type instanceType)
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
                Log.Warning("Group {0} was not found in the match", groupName);
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
                    Log.Warning("Could not parse {0} to a DateTime", propertyValue);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T? HandleEmptyValue<T>(string value, T? defaultValue = default)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return (T)Convert.ChangeType(value, typeof(T));
    }
}