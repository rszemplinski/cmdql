using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.ThirdParty.GCP;

[Deps(["gcloud"])]
[Namespace("gcp")]
public abstract class GCPActionBase<TArgs, TReturnType> : ActionBase<TArgs, TReturnType>
    where TArgs : class, new();

public abstract class GCPActionBase<TReturnType> : GCPActionBase<object, TReturnType>;
