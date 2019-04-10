namespace Supercell.Magic.Servers.Core.Network.Message.Core
{
    using Supercell.Magic.Titan.DataStream;

    public class ServerStatusMessage : ServerCoreMessage
    {
        public ServerStatusType Type { get; set; }
        public int Time { get; set; }
        public int NextTime { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt((int) this.Type);
            stream.WriteVInt(this.Time);
            stream.WriteVInt(this.NextTime);
        }

        public override void Decode(ByteStream stream)
        {
            this.Type = (ServerStatusType) stream.ReadVInt();
            this.Time = stream.ReadVInt();
            this.NextTime = stream.ReadVInt();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.SERVER_STATUS;
        }
    }
}