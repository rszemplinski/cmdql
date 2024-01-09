using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.HostInfo;

[Action]
[Cmd("arch")]
[Regex("(.*)")]
public class Architecture : ActionBase<string>;
