using QL.Core.Attributes;
using QL.Core.Transformers;

namespace QL.Transformers.Standard.Conversions;

public record ToTerabytesArguments;

[Transformer(Description = "Converts bytes to terabytes")]
public class ToTerabytes : TransformerBase<ToTerabytesArguments, string>
{
    protected override string Execute(object value, ToTerabytesArguments arguments)
    {
        if (value is not ulong bytes)
            throw new ArgumentException("Value must be a type of ulong");

        return $"{bytes / 1024 / 1024 / 1024} GB";
    }
}