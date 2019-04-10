namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class DonateAllianceUnitMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14310;

        private LogicCombatItemData m_unitData;
        private LogicLong m_streamId;

        private bool m_quickDonate;

        public DonateAllianceUnitMessage() : this(0)
        {
            // DonateAllianceUnitMessage.
        }

        public DonateAllianceUnitMessage(short messageVersion) : base(messageVersion)
        {
            // DonateAllianceUnitMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_unitData = (LogicCombatItemData) ByteStreamHelper.ReadDataReference(this.m_stream,
                                                                                       this.m_stream.ReadInt() != 0 ? LogicDataType.SPELL : LogicDataType.CHARACTER);
            this.m_streamId = this.m_stream.ReadLong();
            this.m_quickDonate = this.m_stream.ReadBoolean();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(this.m_unitData.GetCombatItemType());
            ByteStreamHelper.WriteDataReference(this.m_stream, this.m_unitData);
            this.m_stream.WriteBoolean(this.m_quickDonate);
        }

        public override short GetMessageType()
        {
            return DonateAllianceUnitMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_unitData = null;
            this.m_streamId = null;
        }

        public LogicCombatItemData GetAllianceUnitData()
        {
            return this.m_unitData;
        }

        public void SetAllianceUnitData(LogicCombatItemData data)
        {
            this.m_unitData = data;
        }

        public LogicLong GetStreamId()
        {
            return this.m_streamId;
        }

        public void SetStreamId(LogicLong id)
        {
            this.m_streamId = id;
        }

        public bool UseQuickDonate()
        {
            return this.m_quickDonate;
        }

        public void SetQuickDonate(bool enabled)
        {
            this.m_quickDonate = enabled;
        }
    }
}