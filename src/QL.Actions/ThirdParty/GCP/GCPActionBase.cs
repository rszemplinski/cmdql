using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.GCP;

[Deps(["gcloud"])]
public class GCPActionBase<TArgs, TReturnType> : ActionBase<TArgs, TReturnType>
    where TArgs: class, new()
    where TReturnType: class, new();
