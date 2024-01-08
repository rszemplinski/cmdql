using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.Docker;

public record NetworksArguments;

public class NetworkResult
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Driver { get; set; }
    public string Scope { get; set; }
}

[Action]
[Cmd("docker network ls")]
[Regex(@"(?<Id>[a-f0-9]{12})\s+(?<Name>\S+)\s+(?<Driver>\S+)\s+(?<Scope>\w+)")]
public class Networks : DockerActionBase<NetworksArguments, List<NetworkResult>>;
