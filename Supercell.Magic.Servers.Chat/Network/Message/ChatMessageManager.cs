namespace Supercell.Magic.Servers.Chat.Network.Message
{
    using System;
    using Supercell.Magic.Logic.Message;
    using Supercell.Magic.Servers.Chat.Session;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Core;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Titan.Message;

    public class ChatMessageManager : ServerMessageManager
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
                    ChatSessionManager.OnStartServerSessionMessageReceived((StartServerSessionMessage) message);
                    break;
                case ServerMessageType.STOP_SERVER_SESSION:
                    ChatSessionManager.OnStopServerSessionMessageReceived((StopServerSessionMessage) message);
                    break;
                case ServerMessageType.UPDATE_SOCKET_SERVER_SESSION:
                    ChatMessageManager.OnUpdateSocketServerSessionMessageReceived((UpdateSocketServerSessionMessage) message);
                    break;
                case ServerMessageType.FORWARD_LOGIC_MESSAGE:
                    ChatMessageManager.OnForwardLogicMessageReceived((ForwardLogicMessage) message);
                    break;
            }
        }

        private static void OnUpdateSocketServerSessionMessageReceived(UpdateSocketServerSessionMessage message)
        {
            if (ChatSessionManager.TryGet(message.SessionId, out ChatSession session))
            {
                session.UpdateSocketServerSessionMessageReceived(message);
            }
        }

        private static void OnForwardLogicMessageReceived(ForwardLogicMessage message)
        {
            if (ChatSessionManager.TryGet(message.SessionId, out ChatSession session))
            {
                PiranhaMessage logicMessage = LogicMagicMessageFactory.Instance.CreateMessageByType(message.MessageType);

                if (logicMessage == null)
                    throw new Exception("logicMessage should not be NULL!");

                logicMessage.GetByteStream().SetByteArray(message.MessageBytes, message.MessageLength);
                logicMessage.SetMessageVersion(message.MessageVersion);
                logicMessage.Decode();

                if (!logicMessage.IsServerToClientMessage())
                {
                    session.LogicMessageManager.ReceiveMessage(logicMessage);
                }
            }
        }

        public override void OnReceiveCoreMessage(ServerCoreMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.SERVER_PERFORMANCE:
                    ServerMessageManager.SendMessage(new ServerPerformanceDataMessage
                    {
                        SessionCount = ChatSessionManager.Count
                    }, message.Sender);
                    break;
            }
        }
    }
}