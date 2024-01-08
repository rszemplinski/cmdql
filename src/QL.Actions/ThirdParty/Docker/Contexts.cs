using System.Text.RegularExpressions;
using QL.Core;
using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.Docker;

public record ContextsArguments;

public class ContextResult
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public string DockerEndpoint { get; set; }
    public string KubernetesEndpoint { get; set; }
    public string Orchestrator { get; set; }
}

[Action]
[Cmd("docker context ls")]
public partial class Contexts : DockerActionBase<ContextsArguments, List<ContextResult>>
{
    protected override List<ContextResult> ParseCommandResults(ICommandOutput commandResults)
    {
        var regex = ContextRegex();
        var lines = commandResults.Result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        var results = new List<ContextResult>();
        foreach (var line in lines.Skip(1))
        {
            var match = regex.Match(line);
            if (!match.Success) continue;
            var result = new ContextResult
            {
                Name = match.Groups["Name"].Value,
                Type = match.Groups["Type"].Value,
                Description = match.Groups["Description"].Value,
                DockerEndpoint = match.Groups["DockerEndpoint"].Value,
                KubernetesEndpoint = match.Groups["KubernetesEndpoint"].Value,
                Orchestrator = match.Groups["Orchestrator"].Value
            };
            results.Add(result);
        }

        return results;
    }

    [GeneratedRegex(
        @"(?<Name>[^\s]+(?:\s+\*)?)\s+(?<Type>[^\s]+)\s+(?<Description>.+?)\s{2,}(?<DockerEndpoint>[^\s]+)(\s{2,}(?<KubernetesEndpoint>[^\s]+(?:\s+\([^)]*\))?)?)?\s{2,}(?<Orchestrator>[^\s]+)?")]
    private static partial Regex ContextRegex();
}