namespace Supercell.Magic.Logic.Message.Alliance.Stream
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class DonateStreamEntry : StreamEntry
    {
        private int m_castleLevel;
        private int m_castleUsedCapacity;
        private int m_castleUsedSpellCapacity;
        private int m_castleTotalCapacity;
        private int m_castleTotalSpellCapacity;
        private int m_donationPendingRequestCount;

        private string m_message;

        private LogicArrayList<LogicUnitSlot> m_unitCount;
        private LogicArrayList<DonationContainer> m_donationContainerList;

        public DonateStreamEntry()
        {
            this.m_donationContainerList = new LogicArrayList<DonationContainer>();
        }

        public override void Destruct()
        {
            base.Destruct();

            if (this.m_donationContainerList != null)
            {
                for (int i = this.m_donationContainerList.Size() - 1; i >= 0; i--)
                {
                    this.m_donationContainerList[i].Destruct();
                    this.m_donationContainerList.Remove(i);
                }

                this.m_donationContainerList = null;
            }

            if (this.m_unitCount != null)
            {
                for (int i = this.m_unitCount.Size() - 1; i >= 0; i--)
                {
                    this.m_unitCount[i].Destruct();
                    this.m_unitCount.Remove(i);
                }

                this.m_unitCount = null;
            }

            this.m_message = null;
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_castleLevel = stream.ReadInt();
            this.m_castleTotalCapacity = stream.ReadInt();
            this.m_castleTotalSpellCapacity = stream.ReadInt();
            this.m_castleUsedCapacity = stream.ReadInt();
            this.m_castleUsedSpellCapacity = stream.ReadInt();

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                DonationContainer donationContainer = new DonationContainer();
                donationContainer.Decode(stream);
                this.m_donationContainerList.Add(donationContainer);
            }

            if (stream.ReadBoolean())
            {
                this.m_message = stream.ReadString(900000);
            }

            int count = stream.ReadInt();

            if (count > -1)
            {
                this.m_unitCount = new LogicArrayList<LogicUnitSlot>(count);

                for (int i = 0; i < count; i++)
                {
                    LogicUnitSlot unitSlot = new LogicUnitSlot(null, -1, 0);
                    unitSlot.Decode(stream);
                    this.m_unitCount.Add(unitSlot);
                }
            }
        }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

            stream.WriteInt(this.m_castleLevel);
            stream.WriteInt(this.m_castleTotalCapacity);
            stream.WriteInt(this.m_castleTotalSpellCapacity);
            stream.WriteInt(this.m_castleUsedCapacity);
            stream.WriteInt(this.m_castleUsedSpellCapacity);
            stream.WriteInt(this.m_donationContainerList.Size());

            for (int i = 0; i < this.m_donationContainerList.Size(); i++)
            {
                this.m_donationContainerList[i].Encode(stream);
            }

            if (this.m_message != null)
            {
                stream.WriteBoolean(true);
                stream.WriteString(this.m_message);
            }
            else
            {
                stream.WriteBoolean(false);
            }

            if (this.m_unitCount != null)
            {
                stream.WriteInt(this.m_unitCount.Size());

                for (int i = 0; i < this.m_unitCount.Size(); i++)
                {
                    this.m_unitCount[i].Encode(stream);
                }
            }
            else
            {
                stream.WriteInt(-1);
            }
        }

        public int GetDonationPendingRequestCount()
        {
            return this.m_donationPendingRequestCount;
        }

        public void SetDonationPendingRequestCount(int count)
        {
            this.m_donationPendingRequestCount = count;
        }

        public int GetCastleLevel()
        {
            return this.m_castleLevel;
        }

        public void SetCasteLevel(int castleLevel, int castleUsedCapacity, int castleUsedSpellCapacity, int castleTotalCapacity, int castleTotalSpellCapacity)
        {
            this.m_castleLevel = castleLevel;
            this.m_castleUsedCapacity = castleUsedCapacity;
            this.m_castleUsedSpellCapacity = castleUsedSpellCapacity;
            this.m_castleTotalCapacity = castleTotalCapacity;
            this.m_castleTotalSpellCapacity = castleTotalSpellCapacity;
        }

        public int GetCastleTotalCapacity(int unitType)
        {
            return unitType == 1 ? this.m_castleTotalSpellCapacity : this.m_castleTotalCapacity;
        }

        public int GetCastleUsedCapacity(int unitType)
        {
            return (unitType == 1 ? this.m_castleUsedSpellCapacity : this.m_castleUsedCapacity) +
                   LogicDonationHelper.GetTotalDonationCapacity(this.m_donationContainerList, unitType);
        }

        public bool IsCastleFull()
        {
            return LogicDonationHelper.GetTotalDonationCapacity(this.m_donationContainerList, 0) + this.m_castleUsedCapacity >= this.m_castleTotalCapacity &&
                   LogicDonationHelper.GetTotalDonationCapacity(this.m_donationContainerList, 1) + this.m_castleUsedSpellCapacity >= this.m_castleTotalSpellCapacity;
        }

        public int GetTotalDonateCount(LogicLong avatarId, int unitType)
        {
            return LogicDonationHelper.GetTotalDonateCount(this.m_donationContainerList, avatarId, unitType);
        }

        public int GetDonateCount(LogicLong avatarId, LogicCombatItemData data)
        {
            return LogicDonationHelper.GetDonateCount(this.m_donationContainerList, avatarId, data);
        }

        public int GetTotalDonationCapacity(int unitType)
        {
            return LogicDonationHelper.GetTotalDonationCapacity(this.m_donationContainerList, unitType);
        }

        public bool CanDonateAnything(LogicLong avatarId, int allianceLevel, bool includeDarkSpell)
        {
            if (!LogicLong.Equals(avatarId, this.GetSenderAvatarId()))
            {
                if (!this.IsCastleFull())
                {
                    int totalTroopDonation = this.GetTotalDonateCount(avatarId, 0);
                    int totalSpellDonation = this.GetTotalDonateCount(avatarId, 1);
                    int freeSpellCapacity = this.m_castleTotalSpellCapacity - this.m_castleUsedSpellCapacity - totalSpellDonation;
                    int maxTroopDonation = LogicDonationHelper.GetMaxUnitDonationCount(allianceLevel, 0);
                    int maxSpellDonation = LogicDonationHelper.GetMaxUnitDonationCount(allianceLevel, 1);

                    if (maxTroopDonation == totalTroopDonation && maxSpellDonation == totalSpellDonation)
                    {
                        return false;
                    }

                    if (!includeDarkSpell && freeSpellCapacity < 2 && this.m_castleTotalCapacity ==
                        this.m_castleUsedSpellCapacity + LogicDonationHelper.GetTotalDonationCapacity(this.m_donationContainerList, 0))
                    {
                        return false;
                    }

                    return LogicDonationHelper.CanDonateAnything(this.m_donationContainerList, avatarId, allianceLevel);
                }
            }

            return false;
        }

        public bool CanAddDonation(LogicLong avatarId, LogicCombatItemData data, int allianceLevel)
        {
            if (!LogicLong.Equals(avatarId, this.GetSenderAvatarId()))
            {
                if (data.GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
                {
                    if (data.GetHousingSpace() + this.m_castleUsedCapacity + LogicDonationHelper.GetTotalDonationCapacity(this.m_donationContainerList, 0) >
                        this.m_castleTotalCapacity)
                    {
                        return false;
                    }
                }
                else
                {
                    if (this.m_castleTotalSpellCapacity == 0 ||
                        data.GetHousingSpace() + this.m_castleUsedSpellCapacity + LogicDonationHelper.GetTotalDonationCapacity(this.m_donationContainerList, 1) >
                        this.m_castleTotalSpellCapacity)
                    {
                        return false;
                    }
                }

                return LogicDonationHelper.CanAddDonation(this.m_donationContainerList, avatarId, data, allianceLevel);
            }

            return false;
        }

        public void AddDonation(LogicLong avatarId, LogicCombatItemData data, int upgLevel)
        {
            LogicDonationHelper.AddDonation(this.m_donationContainerList, avatarId, data, upgLevel);
        }

        public void RemoveDonation(LogicLong avatarId, LogicCombatItemData data, int upgLevel)
        {
            LogicDonationHelper.RemoveDonation(this.m_donationContainerList, avatarId, data, upgLevel);
        }

        public string GetMessage()
        {
            return this.m_message;
        }

        public void SetMessage(string message)
        {
            this.m_message = message;
        }

        public int GetUnitTypeCount()
        {
            if (this.m_unitCount != null)
            {
                return this.m_unitCount.Size();
            }

            return 0;
        }

        public LogicCombatItemData GetUnitType(int idx)
        {
            return (LogicCombatItemData) this.m_unitCount[idx].GetData();
        }

        public LogicArrayList<LogicUnitSlot> GetUnits()
        {
            return this.m_unitCount;
        }

        public void SetUnits(LogicArrayList<LogicUnitSlot> slot)
        {
            this.m_unitCount = slot;
        }

        public int GetXPReward(LogicLong avatarId)
        {
            for (int i = 0; i < this.m_donationContainerList.Size(); i++)
            {
                if (LogicLong.Equals(this.m_donationContainerList[i].GetAvatarId(), avatarId))
                {
                    return this.m_donationContainerList[i].GetXPReward();
                }
            }

            return 0;
        }

        public DonationContainer GetDonationContainer(LogicLong avatarId)
        {
            for (int i = 0; i < this.m_donationContainerList.Size(); i++)
            {
                if (LogicLong.Equals(this.m_donationContainerList[i].GetAvatarId(), avatarId))
                {
                    return this.m_donationContainerList[i];
                }
            }

            return null;
        }

        public override StreamEntryType GetStreamEntryType()
        {
            return StreamEntryType.DONATE;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = jsonObject.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("ChatStreamEntry::load base is NULL");
            }

            base.Load(baseObject);

            this.m_castleLevel = LogicJSONHelper.GetInt(jsonObject, "castle_level");
            this.m_castleUsedCapacity = LogicJSONHelper.GetInt(jsonObject, "castle_used");
            this.m_castleUsedSpellCapacity = LogicJSONHelper.GetInt(jsonObject, "castle_sp_used");
            this.m_castleTotalCapacity = LogicJSONHelper.GetInt(jsonObject, "castle_total");
            this.m_castleTotalSpellCapacity = LogicJSONHelper.GetInt(jsonObject, "castle_sp_total");

            LogicJSONString messageObject = jsonObject.GetJSONString("message");

            if (messageObject != null)
            {
                this.m_message = messageObject.GetStringValue();
            }

            LogicJSONArray donationArray = jsonObject.GetJSONArray("donators");

            if (donationArray != null)
            {
                for (int i = 0; i < donationArray.Size(); i++)
                {
                    DonationContainer donationContainer = new DonationContainer();
                    donationContainer.Load(donationArray.GetJSONObject(i));
                    this.m_donationContainerList.Add(donationContainer);
                }
            }

            LogicJSONArray unitArray = jsonObject.GetJSONArray("units");

            if (unitArray != null)
            {
                this.m_unitCount = new LogicArrayList<LogicUnitSlot>();

                for (int i = 0; i < unitArray.Size(); i++)
                {
                    LogicUnitSlot unitSlot = new LogicUnitSlot(null, -1, 0);
                    unitSlot.ReadFromJSON(unitArray.GetJSONObject(i));
                    this.m_unitCount.Add(unitSlot);
                }
            }
        }

        public override void Save(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = new LogicJSONObject();

            base.Save(baseObject);

            jsonObject.Put("base", baseObject);
            jsonObject.Put("castle_level", new LogicJSONNumber(this.m_castleLevel));
            jsonObject.Put("castle_used", new LogicJSONNumber(this.m_castleUsedCapacity));
            jsonObject.Put("castle_sp_used", new LogicJSONNumber(this.m_castleUsedSpellCapacity));
            jsonObject.Put("castle_total", new LogicJSONNumber(this.m_castleTotalCapacity));
            jsonObject.Put("castle_sp_total", new LogicJSONNumber(this.m_castleTotalSpellCapacity));

            if (this.m_message != null)
            {
                jsonObject.Put("message", new LogicJSONString(this.m_message));
            }

            LogicJSONArray donationArray = new LogicJSONArray();

            for (int i = 0; i < this.m_donationContainerList.Size(); i++)
            {
                donationArray.Add(this.m_donationContainerList[i].Save());
            }

            jsonObject.Put("donators", donationArray);

            if (this.m_unitCount != null)
            {
                LogicJSONArray unitArray = new LogicJSONArray();

                for (int i = 0; i < this.m_unitCount.Size(); i++)
                {
                    LogicJSONObject obj = new LogicJSONObject();
                    this.m_unitCount[i].WriteToJSON(obj);
                    unitArray.Add(obj);
                }

                jsonObject.Put("units", unitArray);
            }
        }
    }
}