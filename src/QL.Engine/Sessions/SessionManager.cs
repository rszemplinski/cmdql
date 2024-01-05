using QL.Parser.AST.Nodes;

namespace QL.Engine.Sessions;

public class SessionManager
{
    public Dictionary<string, (ISession session, ContextBlockNode contextBlockNode)> Sessions { get; }
        = new();

    public void AddSession(ISession session, ContextBlockNode contextBlockNode)
    {
        Sessions.Add(session.Info.Alias, (session, contextBlockNode));
    }

    public void RemoveSession(string alias)
    {
        Sessions.Remove(alias);
    }

    public TSession GetSession<TSession>(string alias) where TSession : class, ISession
    {
        if (TryGetSession(alias, out var session))
        {
            return session.session as TSession ??
                   throw new InvalidCastException($"Session {alias} is not of type {typeof(TSession)}");
        }

        throw new KeyNotFoundException($"Session {alias} not found");
    }

    public TContextBlock GetContextBlock<TContextBlock>(string alias) where TContextBlock : ContextBlockNode
    {
        if (TryGetSession(alias, out var session))
        {
            return session.contextBlockNode as TContextBlock ??
                   throw new InvalidCastException($"Context block {alias} is not of type {typeof(TContextBlock)}");
        }

        throw new KeyNotFoundException($"Context block {alias} not found");
    }

    private bool TryGetSession(string alias, out (ISession session, ContextBlockNode contextBlockNode) session)
    {
        return Sessions.TryGetValue(alias, out session);
    }
}