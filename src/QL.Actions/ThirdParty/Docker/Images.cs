using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.Docker;

public record DockerImagesArguments;

public class DockerImageResult
{
    /**
     * The repository of the image
     */
    public string Repository { get; set; }
    
    /**
     * The tag of the image
     */
    public string Tag { get; set; }
    
    /**
     * The image id
     */
    public string ImageId { get; set; }
    
    /**
     * The date the image was created
     */
    public string Created { get; set; }
    
    /**
     * The size of the image
     */
    public string Size { get; set; }
}

[Action]
[Cmd("docker images")]
[Regex(
    @"(?<Repository>\S+|\S+\/\S+|\S+\.\S+\/\S+\/\S+\/\S+|\S+\.\S+\/\S+\/\S+)\s+(?<Tag>\S+)\s+(?<ImageId>\S+)\s+(?<Created>\d+\s+months?\s+ago)\s+(?<Size>\S+)")]
public class Images : DockerActionBase<DockerImagesArguments, List<DockerImageResult>>;
