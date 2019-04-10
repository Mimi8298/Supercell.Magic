namespace Supercell.Magic.Logic.Message.Alliance.War
{
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class AllianceWarHistoryMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24338;
        private LogicArrayList<AllianceWarHistoryEntry> m_allianceWarHistoryList;

        public AllianceWarHistoryMessage() : this(0)
        {
            // AllianceWarHistoryMessage.
        }

        public AllianceWarHistoryMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceWarHistoryMessage.
        }

        public override void Decode()
        {
            base.Decode();

            int count = this.m_stream.ReadInt();

            if (count >= 0)
            {
                Debugger.DoAssert(count < 1000, "Too many entries for alliance war history message");
                this.m_allianceWarHistoryList = new LogicArrayList<AllianceWarHistoryEntry>(count);

                for (int i = 0; i < count; i++)
                {
                    AllianceWarHistoryEntry allianceWarHistoryEntry = new AllianceWarHistoryEntry();
                    allianceWarHistoryEntry.Decode(this.m_stream);
                    this.m_allianceWarHistoryList.Add(allianceWarHistoryEntry);
                }
            }
        }

        public override void Encode()
        {
            base.Encode();

            if (this.m_allianceWarHistoryList != null)
            {
                this.m_stream.WriteInt(this.m_allianceWarHistoryList.Size());

                for (int i = 0; i < this.m_allianceWarHistoryList.Size(); i++)
                {
                    this.m_allianceWarHistoryList[i].Encode(this.m_stream);
                }
            }
            else
            {
                this.m_stream.WriteInt(-1);
            }
        }

        public override short GetMessageType()
        {
            return AllianceWarDataFailedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public LogicArrayList<AllianceWarHistoryEntry> GetAllianceWarHistoryList()
        {
            return this.m_allianceWarHistoryList;
        }

        public void SetAllianceWarHistoryList(LogicArrayList<AllianceWarHistoryEntry> list)
        {
            this.m_allianceWarHistoryList = list;
        }
    }
}