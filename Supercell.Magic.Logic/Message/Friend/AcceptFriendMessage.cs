namespace Supercell.Magic.Logic.Message.Friend
{
    public class AcceptFriendMessage : FriendAvatarBaseMessage
    {
        public const int MESSAGE_TYPE = 10501;

        public AcceptFriendMessage() : this(0)
        {
            // AcceptFriendMessage.
        }

        public AcceptFriendMessage(short messageVersion) : base(messageVersion)
        {
            // AcceptFriendMessage.
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
            return AcceptFriendMessage.MESSAGE_TYPE;
        }

        public override void Destruct()
        {
            base.Destruct();
        }
    }
}