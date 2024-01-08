using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.Dotnet;

public record VersionArgs;

[Action]
[Cmd("dotnet --version")]
[Regex("(.*)")]
public class Version : DotnetActionBase<VersionArgs, string>;
