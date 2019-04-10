namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Home;
    using Supercell.Magic.Titan.Message;

    public class NpcDataMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24133;

        private LogicClientHome m_clientHome;
        private LogicNpcAvatar m_npcAvatar;
        private LogicClientAvatar m_clientAvatar;

        private int m_secondsSinceLastSave;
        private int m_currentTimestamp;
        private bool m_npcDuel;

        public NpcDataMessage() : this(0)
        {
            // NpcDataMessage.
        }

        public NpcDataMessage(short messageVersion) : base(messageVersion)
        {
            // NpcDataMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_secondsSinceLastSave = this.m_stream.ReadInt();
            this.m_currentTimestamp = this.m_stream.ReadInt();
            this.m_clientHome = new LogicClientHome();
            this.m_clientHome.Decode(this.m_stream);
            this.m_clientAvatar = new LogicClientAvatar();
            this.m_clientAvatar.Decode(this.m_stream);
            this.m_npcAvatar = new LogicNpcAvatar();
            this.m_npcAvatar.Decode(this.m_stream);
            this.m_npcDuel = this.m_stream.ReadBoolean();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(this.m_secondsSinceLastSave);
            this.m_stream.WriteInt(this.m_currentTimestamp);
            this.m_clientHome.Encode(this.m_stream);
            this.m_clientAvatar.Encode(this.m_stream);
            this.m_npcAvatar.Encode(this.m_stream);
            this.m_stream.WriteBoolean(this.m_npcDuel);
        }

        public override short GetMessageType()
        {
            return NpcDataMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 10;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_clientHome = null;
            this.m_clientAvatar = null;
            this.m_npcAvatar = null;
        }

        public LogicClientHome RemoveLogicClientHome()
        {
            LogicClientHome tmp = this.m_clientHome;
            this.m_clientHome = null;
            return tmp;
        }

        public void SetLogicClientHome(LogicClientHome instance)
        {
            this.m_clientHome = instance;
        }

        public LogicClientAvatar RemoveLogicClientAvatar()
        {
            LogicClientAvatar tmp = this.m_clientAvatar;
            this.m_clientAvatar = null;
            return tmp;
        }

        public void SetLogicClientAvatar(LogicClientAvatar instance)
        {
            this.m_clientAvatar = instance;
        }

        public LogicNpcAvatar RemoveLogicNpcAvatar()
        {
            LogicNpcAvatar tmp = this.m_npcAvatar;
            this.m_npcAvatar = null;
            return tmp;
        }

        public void SetLogicNpcAvatar(LogicNpcAvatar instance)
        {
            this.m_npcAvatar = instance;
        }

        public int GetCurrentTimestamp()
        {
            return this.m_currentTimestamp;
        }

        public void SetCurrentTimestamp(int value)
        {
            this.m_currentTimestamp = value;
        }

        public int GetSecondsSinceLastSave()
        {
            return this.m_secondsSinceLastSave;
        }

        public void SetSecondsSinceLastSave(int value)
        {
            this.m_secondsSinceLastSave = value;
        }

        public bool IsNpcDuel()
        {
            return this.m_npcDuel;
        }

        public void SetNpcDuel(bool npcDuel)
        {
            this.m_npcDuel = npcDuel;
        }
    }
}