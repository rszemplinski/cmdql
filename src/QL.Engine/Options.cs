using CommandLine;

namespace QL.Engine;

public enum OutputFormat
{
    Json,
    Yml,
    Table,
}

public class Options
{
    
    [Value(0, Required = false, HelpText = "Raw query string - ex: \"fetch { local { macAddr } }\"")]
    public string Query { get; set; }
    
    [Option(
        'i',
        "input",
        HelpText = "Input file"
    )]
    public required string InputFile { get; set; }
    
    [Option(
        'o',
        "output",
        HelpText = "Output file"
    )]
    public string OutputFile { get; set; }
    
    [Option(
        'f',
        "format",
        Default = OutputFormat.Json,
        HelpText = "Which format to use? (json, yml, table)"
    )]
    public OutputFormat Format { get; set; }
    
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

    [Option(
        'v',
        "verbose",
        HelpText = "Show verbose output?",
        FlagCounter = true
    )]
    public int Verbose { get; set; }
    
    [Option(
        'd',
        "debug",
        Default = false,
        HelpText = "Debug mode"
    )]
    public bool Debug { get; set; }
}