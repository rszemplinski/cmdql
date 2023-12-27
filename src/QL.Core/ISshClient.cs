namespace QL.Core;

public interface ISshClient
{
    public Task<string> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default);
}