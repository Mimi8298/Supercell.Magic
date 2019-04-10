namespace Supercell.Magic.Servers.Admin
{
    using System;
    using Supercell.Magic.Servers.Admin.Logic;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Database;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Core;
    using Supercell.Magic.Servers.Core.Settings;

    public static class ServerAdmin
    {
        public static CouchbaseDatabase AccountDatabase { get; private set; }
        public static CouchbaseDatabase GameDatabase { get; private set; }
        public static RedisDatabase SessionDatabase { get; private set; }

        public static void Init()
        {
            ServerAdmin.AccountDatabase = new CouchbaseDatabase("magic-players", "account");
            ServerAdmin.AccountDatabase.CreateIndexWithFilter("accountIndex", "meta().id LIKE '%KEY_PREFIX%%'", "meta().id", "createTime", "lastSessionTime");
            ServerAdmin.GameDatabase = new CouchbaseDatabase("magic-players", "game");
            ServerAdmin.GameDatabase.CreateIndexWithFilter("gameIndex", "meta().id LIKE '%KEY_PREFIX%%'", "meta().id", "name", "xp_level", "score", "alliance_name");
            ServerAdmin.SessionDatabase = new RedisDatabase("magic-session");

            AuthManager.Init();
            DashboardManager.Init();
            ServerManager.Init();
            UserManager.Init();
            LogManager.Init();

            ServerStatus.OnServerStatusChanged = ServerAdmin.OnServerStatusChanged;
        }

        private static void OnServerStatusChanged(ServerStatusType type, int time, int nextTime)
        {
            for (int i = 1; i < EnvironmentSettings.SERVER_TYPE_COUNT; i++)
            {
                for (int j = 0, count = Core.Network.ServerManager.GetServerCount(i); j < count; j++)
                {
                    ServerMessageManager.SendMessage(new ServerStatusMessage
                    {
                        Type = type,
                        Time = time,
                        NextTime = nextTime
                    }, i, j);
                }
            }
        }
    }
}