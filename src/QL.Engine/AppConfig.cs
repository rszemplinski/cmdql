namespace QL.Engine;

public class AppConfig
{
    public bool Debug { get; init; }
    public bool Sync { get; init; }
    public int MaxConcurrency { get; init; }
    public OutputFormat OutputFormat { get; init; } = OutputFormat.Json;
    public string? OutputFile { get; init; }
}