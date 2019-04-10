namespace Supercell.Magic.Logic.Message.Alliance.War
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Util;

    public class AllianceWarEntry
    {
        private AllianceWarHeader m_allianceWarHeader;
        private LogicArrayList<AllianceWarMemberEntry> m_allianceWarMemberList;
        private LogicArrayList<int> m_allianceExpMap;

        private bool m_ended;

        private int m_warState;
        private int m_warStateRemainingSeconds;
        private int m_allianceWarLootBonusPecentWin;
        private int m_allianceWarLootBonusPecentDraw;
        private int m_allianceWarLootBonusPecentLose;

        public void Decode(ByteStream stream)
        {
            this.m_allianceWarHeader = new AllianceWarHeader();
            this.m_allianceWarHeader.Decode(stream);

            int memberCount = stream.ReadInt();

            if (memberCount >= 0)
            {
                Debugger.DoAssert(memberCount < 1000, "Too many alliance war member entries in AllianceWarEntry");

                this.m_allianceWarMemberList = new LogicArrayList<AllianceWarMemberEntry>();
                this.m_allianceWarMemberList.EnsureCapacity(memberCount);

                for (int i = 0; i < memberCount; i++)
                {
                    AllianceWarMemberEntry allianceWarMemberEntry = new AllianceWarMemberEntry();
                    allianceWarMemberEntry.Decode(stream);
                    this.m_allianceWarMemberList.Add(allianceWarMemberEntry);
                }
            }

            this.m_ended = stream.ReadBoolean();
            stream.ReadBoolean();

            int expMapCount = stream.ReadInt();

            if (expMapCount >= 0)
            {
                Debugger.DoAssert(expMapCount <= 50, "Too many entries in the alliance exp map");

                this.m_allianceExpMap = new LogicArrayList<int>();
                this.m_allianceExpMap.EnsureCapacity(expMapCount);

                for (int i = 0; i < expMapCount; i++)
                {
                    this.m_allianceExpMap.Add(stream.ReadInt());
                }
            }

            this.m_warState = stream.ReadInt();
            this.m_warStateRemainingSeconds = stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            this.m_allianceWarLootBonusPecentWin = stream.ReadInt();
            this.m_allianceWarLootBonusPecentDraw = stream.ReadInt();
            this.m_allianceWarLootBonusPecentLose = stream.ReadInt();
        }

        public void Encode(ByteStream encoder)
        {
            this.m_allianceWarHeader.Encode(encoder);

            if (this.m_allianceWarMemberList != null)
            {
                encoder.WriteInt(this.m_allianceWarMemberList.Size());

                for (int i = 0; i < this.m_allianceWarMemberList.Size(); i++)
                {
                    this.m_allianceWarMemberList[i].Encode(encoder);
                }
            }
            else
            {
                encoder.WriteInt(-1);
            }

            encoder.WriteBoolean(this.m_ended);
            encoder.WriteBoolean(true);

            if (this.m_allianceExpMap != null)
            {
                encoder.WriteInt(this.m_allianceExpMap.Size());

                for (int i = 0; i < this.m_allianceExpMap.Size(); i++)
                {
                    encoder.WriteInt(this.m_allianceExpMap[i]);
                }
            }
            else
            {
                encoder.WriteInt(-1);
            }

            encoder.WriteInt(this.m_warState);
            encoder.WriteInt(0); // remSecs ?
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(this.m_allianceWarLootBonusPecentWin);
            encoder.WriteInt(this.m_allianceWarLootBonusPecentDraw);
            encoder.WriteInt(this.m_allianceWarLootBonusPecentLose);
        }

        public AllianceWarHeader GetAllianceWarHeader()
        {
            return this.m_allianceWarHeader;
        }

        public void SetAllianceWarHeader(AllianceWarHeader value)
        {
            this.m_allianceWarHeader = value;
        }

        public LogicArrayList<AllianceWarMemberEntry> GetAllianceWarMemberList()
        {
            return this.m_allianceWarMemberList;
        }

        public int GetAllianceWarMemberCount()
        {
            return this.m_allianceWarMemberList.Size();
        }

        public void SetAllianceWarMemberList(LogicArrayList<AllianceWarMemberEntry> value)
        {
            this.m_allianceWarMemberList = value;
        }

        public LogicArrayList<int> GetAllianceExpMap()
        {
            return this.m_allianceExpMap;
        }

        public void SetAllianceExpMap(LogicArrayList<int> value)
        {
            this.m_allianceExpMap = value;
        }

        public bool IsEnded()
        {
            return this.m_ended;
        }

        public void SetEnded(bool value)
        {
            this.m_ended = value;
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

        public int GetAllianceWarLootBonusPecentWin()
        {
            return this.m_allianceWarLootBonusPecentWin;
        }

        public void SetAllianceWarLootBonusPecentWin(int value)
        {
            this.m_allianceWarLootBonusPecentWin = value;
        }

        public int GetAllianceWarLootBonusPecentDraw()
        {
            return this.m_allianceWarLootBonusPecentDraw;
        }

        public void SetAllianceWarLootBonusPecentDraw(int value)
        {
            this.m_allianceWarLootBonusPecentDraw = value;
        }

        public int GetAllianceWarLootBonusPecentLose()
        {
            return this.m_allianceWarLootBonusPecentLose;
        }

        public void SetAllianceWarLootBonusPecentLose(int value)
        {
            this.m_allianceWarLootBonusPecentLose = value;
        }
    }
}