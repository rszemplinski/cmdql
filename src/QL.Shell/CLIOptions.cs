using CommandLine;

namespace QLShell;

// ReSharper disable once InconsistentNaming
public class CLIOptions
{
    [Option(
        'i',
        "input",
        Required = true,
        HelpText = "Input files to be processed.")]
    public required string InputFile { get; set; }

    [Option(
        'v',
        "verbose",
        Default = false,
        HelpText = "Prints all messages to standard output.")]
    public bool Verbose { get; set; }
}