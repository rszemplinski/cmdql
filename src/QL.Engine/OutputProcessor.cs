using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace QL.Engine;

public static class OutputProcessor
{
    public static void Process(
        object output, 
        AppConfig appConfig)
    {
        var results = appConfig.OutputFormat switch
        {
            OutputFormat.Json => ProcessJson(output),
            OutputFormat.Table => ProcessTable(output),
            OutputFormat.Yml => ProcessYml(output),
            _ => throw new InvalidOperationException($"Output format {appConfig.OutputFormat} is not supported.")
        };
        
        if (appConfig.OutputFile is not null)
        {
            if (File.Exists(appConfig.OutputFile))
                File.Delete(appConfig.OutputFile);
            
            Log.Debug("Writing output to {0}", Path.GetFullPath(appConfig.OutputFile));
            File.WriteAllText(appConfig.OutputFile, results);
        }
        else
        {
            Console.WriteLine(results);
        }
    }

    private static string ProcessJson(object output)
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
        return json;
    }

    private static string ProcessTable(object output)
    {
        throw new NotImplementedException();
    }

    private static string ProcessYml(object output)
    {
        var yaml = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithIndentedSequences()
            .Build()
            .Serialize(output);
        return yaml;
    }

}