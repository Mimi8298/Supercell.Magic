namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Titan.DataStream;

    public class StopSessionMessage : ServerSessionMessage
    {
        public int Reason { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt(this.Reason);
        }

        public override void Decode(ByteStream stream)
        {
            this.Reason = stream.ReadVInt();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.STOP_SESSION;
        }
    }
}