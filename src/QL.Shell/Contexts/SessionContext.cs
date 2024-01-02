using System.Collections.Concurrent;
using QL.Actions;
using QL.Core;
using QL.Core.Actions;
using QL.Parser.AST.Nodes;
using QLShell.Extensions;
using QLShell.Sessions;

namespace QLShell.Contexts;

public class SessionContext(ISession session, IEnumerable<SelectionNode> selectionSet)
{
    private ISession Session { get; } = session;
    private IEnumerable<SelectionNode> SelectionSet { get; } = selectionSet;

    public async Task<IReadOnlyDictionary<string, object>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var result = new ConcurrentDictionary<string, object>();
        var fields = GetTopLevelFields().ToList();

        var tasks = fields.Select(fieldNode =>
        {
            var action = ActionsLookupTable.Get(fieldNode.Name).CreateAction(Session.Platform, Session.RawPlatform);
            var arguments = fieldNode.BuildArgumentsDictionary();
            var sessionSshClient = new SessionClient(Session);
            var allFields = new Field(fieldNode).Fields;
            return Task.Run(async () =>
            {
                var response = await action
                    .ExecuteCommandAsync(sessionSshClient, arguments,
                        allFields, cancellationToken);

                result.TryAdd(fieldNode.Name, response);
            }, cancellationToken);
        });

        await Task.WhenAll(tasks);

        return result;
    }

    private IEnumerable<FieldNode> GetTopLevelFields()
        => SelectionSet.Select(selection => selection.Field);

    private class Field : IField
    {
        public string Name { get; }
        public IField[] Fields { get; }

        public Field(FieldNode field)
        {
            Name = field.Name;
            Fields = field.SelectionSet?
                .Select(x => new Field(x.Field) as IField)
                .ToArray() ?? [];
        }
    }

    private class SessionClient(ISession session) : IClient
    {
        private ISession Session { get; } = session;

        public Task<ICommandOutput> ExecuteCommandAsync(string command, CancellationToken cancellationToken)
        {
            return Session.ExecuteCommandAsync(command, cancellationToken);
        }

        public Task<ICommandOutput> UploadFileAsync(string localPath, string remotePath,
            CancellationToken cancellationToken = default)
        {
            return Session.UploadFileAsync(localPath, remotePath, cancellationToken);
        }

        public Task<ICommandOutput> DownloadFileAsync(string remotePath, string localPath,
            CancellationToken cancellationToken = default)
        {
            return Session.DownloadFileAsync(remotePath, localPath, cancellationToken);
        }

        public override string ToString()
        {
            return Session.ToString()!;
        }
    }
}