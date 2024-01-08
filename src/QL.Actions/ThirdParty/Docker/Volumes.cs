using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.Docker;

public record VolumesArguments;

public class VolumeResult
{
    public string Driver { get; set; }
    public string Name { get; set; }
}

[Action]
[Cmd("docker volume ls")]
[Regex(@"(?<Driver>\w+)\s+(?<Name>[a-zA-Z0-9-_]+(?:-[a-zA-Z0-9]+)+)")]
public class Volumes : DockerActionBase<VolumesArguments, List<VolumeResult>>;
