namespace Supercell.Magic.Logic.Message.Friend
{
    using Supercell.Magic.Titan.Message;

    public class FriendListUpdateMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20106;
        private FriendEntry m_friendEntry;

        public FriendListUpdateMessage() : this(0)
        {
            // FriendListUpdateMessage.
        }

        public FriendListUpdateMessage(short messageVersion) : base(messageVersion)
        {
            // FriendListUpdateMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_friendEntry = new FriendEntry();
            this.m_friendEntry.Decode(this.m_stream);
        }

        public override void Encode()
        {
            base.Encode();
            this.m_friendEntry.Encode(this.m_stream);
        }

        public override short GetMessageType()
        {
            return FriendListUpdateMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 3;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_friendEntry = null;
        }

        public FriendEntry RemoveFriendEntry()
        {
            FriendEntry tmp = this.m_friendEntry;
            this.m_friendEntry = null;
            return tmp;
        }

        public void SetFriendEntry(FriendEntry entry)
        {
            this.m_friendEntry = entry;
        }
    }
}