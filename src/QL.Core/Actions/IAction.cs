namespace QL.Core.Actions;

public interface IAction
{
    public Task<object> ExecuteCommandAsync(
        IClient client,
        IReadOnlyDictionary<string, object> arguments,
        IReadOnlyCollection<IField> fields, CancellationToken cancellationToken = default);
}