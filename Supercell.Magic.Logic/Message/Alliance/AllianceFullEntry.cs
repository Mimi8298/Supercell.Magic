namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Util;

    public class AllianceFullEntry
    {
        private string m_description;

        private AllianceHeaderEntry m_allianceHeaderEntry;
        private LogicArrayList<AllianceMemberEntry> m_allianceMemberList;

        public void Decode(ByteStream stream)
        {
            this.m_allianceHeaderEntry = new AllianceHeaderEntry();
            this.m_allianceHeaderEntry.Decode(stream);
            this.m_description = stream.ReadString(900000);

            stream.ReadInt();

            if (stream.ReadBoolean())
            {
                stream.ReadLong();
            }

            stream.ReadInt();

            if (stream.ReadBoolean())
            {
                stream.ReadLong();
            }

            int memberCount = stream.ReadInt();

            if (memberCount >= 0)
            {
                Debugger.DoAssert(memberCount < 51, "Too many members in the alliance");

                this.m_allianceMemberList = new LogicArrayList<AllianceMemberEntry>();
                this.m_allianceMemberList.EnsureCapacity(memberCount);

                int idx = 0;

                do
                {
                    AllianceMemberEntry allianceMemberEntry = new AllianceMemberEntry();
                    allianceMemberEntry.Decode(stream);
                    this.m_allianceMemberList.Add(allianceMemberEntry);
                } while (++idx != memberCount);
            }

            stream.ReadInt();
            stream.ReadInt();
        }

        public void Encode(ByteStream stream)
        {
            this.m_allianceHeaderEntry.Encode(stream);
            stream.WriteString(this.m_description);

            stream.WriteInt(0);
            stream.WriteBoolean(false);
            stream.WriteInt(0);
            stream.WriteBoolean(false);

            if (this.m_allianceMemberList != null)
            {
                stream.WriteInt(this.m_allianceMemberList.Size());

                for (int i = 0; i < this.m_allianceMemberList.Size(); i++)
                {
                    this.m_allianceMemberList[i].Encode(stream);
                }
            }
            else
            {
                stream.WriteInt(-1);
            }

            stream.WriteInt(0);
            stream.WriteInt(0);
        }

        public void SetAllianceHeaderEntry(AllianceHeaderEntry entry)
        {
            this.m_allianceHeaderEntry = entry;
        }

        public void SetAllianceDescription(string description)
        {
            this.m_description = description;
        }

        public void SetAllianceMembers(LogicArrayList<AllianceMemberEntry> entry)
        {
            this.m_allianceMemberList = entry;
        }
    }
}