namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Titan.DataStream;

    public class RemoveAvatarStreamMessage : ServerAccountMessage
    {
        public override void Encode(ByteStream stream)
        {
        }

        public override void Decode(ByteStream stream)
        {
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.REMOVE_AVATAR_STREAM;
        }
    }
}