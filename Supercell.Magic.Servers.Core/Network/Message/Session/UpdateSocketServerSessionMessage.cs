namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Titan.DataStream;

    public class UpdateSocketServerSessionMessage : ServerSessionMessage
    {
        public int ServerType { get; set; }
        public int ServerId { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt(this.ServerType);
            stream.WriteVInt(this.ServerId);
        }

        public override void Decode(ByteStream stream)
        {
            this.ServerType = stream.ReadVInt();
            this.ServerId = stream.ReadVInt();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.UPDATE_SOCKET_SERVER_SESSION;
        }
    }
}