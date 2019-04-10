namespace Supercell.Magic.Servers.Proxy
{
    using MaxMind.Db;
    using MaxMind.GeoIP2;

    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Database;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Settings;
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Servers.Proxy.Network;
    using Supercell.Magic.Servers.Proxy.Network.Message;
    using Supercell.Magic.Servers.Proxy.Session;
    using Supercell.Magic.Titan.Math;

    public static class ServerProxy
    {
        public static DatabaseReader MaxMind { get; private set; }
        public static TcpServerSocket ServerSocket { get; private set; }
        public static CouchbaseDatabase AccountDatabase { get; private set; }
        public static RedisDatabase SessionDatabase { get; private set; }

        public static void Init()
        {
            ServerProxy.MaxMind = new DatabaseReader("Assets/GeoIP-Country.mmdb", FileAccessMode.Memory);
            ServerProxy.ServerSocket = new TcpServerSocket(EnvironmentSettings.Servers[ServerCore.Type][ServerCore.Id].ServerIP, 9339);
            ServerProxy.AccountDatabase = new CouchbaseDatabase("magic-players", "account");
            ServerProxy.SessionDatabase = new RedisDatabase("magic-session");

            ServerStatus.OnServerStatusChanged = ServerProxy.OnServerStatusChanged;

            ClientConnectionManager.Init();
            ProxySessionManager.Init();
            MessageHandler.Init();

            ServerProxy.ServerSocket.StartAccept();
        }

        private static void OnServerStatusChanged(ServerStatusType type, int time, int args)
        {
            switch (type)
            {
                case ServerStatusType.SHUTDOWN_STARTED:
                    ClientConnectionManager.SendShutdownStartedMessageToConnections(LogicMath.Max(time - TimeUtil.GetTimestamp(), 0));
                    break;
                case ServerStatusType.MAINTENANCE:
                    ClientConnectionManager.DisconnectConnections();
                    break;
            }
        }

        public static int GetStartupCooldownSeconds()
        {
            if (ServerStatus.Status == ServerStatusType.COOLDOWN_AFTER_MAINTENANCE)
                return LogicMath.Max(ServerStatus.Time - TimeUtil.GetTimestamp(), 0);
            return 0;
        }
    }
}