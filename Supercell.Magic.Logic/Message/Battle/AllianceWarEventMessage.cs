namespace Supercell.Magic.Logic.Message.Battle
{
    using Supercell.Magic.Logic.Message.Alliance.War;
    using Supercell.Magic.Titan.Message;

    public class AllianceWarEventMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 25006;

        private AllianceWarMemberEntry m_allianceWarMemberEntry;
        private EventType m_eventType;

        public AllianceWarEventMessage() : this(0)
        {
            // AllianceWarEventMessage.
        }

        public AllianceWarEventMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceWarEventMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_allianceWarMemberEntry = new AllianceWarMemberEntry();
            this.m_allianceWarMemberEntry.Decode(this.m_stream);
            this.m_eventType = (EventType) this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_allianceWarMemberEntry.Encode(this.m_stream);
            this.m_stream.WriteInt((int) this.m_eventType);
        }

        public override short GetMessageType()
        {
            return AllianceWarEventMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public enum EventType
        {
            DESTRUCTION_25_PERCENT,
            DESTRUCTION_50_PERCENT,
            DESTRUCTION_75_PERCENT,
            TOWN_HALL_DESTROYED
        }
    }
}