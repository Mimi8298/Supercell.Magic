namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class ChangeAllianceMemberRoleMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14306;

        private LogicLong m_memberId;
        private LogicAvatarAllianceRole m_memberRole;

        public ChangeAllianceMemberRoleMessage() : this(0)
        {
            // ChangeAllianceMemberRoleMessage.
        }

        public ChangeAllianceMemberRoleMessage(short messageVersion) : base(messageVersion)
        {
            // ChangeAllianceMemberRoleMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_memberId = this.m_stream.ReadLong();
            this.m_memberRole = (LogicAvatarAllianceRole) this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteLong(this.m_memberId);
            this.m_stream.WriteInt((int) this.m_memberRole);
        }

        public override short GetMessageType()
        {
            return ChangeAllianceMemberRoleMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public LogicLong RemoveMemberId()
        {
            LogicLong tmp = this.m_memberId;
            this.m_memberId = null;
            return tmp;
        }

        public LogicAvatarAllianceRole GetMemberRole()
        {
            return this.m_memberRole;
        }

        public void SetAllianceData(LogicLong memberId, LogicAvatarAllianceRole memberRole)
        {
            this.m_memberId = memberId;
            this.m_memberRole = memberRole;
        }
    }
}