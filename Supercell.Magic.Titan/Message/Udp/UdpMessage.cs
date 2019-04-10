namespace Supercell.Magic.Titan.Message.Udp
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;

    public class UdpMessage
    {
        private int m_messageId;
        private PiranhaMessage m_piranhaMessage;

        public UdpMessage()
        {
            // UdpMessage.
        }

        public UdpMessage(byte messageId)
        {
            this.m_messageId = messageId;
        }

        public int GetMessageId()
        {
            return this.m_messageId;
        }

        public PiranhaMessage GetPiranhaMessage()
        {
            return this.m_piranhaMessage;
        }

        public PiranhaMessage RemovePiranhaMessage()
        {
            PiranhaMessage message = this.m_piranhaMessage;
            this.m_piranhaMessage = null;
            return message;
        }

        public void SetPiranhaMessage(PiranhaMessage message)
        {
            this.m_piranhaMessage = message;
        }

        public void Decode(ByteStream stream, LogicMessageFactory factory)
        {
            this.m_messageId = stream.ReadByte();
            int messageType = stream.ReadVInt();
            this.m_piranhaMessage = factory.CreateMessageByType(messageType);

            if (this.m_piranhaMessage != null)
            {
                int encodingLength = stream.ReadVInt();
                this.m_piranhaMessage.GetByteStream().SetByteArray(stream.ReadBytes(encodingLength, 900000), encodingLength);
            }
            else
            {
                Debugger.Warning("UdpMessage::decode unable to read message type " + messageType);
            }
        }

        public void Encode(ByteStream stream)
        {
            int encodingLength = this.m_piranhaMessage.GetEncodingLength();

            stream.WriteByte((byte) this.m_messageId);
            stream.WriteVInt(this.m_piranhaMessage.GetMessageType());
            stream.WriteVInt(encodingLength);
            stream.WriteBytesWithoutLength(this.m_piranhaMessage.GetByteStream().GetByteArray(), encodingLength);
        }

        public bool IsMoreRecent(char messageId)
        {
            return this.m_messageId > messageId;
        }
    }
}