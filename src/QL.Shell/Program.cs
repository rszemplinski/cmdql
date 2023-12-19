using CommandLine;
using Serilog;
using Parser = QL.Parser.Parser;

namespace QLShell;

internal static class Program
{
    private class Options
    {
        [Option(
            'i',
            "input",
            Required = true,
            HelpText = "Input files to be processed.")]
        public string InputFile { get; set; }

        [Option(
            'v',
            "verbose",
            Default = false,
            HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }
    }
    
    private static async Task Main(string[] args)
    {
        await CommandLine.Parser.Default
            .ParseArguments<Options>(args)
            .WithParsedAsync(Run);
    }

    private static async Task Run(Options options)
    {
        var inputFile = options.InputFile;
        ConfigureLogging(options.Verbose);

        try
        {
            // Load file
            var input = await File.ReadAllTextAsync(inputFile);
            Log.Debug("Loaded file: {0}", inputFile);
            
            // Generate AST
            var ast = Parser.ParseQuery(input);
            Log.Verbose("Generated AST");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while parsing file: {0}", inputFile);
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