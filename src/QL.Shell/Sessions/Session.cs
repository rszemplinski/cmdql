using System.Diagnostics;
using System.Runtime.InteropServices;
using Renci.SshNet;
using Serilog;

namespace QLShell.Sessions;

public abstract class Session(SessionInfo info) : ISession
{
    public virtual bool IsConnected { get; protected set; }
    public SessionInfo Info { get; } = info;

    public abstract Task ConnectAsync(CancellationToken cancellationToken = default);

    public abstract Task<string> ExecuteCommandAsync(string command);

    public abstract Task DisconnectAsync(CancellationToken cancellationToken = default);

    public override string ToString()
    {
        // Check if alias is not the same as host
        var alias = Info.Alias == Info.Host ? string.Empty : $" ({Info.Alias})";
        return alias == string.Empty
            ? $"{alias} {Info.Username}@{Info.Host}:{Info.Port}"
            : $"{Info.Username}@{Info.Host}:{Info.Port}";
    }
}

public class LocalSession(SessionInfo info) : Session(info)
{
    public override Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        IsConnected = true;
        return Task.CompletedTask;
    }

    public override async Task<string> ExecuteCommandAsync(string command)
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var fileName = isWindows ? "cmd" : "bash";
        var arguments = isWindows ? $"/C {command}" : $"-c \"{command}\"";
      
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = startInfo;
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        return output;
    }

    public override Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        IsConnected = false;
        return Task.CompletedTask;
    }
}

public class RemoteSession(SessionInfo info) : Session(info)
{
    public override bool IsConnected => _client?.IsConnected ?? false;

    private SshClient? _client;

    public override async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
            return;

        try
        {
            _client = Info.IsUsingKeyFile
                ? new SshClient(Info.Host, Info.Port, Info.Username, new PrivateKeyFile(Info.KeyFile))
                : new SshClient(Info.Host, Info.Port, Info.Username, Info.Password);

            await _client.ConnectAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while connecting to session: {0}", this);
        }
    }

    public override async Task<string> ExecuteCommandAsync(string command)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Session is not connected");

        try
        {
            var result = await Task.Run(() => _client?.RunCommand(command));
            return result?.Result ?? string.Empty;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while executing command: {0}", command);
            return string.Empty;
        }
    }

    public override async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            return;

        try
        {
            await Task.Run(() => _client?.Disconnect(), cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while disconnecting from session: {0}", this);
        }
    }
}