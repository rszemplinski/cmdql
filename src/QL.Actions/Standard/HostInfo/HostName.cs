using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.HostInfo;

[Action]
[Cmd("hostname")]
[Regex("(.*)")]
public class HostName : ActionBase<string>;
