namespace Supercell.Magic.Tools.Client.Network
{
    using System;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using Supercell.Magic.Logic.Message;
    using Supercell.Magic.Logic.Message.Security;
    using Supercell.Magic.Titan;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class Messaging
    {
        private const int PACKET_HEADER_LENGTH = 7;

        private readonly Socket m_socket;
        private readonly SocketBuffer m_receiveBuffer;
        private readonly LogicMessageFactory m_messageFactory;
        private readonly ConcurrentQueue<PiranhaMessage> m_receiveQueue;
        private readonly ConcurrentQueue<PiranhaMessage> m_sendQueue;

        private StreamEncrypter m_receiveEncrypter;
        private StreamEncrypter m_sendEncrypter;

        private int m_scramblerSeed;

        public Messaging()
        {
            this.m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.m_receiveBuffer = new SocketBuffer(2048);
            this.m_messageFactory = LogicMagicMessageFactory.Instance;
            this.m_receiveQueue = new ConcurrentQueue<PiranhaMessage>();
            this.m_sendQueue = new ConcurrentQueue<PiranhaMessage>();

            this.m_scramblerSeed = Environment.TickCount;
            this.InitEncrypters();
        }

        public void Destruct()
        {
            this.m_socket.Close();
        }

        public void InitEncrypters(string nonce = "nonce")
        {
            this.m_receiveEncrypter = new RC4Encrypter(LogicMessagingConstants.RC4_KEY, nonce);
            this.m_sendEncrypter = new RC4Encrypter(LogicMessagingConstants.RC4_KEY, nonce);
        }

        public void Connect(string host, int port)
        {
            if (this.m_socket.Connected)
                throw new Exception("Messaging.connect: m_socket already connected");

            SocketAsyncEventArgs connectEventArgs = new SocketAsyncEventArgs();

            connectEventArgs.RemoteEndPoint = new DnsEndPoint(host, port);
            connectEventArgs.Completed += this.OnConnect;

            if (!this.m_socket.ConnectAsync(connectEventArgs))
                this.OnConnect(null, connectEventArgs);
        }

        private void OnConnect(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                SocketAsyncEventArgs receiveEventArgs = new SocketAsyncEventArgs();

                receiveEventArgs.SetBuffer(new byte[512], 0, 512);
                receiveEventArgs.Completed += this.OnReceive;

                if (!this.m_socket.ReceiveAsync(receiveEventArgs))
                    this.OnReceive(null, args);

                Debugger.Print("Messaging.onConnect: connected to server " + ((DnsEndPoint) args.RemoteEndPoint).Host);
            }
            else
            {
                this.m_socket.Close();
                Debugger.Warning(string.Format("Messaging.onConnect: unable to connect to server {0} ({1})", args.RemoteEndPoint, args.SocketError));
            }

            args.Dispose();
        }

        private void OnReceive(object sender, SocketAsyncEventArgs args)
        {
            do
            {
                if (args.SocketError != SocketError.Success || args.BytesTransferred <= 0)
                {
                    Debugger.Warning(string.Format("Messaging.onReceive: connection with the server {0} has been lost ({1})", this.m_socket.RemoteEndPoint, args.SocketError));
                    break;
                }

                this.m_receiveBuffer.Write(args.Buffer, args.BytesTransferred);
                this.OnReceive();

            } while (!this.m_socket.ReceiveAsync(args));
        }

        private void OnReceive()
        {
            int length;

            while ((length = this.m_receiveBuffer.Size()) != 0)
            {
                int read = this.OnReceive(this.m_receiveBuffer.GetBuffer(), length);
                if (read <= 0)
                    break;
                this.m_receiveBuffer.Remove(read);
            }
        }

        private int OnReceive(byte[] buffer, int length)
        {
            if (length >= Messaging.PACKET_HEADER_LENGTH)
            {
                Messaging.ReadHeader(buffer, out int messageType, out int messageLength, out int messageVersion);

                if (length >= Messaging.PACKET_HEADER_LENGTH + messageLength)
                {
                    byte[] encryptedBytes = new byte[messageLength];
                    byte[] encodingBytes;

                    Buffer.BlockCopy(buffer, Messaging.PACKET_HEADER_LENGTH, encryptedBytes, 0, messageLength);

                    int encodingLength;

                    if (this.m_receiveEncrypter != null)
                    {
                        encodingLength = messageLength - this.m_receiveEncrypter.GetOverheadEncryption();
                        encodingBytes = new byte[encodingLength];

                        this.m_receiveEncrypter.Decrypt(encryptedBytes, encodingBytes, encodingLength);
                    }
                    else
                    {
                        encodingLength = messageLength;
                        encodingBytes = encryptedBytes;
                    }

                    PiranhaMessage piranhaMessage = this.m_messageFactory.CreateMessageByType(messageType);

                    if (piranhaMessage != null)
                    {
                        piranhaMessage.SetMessageVersion(messageVersion);
                        piranhaMessage.GetByteStream().SetByteArray(encodingBytes, encodingLength);
                        piranhaMessage.Decode();
                        
                        if (piranhaMessage.GetMessageType() != ExtendedSetEncryptionMessage.MESSAGE_TYPE)
                        {
                            this.m_receiveQueue.Enqueue(piranhaMessage);
                        }
                        else
                        {
                            ExtendedSetEncryptionMessage extendedSetEncryptionMessage = (ExtendedSetEncryptionMessage) piranhaMessage;
                            byte[] nonce = extendedSetEncryptionMessage.RemoveNonce();

                            switch (extendedSetEncryptionMessage.GetNonceMethod())
                            {
                                case 1:
                                    this.ScrambleNonceUsingMersenneTwister(nonce);
                                    break;
                                default:
                                    this.ScrambleNonceUsingDefaultMethod(nonce);
                                    break;
                            }

                            char[] nonceChars = new char[nonce.Length];
                            for (int i = 0; i < nonce.Length; i++)
                                nonceChars[i] = (char) nonce[i];
                            this.InitEncrypters(new string(nonceChars));
                        }
                    }
                    else
                    {
                        Debugger.Warning(string.Format("Messaging.onReceive: ignoring message of unknown type {0}", messageType));
                    }

                    return Messaging.PACKET_HEADER_LENGTH + messageLength;
                }
            }

            return 0;
        }

