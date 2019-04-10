namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Titan.DataStream;

    public class BindServerSocketResponseMessage : ServerResponseMessage
    {
        public int ServerType { get; set; }
        public int ServerId { get; set; }

        public override void Encode(ByteStream stream)
        {
            if (this.Success)
            {
                stream.WriteVInt(this.ServerType);
                stream.WriteVInt(this.ServerId);
            }
        }

        public override void Decode(ByteStream stream)
        {
            if (this.Success)
            {
                this.ServerType = stream.ReadVInt();
                this.ServerId = stream.ReadVInt();
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.BIND_SERVER_SOCKET_RESPONSE;
        }
    }
}