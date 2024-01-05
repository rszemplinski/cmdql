using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.Docker;

public record DockerImagesArguments;

public class DockerImageResult
{
    public string Repository { get; set; }
    public string Tag { get; set; }
    public string ImageId { get; set; }
    public string Created { get; set; }
    public string Size { get; set; }
}

[Action]
[Cmd("docker images")]
[Regex(
    @"(?<Repository>\S+|\S+\/\S+|\S+\.\S+\/\S+\/\S+\/\S+|\S+\.\S+\/\S+\/\S+)\s+(?<Tag>\S+)\s+(?<ImageId>\S+)\s+(?<Created>\d+\s+months?\s+ago)\s+(?<Size>\S+)")]
public class Images : DockerActionBase<DockerImagesArguments, List<DockerImageResult>>;
