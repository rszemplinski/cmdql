using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.Users;

[Action]
[Cmd("whoami")]
[Regex("(.*)")]
public class WhoAmI : ActionBase<string>;
