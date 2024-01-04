namespace QL.Core.FieldTransforms;

public abstract class FieldTransformBase<TArgs, TReturnType> : IFieldTransform
    where TArgs : class
{
    public object Apply(object value, IReadOnlyDictionary<string, object> arguments)
    {
        var convertedArguments = Converter.ConvertArguments<TArgs>(arguments);
        if (convertedArguments is null)
            throw new ArgumentException($"Arguments could not be converted to the correct type {typeof(TArgs).Name}");

        var result = Execute(value, convertedArguments);
        return result!;
    }

    protected abstract TReturnType Execute(object value, TArgs arguments);
}