namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Titan.Message;

    public class AttackSpectatorCountMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24125;

        private int m_viewerCount;
        private int m_enemyViewerCount;

        public AttackSpectatorCountMessage() : this(0)
        {
            // AttackSpectatorCountMessage.
        }

        public AttackSpectatorCountMessage(short messageVersion) : base(messageVersion)
        {
            // AttackSpectatorCountMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_viewerCount = this.m_stream.ReadInt();
            this.m_enemyViewerCount = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(this.m_viewerCount);
            this.m_stream.WriteInt(this.m_enemyViewerCount);
        }

        public override short GetMessageType()
        {
            return AttackSpectatorCountMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public void SetViewerCount(int value)
        {
            this.m_viewerCount = value;
        }

        public int GetViewerCount()
        {
            return this.m_viewerCount;
        }

        public void SetEnemyViewerCount(int value)
        {
            this.m_enemyViewerCount = value;
        }

        public int GetEnemyViewerCount()
        {
            return this.m_enemyViewerCount;
        }
    }
}