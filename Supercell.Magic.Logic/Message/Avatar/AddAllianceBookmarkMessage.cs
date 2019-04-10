namespace Supercell.Magic.Logic.Message.Avatar
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class AddAllianceBookmarkMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14343;
        private LogicLong m_allianceId;

        public AddAllianceBookmarkMessage() : this(0)
        {
            // BookmarksListMessage.
        }

        public AddAllianceBookmarkMessage(short messageVersion) : base(messageVersion)
        {
            // AddAllianceBookmarkMessage.
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
            return AddAllianceBookmarkMessage.MESSAGE_TYPE;
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