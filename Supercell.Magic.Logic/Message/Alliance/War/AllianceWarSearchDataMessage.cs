namespace Supercell.Magic.Logic.Message.Alliance.War
{
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class AllianceWarSearchDataMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24325;
        private LogicArrayList<AllianceWarMemberEntry> m_warMemberEntryList;

        public AllianceWarSearchDataMessage() : this(0)
        {
            // AllianceWarSearchDataMessage.
        }

        public AllianceWarSearchDataMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceWarSearchDataMessage.
        }

        public override void Decode()
        {
            base.Decode();

            int count = this.m_stream.ReadInt();

            if (count >= 0)
            {
                this.m_warMemberEntryList = new LogicArrayList<AllianceWarMemberEntry>();
                this.m_warMemberEntryList.EnsureCapacity(count);

                for (int i = 0; i < count; i++)
                {
                    AllianceWarMemberEntry warMemberEntry = new AllianceWarMemberEntry();
                    warMemberEntry.Decode(this.m_stream);
                    this.m_warMemberEntryList.Add(warMemberEntry);
                }
            }
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteInt(this.m_warMemberEntryList.Size());

            for (int i = 0; i < this.m_warMemberEntryList.Size(); i++)
            {
                this.m_warMemberEntryList[i].Encode(this.m_stream);
            }
        }

        public override short GetMessageType()
        {
            return AllianceWarSearchDataMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 25;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_warMemberEntryList = null;
        }

        public LogicArrayList<AllianceWarMemberEntry> GetWarMemberEntryList()
        {
            return this.m_warMemberEntryList;
        }

        public void SetWarMemberEntryList(LogicArrayList<AllianceWarMemberEntry> value)
        {
            this.m_warMemberEntryList = value;
        }
    }
}