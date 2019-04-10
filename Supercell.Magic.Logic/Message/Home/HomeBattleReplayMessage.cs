namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class HomeBattleReplayMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14114;
        private int m_replayShardId;
        private LogicLong m_replayId;

        public HomeBattleReplayMessage() : this(0)
        {
            // HomeBattleReplayMessage.
        }

        public HomeBattleReplayMessage(short messageVersion) : base(messageVersion)
        {
            // HomeBattleReplayMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_replayShardId = this.m_stream.ReadInt();
            this.m_replayId = this.m_stream.ReadLong();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(this.m_replayShardId);
            this.m_stream.WriteLong(this.m_replayId);
        }

        public int GetReplayShardId()
        {
            return this.m_replayShardId;
        }

        public void SetReplayShardId(int value)
        {
            this.m_replayShardId = value;
        }

        public LogicLong GetReplayId()
        {
            return this.m_replayId;
        }

        public void SetReplayId(LogicLong value)
        {
            this.m_replayId = value;
        }

        public override short GetMessageType()
        {
            return HomeBattleReplayMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
        }
    }
}