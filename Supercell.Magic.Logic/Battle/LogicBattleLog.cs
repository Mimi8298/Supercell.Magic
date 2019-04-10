namespace Supercell.Magic.Logic.Battle
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class LogicBattleLog
    {
        private LogicLevel m_level;
        private LogicLong m_attackerHomeId;
        private LogicLong m_defenderHomeId;
        private LogicLong m_attackerAllianceId;
        private LogicLong m_defenderAllianceId;

        private readonly LogicArrayList<LogicDataSlot> m_lootCount;
        private readonly LogicArrayList<LogicDataSlot> m_availableLootCount;
        private readonly LogicArrayList<LogicDataSlot> m_castedSpellCount;
        private readonly LogicArrayList<LogicDataSlot> m_castedUnitCount;
        private readonly LogicArrayList<LogicUnitSlot> m_castedAllianceUnitCount;
        private readonly LogicArrayList<LogicDataSlot> m_unitLevelCount;
        private readonly LogicArrayList<LogicDataSlot> m_costCount;

        private int m_battleTime;
        private int m_villageType;
        private int m_attackerStars;
        private int m_attackerScore;
        private int m_defenderScore;
        private int m_deployedHousingSpace;
        private int m_destructionPercentage;
        private int m_originalAttackerScore;
        private int m_originalDefenderScore;
        private int m_attackerAllianceBadgeId;
        private int m_defenderAllianceBadgeId;
        private int m_armyDeploymentPercentage;
        private int m_lootMultiplierByTownHallDiff;
        private int m_attackerAllianceLevel;
        private int m_defenderAllianceLevel;

        private string m_attackerAllianceName;
        private string m_defenderAllianceName;
        private string m_attackerName;
        private string m_defenderName;

        private bool m_battleEnded;
        private bool m_townhallDestroyed;
        private bool m_allianceUsed;

        public LogicBattleLog(LogicLevel level)
        {
            this.m_level = level;

            this.m_attackerHomeId = new LogicLong();
            this.m_defenderHomeId = new LogicLong();

            this.m_costCount = new LogicArrayList<LogicDataSlot>();
            this.m_lootCount = new LogicArrayList<LogicDataSlot>();
            this.m_availableLootCount = new LogicArrayList<LogicDataSlot>();
            this.m_unitLevelCount = new LogicArrayList<LogicDataSlot>();
            this.m_castedSpellCount = new LogicArrayList<LogicDataSlot>();
            this.m_castedUnitCount = new LogicArrayList<LogicDataSlot>();
            this.m_castedAllianceUnitCount = new LogicArrayList<LogicUnitSlot>();
        }

        public void Destruct()
        {
            this.m_level = null;
            this.m_attackerAllianceName = null;
            this.m_defenderAllianceName = null;
            this.m_attackerName = null;
            this.m_defenderName = null;
            this.m_attackerHomeId = null;
            this.m_defenderHomeId = null;
            this.m_attackerAllianceId = null;
            this.m_defenderAllianceId = null;
        }

        public int GetVillageType()
        {
            return this.m_villageType;
        }

        public void SetVillageType(int value)
        {
            this.m_villageType = value;
        }

        public int GetStars()
        {
            return (this.m_destructionPercentage >= 50 ? 1 : 0) + (this.m_destructionPercentage == 100 ? 1 : 0) + (this.m_townhallDestroyed ? 1 : 0);
        }

        public bool GetTownHallDestroyed()
        {
            return this.m_townhallDestroyed;
        }

        public void SetTownHallDestroyed(bool destroyed)
        {
            this.m_townhallDestroyed = destroyed;
        }

        public int GetStolenResources(LogicResourceData data)
        {
            for (int i = 0; i < this.m_lootCount.Size(); i++)
            {
                if (this.m_lootCount[i].GetData() == data)
                {
                    return this.m_lootCount[i].GetCount();
                }
            }

            return 0;
        }

        public void IncreaseStolenResourceCount(LogicResourceData data, int count)
        {
            if (this.m_level != null)
            {
                this.m_level.GetAchievementManager().IncreaseLoot(data, count);
            }

            int idx = -1;

            for (int i = 0; i < this.m_lootCount.Size(); i++)
            {
                if (this.m_lootCount[i].GetData() == data)
                {
                    idx = i;
                    break;
                }
            }

            if (idx != -1)
            {
                this.m_lootCount[idx].SetCount(this.m_lootCount[idx].GetCount() + count);
            }
            else
            {
                this.m_lootCount.Add(new LogicDataSlot(data, count));
            }
        }

        public LogicArrayList<LogicDataSlot> GetCastedSpells()
        {
            return this.m_castedSpellCount;
        }

        public LogicArrayList<LogicDataSlot> GetCastedUnits()
        {
            return this.m_castedUnitCount;
        }

        public LogicArrayList<LogicUnitSlot> GetCastedAllianceUnits()
        {
            return this.m_castedAllianceUnitCount;
        }

        public int GetCostCount(LogicData data)
        {
            for (int i = 0; i < this.m_costCount.Size(); i++)
            {
                if (this.m_costCount[i].GetData() == data)
                {
                    return this.m_costCount[i].GetCount();
                }
            }

            return 0;
        }

        public bool GetBattleStarted()
        {
            if (!this.m_battleEnded)
            {
                int matchType = this.m_level.GetMatchType();

                if (matchType < 10)
                {
                    if (matchType == 3 || matchType == 7 || matchType == 8 || matchType == 9)
                    {
                        return true;
                    }
                }

                return this.m_castedUnitCount.Size() +
                       this.m_castedSpellCount.Size() +
                       this.m_castedAllianceUnitCount.Size() > 0;
            }

            return true;
        }

        public bool HasDeployedUnits()
        {
            return this.m_castedUnitCount.Size() +
                   this.m_castedSpellCount.Size() +
                   this.m_castedAllianceUnitCount.Size() > 0;
        }

        public bool GetBattleEnded()
        {
            return this.m_battleEnded;
        }

        public void SetBattleEnded(int battleTime)
        {
            this.m_battleTime = battleTime;
            this.m_battleEnded = true;
        }

        public void SetAttackerHomeId(LogicLong homeId)
        {
            this.m_attackerHomeId = homeId;
        }

        public LogicLong GetDefenderHomeId()
        {
            return this.m_defenderHomeId;
        }

        public void SetDefenderHomeId(LogicLong homeId)
        {
            this.m_defenderHomeId = homeId;
        }

        public void SetAttackerAllianceId(LogicLong allianceId)
        {
            this.m_attackerAllianceId = allianceId;
        }

        public void SetDefenderAllianceId(LogicLong allianceId)
        {
            this.m_defenderAllianceId = allianceId;
        }

        public void SetAttackerAllianceBadge(int badgeId)
        {
            this.m_attackerAllianceBadgeId = badgeId;
        }

        public void SetDefenderAllianceBadge(int badgeId)
        {
            this.m_defenderAllianceBadgeId = badgeId;
        }

        public void SetAttackerAllianceLevel(int level)
        {
            this.m_attackerAllianceLevel = level;
        }

        public void SetDefenderAllianceLevel(int level)
        {
            this.m_defenderAllianceLevel = level;
        }

        public void SetAttackerAllianceName(string name)
        {
            this.m_attackerAllianceName = name;
        }

        public void SetDefenderAllianceName(string name)
        {
            this.m_defenderAllianceName = name;
        }

        public void SetAttackerStars(int star)
        {
            this.m_attackerStars = star;
        }

        public void SetAttackerScore(int count)
        {
            this.m_attackerScore = count;
        }

        public void SetDefenderScore(int count)
        {
            this.m_defenderScore = count;
        }

        public void SetOriginalAttackerScore(int count)
        {
            this.m_originalAttackerScore = count;
        }

        public void SetOriginalDefenderScore(int count)
        {
            this.m_originalDefenderScore = count;
        }

        public void SetAttackerName(string name)
        {
            this.m_attackerName = name;
        }

        public void SetDefenderName(string name)
        {
            this.m_defenderName = name;
        }

        public void SetAllianceUsed(bool used)
        {
            this.m_allianceUsed = used;
        }

        public void CalculateAvailableResources(LogicLevel level, int matchType)
        {
            for (int i = this.m_availableLootCount.Size() - 1; i >= 0; i--)
            {
                this.m_availableLootCount[i].Destruct();
                this.m_availableLootCount.Remove(i);
            }

            LogicDataTable resourceTable = LogicDataTables.GetTable(LogicDataType.RESOURCE);

            for (int i = 0; i < resourceTable.GetItemCount(); i++)
            {
                LogicResourceData data = (LogicResourceData) resourceTable.GetItemAt(i);
                LogicResourceData warResourceReferenceData = data.GetWarResourceReferenceData();
                LogicDataSlot dataSlot = null;

                if (warResourceReferenceData != null)
                {
                    for (int j = 0; j < this.m_availableLootCount.Size(); j++)
                    {
                        if (this.m_availableLootCount[j].GetData() == warResourceReferenceData)
                        {
                            dataSlot = this.m_availableLootCount[j];
                            break;
                        }
                    }

                    Debugger.DoAssert(dataSlot != null, "Didn't find the resource slot");
                }
                else
                {
                    this.m_availableLootCount.Add(dataSlot = new LogicDataSlot(data, 0));
                }

                if (matchType == 8 || matchType == 9)
                {
                    LogicAvatar homeOwnerAvatar = level.GetHomeOwnerAvatar();

                    if (homeOwnerAvatar != null)
                    {
                        LogicArrayList<LogicDataSlot> resourceCount = homeOwnerAvatar.GetResources();

                        for (int j = 0; j < resourceCount.Size(); j++)
                        {
                            if (resourceCount[j].GetData() == data)
                            {
                                dataSlot.SetCount(resourceCount[j].GetCount());
                            }
                        }
                    }
                }
                else
                {
                    LogicComponentManager componentManager = level.GetComponentManagerAt(level.GetVillageType());

                    if (warResourceReferenceData == null)
                    {
                        LogicArrayList<LogicComponent> resourceProductionComponents = componentManager.GetComponents(LogicComponentType.RESOURCE_PRODUCTION);
                        LogicArrayList<LogicComponent> resourceStorageComponents = componentManager.GetComponents(LogicComponentType.RESOURCE_STORAGE);

                        for (int j = 0; j < resourceProductionComponents.Size(); j++)
                        {
                            LogicResourceProductionComponent resourceProductionComponent = (LogicResourceProductionComponent) resourceProductionComponents[j];
                            LogicGameObject gameObject = resourceProductionComponent.GetParent();

                            if (gameObject.IsAlive() &&
                                resourceProductionComponent.IsEnabled())
                            {
                                if (resourceProductionComponent.GetResourceData() == data)
                                {
                                    dataSlot.SetCount(dataSlot.GetCount() + resourceProductionComponent.GetStealableResourceCount());
                                }
                            }
                        }

                        for (int j = 0; j < resourceStorageComponents.Size(); j++)
                        {
                            LogicResourceStorageComponent resourceStorageComponent = (LogicResourceStorageComponent) resourceStorageComponents[j];
                            LogicGameObject gameObject = resourceStorageComponent.GetParent();

                            if (gameObject.IsAlive() &&
                                resourceStorageComponent.IsEnabled())
                            {
                                dataSlot.SetCount(dataSlot.GetCount() + resourceStorageComponent.GetStealableResourceCount(i));
                            }
                        }
                    }
                    else
                    {
                        LogicArrayList<LogicComponent> warResourceStorageComponents = componentManager.GetComponents(LogicComponentType.WAR_RESOURCE_STORAGE);

                        for (int j = 0; j < warResourceStorageComponents.Size(); j++)
                        {
                            LogicWarResourceStorageComponent resourceWarStorageComponent = (LogicWarResourceStorageComponent) warResourceStorageComponents[j];
                            LogicGameObject gameObject = resourceWarStorageComponent.GetParent();

                            if (gameObject.IsAlive() &&
                                resourceWarStorageComponent.IsEnabled())
                            {
                                dataSlot.SetCount(dataSlot.GetCount() + resourceWarStorageComponent.GetStealableResourceCount(i));
                            }
                        }
                    }
                }
            }
        }

        public void IncrementDeployedAttackerUnits(LogicCombatItemData data, int count)
        {
            int multiplier = data.GetCombatItemType() != LogicCombatItemData.COMBAT_ITEM_TYPE_HERO
                ? LogicDataTables.GetGlobals().GetUnitHousingCostMultiplier()
                : LogicDataTables.GetGlobals().GetHeroHousingCostMultiplier();
            int maxHousingSpace = LogicDataTables.GetTownHallLevel(this.m_level.GetTownHallLevel(0)).GetMaxHousingSpace();

            if (maxHousingSpace > 0)
            {
                this.m_armyDeploymentPercentage =
                    (100000 * this.m_deployedHousingSpace / maxHousingSpace + 50) / 100;
            }

            this.m_deployedHousingSpace += multiplier * data.GetHousingSpace() * count / 100;

            int index = -1;

            for (int i = 0; i < this.m_castedUnitCount.Size(); i++)
            {
                if (this.m_castedUnitCount[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                this.m_castedUnitCount[index].SetCount(this.m_castedUnitCount[index].GetCount() + count);
            }
            else
            {
                this.m_castedUnitCount.Add(new LogicDataSlot(data, count));
            }
        }

        public void IncrementDeployedAllianceUnits(LogicCombatItemData data, int count, int upgLevel)
        {
            int multiplier = data.GetCombatItemType() != LogicCombatItemData.COMBAT_ITEM_TYPE_HERO
                ? LogicDataTables.GetGlobals().GetUnitHousingCostMultiplier()
                : LogicDataTables.GetGlobals().GetHeroHousingCostMultiplier();
            int maxHousingSpace = LogicDataTables.GetTownHallLevel(this.m_level.GetTownHallLevel(0)).GetMaxHousingSpace();

            if (maxHousingSpace > 0)
            {
                this.m_armyDeploymentPercentage =
                    (100000 * this.m_deployedHousingSpace / maxHousingSpace + 50) / 100;
            }

            this.m_deployedHousingSpace += multiplier * data.GetHousingSpace() * count / 100;

            int index = -1;

            for (int i = 0; i < this.m_castedAllianceUnitCount.Size(); i++)
            {
                if (this.m_castedAllianceUnitCount[i].GetData() == data && this.m_castedAllianceUnitCount[i].GetLevel() == upgLevel)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                this.m_castedAllianceUnitCount[index].SetCount(this.m_castedAllianceUnitCount[index].GetCount() + count);
            }
            else
            {
                this.m_castedAllianceUnitCount.Add(new LogicUnitSlot(data, upgLevel, count));
            }
        }

        public void IncrementCastedSpells(LogicSpellData data, int count)
        {
            this.m_deployedHousingSpace += LogicDataTables.GetGlobals().GetSpellHousingCostMultiplier() * data.GetHousingSpace() * count / 100;
            this.m_armyDeploymentPercentage =
                (100000 * this.m_deployedHousingSpace / LogicDataTables.GetTownHallLevel(this.m_level.GetTownHallLevel(0)).GetMaxHousingSpace() + 50) / 100;

            int index = -1;

            for (int i = 0; i < this.m_castedSpellCount.Size(); i++)
            {
                if (this.m_castedSpellCount[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                this.m_castedSpellCount[index].SetCount(this.m_castedSpellCount[index].GetCount() + count);
            }
            else
            {
                this.m_castedSpellCount.Add(new LogicDataSlot(data, count));
            }
        }

        public void IncrementDestroyedBuildingCount(LogicBuildingData data)
        {
            LogicBuildingClassData buildingClass = data.GetBuildingClass();

            if (buildingClass.IsTownHall() || buildingClass.IsTownHall2())
            {
                this.m_townhallDestroyed = true;
            }

            if (this.m_level != null)
            {
                LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();

                if (homeOwnerAvatar != null && homeOwnerAvatar.IsClientAvatar())
                {
                    if (this.m_level.GetState() == 2)
                    {
                        int matchType = this.m_level.GetMatchType();

                        if (matchType != 2 && matchType != 5)
                        {
                            this.m_level.GetAchievementManager().BuildingDestroyedInPvP(data);
                        }
                    }
                }
            }
        }

        public void SetCombatItemLevel(LogicData data, int upgLevel)
        {
            int index = -1;

            for (int i = 0; i < this.m_unitLevelCount.Size(); i++)
            {
                if (this.m_unitLevelCount[i].GetData() == data)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                this.m_unitLevelCount[index].SetCount(upgLevel);
            }
            else
            {
                this.m_unitLevelCount.Add(new LogicDataSlot(data, upgLevel));
            }
        }

        public int GetDestructionPercentage()
        {
            return this.m_destructionPercentage;
        }

        public int GetDeployedHousingSpace()
        {
            return this.m_deployedHousingSpace;
        }

        public void SetDestructionPercentage(int percentage)
        {
            if (this.m_destructionPercentage <= percentage)
            {
                this.m_destructionPercentage = percentage;
            }
            else
            {
                Debugger.Warning("LogicBattleLog: m_destructionPercentage decreases");
            }
        }

        public LogicJSONObject GenerateDefenderJSON()
        {
            return this.GenerateBattleLogJSON(this.m_attackerHomeId, this.m_attackerAllianceId, this.m_attackerAllianceBadgeId, this.m_attackerAllianceName,
                                              this.m_defenderAllianceBadgeId, this.m_attackerAllianceLevel, this.m_defenderAllianceLevel);
        }

        public LogicJSONObject GenerateAttackerJSON()
        {
            return this.GenerateBattleLogJSON(this.m_defenderHomeId, this.m_defenderAllianceId, this.m_defenderAllianceBadgeId, this.m_defenderAllianceName,
                                              this.m_attackerAllianceBadgeId, this.m_defenderAllianceLevel, this.m_attackerAllianceLevel);
        }

        public LogicJSONObject GenerateBattleLogJSON(LogicLong homeId, LogicLong allianceId, int allianceBadgeId, string allianceName, int allianceBadgeId2, int allianceExpLevel,
                                                     int allianceExpLevel2)
        {
            LogicJSONObject jsonObject = new LogicJSONObject();

            jsonObject.Put("villageType", new LogicJSONNumber(this.m_villageType));

            bool village2Match = true;

            if ((this.m_level.GetMatchType() & 0xFFFFFFFE) != 8)
            {
                jsonObject.Put("loot", LogicBattleLog.DataSlotArrayToJSONArray(this.m_lootCount));
                jsonObject.Put("availableLoot", LogicBattleLog.DataSlotArrayToJSONArray(this.m_availableLootCount));

                village2Match = false;
            }

            jsonObject.Put("units", LogicBattleLog.DataSlotArrayToJSONArray(this.m_castedUnitCount));

            if (!village2Match)
            {
                if (this.m_castedAllianceUnitCount != null && this.m_castedAllianceUnitCount.Size() > 0)
                {
                    jsonObject.Put("cc_units", LogicBattleLog.UnitSlotArrayToJSONArray(this.m_castedAllianceUnitCount));
                }
            }

            if (this.m_costCount != null && this.m_costCount.Size() > 0)
            {
                jsonObject.Put("costs", LogicBattleLog.DataSlotArrayToJSONArray(this.m_costCount));
            }

            if (!village2Match)
            {
                jsonObject.Put("spells", LogicBattleLog.DataSlotArrayToJSONArray(this.m_castedSpellCount));
            }

            jsonObject.Put("levels", LogicBattleLog.DataSlotArrayToJSONArray(this.m_unitLevelCount));

            LogicJSONObject statObject = new LogicJSONObject();

            statObject.Put("townhallDestroyed", new LogicJSONBoolean(this.m_townhallDestroyed));
            statObject.Put("battleEnded", new LogicJSONBoolean(this.m_battleEnded));

            if (!village2Match)
            {
                statObject.Put("allianceUsed", new LogicJSONBoolean(this.m_allianceUsed));
            }

            statObject.Put("destructionPercentage", new LogicJSONNumber(this.m_destructionPercentage));
            statObject.Put("battleTime", new LogicJSONNumber(this.m_battleTime));

            if (!village2Match)
            {
                statObject.Put("originalAttackerScore", new LogicJSONNumber(this.m_originalAttackerScore));
                statObject.Put("attackerScore", new LogicJSONNumber(this.m_attackerScore));
                statObject.Put("originalDefenderScore", new LogicJSONNumber(this.m_originalDefenderScore));
                statObject.Put("defenderScore", new LogicJSONNumber(this.m_defenderScore));
            }

            statObject.Put("allianceName", new LogicJSONString(allianceName));

            if (!village2Match)
            {
                statObject.Put("attackerStars", new LogicJSONNumber(this.m_attackerStars));
            }

            if (this.m_level.GetMatchType() <= 7)
            {
                statObject.Put("attackerName", new LogicJSONString(this.m_attackerName));
                statObject.Put("defenderName", new LogicJSONString(this.m_defenderName));

                int lootMultiplierByTownHallDiff = 100;

                if (LogicDataTables.GetGlobals().UseTownHallLootPenaltyInWar())
                {
                    lootMultiplierByTownHallDiff = LogicDataTables.GetGlobals().GetLootMultiplierByTownHallDiff(this.m_level.GetVisitorAvatar().GetTownHallLevel(),
                                                                                                                this.m_level.GetHomeOwnerAvatar().GetTownHallLevel());
                }

                statObject.Put("lootMultiplierByTownHallDiff", new LogicJSONNumber(lootMultiplierByTownHallDiff));
            }

            LogicJSONArray homeIdArray = new LogicJSONArray(2);

            homeIdArray.Add(new LogicJSONNumber(homeId.GetHigherInt()));
            homeIdArray.Add(new LogicJSONNumber(homeId.GetLowerInt()));

            statObject.Put("homeID", homeIdArray);

            if (allianceBadgeId != -2)
            {
                statObject.Put("allianceBadge", new LogicJSONNumber(allianceBadgeId));
            }

            if (allianceBadgeId2 != -2)
            {
                statObject.Put("allianceBadge2", new LogicJSONNumber(allianceBadgeId2));
            }

            if (allianceId != null)
            {
                LogicJSONArray allianceIdArray = new LogicJSONArray(2);

                allianceIdArray.Add(new LogicJSONNumber(allianceId.GetHigherInt()));
                allianceIdArray.Add(new LogicJSONNumber(allianceId.GetLowerInt()));

                statObject.Put("allianceID", allianceIdArray);
            }

            if (!village2Match)
            {
                statObject.Put("deployedHousingSpace", new LogicJSONNumber(this.m_deployedHousingSpace));
                statObject.Put("armyDeploymentPercentage", new LogicJSONNumber(this.m_armyDeploymentPercentage));
            }

            if (allianceExpLevel != 0)
            {
                statObject.Put("allianceExp", new LogicJSONNumber(allianceExpLevel));
            }

            if (allianceExpLevel2 != 0)
            {
                statObject.Put("allianceExp2", new LogicJSONNumber(allianceExpLevel2));
            }

            jsonObject.Put("stats", statObject);

            return jsonObject;
        }

        public LogicJSONObject LoadBattleLogFromJSON(LogicJSONObject root)
        {
            LogicJSONNumber villageTypeNumber = root.GetJSONNumber("villageType");

            if (villageTypeNumber != null)
            {
                this.m_villageType = villageTypeNumber.GetIntValue();
            }

            LogicJSONNode lootNode = root.Get("loot");

            if (lootNode != null && lootNode.GetJSONNodeType() == LogicJSONNodeType.ARRAY)
            {
                LogicBattleLog.AddJSONDataSlotsToArray((LogicJSONArray) lootNode, this.m_lootCount);
            }
            else if (this.m_villageType != 1)
            {
                Debugger.Warning("LogicBattleLog has no loot.");
            }

            LogicJSONNode unitNode = root.Get("units");

            if (unitNode != null && unitNode.GetJSONNodeType() == LogicJSONNodeType.ARRAY)
            {
                LogicBattleLog.AddJSONDataSlotsToArray((LogicJSONArray) unitNode, this.m_castedUnitCount);
            }
            else
            {
                Debugger.Warning("LogicBattleLog has no loot.");
            }

            LogicJSONNode allianceUnitNode = root.Get("cc_units");

            if (allianceUnitNode != null && allianceUnitNode.GetJSONNodeType() == LogicJSONNodeType.ARRAY)
            {
                LogicBattleLog.AddJSONUnitSlotsToArray((LogicJSONArray) allianceUnitNode, this.m_castedAllianceUnitCount);
            }

            LogicJSONNode costNode = root.Get("costs");

            if (costNode != null && costNode.GetJSONNodeType() == LogicJSONNodeType.ARRAY)
            {
                LogicBattleLog.AddJSONDataSlotsToArray((LogicJSONArray) costNode, this.m_costCount);
            }

            LogicJSONNode spellNode = root.Get("spells");

            if (spellNode != null && spellNode.GetJSONNodeType() == LogicJSONNodeType.ARRAY)
            {
                LogicBattleLog.AddJSONDataSlotsToArray((LogicJSONArray) spellNode, this.m_costCount);
            }
            else if (this.m_villageType != 1)
            {
                Debugger.Warning("LogicBattleLog has no spells.");
            }

            LogicJSONNode levelNode = root.Get("levels");

            if (levelNode != null && levelNode.GetJSONNodeType() == LogicJSONNodeType.ARRAY)
            {
                LogicBattleLog.AddJSONDataSlotsToArray((LogicJSONArray) levelNode, this.m_unitLevelCount);
            }
            else
            {
                Debugger.Warning("LogicBattleLog has no levels.");
            }

            LogicJSONNode statsNode = root.Get("stats");

            if (statsNode != null && statsNode.GetJSONNodeType() == LogicJSONNodeType.OBJECT)
            {
                LogicJSONObject statsObject = (LogicJSONObject) statsNode;
                LogicJSONBoolean townhallDestroyedBoolean = statsObject.GetJSONBoolean("townhallDestroyed");

                if (townhallDestroyedBoolean != null)
                {
                    this.m_townhallDestroyed = townhallDestroyedBoolean.IsTrue();
                }

                LogicJSONBoolean battleEndedBoolean = statsObject.GetJSONBoolean("battleEnded");

                if (battleEndedBoolean != null)
                {
                    this.m_battleEnded = battleEndedBoolean.IsTrue();
                }

                LogicJSONBoolean allianceUsedBoolean = statsObject.GetJSONBoolean("allianceUsed");

                if (allianceUsedBoolean != null)
                {
                    this.m_allianceUsed = allianceUsedBoolean.IsTrue();
                }

                LogicJSONNumber destructionPercentageNumber = statsObject.GetJSONNumber("destructionPercentage");

                if (destructionPercentageNumber != null)
                {
                    this.m_destructionPercentage = destructionPercentageNumber.GetIntValue();
                }

                LogicJSONNumber battleTimeNumber = statsObject.GetJSONNumber("battleTime");

                if (battleTimeNumber != null)
                {
                    this.m_battleTime = battleTimeNumber.GetIntValue();
                }

                LogicJSONNumber attackerScoreNumber = statsObject.GetJSONNumber("attackerScore");

                if (attackerScoreNumber != null)
                {
                    this.m_attackerScore = attackerScoreNumber.GetIntValue();
                }

                LogicJSONNumber defenderScoreNumber = statsObject.GetJSONNumber("defenderScore");

                if (defenderScoreNumber != null)
                {
                    this.m_defenderScore = defenderScoreNumber.GetIntValue();
                }

                LogicJSONNumber originalAttackerScoreNumber = statsObject.GetJSONNumber("originalAttackerScore");

                if (originalAttackerScoreNumber != null)
                {
                    this.m_originalAttackerScore = originalAttackerScoreNumber.GetIntValue();
                }
                else
                {
                    this.m_attackerScore = -1;
                }

                LogicJSONNumber originalDefenderScoreNumber = statsObject.GetJSONNumber("originalDefenderScore");

                if (originalDefenderScoreNumber != null)
                {
                    this.m_originalDefenderScore = originalDefenderScoreNumber.GetIntValue();
                }
                else
                {
                    this.m_originalDefenderScore = -1;
                }

                this.LoadAttackerNameFromJson(statsObject);
                this.LoadDefenderNameFromJson(statsObject);

                LogicJSONNumber lootMultiplierByTownHallDiffNumber = statsObject.GetJSONNumber("lootMultiplierByTownHallDiff");

                if (lootMultiplierByTownHallDiffNumber != null)
                {
                    this.m_lootMultiplierByTownHallDiff = lootMultiplierByTownHallDiffNumber.GetIntValue();
                }
                else
                {
                    this.m_lootMultiplierByTownHallDiff = -1;
                }

                LogicJSONNumber deployedHousingSpaceNumber = statsObject.GetJSONNumber("deployedHousingSpace");

                if (deployedHousingSpaceNumber != null)
                {
                    this.m_deployedHousingSpace = deployedHousingSpaceNumber.GetIntValue();
                }

                LogicJSONNumber armyDeploymentPercentageNumber = statsObject.GetJSONNumber("armyDeploymentPercentage");

                if (armyDeploymentPercentageNumber != null)
                {
                    this.m_armyDeploymentPercentage = armyDeploymentPercentageNumber.GetIntValue();
                }

                LogicJSONNumber attackerStarsNumber = statsObject.GetJSONNumber("attackerStars");

                if (attackerStarsNumber != null)
                {
                    this.m_attackerStars = attackerStarsNumber.GetIntValue();
                }

                return statsObject;
            }

            Debugger.Warning("LogicBattleLog has no stats.");

            return null;
        }

        public void LoadAttackerNameFromJson(LogicJSONObject jsonObject)
        {
            LogicJSONString attackerNameObject = jsonObject.GetJSONString("attackerName");

            if (attackerNameObject != null)
            {
                this.m_attackerName = attackerNameObject.GetStringValue();
            }
            else
            {
                this.m_attackerName = string.Empty;
            }
        }

        public void LoadDefenderNameFromJson(LogicJSONObject jsonObject)
        {
            LogicJSONString defenderNameObject = jsonObject.GetJSONString("defenderName");

            if (defenderNameObject != null)
            {
                this.m_defenderName = defenderNameObject.GetStringValue();
            }
            else
            {
                this.m_defenderName = string.Empty;
            }
        }

        public static void AddJSONDataSlotsToArray(LogicJSONArray jsonArray, LogicArrayList<LogicDataSlot> slot)
        {
            for (int i = 0; i < jsonArray.Size(); i++)
            {
                LogicJSONArray objectArray = jsonArray.GetJSONArray(i);

                if (objectArray != null && objectArray.Size() == 2)
                {
                    LogicData data = LogicDataTables.GetDataById(jsonArray.GetJSONNumber(0).GetIntValue());
                    int count = objectArray.GetJSONNumber(1).GetIntValue();

                    slot.Add(new LogicDataSlot(data, count));
                }
            }
        }

        public static void AddJSONUnitSlotsToArray(LogicJSONArray jsonArray, LogicArrayList<LogicUnitSlot> slot)
        {
            for (int i = 0; i < jsonArray.Size(); i++)
            {
                LogicJSONArray objectArray = jsonArray.GetJSONArray(i);

                if (objectArray != null && objectArray.Size() == 2)
                {
                    LogicData data = LogicDataTables.GetDataById(jsonArray.GetJSONNumber(0).GetIntValue());

                    int level = objectArray.GetJSONNumber(1).GetIntValue();
                    int count = objectArray.GetJSONNumber(2).GetIntValue();

                    slot.Add(new LogicUnitSlot(data, level, count));
                }
            }
        }

        public static LogicJSONArray DataSlotArrayToJSONArray(LogicArrayList<LogicDataSlot> dataSlotArray)
        {
            LogicJSONArray jsonArray = new LogicJSONArray(dataSlotArray.Size());

            for (int i = 0; i < dataSlotArray.Size(); i++)
            {
                LogicDataSlot dataSlot = dataSlotArray[i];
                LogicJSONArray objectArray = new LogicJSONArray();

                objectArray.Add(new LogicJSONNumber(dataSlot.GetData().GetGlobalID()));
                objectArray.Add(new LogicJSONNumber(dataSlot.GetCount()));

                jsonArray.Add(objectArray);
            }

            return jsonArray;
        }

        public static LogicJSONArray UnitSlotArrayToJSONArray(LogicArrayList<LogicUnitSlot> dataSlotArray)
        {
            LogicJSONArray jsonArray = new LogicJSONArray(dataSlotArray.Size());

            for (int i = 0; i < dataSlotArray.Size(); i++)
            {
                LogicUnitSlot unitSlot = dataSlotArray[i];
                LogicJSONArray objectArray = new LogicJSONArray();

                objectArray.Add(new LogicJSONNumber(unitSlot.GetData().GetGlobalID()));
                objectArray.Add(new LogicJSONNumber(unitSlot.GetLevel()));
                objectArray.Add(new LogicJSONNumber(unitSlot.GetCount()));

                jsonArray.Add(objectArray);
            }

            return jsonArray;
        }
    }
}