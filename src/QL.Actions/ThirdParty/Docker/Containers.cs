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
    public List<ushort> Ports { get; set; }
    public string Name { get; set; }
}

[Action]
public class Containers : DockerActionBase<ContainerArgs, List<ContainerResult>>
{
    
}