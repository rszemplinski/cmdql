using QL.Core;
using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.Users;

[Action]
[Cmd("groups")]
public class Groups : ActionBase<List<string>>
{
    protected override List<string>? ParseCommandResults(ICommandOutput commandResults)
    {
        var results = commandResults.Result.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        return results.Select(x => x.Trim()).ToList();
    }
}
