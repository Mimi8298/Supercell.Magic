namespace Supercell.Magic.Servers.Core.Network
{
    using Supercell.Magic.Servers.Core.Settings;
    using Supercell.Magic.Titan.Math;

    public static class ServerManager
    {
        private static bool m_initialized;
        private static int[] m_scrambler;
        private static ServerSocket[][] m_sockets;

        public static void Init()
        {
            ServerManager.m_initialized = true;
            ServerManager.m_scrambler = new int[EnvironmentSettings.SERVER_TYPE_COUNT];
            ServerManager.m_sockets = new ServerSocket[EnvironmentSettings.SERVER_TYPE_COUNT][];

            for (int i = 0; i < EnvironmentSettings.SERVER_TYPE_COUNT; i++)
            {
                EnvironmentSettings.ServerConnectionEntry[] connectionEntries = EnvironmentSettings.Servers[i];
                ServerManager.m_sockets[i] = new ServerSocket[connectionEntries.Length];

                for (int j = 0; j < connectionEntries.Length; j++)
                {
                    ServerManager.m_sockets[i][j] = new ServerSocket(i, j, connectionEntries[j].ServerIP, connectionEntries[j].ServerPort);
                }
            }
        }

        public static bool IsInit()
        {
            return ServerManager.m_initialized;
        }

        public static void DeInit()
        {
            if (ServerManager.m_sockets != null)
            {
                for (int i = 0; i < EnvironmentSettings.SERVER_TYPE_COUNT; i++)
                {
                    ServerSocket[] sockets = ServerManager.m_sockets[i];

                    for (int j = 0; j < sockets.Length; j++)
                    {
                        sockets[j].Destruct();
                    }
                }

                ServerManager.m_sockets = null;
            }

            ServerManager.m_scrambler = null;
        }

        public static ServerSocket GetSocket(int type, int id)
        {
            return ServerManager.m_sockets[type][id];
        }

        public static ServerSocket GetNextSocket(int type)
        {
            ServerSocket[] sockets = ServerManager.m_sockets[type];

            if (sockets.Length > 0)
            {
                int id = ServerManager.m_scrambler[type];
                ServerManager.m_scrambler[type] = (ServerManager.m_scrambler[type] + 1) & (ServerManager.m_sockets[type].Length - 1);
                return ServerManager.m_sockets[type][id];
            }

            return null;
        }

        public static ServerSocket GetDocumentSocket(int type, LogicLong id)
        {
            if (ServerManager.m_sockets[type].Length > 0 && id.GetLowerInt() > 0)
                return ServerManager.m_sockets[type][(id.GetLowerInt() - 1) % ServerManager.m_sockets[type].Length];
            return null;
        }

        public static ServerSocket GetProxySocket(long sessionId)
        {
            return ServerManager.m_sockets[1][(int) (sessionId >> 55)];
        }

        public static int GetServerCount(int type)
        {
            return ServerManager.m_sockets[type].Length;
        }
    }
}