namespace QL.Core.Actions;

public interface IField
{
    public string Name { get; }
    public IField[] Fields { get; }
}