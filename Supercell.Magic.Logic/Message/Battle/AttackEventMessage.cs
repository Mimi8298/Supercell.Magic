namespace Supercell.Magic.Logic.Message.Battle
{
    using Supercell.Magic.Titan.Message;

    public class AttackEventMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 25027;

        private EventType m_eventType;
        private int m_stars;

        public AttackEventMessage() : this(0)
        {
            // AttackEventMessage.
        }

        public AttackEventMessage(short messageVersion) : base(messageVersion)
        {
            // AttackEventMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_eventType = (EventType) this.m_stream.ReadInt();
            this.m_stars = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt((int) this.m_eventType);
            this.m_stream.WriteInt(this.m_stars);
        }

        public override short GetMessageType()
        {
            return AttackEventMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public EventType GetEventType()
        {
            return this.m_eventType;
        }

        public void SetEventType(EventType value)
        {
            this.m_eventType = value;
        }

        public int GetStars()
        {
            return this.m_stars;
        }

        public void SetStars(int value)
        {
            this.m_stars = value;
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