namespace Supercell.Magic.Logic.Message.Alliance.War
{
    using Supercell.Magic.Titan.Message;

    public class AllianceWarDataMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24329;
        private AllianceWarEntry m_allianceWarEntry;

        public AllianceWarDataMessage() : this(0)
        {
            // AllianceWarDataMessage.
        }

        public AllianceWarDataMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceWarDataMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_allianceWarEntry = new AllianceWarEntry();
            this.m_allianceWarEntry.Decode(this.m_stream);
        }

        public override void Encode()
        {
            base.Encode();
            this.m_allianceWarEntry.Encode(this.m_stream);
        }

        public override short GetMessageType()
        {
            return AllianceWarDataMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public AllianceWarEntry GetAllianceWarEntry()
        {
            return this.m_allianceWarEntry;
        }

        public void SetAllianceWarEntry(AllianceWarEntry list)
        {
            this.m_allianceWarEntry = list;
        }
    }
}