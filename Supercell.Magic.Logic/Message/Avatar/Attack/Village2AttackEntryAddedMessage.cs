namespace Supercell.Magic.Logic.Message.Avatar.Attack
{
    using Supercell.Magic.Titan.Message;

    public class Village2AttackEntryAddedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24372;

        private Village2AttackEntry m_attackEntry;

        public Village2AttackEntryAddedMessage() : this(0)
        {
            // Village2AttackEntryAddedMessage.
        }

        public Village2AttackEntryAddedMessage(short messageVersion) : base(messageVersion)
        {
            // Village2AttackEntryAddedMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_attackEntry = Village2AttackEntryFactory.CreateAttackEntryByType(this.m_stream.ReadInt());
            this.m_attackEntry?.Decode(this.m_stream);
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(this.m_attackEntry.GetAttackEntryType());
            this.m_attackEntry.Encode(this.m_stream);
        }

        public override short GetMessageType()
        {
            return Village2AttackEntryAddedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_attackEntry = null;
        }

        public Village2AttackEntry RemoveAttackEntry()
        {
            Village2AttackEntry tmp = this.m_attackEntry;
            this.m_attackEntry = null;
            return tmp;
        }

        public void SetAttackEntry(Village2AttackEntry entry)
        {
            this.m_attackEntry = entry;
        }
    }
}