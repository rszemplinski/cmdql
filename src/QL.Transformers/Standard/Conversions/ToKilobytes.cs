using QL.Core.Attributes;
using QL.Core.Transformers;

namespace QL.Transformers.Standard.Conversions;

public record ToKilobytesArguments;

[Transformer(Description = "Converts bytes to kilobytes")]
public class ToKilobytes : TransformerBase<ToKilobytesArguments, string>
{
    protected override string Execute(object value, ToKilobytesArguments arguments)
    {
        if (value is not ulong bytes)
            throw new ArgumentException("Value must be a type of ulong");

        return $"{bytes / 1024 / 1024 / 1024} GB";
    }
}