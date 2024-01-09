using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.Github;

[Deps(["gh"])]
[Namespace("github")]
public class GithubActionBase<TArgs, TReturnType> : ActionBase<TArgs, TReturnType>
    where TArgs : class, new();

public class GithubActionBase<TReturnType> : GithubActionBase<object, TReturnType>;