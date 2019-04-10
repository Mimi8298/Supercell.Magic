namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class AllianceHeaderEntry
    {
        private LogicLong m_allianceId;
        private LogicData m_localeData;
        private LogicData m_originData;

        private string m_allianceName;

        private int m_allianceBadgeId;
        private AllianceType m_allianceType;
        private int m_memberCount;
        private int m_score;
        private int m_duelScore;
        private int m_requiredScore;
        private int m_requiredDuelScore;
        private int m_winWarCount;
        private int m_lostWarCount;
        private int m_drawWarCount;
        private int m_warFrequency;
        private int m_expPoints;
        private int m_expLevel;
        private int m_consecutiveWinWarCount;

        private bool m_publicWarLog;
        private bool m_amicalWarEnabled;

        public AllianceHeaderEntry()
        {
            this.m_allianceId = new LogicLong();
            this.m_expLevel = 1;
        }

        public void Decode(ByteStream stream)
        {
            this.m_allianceId = stream.ReadLong();
            this.m_allianceName = stream.ReadString(900000);
            this.m_allianceBadgeId = stream.ReadInt();
            this.m_allianceType = (AllianceType) stream.ReadInt();
            this.m_memberCount = stream.ReadInt();
            this.m_score = stream.ReadInt();
            this.m_duelScore = stream.ReadInt();
            this.m_requiredScore = stream.ReadInt();
            this.m_requiredDuelScore = stream.ReadInt();
            this.m_winWarCount = stream.ReadInt();
            this.m_lostWarCount = stream.ReadInt();
            this.m_drawWarCount = stream.ReadInt();
            this.m_localeData = ByteStreamHelper.ReadDataReference(stream);
            this.m_warFrequency = stream.ReadInt();
            this.m_originData = ByteStreamHelper.ReadDataReference(stream);
            this.m_expPoints = stream.ReadInt();
            this.m_expLevel = stream.ReadInt();
            this.m_consecutiveWinWarCount = stream.ReadInt();
            this.m_publicWarLog = stream.ReadBoolean();
            stream.ReadInt();
            this.m_amicalWarEnabled = stream.ReadBoolean();
        }

        public void Encode(ByteStream stream)
        {
            stream.WriteLong(this.m_allianceId);
            stream.WriteString(this.m_allianceName);
            stream.WriteInt(this.m_allianceBadgeId);
            stream.WriteInt((int) this.m_allianceType);
            stream.WriteInt(this.m_memberCount);
            stream.WriteInt(this.m_score);
            stream.WriteInt(this.m_duelScore);
            stream.WriteInt(this.m_requiredScore);
            stream.WriteInt(this.m_requiredDuelScore);
            stream.WriteInt(this.m_winWarCount);
            stream.WriteInt(this.m_lostWarCount);
            stream.WriteInt(this.m_drawWarCount);
            ByteStreamHelper.WriteDataReference(stream, this.m_localeData);
            stream.WriteInt(this.m_warFrequency);
            ByteStreamHelper.WriteDataReference(stream, this.m_originData);
            stream.WriteInt(this.m_expPoints);
            stream.WriteInt(this.m_expLevel);
            stream.WriteInt(this.m_consecutiveWinWarCount);
            stream.WriteBoolean(this.m_publicWarLog);
            stream.WriteInt(0);
            stream.WriteBoolean(this.m_amicalWarEnabled);
        }

        public LogicLong GetAllianceId()
        {
            return this.m_allianceId;
        }

        public void SetAllianceId(LogicLong value)
        {
            this.m_allianceId = value;
        }

        public string GetAllianceName()
        {
            return this.m_allianceName;
        }

        public void SetAllianceName(string value)
        {
            this.m_allianceName = value;
        }

        public AllianceType GetAllianceType()
        {
            return this.m_allianceType;
        }

        public void SetAllianceType(AllianceType value)
        {
            this.m_allianceType = value;
        }

        public int GetRequiredScore()
        {
            return this.m_requiredScore;
        }

        public void SetRequiredScore(int value)
        {
            this.m_requiredScore = value;
        }

        public int GetRequiredDuelScore()
        {
            return this.m_requiredDuelScore;
        }

        public void SetRequiredDuelScore(int value)
        {
            this.m_requiredDuelScore = value;
        }

        public LogicData GetOriginData()
        {
            return this.m_originData;
        }

        public void SetOriginData(LogicData value)
        {
            this.m_originData = value;
        }

        public int GetNumberOfMembers()
        {
            return this.m_memberCount;
        }

        public void SetNumberOfMembers(int value)
        {
            this.m_memberCount = value;
        }

        public int GetAllianceBadgeId()
        {
            return this.m_allianceBadgeId;
        }

        public void SetAllianceBadgeId(int value)
        {
            this.m_allianceBadgeId = value;
        }

        public int GetWarFrequency()
        {
            return this.m_warFrequency;
        }

        public void SetWarFrequency(int value)
        {
            this.m_warFrequency = value;
        }

        public bool IsPublicWarLog()
        {
            return this.m_publicWarLog;
        }

        public void SetPublicWarLog(bool enabled)
        {
            this.m_publicWarLog = enabled;
        }

        public bool IsAmicalWarEnabled()
        {
            return this.m_amicalWarEnabled;
        }

        public void SetArrangedWarEnabled(bool enabled)
        {
            this.m_amicalWarEnabled = enabled;
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

        public int GetAllianceLevel()
        {
            return this.m_expLevel;
        }

        public void SetAllianceLevel(int value)
        {
            this.m_expLevel = value;
        }

        public int GetAllianceExpPoints()
        {
            return this.m_expPoints;
        }

        public void SetAllianceExpPoints(int value)
        {
            this.m_expPoints = value;
        }

        public void Load(LogicJSONObject jsonObject)
        {
            this.m_allianceName = jsonObject.GetJSONString("alliance_name").GetStringValue();
            this.m_allianceBadgeId = jsonObject.GetJSONNumber("badge_id").GetIntValue();
            this.m_allianceType = (AllianceType) jsonObject.GetJSONNumber("type").GetIntValue();
            this.m_memberCount = jsonObject.GetJSONNumber("member_count").GetIntValue();
            this.m_score = jsonObject.GetJSONNumber("score").GetIntValue();
            this.m_duelScore = jsonObject.GetJSONNumber("duel_score").GetIntValue();
            this.m_requiredScore = jsonObject.GetJSONNumber("required_score").GetIntValue();
            this.m_requiredDuelScore = jsonObject.GetJSONNumber("required_duel_score").GetIntValue();
            this.m_winWarCount = jsonObject.GetJSONNumber("win_war_count").GetIntValue();
            this.m_lostWarCount = jsonObject.GetJSONNumber("lost_war_count").GetIntValue();
            this.m_drawWarCount = jsonObject.GetJSONNumber("draw_war_count").GetIntValue();
            this.m_warFrequency = jsonObject.GetJSONNumber("war_freq").GetIntValue();
            this.m_expLevel = jsonObject.GetJSONNumber("xp_level").GetIntValue();
            this.m_expPoints = jsonObject.GetJSONNumber("xp_points").GetIntValue();
            this.m_consecutiveWinWarCount = jsonObject.GetJSONNumber("cons_win_war_count").GetIntValue();
            this.m_publicWarLog = jsonObject.GetJSONBoolean("public_war_log").IsTrue();
            this.m_amicalWarEnabled = jsonObject.GetJSONBoolean("amical_war_enabled").IsTrue();

            LogicJSONNumber localeObject = jsonObject.GetJSONNumber("locale");

            if (localeObject != null)
            {
                this.m_localeData = LogicDataTables.GetDataById(localeObject.GetIntValue());
            }

            LogicJSONNumber originObject = jsonObject.GetJSONNumber("origin");

            if (originObject != null)
            {
                this.m_originData = LogicDataTables.GetDataById(originObject.GetIntValue());
            }
        }

        public void Save(LogicJSONObject jsonObject)
        {
            jsonObject.Put("alliance_name", new LogicJSONString(this.m_allianceName));
            jsonObject.Put("badge_id", new LogicJSONNumber(this.m_allianceBadgeId));
            jsonObject.Put("type", new LogicJSONNumber((int) this.m_allianceType));
            jsonObject.Put("member_count", new LogicJSONNumber(this.m_memberCount));
            jsonObject.Put("score", new LogicJSONNumber(this.m_score));
            jsonObject.Put("duel_score", new LogicJSONNumber(this.m_duelScore));
            jsonObject.Put("required_score", new LogicJSONNumber(this.m_requiredScore));
            jsonObject.Put("required_duel_score", new LogicJSONNumber(this.m_requiredDuelScore));
            jsonObject.Put("win_war_count", new LogicJSONNumber(this.m_winWarCount));
            jsonObject.Put("lost_war_count", new LogicJSONNumber(this.m_lostWarCount));
            jsonObject.Put("draw_war_count", new LogicJSONNumber(this.m_drawWarCount));
            jsonObject.Put("war_freq", new LogicJSONNumber(this.m_warFrequency));
            jsonObject.Put("xp_level", new LogicJSONNumber(this.m_expLevel));
            jsonObject.Put("xp_points", new LogicJSONNumber(this.m_expPoints));
            jsonObject.Put("cons_win_war_count", new LogicJSONNumber(this.m_consecutiveWinWarCount));
            jsonObject.Put("public_war_log", new LogicJSONBoolean(this.m_publicWarLog));
            jsonObject.Put("amical_war_enabled", new LogicJSONBoolean(this.m_amicalWarEnabled));

            if (this.m_localeData != null)
            {
                jsonObject.Put("locale", new LogicJSONNumber(this.m_localeData.GetGlobalID()));
            }

            if (this.m_originData != null)
            {
                jsonObject.Put("origin", new LogicJSONNumber(this.m_originData.GetGlobalID()));
            }
        }
    }

    public enum AllianceType
    {
        OPEN = 1,
        INVITE_ONLY,
        CLOSED
    }
}