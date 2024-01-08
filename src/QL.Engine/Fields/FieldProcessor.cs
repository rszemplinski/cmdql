using System.Collections;
using System.Collections.Specialized;
using QL.Engine.Extensions;

namespace QL.Engine.Fields;

public static class FieldProcessor
{
    public static OrderedDictionary ProcessFields(IEnumerable<Field> fields, IDictionary originalDict)
    {
        var newDict = new OrderedDictionary();

        // Add 'error' and 'exitCode' fields if they exist in the original dictionary
        if (originalDict.Contains("error"))
            newDict.Add("error", originalDict["error"]);
        if (originalDict.Contains("exitCode"))
            newDict.Add("exitCode", originalDict["exitCode"]);

        var allKeys = originalDict.Keys.Cast<string>().ToList();

        foreach (var field in fields)
        {
            // Check if the field name exists in the original dictionary (case insensitive)
            var key = allKeys.FirstOrDefault(x => x.Equals(field.Name, StringComparison.OrdinalIgnoreCase));
            if (key == null)
                continue;

            var value = originalDict[key];

            switch (value)
            {
                case IDictionary subDict when field.Fields.Any():
                    var processedSubDict = ProcessFields(field.Fields.Cast<Field>(), subDict);
                    newDict.Add(field.Name, processedSubDict);
                    break;
                case IList list:
                    var processedList = new List<object>();
                    foreach (var item in list)
                    {
                        var listItemDict = item.ToDictionary();
                        var subFields = field.Fields.Any()
                            ? field.Fields.Cast<Field>()
                            : listItemDict.Keys.Cast<string>().Select(x => new Field(x));
                        var processedListItemDict = ProcessFields(subFields, listItemDict);
                        processedList.Add(processedListItemDict);
                    }

                    newDict.Add(field.Name, processedList);
                    break;
                default:
                {
                    if (value == null)
                    {
                        newDict.Add(field.Name, value);
                        break;
                    }

                    // Convert to dictionary if the value is not a primitive type
                    if (!value.GetType().IsPrimitive && value is not string && !value.GetType().IsEnum)
                    {
                        var valueDict = value.ToDictionary();
                        var newSubDict = ProcessFields(field.Fields.Cast<Field>(), valueDict);
                        newDict.Add(field.Name, newSubDict);
                    }
                    else
                    {
                        value = field.Transformers.Aggregate(value,
                            (current, transformer) => transformer.fieldTransform.Apply(current!, transformer.args));
                        newDict.Add(field.Name, value);
                    }

                    break;
                }
            }
        }

        return newDict;
    }
}