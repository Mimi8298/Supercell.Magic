namespace Supercell.Magic.Servers.Core.Network
{
    using System.Threading;
    using System.Threading.Tasks;
    using NetMQ;
    using NetMQ.Sockets;

    public class ServerSocket
    {
        private NetMQSocket m_socket;

        public int ServerType { get; }
        public int ServerId { get; }

        public ServerSocket(int type, int id, string host, int port)
        {
            this.ServerType = type;
            this.ServerId = id;

            this.m_socket = new PushSocket(string.Format(">tcp://{0}:{1}", host, port));
            this.m_socket.Options.SendHighWatermark = 10000;
        }

        public void Destruct()
        {
            if (this.m_socket != null)
            {
                this.m_socket.Dispose();
                this.m_socket = null;
            }
        }

        public void Send(byte[] buffer)
        {
            this.m_socket.SendFrame(buffer);
        }

        public override string ToString()
        {
            return this.ServerType + "-" + this.ServerId;
        }
    }
}