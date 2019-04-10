namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class AllianceMemberRemovedMessage : PiranhaMessage
    {
        private LogicLong m_allianceMemberId;

        public AllianceMemberRemovedMessage() : this(0)
        {
            // AllianceMemberRemovedMessage.
        }

        public AllianceMemberRemovedMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceMemberRemovedMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_allianceMemberId = this.m_stream.ReadLong();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteLong(this.m_allianceMemberId);
        }

        public override short GetMessageType()
        {
            return 24309;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_allianceMemberId = null;
        }

        public LogicLong RemoveMemberAvatarId()
        {
            LogicLong tmp = this.m_allianceMemberId;
            this.m_allianceMemberId = null;
            return tmp;
        }

        public void SetMemberAvatarId(LogicLong value)
        {
            this.m_allianceMemberId = value;
        }
    }
}