using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace QLShell;

public static class OutputProcessor
{
    public static void Process(
        IReadOnlyDictionary<string, object> output, 
        AppConfig appConfig)
    {
        switch (appConfig.OutputFormat)
        {
            case OutputFormat.Json:
                ProcessJson(output);
                break;
            case OutputFormat.Table:
                ProcessTable(output);
                break;
            case OutputFormat.Yml:
                ProcessYml(output);
                break;
            default:
                throw new InvalidOperationException("Invalid output format");
        }
    }

    private static void ProcessJson(IReadOnlyDictionary<string, object> output)
    {
        var json = JsonSerializer.Serialize(output, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            },
        });
        Console.WriteLine(json);
    }

    private static void ProcessTable(IReadOnlyDictionary<string, object> output)
    {
        throw new NotImplementedException();
    }

    private static void ProcessYml(IReadOnlyDictionary<string, object> output)
    {
        var yaml = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithIndentedSequences()
            .Build()
            .Serialize(output);
        Console.WriteLine(yaml);
    }

}