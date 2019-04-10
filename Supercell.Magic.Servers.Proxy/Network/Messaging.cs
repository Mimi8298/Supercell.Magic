namespace Supercell.Magic.Servers.Proxy.Network
{
    using System;
    using Supercell.Magic.Logic.Message;
    using Supercell.Magic.Logic.Message.Account;
    using Supercell.Magic.Logic.Message.Security;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Settings;
    using Supercell.Magic.Servers.Proxy.Network.Message;
    using Supercell.Magic.Titan;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Message.Security;

    public class Messaging
    {
        public const int HEADER_SIZE = 7;
        public const int NONCE_LENGTH = 24;
        public const int NONCE_METHOD = 1;

        private readonly ClientConnection m_clientConnection;
        private readonly LogicMessageFactory m_messageFactory;

        private StreamEncrypter m_receiveEncrypter;
        private StreamEncrypter m_sendEncrypter;
        
        private int m_scramblerSeed;
        private bool m_encryptionScrambled;

        private PepperState m_pepperState;

        public Messaging(ClientConnection clientConnection)
        {
            this.m_clientConnection = clientConnection;
            this.m_messageFactory = LogicMagicMessageFactory.Instance;
            this.m_pepperState = PepperState.DEFAULT;
        }

        public void InitRC4Encryption(string nonce)
        {
            if (this.m_receiveEncrypter != null)
                this.m_receiveEncrypter.Destruct();
            if (this.m_sendEncrypter != null)
                this.m_sendEncrypter.Destruct();

            this.m_receiveEncrypter = new RC4Encrypter(LogicMessagingConstants.RC4_KEY, nonce);
            this.m_sendEncrypter = new RC4Encrypter(LogicMessagingConstants.RC4_KEY, nonce);
        }

        public void InitPepperEncryption()
        {
            throw new NotImplementedException();
        }

        public string GetDefaultNonce()
        {
            if (EnvironmentSettings.IsStageServer()) return ExtendedSetEncryptionMessage.STAGE_NONCE;
            if (EnvironmentSettings.IsIntegrationServer()) return ExtendedSetEncryptionMessage.INTEGRATION_NONCE;
            return ExtendedSetEncryptionMessage.DEFAULT_NONCE;
        }

        private int DecryptUsingEncrypter(byte[] input, byte[] output, int length)
        {
            return this.m_receiveEncrypter.Decrypt(input, output, length);
        }

        private int EncryptUsingEncrypter(byte[] intput, byte[] output, int length)
        {
            return this.m_sendEncrypter.Encrypt(intput, output, length);
        }

        public void SetScramblerSeed(int seed)
        {
            this.m_scramblerSeed = seed;
        }

        private void SetEncryption(ExtendedSetEncryptionMessage message)
        {
            byte[] nonce = message.RemoveNonce();
            int nonceMethod = message.GetNonceMethod();

            if (nonceMethod == 1)
            {
                this.ScrambleNonceUsingMersenneTwister(nonce, nonce.Length);
            }
            else
            {
                this.ScrambleNonceUsingDefaultMethod(nonce, nonce.Length);
            }

            char[] nonceChars = new char[nonce.Length];

            for (int i = 0; i < nonce.Length; i++)
            {
                nonceChars[i] = (char)nonce[i];
            }

            this.InitRC4Encryption(new string(nonceChars));
            this.m_encryptionScrambled = true;
        }

        private void ScrambleNonceUsingMersenneTwister(byte[] nonce, int length)
        {
            LogicMersenneTwisterRandom scrambler = new LogicMersenneTwisterRandom(this.m_scramblerSeed);

            int key = 0;

            for (int i = 0; i < 100; i++)
                key = scrambler.Rand(256);
            for (int i = 0; i < length; i++)
                nonce[i] ^= (byte)(key & (byte)scrambler.Rand(256));
        }

        private void ScrambleNonceUsingDefaultMethod(byte[] nonce, int length)
        {
            LogicRandom scrambler = new LogicRandom(this.m_scramblerSeed);

            for (int i = 0; i < length; i++)
            {
                nonce[i] ^= (byte) scrambler.Rand(256);
            }
        }

        public int OnReceive(byte[] buffer, int length)
        {
            if (length >= Messaging.HEADER_SIZE)
            {
                Messaging.ReadHeader(buffer, out int messageType, out int messageLength, out int messageVersion);

                if (length - Messaging.HEADER_SIZE >= messageLength)
                {
                    byte[] encryptedBytes = new byte[messageLength];
                    byte[] encodingBytes;

                    Buffer.BlockCopy(buffer, Messaging.HEADER_SIZE, encryptedBytes, 0, messageLength);

                    int encodingLength;

                    if (this.m_receiveEncrypter != null)
                    {
                        encodingLength = messageLength - this.m_receiveEncrypter.GetOverheadEncryption();
                        encodingBytes = new byte[encodingLength];

                        this.DecryptUsingEncrypter(encryptedBytes, encodingBytes, messageLength);
                    }
                    else
                    {
                        if (this.m_pepperState == PepperState.DEFAULT)
                        {
                            if (messageType == ClientHelloMessage.MESSAGE_TYPE)
                            {
                                this.m_pepperState = PepperState.AUTHENTIFICATION;

                                encodingLength = messageLength;
                                encodingBytes = encryptedBytes;
                            }
                            else if (messageType == LoginMessage.MESSAGE_TYPE)
                            {
                                this.m_pepperState = PepperState.DISABLED;
                                this.InitRC4Encryption(this.GetDefaultNonce());

                                encodingLength = messageLength - this.m_receiveEncrypter.GetOverheadEncryption();
                                encodingBytes = new byte[encodingLength];

                                this.DecryptUsingEncrypter(encryptedBytes, encodingBytes, messageLength);
                            }
                            else
                            {
                                return messageLength + Messaging.HEADER_SIZE;
                            }
                        }
                        else if (this.m_pepperState == PepperState.AUTHENTIFICATION_SERVER)
                        {
                            if (messageType != LoginMessage.MESSAGE_TYPE)
                                return messageLength + Messaging.HEADER_SIZE;
                            throw new NotImplementedException();
                        }
                        else
                        {
                            return messageLength + Messaging.HEADER_SIZE;
                        }
                    }

                    PiranhaMessage piranhaMessage = this.m_messageFactory.CreateMessageByType(messageType);

                    if (piranhaMessage != null)
                    {
                        piranhaMessage.GetByteStream().SetByteArray(encodingBytes, encodingLength);
                        piranhaMessage.SetMessageVersion(messageVersion);

                        try
                        {
                            piranhaMessage.Decode();

                            if (!piranhaMessage.IsServerToClientMessage())
                            {
                                MessageHandler.EnqueueReceive(piranhaMessage, this.m_clientConnection);
                            }
                        }
                        catch (Exception exception)
                        {
                            Logging.Error(string.Format("Messaging::onReceive: error while the decoding of message type {0}, trace: {1}", messageType, exception));
                        }
                    }
                    else
                    {
                        Logging.Warning(string.Format("Messaging::onReceive: ignoring message of unknown type {0}", messageType));
                    }

                    return messageLength + Messaging.HEADER_SIZE;
                }
                else
                {
                    int httpHeader = buffer[0] << 16 | buffer[1] << 8 | buffer[2];

                    if (httpHeader == 0x474554) // httpHeader == GET
                    {
                        return -1;
                    }
                }
            }

            return 0;
        }

        public void Send(PiranhaMessage message)
        {
            if (message.IsServerToClientMessage())
            {
                MessageHandler.EnqueueSend(message, this.m_clientConnection);
            }
        }

        public void InternalSend(PiranhaMessage message)
        {
            if (message.GetEncodingLength() == 0)
                message.Encode();

            int encodingLength = message.GetEncodingLength();
            int encryptedLength;

            byte[] encodingBytes = message.GetMessageBytes();
            byte[] encryptedBytes;

            if (this.m_sendEncrypter != null)
            {
                if (!this.m_encryptionScrambled && message.GetMessageType() == LoginOkMessage.MESSAGE_TYPE)
                {
                    byte[] nonce = Messaging.CreateNonce();

                    ExtendedSetEncryptionMessage extendedSetEncryptionMessage = new ExtendedSetEncryptionMessage();

                    extendedSetEncryptionMessage.SetNonce(nonce);
                    extendedSetEncryptionMessage.SetNonceMethod(Messaging.NONCE_METHOD);

                    this.InternalSend(extendedSetEncryptionMessage);
                    this.SetEncryption(extendedSetEncryptionMessage);
                }

                encryptedLength = encodingLength + this.m_sendEncrypter.GetOverheadEncryption();
                encryptedBytes = new byte[encryptedLength];

                this.EncryptUsingEncrypter(encodingBytes, encryptedBytes, encodingLength);
            }
            else if (this.m_pepperState != PepperState.DISABLED)
            {
                if (this.m_pepperState == PepperState.AUTHENTIFICATION)
                {
                    if (message.GetMessageType() == ServerHelloMessage.MESSAGE_TYPE)
                        this.m_pepperState = PepperState.AUTHENTIFICATION_SERVER;

                    encryptedLength = encodingLength;
                    encryptedBytes = encodingBytes;
                }
                else if (this.m_pepperState == PepperState.LOGIN)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    encryptedLength = encodingLength;
                    encryptedBytes = encodingBytes;
                }
            }
            else
            {
                encryptedBytes = encodingBytes;
                encryptedLength = encodingLength;
            }

            byte[] stream = new byte[encryptedLength + Messaging.HEADER_SIZE];

            Messaging.WriteHeader(message, stream, encryptedLength);
            Buffer.BlockCopy(encryptedBytes, 0, stream, Messaging.HEADER_SIZE, encryptedLength);

            this.m_clientConnection.Send(stream, encryptedLength + Messaging.HEADER_SIZE);
        }

        private static byte[] CreateNonce()
        {
            byte[] sessionKey = new byte[Messaging.NONCE_LENGTH];

            for (int i = 0; i < Messaging.NONCE_LENGTH; i += 4)
            {
                int random = ServerCore.Random.Rand(int.MaxValue);

                sessionKey[i] = (byte)(random >> 24);
                sessionKey[i + 1] = (byte)(random >> 16);
                sessionKey[i + 2] = (byte)(random >> 8);
                sessionKey[i + 3] = (byte)(random);
            }

            return sessionKey;
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
                Logging.Error("NetworkMessaging::writeHeader trying to send too big message, type " + messageType);
            }
        }

        private enum PepperState
        {
            DISABLED,
            DEFAULT,
            AUTHENTIFICATION,
            AUTHENTIFICATION_SERVER,
            LOGIN
        }
    }
}