using System.Collections;
using System.Collections.Specialized;

namespace QL.Engine.Fields;

public abstract class FieldProcessor
{
    public static OrderedDictionary ProcessFields(IEnumerable<Field> fields, IDictionary originalDict)
    {
        var newDict = new OrderedDictionary();

        // Add 'error' and 'exitCode' fields if they exist in the original dictionary
        if (originalDict.Contains("error"))
            newDict.Add("error", originalDict["error"]);
        if (originalDict.Contains("exitCode"))
            newDict.Add("exitCode", originalDict["exitCode"]);

        foreach (var field in fields)
        {
            if (!originalDict.Contains(field.Name)) continue;
            var value = originalDict[field.Name];

            switch (value)
            {
                // Check if value is a dictionary and process it recursively
                case IDictionary subDict:
                {
                    var processedSubDict = ProcessFields(field.Fields.Cast<Field>(), subDict);
                    newDict.Add(field.Name, processedSubDict);
                    break;
                }
                // Check if value is a list and process each item if it's a dictionary
                case IList list:
                {
                    var processedList = new List<object>();
                    foreach (var item in list)
                    {
                        if (item is IDictionary listItemDict)
                        {
                            var processedListItemDict = ProcessFields(field.Fields.Cast<Field>(), listItemDict);
                            processedList.Add(processedListItemDict);
                        }
                        else
                        {
                            processedList.Add(item);
                        }
                    }

                    newDict.Add(field.Name, processedList);
                    break;
                }
                default:
                    var transformedValue = field.Transformers
                        .Aggregate(value,
                            (current, transformer) => transformer.fieldTransform.Apply(current!, transformer.args));
                    newDict.Add(field.Name, transformedValue);
                    break;
            }
        }

        return newDict;
    }
}