namespace Supercell.Magic.Logic.Message.Friend
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class FriendEntry
    {
        private LogicLong m_avatarId;
        private LogicLong m_homeId;
        private LogicLong m_allianceId;
        private LogicLong m_liveReplayId;

        private string m_name;
        private string m_allianceName;
        private string m_facebookId;
        private string m_gamecenterId;

        private int m_protectionDurationSeconds;
        private int m_expLevel;
        private int m_leagueType;
        private int m_score;
        private int m_duelScore;
        private int m_friendState;
        private int m_allianceBadgeId;
        private int m_allianceRole;
        private int m_allianceLevel;

        public FriendEntry()
        {
            this.m_friendState = -1;
            this.m_allianceBadgeId = -1;
        }

        public void Decode(ByteStream stream)
        {
            this.m_avatarId = stream.ReadLong();

            if (stream.ReadBoolean())
            {
                this.m_homeId = stream.ReadLong();
            }

            this.m_name = stream.ReadString(900000);
            this.m_facebookId = stream.ReadString(900000);
            this.m_gamecenterId = stream.ReadString(900000);

            stream.ReadString(900000);

            this.m_protectionDurationSeconds = stream.ReadInt();
            this.m_expLevel = stream.ReadInt();
            this.m_leagueType = stream.ReadInt();
            this.m_score = stream.ReadInt();
            this.m_duelScore = stream.ReadInt();
            this.m_friendState = stream.ReadInt();

            stream.ReadInt();

            if (stream.ReadBoolean())
            {
                this.m_allianceId = stream.ReadLong();
                this.m_allianceBadgeId = stream.ReadInt();
                this.m_allianceName = stream.ReadString(900000);
                this.m_allianceRole = stream.ReadInt();
                this.m_allianceLevel = stream.ReadInt();
            }

            if (stream.ReadBoolean())
            {
                this.m_liveReplayId = stream.ReadLong();
                stream.ReadInt();
            }
        }

        public void Encode(ByteStream stream)
        {
            stream.WriteLong(this.m_avatarId);

            if (this.m_homeId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteLong(this.m_homeId);
            }
            else
            {
                stream.WriteBoolean(false);
            }

            stream.WriteString(this.m_name);
            stream.WriteString(this.m_facebookId);
            stream.WriteString(this.m_gamecenterId);
            stream.WriteString(null);
            stream.WriteInt(this.m_protectionDurationSeconds);
            stream.WriteInt(this.m_expLevel);
            stream.WriteInt(this.m_leagueType);
            stream.WriteInt(this.m_score);
            stream.WriteInt(this.m_duelScore);
            stream.WriteInt(this.m_friendState);
            stream.WriteInt(0);

            if (this.m_allianceId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteLong(this.m_allianceId);
                stream.WriteInt(this.m_allianceBadgeId);
                stream.WriteString(this.m_allianceName);
                stream.WriteInt(this.m_allianceRole);
                stream.WriteInt(this.m_allianceLevel);
            }
            else
            {
                stream.WriteBoolean(false);
            }

            if (this.m_liveReplayId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteLong(this.m_liveReplayId);
                stream.WriteInt(0);
            }
            else
            {
                stream.WriteBoolean(false);
            }
        }

        public LogicLong GetAvatarId()
        {
            return this.m_avatarId;
        }

        public void SetAvatarId(LogicLong value)
        {
            this.m_avatarId = value;
        }

        public LogicLong GetHomeId()
        {
            return this.m_homeId;
        }

        public void SetHomeId(LogicLong value)
        {
            this.m_homeId = value;
        }

        public LogicLong GetAllianceId()
        {
            return this.m_allianceId;
        }

        public void SetAllianceId(LogicLong value)
        {
            this.m_allianceId = value;
        }

        public LogicLong GetLiveReplayId()
        {
            return this.m_liveReplayId;
        }

        public void SetLiveReplayId(LogicLong value)
        {
            this.m_liveReplayId = value;
        }

        public string GetName()
        {
            return this.m_name;
        }

        public void SetName(string value)
        {
            this.m_name = value;
        }

        public string GetAllianceName()
        {
            return this.m_allianceName;
        }

        public void SetAllianceName(string value)
        {
            this.m_allianceName = value;
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

        public int GetProtectionDurationSeconds()
        {
            return this.m_protectionDurationSeconds;
        }

        public void SetProtectionDurationSeconds(int value)
        {
            this.m_protectionDurationSeconds = value;
        }

        public int GetExpLevel()
        {
            return this.m_expLevel;
        }

        public void SetExpLevel(int value)
        {
            this.m_expLevel = value;
        }

        public int GetLeagueType()
        {
            return this.m_leagueType;
        }

        public void SetLeagueType(int value)
        {
            this.m_leagueType = value;
        }

        public int GetScore()
        {
            return this.m_score;
        }

        public void SetScore(int value)
        {
            this.m_score = value;
        }

        public int GetDuelScore()
        {
            return this.m_duelScore;
        }

        public void SetDuelScore(int value)
        {
            this.m_duelScore = value;
        }

        public int GetFriendState()
        {
            return this.m_friendState;
        }

        public void SetFriendState(int value)
        {
            this.m_friendState = value;
        }

        public int GetAllianceBadgeId()
        {
            return this.m_allianceBadgeId;
        }

        public void SetAllianceBadgeId(int value)
        {
            this.m_allianceBadgeId = value;
        }

        public int GetAllianceRole()
        {
            return this.m_allianceRole;
        }

        public void SetAllianceRole(int value)
        {
            this.m_allianceRole = value;
        }

        public int GetAllianceLevel()
        {
            return this.m_allianceLevel;
        }

        public void SetAllianceLevel(int value)
        {
            this.m_allianceLevel = value;
        }
    }
}