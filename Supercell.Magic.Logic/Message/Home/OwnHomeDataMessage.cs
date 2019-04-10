namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Home;
    using Supercell.Magic.Titan.Message;

    public class OwnHomeDataMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24101;

        private int m_secondsSinceLastMaintenance;
        private int m_currentTimestamp;
        private int m_secondsSinceLastSave;
        private int m_reengagementSeconds;

        private int m_mapId;
        private int m_layoutId;

        private LogicClientAvatar m_logicClientAvatar;
        private LogicClientHome m_logicClientHome;

        public OwnHomeDataMessage() : this(0)
        {
            // OwnHomeDataMessage.
        }

        public OwnHomeDataMessage(short messageVersion) : base(messageVersion)
        {
            // OwnHomeDataMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_secondsSinceLastSave = this.m_stream.ReadInt();
            this.m_secondsSinceLastMaintenance = this.m_stream.ReadInt();
            this.m_currentTimestamp = this.m_stream.ReadInt();

            this.m_logicClientHome = new LogicClientHome();
            this.m_logicClientHome.Decode(this.m_stream);
            this.m_logicClientAvatar = new LogicClientAvatar();
            this.m_logicClientAvatar.Decode(this.m_stream);

            this.m_mapId = this.m_stream.ReadInt();
            this.m_layoutId = this.m_stream.ReadInt();

            /* sub_36BCBC - START */

            this.m_stream.ReadInt();
            this.m_stream.ReadInt();

            this.m_stream.ReadInt();
            this.m_stream.ReadInt();

            this.m_stream.ReadInt();
            this.m_stream.ReadInt();

            /* sub_36BCBC - END */

            this.m_reengagementSeconds = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(this.m_secondsSinceLastSave);
            this.m_stream.WriteInt(this.m_secondsSinceLastMaintenance);
            this.m_stream.WriteInt(this.m_currentTimestamp);

            this.m_logicClientHome.Encode(this.m_stream);
            this.m_logicClientAvatar.Encode(this.m_stream);

            this.m_stream.WriteInt(this.m_mapId);
            this.m_stream.WriteInt(this.m_layoutId);

            this.m_stream.WriteInt(352);
            this.m_stream.WriteInt(1190797808);

            this.m_stream.WriteInt(352);
            this.m_stream.WriteInt(1192597808);

            this.m_stream.WriteInt(352);
            this.m_stream.WriteInt(1192597808);

            this.m_stream.WriteInt(this.m_reengagementSeconds);
        }

        public override short GetMessageType()
        {
            return OwnHomeDataMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 10;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_logicClientHome = null;
            this.m_logicClientAvatar = null;
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

        public int GetSecondsSinceLastMaintenance()
        {
            return this.m_secondsSinceLastMaintenance;
        }

        public void SetSecondsSinceLastMaintenance(int value)
        {
            this.m_secondsSinceLastMaintenance = value;
        }

        public int GetReengagementSeconds()
        {
            return this.m_reengagementSeconds;
        }

        public void SetReengagementSeconds(int value)
        {
            this.m_reengagementSeconds = value;
        }

        public int GetMapId()
        {
            return this.m_mapId;
        }

        public void SetMapId(int value)
        {
            this.m_mapId = value;
        }

        public int GetLayoutId()
        {
            return this.m_layoutId;
        }

        public void SetLayoutId(int value)
        {
            this.m_layoutId = value;
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

        public LogicClientAvatar RemoveLogicClientAvatar()
        {
            LogicClientAvatar tmp = this.m_logicClientAvatar;
            this.m_logicClientAvatar = null;
            return tmp;
        }

        public void SetLogicClientAvatar(LogicClientAvatar logicClientAvatar)
        {
            this.m_logicClientAvatar = logicClientAvatar;
        }
    }
}