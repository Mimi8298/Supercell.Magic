namespace Supercell.Magic.Servers.Proxy.Network
{
    using System.Collections.Concurrent;
    using System.Net.Sockets;
    using System.Threading;
    using Supercell.Magic.Logic.Message.Account;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Titan.Debug;

    public static class ClientConnectionManager
    {
        private static Thread m_thread;
        private static ConcurrentDictionary<long, ClientConnection> m_connections;

        private static bool m_started;
        private static long m_counter;

        public static void Init()
        {
            ClientConnectionManager.m_started = true;

            ClientConnectionManager.m_connections = new ConcurrentDictionary<long, ClientConnection>();
            ClientConnectionManager.m_thread = new Thread(ClientConnectionManager.Update);
            ClientConnectionManager.m_thread.Name = "Client Connection Manager";
            ClientConnectionManager.m_thread.Start();
        }

        private static void Update()
        {
            while (ClientConnectionManager.m_started)
            {
                foreach (ClientConnection connection in ClientConnectionManager.m_connections.Values)
                {
                    if (!connection.MessageManager.IsAlive())
                    {
                        TcpServerSocket.Disconnect(connection);
                        continue;
                    }
                }
                
                Thread.Sleep(1000);
            }
        }

        public static ClientConnection Create(Socket socket, SocketAsyncEventArgs args)
        {
            long id = ClientConnectionManager.m_counter++;
            ClientConnection clientConnection = new ClientConnection(socket, args, id);
            Logging.Assert(ClientConnectionManager.m_connections.TryAdd(id, clientConnection),
                "ClientConnectionManager.m_connections.TryAdd(id, clientConnection) return false");
            return clientConnection;
        }

        public static bool TryGet(long id, out ClientConnection clientConnection)
        {
            return ClientConnectionManager.m_connections.TryGetValue(id, out clientConnection);
        }

        public static bool Remove(ClientConnection clientConnection)
        {
            return ClientConnectionManager.m_connections.TryRemove(clientConnection.Id, out _);
        }
        
        public static void DisconnectConnections()
        {
            foreach (ClientConnection connection in ClientConnectionManager.m_connections.Values)
            {
                TcpServerSocket.Disconnect(connection);
            }
        }

        public static void SendShutdownStartedMessageToConnections(int remainingSeconds)
        {
            foreach (ClientConnection connection in ClientConnectionManager.m_connections.Values)
            {
                ShutdownStartedMessage shutdownStartedMessage = new ShutdownStartedMessage();
                shutdownStartedMessage.SetSecondsUntilShutdown(remainingSeconds);
                connection.Messaging.Send(shutdownStartedMessage);
            }
        }

        public static void Disconnect(ClientConnection clientConnection, int reason)
        {
            if (clientConnection.State == ClientConnectionState.LOGGED)
            {
                if (reason != 0)
                {
                    DisconnectedMessage disconnectedMessage = new DisconnectedMessage();
                    disconnectedMessage.SetReason(reason);
                    clientConnection.Messaging.Send(disconnectedMessage);
                }
                else
                {
                    TcpServerSocket.Disconnect(clientConnection);
                }
            }

            clientConnection.SetState(ClientConnectionState.DISCONNECTED);
        }

        public static int GetCount()
        {
            return ClientConnectionManager.m_connections.Count;
        }
    }
}