namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.Message;

    public class AttackNpcMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14134;

        private LogicNpcData m_npcData;

        public AttackNpcMessage() : this(0)
        {
            // AttackNpcMessage.
        }

        public AttackNpcMessage(short messageVersion) : base(messageVersion)
        {
            // AttackNpcMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_npcData = (LogicNpcData) ByteStreamHelper.ReadDataReference(this.m_stream, LogicDataType.NPC);
        }

        public override void Encode()
        {
            base.Encode();
            ByteStreamHelper.WriteDataReference(this.m_stream, this.m_npcData);
        }

        public override short GetMessageType()
        {
            return AttackNpcMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 10;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_npcData = null;
        }

        public LogicNpcData GetNpcData()
        {
            return this.m_npcData;
        }

        public void SetNpcData(LogicNpcData data)
        {
            this.m_npcData = data;
        }
    }
}