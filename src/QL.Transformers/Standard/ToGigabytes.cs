using QL.Core.Attributes;
using QL.Core.Transformers;

namespace QL.Transformers.Standard;

public record ToGigabytesArguments;

[Transformer(description: "Converts bytes to gigabytes")]
public class ToGigabytes : TransformerBase<ToGigabytesArguments, string>
{
    protected override string Execute(object value, ToGigabytesArguments arguments)
    {
        if (value is not ulong bytes)
            throw new ArgumentException("Value must be a type of ulong");

        return $"{bytes / 1024 / 1024 / 1024} GB";
    }
}