using QL.Core.Attributes;
using QL.Core.Transformers;

namespace QL.Transformers.Standard.Conversions;

public record ToMegabytesArguments;

[Transformer(description: "Converts bytes to gigabytes")]
public class ToMegabytes : TransformerBase<ToMegabytesArguments, string>
{
    protected override string Execute(object value, ToMegabytesArguments arguments)
    {
        if (value is not ulong bytes)
            throw new ArgumentException("Value must be a type of ulong");

        return $"{bytes / 1024 / 1024 / 1024} GB";
    }
}