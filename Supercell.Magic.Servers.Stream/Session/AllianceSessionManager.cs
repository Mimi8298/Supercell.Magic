namespace Supercell.Magic.Servers.Stream.Session
{
    using System;
    using System.Collections.Generic;
    using Supercell.Magic.Servers.Core.Network.Message.Session;

    public static class AllianceSessionManager
    {
        private static Dictionary<long, AllianceSession> m_sessions;

        public static int Count
        {
            get { return AllianceSessionManager.m_sessions.Count; }
        }

        public static void Init()
        {
            AllianceSessionManager.m_sessions = new Dictionary<long, AllianceSession>();
        }

        public static void OnStartServerSessionMessageReceived(StartServerSessionMessage message)
        {
            if (AllianceSessionManager.m_sessions.ContainsKey(message.SessionId))
                throw new Exception("AllianceSessionManager.onStartSessionMessageReceived: session already started!");
            AllianceSessionManager.m_sessions.Add(message.SessionId, new AllianceSession(message));
        }

        public static void OnStopServerSessionMessageReceived(StopServerSessionMessage message)
        {
            if (AllianceSessionManager.m_sessions.Remove(message.SessionId, out AllianceSession session))
                session.Destruct();
        }

        public static bool TryGet(long id, out AllianceSession session)
        {
            return AllianceSessionManager.m_sessions.TryGetValue(id, out session);
        }

        public static AllianceSession Remove(long id)
        {
            if (AllianceSessionManager.m_sessions.Remove(id, out AllianceSession session))
                session.Destruct();
            return session;
        }
    }
}