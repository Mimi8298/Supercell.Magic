namespace Supercell.Magic.Logic.Message.Scoring
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;

    public class AllianceRankingEntry : RankingEntry
    {
        public const string JSON_ATTRIBUTE_BADGE_ID = "badgeId";
        public const string JSON_ATTRIBUTE_EXP_LEVEL = "xpLvl";
        public const string JSON_ATTRIBUTE_MEMBER_COUNT = "memberCnt";
        public const string JSON_ATTRIBUTE_ORIGIN = "origin";

        private int m_badgeId;
        private int m_expLevel;
        private int m_memberCount;

        private LogicData m_originData;

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

            stream.WriteInt(this.m_badgeId);
            stream.WriteInt(this.m_memberCount);
            ByteStreamHelper.WriteDataReference(stream, this.m_originData);
            stream.WriteInt(this.m_expLevel);
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_badgeId = stream.ReadInt();
            this.m_memberCount = stream.ReadInt();
            this.m_originData = ByteStreamHelper.ReadDataReference(stream);
            this.m_expLevel = stream.ReadInt();
        }

        public int GetAllianceBadgeId()
        {
            return this.m_badgeId;
        }

        public void SetAllianceBadgeId(int value)
        {
            this.m_badgeId = value;
        }

        public int GetMemberCount()
        {
            return this.m_memberCount;
        }

        public void SetMemberCount(int value)
        {
            this.m_memberCount = value;
        }

        public int GetAllianceLevel()
        {
            return this.m_expLevel;
        }

        public void SetAllianceLevel(int value)
        {
            this.m_expLevel = value;
        }

        public LogicData GetOriginData()
        {
            return this.m_originData;
        }

        public void SetOriginData(LogicData data)
        {
            this.m_originData = data;
        }

        public override LogicJSONObject Save()
        {
            LogicJSONObject jsonObject = base.Save();

            jsonObject.Put(AllianceRankingEntry.JSON_ATTRIBUTE_BADGE_ID, new LogicJSONNumber(this.m_badgeId));
            jsonObject.Put(AllianceRankingEntry.JSON_ATTRIBUTE_EXP_LEVEL, new LogicJSONNumber(this.m_expLevel));
            jsonObject.Put(AllianceRankingEntry.JSON_ATTRIBUTE_MEMBER_COUNT, new LogicJSONNumber(this.m_memberCount));

            if (this.m_originData != null)
                jsonObject.Put(AllianceRankingEntry.JSON_ATTRIBUTE_ORIGIN, new LogicJSONNumber(this.m_originData.GetGlobalID()));

            return jsonObject;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            base.Load(jsonObject);

            this.m_badgeId = jsonObject.GetJSONNumber(AllianceRankingEntry.JSON_ATTRIBUTE_BADGE_ID).GetIntValue();
            this.m_expLevel = jsonObject.GetJSONNumber(AllianceRankingEntry.JSON_ATTRIBUTE_EXP_LEVEL).GetIntValue();
            this.m_memberCount = jsonObject.GetJSONNumber(AllianceRankingEntry.JSON_ATTRIBUTE_MEMBER_COUNT).GetIntValue();

            LogicJSONNumber originNumber = jsonObject.GetJSONNumber(AllianceRankingEntry.JSON_ATTRIBUTE_ORIGIN);

            if (originNumber != null)
                this.m_originData = LogicDataTables.GetDataById(originNumber.GetIntValue());
        }
    }
}