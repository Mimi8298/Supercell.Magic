namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Titan.Message;

    public class HomeBattleReplayFailedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24116;
        private Reason m_reason;

        public HomeBattleReplayFailedMessage() : this(0)
        {
            // HomeBattleReplayFailedMessage.
        }

        public HomeBattleReplayFailedMessage(short messageVersion) : base(messageVersion)
        {
            // HomeBattleReplayFailedMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_reason = (Reason) this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteInt((int) this.m_reason);
        }

        public override short GetMessageType()
        {
            return HomeBattleReplayFailedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public enum Reason
        {
        }
    }
}