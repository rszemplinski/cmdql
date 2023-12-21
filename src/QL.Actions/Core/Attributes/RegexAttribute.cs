using System.Text.RegularExpressions;

namespace QL.Actions.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class RegexAttribute(string regex) : Attribute
{
    public Regex Regex { get; set; } = new(regex);
}