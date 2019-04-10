namespace Supercell.Magic.Logic.Avatar
{
    using Supercell.Magic.Logic.Avatar.Change;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public abstract class LogicAvatar
    {
        protected LogicAvatarChangeListener m_listener;

        protected int m_townHallLevel;
        protected int m_townHallLevelVillage2;
        protected int m_allianceCastleLevel;
        protected int m_allianceCastleTotalCapacity;
        protected int m_allianceCastleUsedCapacity;
        protected int m_allianceCastleTotalSpellCapacity;
        protected int m_allianceCastleUsedSpellCapacity;
        protected int m_allianceUnitVisitCapacity;
        protected int m_allianceUnitSpellVisitCapacity;
        protected int m_allianceBadgeId;
        protected int m_leagueType;
        protected int m_redPackageState;

        protected LogicLevel m_level;

        protected LogicArrayList<LogicDataSlot> m_resourceCount;
        protected LogicArrayList<LogicDataSlot> m_resourceCap;
        protected LogicArrayList<LogicDataSlot> m_unitCount;
        protected LogicArrayList<LogicDataSlot> m_spellCount;
        protected LogicArrayList<LogicDataSlot> m_unitUpgrade;
        protected LogicArrayList<LogicDataSlot> m_spellUpgrade;
        protected LogicArrayList<LogicDataSlot> m_heroUpgrade;
        protected LogicArrayList<LogicDataSlot> m_heroHealth;
        protected LogicArrayList<LogicDataSlot> m_heroState;
        protected LogicArrayList<LogicDataSlot> m_heroMode;
        protected LogicArrayList<LogicDataSlot> m_unitCountVillage2;
        protected LogicArrayList<LogicDataSlot> m_unitCountNewVillage2;
        protected LogicArrayList<LogicDataSlot> m_achievementProgress;
        protected LogicArrayList<LogicDataSlot> m_lootedNpcGold;
        protected LogicArrayList<LogicDataSlot> m_lootedNpcElixir;
        protected LogicArrayList<LogicDataSlot> m_npcStars;
        protected LogicArrayList<LogicDataSlot> m_variables;
        protected LogicArrayList<LogicDataSlot> m_unitPreset1;
        protected LogicArrayList<LogicDataSlot> m_unitPreset2;
        protected LogicArrayList<LogicDataSlot> m_unitPreset3;
        protected LogicArrayList<LogicDataSlot> m_previousArmy;
        protected LogicArrayList<LogicDataSlot> m_eventUnitCounter;
        protected LogicArrayList<LogicDataSlot> m_freeActionCount;

        protected LogicArrayList<LogicUnitSlot> m_allianceUnitCount;

        protected LogicArrayList<LogicData> m_achievementRewardClaimed;
        protected LogicArrayList<LogicData> m_missionCompleted;

        public LogicAvatar()
        {
            this.m_allianceCastleLevel = -1;
        }

        public virtual void InitBase()
        {
            this.m_listener = new LogicAvatarChangeListener();
            this.m_resourceCount = new LogicArrayList<LogicDataSlot>();
            this.m_resourceCap = new LogicArrayList<LogicDataSlot>();
            this.m_unitCount = new LogicArrayList<LogicDataSlot>();
            this.m_spellCount = new LogicArrayList<LogicDataSlot>();
            this.m_unitUpgrade = new LogicArrayList<LogicDataSlot>();
            this.m_spellUpgrade = new LogicArrayList<LogicDataSlot>();
            this.m_heroUpgrade = new LogicArrayList<LogicDataSlot>();
            this.m_heroHealth = new LogicArrayList<LogicDataSlot>();
            this.m_heroState = new LogicArrayList<LogicDataSlot>();
            this.m_heroMode = new LogicArrayList<LogicDataSlot>();
            this.m_unitCountVillage2 = new LogicArrayList<LogicDataSlot>();
            this.m_unitCountNewVillage2 = new LogicArrayList<LogicDataSlot>();
            this.m_achievementProgress = new LogicArrayList<LogicDataSlot>();
            this.m_lootedNpcGold = new LogicArrayList<LogicDataSlot>();
            this.m_lootedNpcElixir = new LogicArrayList<LogicDataSlot>();
            this.m_npcStars = new LogicArrayList<LogicDataSlot>();
            this.m_variables = new LogicArrayList<LogicDataSlot>();
            this.m_unitPreset1 = new LogicArrayList<LogicDataSlot>();
            this.m_unitPreset2 = new LogicArrayList<LogicDataSlot>();
            this.m_unitPreset3 = new LogicArrayList<LogicDataSlot>();
            this.m_previousArmy = new LogicArrayList<LogicDataSlot>();
            this.m_eventUnitCounter = new LogicArrayList<LogicDataSlot>();
            this.m_allianceUnitCount = new LogicArrayList<LogicUnitSlot>();
            this.m_achievementRewardClaimed = new LogicArrayList<LogicData>();
            this.m_missionCompleted = new LogicArrayList<LogicData>();
            this.m_freeActionCount = new LogicArrayList<LogicDataSlot>();
        }

        public virtual void Destruct()
        {
            if (this.m_listener != null)
            {
                this.m_listener.Destruct();
                this.m_listener = null;
            }

            if (this.m_resourceCap != null)
            {
                this.ClearDataSlotArray(this.m_resourceCap);
                this.m_resourceCap = null;
            }

            if (this.m_unitCount != null)
            {
                this.ClearDataSlotArray(this.m_unitCount);
                this.m_unitCount = null;
            }

            if (this.m_spellCount != null)
            {
                this.ClearDataSlotArray(this.m_spellCount);
                this.m_spellCount = null;
            }

            if (this.m_unitUpgrade != null)
            {
                this.ClearDataSlotArray(this.m_unitUpgrade);
                this.m_unitUpgrade = null;
            }

            if (this.m_spellUpgrade != null)
            {
                this.ClearDataSlotArray(this.m_spellUpgrade);
                this.m_spellUpgrade = null;
            }

            if (this.m_heroUpgrade != null)
            {
                this.ClearDataSlotArray(this.m_heroUpgrade);
                this.m_heroUpgrade = null;
            }

            if (this.m_heroHealth != null)
            {
                this.ClearDataSlotArray(this.m_heroHealth);
                this.m_heroHealth = null;
            }

            if (this.m_heroState != null)
            {
                this.ClearDataSlotArray(this.m_heroState);
                this.m_heroState = null;
            }

            if (this.m_allianceUnitCount != null)
            {
                this.ClearUnitSlotArray(this.m_allianceUnitCount);
                this.m_allianceUnitCount = null;
            }

            if (this.m_achievementProgress != null)
            {
                this.ClearDataSlotArray(this.m_achievementProgress);
                this.m_achievementProgress = null;
            }

            if (this.m_npcStars != null)
            {
                this.ClearDataSlotArray(this.m_npcStars);
                this.m_npcStars = null;
            }

            if (this.m_lootedNpcGold != null)
            {
                this.ClearDataSlotArray(this.m_lootedNpcGold);
                this.m_lootedNpcGold = null;
            }

            if (this.m_lootedNpcElixir != null)
            {
                this.ClearDataSlotArray(this.m_lootedNpcElixir);
                this.m_lootedNpcElixir = null;
            }

            if (this.m_heroMode != null)
            {
                this.ClearDataSlotArray(this.m_heroMode);
                this.m_heroMode = null;
            }

            if (this.m_variables != null)
            {
                this.ClearDataSlotArray(this.m_variables);
                this.m_variables = null;
            }

            if (this.m_unitPreset1 != null)
            {
                this.ClearDataSlotArray(this.m_unitPreset1);
                this.m_unitPreset1 = null;
            }

            if (this.m_unitPreset2 != null)
            {
                this.ClearDataSlotArray(this.m_unitPreset2);
                this.m_unitPreset2 = null;
            }

            if (this.m_unitPreset3 != null)
            {
                this.ClearDataSlotArray(this.m_unitPreset3);
                this.m_unitPreset3 = null;
            }

            if (this.m_previousArmy != null)
            {
                this.ClearDataSlotArray(this.m_previousArmy);
                this.m_previousArmy = null;
            }

            if (this.m_eventUnitCounter != null)
            {
                this.ClearDataSlotArray(this.m_eventUnitCounter);
                this.m_eventUnitCounter = null;
            }

            if (this.m_unitCountVillage2 != null)
            {
                this.ClearDataSlotArray(this.m_unitCountVillage2);
                this.m_unitCountVillage2 = null;
            }

            if (this.m_unitCountNewVillage2 != null)
            {
                this.ClearDataSlotArray(this.m_unitCountNewVillage2);
                this.m_unitCountNewVillage2 = null;
            }
        }

        public LogicAvatarChangeListener GetChangeListener()
        {
            return this.m_listener;
        }

        public void SetChangeListener(LogicAvatarChangeListener listener)
        {
            this.m_listener = listener;
        }

        public void SetLevel(LogicLevel level)
        {
            this.m_level = level;
        }

        public virtual bool IsClientAvatar()
        {
            return false;
        }

        public virtual bool IsNpcAvatar()
        {
            return false;
        }

        public virtual void GetChecksum(ChecksumHelper checksumHelper)
        {
            checksumHelper.StartObject("LogicAvatar");
            checksumHelper.StartArray("m_pResourceCount");

            for (int i = 0; i < this.m_resourceCount.Size(); i++)
            {
                this.m_resourceCount[i].GetChecksum(checksumHelper);
            }

            checksumHelper.EndArray();
            checksumHelper.StartArray("m_pResourceCap");

            for (int i = 0; i < this.m_resourceCap.Size(); i++)
            {
                this.m_resourceCap[i].GetChecksum(checksumHelper);
            }

            checksumHelper.EndArray();
            checksumHelper.StartArray("m_pUnitCount");

            for (int i = 0; i < this.m_unitCount.Size(); i++)
            {
                this.m_unitCount[i].GetChecksum(checksumHelper);
            }

            checksumHelper.EndArray();
            checksumHelper.StartArray("m_pSpellCount");

            for (int i = 0; i < this.m_spellCount.Size(); i++)
            {
                this.m_spellCount[i].GetChecksum(checksumHelper);
            }

            checksumHelper.EndArray();
            checksumHelper.StartArray("m_pAllianceUnitCount");

            for (int i = 0; i < this.m_allianceUnitCount.Size(); i++)
            {
                this.m_allianceUnitCount[i].GetChecksum(checksumHelper);
            }

            checksumHelper.EndArray();
            checksumHelper.StartArray("m_pUnitUpgrade");

            for (int i = 0; i < this.m_unitUpgrade.Size(); i++)
            {
                this.m_unitUpgrade[i].GetChecksum(checksumHelper);
            }

            checksumHelper.EndArray();
            checksumHelper.StartArray("m_pSpellUpgrade");

            for (int i = 0; i < this.m_spellUpgrade.Size(); i++)
            {
                this.m_spellUpgrade[i].GetChecksum(checksumHelper);
            }

            checksumHelper.EndArray();
            checksumHelper.StartArray("m_pUnitCountVillage2");

            for (int i = 0; i < this.m_unitCountVillage2.Size(); i++)
            {
                this.m_unitCountVillage2[i].GetChecksum(checksumHelper);
            }

            checksumHelper.EndArray();
            checksumHelper.WriteValue("m_townHallLevel", this.m_townHallLevel);
            checksumHelper.WriteValue("m_townHallLevelVillage2", this.m_townHallLevelVillage2);
            checksumHelper.EndObject();
        }

        public void CommodityCountChangeHelper(int commodityType, LogicData data, int count)
        {
            switch (data.GetDataType())
            {
                case LogicDataType.RESOURCE:
                {
                    switch (commodityType)
                    {
                        case 0:
                        {
                            int resourceCount = this.GetResourceCount((LogicResourceData) data);
                            int newResourceCount = LogicMath.Max(resourceCount + count, 0);

                            if (count > 0)
                            {
                                int resourceCap = this.GetResourceCap((LogicResourceData) data);

                                if (newResourceCount > resourceCap)
                                {
                                    newResourceCount = resourceCap;
                                }

                                if (resourceCount >= resourceCap)
                                {
                                    newResourceCount = resourceCap;
                                }
                            }

                            this.SetResourceCount((LogicResourceData) data, newResourceCount);
                            this.m_listener.CommodityCountChanged(0, data, newResourceCount);
                            break;
                        }
                        case 1:
                        {
                            int newCount = this.GetResourceCap((LogicResourceData) data) + count;

                            this.SetResourceCap((LogicResourceData) data, newCount);
                            this.m_listener.CommodityCountChanged(1, data, newCount);
                            break;
                        }
                    }

                    break;
                }

                case LogicDataType.CHARACTER:
                case LogicDataType.SPELL:
                    switch (commodityType)
                    {
                        case 0:
                        {
                            int newCount = LogicMath.Max(this.GetUnitCount((LogicCombatItemData) data) + count, 0);

                            this.SetUnitCount((LogicCombatItemData) data, newCount);
                            this.m_listener.CommodityCountChanged(0, data, newCount);
                            break;
                        }
                        case 1:
                        {
                            LogicCombatItemData combatItemData = (LogicCombatItemData) data;

                            int newCount = LogicMath.Clamp(this.GetUnitUpgradeLevel((LogicCombatItemData) data) + count, 0, combatItemData.GetUpgradeLevelCount() - 1);

                            this.SetUnitUpgradeLevel((LogicCombatItemData) data, newCount);
                            this.m_listener.CommodityCountChanged(1, data, newCount);
                            break;
                        }
                        case 7:
                        {
                            int newCount = LogicMath.Max(this.GetUnitCountVillage2((LogicCombatItemData) data) + count, 0);

                            this.SetUnitCountVillage2((LogicCombatItemData) data, newCount);
                            this.m_listener.CommodityCountChanged(7, data, newCount);
                            break;
                        }
                        case 8:
                        {
                            int newCount = LogicMath.Max(this.GetUnitCountNewVillage2((LogicCombatItemData) data) + count, 0);

                            this.SetUnitCountNewVillage2((LogicCombatItemData) data, newCount);
                            this.m_listener.CommodityCountChanged(8, data, newCount);
                            break;
                        }
                        case 9:
                        {
                            int newCount = LogicMath.Max(this.GetFreeActionCount(data) + count, 0);

                            this.SetFreeActionCount(data, newCount);
                            this.m_listener.CommodityCountChanged(9, data, newCount);
                            break;
                        }
                    }

                    break;

                case LogicDataType.NPC:
                    switch (commodityType)
                    {
                        case 0:
                        {
                            int newCount = this.GetNpcStars((LogicNpcData) data) + count;

                            this.SetNpcStars((LogicNpcData) data, newCount);
                            this.m_listener.CommodityCountChanged(0, data, newCount);
                            break;
                        }
                        case 1:
                        {
                            int newCount = this.GetLootedNpcGold((LogicNpcData) data) + count;

                            this.SetLootedNpcGold((LogicNpcData) data, newCount);
                            this.m_listener.CommodityCountChanged(1, data, newCount);
                            break;
                        }
                        case 2:
                        {
                            int newCount = this.GetLootedNpcElixir((LogicNpcData) data) + count;

                            this.SetLootedNpcElixir((LogicNpcData) data, newCount);
                            this.m_listener.CommodityCountChanged(2, data, newCount);
                            break;
                        }
                    }

                    break;
                case LogicDataType.ACHIEVEMENT:
                    int tmp = LogicMath.Max(this.GetAchievementProgress((LogicAchievementData) data) + count, 0);

                    this.SetAchievementProgress((LogicAchievementData) data, tmp);
                    this.m_listener.CommodityCountChanged(0, data, tmp);
                    break;
                case LogicDataType.HERO:
                    LogicHeroData heroData = (LogicHeroData) data;

                    switch (commodityType)
                    {
                        case 0:
                        {
                            int newCount = LogicMath.Clamp(this.GetHeroHealth(heroData) + count, 0, heroData.GetFullRegenerationTimeSec(this.GetUnitUpgradeLevel(heroData)));

                            this.SetHeroHealth(heroData, newCount);
                            this.GetChangeListener().CommodityCountChanged(0, data, newCount);
                            break;
                        }
                        case 1:
                        {
                            int newCount = LogicMath.Clamp(this.GetUnitUpgradeLevel(heroData) + count, 0, heroData.GetUpgradeLevelCount() - 1);

                            this.SetUnitUpgradeLevel((LogicCombatItemData) data, newCount);
                            this.m_listener.CommodityCountChanged(1, data, newCount);
                            break;
                        }
                    }

                    break;
            }
        }

        public void SetCommodityCount(int commodityType, LogicData data, int count)
        {
            switch (data.GetDataType())
            {
                case LogicDataType.RESOURCE:
                    switch (commodityType)
                    {
                        case 0:
                            this.SetResourceCount((LogicResourceData) data, count);
                            break;
                        case 1:
                            this.SetResourceCap((LogicResourceData) data, count);
                            break;
                        default:
                            Debugger.Error("setCommodityCount - Unknown resource commodity");
                            break;
                    }

                    break;
                case LogicDataType.CHARACTER:
                case LogicDataType.SPELL:
                    switch (commodityType)
                    {
                        case 0:
                            this.SetUnitCount((LogicCombatItemData) data, count);
                            break;
                        case 1:
                            this.SetUnitUpgradeLevel((LogicCombatItemData) data, count);
                            break;
                        case 2:
                            this.SetUnitPresetCount((LogicCombatItemData) data, 0, count);
                            break;
                        case 3:
                            this.SetUnitPresetCount((LogicCombatItemData) data, 1, count);
                            break;
                        case 4:
                            this.SetUnitPresetCount((LogicCombatItemData) data, 2, count);
                            break;
                        case 5:
                            this.SetUnitPresetCount((LogicCombatItemData) data, 3, count);
                            break;
                        case 6:
                            this.SetEventUnitCounterCount((LogicCombatItemData) data, count);
                            break;
                        case 7:
                            this.SetUnitCountVillage2((LogicCombatItemData) data, count);
                            break;
                        case 8:
                            this.SetUnitCountNewVillage2((LogicCombatItemData) data, count);
                            break;
                        case 9:
                            this.SetFreeActionCount(data, count);
                            break;
                        default:
                            Debugger.Error("setCommodityCount - Unknown resource commodity");
                            break;
                    }

                    break;
                case LogicDataType.NPC:
                    switch (commodityType)
                    {
                        case 0:
                            this.SetNpcStars((LogicNpcData) data, count);
                            break;
                        case 1:
                            this.SetLootedNpcGold((LogicNpcData) data, count);
                            break;
                        case 2:
                            this.SetLootedNpcElixir((LogicNpcData) data, count);
                            break;
                    }

                    break;
                case LogicDataType.MISSION:
                    if (commodityType == 0)
                    {
                        this.SetMissionCompleted((LogicMissionData) data, count != 0);
                    }

                    break;
                case LogicDataType.ACHIEVEMENT:
                    switch (commodityType)
                    {
                        case 0:
                            this.SetAchievementProgress((LogicAchievementData) data, count);
                            break;
                        case 1:
                            this.SetAchievementRewardClaimed((LogicAchievementData) data, count != 0);
                            break;
                    }

                    break;
                case LogicDataType.HERO:
                    switch (commodityType)
                    {
                        case 0:
                            this.SetHeroHealth((LogicHeroData) data, count);
                            break;
                        case 1:
                            this.SetUnitUpgradeLevel((LogicHeroData) data, count);
                            break;
                        case 2:
                            this.SetHeroState((LogicHeroData) data, count);
                            break;
                        case 3:
                            this.SetHeroMode((LogicHeroData) data, count);
                            break;
                    }

                    break;
                case LogicDataType.VARIABLE:
                    if (commodityType == 0)
                    {
                        this.SetVariable(data, count);
                    }

                    break;
            }
        }

        public void ClearDataSlotArray(LogicArrayList<LogicDataSlot> dataSlotArray)
        {
            for (int i = 0; i < dataSlotArray.Size(); i++)
                dataSlotArray[i].Destruct();
            dataSlotArray.Clear();
        }

        public void ClearUnitSlotArray(LogicArrayList<LogicUnitSlot> unitSlotArray)
        {
            for (int i = 0; i < unitSlotArray.Size(); i++)
                unitSlotArray[i].Destruct();
            unitSlotArray.Clear();
        }

        public virtual bool AddDuelReward(int goldCount, int elixirCount, int bonusGoldCount, int bonusElixirCount, LogicLong matchId)
        {
            return false;
        }

        public virtual bool AddStarBonusReward(int goldCount, int elixirCount, int darkElixirCount)
        {
            return false;
        }

        public virtual bool AddWarReward(int gold, int elixir, int darkElixir, int unk, LogicLong warInstanceId)
        {
            return false;
        }

        public virtual void UpdateLootLimitCooldown()
        {
            // UpdateLootLimitCooldown.
        }

        public virtual void UpdateStarBonusLimitCooldown()
        {
            // UpdateStarBonusLimitCooldown.
        }

        public virtual void FastForwardLootLimit(int secs)
        {
            // FastForwardLootLimit.
        }

        public virtual void FastForwardStarBonusLimit(int secs)
        {
            // FastForwardStarBonusLimit.
        }

        public virtual bool IsInAlliance()
        {
            return false;
        }

        public virtual LogicLong GetAllianceId()
        {
            return 0;
        }

        public virtual int GetAllianceBadgeId()
        {
            return 0;
        }

        public virtual LogicAvatarAllianceRole GetAllianceRole()
        {
            return LogicAvatarAllianceRole.MEMBER;
        }

        public virtual string GetAllianceName()
        {
            return null;
        }

        public virtual int GetExpLevel()
        {
            return 1;
        }


        public virtual string GetName()
        {
            return null;
        }

        public bool IsInExpLevelCap()
        {
            return this.GetExpLevel() >= LogicDataTables.GetExperienceLevelCount();
        }

        public bool IsHeroAvailableForAttack(LogicHeroData data)
        {
            if ((this.GetHeroState(data) & 0xFFFFFFFE) == 2)
            {
                int heroUpgLevel = this.GetUnitUpgradeLevel(data);
                int heroHealth = data.GetHeroHitpoints(this.GetHeroHealth(data), heroUpgLevel);

                return data.HasEnoughHealthForAttack(heroHealth, heroUpgLevel);
            }

            return false;
        }

        public int GetHeroHealth(LogicHeroData data)
        {
            int index = -1;

            for (int i = 0; i < this.m_heroHealth.Size(); i++)
            {
                if (this.m_heroHealth[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                return this.m_heroHealth[index].GetCount();
            }

            return 0;
        }

        public void SetHeroHealth(LogicHeroData data, int count)
        {
            int index = -1;

            for (int i = 0; i < this.m_heroHealth.Size(); i++)
            {
                if (this.m_heroHealth[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                this.m_heroHealth[index].SetCount(count);
            }
            else
            {
                this.m_heroHealth.Add(new LogicDataSlot(data, count));
            }
        }

        public bool FastForwardHeroHealth(LogicHeroData data, int secs)
        {
            int totalSecs = LogicMath.Max(0, secs);
            int health = this.GetHeroHealth(data);

            if (health != 0)
            {
                health = LogicMath.Max(0, health - totalSecs);

                this.SetHeroHealth(data, health);
                this.GetChangeListener().CommodityCountChanged(0, data, health);

                return health == 0;
            }

            return false;
        }

        public int GetFreeActionCount(LogicData data)
        {
            for (int i = 0; i < this.m_freeActionCount.Size(); i++)
            {
                if (this.m_freeActionCount[i].GetData() == data)
                    return this.m_freeActionCount[i].GetCount();
            }

            return 0;
        }

        public void SetFreeActionCount(LogicData data, int count)
        {
            int index = -1;

            for (int i = 0; i < this.m_achievementProgress.Size(); i++)
            {
                if (this.m_achievementProgress[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                this.m_freeActionCount[index].SetCount(count);
            }
            else
            {
                this.m_freeActionCount.Add(new LogicDataSlot(data, count));
            }
        }

        public void SetFreeUnitCount(LogicCombatItemData data, int count, LogicLevel level)
        {
            int cap = data.GetCombatItemType() == 2
                ? LogicDataTables.GetGlobals().GetFreeHeroHealthCap()
                : data.GetHousingSpace() + 2 * level.GetComponentManagerAt(data.GetVillageType()).GetTotalMaxHousing(data.GetCombatItemType()) *
                  LogicDataTables.GetGlobals().GetFreeUnitHousingCapPercentage() / (2 * data.GetHousingSpace());
            int currentCount = this.GetFreeActionCount(data);
            int newCount = LogicMath.Clamp(currentCount + count, 0, cap);

            if (newCount != currentCount)
            {
                this.SetFreeActionCount(data, newCount);
                this.m_listener.CommodityCountChanged(9, data, newCount);
            }
        }

        public int GetHeroState(LogicHeroData data)
        {
            int index = -1;

            for (int i = 0; i < this.m_heroState.Size(); i++)
            {
                if (this.m_heroState[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                return this.m_heroState[index].GetCount();
            }

            return 0;
        }

        public void SetHeroState(LogicHeroData data, int count)
        {
            int index = -1;

            for (int i = 0; i < this.m_heroState.Size(); i++)
            {
                if (this.m_heroState[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                this.m_heroState[index].SetCount(count);
            }
            else
            {
                this.m_heroState.Add(new LogicDataSlot(data, count));
            }
        }

        public int GetHeroMode(LogicHeroData data)
        {
            int index = -1;

            for (int i = 0; i < this.m_heroMode.Size(); i++)
            {
                if (this.m_heroMode[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                return this.m_heroMode[index].GetCount();
            }

            return 0;
        }

        public void SetHeroMode(LogicHeroData data, int count)
        {
            int index = -1;

            for (int i = 0; i < this.m_heroMode.Size(); i++)
            {
                if (this.m_heroMode[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                this.m_heroMode[index].SetCount(count);
            }
            else
            {
                this.m_heroMode.Add(new LogicDataSlot(data, count));
            }
        }

        public int GetHeroHealCost(LogicHeroData data)
        {
            return LogicGamePlayUtil.GetSpeedUpCost(this.GetHeroHealth(data), 2, data.GetVillageType());
        }

        public int GetVariable(LogicData data)
        {
            int index = -1;

            for (int i = 0; i < this.m_variables.Size(); i++)
            {
                if (this.m_variables[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                return this.m_variables[index].GetCount();
            }

            return 0;
        }

        public void SetVariable(LogicData data, int count)
        {
            int index = -1;

            for (int i = 0; i < this.m_variables.Size(); i++)
            {
                if (this.m_variables[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                this.m_variables[index].SetCount(count);
            }
            else
            {
                this.m_variables.Add(new LogicDataSlot(data, count));
            }

            this.m_listener.CommodityCountChanged(0, data, count);
        }

        public int GetVariableByName(string name)
        {
            LogicData data = LogicDataTables.GetVariableByName(name, null);

            if (data == null)
            {
                Debugger.Error("getVariableByName() Invalid Name " + name);
            }

            return this.GetVariable(data);
        }

        public void SetVariableByName(string name, int value)
        {
            LogicData data = LogicDataTables.GetVariableByName(name, null);

            if (data == null)
            {
                Debugger.Error("setVariableByName() Invalid Name " + name);
            }

            this.SetVariable(data, value);
        }

        public int GetVillageToGoTo()
        {
            return this.GetVariableByName("VillageToGoTo");
        }

        public void SetAccountBound()
        {
            this.SetVariableByName("AccountBound", 1);
        }

        public int GetVillage2BarrackLevel()
        {
            return this.GetVariableByName("Village2BarrackLevel");
        }

        public void SetVillage2BarrackLevel(int value)
        {
            this.SetVariableByName("Village2BarrackLevel", value);
        }

        public bool IsChallengeStarted()
        {
            return this.GetVariableByName("ChallengeStarted") != 0;
        }

        public int GetUnusedResourceCap(LogicResourceData data)
        {
            return LogicMath.Max(this.GetResourceCap(data) - this.GetResourceCount(data), 0);
        }

        public bool IsAccountBound()
        {
            return this.GetVariableByName("AccountBound") != 0;
        }

        public int GetSecondsSinceLastFriendListOpened()
        {
            return this.m_level.GetGameMode().GetStartTime() + this.m_level.GetLogicTime().GetTotalMS() / 1000 - this.GetVariableByName("FriendListLastOpened");
        }

        public void UpdateLastFriendListOpened()
        {
            this.SetVariableByName("FriendListLastOpened", this.m_level.GetGameMode().GetStartTime() + this.m_level.GetLogicTime().GetTotalMS() / 1000);
        }

        public int GetResourceCap(LogicResourceData data)
        {
            if (data.IsPremiumCurrency())
            {
                Debugger.Warning("LogicClientAvatar::getResourceCap shouldn't be used for diamonds");
            }
            else
            {
                int index = -1;

                for (int i = 0; i < this.m_resourceCap.Size(); i++)
                {
                    if (this.m_resourceCap[i].GetData() == data)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    return this.m_resourceCap[index].GetCount();
                }
            }

            return 0;
        }

        public void SetResourceCap(LogicResourceData data, int count)
        {
            if (data.IsPremiumCurrency())
            {
                Debugger.Warning("LogicClientAvatar::setResourceCap shouldn't be used for diamonds");
            }
            else
            {
                int index = -1;

                for (int i = 0; i < this.m_resourceCap.Size(); i++)
                {
                    if (this.m_resourceCap[i].GetData() == data)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    this.m_resourceCap[index].SetCount(count);
                }
                else
                {
                    this.m_resourceCap.Add(new LogicDataSlot(data, count));
                }
            }
        }

        public LogicArrayList<LogicDataSlot> GetResources()
        {
            return this.m_resourceCount;
        }

        public virtual int GetResourceCount(LogicResourceData data)
        {
            if (!data.IsPremiumCurrency())
            {
                int index = -1;

                for (int i = 0; i < this.m_resourceCount.Size(); i++)
                {
                    if (this.m_resourceCount[i].GetData() == data)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    return this.m_resourceCount[index].GetCount();
                }
            }
            else
            {
                Debugger.Warning("LogicAvatar::getResourceCount shouldn't be used for diamonds");
            }

            return 0;
        }

        public void SetResourceCount(LogicResourceData data, int count)
        {
            if (!data.IsPremiumCurrency())
            {
                int index = -1;

                for (int i = 0; i < this.m_resourceCount.Size(); i++)
                {
                    if (this.m_resourceCount[i].GetData() == data)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    this.m_resourceCount[index].SetCount(count);
                }
                else
                {
                    this.m_resourceCount.Add(new LogicDataSlot(data, count));
                }

                if (this.m_level != null && this.m_level.GetState() == 1)
                {
                    this.m_level.GetComponentManagerAt(data.GetVillageType()).DivideAvatarResourcesToStorages();
                }
            }
            else
            {
                Debugger.Warning("LogicAvatar::setResourceCount shouldn't be used for diamonds");
            }
        }

        public bool IsDarkElixirUnlocked()
        {
            return this.GetResourceCap(LogicDataTables.GetDarkElixirData()) > 0;
        }

        public int GetDamagingSpellsTotal()
        {
            int cnt = 0;

            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.SPELL);

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicSpellData data = (LogicSpellData) table.GetItemAt(i);

                int idx = -1;

                for (int j = 0; j < this.m_spellCount.Size(); j++)
                {
                    if (this.m_spellCount[j].GetData() == data)
                    {
                        idx = j;
                        break;
                    }
                }

                if (idx != -1 && (data.IsBuildingDamageSpell() || data.GetSummonTroop() != null))
                {
                    cnt += this.m_spellCount[idx].GetCount();
                }
            }

            return cnt;
        }

        public int GetSpellsTotalCapacity()
        {
            int cnt = 0;

            for (int i = 0; i < this.m_spellCount.Size(); i++)
            {
                LogicDataSlot slot = this.m_spellCount[i];
                LogicCombatItemData data = (LogicCombatItemData) slot.GetData();

                cnt += data.GetHousingSpace() * slot.GetCount();
            }

            return cnt;
        }

        public int GetUnitsTotalCapacity()
        {
            int cnt = 0;

            for (int i = 0; i < this.m_unitCount.Size(); i++)
            {
                LogicDataSlot slot = this.m_unitCount[i];
                LogicCombatItemData data = (LogicCombatItemData) slot.GetData();

                cnt += data.GetHousingSpace() * slot.GetCount();
            }

            return cnt;
        }

        public int GetUnitsTotal()
        {
            int count = 0;

            for (int i = 0; i < this.m_unitCount.Size(); i++)
            {
                count += this.m_unitCount[i].GetCount();
            }

            return count;
        }

        public int GetUnitsTotalVillage2()
        {
            int count = 0;

            for (int i = 0; i < this.m_unitCountVillage2.Size(); i++)
            {
                count += this.m_unitCountVillage2[i].GetCount();
            }

            return count;
        }

        public int GetUnitsNewTotalVillage2()
        {
            int count = 0;

            for (int i = 0; i < this.m_unitCountNewVillage2.Size(); i++)
            {
                count += this.m_unitCountNewVillage2[i].GetCount();
            }

            return count;
        }

        public LogicArrayList<LogicUnitSlot> GetAllianceUnits()
        {
            return this.m_allianceUnitCount;
        }

        public int GetAllianceUnitCount(LogicCombatItemData data, int upgLevel)
        {
            int index = -1;

            for (int i = 0; i < this.m_allianceUnitCount.Size(); i++)
            {
                if (this.m_allianceUnitCount[i].GetData() == data && this.m_allianceUnitCount[i].GetLevel() == upgLevel)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                return this.m_allianceUnitCount[index].GetCount();
            }

            return 0;
        }

        public void AddAllianceUnit(LogicCombatItemData data, int upgLevel)
        {
            this.SetAllianceUnitCount(data, upgLevel, this.GetAllianceUnitCount(data, upgLevel) + 1);
        }

        public void SetAllianceUnitCount(LogicCombatItemData data, int upgLevel, int count)
        {
            int index = -1;

            for (int i = 0; i < this.m_allianceUnitCount.Size(); i++)
            {
                if (this.m_allianceUnitCount[i].GetData() == data && this.m_allianceUnitCount[i].GetLevel() == upgLevel)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                if (data.GetCombatItemType() != LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
                {
                    this.m_allianceCastleUsedSpellCapacity += (count - this.m_allianceUnitCount[index].GetCount()) * data.GetHousingSpace();
                }
                else
                {
                    this.m_allianceCastleUsedCapacity += (count - this.m_allianceUnitCount[index].GetCount()) * data.GetHousingSpace();
                }

                this.m_allianceUnitCount[index].SetCount(count);
            }
            else
            {
                this.m_allianceUnitCount.Add(new LogicUnitSlot(data, upgLevel, count));

                if (data.GetCombatItemType() != LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
                {
                    this.m_allianceCastleUsedSpellCapacity += count * data.GetHousingSpace();
                }
                else
                {
                    this.m_allianceCastleUsedCapacity += count * data.GetHousingSpace();
                }
            }
        }

        public void RemoveAllianceUnit(LogicCombatItemData data, int upgLevel)
        {
            int count = this.GetAllianceUnitCount(data, upgLevel);

            if (count > 0)
            {
                this.SetAllianceUnitCount(data, upgLevel, count - 1);
            }
            else
            {
                Debugger.Warning("LogicClientAvatar::removeAllianceUnit called but unit count is already 0");
            }
        }

        public int GetUnitCount(LogicCombatItemData data)
        {
            if (data.GetCombatItemType() != LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
            {
                int index = -1;

                for (int i = 0; i < this.m_spellCount.Size(); i++)
                {
                    if (this.m_spellCount[i].GetData() == data)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    return this.m_spellCount[index].GetCount();
                }
            }
            else
            {
                int index = -1;

                for (int i = 0; i < this.m_unitCount.Size(); i++)
                {
                    if (this.m_unitCount[i].GetData() == data)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    return this.m_unitCount[index].GetCount();
                }
            }

            return 0;
        }

        public void SetUnitCount(LogicCombatItemData data, int count)
        {
            if (data.GetCombatItemType() != LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
            {
                int index = -1;

                for (int i = 0; i < this.m_spellCount.Size(); i++)
                {
                    if (this.m_spellCount[i].GetData() == data)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    this.m_spellCount[index].SetCount(count);
                }
                else
                {
                    this.m_spellCount.Add(new LogicDataSlot(data, count));
                }
            }
            else
            {
                int index = -1;

                for (int i = 0; i < this.m_unitCount.Size(); i++)
                {
                    if (this.m_unitCount[i].GetData() == data)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    this.m_unitCount[index].SetCount(count);
                }
                else
                {
                    this.m_unitCount.Add(new LogicDataSlot(data, count));
                }
            }
        }

        public int GetUnitCountVillage2(LogicCombatItemData data)
        {
            if (data.GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
            {
                int index = -1;

                for (int i = 0; i < this.m_unitCountVillage2.Size(); i++)
                {
                    if (this.m_unitCountVillage2[i].GetData() == data)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    return this.m_unitCountVillage2[index].GetCount();
                }
            }

            return 0;
        }

        public void SetUnitCountVillage2(LogicCombatItemData data, int count)
        {
            if (data.GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
            {
                int index = -1;

                for (int i = 0; i < this.m_unitCountVillage2.Size(); i++)
                {
                    if (this.m_unitCountVillage2[i].GetData() == data)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    this.m_unitCountVillage2[index].SetCount(count);
                }
                else
                {
                    this.m_unitCountVillage2.Add(new LogicDataSlot(data, count));
                }
            }
        }

        public int GetUnitCountNewVillage2(LogicCombatItemData data)
        {
            if (data.GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
            {
                int index = -1;

                for (int i = 0; i < this.m_unitCountNewVillage2.Size(); i++)
                {
                    if (this.m_unitCountNewVillage2[i].GetData() == data)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    return this.m_unitCountNewVillage2[index].GetCount();
                }
            }

            return 0;
        }

        public void SetUnitCountNewVillage2(LogicCombatItemData data, int count)
        {
            if (data.GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
            {
                int index = -1;

                for (int i = 0; i < this.m_unitCountNewVillage2.Size(); i++)
                {
                    if (this.m_unitCountNewVillage2[i].GetData() == data)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    this.m_unitCountNewVillage2[index].SetCount(count);
                }
                else
                {
                    this.m_unitCountNewVillage2.Add(new LogicDataSlot(data, count));
                }
            }
        }

        public int GetUnitUpgradeLevel(LogicCombatItemData data)
        {
            if (!data.UseUpgradeLevelByTownHall())
            {
                if (data.GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
                {
                    int index = -1;

                    for (int i = 0; i < this.m_unitUpgrade.Size(); i++)
                    {
                        if (this.m_unitUpgrade[i].GetData() == data)
                        {
                            index = i;
                            break;
                        }
                    }

                    if (index != -1)
                    {
                        return this.m_unitUpgrade[index].GetCount();
                    }
                }
                else if (data.GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_SPELL)
                {
                    int index = -1;

                    for (int i = 0; i < this.m_spellUpgrade.Size(); i++)
                    {
                        if (this.m_spellUpgrade[i].GetData() == data)
                        {
                            index = i;
                            break;
                        }
                    }

                    if (index != -1)
                    {
                        return this.m_spellUpgrade[index].GetCount();
                    }
                }
                else
                {
                    int index = -1;

                    for (int i = 0; i < this.m_heroUpgrade.Size(); i++)
                    {
                        if (this.m_heroUpgrade[i].GetData() == data)
                        {
                            index = i;
                            break;
                        }
                    }

                    if (index != -1)
                    {
                        return this.m_heroUpgrade[index].GetCount();
                    }
                }

                return 0;
            }

            return data.GetUpgradeLevelByTownHall(data.GetVillageType() == 1 ? this.m_townHallLevelVillage2 : this.m_townHallLevel);
        }

        public void SetUnitUpgradeLevel(LogicCombatItemData data, int count)
        {
            int combatItemType = data.GetCombatItemType();
            int upgradeCount = data.GetUpgradeLevelCount();

            if (combatItemType > 0)
            {
                if (combatItemType == 2)
                {
                    if (upgradeCount <= count)
                    {
                        Debugger.Warning("LogicAvatar::setUnitUpgradeLevel - Level is out of bounds!");
                        count = upgradeCount - 1;
                    }

                    int index = -1;

                    for (int i = 0; i < this.m_heroUpgrade.Size(); i++)
                    {
                        if (this.m_heroUpgrade[i].GetData() == data)
                        {
                            index = i;
                            break;
                        }
                    }

                    if (index != -1)
                    {
                        this.m_heroUpgrade[index].SetCount(count);
                    }
                    else
                    {
                        this.m_heroUpgrade.Add(new LogicDataSlot(data, count));
                    }
                }
                else
                {
                    if (upgradeCount <= count)
                    {
                        Debugger.Warning("LogicAvatar::setSpellUpgradeLevel - Level is out of bounds!");
                        count = upgradeCount - 1;
                    }

                    int index = -1;

                    for (int i = 0; i < this.m_spellUpgrade.Size(); i++)
                    {
                        if (this.m_spellUpgrade[i].GetData() == data)
                        {
                            index = i;
                            break;
                        }
                    }

                    if (index != -1)
                    {
                        this.m_spellUpgrade[index].SetCount(count);
                    }
                    else
                    {
                        this.m_spellUpgrade.Add(new LogicDataSlot(data, count));
                    }
                }
            }
            else
            {
                if (upgradeCount <= count)
                {
                    Debugger.Warning("LogicAvatar::setUnitUpgradeLevel - Level is out of bounds!");
                    count = upgradeCount - 1;
                }

                int index = -1;

                for (int i = 0; i < this.m_unitUpgrade.Size(); i++)
                {
                    if (this.m_unitUpgrade[i].GetData() == data)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    this.m_unitUpgrade[index].SetCount(count);
                }
                else
                {
                    this.m_unitUpgrade.Add(new LogicDataSlot(data, count));
                }
            }
        }

        public int GetNpcStars(LogicNpcData data)
        {
            int index = -1;

            for (int i = 0; i < this.m_npcStars.Size(); i++)
            {
                if (this.m_npcStars[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                return this.m_npcStars[index].GetCount();
            }

            return 0;
        }

        public void SetNpcStars(LogicNpcData data, int count)
        {
            int index = -1;

            for (int i = 0; i < this.m_npcStars.Size(); i++)
            {
                if (this.m_npcStars[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                this.m_npcStars[index].SetCount(count);
            }
            else
            {
                this.m_npcStars.Add(new LogicDataSlot(data, count));
            }
        }

        public int GetTotalNpcStars()
        {
            int cnt = 0;

            for (int i = 0; i < this.m_npcStars.Size(); i++)
            {
                cnt += this.m_npcStars[i].GetCount();
            }

            return cnt;
        }

        public int GetLootedNpcGold(LogicNpcData data)
        {
            int index = -1;

            for (int i = 0; i < this.m_lootedNpcGold.Size(); i++)
            {
                if (this.m_lootedNpcGold[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                return this.m_lootedNpcGold[index].GetCount();
            }

            return 0;
        }

        public void SetLootedNpcGold(LogicNpcData data, int count)
        {
            int index = -1;

            for (int i = 0; i < this.m_lootedNpcGold.Size(); i++)
            {
                if (this.m_lootedNpcGold[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                this.m_lootedNpcGold[index].SetCount(count);
            }
            else
            {
                this.m_lootedNpcGold.Add(new LogicDataSlot(data, count));
            }
        }

        public int GetLootedNpcElixir(LogicNpcData data)
        {
            int index = -1;

            for (int i = 0; i < this.m_lootedNpcElixir.Size(); i++)
            {
                if (this.m_lootedNpcElixir[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                return this.m_lootedNpcElixir[index].GetCount();
            }

            return 0;
        }

        public void SetLootedNpcElixir(LogicNpcData data, int count)
        {
            int index = -1;

            for (int i = 0; i < this.m_lootedNpcElixir.Size(); i++)
            {
                if (this.m_lootedNpcElixir[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                this.m_lootedNpcElixir[index].SetCount(count);
            }
            else
            {
                this.m_lootedNpcElixir.Add(new LogicDataSlot(data, count));
            }
        }

        public int GetUnitPresetCount(LogicCombatItemData data, int type)
        {
            LogicArrayList<LogicDataSlot> slots = null;

            switch (type)
            {
                case 0:
                    slots = this.m_previousArmy;
                    break;
                case 1:
                    slots = this.m_unitPreset1;
                    break;
                case 2:
                    slots = this.m_unitPreset2;
                    break;
                case 3:
                    slots = this.m_unitPreset3;
                    break;
            }

            int index = -1;

            for (int i = 0; i < slots.Size(); i++)
            {
                if (slots[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                return slots[index].GetCount();
            }

            return 0;
        }

        public void SetUnitPresetCount(LogicCombatItemData data, int type, int count)
        {
            LogicArrayList<LogicDataSlot> slots = null;

            switch (type)
            {
                case 0:
                    slots = this.m_previousArmy;
                    break;
                case 1:
                    slots = this.m_unitPreset1;
                    break;
                case 2:
                    slots = this.m_unitPreset2;
                    break;
                case 3:
                    slots = this.m_unitPreset3;
                    break;
            }

            int index = -1;

            for (int i = 0; i < slots.Size(); i++)
            {
                if (slots[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                slots[index].SetCount(count);
            }
            else
            {
                slots.Add(new LogicDataSlot(data, count));
            }
        }

        public int GetEventUnitCounterCount(LogicCombatItemData data)
        {
            int index = -1;

            for (int i = 0; i < this.m_eventUnitCounter.Size(); i++)
            {
                if (this.m_eventUnitCounter[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                return this.m_eventUnitCounter[index].GetCount();
            }

            return 0;
        }

        public void SetEventUnitCounterCount(LogicCombatItemData data, int count)
        {
            int index = -1;

            for (int i = 0; i < this.m_eventUnitCounter.Size(); i++)
            {
                if (this.m_eventUnitCounter[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                this.m_eventUnitCounter[index].SetCount(count);
            }
            else
            {
                this.m_eventUnitCounter.Add(new LogicDataSlot(data, count));
            }
        }

        public virtual void XpGainHelper(int count)
        {
            // XpGainHelper.
        }

        public LogicArrayList<LogicDataSlot> GetUnits()
        {
            return this.m_unitCount;
        }

        public LogicArrayList<LogicDataSlot> GetUnitUpgradeLevels()
        {
            return this.m_unitUpgrade;
        }

        public LogicArrayList<LogicDataSlot> GetUnitsVillage2()
        {
            return this.m_unitCountVillage2;
        }

        public LogicArrayList<LogicDataSlot> GetUnitsNewVillage2()
        {
            return this.m_unitCountNewVillage2;
        }

        public LogicArrayList<LogicDataSlot> GetSpells()
        {
            return this.m_spellCount;
        }

        public LogicArrayList<LogicDataSlot> GetSpellUpgradeLevels()
        {
            return this.m_spellUpgrade;
        }

        public LogicArrayList<LogicDataSlot> GetResourceCaps()
        {
            return this.m_resourceCap;
        }

        public void SetMissionCompleted(LogicMissionData data, bool state)
        {
            int index = -1;

            for (int i = 0; i < this.m_missionCompleted.Size(); i++)
            {
                if (this.m_missionCompleted[i] == data)
                {
                    index = i;
                    break;
                }
            }

            if (state)
            {
                if (index == -1)
                {
                    this.m_missionCompleted.Add(data);
                }
            }
            else
            {
                if (index != -1)
                {
                    this.m_missionCompleted.Remove(index);
                }
            }
        }

        public bool IsMissionCompleted(LogicMissionData data)
        {
            int index = -1;

            for (int i = 0; i < this.m_missionCompleted.Size(); i++)
            {
                if (this.m_missionCompleted[i] == data)
                {
                    index = i;
                    break;
                }
            }

            return index != -1;
        }

        public int GetAchievementProgress(LogicAchievementData data)
        {
            int index = -1;

            for (int i = 0; i < this.m_achievementProgress.Size(); i++)
            {
                if (this.m_achievementProgress[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                return this.m_achievementProgress[index].GetCount();
            }

            return 0;
        }

        public void SetAchievementProgress(LogicAchievementData data, int count)
        {
            int index = -1;

            for (int i = 0; i < this.m_achievementProgress.Size(); i++)
            {
                if (this.m_achievementProgress[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                this.m_achievementProgress[index].SetCount(count);
            }
            else
            {
                this.m_achievementProgress.Add(new LogicDataSlot(data, count));
            }
        }

        public void SetAchievementRewardClaimed(LogicAchievementData data, bool claimed)
        {
            int index = -1;

            for (int i = 0; i < this.m_achievementRewardClaimed.Size(); i++)
            {
                if (this.m_achievementRewardClaimed[i] == data)
                {
                    index = i;
                    break;
                }
            }

            if (claimed)
            {
                if (index == -1)
                {
                    this.m_achievementRewardClaimed.Add(data);
                }
            }
            else
            {
                if (index != -1)
                {
                    this.m_achievementRewardClaimed.Remove(index);
                }
            }
        }

        public bool IsAchievementRewardClaimed(LogicAchievementData data)
        {
            int index = -1;

            for (int i = 0; i < this.m_achievementRewardClaimed.Size(); i++)
            {
                if (this.m_achievementRewardClaimed[i] == data)
                {
                    index = i;
                    break;
                }
            }

            return index != -1;
        }

        public bool IsAchievementCompleted(LogicAchievementData data)
        {
            int index = -1;
            int progressCount = 0;

            for (int i = 0; i < this.m_achievementProgress.Size(); i++)
            {
                if (this.m_achievementProgress[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                progressCount = this.m_achievementProgress[index].GetCount();
            }

            return progressCount >= data.GetActionCount();
        }

        public int GetTownHallLevel()
        {
            return this.m_townHallLevel;
        }

        public void SetTownHallLevel(int level)
        {
            this.m_townHallLevel = level;
        }

        public int GetVillage2TownHallLevel()
        {
            return this.m_townHallLevelVillage2;
        }

        public void SetVillage2TownHallLevel(int level)
        {
            this.m_townHallLevelVillage2 = level;
        }

        public int GetAllianceCastleLevel()
        {
            return this.m_allianceCastleLevel;
        }

        public void SetAllianceCastleLevel(int level)
        {
            this.m_allianceCastleLevel = level;

            if (this.m_allianceCastleLevel == -1)
            {
                this.m_allianceCastleTotalCapacity = 0;
                this.m_allianceCastleTotalSpellCapacity = 0;
            }
            else
            {
                LogicBuildingData allianceCastleData = LogicDataTables.GetAllianceCastleData();

                this.m_allianceCastleTotalCapacity = allianceCastleData.GetUnitStorageCapacity(level);
                this.m_allianceCastleTotalSpellCapacity = allianceCastleData.GetAltUnitStorageCapacity(level);
            }
        }

        public int GetRedPackageState()
        {
            return this.m_redPackageState;
        }

        public int GetRedPackageCount()
        {
            int count = 0;

            for (int i = 4; i < 17; i *= 2)
            {
                if ((this.m_redPackageState & i) != 0)
                    count += 1;
            }

            return count;
        }

        public void SetRedPackageState(int state)
        {
            if (this.m_redPackageState != state)
            {
                this.m_redPackageState = state;
                this.m_listener.REDPackageStateChanged(state);
            }
        }

        public void ResetRedPackageState()
        {
            this.m_redPackageState &= 252;
        }

        public bool HasAllianceCastle()
        {
            return this.m_allianceCastleLevel != -1;
        }

        public int GetAllianceCastleTotalCapacity()
        {
            return this.m_allianceCastleTotalCapacity;
        }

        public int GetAllianceCastleTotalSpellCapacity()
        {
            return this.m_allianceCastleTotalSpellCapacity;
        }

        public int GetAllianceCastleUsedCapacity()
        {
            return this.m_allianceCastleUsedCapacity;
        }

        public int GetAllianceCastleUsedSpellCapacity()
        {
            return this.m_allianceCastleUsedSpellCapacity;
        }

        public int GetAllianceCastleFreeCapacity()
        {
            return this.m_allianceCastleTotalCapacity - this.m_allianceCastleUsedCapacity;
        }

        public int GetAllianceCastleFreeSpellCapacity()
        {
            return this.m_allianceCastleTotalSpellCapacity - this.m_allianceCastleUsedSpellCapacity;
        }

        public int GetAttackStrength(int villageType)
        {
            LogicComponentManager componentManager = this.m_level.GetComponentManagerAt(villageType);
            LogicArrayList<LogicHeroData> unlockedHeroData = new LogicArrayList<LogicHeroData>();
            LogicArrayList<LogicCharacterData> unlockedCharacterData = new LogicArrayList<LogicCharacterData>();
            LogicArrayList<LogicSpellData> unlockedSpellData = new LogicArrayList<LogicSpellData>();

            LogicDataTable heroTable = LogicDataTables.GetTable(LogicDataType.HERO);
            LogicDataTable characterTable = LogicDataTables.GetTable(LogicDataType.CHARACTER);
            LogicDataTable spellTable = LogicDataTables.GetTable(LogicDataType.SPELL);

            int maxBarrackLevel = componentManager.GetMaxBarrackLevel();
            int maxDarkBarrackLevel = componentManager.GetMaxDarkBarrackLevel();
            int maxSpellForgeLevel = componentManager.GetMaxSpellForgeLevel();
            int maxMiniSpellForgeLevel = componentManager.GetMaxMiniSpellForgeLevel();

            for (int i = 0; i < heroTable.GetItemCount(); i++)
            {
                LogicHeroData heroData = (LogicHeroData) heroTable.GetItemAt(i);

                if (componentManager.IsHeroUnlocked(heroData) && heroData.IsProductionEnabled() && heroData.GetVillageType() == villageType)
                {
                    unlockedHeroData.Add(heroData);
                }
            }

            for (int i = 0; i < characterTable.GetItemCount(); i++)
            {
                LogicCharacterData characterData = (LogicCharacterData) characterTable.GetItemAt(i);

                if (characterData.GetVillageType() == villageType &&
                    characterData.IsUnlockedForBarrackLevel(characterData.GetUnitOfType() == 1 ? maxBarrackLevel : maxDarkBarrackLevel) &&
                    characterData.IsProductionEnabled())
                {
                    unlockedCharacterData.Add(characterData);
                }
            }

            for (int i = 0; i < spellTable.GetItemCount(); i++)
            {
                LogicSpellData spellData = (LogicSpellData) spellTable.GetItemAt(i);

                if (spellData.GetVillageType() == villageType &&
                    spellData.IsUnlockedForProductionHouseLevel(spellData.GetUnitOfType() == 1 ? maxSpellForgeLevel : maxMiniSpellForgeLevel) &&
                    spellData.IsProductionEnabled())
                {
                    unlockedSpellData.Add(spellData);
                }
            }

            int[] heroUpgradeLevel = new int[unlockedHeroData.Size()];
            int[] characterUpgradeLevel = new int[unlockedCharacterData.Size()];
            int[] spellUpgradeLevel = new int[unlockedSpellData.Size()];

            for (int i = 0; i < unlockedHeroData.Size(); i++)
            {
                heroUpgradeLevel[i] = this.GetUnitUpgradeLevel(unlockedHeroData[i]);
            }

            for (int i = 0; i < unlockedCharacterData.Size(); i++)
            {
                characterUpgradeLevel[i] = this.GetUnitUpgradeLevel(unlockedCharacterData[i]);
            }

            for (int i = 0; i < unlockedSpellData.Size(); i++)
            {
                spellUpgradeLevel[i] = this.GetUnitUpgradeLevel(unlockedSpellData[i]);
            }

            int totalMaxHousing = componentManager.GetTotalMaxHousing(0);
            int spellForgeCapacity = 0;
            int miniSpellForgeCapacity = 0;

            if (spellForgeCapacity != -1)
            {
                spellForgeCapacity = LogicDataTables.GetBuildingByName("Spell Forge", null).GetUnitStorageCapacity(spellForgeCapacity);
            }

            if (miniSpellForgeCapacity != -1)
            {
                miniSpellForgeCapacity = LogicDataTables.GetBuildingByName("Mini Spell Factory", null).GetUnitStorageCapacity(miniSpellForgeCapacity);
            }

            int castleLevel = villageType == 0 ? this.m_allianceCastleLevel : -1;

            return (int) LogicStrengthUtil.GetAttackStrength(this.m_townHallLevel, castleLevel, unlockedHeroData, heroUpgradeLevel, unlockedCharacterData, characterUpgradeLevel,
                                                             totalMaxHousing, unlockedSpellData, spellUpgradeLevel, spellForgeCapacity + miniSpellForgeCapacity);
        }

        public abstract LogicLeagueData GetLeagueTypeData();
        public abstract void SaveToReplay(LogicJSONObject jsonObject);
        public abstract void SaveToDirect(LogicJSONObject jsonObject);
        public abstract void LoadForReplay(LogicJSONObject jsonObject, bool direct);
    }

    public enum LogicAvatarAllianceRole
    {
        MEMBER = 1,
        LEADER,
        ELDER,
        CO_LEADER
    }
}