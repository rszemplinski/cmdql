namespace QL.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class NamespaceAttribute(string @namespace) : Attribute
{
    public string Namespace { get; } = @namespace;
}