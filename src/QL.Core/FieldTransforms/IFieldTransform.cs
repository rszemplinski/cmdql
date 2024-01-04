namespace QL.Core.FieldTransforms;

public interface IFieldTransform
{
    public object Apply(object value, IReadOnlyDictionary<string, object> arguments);
}