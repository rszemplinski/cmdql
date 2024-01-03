using System.Runtime.InteropServices;

namespace QL.Core.Actions;

public interface IAction
{
    public void Initialize(OSPlatform platform);
    
    public Task<object> ExecuteCommandAsync(
        IClient client,
        IReadOnlyDictionary<string, object> arguments,
        IReadOnlyCollection<IField> fields, CancellationToken cancellationToken = default);
}