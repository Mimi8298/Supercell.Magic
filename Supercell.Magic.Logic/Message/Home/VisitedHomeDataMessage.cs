namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Home;
    using Supercell.Magic.Titan.Message;

    public class VisitedHomeDataMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24113;

        private int m_currentTimestamp;
        private int m_secondsSinceLastSave;

        private LogicClientAvatar m_visitorLogicClientAvatar;
        private LogicClientAvatar m_ownerLogicClientAvatar;
        private LogicClientHome m_logicClientHome;

        public VisitedHomeDataMessage() : this(0)
        {
            // VisitedHomeDataMessage.
        }

        public VisitedHomeDataMessage(short messageVersion) : base(messageVersion)
        {
            // VisitedHomeDataMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_secondsSinceLastSave = this.m_stream.ReadInt();
            this.m_currentTimestamp = this.m_stream.ReadInt();

            this.m_logicClientHome = new LogicClientHome();
            this.m_logicClientHome.Decode(this.m_stream);

            this.m_ownerLogicClientAvatar = new LogicClientAvatar();
            this.m_ownerLogicClientAvatar.Decode(this.m_stream);

            this.m_stream.ReadInt();

            if (this.m_stream.ReadBoolean())
            {
                this.m_visitorLogicClientAvatar = new LogicClientAvatar();
                this.m_visitorLogicClientAvatar.Decode(this.m_stream);
            }
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(this.m_secondsSinceLastSave);
            this.m_stream.WriteInt(this.m_currentTimestamp);

            this.m_logicClientHome.Encode(this.m_stream);
            this.m_ownerLogicClientAvatar.Encode(this.m_stream);
            this.m_stream.WriteInt(0);

            if (this.m_visitorLogicClientAvatar != null)
            {
                this.m_stream.WriteBoolean(true);
                this.m_visitorLogicClientAvatar.Encode(this.m_stream);
            }
            else
            {
                this.m_stream.WriteBoolean(false);
            }
        }

        public override short GetMessageType()
        {
            return VisitedHomeDataMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 10;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_logicClientHome = null;
            this.m_ownerLogicClientAvatar = null;
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

        public LogicClientHome RemoveLogicClientHome()
        {
            LogicClientHome tmp = this.m_logicClientHome;
            this.m_logicClientHome = null;
            return tmp;
        }

        public void SetLogicClientHome(LogicClientHome logicClientHome)
        {
            this.m_logicClientHome = logicClientHome;
        }

        public LogicClientAvatar RemoveVisitorLogicClientAvatar()
        {
            LogicClientAvatar tmp = this.m_visitorLogicClientAvatar;
            this.m_visitorLogicClientAvatar = null;
            return tmp;
        }

        public void SetVisitorLogicClientAvatar(LogicClientAvatar logicClientAvatar)
        {
            this.m_visitorLogicClientAvatar = logicClientAvatar;
        }

        public LogicClientAvatar RemoveOwnerLogicClientAvatar()
        {
            LogicClientAvatar tmp = this.m_ownerLogicClientAvatar;
            this.m_ownerLogicClientAvatar = null;
            return tmp;
        }

        public void SetOwnerLogicClientAvatar(LogicClientAvatar logicClientAvatar)
        {
            this.m_ownerLogicClientAvatar = logicClientAvatar;
        }
    }
}