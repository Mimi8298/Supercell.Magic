namespace Supercell.Magic.Logic.Message.League
{
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class LeagueMemberListMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24503;

        private int m_remainingSeasonTime;
        private LogicArrayList<LeagueMemberEntry> m_memberList;

        public LeagueMemberListMessage() : this(0)
        {
            // LeagueMemberListMessage.
        }

        public LeagueMemberListMessage(short messageVersion) : base(messageVersion)
        {
            // LeagueMemberListMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_remainingSeasonTime = this.m_stream.ReadInt();
            int arraySize = this.m_stream.ReadInt();

            if (arraySize > -1)
            {
                this.m_memberList = new LogicArrayList<LeagueMemberEntry>(arraySize);

                for (int i = 0; i < arraySize; i++)
                {
                    LeagueMemberEntry leagueMemberEntry = new LeagueMemberEntry();
                    leagueMemberEntry.Decode(this.m_stream);
                    this.m_memberList.Add(leagueMemberEntry);
                }
            }
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(this.m_remainingSeasonTime);

            if (this.m_memberList != null)
            {
                this.m_stream.WriteInt(this.m_memberList.Size());

                for (int i = 0; i < this.m_memberList.Size(); i++)
                {
                    this.m_memberList[i].Encode(this.m_stream);
                }
            }
            else
            {
                this.m_stream.WriteInt(-1);
            }
        }

        public override short GetMessageType()
        {
            return LeagueMemberListMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 13;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_memberList = null;
        }

        public LogicArrayList<LeagueMemberEntry> GetMemberList()
        {
            return this.m_memberList;
        }

        public void SetMemberList(LogicArrayList<LeagueMemberEntry> entry)
        {
            this.m_memberList = entry;
        }

        public int GetRemainingSeasonTime()
        {
            return this.m_remainingSeasonTime;
        }

        public void SetRemainingSeasonTime(int value)
        {
            this.m_remainingSeasonTime = value;
        }
    }
}