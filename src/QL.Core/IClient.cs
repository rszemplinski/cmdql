namespace QL.Core;

public interface ICommandOutput
{
    /**
     * Result output of the command.
     */
    public string Result { get; }

    /**
     * Error output of the command.
     */
    public string Error { get; }

    /**
     * Exit code of the command.
     */
    public int ExitCode { get; }
}

public interface IClient
{
    /**
     * Execute a command on the client.
     */
    public Task<ICommandOutput> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default);

    /**
     * Upload a file to the client.
     */
    public Task<ICommandOutput> UploadFileAsync(string localPath, string remotePath,
        CancellationToken cancellationToken = default);

    /**
     * Download a file from the client.
     */
    public Task<ICommandOutput> DownloadFileAsync(string remotePath, string localPath,
        CancellationToken cancellationToken = default);

}