using CommandLine;
using QL.Actions.Core;
using Serilog;
using AppContext = QLShell.Contexts.AppContext;
using File = System.IO.File;
using Parser = QL.Parser.Parser;

namespace QLShell;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        await CommandLine.Parser.Default
            .ParseArguments<CLIOptions>(args)
            .WithParsedAsync(Run);
    }

    private static async Task Run(CLIOptions options)
    {
        var inputFile = Path.GetFullPath(options.InputFile);
        ConfigureLogging(options.Verbose);

        try
        {
            // Load file
            var input = await File.ReadAllTextAsync(inputFile);
            Log.Verbose("Loaded file: {0}", inputFile);
            
            // Generate AST
            var ast = Parser.ParseQuery(input);

            Log.Information("Executing query...");
            var context = new AppContext(ast);
            await context.ExecuteAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An error occurred while executing the query");
        }
    }

    private static void ConfigureLogging(bool verbose)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(verbose
                ? Serilog.Events.LogEventLevel.Verbose
                : Serilog.Events.LogEventLevel.Information)
            .WriteTo.Console()
            .CreateLogger();
    }
}