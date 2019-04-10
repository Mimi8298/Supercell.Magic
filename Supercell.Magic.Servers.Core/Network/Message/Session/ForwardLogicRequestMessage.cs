namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class ForwardLogicRequestMessage : ServerSessionMessage
    {
        public LogicLong AccountId { get; set; }

        public short MessageType { get; set; }
        public short MessageVersion { get; set; }
        public int MessageLength { get; set; }

        public byte[] MessageBytes { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.AccountId);
            stream.WriteShort(this.MessageType);
            stream.WriteShort(this.MessageVersion);
            stream.WriteVInt(this.MessageLength);
            stream.WriteBytesWithoutLength(this.MessageBytes, this.MessageLength);
        }

        public override void Decode(ByteStream stream)
        {
            this.AccountId = stream.ReadLong();
            this.MessageType = stream.ReadShort();
            this.MessageVersion = stream.ReadShort();
            this.MessageLength = stream.ReadVInt();
            this.MessageBytes = stream.ReadBytes(this.MessageLength, 0xFFFFFF);
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.FORWARD_LOGIC_REQUEST_MESSAGE;
        }
    }
}