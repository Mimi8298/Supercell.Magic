namespace Supercell.Magic.Logic.Message.Friend
{
    public class StartFriendLiveSpectateMessage : FriendAvatarBaseMessage
    {
        public const int MESSAGE_TYPE = 10507;

        public StartFriendLiveSpectateMessage() : this(0)
        {
            // StartFriendLiveSpectateMessage.
        }

        public StartFriendLiveSpectateMessage(short messageVersion) : base(messageVersion)
        {
            // StartFriendLiveSpectateMessage.
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
            return StartFriendLiveSpectateMessage.MESSAGE_TYPE;
        }

        public override void Destruct()
        {
            base.Destruct();
        }
    }
}