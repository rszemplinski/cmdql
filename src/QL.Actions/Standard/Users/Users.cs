using QL.Core;
using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.Users;

[Action]
[Cmd("users")]
public class Users : ActionBase<List<string>>
{
    protected override List<string>? ParseCommandResults(ICommandOutput commandResults)
    {
        var results = commandResults.Result.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        return results.Select(x => x.Trim()).ToList();
    }
}