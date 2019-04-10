namespace Supercell.Magic.Servers.Core
{
    using System;
    using System.IO;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Core;
    using Supercell.Magic.Servers.Core.Network.Request;
    using Supercell.Magic.Servers.Core.Settings;
    using Supercell.Magic.Titan.Math;

    public static class ServerCore
    {
        public static int Type { get; private set; }
        public static int Id { get; private set; }
        public static string ConfigurationServer { get; private set; }

        public static LogicRandom Random { get; private set; }
        public static ServerSocket Socket { get; private set; }
        
        public static void Init(int type, string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += ServerCore.OnUnhandledExceptionThrown;
            ServerCore.Type = type;
            ServerCore.Id = 0;
            ServerCore.ConfigurationServer = "http://127.0.0.1/supercell/";

            if (args.Length > 0)
            {
                if (args.Length % 2 == 0)
                {
                    for (int i = 0; i < args.Length; i += 2)
                    {
                        string name = args[i];
                        string value = args[i + 1];

                        switch (name)
                        {
                            case "-conf":
                                if (value.Length > 0)
                                {
                                    if (value.StartsWith("http://") || value.StartsWith("https://"))
                                    {
                                        if (!value.EndsWith("/"))
                                        {
                                            value += "/";
                                        }

                                        ServerCore.ConfigurationServer = value;
                                    }
                                    else
                                    {
                                        Logging.Warning("ServerCore.init: invalid server url: " + value);
                                    }
                                }
                                else
                                {
                                    Logging.Warning("ServerCore.init server url is empty");
                                }

                                break;
                            case "-id":
                                ServerCore.Id = int.Parse(value);
                                break;
                        }
                    }
                }
                else
                {
                    Logging.Warning("ServerCore.init invalid args length");
                }
            }

            ServerCore.Random = new LogicRandom((int) DateTime.UtcNow.Ticks);

            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            Logging.Init();
            EnvironmentSettings.Init();
            ResourceSettings.Init();
            GamePlaySettings.Init();
            ResourceManager.Init();
            ServerRequestManager.Init();
            ServerManager.Init();

            ServerCore.Socket = ServerManager.GetSocket(ServerCore.Type, ServerCore.Id);
        }

        private static void OnUnhandledExceptionThrown(object sender, UnhandledExceptionEventArgs args)
        {
            if (args.ExceptionObject is Exception exception)
            {
                Logging.Error("ServerCore.onUnhandledExceptionThrown: " + exception);
            }
        }

        public static void Start(ServerMessageManager messageManager)
        {
            Logging.SEND_LOGS = true;
            ServerMessaging.Init(messageManager);
            ServerListenSocket.Init();

            if (ServerManager.GetServerCount(0) != 0 && ServerCore.Type != 0)
            {
                ServerStatus.SetStatus(ServerStatusType.MAINTENANCE, 0, 0);
                ServerMessageManager.SendMessage(new AskForServerStatusMessage(), ServerManager.GetSocket(0, 0));
            }
        }
    }

    public static class ServerStatus
    {
        public delegate void ServerStatusChanged(ServerStatusType type, int time, int nextTime);
        
        public static ServerStatusChanged OnServerStatusChanged { get; set; }

        public static ServerStatusType Status { get; private set; }
        public static int Time { get; private set; }
        public static int NextTime { get; private set; }
        
        public static void SetStatus(ServerStatusType type, int time, int nextTime)
        {
            ServerStatus.Status = type;
            ServerStatus.Time = time;
            ServerStatus.NextTime = nextTime;

            if (ServerStatus.OnServerStatusChanged != null)
                ServerStatus.OnServerStatusChanged(type, time, nextTime);
        }
    }

    public enum ServerStatusType
    {
        NORMAL,
        SHUTDOWN_STARTED,
        MAINTENANCE,
        COOLDOWN_AFTER_MAINTENANCE
    }
}