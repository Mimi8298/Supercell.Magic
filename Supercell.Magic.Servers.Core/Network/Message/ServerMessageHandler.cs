namespace Supercell.Magic.Servers.Core.Network.Message
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Core;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Network.Request;
    using Supercell.Magic.Titan.DataStream;

    internal class ServerMessageHandler
    {
        private readonly Thread m_sendThread;
        private readonly Thread m_receiveThread;
        private readonly ConcurrentQueue<QueueItem> m_sendQueue;
        private readonly ConcurrentQueue<ServerMessage> m_receiveQueue;
        private readonly AutoResetEvent m_sendResetEvent;
        private readonly AutoResetEvent m_receiveResetEvent;
        private readonly ServerMessageManager m_messageManager;

        private bool m_started;

        public ServerMessageHandler(ServerMessageManager manager)
        {
            this.m_started = true;

            this.m_sendQueue = new ConcurrentQueue<QueueItem>();
            this.m_receiveQueue = new ConcurrentQueue<ServerMessage>();
            this.m_sendResetEvent = new AutoResetEvent(false);
            this.m_receiveResetEvent = new AutoResetEvent(false);
            this.m_messageManager = manager;
            this.m_sendThread = new Thread(this.Send);
            this.m_sendThread.Start();
            this.m_receiveThread = new Thread(this.Receive);
            this.m_receiveThread.Start();
        }
        
        private void Receive()
        {
            while (this.m_started)
            {
                this.m_receiveResetEvent.WaitOne();

                while (this.m_receiveQueue.TryDequeue(out ServerMessage message))
                {
                    try
                    {
                        switch (message.GetMessageCategory())
                        {
                            case ServerMessageCategory.ACCOUNT:
                                this.m_messageManager.OnReceiveAccountMessage((ServerAccountMessage) message);
                                break;
                            case ServerMessageCategory.REQUEST:
                                this.m_messageManager.OnReceiveRequestMessage((ServerRequestMessage) message);
                                break;
                            case ServerMessageCategory.SESSION:
                                this.m_messageManager.OnReceiveSessionMessage((ServerSessionMessage) message);
                                break;
                            case ServerMessageCategory.RESPONSE:
                                ServerRequestManager.ResponseReceived((ServerResponseMessage) message);
                                break;
                            case ServerMessageCategory.CORE:
                                if (!ServerMessageManager.ReceiveCoreMessage((ServerCoreMessage) message))
                                    this.m_messageManager.OnReceiveCoreMessage((ServerCoreMessage) message);
                                break;
                            default:
                                Logging.Error("ServerMessageHandler.receive: unknown message category: " + message.GetMessageCategory());
                                break;
                        }
                    }
                    catch (Exception exception)
                    {
                        Logging.Warning("ServerMessageHandler.receive: exception when the handle of message type " + message.GetMessageType() + ", trace: " + exception);
                    }
                }
            }
        }

        private void Send()
        {
            while (this.m_started)
            {
                this.m_sendResetEvent.WaitOne();

                while (this.m_sendQueue.TryDequeue(out QueueItem item))
                {
                    item.Socket.Send(ServerMessaging.WriteMessage(item.Message));
                }
            }
        }

        public void Enqueue(ServerMessage message)
        {
            this.m_receiveQueue.Enqueue(message);
            this.m_receiveResetEvent.Set();
        }

        public void Enqueue(ServerMessage message, ServerSocket socket)
        {
            this.m_sendQueue.Enqueue(new QueueItem(message, socket));
            this.m_sendResetEvent.Set();
        }

        public virtual void Destruct()
        {
            while (this.m_sendQueue.Count != 0 || this.m_receiveQueue.Count != 0)
            {
                Thread.Sleep(50);
            }

            this.m_started = false;
        }

        private struct QueueItem
        {
            internal ServerMessage Message { get; }
            internal ServerSocket Socket { get; }

            internal QueueItem(ServerMessage message, ServerSocket socket)
            {
                this.Message = message;
                this.Socket = socket;
            }
        }
    }
}