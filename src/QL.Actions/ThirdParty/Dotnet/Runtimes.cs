using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.Dotnet;

public record RuntimeArgs;

public class RuntimeResult
{
    public int Major => int.Parse(Version.Split('.')[0]);
    public int Minor => int.Parse(Version.Split('.')[1]);
    public int Patch => int.Parse(Version.Split('.')[2]);
    
    public string Package { get; set; }
    public string Version { get; set; }
    public string Path { get; set; }
}

[Action]
[Cmd("dotnet --list-runtimes")]
[Regex(@"(?<package>[^\s]+)\s+(?<version>\d+\.\d+\.\d+)\s+\[(?<path>.+?)\]")]
public class Runtimes : DotnetActionBase<RuntimeArgs, List<RuntimeResult>>;