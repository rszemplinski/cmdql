using QLShell.Sessions;

namespace QLShell.Contexts;

public class Context<TSession>(TSession session)
    where TSession : ISession
{
    public TSession Session { get; } = session;
}