namespace Supercell.Magic.Servers.Admin.Network.Message
{
    using Supercell.Magic.Servers.Admin.Logic;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Core;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Session;

    public class AdminMessageManager : ServerMessageManager
    {
        public override void OnReceiveAccountMessage(ServerAccountMessage message)
        {
            throw new System.NotImplementedException();
        }

        public override void OnReceiveRequestMessage(ServerRequestMessage message)
        {
            throw new System.NotImplementedException();
        }

        public override void OnReceiveSessionMessage(ServerSessionMessage message)
        {
            throw new System.NotImplementedException();
        }

        public override void OnReceiveCoreMessage(ServerCoreMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.PONG:
                    ServerManager.OnPongMessageReceived((PongMessage) message);
                    break;
                case ServerMessageType.SERVER_PERFORMANCE:
                    ServerMessageManager.SendMessage(new ServerPerformanceDataMessage(), message.SenderType, message.SenderId);
                    break;
                case ServerMessageType.SERVER_PERFORMANCE_DATA:
                    ServerManager.OnServerPerformanceDataMessageReceived((ServerPerformanceDataMessage) message);
                    break;
                case ServerMessageType.CLUSTER_PERFORMANCE_DATA:
                    ServerManager.OnClusterPerformanceDataMessageReceived((ClusterPerformanceDataMessage) message);
                    break;
                case ServerMessageType.SERVER_LOG:
                    LogManager.OnServerLogMessage((ServerLogMessage) message);
                    break;
                case ServerMessageType.GAME_LOG:
                    LogManager.OnGameLogMessage((GameLogMessage) message);
                    break;
                case ServerMessageType.ASK_FOR_SERVER_STATUS:
                    ServerMessageManager.SendMessage(new ServerStatusMessage
                    {
                        Type = ServerStatus.Status,
                        Time = ServerStatus.Time,
                        NextTime = ServerStatus.NextTime
                    }, message.Sender);

                    break;
            }
        }
    }
}