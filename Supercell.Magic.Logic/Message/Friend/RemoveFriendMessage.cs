namespace Supercell.Magic.Logic.Message.Friend
{
    public class RemoveFriendMessage : FriendAvatarBaseMessage
    {
        public const int MESSAGE_TYPE = 10506;

        public RemoveFriendMessage() : this(0)
        {
            // RemoveFriendMessage.
        }

        public RemoveFriendMessage(short messageVersion) : base(messageVersion)
        {
            // RemoveFriendMessage.
        }

        public override void Decode()
        {
            base.Decode();
        }

        public override void Encode()
        {
            base.Encode();
        }

        public override short GetMessageType()
        {
            return RemoveFriendMessage.MESSAGE_TYPE;
        }

        public override void Destruct()
        {
            base.Destruct();
        }
    }
}