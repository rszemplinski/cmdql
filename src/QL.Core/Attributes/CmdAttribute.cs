namespace QL.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CmdAttribute(string cmdTemplate) : Attribute
{
    public string CmdTemplate { get; set; } = cmdTemplate;
}