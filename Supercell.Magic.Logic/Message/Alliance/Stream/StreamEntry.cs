namespace Supercell.Magic.Logic.Message.Alliance.Stream
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public abstract class StreamEntry
    {
        private LogicLong m_id;
        private LogicLong m_senderHomeId;
        private LogicLong m_senderAvatarId;

        private string m_senderName;

        private int m_senderLevel;
        private int m_senderLeagueType;
        private LogicAvatarAllianceRole m_senderRole;
        private int m_ageSeconds;

        private bool m_removed;

        protected StreamEntry()
        {
            this.m_id = new LogicLong();
        }

        public virtual void Destruct()
        {
            this.m_id = null;
            this.m_senderHomeId = null;
            this.m_senderAvatarId = null;
            this.m_senderName = null;
        }

        public virtual void Decode(ByteStream stream)
        {
            this.m_id = stream.ReadLong();

            bool hasSenderAvatarId = stream.ReadBoolean();
            bool hasSenderHomeId = stream.ReadBoolean();

            this.m_removed = stream.ReadBoolean();

            if (hasSenderAvatarId)
            {
                this.m_senderAvatarId = stream.ReadLong();
            }

            if (hasSenderHomeId)
            {
                this.m_senderHomeId = stream.ReadLong();
            }

            this.m_senderName = stream.ReadString(900000);
            this.m_senderLevel = stream.ReadInt();
            this.m_senderLeagueType = stream.ReadInt();
            this.m_senderRole = (LogicAvatarAllianceRole) stream.ReadInt();
            this.m_ageSeconds = stream.ReadInt();
        }

        public virtual void Encode(ByteStream stream)
        {
            stream.WriteLong(this.m_id);
            stream.WriteBoolean(this.m_senderAvatarId != null);
            stream.WriteBoolean(this.m_senderHomeId != null);
            stream.WriteBoolean(this.m_removed);

            if (this.m_senderAvatarId != null)
            {
                stream.WriteLong(this.m_senderAvatarId);
            }

            if (this.m_senderHomeId != null)
            {
                stream.WriteLong(this.m_senderHomeId);
            }

            stream.WriteString(this.m_senderName);
            stream.WriteInt(this.m_senderLevel);
            stream.WriteInt(this.m_senderLeagueType);
            stream.WriteInt((int) this.m_senderRole);
            stream.WriteInt(this.m_ageSeconds);
        }

        public LogicLong GetSenderAvatarId()
        {
            return this.m_senderAvatarId;
        }

        public void SetSenderAvatarId(LogicLong id)
        {
            this.m_senderAvatarId = id;
        }

        public LogicLong GetSenderHomeId()
        {
            return this.m_senderHomeId;
        }

        public void SetSenderHomeId(LogicLong id)
        {
            this.m_senderHomeId = id;
        }

        public LogicLong GetId()
        {
            return this.m_id;
        }

        public void SetId(LogicLong id)
        {
            this.m_id = id;
        }

        public string GetSenderName()
        {
            return this.m_senderName;
        }

        public void SetSenderName(string name)
        {
            this.m_senderName = name;
        }

        public int GetSenderLevel()
        {
            return this.m_senderLevel;
        }

        public void SetSenderLevel(int value)
        {
            this.m_senderLevel = value;
        }

        public int GetSenderLeagueType()
        {
            return this.m_senderLeagueType;
        }

        public void SetSenderLeagueType(int value)
        {
            this.m_senderLeagueType = value;
        }

        public LogicAvatarAllianceRole GetSenderRole()
        {
            return this.m_senderRole;
        }

        public void SetSenderRole(LogicAvatarAllianceRole value)
        {
            this.m_senderRole = value;
        }

        public int GetAgeSeconds()
        {
            return this.m_ageSeconds;
        }

        public void SetAgeSeconds(int value)
        {
            this.m_ageSeconds = value;
        }

        public bool IsRemoved()
        {
            return this.m_removed;
        }

        public void SetRemoved(bool removed)
        {
            this.m_removed = removed;
        }

        public abstract StreamEntryType GetStreamEntryType();

        public virtual void Save(LogicJSONObject baseObject)
        {
            if (this.m_senderAvatarId != null)
            {
                baseObject.Put("sender_avatar_id_high", new LogicJSONNumber(this.m_senderAvatarId.GetHigherInt()));
                baseObject.Put("sender_avatar_id_low", new LogicJSONNumber(this.m_senderAvatarId.GetLowerInt()));
            }

            if (this.m_senderHomeId != null)
            {
                baseObject.Put("sender_home_id_high", new LogicJSONNumber(this.m_senderHomeId.GetHigherInt()));
                baseObject.Put("sender_home_id_low", new LogicJSONNumber(this.m_senderHomeId.GetLowerInt()));
            }

            baseObject.Put("sender_name", new LogicJSONString(this.m_senderName));
            baseObject.Put("sender_level", new LogicJSONNumber(this.m_senderLevel));
            baseObject.Put("sender_league_type", new LogicJSONNumber(this.m_senderLeagueType));
            baseObject.Put("sender_role", new LogicJSONNumber((int) this.m_senderRole));
            baseObject.Put("removed", new LogicJSONBoolean(this.m_removed));
        }

        public virtual void Load(LogicJSONObject jsonObject)
        {
            LogicJSONNumber senderAvatarIdHighObject = jsonObject.GetJSONNumber("sender_avatar_id_high");
            LogicJSONNumber senderAvatarIdLowObject = jsonObject.GetJSONNumber("sender_avatar_id_low");

            if (senderAvatarIdHighObject != null && senderAvatarIdLowObject != null)
            {
                this.m_senderAvatarId = new LogicLong(senderAvatarIdHighObject.GetIntValue(), senderAvatarIdLowObject.GetIntValue());
            }

            LogicJSONNumber senderHomeIdHighObject = jsonObject.GetJSONNumber("sender_home_id_high");
            LogicJSONNumber senderHomeIdLowObject = jsonObject.GetJSONNumber("sender_home_id_low");

            if (senderHomeIdHighObject != null && senderHomeIdLowObject != null)
            {
                this.m_senderHomeId = new LogicLong(senderHomeIdHighObject.GetIntValue(), senderHomeIdLowObject.GetIntValue());
            }


            this.m_senderName = LogicJSONHelper.GetString(jsonObject, "sender_name");
            this.m_senderLevel = LogicJSONHelper.GetInt(jsonObject, "sender_level");
            this.m_senderLeagueType = LogicJSONHelper.GetInt(jsonObject, "sender_league_type");
            this.m_senderRole = (LogicAvatarAllianceRole) LogicJSONHelper.GetInt(jsonObject, "sender_role");
            this.m_removed = LogicJSONHelper.GetBool(jsonObject, "removed");
        }
    }

    public enum StreamEntryType
    {
        DONATE = 1,
        CHAT = 2,
        JOIN_REQUEST = 3,
        ALLIANCE_EVENT = 4,
        REPLAY = 5,
        CHALLENGE_REPLAY = 11,
        CHALLENGE = 12,
        ALLIANCE_GIFT = 16,
        VERSUS_BATTLE_REQUEST = 18,
        VERSUS_BATTLE_REPLAY = 19,
        DUEL_REPLAY = 21
    }
}