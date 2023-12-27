namespace QL.Core;

public static class Utils
{
    
    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrEmpty(str) || !char.IsUpper(str[0]))
            return str;
        
        var chars = str.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (i > 0 && i + 1 < chars.Length && !char.IsUpper(chars[i + 1]))
                break;
            
            chars[i] = char.ToLowerInvariant(chars[i]);
        }
        
        return new string(chars);
    }
    
}