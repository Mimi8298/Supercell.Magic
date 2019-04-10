namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Titan.DataStream;

    public class BindServerSocketRequestMessage : ServerRequestMessage
    {
        public long SessionId { get; set; }
        public int ServerType { get; set; }
        public int ServerId { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLongLong(this.SessionId);
            stream.WriteVInt(this.ServerType);
            stream.WriteVInt(this.ServerId);
        }

        public override void Decode(ByteStream stream)
        {
            this.SessionId = stream.ReadLongLong();
            this.ServerType = stream.ReadVInt();
            this.ServerId = stream.ReadVInt();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.BIND_SERVER_SOCKET_REQUEST;
        }
    }
}