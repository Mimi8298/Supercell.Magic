namespace Supercell.Magic.Servers.Core.Network
{
    using System;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Titan.DataStream;

    internal static class ServerMessaging
    {
        private static ServerMessageHandler m_handler;

        public static void Init(ServerMessageManager messageManager)
        {
            ServerMessaging.m_handler = new ServerMessageHandler(messageManager);
        }

        public static void DeInit()
        {
            if (ServerMessaging.m_handler != null)
            {
                ServerMessaging.m_handler.Destruct();
                ServerMessaging.m_handler = null;
            }
        }

        public static void OnReceive(byte[] buffer, int length)
        {
            ServerMessage message = ServerMessaging.ReadMessage(buffer, length);

            if (message != null)
            {
                ServerMessaging.m_handler.Enqueue(message);
            }
        }

        public static ServerMessage ReadMessage(byte[] buffer, int length)
        {
            ByteStream byteStream = new ByteStream(buffer, length);
            ServerMessage message = ServerMessageFactory.CreateMessageByType((ServerMessageType)byteStream.ReadShort());

            if (message != null)
            {
                try
                {
                    message.DecodeHeader(byteStream);
                    message.Decode(byteStream);

                    return message;
                }
                catch (Exception exception)
                {
                    Logging.Error(string.Format("ServerMessaging::onReceive exception when the decoding of message type {0}, trace: {1}", message.GetMessageType(),
                        exception));
                }
            }

            return null;
        }

        public static byte[] WriteMessage(ServerMessage message)
        {
            ByteStream stream = new ByteStream(64);

            stream.WriteShort((short) message.GetMessageType());

            message.EncodeHeader(stream);
            message.Encode(stream);

            return stream.GetByteArray();
        }

        public static void Send(ServerMessage message, ServerSocket socket)
        {
            if (socket == null)
            {
                Logging.Warning("ServerMessaging::send - socket is NULL");
                return;
            }

            message.SenderType = ServerCore.Type;
            message.SenderId = ServerCore.Id;
            
            ServerMessaging.m_handler.Enqueue(message, socket);
        }
    }
}