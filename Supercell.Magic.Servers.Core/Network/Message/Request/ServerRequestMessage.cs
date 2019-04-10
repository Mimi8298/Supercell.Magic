namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Titan.DataStream;

    public abstract class ServerRequestMessage : ServerMessage
    {
        internal long RequestId { get; set; }

        public sealed override void EncodeHeader(ByteStream stream)
        {
            base.EncodeHeader(stream);
            stream.WriteLongLong(this.RequestId);
        }

        public sealed override void DecodeHeader(ByteStream stream)
        {
            base.DecodeHeader(stream);
            this.RequestId = stream.ReadLongLong();
        }

        public sealed override ServerMessageCategory GetMessageCategory()
        {
            return ServerMessageCategory.REQUEST;
        }
    }
}