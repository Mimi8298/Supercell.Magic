namespace Supercell.Magic.Logic.Message.Avatar.Attack
{
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class Village2AttackEntryListMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24370;

        private bool m_targetList;
        private LogicArrayList<Village2AttackEntry> m_attackEntryList;

        public Village2AttackEntryListMessage() : this(0)
        {
            // Village2AttackEntryListMessage.
        }

        public Village2AttackEntryListMessage(short messageVersion) : base(messageVersion)
        {
            // Village2AttackEntryListMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_targetList = this.m_stream.ReadBoolean();
            int cnt = this.m_stream.ReadInt();

            if (cnt != -1)
            {
                this.m_attackEntryList = new LogicArrayList<Village2AttackEntry>(cnt);

                for (int i = 0; i < cnt; i++)
                {
                    Village2AttackEntry entry = Village2AttackEntryFactory.CreateAttackEntryByType(this.m_stream.ReadInt());
                    entry.Decode(this.m_stream);
                    this.m_attackEntryList.Add(entry);
                }
            }
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteBoolean(this.m_targetList);

            if (this.m_attackEntryList != null)
            {
                this.m_stream.WriteInt(this.m_attackEntryList.Size());

                for (int i = 0; i < this.m_attackEntryList.Size(); i++)
                {
                    this.m_stream.WriteInt(this.m_attackEntryList[i].GetAttackEntryType());
                    this.m_attackEntryList[i].Encode(this.m_stream);
                }
            }
            else
            {
                this.m_stream.WriteInt(-1);
            }
        }

        public override short GetMessageType()
        {
            return Village2AttackEntryListMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_attackEntryList = null;
        }

        public LogicArrayList<Village2AttackEntry> RemoveStreamEntries()
        {
            LogicArrayList<Village2AttackEntry> tmp = this.m_attackEntryList;
            this.m_attackEntryList = null;
            return tmp;
        }

        public void SetStreamEntries(LogicArrayList<Village2AttackEntry> entry)
        {
            this.m_attackEntryList = entry;
        }
    }
}