namespace Supercell.Magic.Servers.Battle.Session
{
    using System;
    using System.Collections.Generic;
    using Supercell.Magic.Servers.Core.Network.Message.Session;

    public class BattleSessionManager
    {
        private readonly Dictionary<long, BattleSession> m_sessions;

        public int Count
        {
            get { return this.m_sessions.Count; }
        }

        public BattleSessionManager()
        {
            this.m_sessions = new Dictionary<long, BattleSession>();
        }

        public void OnStartServerSessionMessageReceived(StartServerSessionMessage message)
        {
            if (this.m_sessions.ContainsKey(message.SessionId))
                throw new Exception("BattleSessionManager.onStartSessionMessageReceived: session already started!");
            this.m_sessions.Add(message.SessionId, new BattleSession(message));
        }

        public void OnStopServerSessionMessageReceived(StopServerSessionMessage message)
        {
            if (this.m_sessions.Remove(message.SessionId, out BattleSession session))
                session.Destruct();
        }

        public bool TryGet(long id, out BattleSession session)
        {
            return this.m_sessions.TryGetValue(id, out session);
        }
    }
}