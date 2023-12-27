using System.Text.RegularExpressions;

namespace QL.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class RegexAttribute(string regex, RegexOptions options = RegexOptions.Multiline) : Attribute
{
    public Regex Regex { get; } = new(regex, options);
}