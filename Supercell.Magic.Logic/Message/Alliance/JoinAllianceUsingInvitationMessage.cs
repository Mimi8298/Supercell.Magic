namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class JoinAllianceUsingInvitationMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14323;

        private LogicLong m_avatarStreamEntryId;

        public JoinAllianceUsingInvitationMessage() : this(0)
        {
            // JoinAllianceUsingInvitationMessage.
        }

        public JoinAllianceUsingInvitationMessage(short messageVersion) : base(messageVersion)
        {
            // JoinAllianceUsingInvitationMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_avatarStreamEntryId = this.m_stream.ReadLong();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteLong(this.m_avatarStreamEntryId);
        }

        public override short GetMessageType()
        {
            return JoinAllianceUsingInvitationMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public LogicLong GetAvatarStreamEntryId()
        {
            return this.m_avatarStreamEntryId;
        }

        public void SetAvatarStreamEntryId(LogicLong value)
        {
            this.m_avatarStreamEntryId = value;
        }
    }
}