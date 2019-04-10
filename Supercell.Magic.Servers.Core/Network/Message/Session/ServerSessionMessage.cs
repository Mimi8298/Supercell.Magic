namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Titan.DataStream;

    public abstract class ServerSessionMessage : ServerMessage
    {
        public long SessionId { get; set; }

        public sealed override void EncodeHeader(ByteStream stream)
        {
            base.EncodeHeader(stream);
            stream.WriteLongLong(this.SessionId);
        }

        public sealed override void DecodeHeader(ByteStream stream)
        {
            base.DecodeHeader(stream);
            this.SessionId = stream.ReadLongLong();
        }

        public sealed override ServerMessageCategory GetMessageCategory()
        {
            return ServerMessageCategory.SESSION;
        }
    }
}