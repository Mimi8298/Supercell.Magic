namespace Supercell.Magic.Servers.Game.Logic.Live
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Game.Session;

    using Supercell.Magic.Titan.Math;

    public static class LiveReplayManager
    {
        public const int LIVE_REPLAY_UPDATE_INTERVAL_MS = 2000;

        private static LiveReplayCluster m_cluster;
        private static ConcurrentDictionary<long, LiveReplay> m_liveReplays;
        private static Thread m_thread;
        private static int[] m_counters;

        public static void Init()
        {
            LiveReplayManager.m_cluster = new LiveReplayCluster(0);
            LiveReplayManager.m_liveReplays = new ConcurrentDictionary<long, LiveReplay>();
            LiveReplayManager.m_counters = new int[256];

            LiveReplayManager.m_thread = new Thread(LiveReplayManager.Update);
            LiveReplayManager.m_thread.Start();
        }

        private static void Update()
        {
            while (true)
            {
                foreach (long id in LiveReplayManager.m_liveReplays.Keys)
                {
                    LiveReplayManager.ReceiveMessage(new ServerUpdateLiveReplayMessage
                    {
                        AccountId = id,
                        Milliseconds = LiveReplayManager.LIVE_REPLAY_UPDATE_INTERVAL_MS
                    });
                }

                Thread.Sleep(LiveReplayManager.LIVE_REPLAY_UPDATE_INTERVAL_MS);
            }
        }

        private static LogicLong GetNextLiveReplayId()
        {
            int highId = -1;
            int lowId = -1;

            for (int i = 0; i < LiveReplayManager.m_counters.Length; i++)
            {
                if (lowId == -1 || LiveReplayManager.m_counters[i] < lowId)
                {
                    lowId = LiveReplayManager.m_counters[i];
                    highId = i;
                }
            }

            if (lowId == 0)
                return new LogicLong(highId, LiveReplayManager.m_counters[highId] = ServerCore.Id + 1);
            return new LogicLong(highId, LiveReplayManager.m_counters[highId] += ServerManager.GetServerCount(9));
        }

        public static LogicLong Create(GameSession session, LogicLong allianceId, LogicLong allianceStreamId)
        {
            LogicLong id = LiveReplayManager.GetNextLiveReplayId();
            Logging.Assert(LiveReplayManager.m_liveReplays.TryAdd(id, new LiveReplay(id, allianceId, allianceStreamId, session)),
                           "LiveReplayManager.m_liveReplays.TryAdd(id, allianceId, allianceStreamId, new LiveReplay(session)) return false");
            return id;
        }

        public static void Remove(LogicLong id)
        {
            LiveReplayManager.m_liveReplays.TryRemove(id, out _);
        }

        public static bool TryGet(LogicLong id, out LiveReplay liveReplay)
        {
            return LiveReplayManager.m_liveReplays.TryGetValue(id, out liveReplay);
        }

        public static void ReceiveMessage(ServerMessage message)
        {
            LiveReplayManager.m_cluster.SendMessage(message);
        }
    }
}