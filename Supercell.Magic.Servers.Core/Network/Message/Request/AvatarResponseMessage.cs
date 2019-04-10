namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Titan.DataStream;

    public class AvatarResponseMessage : ServerResponseMessage
    {
        public LogicClientAvatar LogicClientAvatar { get; set; }
        
        public override void Encode(ByteStream stream)
        {
            if (this.Success)
            {
                this.LogicClientAvatar.Encode(stream);
            }
        }

        public override void Decode(ByteStream stream)
        {
            if (this.Success)
            {
                this.LogicClientAvatar = new LogicClientAvatar();
                this.LogicClientAvatar.Decode(stream);
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.AVATAR_RESPONSE;
        }
    }
}