namespace Supercell.Magic.Servers.Core.Network
{
    using System.Threading;
    using NetMQ;
    using NetMQ.Sockets;
    using Supercell.Magic.Servers.Core.Settings;

    public static class ServerListenSocket
    {
        private static NetMQSocket m_listener;
        private static Thread m_receiveThread;

        private static bool m_started;

        public static void Init()
        {
            EnvironmentSettings.ServerConnectionEntry connectionEntry = EnvironmentSettings.Servers[ServerCore.Type][ServerCore.Id];

            ServerListenSocket.m_started = true;
            ServerListenSocket.m_listener = new PullSocket(string.Format("@tcp://{0}:{1}", connectionEntry.ServerIP, connectionEntry.ServerPort));
            ServerListenSocket.m_listener.Options.ReceiveHighWatermark = 10000;
            ServerListenSocket.m_receiveThread = new Thread(ServerListenSocket.Receive);
            ServerListenSocket.m_receiveThread.Start();
        }

        public static void DeInit()
        {
            ServerListenSocket.m_started = false;
        }

        private static void Receive()
        {
            while (ServerListenSocket.m_started)
            {
                NetMQMessage message = ServerListenSocket.m_listener.ReceiveMultipartMessage();

                while (!message.IsEmpty)
                {
                    ServerListenSocket.ProcessReceive(message.Pop().Buffer);
                }
            }
        }

        private static void ProcessReceive(byte[] buffer)
        {
            ServerMessaging.OnReceive(buffer, buffer.Length);
        }
    }
}