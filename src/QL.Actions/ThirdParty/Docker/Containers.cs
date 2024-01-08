using System.Text.RegularExpressions;
using QL.Core;
using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.Docker;

public record ContainerArgs
{
    public int Limit { get; set; } = -1;
}

public enum Protocol { Tcp, Udp }

public class ContainerPort
{
    public ushort Port { get; set; }
    public Protocol Protocol { get; set; }
}

public class ContainerResult
{
    public string Id { get; set; }
    public string Image { get; set; }
    public string Command { get; set; }
    public string Created { get; set; }
    public string Status { get; set; }
    public List<ContainerPort> Ports { get; set; }
    public string Names { get; set; }
}

[Action]
[Cmd("docker ps")]
public partial class Containers : DockerActionBase<ContainerArgs, List<ContainerResult>>
{
    protected override List<ContainerResult> ParseCommandResults(ICommandOutput commandResults)
    {
        var results = new List<ContainerResult>();
        var lines = commandResults.Result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            var match = ContainerRegex().Match(line);

            if (match.Success)
            {
                results.Add(new ContainerResult
                {
                    Id = match.Groups[1].Value,
                    Image = match.Groups[2].Value,
                    Command = match.Groups[3].Value.Trim('"'),
                    Created = match.Groups[4].Value,
                    Status = match.Groups[5].Value,
                    Ports = ParsePorts(match.Groups[6].Value),
                    Names = match.Groups[7].Value
                });
            }
        }

        return results;
    }
    
    private static List<ContainerPort> ParsePorts(string ports)
    {
        var results = new List<ContainerPort>();
        var portStrings = ports.Split(", ");

        foreach (var portString in portStrings)
        {
            var portMatch = PortRegex().Match(portString);
            if (portMatch.Success)
            {
                results.Add(new ContainerPort
                {
                    Port = ushort.Parse(portMatch.Groups[1].Value),
                    Protocol = portMatch.Groups[2].Value switch
                    {
                        "tcp" => Protocol.Tcp,
                        "udp" => Protocol.Udp,
                        _ => throw new ArgumentOutOfRangeException()
                    }
                });
            }
        }

        return results;
    }
    
    [GeneratedRegex(@"^(\d+)/(tcp|udp)$")]
    private static partial Regex PortRegex();

    [GeneratedRegex("""^(\S+)\s+(\S+)\s+(\".+?\"|\S+)\s+(\d+\s+\S+\s+ago)\s+(Up\s+\d+\s+\S+(?:\s+\([^)]+\))?)\s*(.*?)\s+(\S+)$""")]
    private static partial Regex ContainerRegex();
}