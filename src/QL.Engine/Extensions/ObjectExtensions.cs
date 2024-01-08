using System.Collections;
using QL.Core.Extensions;

namespace QL.Engine.Extensions;

public static class ObjectExtensions
{
    public static IDictionary ToDictionary(this object obj)
    {
        switch (obj)
        {
            case null:
                throw new ArgumentNullException(nameof(obj));
            case IDictionary dictionary:
                return dictionary;
        }

        var objDict = new Dictionary<string, object>();
        foreach (var property in obj.GetType().GetProperties())
        {
            // Check if the property can be read
            if (!property.CanRead)
            {
                continue;
            }

            var value = property.GetValue(obj, null);
            objDict.Add(property.Name.ToCamelCase(), value!);
        }
        return objDict;
    }
}