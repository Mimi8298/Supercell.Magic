namespace Supercell.Magic.Logic.Message.Scoring
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class AvatarRankingEntry : RankingEntry
    {
        private const string JSON_ATTRIBUTE_EXP_LEVEL = "xpLvl";
        private const string JSON_ATTRIBUTE_ATTACK_WIN_COUNT = "attWinCnt";
        private const string JSON_ATTRIBUTE_ATTACK_LOSE_COUNT = "attLoseCnt";
        private const string JSON_ATTRIBUTE_DEFENSE_WIN_COUNT = "defWinCnt";
        private const string JSON_ATTRIBUTE_DEFENSE_LOSE_COUNT = "defLoseCnt";
        private const string JSON_ATTRIBUTE_LEAGUE_TYPE = "leagueT";
        private const string JSON_ATTRIBUTE_ALLIANCE = "alli";
        private const string JSON_ATTRIBUTE_ALLIANCE_ID = "id";
        private const string JSON_ATTRIBUTE_ALLIANCE_NAME = "name";
        private const string JSON_ATTRIBUTE_ALLIANCE_BADGE_ID = "badgeId";

        private int m_expLevel;
        private int m_attackWinCount;
        private int m_attackLoseCount;
        private int m_defenseWinCount;
        private int m_defenseLoseCount;
        private int m_leagueType;
        private int m_allianceBadgeId;

        private string m_country;
        private string m_allianceName;

        private LogicLong m_homeId;
        private LogicLong m_allianceId;

        public AvatarRankingEntry()
        {
            this.m_allianceBadgeId = -1;
        }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

            stream.WriteInt(this.m_expLevel);
            stream.WriteInt(this.m_attackWinCount);
            stream.WriteInt(this.m_attackLoseCount);
            stream.WriteInt(this.m_defenseWinCount);
            stream.WriteInt(this.m_defenseLoseCount);
            stream.WriteInt(this.m_leagueType);
            stream.WriteString(this.m_country);
            stream.WriteLong(this.m_homeId);
            stream.WriteInt(0);
            stream.WriteInt(0);

            if (this.m_allianceId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteLong(this.m_allianceId);
                stream.WriteString(this.m_allianceName);
                stream.WriteInt(this.m_allianceBadgeId);
            }
            else
            {
                stream.WriteBoolean(false);
            }
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_expLevel = stream.ReadInt();
            this.m_attackWinCount = stream.ReadInt();
            this.m_attackLoseCount = stream.ReadInt();
            this.m_defenseWinCount = stream.ReadInt();
            this.m_defenseLoseCount = stream.ReadInt();
            this.m_leagueType = stream.ReadInt();
            this.m_country = stream.ReadString(900000);
            this.m_homeId = stream.ReadLong();

            stream.ReadInt();
            stream.ReadInt();

            if (stream.ReadBoolean())
            {
                this.m_allianceId = stream.ReadLong();
                this.m_allianceName = stream.ReadString(900000);
                this.m_allianceBadgeId = stream.ReadInt();
            }
        }

        public int GetExpLevel()
        {
            return this.m_expLevel;
        }

        public void SetExpLevel(int value)
        {
            this.m_expLevel = value;
        }

        public int GetAttackWinCount()
        {
            return this.m_attackWinCount;
        }

        public void SetAttackWinCount(int value)
        {
            this.m_attackWinCount = value;
        }

        public int GetAttackLoseCount()
        {
            return this.m_attackLoseCount;
        }

        public void SetAttackLoseCount(int value)
        {
            this.m_attackLoseCount = value;
        }

        public int GetDefenseWinCount()
        {
            return this.m_defenseWinCount;
        }

        public void SetDefenseWinCount(int value)
        {
            this.m_defenseWinCount = value;
        }

        public int GetDefenseLoseCount()
        {
            return this.m_defenseLoseCount;
        }

        public void SetDefenseLoseCount(int value)
        {
            this.m_defenseLoseCount = value;
        }

        public int GetLeagueType()
        {
            return this.m_leagueType;
        }

        public void SetLeagueType(int value)
        {
            this.m_leagueType = value;
        }

        public int GetAllianceBadgeId()
        {
            return this.m_allianceBadgeId;
        }

        public void SetAllianceBadgeId(int value)
        {
            this.m_allianceBadgeId = value;
        }

        public string GetCountry()
        {
            return this.m_country;
        }

        public void SetCountry(string value)
        {
            this.m_country = value;
        }

        public string GetAllianceName()
        {
            return this.m_allianceName;
        }

        public void SetAllianceName(string value)
        {
            this.m_allianceName = value;
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

        public override LogicJSONObject Save()
        {
            LogicJSONObject jsonObject = base.Save();

            jsonObject.Put(AvatarRankingEntry.JSON_ATTRIBUTE_EXP_LEVEL, new LogicJSONNumber(this.m_expLevel));
            jsonObject.Put(AvatarRankingEntry.JSON_ATTRIBUTE_ATTACK_WIN_COUNT, new LogicJSONNumber(this.m_attackWinCount));
            jsonObject.Put(AvatarRankingEntry.JSON_ATTRIBUTE_ATTACK_LOSE_COUNT, new LogicJSONNumber(this.m_attackLoseCount));
            jsonObject.Put(AvatarRankingEntry.JSON_ATTRIBUTE_DEFENSE_WIN_COUNT, new LogicJSONNumber(this.m_defenseWinCount));
            jsonObject.Put(AvatarRankingEntry.JSON_ATTRIBUTE_DEFENSE_LOSE_COUNT, new LogicJSONNumber(this.m_defenseLoseCount));
            jsonObject.Put(AvatarRankingEntry.JSON_ATTRIBUTE_LEAGUE_TYPE, new LogicJSONNumber(this.m_leagueType));

            if (this.m_allianceId != null)
            {
                LogicJSONObject allianceObject = new LogicJSONObject();
                LogicJSONArray allianceIdArray = new LogicJSONArray(2);

                allianceIdArray.Add(new LogicJSONNumber(this.m_allianceId.GetHigherInt()));
                allianceIdArray.Add(new LogicJSONNumber(this.m_allianceId.GetLowerInt()));

                allianceObject.Put(AvatarRankingEntry.JSON_ATTRIBUTE_ALLIANCE_ID, allianceIdArray);
                allianceObject.Put(AvatarRankingEntry.JSON_ATTRIBUTE_ALLIANCE_NAME, new LogicJSONString(this.m_allianceName));
                allianceObject.Put(AvatarRankingEntry.JSON_ATTRIBUTE_ALLIANCE_BADGE_ID, new LogicJSONNumber(this.m_allianceBadgeId));

                jsonObject.Put(AvatarRankingEntry.JSON_ATTRIBUTE_ALLIANCE, allianceObject);
            }

            return jsonObject;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            base.Load(jsonObject);

            this.m_expLevel = jsonObject.GetJSONNumber(AvatarRankingEntry.JSON_ATTRIBUTE_EXP_LEVEL).GetIntValue();
            this.m_attackWinCount = jsonObject.GetJSONNumber(AvatarRankingEntry.JSON_ATTRIBUTE_ATTACK_WIN_COUNT).GetIntValue();
            this.m_attackLoseCount = jsonObject.GetJSONNumber(AvatarRankingEntry.JSON_ATTRIBUTE_ATTACK_LOSE_COUNT).GetIntValue();
            this.m_defenseWinCount = jsonObject.GetJSONNumber(AvatarRankingEntry.JSON_ATTRIBUTE_DEFENSE_WIN_COUNT).GetIntValue();
            this.m_defenseLoseCount = jsonObject.GetJSONNumber(AvatarRankingEntry.JSON_ATTRIBUTE_DEFENSE_LOSE_COUNT).GetIntValue();
            this.m_leagueType = jsonObject.GetJSONNumber(AvatarRankingEntry.JSON_ATTRIBUTE_LEAGUE_TYPE).GetIntValue();

            LogicJSONObject allianceObject = jsonObject.GetJSONObject(AvatarRankingEntry.JSON_ATTRIBUTE_ALLIANCE);

            if (allianceObject != null)
            {
                LogicJSONArray allianceIdArray = allianceObject.GetJSONArray(AvatarRankingEntry.JSON_ATTRIBUTE_ALLIANCE_ID);

                this.m_allianceId = new LogicLong(allianceIdArray.GetJSONNumber(0).GetIntValue(), allianceIdArray.GetJSONNumber(1).GetIntValue());
                this.m_allianceName = allianceObject.GetJSONString(AvatarRankingEntry.JSON_ATTRIBUTE_ALLIANCE_NAME).GetStringValue();
                this.m_allianceBadgeId = allianceObject.GetJSONNumber(AvatarRankingEntry.JSON_ATTRIBUTE_ALLIANCE_BADGE_ID).GetIntValue();
            }

            this.m_homeId = this.GetId().Clone();
        }
    }
}