namespace Supercell.Magic.Servers.Home.Network.Message
{
    using System;

    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Core;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Session;

    using Supercell.Magic.Servers.Home.Cluster;

    public class HomeMessageManager : ServerMessageManager
    {
        public override void OnReceiveAccountMessage(ServerAccountMessage message)
        {
            throw new NotSupportedException();
        }

        public override void OnReceiveRequestMessage(ServerRequestMessage message)
        {
            throw new NotSupportedException();
        }

        public override void OnReceiveSessionMessage(ServerSessionMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.START_SERVER_SESSION:
                    GameModeClusterManager.OnStartServerSessionMessageReceived((StartServerSessionMessage) message);
                    break;
                case ServerMessageType.STOP_SERVER_SESSION:
                    GameModeClusterManager.OnStopServerSessionMessageReceived((StopServerSessionMessage) message);
                    break;
                default:
                    GameModeClusterManager.ReceiveMessage(message);
                    break;
            }
        }

        public override void OnReceiveCoreMessage(ServerCoreMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.SERVER_PERFORMANCE:
                    ServerMessageManager.SendMessage(new ServerPerformanceDataMessage
                    {
                        SessionCount = GameModeClusterManager.SessionCount,
                        ClusterCount = GameModeClusterManager.ClusterCount
                    }, message.Sender);
                    GameModeClusterManager.StartPing();
                    break;
            }
        }
    }
}