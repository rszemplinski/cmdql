namespace QL.Core.Transformers;

public abstract class TransformerBase<TArgs, TReturnType> : ITransformer
    where TArgs : class
    where TReturnType : class
{
    public object Transform(object value, IReadOnlyDictionary<string, object> arguments)
    {
        var convertedArguments = Converter.ConvertArguments<TArgs>(arguments);
        if (convertedArguments is null)
            throw new ArgumentException($"Arguments could not be converted to the correct type {typeof(TArgs).Name}");

        var result = Execute(value, convertedArguments);
        return result;
    }

    protected abstract TReturnType Execute(object value, TArgs arguments);
}