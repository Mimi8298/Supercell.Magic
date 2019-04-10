namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public abstract class ServerAccountMessage : ServerMessage
    {
        public LogicLong AccountId { get; set; }

        public sealed override void EncodeHeader(ByteStream stream)
        {
            base.EncodeHeader(stream);
            stream.WriteLong(this.AccountId);
        }

        public sealed override void DecodeHeader(ByteStream stream)
        {
            base.DecodeHeader(stream);
            this.AccountId = stream.ReadLong();
        }

        public sealed override ServerMessageCategory GetMessageCategory()
        {
            return ServerMessageCategory.ACCOUNT;
        }
    }
}