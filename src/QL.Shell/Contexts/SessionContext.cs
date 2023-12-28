using System.Collections.Concurrent;
using System.Diagnostics;
using QL.Actions;
using QL.Core;
using QL.Core.Actions;
using QL.Parser.AST.Nodes;
using QLShell.Extensions;
using QLShell.Sessions;
using Serilog;

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
            var action = ActionsLookupTable.Get(fieldNode.Name).CreateAction();
            var arguments = fieldNode.BuildArgumentsDictionary();
            var sessionSshClient = new SessionSshClient(Session);
            return TaskLimiter.Instance.ProcessAsync(async () =>
            {
                var command = await action.BuildCommandAsync(arguments);

                var sw = Stopwatch.StartNew();
                var response = await Session.ExecuteCommandAsync(command, cancellationToken);
                sw.Stop();
                Log.Debug("[{0}] => Executed command: {1} in {2}ms", Session, command, sw.ElapsedMilliseconds);

                sw.Restart();
                response = await action.PostExecutionAsync(response, sessionSshClient);
                sw.Stop();
                Log.Debug("[{0}] => Post execution in {1}ms", Session, sw.ElapsedMilliseconds);

                sw.Restart();
                var commandResults = await action.ParseCommandResultsAsync(response, new Field(fieldNode).Fields);
                sw.Stop();
                Log.Debug("[{0}] => Parsed command results in {1}ms", Session, sw.ElapsedMilliseconds);

                result.TryAdd(fieldNode.Name, commandResults);
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

    private class SessionSshClient(ISession session) : ISshClient
    {
        private ISession Session { get; } = session;

        public async Task<string> ExecuteCommandAsync(string command, CancellationToken cancellationToken)
        {
            return await Session.ExecuteCommandAsync(command, cancellationToken);
        }
    }
}