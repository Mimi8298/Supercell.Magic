namespace Supercell.Magic.Logic.Message.Avatar.Attack
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class Village2AttackEntry
    {
        public const int ATTACK_ENTRY_TYPE_BASE = 0;
        public const int ATTACK_ENTRY_TYPE_BATTLE_PROGRESS = 1;

        private bool m_new;
        private bool m_removed;

        private LogicLong m_id;
        private LogicLong m_accountId;
        private LogicLong m_avatarId;
        private LogicLong m_homeId;
        private LogicLong m_allianceId;

        private string m_name;
        private string m_allianceName;

        private int m_allianceBadgeId;
        private int m_allianceExpLevel;
        private int m_remainingSeconds;

        public Village2AttackEntry()
        {
            this.m_new = true;
        }

        public virtual void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteBoolean(this.m_new);
            encoder.WriteBoolean(this.m_removed);

            if (!this.m_removed)
            {
                encoder.WriteLong(this.m_id);

                if (this.m_allianceId != null)
                {
                    encoder.WriteBoolean(true);
                    encoder.WriteLong(this.m_allianceId);
                    encoder.WriteString(this.m_allianceName);
                    encoder.WriteInt(this.m_allianceBadgeId);
                    encoder.WriteInt(this.m_allianceExpLevel);
                }
                else
                {
                    encoder.WriteBoolean(false);
                }

                encoder.WriteLong(this.m_accountId);
                encoder.WriteLong(this.m_avatarId);
                encoder.WriteString(this.m_name);
                encoder.WriteInt(-1);

                if (this.m_homeId != null)
                {
                    encoder.WriteBoolean(true);
                    encoder.WriteLong(this.m_homeId);
                }
                else
                {
                    encoder.WriteBoolean(false);
                }

                encoder.WriteVInt(5);
                encoder.WriteVInt(10);
                encoder.WriteVInt(15);
                encoder.WriteVInt(20);
                encoder.WriteBoolean(false);
                encoder.WriteVInt(this.m_remainingSeconds);
            }
        }

        public virtual void Decode(ByteStream stream)
        {
            this.m_new = stream.ReadBoolean();
            this.m_removed = stream.ReadBoolean();

            if (!this.m_removed)
            {
                this.m_id = stream.ReadLong();

                if (stream.ReadBoolean())
                {
                    this.m_allianceId = stream.ReadLong();
                    this.m_allianceName = stream.ReadString(900000);
                    this.m_allianceBadgeId = stream.ReadInt();
                    this.m_allianceExpLevel = stream.ReadInt();
                }

                this.m_accountId = stream.ReadLong();
                this.m_avatarId = stream.ReadLong();
                this.m_name = stream.ReadString(900000);

                stream.ReadInt();

                if (stream.ReadBoolean())
                {
                    this.m_homeId = stream.ReadLong();
                }

                stream.ReadVInt();
                stream.ReadVInt();
                stream.ReadVInt();
                stream.ReadVInt();
                stream.ReadBoolean();

                this.m_remainingSeconds = stream.ReadVInt();
            }
        }

        public virtual int GetAttackEntryType()
        {
            return Village2AttackEntry.ATTACK_ENTRY_TYPE_BASE;
        }

        public bool IsNew()
        {
            return this.m_new;
        }

        public void SetNew(bool value)
        {
            this.m_new = value;
        }

        public bool IsRemoved()
        {
            return this.m_removed;
        }

        public void SetRemoved(bool value)
        {
            this.m_removed = value;
        }

        public LogicLong GetId()
        {
            return this.m_id;
        }

        public void SetId(LogicLong value)
        {
            this.m_id = value;
        }

        public LogicLong GetAccountId()
        {
            return this.m_accountId;
        }

        public void SetAccountId(LogicLong value)
        {
            this.m_accountId = value;
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

        public int GetAllianceBadgeId()
        {
            return this.m_allianceBadgeId;
        }

        public void SetAllianceBadgeId(int value)
        {
            this.m_allianceBadgeId = value;
        }

        public int GetAllianceExpLevel()
        {
            return this.m_allianceExpLevel;
        }

        public void SetAllianceExpLevel(int value)
        {
            this.m_allianceExpLevel = value;
        }

        public int GetRemainingSeconds()
        {
            return this.m_remainingSeconds;
        }

        public void SetRemainingSeconds(int value)
        {
            this.m_remainingSeconds = value;
        }

        public virtual void Load(LogicJSONObject jsonObject)
        {
            LogicJSONNumber allianceIdHighNumber = jsonObject.GetJSONNumber("alliance_id_hi");

            if (allianceIdHighNumber != null)
            {
                this.m_allianceId = new LogicLong(allianceIdHighNumber.GetIntValue(), jsonObject.GetJSONNumber("alliance_id_lo").GetIntValue());
                this.m_allianceName = jsonObject.GetJSONString("alliance_name").GetStringValue();
                this.m_allianceBadgeId = jsonObject.GetJSONNumber("alliance_badge").GetIntValue();
                this.m_allianceExpLevel = jsonObject.GetJSONNumber("alliance_xp_lvl").GetIntValue();
            }

            this.m_accountId = new LogicLong(jsonObject.GetJSONNumber("acc_id_hi").GetIntValue(), jsonObject.GetJSONNumber("acc_id_lo").GetIntValue());
            this.m_avatarId = new LogicLong(jsonObject.GetJSONNumber("avatar_id_hi").GetIntValue(), jsonObject.GetJSONNumber("avatar_id_lo").GetIntValue());

            LogicJSONNumber homeIdHighNumber = jsonObject.GetJSONNumber("home_id_hi");

            if (homeIdHighNumber != null)
            {
                this.m_homeId = new LogicLong(homeIdHighNumber.GetIntValue(), jsonObject.GetJSONNumber("home_id_lo").GetIntValue());
            }

            this.m_name = jsonObject.GetJSONString("name").GetStringValue();
            this.m_remainingSeconds = jsonObject.GetJSONNumber("remainingSecs").GetIntValue();
        }

        public virtual void Save(LogicJSONObject jsonObject)
        {
            if (this.m_allianceId != null)
            {
                jsonObject.Put("alliance_id_hi", new LogicJSONNumber(this.m_allianceId.GetHigherInt()));
                jsonObject.Put("alliance_id_lo", new LogicJSONNumber(this.m_allianceId.GetLowerInt()));
                jsonObject.Put("alliance_name", new LogicJSONString(this.m_allianceName));
                jsonObject.Put("alliance_badge", new LogicJSONNumber(this.m_allianceBadgeId));
                jsonObject.Put("alliance_xp_lvl", new LogicJSONNumber(this.m_allianceExpLevel));
            }

            jsonObject.Put("acc_id_hi", new LogicJSONNumber(this.m_accountId.GetHigherInt()));
            jsonObject.Put("acc_id_lo", new LogicJSONNumber(this.m_accountId.GetLowerInt()));
            jsonObject.Put("avatar_id_hi", new LogicJSONNumber(this.m_avatarId.GetHigherInt()));
            jsonObject.Put("avatar_id_lo", new LogicJSONNumber(this.m_avatarId.GetLowerInt()));

            if (this.m_homeId != null)
            {
                jsonObject.Put("home_id_hi", new LogicJSONNumber(this.m_homeId.GetHigherInt()));
                jsonObject.Put("home_id_lo", new LogicJSONNumber(this.m_homeId.GetLowerInt()));
            }

            jsonObject.Put("name", new LogicJSONString(this.m_name));
            jsonObject.Put("remainingSecs", new LogicJSONNumber(this.m_remainingSeconds));
        }
    }
}