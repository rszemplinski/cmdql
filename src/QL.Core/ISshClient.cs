namespace QL.Core;

public interface ICommandOutput
{
    public string Result { get; }
    public string Error { get; }
    public int ExitCode { get; }
}

public interface ISshClient
{
    public Task<ICommandOutput> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default);
}