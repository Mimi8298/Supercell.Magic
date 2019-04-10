namespace Supercell.Magic.Logic.Message.Avatar
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class RemoveAllianceBookmarkMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14344;
        private LogicLong m_allianceId;

        public RemoveAllianceBookmarkMessage() : this(0)
        {
            // RemoveAllianceBookmarkMessage.
        }

        public RemoveAllianceBookmarkMessage(short messageVersion) : base(messageVersion)
        {
            // RemoveAllianceBookmarkMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_allianceId = this.m_stream.ReadLong();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteLong(this.m_allianceId);
        }

        public override short GetMessageType()
        {
            return RemoveAllianceBookmarkMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_allianceId = null;
        }

        public LogicLong GetAllianceId()
        {
            return this.m_allianceId;
        }

        public void SetAllianceId(LogicLong value)
        {
            this.m_allianceId = value;
        }
    }
}