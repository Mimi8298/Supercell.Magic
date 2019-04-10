namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class ScoutFriendlyBattleMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14111;
        private LogicLong m_streamId;

        public ScoutFriendlyBattleMessage() : this(0)
        {
            // GoHomeMessage.
        }

        public ScoutFriendlyBattleMessage(short messageVersion) : base(messageVersion)
        {
            // GoHomeMessage.
        }

        public override void Decode()
        {
            this.m_streamId = this.m_stream.ReadLong();
        }

        public override void Encode()
        {
            this.m_stream.WriteLong(this.m_streamId);
        }

        public LogicLong GetStreamId()
        {
            return this.m_streamId;
        }

        public void SetStreamId(LogicLong id)
        {
            this.m_streamId = id;
        }

        public override short GetMessageType()
        {
            return ScoutFriendlyBattleMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();
        }
    }
}