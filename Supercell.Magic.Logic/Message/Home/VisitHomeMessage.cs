namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class VisitHomeMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14113;

        private LogicLong m_homeId;
        private int m_villageType;

        public VisitHomeMessage() : this(0)
        {
            // VisitHomeMessage.
        }

        public VisitHomeMessage(short messageVersion) : base(messageVersion)
        {
            // VisitHomeMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_homeId = this.m_stream.ReadLong();
            this.m_villageType = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteLong(this.m_homeId);
            this.m_stream.WriteInt(this.m_villageType);
        }

        public override short GetMessageType()
        {
            return VisitHomeMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 10;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_homeId = null;
        }

        public int GetVillageType()
        {
            return this.m_villageType;
        }

        public void SetVillageType(int villageType)
        {
            this.m_villageType = villageType;
        }

        public LogicLong RemoveHomeId()
        {
            LogicLong tmp = this.m_homeId;
            this.m_homeId = null;
            return tmp;
        }

        public void SetHomeId(LogicLong id)
        {
            this.m_homeId = id;
        }
    }
}