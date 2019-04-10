namespace Supercell.Magic.Logic.Message.Alliance.War
{
    using Supercell.Magic.Logic.Message.Alliance.War.Event;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class AllianceWarFullEntryMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24335;

        private int m_warState;
        private int m_warStateRemainingSeconds;

        private LogicLong m_warId;

        private AllianceWarEntry m_ownAllianceWarEntry;
        private AllianceWarEntry m_enemyAllianceWarEntry;
        private LogicArrayList<WarEventEntry> m_warEventEntryList;

        public AllianceWarFullEntryMessage() : this(0)
        {
            // AllianceWarDataMessage.
        }

        public AllianceWarFullEntryMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceWarFullEntryMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_warState = this.m_stream.ReadInt();
            this.m_warStateRemainingSeconds = this.m_stream.ReadInt();
            this.m_ownAllianceWarEntry = new AllianceWarEntry();
            this.m_ownAllianceWarEntry.Decode(this.m_stream);

            if (this.m_stream.ReadBoolean())
            {
                this.m_enemyAllianceWarEntry = new AllianceWarEntry();
                this.m_enemyAllianceWarEntry.Decode(this.m_stream);
            }

            if (this.m_stream.ReadBoolean())
            {
                this.m_warId = this.m_stream.ReadLong();
            }

            this.m_stream.ReadInt();

            int count = this.m_stream.ReadInt();

            if (count >= 0)
            {
                this.m_warEventEntryList = new LogicArrayList<WarEventEntry>(count);

                for (int i = count - 1; i >= 0; i--)
                {
                    WarEventEntry warEventEntry = WarEventEntryFactory.CreateWarEventEntryByType(this.m_stream.ReadInt());
                    warEventEntry.Decode(this.m_stream);
                    this.m_warEventEntryList.Add(warEventEntry);
                }
            }
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(this.m_warState);
            this.m_stream.WriteInt(this.m_warStateRemainingSeconds);

            this.m_ownAllianceWarEntry.Encode(this.m_stream);

            if (this.m_enemyAllianceWarEntry != null)
            {
                this.m_stream.WriteBoolean(true);
                this.m_enemyAllianceWarEntry.Encode(this.m_stream);
            }
            else
            {
                this.m_stream.WriteBoolean(false);
            }

            this.m_stream.WriteInt(1);

            if (this.m_warEventEntryList != null)
            {
                this.m_stream.WriteInt(this.m_warEventEntryList.Size());

                for (int i = 0; i < this.m_warEventEntryList.Size(); i++)
                {
                    this.m_stream.WriteInt(this.m_warEventEntryList[i].GetWarEventEntryType());
                    this.m_warEventEntryList[i].Encode(this.m_stream);
                }
            }
            else
            {
                this.m_stream.WriteInt(-1);
            }
        }

        public override short GetMessageType()
        {
            return AllianceWarFullEntryMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 25;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public int GetWarState()
        {
            return this.m_warState;
        }

        public void SetWarState(int value)
        {
            this.m_warState = value;
        }

        public int GetWarStateRemainingSeconds()
        {
            return this.m_warStateRemainingSeconds;
        }

        public void SetWarStateRemainingSeconds(int value)
        {
            this.m_warStateRemainingSeconds = value;
        }

        public LogicLong GetWarId()
        {
            return this.m_warId;
        }

        public void SetWarId(LogicLong value)
        {
            this.m_warId = value;
        }

        public AllianceWarEntry GetOwnAllianceWarEntry()
        {
            return this.m_ownAllianceWarEntry;
        }

        public void SetOwnAllianceWarEntry(AllianceWarEntry value)
        {
            this.m_ownAllianceWarEntry = value;
        }

        public AllianceWarEntry GetEnemyAllianceWarEntry()
        {
            return this.m_enemyAllianceWarEntry;
        }

        public void SetEnemyAllianceWarEntry(AllianceWarEntry value)
        {
            this.m_enemyAllianceWarEntry = value;
        }

        public LogicArrayList<WarEventEntry> GetWarEventEntryList()
        {
            return this.m_warEventEntryList;
        }

        public void SetWarEventEntryList(LogicArrayList<WarEventEntry> value)
        {
            this.m_warEventEntryList = value;
        }
    }
}