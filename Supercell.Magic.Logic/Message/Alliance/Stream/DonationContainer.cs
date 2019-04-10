namespace Supercell.Magic.Logic.Message.Alliance.Stream
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class DonationContainer
    {
        private LogicLong m_avatarId;
        private LogicArrayList<LogicCombatItemData> m_donationData;
        private LogicArrayList<int> m_donationLevel;

        public DonationContainer()
        {
            this.m_avatarId = new LogicLong();
            this.m_donationData = new LogicArrayList<LogicCombatItemData>();
            this.m_donationLevel = new LogicArrayList<int>();
        }

        public DonationContainer(LogicLong avatarId)
        {
            this.m_avatarId = avatarId;
            this.m_donationData = new LogicArrayList<LogicCombatItemData>();
            this.m_donationLevel = new LogicArrayList<int>();
        }

        public void Destruct()
        {
            this.m_avatarId = null;
            this.m_donationData = null;
            this.m_donationLevel = null;
        }

        public void Decode(ByteStream stream)
        {
            this.m_avatarId = stream.ReadLong();

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicCombatItemData data = ByteStreamHelper.ReadDataReference(stream) as LogicCombatItemData;
                int level = stream.ReadInt();

                if (data != null)
                {
                    this.m_donationData.Add(data);
                    this.m_donationLevel.Add(level);
                }
                else
                {
                    Debugger.Error("DonationContainer::decode() character data is NULL");
                }
            }
        }

        public void Encode(ByteStream stream)
        {
            stream.WriteLong(this.m_avatarId);
            stream.WriteInt(this.m_donationData.Size());

            for (int i = 0; i < this.m_donationData.Size(); i++)
            {
                ByteStreamHelper.WriteDataReference(stream, this.m_donationData[i]);
                stream.WriteInt(this.m_donationLevel[i]);
            }
        }

        public LogicLong GetAvatarId()
        {
            return this.m_avatarId;
        }

        public int GetDonationLimit(int allianceLevel)
        {
            if (allianceLevel > 0)
            {
                LogicAllianceLevelData allianceLevelData = LogicDataTables.GetAllianceLevel(allianceLevel);

                if (allianceLevelData != null)
                {
                    return allianceLevelData.GetTroopDonationLimit();
                }
            }

            return LogicDataTables.GetGlobals().GetMaxTroopDonationCount();
        }

        public bool CanAddUnit(LogicCombatItemData data, int allianceLevel)
        {
            int donationCount = 0;

            for (int i = 0; i < this.m_donationData.Size(); i++)
            {
                if (this.m_donationData[i].GetCombatItemType() == data.GetCombatItemType())
                {
                    ++donationCount;
                }
            }

            return donationCount < (data.GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_SPELL
                       ? LogicDataTables.GetGlobals().GetMaxSpellDonationCount()
                       : this.GetDonationLimit(allianceLevel));
        }

        public bool IsDonationLimitReached(int allianceLevel)
        {
            int totalUnitDonation = 0;
            int totalSpellDonation = 0;

            for (int i = 0; i < this.m_donationData.Size(); i++)
            {
                if (this.m_donationData[i].GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
                {
                    ++totalUnitDonation;
                }
            }

            if (totalUnitDonation >= this.GetDonationLimit(allianceLevel))
            {
                for (int i = 0; i < this.m_donationData.Size(); i++)
                {
                    if (this.m_donationData[i].GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_SPELL)
                    {
                        ++totalSpellDonation;
                    }
                }

                return totalSpellDonation >= LogicDataTables.GetGlobals().GetMaxSpellDonationCount();
            }

            return false;
        }

        public void AddUnit(LogicCombatItemData data, int upgLevel)
        {
            if (data != null)
            {
                if (upgLevel < 0)
                {
                    int idx = this.m_donationData.IndexOf(data);

                    if (idx != -1)
                    {
                        upgLevel = this.m_donationLevel[idx];
                    }
                }

                this.m_donationData.Add(data);
                this.m_donationLevel.Add(upgLevel);
            }
        }

        public int GetUnitLevel(LogicCombatItemData data)
        {
            int upgLevel = -1;
            int idx = this.m_donationData.IndexOf(data);

            if (idx != -1)
            {
                upgLevel = this.m_donationLevel[idx];
            }

            return upgLevel;
        }

        public void RemoveUnit(LogicCombatItemData data, int upgLevel)
        {
            if (data != null)
            {
                int idx = -1;

                for (int i = 0; i < this.m_donationData.Size(); i++)
                {
                    if (this.m_donationData[i] == data && this.m_donationLevel[i] == upgLevel)
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx != -1)
                {
                    this.m_donationData.Remove(idx);
                    this.m_donationLevel.Remove(idx);
                }
            }
        }

        public void RemoveUnit(LogicCombatItemData data)
        {
            if (data != null)
            {
                int idx = this.m_donationData.IndexOf(data);

                if (idx != -1)
                {
                    this.m_donationData.Remove(idx);
                    this.m_donationLevel.Remove(idx);
                }
            }
        }

        public void RemoveLastUnit(LogicCombatItemData data)
        {
            if (data != null)
            {
                int idx = -1;

                for (int i = this.m_donationData.Size() - 1; i >= 0; i--)
                {
                    if (this.m_donationData[i] == data)
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx != -1)
                {
                    this.m_donationData.Remove(idx);
                    this.m_donationLevel.Remove(idx);
                }
            }
        }

        public void SetUnitLevel(LogicCombatItemData data, int level)
        {
            int idx = this.m_donationData.IndexOf(data);

            if (idx != -1)
            {
                this.m_donationLevel[idx] = level;
            }
        }

        public int GetDonationCount()
        {
            return this.m_donationData.Size();
        }

        public int GetTotalDonationCount(int unitType)
        {
            int count = 0;

            for (int i = 0; i < this.m_donationData.Size(); i++)
            {
                if (this.m_donationData[i].GetCombatItemType() == unitType)
                {
                    ++count;
                }
            }

            return count;
        }

        public int GetDonationCount(LogicCombatItemData data)
        {
            int count = 0;

            for (int i = 0; i < this.m_donationData.Size(); i++)
            {
                if (this.m_donationData[i] == data)
                {
                    ++count;
                }
            }

            return count;
        }

        public int GetDonationCapacity(int combatItemType)
        {
            int count = 0;

            for (int i = 0; i < this.m_donationData.Size(); i++)
            {
                if (this.m_donationData[i].GetCombatItemType() == combatItemType)
                {
                    count += this.m_donationData[i].GetHousingSpace();
                }
            }

            return count;
        }

        public int GetXPReward()
        {
            int count = 0;

            for (int i = 0; i < this.m_donationData.Size(); i++)
            {
                if (this.m_donationData[i].GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
                {
                    count += ((LogicCharacterData) this.m_donationData[i]).GetDonateXP();
                }
                else
                {
                    count += LogicDataTables.GetGlobals().GetDarkSpellDonationXP() * this.m_donationData[i].GetHousingSpace();
                }
            }

            return count;
        }

        public LogicCombatItemData GetDonationType(int idx)
        {
            return this.m_donationData[idx];
        }

        public int GetDonationLevel(int idx)
        {
            return this.m_donationLevel[idx];
        }

        public LogicJSONObject Save()
        {
            LogicJSONObject jsonObject = new LogicJSONObject();

            jsonObject.Put("avatar_id_high", new LogicJSONNumber(this.m_avatarId.GetHigherInt()));
            jsonObject.Put("avatar_id_low", new LogicJSONNumber(this.m_avatarId.GetLowerInt()));

            LogicJSONArray donationArray = new LogicJSONArray(this.m_donationData.Size());

            for (int i = 0; i < this.m_donationData.Size(); i++)
            {
                LogicJSONArray array = new LogicJSONArray();

                array.Add(new LogicJSONNumber(this.m_donationData[i].GetGlobalID()));
                array.Add(new LogicJSONNumber(this.m_donationLevel[i]));

                donationArray.Add(array);
            }

            jsonObject.Put("donations", donationArray);

            return jsonObject;
        }

        public void Load(LogicJSONObject jsonObject)
        {
            LogicJSONNumber avatarIdHighObject = jsonObject.GetJSONNumber("avatar_id_high");
            LogicJSONNumber avatarIdLowObject = jsonObject.GetJSONNumber("avatar_id_low");

            if (avatarIdHighObject != null && avatarIdLowObject != null)
            {
                this.m_avatarId = new LogicLong(avatarIdHighObject.GetIntValue(), avatarIdLowObject.GetIntValue());
            }

            LogicJSONArray donationArray = jsonObject.GetJSONArray("donations");

            if (donationArray != null)
            {
                for (int i = 0; i < donationArray.Size(); i++)
                {
                    LogicJSONArray array = donationArray.GetJSONArray(i);

                    if (array != null && array.Size() == 2)
                    {
                        LogicData data = LogicDataTables.GetDataById(array.GetJSONNumber(0).GetIntValue());

                        if (data != null)
                        {
                            this.m_donationData.Add((LogicCombatItemData) data);
                            this.m_donationLevel.Add(array.GetJSONNumber(1).GetIntValue());
                        }
                    }
                }
            }
        }
    }
}