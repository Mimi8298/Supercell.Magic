namespace Supercell.Magic.Logic.Message.Chat
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class GlobalChatLineMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24715;

        private string m_message;
        private string m_avatarName;
        private string m_allianceName;

        private int m_avatarExpLevel;
        private int m_avatarLeagueType;
        private int m_allianceBadgeId;

        private LogicLong m_avatarId;
        private LogicLong m_homeId;
        private LogicLong m_allianceId;

        public GlobalChatLineMessage() : this(0)
        {
            // GlobalChatLineMessage.
        }

        public GlobalChatLineMessage(short messageVersion) : base(messageVersion)
        {
            // GlobalChatLineMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_message = this.m_stream.ReadString(900000);
            this.m_avatarName = this.m_stream.ReadString(900000);
            this.m_avatarExpLevel = this.m_stream.ReadInt();
            this.m_avatarLeagueType = this.m_stream.ReadInt();
            this.m_avatarId = this.m_stream.ReadLong();
            this.m_homeId = this.m_stream.ReadLong();

            if (this.m_stream.ReadBoolean())
            {
                this.m_allianceId = this.m_stream.ReadLong();
                this.m_allianceName = this.m_stream.ReadString(900000);
                this.m_allianceBadgeId = this.m_stream.ReadInt();
            }
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteString(this.m_message);
            this.m_stream.WriteString(this.m_avatarName);
            this.m_stream.WriteInt(this.m_avatarExpLevel);
            this.m_stream.WriteInt(this.m_avatarLeagueType);
            this.m_stream.WriteLong(this.m_avatarId);
            this.m_stream.WriteLong(this.m_homeId);

            if (this.m_allianceId != null)
            {
                this.m_stream.WriteBoolean(true);
                this.m_stream.WriteLong(this.m_allianceId);
                this.m_stream.WriteString(this.m_allianceName);
                this.m_stream.WriteInt(this.m_allianceBadgeId);
            }
            else
            {
                this.m_stream.WriteBoolean(false);
            }
        }

        public override short GetMessageType()
        {
            return GlobalChatLineMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 6;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_message = null;
            this.m_avatarName = null;
            this.m_allianceName = null;
            this.m_avatarId = null;
            this.m_homeId = null;
            this.m_allianceId = null;
        }

        public string RemoveMessage(string message)
        {
            string tmp = this.m_message;
            this.m_message = null;
            return tmp;
        }

        public void SetMessage(string message)
        {
            this.m_message = message;
        }

        public string GetAvatarName()
        {
            return this.m_avatarName;
        }

        public void SetAvatarName(string name)
        {
            this.m_avatarName = name;
        }

        public string GetAllianceName()
        {
            return this.m_allianceName;
        }

        public void SetAllianceName(string name)
        {
            this.m_allianceName = name;
        }

        public int GetAvatarExpLevel()
        {
            return this.m_avatarExpLevel;
        }

        public void SetAvatarExpLevel(int lvl)
        {
            this.m_avatarExpLevel = lvl;
        }

        public int GetAvatarLeagueType()
        {
            return this.m_avatarLeagueType;
        }

        public void SetAvatarLeagueType(int leagueType)
        {
            this.m_avatarLeagueType = leagueType;
        }

        public LogicLong GetAvatarId()
        {
            return this.m_avatarId;
        }

        public void SetAvatarId(LogicLong id)
        {
            this.m_avatarId = id;
        }

        public LogicLong GetHomeId()
        {
            return this.m_homeId;
        }

        public void SetHomeId(LogicLong id)
        {
            this.m_homeId = id;
        }

        public LogicLong GetAllianceId()
        {
            return this.m_allianceId;
        }

        public void SetAllianceId(LogicLong id)
        {
            this.m_allianceId = id;
        }

        public int GetAllianceBadgeId()
        {
            return this.m_allianceBadgeId;
        }

        public void SetAllianceBadgeId(int id)
        {
            this.m_allianceBadgeId = id;
        }
    }
}