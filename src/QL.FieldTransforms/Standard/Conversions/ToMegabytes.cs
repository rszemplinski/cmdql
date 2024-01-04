using QL.Core.Attributes;
using QL.Core.FieldTransforms;

namespace QL.FieldTransforms.Standard.Conversions;

public record ToMegabytesArguments;

[Transformer(Description = "Converts bytes to megabytes")]
public class ToMegabytes : FieldTransformBase<ToMegabytesArguments, double>
{
    protected override double Execute(object value, ToMegabytesArguments arguments)
    {
        if (value is not ulong bytes)
            throw new ArgumentException("Value must be a type of ulong");

        var megabytes = bytes / 1024.0 / 1024.0;
        return megabytes;
    }
}