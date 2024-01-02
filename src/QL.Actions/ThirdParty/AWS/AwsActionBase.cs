using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.AWS;

[Deps(["aws"])]
public class AwsActionBase<TArgs, TReturnType> : ActionBase<TArgs, TReturnType>
    where TArgs: class, new()
    where TReturnType: class, new();
