using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.Dotnet;

[Action]
[Cmd("dotnet --version")]
[Regex("(.*)")]
public class Version : DotnetActionBase<string>;
