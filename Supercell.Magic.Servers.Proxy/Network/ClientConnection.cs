namespace Supercell.Magic.Servers.Proxy.Network
{
    using System.Net;
    using System.Net.Sockets;
    using MaxMind.GeoIP2.Responses;
    using Supercell.Magic.Servers.Proxy.Network.Message;
    using Supercell.Magic.Servers.Proxy.Session;

    public class ClientConnection
    {
        private readonly SocketBuffer m_receiveBuffer;
        private readonly SocketAsyncEventArgs m_receiveAsyncEventArgs;

        public Socket Socket { get; }
        public Messaging Messaging { get; }
        public MessageManager MessageManager { get; }
        public ProxySession Session { get; private set; }

        public string Location { get; }

        public bool Destructed { get; private set; }
        public long Id { get; private set; }
        public ClientConnectionState State { get; private set; }

        public IPAddress ClientIP
        {
            get { return ((IPEndPoint) this.Socket.RemoteEndPoint).Address; }
        }

        public ClientConnection(Socket socket, SocketAsyncEventArgs receiveAsyncEventArgs, long id)
        {
            this.Id = id;
            this.Socket = socket;
            this.m_receiveAsyncEventArgs = receiveAsyncEventArgs;
            this.m_receiveBuffer = new SocketBuffer(4096);
            this.Messaging = new Messaging(this);
            this.MessageManager = new MessageManager(this);
            this.State = ClientConnectionState.DEFAULT;
            this.Location = ServerProxy.MaxMind.TryCountry(this.ClientIP, out CountryResponse response)
                ? response.Country.IsoCode
                : "LO";
        }

        public void Destruct()
        {
            if (!this.Destructed)
            {
                this.Destructed = true;
                this.Id = -1;
                this.Socket.Close(5);
                this.m_receiveAsyncEventArgs.Dispose();
                this.State = ClientConnectionState.DISCONNECTED;
                this.DestructSession();
            }
        }

        public void DestructSession()
        {
            if (this.Session != null)
            {
                ProxySessionManager.Remove(this.Session);

                this.Session.Destruct();
                this.Session = null;
            }
        }

        public void SetState(ClientConnectionState state)
        {
            if (this.State != ClientConnectionState.DISCONNECTED)
            {
                this.State = state;
            }
        }

        public void SetSession(ProxySession session)
        {
            this.Session = session;
        }

        public void ReceiveData()
        {
            if (!this.Destructed)
            {
                this.m_receiveBuffer.Write(this.m_receiveAsyncEventArgs.Buffer, this.m_receiveAsyncEventArgs.BytesTransferred);

                int length = this.m_receiveBuffer.Size();

                do
                {
                    int read = this.Messaging.OnReceive(this.m_receiveBuffer.GetBuffer(), length);

                    if (read <= 0)
                    {
                        if (read == -1)
                            TcpServerSocket.Disconnect(this);
                        break;
                    }

                    this.m_receiveBuffer.Remove(read);
                } while ((length = this.m_receiveBuffer.Size()) > 0);
            }
        }

        public void Send(byte[] buffer, int length)
        {
            if (!this.Destructed)
            {
                TcpServerSocket.Send(this, buffer, length);
            }
        }
    }

    public enum ClientConnectionState
    {
        DEFAULT,
        LOGIN,
        LOGGED,
        LOGIN_FAILED,
        DISCONNECTED,
    }
}