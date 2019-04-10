namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class AllianceMemberEntry
    {
        private LogicLong m_avatarId;
        private LogicLong m_homeId;

        private string m_name;

        private LogicAvatarAllianceRole m_allianceRole;
        private int m_expLevel;
        private int m_leagueType;
        private int m_score;
        private int m_duelScore;
        private int m_donationCount;
        private int m_receivedDonationCount;
        private int m_order;
        private int m_previousOrder;
        private int m_orderVillage2;
        private int m_previousOrderVillage2;
        private int m_createdTime;
        private int m_warCooldown;
        private int m_warPreference;

        public void Decode(ByteStream stream)
        {
            this.m_avatarId = stream.ReadLong();
            this.m_name = stream.ReadString(900000);
            this.m_allianceRole = (LogicAvatarAllianceRole) stream.ReadInt();
            this.m_expLevel = stream.ReadInt();
            this.m_leagueType = stream.ReadInt();
            this.m_score = stream.ReadInt();
            this.m_duelScore = stream.ReadInt();
            this.m_donationCount = stream.ReadInt();
            this.m_receivedDonationCount = stream.ReadInt();
            this.m_order = stream.ReadInt();
            this.m_previousOrder = stream.ReadInt();
            this.m_orderVillage2 = stream.ReadInt();
            this.m_previousOrderVillage2 = stream.ReadInt();
            this.m_createdTime = stream.ReadInt();
            this.m_warCooldown = stream.ReadInt();
            this.m_warPreference = stream.ReadInt();

            if (stream.ReadBoolean())
            {
                this.m_homeId = stream.ReadLong();
            }
        }

        public void Encode(ByteStream stream)
        {
            stream.WriteLong(this.m_avatarId);
            stream.WriteString(this.m_name);
            stream.WriteInt((int) this.m_allianceRole);
            stream.WriteInt(this.m_expLevel);
            stream.WriteInt(this.m_leagueType);
            stream.WriteInt(this.m_score);
            stream.WriteInt(this.m_duelScore);
            stream.WriteInt(this.m_donationCount);
            stream.WriteInt(this.m_receivedDonationCount);
            stream.WriteInt(this.m_order);
            stream.WriteInt(this.m_previousOrder);
            stream.WriteInt(this.m_orderVillage2);
            stream.WriteInt(this.m_previousOrderVillage2);
            stream.WriteInt(this.m_createdTime);
            stream.WriteInt(this.m_warCooldown);
            stream.WriteInt(this.m_warPreference);

            if (this.m_homeId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteLong(this.m_homeId);
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

        public string GetName()
        {
            return this.m_name;
        }

        public void SetName(string name)
        {
            this.m_name = name;
        }

        public LogicAvatarAllianceRole GetAllianceRole()
        {
            return this.m_allianceRole;
        }

        public void SetAllianceRole(LogicAvatarAllianceRole value)
        {
            this.m_allianceRole = value;
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

        public int GetDonations()
        {
            return this.m_donationCount;
        }

        public void SetDonations(int value)
        {
            this.m_donationCount = value;
        }

        public int GetReceivedDonations()
        {
            return this.m_receivedDonationCount;
        }

        public void SetReceivedDonations(int value)
        {
            this.m_receivedDonationCount = value;
        }

        public int GetOrder()
        {
            return this.m_order;
        }

        public void SetOrder(int order)
        {
            this.m_order = order;
        }

        public int GetPreviousOrder()
        {
            return this.m_previousOrder;
        }

        public void SetPreviousOrder(int order)
        {
            this.m_previousOrder = order;
        }

        public int GetOrderVillage2()
        {
            return this.m_orderVillage2;
        }

        public void SetOrderVillage2(int order)
        {
            this.m_orderVillage2 = order;
        }

        public int GetPreviousOrderVillage2()
        {
            return this.m_previousOrderVillage2;
        }

        public void SetPreviousOrderVillage2(int order)
        {
            this.m_previousOrderVillage2 = order;
        }

        public bool IsNewMember()
        {
            return this.m_createdTime < 259200;
        }

        public int GetCreatedTime()
        {
            return this.m_createdTime;
        }

        public void SetCreatedTime(int value)
        {
            this.m_createdTime = value;
        }

        public int GetWarCooldown()
        {
            return this.m_warCooldown;
        }

        public void SetWarCooldown(int value)
        {
            this.m_warCooldown = value;
        }

        public int GetWarPreference()
        {
            return this.m_warPreference;
        }

        public void SetWarPreference(int value)
        {
            this.m_warPreference = value;
        }

        public bool HasLowerRoleThan(LogicAvatarAllianceRole role)
        {
            switch (role)
            {
                case LogicAvatarAllianceRole.LEADER:
                    return this.m_allianceRole != LogicAvatarAllianceRole.LEADER;
                case LogicAvatarAllianceRole.ELDER:
                    return this.m_allianceRole == LogicAvatarAllianceRole.MEMBER;
                case LogicAvatarAllianceRole.CO_LEADER:
                    return this.m_allianceRole != LogicAvatarAllianceRole.LEADER &&
                           this.m_allianceRole != LogicAvatarAllianceRole.CO_LEADER;
                default:
                    return false;
            }
        }

        public static bool HasLowerRole(LogicAvatarAllianceRole role1, LogicAvatarAllianceRole role2)
        {
            switch (role2)
            {
                case LogicAvatarAllianceRole.LEADER:
                    return role1 != LogicAvatarAllianceRole.LEADER;
                case LogicAvatarAllianceRole.ELDER:
                    return role1 == LogicAvatarAllianceRole.MEMBER;
                case LogicAvatarAllianceRole.CO_LEADER:
                    return role1 != LogicAvatarAllianceRole.LEADER &&
                           role1 != LogicAvatarAllianceRole.CO_LEADER;
                default:
                    return false;
            }
        }

        public void Load(LogicJSONObject jsonObject)
        {
            LogicJSONNumber avatarIdHighObject = jsonObject.GetJSONNumber("avatar_id_high");
            LogicJSONNumber avatarIdLowObject = jsonObject.GetJSONNumber("avatar_id_low");

            if (avatarIdHighObject != null && avatarIdLowObject != null)
            {
                this.m_avatarId = new LogicLong(avatarIdHighObject.GetIntValue(), avatarIdLowObject.GetIntValue());
            }

            LogicJSONNumber homeIdHighObject = jsonObject.GetJSONNumber("home_id_high");
            LogicJSONNumber homeIdLowObject = jsonObject.GetJSONNumber("home_id_low");

            if (homeIdHighObject != null && homeIdLowObject != null)
            {
                this.m_homeId = new LogicLong(homeIdHighObject.GetIntValue(), homeIdLowObject.GetIntValue());
            }

            this.m_name = LogicJSONHelper.GetString(jsonObject, "name");
            this.m_allianceRole = (LogicAvatarAllianceRole) LogicJSONHelper.GetInt(jsonObject, "alliance_role");
            this.m_expLevel = LogicJSONHelper.GetInt(jsonObject, "xp_level");
            this.m_leagueType = LogicJSONHelper.GetInt(jsonObject, "league_type");
            this.m_score = LogicJSONHelper.GetInt(jsonObject, "score");
            this.m_duelScore = LogicJSONHelper.GetInt(jsonObject, "duel_score");
            this.m_donationCount = LogicJSONHelper.GetInt(jsonObject, "donations");
            this.m_receivedDonationCount = LogicJSONHelper.GetInt(jsonObject, "received_donations");
            this.m_order = LogicJSONHelper.GetInt(jsonObject, "order");
            this.m_previousOrder = LogicJSONHelper.GetInt(jsonObject, "prev_order");
            this.m_orderVillage2 = LogicJSONHelper.GetInt(jsonObject, "order_v2");
            this.m_previousOrderVillage2 = LogicJSONHelper.GetInt(jsonObject, "prev_order_v2");
            this.m_warCooldown = LogicJSONHelper.GetInt(jsonObject, "war_cooldown");
            this.m_warPreference = LogicJSONHelper.GetInt(jsonObject, "war_preference");
        }

        public LogicJSONObject Save()
        {
            LogicJSONObject jsonObject = new LogicJSONObject();

            jsonObject.Put("avatar_id_high", new LogicJSONNumber(this.m_avatarId.GetHigherInt()));
            jsonObject.Put("avatar_id_low", new LogicJSONNumber(this.m_avatarId.GetLowerInt()));

            if (this.m_homeId != null)
            {
                jsonObject.Put("home_id_high", new LogicJSONNumber(this.m_homeId.GetHigherInt()));
                jsonObject.Put("home_id_low", new LogicJSONNumber(this.m_homeId.GetLowerInt()));
            }

            jsonObject.Put("name", new LogicJSONString(this.m_name));
            jsonObject.Put("alliance_role", new LogicJSONNumber((int) this.m_allianceRole));
            jsonObject.Put("xp_level", new LogicJSONNumber(this.m_expLevel));
            jsonObject.Put("league_type", new LogicJSONNumber(this.m_leagueType));
            jsonObject.Put("score", new LogicJSONNumber(this.m_score));
            jsonObject.Put("duel_score", new LogicJSONNumber(this.m_duelScore));
            jsonObject.Put("donations", new LogicJSONNumber(this.m_donationCount));
            jsonObject.Put("received_donations", new LogicJSONNumber(this.m_receivedDonationCount));
            jsonObject.Put("order", new LogicJSONNumber(this.m_order));
            jsonObject.Put("prev_order", new LogicJSONNumber(this.m_previousOrder));
            jsonObject.Put("order_v2", new LogicJSONNumber(this.m_orderVillage2));
            jsonObject.Put("prev_order_v2", new LogicJSONNumber(this.m_previousOrderVillage2));
            jsonObject.Put("war_cooldown", new LogicJSONNumber(this.m_warCooldown));
            jsonObject.Put("war_preference", new LogicJSONNumber(this.m_warPreference));

            return jsonObject;
        }
    }
}