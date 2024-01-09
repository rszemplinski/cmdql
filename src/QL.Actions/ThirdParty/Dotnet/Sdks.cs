using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.Dotnet;

public class SdkResult
{
    public int Major => int.Parse(Version.Split('.')[0]);
    public int Minor => int.Parse(Version.Split('.')[1]);
    public int Patch => int.Parse(Version.Split('.')[2]);
    public string Version { get; set; }
    public string Path { get; set; }
}

[Action]
[Cmd("dotnet --list-sdks")]
[Regex(@"(?<version>\d+\.\d+\.\d+)\s+\[(?<path>.+?)\]")]
public class Sdks : DotnetActionBase<List<SdkResult>>;
