namespace Supercell.Magic.Servers.Proxy.Session
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Database.Document;
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Servers.Proxy.Network;

    public static class ProxySessionManager
    {
        private static ConcurrentDictionary<long, ProxySession> m_sessions;
        private static Thread m_updateThread;

        private static long m_counter;

        public static int Count
        {
            get { return ProxySessionManager.m_sessions.Count; }
        }

        public static void Init()
        {
            ProxySessionManager.m_sessions = new ConcurrentDictionary<long, ProxySession>();
            ProxySessionManager.m_counter = TimeUtil.GetTimestamp() & 0x007FFFFFFFFFFFFF;
            ProxySessionManager.m_updateThread = new Thread(ProxySessionManager.Update);
            ProxySessionManager.m_updateThread.Start();
        }

        private static void Update()
        {
            while (true)
            {
                foreach (ProxySession session in ProxySessionManager.m_sessions.Values)
                {
                    session.Update();
                }

                Thread.Sleep(5000);
            }
        }

        private static long GetScrambledId()
        {
            long id;

            do
            {
                id = ProxySessionManager.m_counter;
                ProxySessionManager.m_counter = (ProxySessionManager.m_counter + 1) & 0x007FFFFFFFFFFFFF;
            } while (ProxySessionManager.m_sessions.ContainsKey(id));

            return ((long) ServerCore.Id << 55) | id;
        }

        public static ProxySession Create(ClientConnection clientConnection, AccountDocument accountDocument)
        {
            long id = ProxySessionManager.GetScrambledId();

            ProxySession proxySession = new ProxySession(id, clientConnection, accountDocument);
            Logging.Assert(ProxySessionManager.m_sessions.TryAdd(id, proxySession), "ProxySessionManager.m_sessions.TryAdd(id, proxySession) return false");

            return proxySession;
        }

        public static bool TryGet(long id, out ProxySession session)
        {
            return ProxySessionManager.m_sessions.TryGetValue(id, out session);
        }

        public static void Remove(ProxySession session)
        {
            ProxySessionManager.m_sessions.TryRemove(session.Id, out _);
        }
    }
}