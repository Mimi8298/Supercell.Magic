namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Titan.Message;

    public class HomeBattleReplayDataMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24114;
        private byte[] m_replayData;

        public HomeBattleReplayDataMessage() : this(0)
        {
            // HomeBattleReplayDataMessage.
        }

        public HomeBattleReplayDataMessage(short messageVersion) : base(messageVersion)
        {
            // HomeBattleReplayDataMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_replayData = this.m_stream.ReadBytes(this.m_stream.ReadBytesLength(), 900000);
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteBytes(this.m_replayData, this.m_replayData.Length);
        }

        public byte[] RemoveReplayData()
        {
            byte[] tmp = this.m_replayData;
            this.m_replayData = null;
            return tmp;
        }

        public void SetReplayData(byte[] data)
        {
            this.m_replayData = data;
        }

        public override short GetMessageType()
        {
            return HomeBattleReplayDataMessage.MESSAGE_TYPE;
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