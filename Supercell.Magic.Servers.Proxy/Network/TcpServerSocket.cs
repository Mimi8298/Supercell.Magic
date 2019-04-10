namespace Supercell.Magic.Servers.Proxy.Network
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    using Supercell.Magic.Servers.Core;

    public class TcpServerSocket
    {
        public const int BACKLOG_SIZE = 100;
        public const int RECEIVE_BUFFER_SIZE = 1024;

        private readonly Socket m_listener;

        public TcpServerSocket(string host, int port)
        {
            this.m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.m_listener.Bind(new IPEndPoint(IPAddress.Parse(host), port));
            this.m_listener.Listen(TcpServerSocket.BACKLOG_SIZE);
        }

        public void StartAccept()
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += this.OnAccept;
            this.StartAccept(args);
        }

        private void StartAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            if (!this.m_listener.AcceptAsync(args))
            {
                this.OnAccept(null, args);
            }
        }

        private void OnAccept(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Socket socket = args.AcceptSocket;
                SocketAsyncEventArgs receiveAsyncEventArgs = new SocketAsyncEventArgs();

                receiveAsyncEventArgs.SetBuffer(new byte[TcpServerSocket.RECEIVE_BUFFER_SIZE], 0, TcpServerSocket.RECEIVE_BUFFER_SIZE);
                receiveAsyncEventArgs.Completed += TcpServerSocket.OnReceive;

                ClientConnection clientConnection = ClientConnectionManager.Create(socket, receiveAsyncEventArgs);

                receiveAsyncEventArgs.UserToken = clientConnection;

                if (!socket.ReceiveAsync(receiveAsyncEventArgs))
                {
                    TcpServerSocket.OnReceive(null, receiveAsyncEventArgs);
                }
            }

            this.StartAccept(args);
        }

        private static void OnReceive(object sender, SocketAsyncEventArgs args)
        {
            ClientConnection clientConnection = (ClientConnection)args.UserToken;

            if (clientConnection != null)
            {
                try
                {
                    do
                    {
                        if (args.SocketError != SocketError.Success || args.BytesTransferred <= 0)
                        {
                            TcpServerSocket.Disconnect(clientConnection);
                            break;
                        }

                        clientConnection.ReceiveData();
                    } while (!clientConnection.Destructed && !clientConnection.Socket.ReceiveAsync(args));
                }
                catch (SocketException)
                {
                    TcpServerSocket.Disconnect(clientConnection);
                }
                catch (Exception exception)
                {
                    Logging.Error("TcpServerSocket.onReceive - unhandled exception thrown : " + exception);
                    TcpServerSocket.Disconnect(clientConnection);
                }
            }
        }

        private static void OnSend(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success)
            {
                TcpServerSocket.Disconnect((ClientConnection)args.UserToken);
            }

            args.Dispose();
        }

        public static void Send(ClientConnection clientConnection, byte[] buffer, int length)
        {
            SocketAsyncEventArgs sendAsyncEventArgs = new SocketAsyncEventArgs();

            sendAsyncEventArgs.SetBuffer(buffer, 0, length);
            sendAsyncEventArgs.UserToken = clientConnection;
            sendAsyncEventArgs.Completed += TcpServerSocket.OnSend;

            try
            {
                if (!clientConnection.Socket.SendAsync(sendAsyncEventArgs))
                {
                    TcpServerSocket.OnSend(null, sendAsyncEventArgs);
                }
            }
            catch (Exception)
            {
                TcpServerSocket.OnSend(null, sendAsyncEventArgs);
            }
        }

        public static void Disconnect(ClientConnection clientConnection)
        {
            if (!clientConnection.Destructed)
            {
                bool removed = ClientConnectionManager.Remove(clientConnection);

                if (removed)
                {
                    clientConnection.Destruct();
                }
            }
        }
    }
}