using System.Diagnostics;
using CommandLine;
using Serilog;
using Serilog.Events;
using AppContext = QL.Engine.Contexts.AppContext;
using File = System.IO.File;

namespace QL.Engine;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var parser = new CommandLine.Parser(config =>
        {
            config.AllowMultiInstance = true;
            config.CaseInsensitiveEnumValues = true;
            config.AutoHelp = true;
            config.HelpWriter = Console.Out;
        });
        
        await parser.ParseArguments<Options>(args)
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
        ConfigureLogging(options.Verbose);

        var input = options.Query;
        var sw = Stopwatch.StartNew();
         if (string.IsNullOrEmpty(input))
        {
            var inputFile = Path.GetFullPath(options.InputFile);
            input = await File.ReadAllTextAsync(inputFile, cts.Token);
            sw.Stop();
            Log.Debug("Read input file {0} in {1}ms", inputFile, sw.ElapsedMilliseconds);
        }

        sw.Restart();
        var ast = Parser.Parser.ParseQuery(input);
        sw.Stop();
        Log.Debug("Generated AST in {0}ms", sw.ElapsedMilliseconds);

        var concurrencyCount = options.Sync ? 1 : options.Concurrency;
        Log.Debug("Concurrency limit set to {0}", concurrencyCount);

        var appConfig = new AppConfig
        {
            Debug = options.Debug,
            OutputFormat = options.Format,
            MaxConcurrency = concurrencyCount,
            Sync = options.Sync,
        };
        
        var output = await
            new AppContext(ast, appConfig).ExecuteAsync(cts.Token);

        OutputProcessor.Process(output, appConfig);
        Log.Debug("Finished in {0}ms", programSw.ElapsedMilliseconds);
    }

    private static void ConfigureLogging(int verboseCount)
    {
        var logLevel = verboseCount switch
        {
            1 => LogEventLevel.Warning,
            2 => LogEventLevel.Information,
            3 => LogEventLevel.Debug,
            4 => LogEventLevel.Verbose,
            _ => LogEventLevel.Error,
        };

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel)
            .WriteTo.Console()
            .CreateLogger();
    }
}