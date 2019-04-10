namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Home;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class FriendlyScoutHomeDataMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 25007;

        private int m_currentTimestamp;
        private int m_mapId;

        private LogicLong m_avatarId;
        private LogicLong m_accountId;
        private LogicLong m_streamId;

        private LogicClientAvatar m_logicClientAvatar;
        private LogicClientHome m_logicClientHome;

        public FriendlyScoutHomeDataMessage() : this(0)
        {
            // FriendlyScoutHomeDataMessage.
        }

        public FriendlyScoutHomeDataMessage(short messageVersion) : base(messageVersion)
        {
            // FriendlyScoutHomeDataMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_currentTimestamp = this.m_stream.ReadInt();
            this.m_avatarId = this.m_stream.ReadLong();
            this.m_accountId = this.m_stream.ReadLong();
            this.m_logicClientHome = new LogicClientHome();
            this.m_logicClientHome.Decode(this.m_stream);
            this.m_mapId = this.m_stream.ReadInt();

            if (this.m_stream.ReadBoolean())
            {
                this.m_logicClientAvatar = new LogicClientAvatar();
                this.m_logicClientAvatar.Decode(this.m_stream);
            }

            this.m_streamId = this.m_stream.ReadLong();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(this.m_currentTimestamp);
            this.m_stream.WriteLong(this.m_avatarId);
            this.m_stream.WriteLong(this.m_accountId);
            this.m_logicClientHome.Encode(this.m_stream);
            this.m_stream.WriteInt(this.m_mapId);

            if (this.m_logicClientAvatar != null)
            {
                this.m_stream.WriteBoolean(true);
                this.m_logicClientAvatar.Encode(this.m_stream);
            }
            else
            {
                this.m_stream.WriteBoolean(false);
            }

            this.m_stream.WriteLong(this.m_streamId);
        }

        public override short GetMessageType()
        {
            return FriendlyScoutHomeDataMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
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

        public int GetMapId()
        {
            return this.m_mapId;
        }

        public void SetMapId(int value)
        {
            this.m_mapId = value;
        }

        public LogicLong GetAvatarId()
        {
            return this.m_avatarId;
        }

        public void SetAvatarId(LogicLong value)
        {
            this.m_avatarId = value;
        }

        public LogicLong GetAccountId()
        {
            return this.m_accountId;
        }

        public void SetAccountId(LogicLong value)
        {
            this.m_accountId = value;
        }

        public LogicLong GetStreamId()
        {
            return this.m_streamId;
        }

        public void SetStreamId(LogicLong value)
        {
            this.m_streamId = value;
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