        public bool TryDequeueReceiveMessage(out PiranhaMessage message)
        {
            return this.m_receiveQueue.TryDequeue(out message);
        }

        public void Send(PiranhaMessage piranhaMessage)
        {
            if (!piranhaMessage.IsServerToClientMessage())
                this.m_sendQueue.Enqueue(piranhaMessage);
        }

        public void SendQueue()
        {
            while (this.m_sendQueue.TryDequeue(out PiranhaMessage piranhaMessage))
            {
                if (piranhaMessage.GetEncodingLength() == 0)
                    piranhaMessage.Encode();

                int encodingLength = piranhaMessage.GetEncodingLength();
                int encryptedLength;

                byte[] encodingBytes = piranhaMessage.GetMessageBytes();
                byte[] encryptedBytes;

                if (this.m_sendEncrypter != null)
                {
                    encryptedLength = encodingLength + this.m_sendEncrypter.GetOverheadEncryption();
                    encryptedBytes = new byte[encryptedLength];

                    this.m_sendEncrypter.Encrypt(encodingBytes, encryptedBytes, encodingLength);
                }
                else
                {
                    encryptedBytes = encodingBytes;
                    encryptedLength = encodingLength;
                }

                byte[] stream = new byte[encryptedLength + Messaging.PACKET_HEADER_LENGTH];

                Messaging.WriteHeader(piranhaMessage, stream, encryptedLength);
                Buffer.BlockCopy(encryptedBytes, 0, stream, Messaging.PACKET_HEADER_LENGTH, encryptedLength);

                this.Send(stream, encryptedLength + Messaging.PACKET_HEADER_LENGTH);
            }
        }

        private void Send(byte[] buffer, int length)
        {
            if (this.m_socket.Connected)
            {
                SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();

                sendEventArgs.SetBuffer(buffer, 0, length);
                sendEventArgs.Completed += this.OnSend;

                if (!this.m_socket.SendAsync(sendEventArgs))
                    this.OnSend(null, sendEventArgs);
            }
        }

        private void OnSend(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success)
            {
                this.m_socket.Close();
                Debugger.Warning(string.Format("Messaging.onSend: connection with the server {0} has been lost ({1})", this.m_socket.RemoteEndPoint, args.SocketError));
            }

            args.Dispose();
        }

        private void ScrambleNonceUsingMersenneTwister(byte[] nonce)
        {
            LogicMersenneTwisterRandom scrambler = new LogicMersenneTwisterRandom(this.m_scramblerSeed);

            int key = 0;

            for (int i = 0; i < 100; i++)
                key = scrambler.Rand(256);
            for (int i = 0; i < nonce.Length; i++)
                nonce[i] ^= (byte)(key & (byte)scrambler.Rand(256));
        }

        private void ScrambleNonceUsingDefaultMethod(byte[] nonce)
        {
            LogicRandom scrambler = new LogicRandom(this.m_scramblerSeed);

            for (int i = 0; i < nonce.Length; i++)
            {
                nonce[i] ^= (byte)scrambler.Rand(256);
            }
        }

        public int GetScramblerSeed()
        {
            return this.m_scramblerSeed;
        }

        public bool IsConnected()
        {
            return this.m_socket.Connected;
        }

        private static void ReadHeader(byte[] stream, out int messageType, out int messageLength, out int messageVersion)
        {
            messageType = stream[0] << 8 | stream[1];
            messageLength = stream[2] << 16 | stream[3] << 8 | stream[4];
            messageVersion = stream[5] << 8 | stream[6];
        }

        private static void WriteHeader(PiranhaMessage message, byte[] stream, int length)
        {
            int messageType = message.GetMessageType();
            int messageVersion = message.GetMessageVersion();

            stream[0] = (byte)(messageType >> 8);
            stream[1] = (byte)(messageType);
            stream[2] = (byte)(length >> 16);
            stream[3] = (byte)(length >> 8);
            stream[4] = (byte)(length);
            stream[5] = (byte)(messageVersion >> 8);
            stream[6] = (byte)(messageVersion);

            if (length > 0xFFFFFF)
            {
                Debugger.Error("NetworkMessaging::writeHeader trying to send too big message, type " + messageType);
            }
        }
    }
}