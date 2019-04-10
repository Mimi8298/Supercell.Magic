namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class AvatarRequestMessage : ServerRequestMessage
    {
        public LogicLong AccountId { get; set; }
        
        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.AccountId);
        }

        public override void Decode(ByteStream stream)
        {
            this.AccountId = stream.ReadLong();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.AVATAR_REQUEST;
        }
    }
}