namespace Supercell.Magic.Logic.Message.Alliance.War.Event
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class WarEventEntry
    {
        public const int WAR_EVENT_ENTRY_TYPE_ATTACK = 1;

        private LogicLong m_id;
        private int m_ageSeconds;

        public WarEventEntry()
        {
        }

        public virtual void Encode(ByteStream stream)
        {
            stream.WriteLong(this.m_id);
            stream.WriteInt(this.m_ageSeconds);
        }

        public virtual void Decode(ByteStream stream)
        {
            this.m_id = stream.ReadLong();
            this.m_ageSeconds = stream.ReadInt();
        }

        public virtual int GetWarEventEntryType()
        {
            return -1;
        }
    }
}