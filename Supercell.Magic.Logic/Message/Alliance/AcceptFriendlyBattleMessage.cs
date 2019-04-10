namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class AcceptFriendlyBattleMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14120;

        private LogicLong m_streamId;
        private int m_layoutId;

        public AcceptFriendlyBattleMessage() : this(0)
        {
            // AcceptFriendlyBattleMessage.
        }

        public AcceptFriendlyBattleMessage(short messageVersion) : base(messageVersion)
        {
            // AcceptFriendlyBattleMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_streamId = this.m_stream.ReadLong();
            this.m_layoutId = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteLong(this.m_streamId);
            this.m_stream.WriteInt(this.m_layoutId);
        }

        public override short GetMessageType()
        {
            return AcceptFriendlyBattleMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public LogicLong GetStreamId()
        {
            return this.m_streamId;
        }

        public void SetStreamId(LogicLong value)
        {
            this.m_streamId = value;
        }

        public int GetLayoutId()
        {
            return this.m_layoutId;
        }

        public void SetLayoutId(int value)
        {
            this.m_layoutId = value;
        }
    }
}