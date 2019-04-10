namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class Village2AttackStartSpectateMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 15110;
        private LogicLong m_streamId;

        public Village2AttackStartSpectateMessage() : this(0)
        {
            // Village2AttackStartSpectateMessage.
        }

        public Village2AttackStartSpectateMessage(short messageVersion) : base(messageVersion)
        {
            // Village2AttackStartSpectateMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_streamId = this.m_stream.ReadLong();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteLong(this.m_streamId);
        }

        public override short GetMessageType()
        {
            return Village2AttackStartSpectateMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_streamId = null;
        }

        public LogicLong GetStreamId()
        {
            return this.m_streamId;
        }

        public void SetStreamId(LogicLong id)
        {
            this.m_streamId = id;
        }
    }
}