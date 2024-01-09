using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.Azure;

[Deps(["az"])]
[Namespace("azure")]
public abstract class AzureActionBase<TArgs, TReturnType> : ActionBase<TArgs, TReturnType>
    where TArgs : class, new();

public abstract class AzureActionBase<TReturnType> : AzureActionBase<object, TReturnType>;
