namespace Supercell.Magic.Logic.Message.Avatar.Stream
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class AvatarStreamEntry
    {
        private LogicLong m_id;
        private LogicLong m_senderAvatarId;

        private string m_senderName;

        private int m_senderExpLevel;
        private int m_senderLeagueType;
        private int m_ageSeconds;

        private bool m_new;
        private bool m_dismiss;

        public AvatarStreamEntry()
        {
            this.m_new = true;
        }

        public virtual void Destruct()
        {
            this.m_id = null;
            this.m_senderAvatarId = null;
            this.m_senderName = null;
        }

        public virtual void Encode(ByteStream stream)
        {
            stream.WriteLong(this.m_id);

            if (this.m_senderAvatarId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteLong(this.m_senderAvatarId);
            }
            else
            {
                stream.WriteBoolean(false);
            }

            stream.WriteString(this.m_senderName);
            stream.WriteInt(this.m_senderExpLevel);
            stream.WriteInt(this.m_senderLeagueType);
            stream.WriteInt(this.m_ageSeconds);
            stream.WriteBoolean(this.m_dismiss);
            stream.WriteBoolean(this.m_new);
        }

        public virtual void Decode(ByteStream stream)
        {
            this.m_id = stream.ReadLong();

            if (stream.ReadBoolean())
            {
                this.m_senderAvatarId = stream.ReadLong();
            }

            this.m_senderName = stream.ReadString(900000);
            this.m_senderExpLevel = stream.ReadInt();
            this.m_senderLeagueType = stream.ReadInt();
            this.m_ageSeconds = stream.ReadInt();
            this.m_dismiss = stream.ReadBoolean();
            this.m_new = stream.ReadBoolean();
        }

        public virtual AvatarStreamEntryType GetAvatarStreamEntryType()
        {
            Debugger.Error("getAvatarStreamEntryType() must be overridden");
            return (AvatarStreamEntryType) (-1);
        }

        public virtual void Save(LogicJSONObject jsonObject)
        {
            LogicJSONObject senderObject = new LogicJSONObject();

            if (this.m_senderAvatarId != null)
            {
                senderObject.Put("avatar_id_hi", new LogicJSONNumber(this.m_senderAvatarId.GetHigherInt()));
                senderObject.Put("avatar_id_lo", new LogicJSONNumber(this.m_senderAvatarId.GetLowerInt()));
            }

            senderObject.Put("name", new LogicJSONString(this.m_senderName));
            senderObject.Put("exp_lvl", new LogicJSONNumber(this.m_senderExpLevel));
            senderObject.Put("league_type", new LogicJSONNumber(this.m_senderLeagueType));
            senderObject.Put("is_dismissed", new LogicJSONBoolean(this.m_dismiss));
            senderObject.Put("is_new", new LogicJSONBoolean(this.m_new));

            jsonObject.Put("sender", senderObject);
        }

        public virtual void Load(LogicJSONObject jsonObject)
        {
            LogicJSONObject senderObject = jsonObject.GetJSONObject("sender");

            if (senderObject != null)
            {
                LogicJSONNumber avatarIdHighNumber = senderObject.GetJSONNumber("avatar_id_hi");

                if (avatarIdHighNumber != null)
                {
                    this.m_senderAvatarId = new LogicLong(avatarIdHighNumber.GetIntValue(), senderObject.GetJSONNumber("avatar_id_lo").GetIntValue());
                }

                this.m_senderName = senderObject.GetJSONString("name").GetStringValue();
                this.m_senderExpLevel = senderObject.GetJSONNumber("exp_lvl").GetIntValue();
                this.m_senderLeagueType = senderObject.GetJSONNumber("league_type").GetIntValue();

                LogicJSONBoolean isDismissedObject = senderObject.GetJSONBoolean("is_dismissed");

                if (isDismissedObject != null)
                {
                    this.m_dismiss = isDismissedObject.IsTrue();
                }

                LogicJSONBoolean isNewObject = senderObject.GetJSONBoolean("is_new");

                if (isNewObject != null)
                {
                    this.m_new = isNewObject.IsTrue();
                }
            }
        }

        public LogicLong GetId()
        {
            return this.m_id;
        }

        public void SetId(LogicLong id)
        {
            this.m_id = id;
        }

        public LogicLong GetSenderAvatarId()
        {
            return this.m_senderAvatarId;
        }

        public void SetSenderAvatarId(LogicLong allianceId)
        {
            this.m_senderAvatarId = allianceId;
        }

        public int GetAgeSeconds()
        {
            return this.m_ageSeconds;
        }

        public void SetAgeSeconds(int value)
        {
            this.m_ageSeconds = value;
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
            return this.m_senderExpLevel;
        }

        public void SetSenderLevel(int value)
        {
            this.m_senderExpLevel = value;
        }

        public int GetSenderLeagueType()
        {
            return this.m_senderLeagueType;
        }

        public void SetSenderLeagueType(int value)
        {
            this.m_senderLeagueType = value;
        }

        public bool IsNew()
        {
            return this.m_new;
        }

        public void SetNew(bool isNew)
        {
            this.m_new = isNew;
        }
    }

    public enum AvatarStreamEntryType
    {
        DEFENDER_BATTLE_REPORT = 2,
        JOIN_ALLIANCE_RESPONSE,
        ALLIANCE_INVITATION,
        ALLIANCE_KICKOUT,
        ALLIANCE_MAIL,
        ATTACKER_BATTLE_REPORT,
        DEVICE_LINKED,
        ADMIN_MESSAGE = 10
    }
}