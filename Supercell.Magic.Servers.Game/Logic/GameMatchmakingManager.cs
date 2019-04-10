namespace Supercell.Magic.Servers.Game.Logic
{
    using System.Collections.Generic;
    using System.Threading;

    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Message.Home;

    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Servers.Game.Logic.Live;
    using Supercell.Magic.Servers.Game.Session;

    using Supercell.Magic.Titan.Math;

    public static class GameMatchmakingManager
    {
        private static Dictionary<long, MatchmakingEntry> m_queue;
        private static Dictionary<long, GameAvatar> m_offlinePlayers;
        private static Thread m_thread;
        
        public static void Init()
        {
            GameMatchmakingManager.m_queue = new Dictionary<long, MatchmakingEntry>();
            GameMatchmakingManager.m_offlinePlayers = new Dictionary<long, GameAvatar>();
            GameMatchmakingManager.m_thread = new Thread(GameMatchmakingManager.Update);
            GameMatchmakingManager.m_thread.Start();
        }
        
        private static void Update()
        {
            while (true)
            {
                Dictionary<long, MatchmakingEntry> sessions = new Dictionary<long, MatchmakingEntry>(GameMatchmakingManager.m_queue);
                Dictionary<long, GameAvatar> offlineUsers = new Dictionary<long, GameAvatar>(GameMatchmakingManager.m_offlinePlayers);

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

                    GameAvatar enemy = GameMatchmakingManager.FindEnemy(entry, offlineUsers);

                    if (enemy != null)
                    {
                        entry.Session.SendMessage(new GameMatchmakingResultMessage
                        {
                            EnemyId = enemy.Id
                        }, 9);
                        entry.MatchConfirmation = true;
                    }
                }

                Thread.Sleep(250);
            }
        }

        private static GameAvatar FindEnemy(MatchmakingEntry entry, Dictionary<long, GameAvatar> documents)
        {
            GameAvatar document = entry.Session.GameAvatar;
            LogicClientAvatar playerAvatar1 = document.LogicClientAvatar;

            int totalSecs = TimeUtil.GetTimestamp() - entry.Timestamp;
            int strength1 = GameMatchmakingManager.CalculateStrength(playerAvatar1);
            int maxStrengthDiff = 50 + LogicMath.Clamp(totalSecs * 150 / 60, 0, 150);

            foreach (GameAvatar enemy in documents.Values)
            {
                LogicClientAvatar playerAvatar2 = enemy.LogicClientAvatar;

#if DEBUG
                if (ServerCore.Random.Rand(100) <= 95)
                    continue;
#else
                if (playerAvatar1.IsInAlliance() && playerAvatar2.IsInAlliance() && playerAvatar1.GetAllianceId().Equals(playerAvatar2.GetAllianceId()))
                    continue;

                int strength2 = GameMatchmakingManager.CalculateStrength(playerAvatar2);

                if (LogicMath.Abs(strength1 - strength2) >= maxStrengthDiff)
                    continue;
                if (document.HasRecentlyMatchedWithEnemy(enemy.Id))
                    continue;
#endif
                return enemy;
            }

            return null;
        }

        private static int CalculateStrength(LogicClientAvatar playerAvatar)
        {
            return playerAvatar.GetScore() + playerAvatar.GetTownHallLevel() * 75;
        }

        public static void Enqueue(GameSession session)
        {
            if (!session.InMatchmaking)
            {
                GameMatchmakingManager.m_queue.Add(session.Id, new MatchmakingEntry(session));
                session.InMatchmaking = true;
            }
        }

        public static void Enqueue(GameAvatar document)
        {
            GameMatchmakingManager.m_offlinePlayers.Add(document.Id, document);
        }

        public static void Dequeue(GameSession session)
        {
            if (session.InMatchmaking)
                session.InMatchmaking = !GameMatchmakingManager.m_queue.Remove(session.Id);
        }

        public static void Dequeue(GameAvatar document)
        {
            GameMatchmakingManager.m_offlinePlayers.Remove(document.Id);
        }

        public static void OnGameMatchmakingResultMessageReceived(GameMatchmakingResultMessage message)
        {
            if (GameMatchmakingManager.m_queue.TryGetValue(message.SessionId, out MatchmakingEntry entry))
            {
                GameSession session = entry.Session;

                if (message.EnemyId != null)
                {
                    GameAvatar enemy = GameAvatarManager.Get(message.EnemyId);
                    GameMatchmakingManager.Dequeue(session);

                    entry.Session.GameAvatar.AddRecentlyMatchedEnemy(message.EnemyId);
                    session.LoadGameState(new GameMatchedAttackState
                    {
                        Home = enemy.LogicClientHome,
                        HomeOwnerAvatar = enemy.LogicClientAvatar,
                        PlayerAvatar = session.GameAvatar.LogicClientAvatar,
                        MaintenanceTime = enemy.MaintenanceTime,
                        SaveTime = enemy.SaveTime,

                        LiveReplayId = LiveReplayManager.Create(session, null, null)
                    });
                }
                else
                {
                    AttackHomeFailedMessage attackHomeFailedMessage = new AttackHomeFailedMessage();

                    switch (ServerStatus.Status)
                    {
                        case ServerStatusType.SHUTDOWN_STARTED:
                        case ServerStatusType.MAINTENANCE:
                            attackHomeFailedMessage.SetReason(AttackHomeFailedMessage.Reason.SHUTDOWN_ATTACK_DISABLED);
                            break;
                        case ServerStatusType.COOLDOWN_AFTER_MAINTENANCE:
                            attackHomeFailedMessage.SetReason(AttackHomeFailedMessage.Reason.COOLDOWN_AFTER_MAINTENANCE);
                            break;
                        default:
                            entry.MatchConfirmation = false;
                            return;
                    }

                    session.SendPiranhaMessage(attackHomeFailedMessage, 1);
                    GameMatchmakingManager.Dequeue(session);
                }
            }
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