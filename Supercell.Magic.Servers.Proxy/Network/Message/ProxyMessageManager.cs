namespace Supercell.Magic.Servers.Proxy.Network.Message
{
    using Supercell.Magic.Logic.Message;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Core;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Network.Request;

    using Supercell.Magic.Servers.Proxy.Session;

    using Supercell.Magic.Titan.Message;

    public class ProxyMessageManager : ServerMessageManager
    {
        public override void OnReceiveAccountMessage(ServerAccountMessage message)
        {
        }

        public override void OnReceiveRequestMessage(ServerRequestMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.BIND_SERVER_SOCKET_REQUEST:
                    ProxyMessageManager.OnBindServerSocketRequestMessageReceived((BindServerSocketRequestMessage) message);
                    break;
            }
        }

        private static void OnBindServerSocketRequestMessageReceived(BindServerSocketRequestMessage message)
        {
            if (ProxySessionManager.TryGet(message.SessionId, out ProxySession session))
            {
                ServerSocket socket = message.ServerId != -1 
                    ? ServerManager.GetSocket(message.ServerType, message.ServerId)
                    : ServerManager.GetNextSocket(message.ServerType);

                if (socket != null)
                {
                    session.SetSocket(socket, message);
                }
                else
                {
                    ServerRequestManager.SendResponse(new BindServerSocketResponseMessage
                    {
                        ServerType = message.ServerType,
                        ServerId = message.ServerId,

                        Success = false
                    }, message);
                }
            }
            else
            {
                ServerRequestManager.SendResponse(new BindServerSocketResponseMessage
                {
                    ServerType = message.ServerType,
                    ServerId = message.ServerId,

                    Success = false
                }, message);
            }
        }
        
        public override void OnReceiveSessionMessage(ServerSessionMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.START_SERVER_SESSION_FAILED:
                    ProxyMessageManager.OnStartServerSessionFailedMessageReceived((StartServerSessionFailedMessage) message);
                    break;
                case ServerMessageType.STOP_SESSION:
                    ProxyMessageManager.OnStopSessionMessageReceived((StopSessionMessage) message);
                    break;
                case ServerMessageType.STOP_SERVER_SESSION:
                    ProxyMessageManager.OnStopServerSessionMessageReceived((StopServerSessionMessage) message);
                    break;
                case ServerMessageType.STOP_SPECIFIED_SERVER_SESSION:
                    ProxyMessageManager.OnStopSpecifiedServerSessionMessageReceived((StopSpecifiedServerSessionMessage) message);
                    break;

                case ServerMessageType.FORWARD_LOGIC_MESSAGE:
                    ProxyMessageManager.OnForwardLogicMessageReceived((ForwardLogicMessage) message);
                    break;
            }
        }

        private static void OnStartServerSessionFailedMessageReceived(StartServerSessionFailedMessage message)
        {
            if (ProxySessionManager.TryGet(message.SessionId, out ProxySession session))
            {
                if (session.IsBoundToServerType(message.SenderType) && session.GetServer(message.SenderType).ServerId == message.SenderId)
                {
                    session.UnbindServer(message.SenderType);
                }
            }
        }

        private static void OnStopSessionMessageReceived(StopSessionMessage message)
        {
            if (ProxySessionManager.TryGet(message.SessionId, out ProxySession session))
            {
                ClientConnectionManager.Disconnect(session.ClientConnection, message.Reason);
            }
        }

        private static void OnStopServerSessionMessageReceived(StopServerSessionMessage message)
        {
            if (ProxySessionManager.TryGet(message.SessionId, out ProxySession session))
            {
                if (session.GetServer(message.SenderType)?.ServerId == message.SenderId)
                    session.UnbindServer(message.SenderType);
            }
        }

        private static void OnStopSpecifiedServerSessionMessageReceived(StopSpecifiedServerSessionMessage message)
        {
            if (ProxySessionManager.TryGet(message.SessionId, out ProxySession session))
            {
                if (session.GetServer(message.ServerType)?.ServerId == message.ServerId)
                    session.UnbindServer(message.ServerType);
            }
        }

        private static void OnForwardLogicMessageReceived(ForwardLogicMessage message)
        {
            if (ProxySessionManager.TryGet(message.SessionId, out ProxySession session))
            {
                PiranhaMessage piranhaMessage = LogicMagicMessageFactory.Instance.CreateMessageByType(message.MessageType);

                if (piranhaMessage == null)
                {
                    Logging.Error("ProxyMessageManager.onForwardLogicMessageReceived: unknown logic message type: " + message.MessageType);
                    return;
                }

                piranhaMessage.SetMessageVersion(message.MessageVersion);
                piranhaMessage.GetByteStream().SetByteArray(message.MessageBytes, message.MessageLength);
                piranhaMessage.GetByteStream().SetOffset(message.MessageLength);

                session.ClientConnection.Messaging.Send(piranhaMessage);
            }
        }

        public override void OnReceiveCoreMessage(ServerCoreMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.SERVER_PERFORMANCE:
                    ServerMessageManager.SendMessage(new ServerPerformanceDataMessage
                    {
                        SessionCount = ProxySessionManager.Count
                    }, message.Sender);
                    break;
            }
        }
    }
}