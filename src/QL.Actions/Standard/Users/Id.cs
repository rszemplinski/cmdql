using QL.Core;
using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.Users;

public class GroupInfo
{
    public uint Id { get; set; }
    public string Name { get; set; }
}

public class IdResults
{
    public string Username { get; set; }
    public uint Uid { get; set; }
    public GroupInfo PrimaryGroup { get; set; }
    public List<GroupInfo> Groups { get; set; }
}

[Action]
[Cmd("id")]
public class Id : ActionBase<IdResults>
{
    protected override IdResults? ParseCommandResults(ICommandOutput commandResults)
    {
        var result = commandResults.Result;
        var idResults = new IdResults();
        var parts = result.Split(' ');

        // Parse UID and username
        var uidPart = parts[0].Split('=');
        idResults.Uid = uint.Parse(uidPart[1].Split('(')[0]);
        idResults.Username = uidPart[1].Split('(')[1].Split(')')[0];

        // Parse primary GID and group name
        var gidPart = parts[1].Split('=');
        idResults.PrimaryGroup = new GroupInfo
        {
            Id = uint.Parse(gidPart[1].Split('(')[0]),
            Name = gidPart[1].Split('(')[1].Split(')')[0]
        };

        // Parse additional groups
        var groupsPart = parts[2].Substring(7).Split(',');
        idResults.Groups = [];
        foreach (var group in groupsPart)
        {
            var groupDetails = group.Split('(');
            idResults.Groups.Add(new GroupInfo
            {
                Id = uint.Parse(groupDetails[0]),
                Name = groupDetails[1].TrimEnd(')')
            });
        }

        return idResults;
    }
}