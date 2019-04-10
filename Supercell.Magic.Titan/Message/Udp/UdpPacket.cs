namespace Supercell.Magic.Titan.Message.Udp
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Util;

    public class UdpPacket
    {
        private readonly LogicArrayList<UdpMessage> m_messages;

        private byte[] m_ackMessageIds;
        private int m_ackMessageIdCount;

        public UdpPacket()
        {
            this.m_messages = new LogicArrayList<UdpMessage>();
        }

        public void Decode(byte[] buffer, int length, LogicMessageFactory factory)
        {
            ByteStream stream = new ByteStream(buffer, length);

            if (!stream.IsAtEnd())
            {
                this.m_ackMessageIdCount = stream.ReadVInt();

                if (this.m_ackMessageIdCount <= 1400)
                {
                    this.m_ackMessageIds = stream.ReadBytes(this.m_ackMessageIdCount, 1400);

                    if (!stream.IsAtEnd())
                    {
                        int messageCount = stream.ReadVInt();

                        if (messageCount <= 1400)
                        {
                            this.m_messages.EnsureCapacity(messageCount);

                            for (int i = 0; i < messageCount; i++)
                            {
                                UdpMessage message = new UdpMessage();

                                message.Decode(stream, factory);

                                if (message.GetPiranhaMessage() == null)
                                {
                                    break;
                                }

                                this.m_messages.Add(message);
                            }
                        }
                    }
                }
            }

            stream.Destruct();
        }

        public void Encode(ByteStream byteStream)
        {
            if (this.m_ackMessageIdCount != 0 || this.m_messages.Size() != 0)
            {
                byteStream.WriteVInt(this.m_ackMessageIdCount);
                byteStream.WriteBytes(this.m_ackMessageIds, this.m_ackMessageIdCount);

                ByteStream stream = new ByteStream(1400 - byteStream.GetOffset());

                if (this.m_messages.Size() > 0)
                {
                    int streamLength = 0;
                    int encodedMessage = 0;

                    for (int i = this.m_messages.Size() - 1; i >= 0; i--, encodedMessage++, streamLength = stream.GetLength())
                    {
                        this.m_messages[i].Encode(stream);

                        if (stream.GetLength() + byteStream.GetLength() > 1410)
                        {
                            Debugger.Warning("UdpPacket::encode over max size");
                            break;
                        }
                    }

                    if (encodedMessage > 0)
                    {
                        stream.WriteVInt(encodedMessage);
                        stream.WriteBytes(stream.GetByteArray(), streamLength);
                    }
                }

                stream.Destruct();

                if (byteStream.GetLength() > 1400)
                {
                    Debugger.Warning("UdpPacket::encode too big");
                }
            }
        }

        public void AddMessage(UdpMessage message)
        {
            this.m_messages.Add(message);
        }

        public LogicArrayList<UdpMessage> GetMessages()
        {
            return this.m_messages;
        }

        public byte[] GetAckMessageIds()
        {
            return this.m_ackMessageIds;
        }

        public int GetAckMessageIdCount()
        {
            return this.m_ackMessageIdCount;
        }

        public void SetAckMessageIds(byte[] ackMessageIds, int count)
        {
            this.m_ackMessageIds = ackMessageIds;
            this.m_ackMessageIdCount = count;
        }
    }
}