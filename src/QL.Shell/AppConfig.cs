namespace QLShell;

public class AppConfig
{
    public string InputFile { get; init; } = string.Empty;
    public bool Verbose { get; init; }
    public bool Debug { get; init; }
    public bool Sync { get; init; }
    public int MaxConcurrency { get; init; }
    public OutputFormat OutputFormat { get; init; } = OutputFormat.Json;
    public string? OutputFile { get; init; }
}