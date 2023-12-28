using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommandLine;
using Serilog;
using Serilog.Events;
using AppContext = QLShell.Contexts.AppContext;
using File = System.IO.File;
using Parser = QL.Parser.Parser;

namespace QLShell;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        await CommandLine.Parser.Default
            .ParseArguments<Options>(args)
            .WithParsedAsync(Run);
    }

    private static async Task Run(Options options)
    {
        var cts = new CancellationTokenSource();
        
        Console.CancelKeyPress += (_, e) =>
        {
            cts.Cancel();
            e.Cancel = true;
        };
        
        var programSw = Stopwatch.StartNew();

        var inputFile = Path.GetFullPath(options.InputFile);
        ConfigureLogging(options.Verbose);

        try
        {
            var sw = Stopwatch.StartNew();
            var input = await File.ReadAllTextAsync(inputFile, cts.Token);
            sw.Stop();
            Log.Debug("Read input file {0} in {1}ms", inputFile, sw.ElapsedMilliseconds);

            sw.Restart();
            var ast = Parser.ParseQuery(input);
            sw.Stop();
            Log.Debug("Generated AST in {0}ms", sw.ElapsedMilliseconds);

            var concurrencyCount = options.Sync ? 1 : options.Concurrency;
            TaskLimiter.Create(concurrencyCount);
            Log.Debug("Concurrency limit set to {0}", concurrencyCount);
            
            var appConfig = new AppConfig
            {
                InputFile = inputFile,
                Verbose = options.Verbose,
                Debug = options.Debug,
                OutputFormat = options.Format,
                Sync = options.Sync,
            };
            var output = await 
                new AppContext(ast, appConfig).ExecuteAsync(cts.Token);
            
            sw.Restart();
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
            sw.Stop();
            Log.Debug("Serialized output in {0}ms", sw.ElapsedMilliseconds);

            Console.WriteLine(json);
        }
        catch (TaskCanceledException)
        {
            Log.Warning("The operation was cancelled");
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while executing the query");
            Environment.Exit(1);
        }
        finally
        {
            programSw.Stop();
            Log.Debug("Finished in {0}ms", programSw.ElapsedMilliseconds);
        }
    }

    private static void ConfigureLogging(bool verbose)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(verbose
                ? LogEventLevel.Verbose
                : LogEventLevel.Information)
            .WriteTo.Console()
            .CreateLogger();
    }
}