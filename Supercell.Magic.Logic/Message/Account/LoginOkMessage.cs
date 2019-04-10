namespace Supercell.Magic.Logic.Message.Account
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class LoginOkMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20104;

        private LogicLong m_accountId;
        private LogicLong m_homeId;

        private string m_passToken;
        private string m_facebookId;
        private string m_gamecenterId;
        private string m_serverEnvironment;
        private string m_facebookAppId;
        private string m_serverTime;
        private string m_accountCreatedDate;
        private string m_googleServiceId;
        private string m_region;

        private int m_serverMajorVersion;
        private int m_serverBuildVersion;
        private int m_contentVersion;
        private int m_sessionCount;
        private int m_playTimeSeconds;
        private int m_daysSinceStartedPlaying;
        private int m_startupCooldownSeconds;

        private LogicArrayList<string> m_contentUrlList;
        private LogicArrayList<string> m_chronosContentUrlList;

        public LoginOkMessage() : this(1)
        {
            // LoginOkMessage.
        }

        public LoginOkMessage(short messageVersion) : base(messageVersion)
        {
            // LoginOkMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_accountId = this.m_stream.ReadLong();
            this.m_homeId = this.m_stream.ReadLong();
            this.m_passToken = this.m_stream.ReadString(900000);
            this.m_facebookId = this.m_stream.ReadString(900000);
            this.m_gamecenterId = this.m_stream.ReadString(900000);
            this.m_serverMajorVersion = this.m_stream.ReadInt();
            this.m_serverBuildVersion = this.m_stream.ReadInt();
            this.m_contentVersion = this.m_stream.ReadInt();
            this.m_serverEnvironment = this.m_stream.ReadString(900000);
            this.m_sessionCount = this.m_stream.ReadInt();
            this.m_playTimeSeconds = this.m_stream.ReadInt();
            this.m_daysSinceStartedPlaying = this.m_stream.ReadInt();
            this.m_facebookAppId = this.m_stream.ReadString(900000);
            this.m_serverTime = this.m_stream.ReadString(900000);
            this.m_accountCreatedDate = this.m_stream.ReadString(900000);
            this.m_startupCooldownSeconds = this.m_stream.ReadInt();
            this.m_googleServiceId = this.m_stream.ReadString(9000);
            this.m_region = this.m_stream.ReadString(9000);
            this.m_stream.ReadString(9000);
            this.m_stream.ReadInt();
            this.m_stream.ReadString(9000);
            this.m_stream.ReadString(9000);
            this.m_stream.ReadString(9000);

            int contentUrlListSize = this.m_stream.ReadInt();

            if (contentUrlListSize != -1)
            {
                this.m_contentUrlList = new LogicArrayList<string>(contentUrlListSize);

                if (contentUrlListSize != 0)
                {
                    for (int i = 0; i < contentUrlListSize; i++)
                    {
                        this.m_contentUrlList.Add(this.m_stream.ReadString(900000));
                    }
                }
            }

            int chronosContentUrlListSize = this.m_stream.ReadInt();

            if (chronosContentUrlListSize != -1)
            {
                this.m_chronosContentUrlList = new LogicArrayList<string>(chronosContentUrlListSize);

                if (chronosContentUrlListSize != 0)
                {
                    for (int i = 0; i < chronosContentUrlListSize; i++)
                    {
                        this.m_chronosContentUrlList.Add(this.m_stream.ReadString(900000));
                    }
                }
            }
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteLong(this.m_accountId);
            this.m_stream.WriteLong(this.m_homeId);
            this.m_stream.WriteString(this.m_passToken);
            this.m_stream.WriteString(this.m_facebookId);
            this.m_stream.WriteString(this.m_gamecenterId);
            this.m_stream.WriteInt(this.m_serverMajorVersion);
            this.m_stream.WriteInt(this.m_serverBuildVersion);
            this.m_stream.WriteInt(this.m_contentVersion);
            this.m_stream.WriteString(this.m_serverEnvironment);
            this.m_stream.WriteInt(this.m_sessionCount);
            this.m_stream.WriteInt(this.m_playTimeSeconds);
            this.m_stream.WriteInt(this.m_daysSinceStartedPlaying);
            this.m_stream.WriteString(this.m_facebookAppId);
            this.m_stream.WriteString(this.m_serverTime);
            this.m_stream.WriteString(this.m_accountCreatedDate);
            this.m_stream.WriteInt(this.m_startupCooldownSeconds);
            this.m_stream.WriteString(this.m_googleServiceId);
            this.m_stream.WriteString(this.m_region);
            this.m_stream.WriteString(null);
            this.m_stream.WriteInt(1);
            this.m_stream.WriteString(null);
            this.m_stream.WriteString(null);
            this.m_stream.WriteString(null);

            if (this.m_contentUrlList != null)
            {
                this.m_stream.WriteInt(this.m_contentUrlList.Size());

                for (int i = 0; i < this.m_contentUrlList.Size(); i++)
                {
                    this.m_stream.WriteString(this.m_contentUrlList[i]);
                }
            }
            else
            {
                this.m_stream.WriteInt(-1);
            }

            if (this.m_chronosContentUrlList != null)
            {
                this.m_stream.WriteInt(this.m_chronosContentUrlList.Size());

                for (int i = 0; i < this.m_chronosContentUrlList.Size(); i++)
                {
                    this.m_stream.WriteString(this.m_chronosContentUrlList[i]);
                }
            }
            else
            {
                this.m_stream.WriteInt(-1);
            }
        }

        public override short GetMessageType()
        {
            return LoginOkMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_chronosContentUrlList = null;
            this.m_contentUrlList = null;

            this.m_passToken = null;
            this.m_facebookId = null;
            this.m_gamecenterId = null;
            this.m_serverEnvironment = null;
            this.m_facebookAppId = null;
            this.m_serverTime = null;
            this.m_accountCreatedDate = null;
            this.m_googleServiceId = null;
            this.m_region = null;
        }

        public LogicLong GetAccountId()
        {
            return this.m_accountId;
        }

        public void SetAccountId(LogicLong value)
        {
            this.m_accountId = value;
        }

        public LogicLong GetHomeId()
        {
            return this.m_homeId;
        }

        public void SetHomeId(LogicLong value)
        {
            this.m_homeId = value;
        }

        public string GetPassToken()
        {
            return this.m_passToken;
        }

        public void SetPassToken(string value)
        {
            this.m_passToken = value;
        }

        public string GetFacebookId()
        {
            return this.m_facebookId;
        }

        public void SetFacebookId(string value)
        {
            this.m_facebookId = value;
        }

        public string GetGamecenterId()
        {
            return this.m_gamecenterId;
        }

        public void SetGamecenterId(string value)
        {
            this.m_gamecenterId = value;
        }

        public string GetServerEnvironment()
        {
            return this.m_serverEnvironment;
        }

        public void SetServerEnvironment(string value)
        {
            this.m_serverEnvironment = value;
        }

        public string GetFacebookAppId()
        {
            return this.m_facebookAppId;
        }

        public void SetFacebookAppId(string value)
        {
            this.m_facebookAppId = value;
        }

        public string GetServerTime()
        {
            return this.m_serverTime;
        }

        public void SetServerTime(string value)
        {
            this.m_serverTime = value;
        }

        public string GetAccountCreatedDate()
        {
            return this.m_accountCreatedDate;
        }

        public void SetAccountCreatedDate(string value)
        {
            this.m_accountCreatedDate = value;
        }

        public string GetGoogleServiceId()
        {
            return this.m_googleServiceId;
        }

        public void SetGoogleServiceId(string value)
        {
            this.m_googleServiceId = value;
        }

        public string GetRegion()
        {
            return this.m_region;
        }

        public void SetRegion(string value)
        {
            this.m_region = value;
        }

        public int GetServerMajorVersion()
        {
            return this.m_serverMajorVersion;
        }

        public void SetServerMajorVersion(int value)
        {
            this.m_serverMajorVersion = value;
        }

        public int GetServerBuildVersion()
        {
            return this.m_serverBuildVersion;
        }

        public void SetServerBuildVersion(int value)
        {
            this.m_serverBuildVersion = value;
        }

        public int GetContentVersion()
        {
            return this.m_contentVersion;
        }

        public void SetContentVersion(int value)
        {
            this.m_contentVersion = value;
        }

        public int GetSessionCount()
        {
            return this.m_sessionCount;
        }

        public void SetSessionCount(int value)
        {
            this.m_sessionCount = value;
        }

        public int GetPlayTimeSeconds()
        {
            return this.m_playTimeSeconds;
        }

        public void SetPlayTimeSeconds(int value)
        {
            this.m_playTimeSeconds = value;
        }

        public int GetDaysSinceStartedPlaying()
        {
            return this.m_daysSinceStartedPlaying;
        }

        public void SetDaysSinceStartedPlaying(int value)
        {
            this.m_daysSinceStartedPlaying = value;
        }

        public int GetStartupCooldownSeconds()
        {
            return this.m_startupCooldownSeconds;
        }

        public void SetStartupCooldownSeconds(int value)
        {
            this.m_startupCooldownSeconds = value;
        }

        public LogicArrayList<string> GetContentUrlList()
        {
            return this.m_contentUrlList;
        }

        public void SetContentUrlList(LogicArrayList<string> value)
        {
            this.m_contentUrlList = value;
        }

        public LogicArrayList<string> GetChronosContentUrlList()
        {
            return this.m_chronosContentUrlList;
        }

        public void SetChronosContentUrlList(LogicArrayList<string> value)
        {
            this.m_chronosContentUrlList = value;
        }
    }
}