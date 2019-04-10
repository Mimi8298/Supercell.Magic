namespace Supercell.Magic.Servers.Core.Network.Message
{
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Core;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Titan.Message;

    public abstract class ServerMessageManager
    {
        public delegate void ReceiveServerCoreMessage(ServerCoreMessage message);

        public abstract void OnReceiveAccountMessage(ServerAccountMessage message);
        public abstract void OnReceiveRequestMessage(ServerRequestMessage message);
        public abstract void OnReceiveSessionMessage(ServerSessionMessage message);
        public abstract void OnReceiveCoreMessage(ServerCoreMessage message);

        internal static bool ReceiveCoreMessage(ServerCoreMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.PING:
                    ServerMessageManager.SendMessage(new PongMessage(), message.Sender);
                    return true;
                case ServerMessageType.SERVER_STATUS:
                    ServerStatusMessage serverStatusMessage = (ServerStatusMessage) message;
                    ServerStatus.SetStatus(serverStatusMessage.Type, serverStatusMessage.Time, serverStatusMessage.NextTime);
                    return true;
                default:
                    return false;
            }
        }

        public static void SendMessage(ServerMessage message, ServerSocket socket)
        {
            ServerMessaging.Send(message, socket);
        }

        public static void SendMessage(ServerMessage message, int serverType, int serverId)
        {
            ServerMessaging.Send(message, ServerManager.GetSocket(serverType, serverId));
        }

        public static void SendMessage(ServerAccountMessage message, int serverType)
        {
            ServerMessaging.Send(message, ServerManager.GetDocumentSocket(serverType, message.AccountId));
        }
    }
}