namespace QL.Core.Transformers;

public interface ITransformer
{
    public object Transform(object value, IReadOnlyDictionary<string, object> arguments);
}