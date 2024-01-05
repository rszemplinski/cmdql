using System.Collections;

namespace QL.Engine.Utils;

public static class ObjectModifier
{
    public static T ModifyObject<T>(T obj, Func<string, object, object> valueProvider)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        if (valueProvider == null) throw new ArgumentNullException(nameof(valueProvider));

        var type = obj.GetType();

        // Check if the object is a dictionary
        if (typeof(IDictionary).IsAssignableFrom(type))
        {
            var dictionary = (IDictionary)obj;
            var keys = new ArrayList(dictionary.Keys);

            foreach (var key in keys)
            {
                var currentValue = dictionary[key];
                var newValue = valueProvider(key.ToString()!, currentValue!);

                dictionary[key] = newValue;
            }
        }
        // Check if the object is a list
        else if (typeof(IList).IsAssignableFrom(type))
        {
            var list = (IList)obj;
            for (var i = 0; i < list.Count; i++)
            {
                var element = list[i];
                if (element != null)
                {
                    ModifyObject(element, valueProvider); // Recursive call for each element
                }
            }
        }
        else // Handle as a regular object
        {
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                if (!property.CanWrite) continue;
                try
                {
                    var currentValue = property.GetValue(obj);
                    var newValue = valueProvider(property.Name, currentValue!);

                    if (property.PropertyType.IsInstanceOfType(newValue))
                    {
                        property.SetValue(obj, newValue);
                    }
                }
                catch (Exception ex)
                {
                    // Handle or log the exception as needed
                    Console.WriteLine($"Error setting property {property.Name}: {ex.Message}");
                }
            }
        }

        return obj;
    }
}