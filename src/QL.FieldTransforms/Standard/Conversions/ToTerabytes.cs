using QL.Core.Attributes;
using QL.Core.FieldTransforms;

namespace QL.FieldTransforms.Standard.Conversions;

public record ToTerabytesArguments;

[Transformer(Description = "Converts bytes to terabytes")]
public class ToTerabytes : FieldTransformBase<ToTerabytesArguments, double>
{
    protected override double Execute(object value, ToTerabytesArguments arguments)
    {
        const string defaultFormat = "0.00";
        if (value is not ulong bytes)
            throw new ArgumentException("Value must be a type of ulong");

        var terabytes = bytes / 1024.0 / 1024.0 / 1024.0 / 1024.0;
        return terabytes;
    }
}