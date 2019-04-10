namespace Supercell.Magic.Servers.Core.Cluster
{
    using System.Diagnostics;
    using System.Threading;

    using NetMQ;
    using NetMQ.Sockets;

    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;

    public class ClusterInstance
    {
        private bool m_started;

        protected readonly int m_id;
        private readonly int m_logicUpdateFrequency;

        private readonly Thread m_networkThread;
        private readonly Thread m_logicThread;
        private readonly PushSocket m_pushSocket;
        private readonly PullSocket m_pullSocket;
        private readonly Stopwatch m_pingWatch;

        protected int m_ping;

        public ClusterInstance(int id, int logicUpdateFrequency = -1)
        {
            this.m_started = true;
            this.m_id = id;
            this.m_pullSocket = new PullSocket();
            this.m_pullSocket.Bind(this.GetConnectionString());
            this.m_pushSocket = new PushSocket();
            this.m_pushSocket.Connect(this.GetConnectionString());
            this.m_networkThread = new Thread(this.NetworkUpdate);
            this.m_networkThread.Name = string.Format("Cluster #{0}: Network Thread", this.m_id);

            if (logicUpdateFrequency >= 0)
            {
                this.m_logicUpdateFrequency = logicUpdateFrequency;
                this.m_logicThread = new Thread(this.LogicUpdate);
                this.m_logicThread.Name = string.Format("Cluster #{0}: Logic Thread", this.m_id);
                this.m_logicThread.Start();
            }

            this.m_networkThread.Start();
            this.m_pingWatch = new Stopwatch();
        }

        public void Stop()
        {
            this.m_started = false;
        }

        private void NetworkUpdate()
        {
            while (this.m_started)
            {
                NetMQMessage message = this.m_pullSocket.ReceiveMultipartMessage();

                while (!message.IsEmpty)
                {
                    this.OnReceive(message.Pop().Buffer);
                }
            }
        }

        private void LogicUpdate()
        {
            while (this.m_started)
            {
                Thread.Sleep(this.m_logicUpdateFrequency);
                this.Tick();
            }
        }

        private void OnReceive(byte[] buffer)
        {
            if (buffer.Length > 0)
            {
                ServerMessage message = ServerMessaging.ReadMessage(buffer, buffer.Length);

                if (message != null)
                {
                    this.ReceiveMessage(message);
                }
            }
            else
            {
                this.m_pingWatch.Stop();
                this.m_ping = (int) this.m_pingWatch.ElapsedMilliseconds;
                this.OnPingTestCompleted();
            }
        }

        private string GetConnectionString()
        {
            return "inproc://cluster-" + this.m_id;
        }

        protected virtual void ReceiveMessage(ServerMessage message)
        {
        }

        protected virtual void Tick()
        {
        }

        protected virtual void OnPingTestCompleted()
        {
        }

        public void SendMessage(ServerMessage message)
        {
            this.m_pushSocket.SendFrame(ServerMessaging.WriteMessage(message));
        }

        public void SendPing()
        {
            this.m_pingWatch.Start();
            this.m_pushSocket.SendFrame(new byte[0]);
        }
    }
}