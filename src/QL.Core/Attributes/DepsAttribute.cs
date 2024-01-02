namespace QL.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class DepsAttribute(string[] deps) : Attribute
{
    /**
     * A list of dependencies that the <see cref="Actions.IAction"/> requires.
     */
    public string[] Deps { get; } = deps;
}