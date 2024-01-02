using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.Azure;

[Deps(["az"])]
public class AzureActionBase<TArgs, TReturnType> : ActionBase<TArgs, TReturnType>
    where TArgs: class, new()
    where TReturnType: class, new();
