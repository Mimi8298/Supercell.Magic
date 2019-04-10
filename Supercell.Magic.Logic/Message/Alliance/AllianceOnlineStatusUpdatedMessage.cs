namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Message;

    public class AllianceOnlineStatusUpdatedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20207;

        private int m_memberCount;
        private int m_onlineMemberCount;

        public AllianceOnlineStatusUpdatedMessage() : this(0)
        {
            // AllianceOnlineStatusUpdatedMessage.
        }

        public AllianceOnlineStatusUpdatedMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceOnlineStatusUpdatedMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_onlineMemberCount = this.m_stream.ReadVInt();
            this.m_memberCount = this.m_stream.ReadVInt();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteVInt(this.m_onlineMemberCount);
            this.m_stream.WriteVInt(this.m_memberCount);
        }

        public override short GetMessageType()
        {
            return AllianceOnlineStatusUpdatedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public int GetMembersOnline()
        {
            return this.m_onlineMemberCount;
        }

        public void SetMembersOnline(int value)
        {
            this.m_onlineMemberCount = value;
        }

        public int GetMembersCount()
        {
            return this.m_memberCount;
        }

        public void SetMembersCount(int value)
        {
            this.m_memberCount = value;
        }
    }
}