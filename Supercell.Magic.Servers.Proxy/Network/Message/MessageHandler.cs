namespace Supercell.Magic.Servers.Proxy.Network.Message
{
    using System.Collections.Concurrent;
    using System.Threading;
    using Supercell.Magic.Titan.Message;

    internal static class MessageHandler
    {
        private static Thread m_receiveThread;
        private static Thread m_sendThread;
        private static ConcurrentQueue<HandleItem> m_receiveQueue;
        private static ConcurrentQueue<HandleItem> m_sendQueue;
        private static AutoResetEvent m_receiveEvent;
        private static AutoResetEvent m_sendEvent;

        private static bool m_started;

        internal static void Init()
        {
            MessageHandler.m_started = true;

            MessageHandler.m_receiveEvent = new AutoResetEvent(false);
            MessageHandler.m_sendEvent = new AutoResetEvent(false);
            MessageHandler.m_receiveQueue = new ConcurrentQueue<HandleItem>();
            MessageHandler.m_sendQueue = new ConcurrentQueue<HandleItem>();

            MessageHandler.m_receiveThread = new Thread(MessageHandler.Receive);
            MessageHandler.m_receiveThread.Name = "Message Receive Handler";
            MessageHandler.m_receiveThread.Start();

            MessageHandler.m_sendThread = new Thread(MessageHandler.Send);
            MessageHandler.m_sendThread.Name = "Message Send Handler";
            MessageHandler.m_sendThread.Start();
        }

        private static void Receive()
        {
            while (MessageHandler.m_started)
            {
                MessageHandler.m_receiveEvent.WaitOne();

                while (MessageHandler.m_receiveQueue.TryDequeue(out HandleItem item))
                {
                    item.ClientConnection.MessageManager.ReceiveMessage(item.PiranhaMessage);
                }
            }
        }

        private static void Send()
        {
            while (MessageHandler.m_started)
            {
                MessageHandler.m_sendEvent.WaitOne();

                while (MessageHandler.m_sendQueue.TryDequeue(out HandleItem item))
                {
                    item.ClientConnection.Messaging.InternalSend(item.PiranhaMessage);
                }
            }
        }

        internal static void EnqueueReceive(PiranhaMessage message, ClientConnection clientConnection)
        {
            MessageHandler.m_receiveQueue.Enqueue(new HandleItem(message, clientConnection));
            MessageHandler.m_receiveEvent.Set();
        }

        internal static void EnqueueSend(PiranhaMessage message, ClientConnection clientConnection)
        {
            MessageHandler.m_sendQueue.Enqueue(new HandleItem(message, clientConnection));
            MessageHandler.m_sendEvent.Set();
        }

        private struct HandleItem
        {
            internal readonly PiranhaMessage PiranhaMessage;
            internal readonly ClientConnection ClientConnection;

            internal HandleItem(PiranhaMessage piranhaMessage, ClientConnection clientConnection)
            {
                this.PiranhaMessage = piranhaMessage;
                this.ClientConnection = clientConnection;
            }
        }
    }
}