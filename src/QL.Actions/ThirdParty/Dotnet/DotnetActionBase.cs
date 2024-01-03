using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.Dotnet;

[Deps(["dotnet"])]
public class DotnetActionBase<TArgs, TReturnType> : ActionBase<TArgs, TReturnType>
    where TArgs: class, new()
    where TReturnType: class, new();