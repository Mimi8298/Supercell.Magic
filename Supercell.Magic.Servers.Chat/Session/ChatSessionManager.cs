namespace Supercell.Magic.Servers.Chat.Session
{
    using System;
    using System.Collections.Generic;
    using Supercell.Magic.Servers.Core.Network.Message.Session;

    public static class ChatSessionManager
    {
        private static Dictionary<long, ChatSession> m_sessions;

        public static int Count
        {
            get { return ChatSessionManager.m_sessions.Count; }
        }

        public static void Init()
        {
            ChatSessionManager.m_sessions = new Dictionary<long, ChatSession>();
        }

        public static void OnStartServerSessionMessageReceived(StartServerSessionMessage message)
        {
            if (ChatSessionManager.m_sessions.ContainsKey(message.SessionId))
                throw new Exception("ChatSessionManager.onStartSessionMessageReceived: session already started!");
            ChatSessionManager.m_sessions.Add(message.SessionId, new ChatSession(message));
        }

        public static void OnStopServerSessionMessageReceived(StopServerSessionMessage message)
        {
            if (ChatSessionManager.m_sessions.Remove(message.SessionId, out ChatSession session))
                session.Destruct();
        }

        public static bool TryGet(long id, out ChatSession session)
        {
            return ChatSessionManager.m_sessions.TryGetValue(id, out session);
        }
    }
}