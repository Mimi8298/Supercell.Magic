namespace Supercell.Magic.Servers.Game.Logic
{
    using System.Collections.Generic;
    using System.Threading;

    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Message.Home;

    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Servers.Game.Session;
    
    public static class GameDuelMatchmakingManager
    {
        private static Dictionary<long, MatchmakingEntry> m_queue;
        private static Thread m_thread;
        
        public static void Init()
        {
            GameDuelMatchmakingManager.m_queue = new Dictionary<long, MatchmakingEntry>();
            GameDuelMatchmakingManager.m_thread = new Thread(GameDuelMatchmakingManager.Update);
            GameDuelMatchmakingManager.m_thread.Start();
        }
        
        private static void Update()
        {
            while (true)
            {
                Dictionary<long, MatchmakingEntry> sessions = new Dictionary<long, MatchmakingEntry>(GameDuelMatchmakingManager.m_queue);

                foreach (MatchmakingEntry entry in sessions.Values)
                {
                    if (entry.MatchConfirmation)
                        continue;
                    if (ServerStatus.Status == ServerStatusType.SHUTDOWN_STARTED ||
                        ServerStatus.Status == ServerStatusType.MAINTENANCE ||
                        ServerStatus.Status == ServerStatusType.COOLDOWN_AFTER_MAINTENANCE)
                    {
                        entry.Session.SendMessage(new GameMatchmakingResultMessage(), 9);
                        entry.MatchConfirmation = true;
                        continue;
                    }
                }

                Thread.Sleep(250);
            }
        }

        private static GameAvatar FindEnemy(MatchmakingEntry entry, Dictionary<long, GameAvatar> documents)
        {
            return null;
        }

        private static int CalculateStrength(LogicClientAvatar playerAvatar)
        {
            return playerAvatar.GetScore() + playerAvatar.GetTownHallLevel() * 75;
        }

        public static void Enqueue(GameSession session)
        {
            if (!session.InDuelMatchmaking)
            {
                GameDuelMatchmakingManager.m_queue.Add(session.Id, new MatchmakingEntry(session));
                session.InDuelMatchmaking = true;
            }
        }
        
        public static void Dequeue(GameSession session)
        {
            if (session.InDuelMatchmaking)
                session.InDuelMatchmaking = !GameDuelMatchmakingManager.m_queue.Remove(session.Id);
        }
        
        private class MatchmakingEntry
        {
            public readonly GameSession Session;
            public readonly int Timestamp;
            public bool MatchConfirmation;

            public MatchmakingEntry(GameSession session)
            {
                this.Timestamp = TimeUtil.GetTimestamp();
                this.Session = session;
            }
        }
    }
}