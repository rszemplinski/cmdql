using QL.Core.Attributes;
using QL.Core.FieldTransforms;

namespace QL.FieldTransforms.Standard.Conversions;

public record ToKilobytesArguments;

[Transformer(Description = "Converts bytes to kilobytes")]
public class ToKilobytes : FieldTransformBase<ToKilobytesArguments, double>
{
    protected override double Execute(object value, ToKilobytesArguments arguments)
    {
        if (value is not ulong bytes)
            throw new ArgumentException("Value must be a type of ulong");

        var kiloBytes = bytes / 1024.0;
        return kiloBytes;
    }
}