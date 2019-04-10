namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Titan.DataStream;

    public class ForwardLogicMessage : ServerSessionMessage
    {
        public short MessageType { get; set; }
        public short MessageVersion { get; set; }
        public int MessageLength { get; set; }

        public byte[] MessageBytes { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteShort(this.MessageType);
            stream.WriteShort(this.MessageVersion);
            stream.WriteVInt(this.MessageLength);
            stream.WriteBytesWithoutLength(this.MessageBytes, this.MessageLength);
        }

        public override void Decode(ByteStream stream)
        {
            this.MessageType = stream.ReadShort();
            this.MessageVersion = stream.ReadShort();
            this.MessageLength = stream.ReadVInt();
            this.MessageBytes = stream.ReadBytes(this.MessageLength, 0xFFFFFF);
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.FORWARD_LOGIC_MESSAGE;
        }
    }
}