using System.Text.Json;
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

    public async Task<Dictionary<string, object>> ExecuteAsync()
    {
        var result = new Dictionary<string, object>();
        var fields = GetSelectionSetFields();
        foreach (var field in fields)
        {
            var action = ActionsLookupTable.Get(field.Name).CreateAction();
            var arguments = field.BuildArgumentsDictionary();
            var command = action.BuildCommand(arguments);
            var response = await Session.ExecuteCommandAsync(command);
            var commandResults = action.ParseCommandResults(response);
            result.Add(field.Name, commandResults);
        }
        return result;
    }

    private IEnumerable<FieldNode> GetSelectionSetFields()
    {
        return SelectionSet
            .Select(selection => selection.Field)
            .Where(field => field.SelectionSet != null);
    }
}