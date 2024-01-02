using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.Docker;

[Deps(["docker"])]
public abstract class DockerActionBase<TArgs, TReturnType> : ActionBase<TArgs, TReturnType>
    where TArgs : class, new()
    where TReturnType : class, new();
