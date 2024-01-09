using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.AWS;

[Deps(["aws"])]
[Namespace("aws")]
public abstract class AwsActionBase<TArgs, TReturnType> : ActionBase<TArgs, TReturnType>
    where TArgs : class, new();

public abstract class AwsActionBase<TReturnType> : AwsActionBase<object, TReturnType>;
