using QL.Core.Attributes;
using QL.Core.FieldTransforms;

namespace QL.FieldTransforms.Standard.Conversions;

public record ToGigabytesArguments;

[Transformer(Description = "Converts bytes to gigabytes")]
public class ToGigabytes : FieldTransformBase<ToGigabytesArguments, double>
{
    protected override double Execute(object value, ToGigabytesArguments arguments)
    {
        if (value is not ulong bytes)
            throw new ArgumentException("Value must be a type of ulong");

        var gigabytes = bytes / 1024.0 / 1024.0 / 1024.0;
        return gigabytes;
    }
}