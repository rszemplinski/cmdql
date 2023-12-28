using CommandLine;

namespace QLShell;

public enum OutputFormat
{
    Json,
    Yml,
    Table,
}

public class Options
{
    [Option(
        'i',
        "input",
        Required = true,
        HelpText = "Input files to be processed."
    )]
    public required string InputFile { get; set; }

    [Option(
        'v',
        "verbose",
        HelpText = "Show verbose output?"
    )]
    public bool Verbose { get; set; }
    
    [Option(
        'd',
        "debug",
        Default = false,
        HelpText = "Debug mode"
    )]
    public bool Debug { get; set; }

    [Option(
        'f',
        "format",
        Default = OutputFormat.Json,
        HelpText = "Which format to use? (json, yml, table)"
    )]
    public OutputFormat Format { get; set; }
    
    [Option(
        'o',
        "output",
        HelpText = "Output file"
    )]
    public string OutputFile { get; set; }

    [Option(
        'c',
        "concurrency",
        HelpText = "Concurrency limit"
    )]
    public int Concurrency { get; set; } = Environment.ProcessorCount;
    
    [Option(
        "sync",
        Default = false,
        HelpText = "Run synchronously?"
    )]
    public bool Sync { get; set; }
}