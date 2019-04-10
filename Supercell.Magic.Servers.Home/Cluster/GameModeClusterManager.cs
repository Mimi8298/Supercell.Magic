namespace Supercell.Magic.Servers.Home.Cluster
{
    using System;
    using System.Collections.Generic;

    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Network.Request;

    public static class GameModeClusterManager
    {
        private static Dictionary<long, GameModeCluster> m_sessionClusters;
        private static GameModeCluster[] m_clusters;

        private static long m_logicUpdateSpeed;
        private static int m_logicUpdateCount;

        public static int SessionCount
        {
            get { return GameModeClusterManager.m_sessionClusters.Count; }
        }

        public static int ClusterCount
        {
            get { return GameModeClusterManager.m_clusters.Length; }
        }

        public static void Init()
        {
            GameModeClusterManager.m_sessionClusters = new Dictionary<long, GameModeCluster>();
            GameModeClusterManager.m_clusters = new GameModeCluster[Environment.ProcessorCount * 2];

            for (int i = 0; i < Environment.ProcessorCount * 2; i++)
            {
                GameModeClusterManager.m_clusters[i] = new GameModeCluster(i);
            }
        }

        public static void OnStartServerSessionMessageReceived(StartServerSessionMessage message)
        {
            if (GameModeClusterManager.m_sessionClusters.ContainsKey(message.SessionId))
                throw new Exception("GameModeClusterManager.onStartSessionMessageReceived: session already started!");

            GameModeCluster gameModeCluster = GameModeClusterManager.GetFastestCluster();

            if (gameModeCluster != null)
            {
                GameModeClusterManager.m_sessionClusters.Add(message.SessionId, gameModeCluster);
                gameModeCluster.SendMessage(message);
            }
            else
            {
                if (message.BindRequestMessage != null)
                    ServerRequestManager.SendResponse(new BindServerSocketResponseMessage(), message.BindRequestMessage);
                ServerMessageManager.SendMessage(new StartServerSessionFailedMessage
                {
                    SessionId = message.SessionId
                }, message.Sender);
            }
        }

        public static void OnStopServerSessionMessageReceived(StopServerSessionMessage message)
        {
            if (GameModeClusterManager.m_sessionClusters.Remove(message.SessionId, out GameModeCluster cluster))
                cluster.SendMessage(message);
        }

        public static void ReceiveMessage(ServerSessionMessage message)
        {
            if (GameModeClusterManager.m_sessionClusters.TryGetValue(message.SessionId, out GameModeCluster cluster))
                cluster.SendMessage(message);
        }

        private static GameModeCluster GetFastestCluster()
        {
            GameModeCluster fastestCluster = null;

            for (int i = 0, fastestClusterTime = 0x7FFFFFFF; i < GameModeClusterManager.m_clusters.Length; i++)
            {
                GameModeCluster instance = GameModeClusterManager.m_clusters[i];
                long executeTime = instance.GetAverageMessageProcessSpeed();

                if (executeTime <= 2000L && (fastestCluster == null || executeTime < fastestClusterTime))
                {
                    fastestCluster = instance;
                    fastestClusterTime = (int) executeTime;
                }
            }

            return fastestCluster;
        }

        public static void ReportLogicUpdateSpeed(long ms)
        {
            GameModeClusterManager.m_logicUpdateSpeed += ms;
            GameModeClusterManager.m_logicUpdateCount += 1;

            if (GameModeClusterManager.m_logicUpdateCount >= 1000)
            {
                GameModeClusterManager.m_logicUpdateSpeed /= GameModeClusterManager.m_logicUpdateCount;
                GameModeClusterManager.m_logicUpdateCount = 1;
            }
        }

        public static long GetAverageLogicUpdateSpeed()
        {
            if(GameModeClusterManager.m_logicUpdateCount != 0)
                return GameModeClusterManager.m_logicUpdateSpeed / GameModeClusterManager.m_logicUpdateCount;
            return 0L;
        }

        public static void StartPing()
        {
            for (int i = 0; i < GameModeClusterManager.m_clusters.Length; i++)
                GameModeClusterManager.m_clusters[i].SendPing();
        }
    }
}