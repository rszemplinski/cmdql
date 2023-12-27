using CommandLine;

namespace QLShell;

public enum OutputFormat
{
    Json,
    Table,
}

// ReSharper disable once InconsistentNaming
public class CLIOptions
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
        Default = false,
        HelpText = "Show verbose output."
    )]
    public bool Verbose { get; set; }
    
    [Option(
        'd',
        "debug",
        Default = false,
        HelpText = "Debug mode."
    )]
    public bool Debug { get; set; }

    [Option(
        'o',
        "output",
        Default = OutputFormat.Json,
        HelpText = "Which output format to use. (Json, Table)"
    )]
    public OutputFormat OutputFormat { get; set; }

    [Option(
        'c',
        "concurrency",
        Default = 8,
        HelpText = "Concurrency limit."
    )]
    public int MaxDegreeOfParallelism { get; set; }
    
    [Option(
        "sync",
        Default = false,
        HelpText = "Run synchronously."
    )]
    public bool Sync { get; set; }
}