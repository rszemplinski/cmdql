using System.Collections.Concurrent;
using System.Diagnostics;
using QL.Actions.Core;
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
        
        var tasks = fields.Select(field =>
        {
            return TaskLimiter.Instance.ProcessAsync(async () =>
            {
                var selectedFields = GetFields(field)
                    .Select(x => x.Name)
                    .ToArray();
                
                var action = ActionsLookupTable.Get(field.Name).CreateAction();
                var arguments = field.BuildArgumentsDictionary();
                var command = action.BuildCommand(arguments);
                
                var sw = Stopwatch.StartNew();
                var response = await Session.ExecuteCommandAsync(command, cancellationToken);
                sw.Stop();
                Log.Debug("[{0}] => Executed command: {1} in {2}ms", Session, command, sw.ElapsedMilliseconds);
                
                sw.Restart();
                var commandResults = action.ParseCommandResults(response, selectedFields);
                sw.Stop();
                Log.Debug("[{0}] => Parsed command results in {1}ms", Session, sw.ElapsedMilliseconds);
                
                result.TryAdd(field.Name, commandResults);
            }, cancellationToken);
        });
        
        await Task.WhenAll(tasks);
        
        return result;
    }

    private IEnumerable<FieldNode> GetTopLevelFields()
    {
        return SelectionSet
            .Select(selection => selection.Field);
    }
    
    private static IEnumerable<FieldNode> GetFields(FieldNode field)
    {
        if (field.SelectionSet == null || !field.SelectionSet.Any())
            throw new InvalidOperationException($"Field {field.Name} does not have a selection set");
        
        return field.SelectionSet
            .Select(selection => selection.Field);
    }
}