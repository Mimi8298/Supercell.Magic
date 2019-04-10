namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Titan.DataStream;

    public abstract class ServerResponseMessage : ServerMessage
    {
        internal long RequestId { get; set; }
        public bool Success { get; set; }

        public sealed override void EncodeHeader(ByteStream stream)
        {
            base.EncodeHeader(stream);
            stream.WriteLongLong(this.RequestId);
            stream.WriteBoolean(this.Success);
        }

        public sealed override void DecodeHeader(ByteStream stream)
        {
            base.DecodeHeader(stream);
            this.RequestId = stream.ReadLongLong();
            this.Success = stream.ReadBoolean();
        }

        public sealed override ServerMessageCategory GetMessageCategory()
        {
            return ServerMessageCategory.RESPONSE;
        }
    }
}