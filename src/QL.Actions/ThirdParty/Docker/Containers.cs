using System.Text.RegularExpressions;
using QL.Core;
using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.Docker;

public record ContainerArgs
{
    public int Limit { get; set; } = -1;
}

public class ContainerResult
{
    public string Id { get; set; }
    public string Image { get; set; }
    public string Command { get; set; }
    public string Created { get; set; }
    public string Status { get; set; }
    public string Ports { get; set; }
    public string Names { get; set; }
}

[Action]
[Cmd("docker ps")]
[Regex(
    """^(?<containerId>\S+)\s+(?<image>[\w\/\:\-\.]+)\s+\"(?<command>[^\"]+)\"\s+(?<created>[\w\s]+ago)\s+(?<status>(?:Up|Exited)\s[\w\s]+?)(?:\s{2,}(?<ports>[\w\-\/]+))?(\s+)(?<names>\S+)$""")]
public class Containers : DockerActionBase<ContainerArgs, List<ContainerResult>>
{
    protected override List<ContainerResult> ParseCommandResults(ICommandOutput commandResults)
    {
        var results = new List<ContainerResult>();
        var regex = GetRegex();
        var matches = regex.Matches(commandResults.Result);
        foreach (var match in matches)
        {
            var containerResult = new ContainerResult();
            var groups = ((Match)match).Groups;
            containerResult.Id = groups["containerId"].Value;
            containerResult.Image = groups["image"].Value;
            containerResult.Command = groups["command"].Value;
            containerResult.Created = groups["created"].Value;
            containerResult.Status = groups["status"].Value;
            containerResult.Ports = groups["ports"].Value;
            containerResult.Names = groups["names"].Value;
            results.Add(containerResult);
        }

        return results;
    }
}