namespace Supercell.Magic.Logic.Message.Avatar.Attack
{
    using Supercell.Magic.Titan.Message;

    public class Village2AttackEntryUpdateMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24371;

        private Village2AttackEntry m_attackEntry;

        public Village2AttackEntryUpdateMessage() : this(0)
        {
            // Village2AttackEntryUpdateMessage.
        }

        public Village2AttackEntryUpdateMessage(short messageVersion) : base(messageVersion)
        {
            // Village2AttackEntryUpdateMessage.
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
            return Village2AttackEntryUpdateMessage.MESSAGE_TYPE;
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