namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class SendAllianceInvitationMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14322;

        private LogicLong m_avatarId;

        public SendAllianceInvitationMessage() : this(0)
        {
            // SendAllianceInvitationMessage.
        }

        public SendAllianceInvitationMessage(short messageVersion) : base(messageVersion)
        {
            // SendAllianceInvitationMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_avatarId = this.m_stream.ReadLong();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteLong(this.m_avatarId);
        }

        public override short GetMessageType()
        {
            return SendAllianceInvitationMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public LogicLong GetAvatarId()
        {
            return this.m_avatarId;
        }

        public void SetAvatarId(LogicLong id)
        {
            this.m_avatarId = id;
        }
    }
}