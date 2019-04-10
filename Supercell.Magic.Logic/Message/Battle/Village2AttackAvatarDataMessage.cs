namespace Supercell.Magic.Logic.Message.Battle
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Home;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class Village2AttackAvatarDataMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 25023;

        private LogicClientAvatar m_logicClientAvatar;
        private LogicClientHome m_logicClientHome;

        private LogicLong m_enemyId;

        private int m_timestamp;

        public Village2AttackAvatarDataMessage() : this(0)
        {
            // Village2AttackAvatarDataMessage.
        }

        public Village2AttackAvatarDataMessage(short messageVersion) : base(messageVersion)
        {
            // Village2AttackAvatarDataMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_logicClientAvatar = new LogicClientAvatar();
            this.m_logicClientAvatar.Decode(this.m_stream);
            this.m_logicClientHome = new LogicClientHome();
            this.m_logicClientHome.Decode(this.m_stream);
            this.m_enemyId = this.m_stream.ReadLong();
            this.m_timestamp = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_logicClientAvatar.Encode(this.m_stream);
            this.m_logicClientHome.Encode(this.m_stream);

            this.m_stream.WriteLong(new LogicLong(0, 1));
            this.m_stream.WriteInt(this.m_timestamp);
        }

        public override short GetMessageType()
        {
            return Village2AttackAvatarDataMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 27;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public LogicClientAvatar GetLogicClientAvatar()
        {
            return this.m_logicClientAvatar;
        }

        public void SetLogicClientAvatar(LogicClientAvatar value)
        {
            this.m_logicClientAvatar = value;
        }

        public LogicClientHome GetLogicClientHome()
        {
            return this.m_logicClientHome;
        }

        public void SetLogicClientHome(LogicClientHome value)
        {
            this.m_logicClientHome = value;
        }

        public LogicLong GetEnemyId()
        {
            return this.m_enemyId;
        }

        public void SetEnemyId(LogicLong value)
        {
            this.m_enemyId = value;
        }

        public int GetTimestamp()
        {
            return this.m_timestamp;
        }

        public void SetTimestamp(int value)
        {
            this.m_timestamp = value;
        }
    }
}