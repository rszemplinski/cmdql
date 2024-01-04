namespace QL.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class NamespaceAttribute(string ns) : Attribute
{
    public string Namespace { get; } = ns;
}