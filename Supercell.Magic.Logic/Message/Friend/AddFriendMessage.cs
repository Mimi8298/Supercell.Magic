namespace Supercell.Magic.Logic.Message.Friend
{
    public class AddFriendMessage : FriendAvatarBaseMessage
    {
        public const int MESSAGE_TYPE = 10502;

        public AddFriendMessage() : this(0)
        {
            // AddFriendMessage.
        }

        public AddFriendMessage(short messageVersion) : base(messageVersion)
        {
            // AddFriendMessage.
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
            return AddFriendMessage.MESSAGE_TYPE;
        }

        public override void Destruct()
        {
            base.Destruct();
        }
    }
}