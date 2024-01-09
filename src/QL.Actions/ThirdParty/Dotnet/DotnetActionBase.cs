using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.Dotnet;

[Deps(["dotnet"])]
[Namespace("dotnet")]
public abstract class DotnetActionBase<TArgs, TReturnType> : ActionBase<TArgs, TReturnType>
    where TArgs : class, new();


public abstract class DotnetActionBase<TReturnType> : DotnetActionBase<object, TReturnType>;
