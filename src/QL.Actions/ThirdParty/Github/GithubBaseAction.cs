using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.Github;

[Deps(["gh"])]
public class GithubBaseAction<TArgs, TReturnType> : ActionBase<TArgs, TReturnType>
    where TArgs: class, new()
    where TReturnType: class, new();
