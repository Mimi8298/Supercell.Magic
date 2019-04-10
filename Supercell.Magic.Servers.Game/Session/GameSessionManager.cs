namespace Supercell.Magic.Servers.Game.Session
{
    using System;
    using System.Collections.Generic;

    using Supercell.Magic.Servers.Core.Network.Message.Session;

    public static class GameSessionManager
    {
        private static Dictionary<long, GameSession> m_sessions;

        public static int Count
        {
            get { return GameSessionManager.m_sessions.Count; }
        }

        public static void Init()
        {
            GameSessionManager.m_sessions = new Dictionary<long, GameSession>();
        }

        public static void OnStartServerSessionMessageReceived(StartServerSessionMessage message)
        {
            if (GameSessionManager.m_sessions.ContainsKey(message.SessionId))
                throw new Exception("GameSessionManager.onStartSessionMessageReceived: session already started!");
            GameSessionManager.m_sessions.Add(message.SessionId, new GameSession(message));
        }

        public static void OnStopServerSessionMessageReceived(StopServerSessionMessage message)
        {
            if (GameSessionManager.m_sessions.Remove(message.SessionId, out GameSession session))
                session.Destruct();
        }

        public static bool TryGet(long id, out GameSession session)
        {
            return GameSessionManager.m_sessions.TryGetValue(id, out session);
        }

        public static GameSession Remove(long id)
        {
            if (GameSessionManager.m_sessions.Remove(id, out GameSession session))
                session.Destruct();
            return session;

        }
    }
}