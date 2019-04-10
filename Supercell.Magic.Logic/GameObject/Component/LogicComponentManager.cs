namespace Supercell.Magic.Logic.GameObject.Component
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class LogicComponentManager
    {
        private readonly LogicLevel m_level;
        private readonly LogicArrayList<LogicComponent>[] m_components;
        private readonly LogicArrayList<LogicDataSlot> m_units;

        public LogicComponentManager(LogicLevel level)
        {
            this.m_level = level;
            this.m_units = new LogicArrayList<LogicDataSlot>();
            this.m_components = new LogicArrayList<LogicComponent>[(int) LogicComponent.COMPONENT_TYPE_COUNT];

            for (int i = 0; i < LogicComponent.COMPONENT_TYPE_COUNT; i++)
            {
                this.m_components[i] = new LogicArrayList<LogicComponent>(32);
            }
        }

        public void Destruct()
        {
            for (int i = 0; i < LogicComponent.COMPONENT_TYPE_COUNT; i++)
            {
                if (this.m_components[i] != null)
                {
                    this.m_components[i].Destruct();
                    this.m_components[i] = null;
                }
            }

            if (this.m_units != null)
            {
                for (int i = this.m_units.Size() - 1; i >= 0; i--)
                {
                    this.m_units[i].Destruct();
                    this.m_units.Remove(i);
                }
            }
        }

        public void AddComponent(LogicComponent component)
        {
            this.m_components[(int) component.GetComponentType()].Add(component);
        }

        public void ValidateTroopUpgradeLevels()
        {
            LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();

            if (homeOwnerAvatar != null)
            {
                if (homeOwnerAvatar.IsClientAvatar())
                {
                    int[] laboratoryLevels = new int[2];

                    for (int i = 0; i < 2; i++)
                    {
                        LogicBuilding laboratory = this.m_level.GetGameObjectManagerAt(i).GetLaboratory();

                        if (laboratory != null)
                        {
                            laboratoryLevels[i] = laboratory.GetUpgradeLevel();
                        }
                    }

                    LogicDataTable characterTable = LogicDataTables.GetTable(LogicDataType.CHARACTER);

                    for (int i = 0; i < characterTable.GetItemCount(); i++)
                    {
                        LogicCharacterData characterData = (LogicCharacterData) characterTable.GetItemAt(i);

                        int upgradeLevel = homeOwnerAvatar.GetUnitUpgradeLevel(characterData);
                        int villageType = characterData.GetVillageType();
                        int newUpgradeLevel = upgradeLevel;

                        if (upgradeLevel >= characterData.GetUpgradeLevelCount())
                        {
                            newUpgradeLevel = characterData.GetUpgradeLevelCount() - 1;
                        }

                        int laboratoryLevel = laboratoryLevels[villageType];
                        int requireLaboratoryLevel;

                        do
                        {
                            requireLaboratoryLevel = characterData.GetRequiredLaboratoryLevel(newUpgradeLevel--);
                        } while (newUpgradeLevel >= 0 && requireLaboratoryLevel > laboratoryLevel);

                        newUpgradeLevel += 1;

                        if (upgradeLevel > newUpgradeLevel)
                        {
                            homeOwnerAvatar.SetUnitUpgradeLevel(characterData, newUpgradeLevel);
                            homeOwnerAvatar.GetChangeListener().CommodityCountChanged(1, characterData, newUpgradeLevel);
                        }
                    }

                    LogicDataTable spellTable = LogicDataTables.GetTable(LogicDataType.SPELL);

                    for (int i = 0; i < spellTable.GetItemCount(); i++)
                    {
                        LogicSpellData spellData = (LogicSpellData) spellTable.GetItemAt(i);

                        int upgradeLevel = homeOwnerAvatar.GetUnitUpgradeLevel(spellData);
                        int villageType = spellData.GetVillageType();
                        int newUpgradeLevel = upgradeLevel;

                        if (upgradeLevel >= spellData.GetUpgradeLevelCount())
                        {
                            newUpgradeLevel = spellData.GetUpgradeLevelCount() - 1;
                        }

                        int laboratoryLevel = laboratoryLevels[villageType];
                        int requireLaboratoryLevel;

                        do
                        {
                            requireLaboratoryLevel = spellData.GetRequiredLaboratoryLevel(newUpgradeLevel--);
                        } while (newUpgradeLevel >= 0 && requireLaboratoryLevel > laboratoryLevel);

                        newUpgradeLevel += 1;

                        if (upgradeLevel > newUpgradeLevel)
                        {
                            homeOwnerAvatar.SetUnitUpgradeLevel(spellData, newUpgradeLevel);
                            homeOwnerAvatar.GetChangeListener().CommodityCountChanged(1, spellData, newUpgradeLevel);
                        }
                    }
                }
            }
        }

        public void CalculateLoot(bool includeStorage)
        {
            if (includeStorage)
            {
                LogicArrayList<LogicComponent> resourceStorageComponents = this.m_components[(int) LogicComponentType.RESOURCE_STORAGE];

                for (int i = 0; i < resourceStorageComponents.Size(); i++)
                {
                    ((LogicResourceStorageComponent) resourceStorageComponents[i]).RecalculateAvailableLoot();
                }
            }

            LogicArrayList<LogicComponent> resourceProductionComponents = this.m_components[(int) LogicComponentType.RESOURCE_PRODUCTION];

            for (int i = 0; i < resourceProductionComponents.Size(); i++)
            {
                ((LogicResourceProductionComponent) resourceProductionComponents[i]).RecalculateAvailableLoot();
            }

            LogicArrayList<LogicComponent> warResourceStorageComponents = this.m_components[(int) LogicComponentType.WAR_RESOURCE_STORAGE];
            Debugger.DoAssert(warResourceStorageComponents.Size() < 2, "Too many war storage components");

            for (int i = 0; i < warResourceStorageComponents.Size(); i++)
            {
                ((LogicWarResourceStorageComponent) warResourceStorageComponents[i]).RecalculateAvailableLoot();
            }
        }

        public void AddAvatarAllianceUnitsToCastle()
        {
            LogicGameObjectManager gameObjectManager = this.m_level.GetGameObjectManagerAt(0);
            LogicBuilding allianceCastle = gameObjectManager.GetAllianceCastle();

            if (allianceCastle != null)
            {
                LogicBunkerComponent bunkerComponent = allianceCastle.GetBunkerComponent();

                if (bunkerComponent != null)
                {
                    bunkerComponent.RemoveAllUnits();

                    LogicArrayList<LogicUnitSlot> units = this.m_level.GetHomeOwnerAvatar().GetAllianceUnits();

                    for (int i = 0; i < units.Size(); i++)
                    {
                        LogicUnitSlot unitSlot = units[i];
                        LogicCombatItemData data = (LogicCombatItemData) unitSlot.GetData();
                        int count = unitSlot.GetCount();

                        if (data != null)
                        {
                            if (data.GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
                            {
                                for (int j = 0; j < count; j++)
                                {
                                    if (bunkerComponent.GetUnusedCapacity() >= data.GetHousingSpace())
                                    {
                                        bunkerComponent.AddUnitImpl(data, unitSlot.GetLevel());
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debugger.Error("LogicComponentManager::addAvatarAllianceUnitsToCastle - NULL character");
                        }
                    }
                }
            }
        }

        public void DivideAvatarResourcesToStorages()
        {
            LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();

            if (homeOwnerAvatar != null)
            {
                LogicArrayList<LogicComponent> resourceStorageComponents = this.m_components[(int) LogicComponentType.RESOURCE_STORAGE];
                LogicArrayList<LogicComponent> warResourceStorageComponents = this.m_components[(int) LogicComponentType.WAR_RESOURCE_STORAGE];
                LogicDataTable resourceTable = LogicDataTables.GetTable(LogicDataType.RESOURCE);

                for (int i = 0; i < resourceTable.GetItemCount(); i++)
                {
                    LogicResourceData data = (LogicResourceData) resourceTable.GetItemAt(i);

                    if (!data.IsPremiumCurrency())
                    {
                        if (data.GetWarResourceReferenceData() != null)
                        {
                            Debugger.DoAssert(warResourceStorageComponents.Size() < 2, "Too many war storage components");

                            for (int j = 0; j < warResourceStorageComponents.Size(); j++)
                            {
                                LogicWarResourceStorageComponent warResourceStorageComponent = (LogicWarResourceStorageComponent) warResourceStorageComponents[j];
                                warResourceStorageComponent.SetCount(i, homeOwnerAvatar.GetResourceCount(data));
                            }
                        }
                        else
                        {
                            for (int j = 0; j < resourceStorageComponents.Size(); j++)
                            {
                                ((LogicResourceStorageComponent) resourceStorageComponents[j]).SetCount(i, 0);
                            }

                            int resourceCount = homeOwnerAvatar.GetResourceCount(data);

                            if (this.m_level.GetBattleLog() != null && data.GetVillageType() == 1)
                            {
                                resourceCount = LogicMath.Max(resourceCount - this.m_level.GetBattleLog().GetCostCount(data), 0);
                            }

                            this.AddResources(i, resourceCount, true);
                        }
                    }
                }
            }
        }

        public int AddResources(int idx, int count, bool useRecommencedMax)
        {
            LogicArrayList<LogicComponent> resourceStorageComponents = this.m_components[(int) LogicComponentType.RESOURCE_STORAGE];

            while (count > 0)
            {
                int freeComponentCount = 0;

                for (int i = 0; i < resourceStorageComponents.Size(); i++)
                {
                    LogicResourceStorageComponent resourceStorageComponent = (LogicResourceStorageComponent) resourceStorageComponents[i];

                    int cap = useRecommencedMax ? resourceStorageComponent.GetRecommendedMax(idx, count) : resourceStorageComponent.GetMax(idx);
                    int usedCount = resourceStorageComponent.GetCount(idx);

                    if (cap > usedCount)
                        ++freeComponentCount;
                }

                if (freeComponentCount > 0)
                {
                    int countPerStorage = (count + freeComponentCount - 1) / freeComponentCount;

                    for (int i = 0; i < resourceStorageComponents.Size(); i++)
                    {
                        LogicResourceStorageComponent resourceStorageComponent = (LogicResourceStorageComponent) resourceStorageComponents[i];

                        int cap = useRecommencedMax ? resourceStorageComponent.GetRecommendedMax(idx, count) : resourceStorageComponent.GetMax(idx);
                        int usedCount = resourceStorageComponent.GetCount(idx);
                        int unusedResourceCap = cap - usedCount;

                        if (unusedResourceCap > 0)
                        {
                            int resourceCount = LogicMath.Min(countPerStorage, count);

                            if (resourceCount >= unusedResourceCap)
                                --freeComponentCount;
                            if (resourceCount <= unusedResourceCap)
                                unusedResourceCap = resourceCount;
                            resourceStorageComponent.SetCount(idx, unusedResourceCap + usedCount);
                            count -= unusedResourceCap;

                            if (count == 0)
                                return 0;
                        }
                    }
                }

                if (freeComponentCount <= 0 && !useRecommencedMax)
                    return count;
                useRecommencedMax &= freeComponentCount > 0;
            }

            return 0;
        }

        public LogicArrayList<LogicComponent> GetComponents(LogicComponentType componentType)
        {
            return this.m_components[(int) componentType];
        }

        public void RemoveComponent(LogicComponent component)
        {
            LogicArrayList<LogicComponent> components = this.m_components[(int) component.GetComponentType()];

            for (int i = 0, j = components.Size(); i < j; i++)
            {
                if (components[i] == component)
                {
                    components.Remove(i);
                    break;
                }
            }
        }

        public void RemoveGameObjectReferences(LogicGameObject gameObject)
        {
            for (int i = 0; i < LogicComponent.COMPONENT_TYPE_COUNT; i++)
            {
                LogicArrayList<LogicComponent> components = this.m_components[(int) LogicComponentType.UNIT_STORAGE];

                for (int j = 0; j < components.Size(); j++)
                {
                    components[j].RemoveGameObjectReferences(gameObject);
                }
            }
        }

        public LogicComponent GetClosestComponent(int x, int y, LogicComponentFilter filter)
        {
            LogicArrayList<LogicComponent> components = this.m_components[(int) filter.GetComponentType()];
            LogicComponent closestComponent = null;

            int closestDistance = 0;

            for (int i = 0, size = components.Size(); i < size; i++)
            {
                LogicComponent component = components[i];

                if (filter.TestComponent(component))
                {
                    int distance = component.GetParent().GetPosition().GetDistanceSquaredTo(x, y);

                    if (distance < closestDistance || closestComponent == null)
                    {
                        closestDistance = distance;
                        closestComponent = component;
                    }
                }
            }

            return closestComponent;
        }

        public int GetTotalUsedHousing(int unitType)
        {
            LogicArrayList<LogicComponent> components = this.m_components[(int) LogicComponentType.UNIT_STORAGE];

            int housingSpace = 0;

            for (int i = 0; i < components.Size(); i++)
            {
                LogicUnitStorageComponent unitStorageComponent = (LogicUnitStorageComponent) components[i];

                if (unitStorageComponent.GetStorageType() == unitType)
                {
                    housingSpace += unitStorageComponent.GetUsedCapacity();
                }
            }

            return housingSpace;
        }

        public int GetTotalMaxHousing(int combatItemType)
        {
            LogicArrayList<LogicComponent> unitStorageComponents = this.m_components[(int) LogicComponentType.UNIT_STORAGE];
            LogicArrayList<LogicComponent> village2UnitComponents = this.m_components[(int) LogicComponentType.VILLAGE2_UNIT];

            int housing = 0;

            for (int i = 0; i < unitStorageComponents.Size(); i++)
            {
                LogicUnitStorageComponent unitStorageComponent = (LogicUnitStorageComponent) unitStorageComponents[i];

                if (unitStorageComponent.GetStorageType() == combatItemType)
                {
                    housing += unitStorageComponent.GetMaxCapacity();
                }
            }

            return housing + (int) (village2UnitComponents.Size() * 30f);
        }

        public int GetMaxBarrackLevel()
        {
            LogicArrayList<LogicComponent> components = this.m_components[(int) LogicComponentType.UNIT_PRODUCTION];

            if (components.Size() > 0)
            {
                int maxUpgLevel = -1;
                int idx = 0;

                do
                {
                    LogicUnitProductionComponent component = (LogicUnitProductionComponent) components[idx];

                    if (component.GetProductionType() == 0)
                    {
                        if (component.GetParent().GetGameObjectType() == LogicGameObjectType.BUILDING)
                        {
                            LogicBuilding parent = (LogicBuilding) component.GetParent();

                            if (parent.GetBuildingData().GetProducesUnitsOfType() == 1)
                            {
                                maxUpgLevel = LogicMath.Max(parent.GetUpgradeLevel(), maxUpgLevel);
                            }
                        }
                    }
                } while (++idx != components.Size());

                return maxUpgLevel;
            }

            return -1;
        }

        public int GetMaxDarkBarrackLevel()
        {
            LogicArrayList<LogicComponent> components = this.m_components[(int) LogicComponentType.UNIT_PRODUCTION];

            if (components.Size() > 0)
            {
                int maxUpgLevel = -1;
                int idx = 0;

                do
                {
                    LogicUnitProductionComponent component = (LogicUnitProductionComponent) components[idx];

                    if (component.GetProductionType() == 0)
                    {
                        if (component.GetParent().GetGameObjectType() == LogicGameObjectType.BUILDING)
                        {
                            LogicBuilding parent = (LogicBuilding) component.GetParent();

                            if (parent.GetBuildingData().GetProducesUnitsOfType() == 2 && (!parent.IsConstructing() || parent.IsUpgrading()))
                            {
                                maxUpgLevel = LogicMath.Max(parent.GetUpgradeLevel(), maxUpgLevel);
                            }
                        }
                    }
                } while (++idx != components.Size());

                return maxUpgLevel;
            }

            return -1;
        }

        public int GetMaxSpellForgeLevel()
        {
            LogicArrayList<LogicComponent> components = this.m_components[(int) LogicComponentType.UNIT_PRODUCTION];

            if (components.Size() > 0)
            {
                int maxUpgLevel = -1;
                int idx = 0;

                do
                {
                    LogicUnitProductionComponent component = (LogicUnitProductionComponent) components[idx];

                    if (component.GetProductionType() != 0)
                    {
                        if (component.GetParent().GetGameObjectType() == LogicGameObjectType.BUILDING)
                        {
                            LogicBuilding parent = (LogicBuilding) component.GetParent();

                            if (parent.GetBuildingData().GetProducesUnitsOfType() == 1 && (!parent.IsConstructing() || parent.IsUpgrading()))
                            {
                                maxUpgLevel = LogicMath.Max(parent.GetUpgradeLevel(), maxUpgLevel);
                            }
                        }
                    }
                } while (++idx != components.Size());

                return maxUpgLevel;
            }

            return -1;
        }

        public int GetMaxMiniSpellForgeLevel()
        {
            LogicArrayList<LogicComponent> components = this.m_components[(int) LogicComponentType.UNIT_PRODUCTION];

            if (components.Size() > 0)
            {
                int maxUpgLevel = -1;
                int idx = 0;

                do
                {
                    LogicUnitProductionComponent component = (LogicUnitProductionComponent) components[idx];

                    if (component.GetProductionType() != 0)
                    {
                        if (component.GetParent().GetGameObjectType() == LogicGameObjectType.BUILDING)
                        {
                            LogicBuilding parent = (LogicBuilding) component.GetParent();

                            if (parent.GetBuildingData().GetProducesUnitsOfType() == 2 && (!parent.IsConstructing() || parent.IsUpgrading()))
                            {
                                maxUpgLevel = LogicMath.Max(parent.GetUpgradeLevel(), maxUpgLevel);
                            }
                        }
                    }
                } while (++idx != components.Size());

                return maxUpgLevel;
            }

            return -1;
        }

        public void DivideAvatarUnitsToStorages(int villageType)
        {
            if (this.m_level.GetHomeOwnerAvatar() != null)
            {
                if (villageType == 1)
                {
                    for (int i = this.m_units.Size() - 1; i >= 0; i--)
                    {
                        this.m_units[i].Destruct();
                        this.m_units.Remove(i);
                    }

                    LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();
                    LogicArrayList<LogicComponent> components = this.m_components[(int) LogicComponentType.VILLAGE2_UNIT];

                    if (homeOwnerAvatar.GetUnitsNewTotalVillage2() <= 0 || LogicDataTables.GetGlobals().Village2TrainingOnlyUseRegularStorage())
                    {
                        for (int i = 0; i < components.Size(); i++)
                        {
                            LogicVillage2UnitComponent village2UnitComponent = (LogicVillage2UnitComponent) components[i];
                            LogicCombatItemData unitData = village2UnitComponent.GetUnitData();

                            int idx = -1;

                            for (int j = 0; j < this.m_units.Size(); j++)
                            {
                                if (this.m_units[j].GetData() == unitData)
                                {
                                    idx = j;
                                    break;
                                }
                            }

                            if (idx == -1)
                            {
                                this.m_units.Add(new LogicDataSlot(unitData, -village2UnitComponent.GetUnitCount()));
                            }
                            else
                            {
                                this.m_units[idx].SetCount(this.m_units[idx].GetCount() - village2UnitComponent.GetUnitCount());
                            }
                        }

                        LogicArrayList<LogicDataSlot> units = homeOwnerAvatar.GetUnitsVillage2();

                        for (int i = 0; i < units.Size(); i++)
                        {
                            LogicDataSlot slot = units[i];
                            LogicCombatItemData data = (LogicCombatItemData) slot.GetData();

                            int count = slot.GetCount();
                            int idx = -1;

                            for (int j = 0; j < this.m_units.Size(); j++)
                            {
                                if (this.m_units[j].GetData() == data)
                                {
                                    idx = j;
                                    break;
                                }
                            }

                            if (idx == -1)
                            {
                                this.m_units.Add(new LogicDataSlot(data, count));
                            }
                            else
                            {
                                this.m_units[idx].SetCount(this.m_units[idx].GetCount() + count);
                            }
                        }

                        for (int i = 0; i < this.m_units.Size(); i++)
                        {
                            LogicDataSlot slot = this.m_units[i];
                            LogicCharacterData unitData = (LogicCharacterData) slot.GetData();

                            int unitCount = slot.GetCount();

                            if (unitCount != 0)
                            {
                                for (int j = 0; j < components.Size(); j++)
                                {
                                    LogicVillage2UnitComponent component = (LogicVillage2UnitComponent) components[j];

                                    if (component.GetUnitData() == unitData)
                                    {
                                        int highestBuildingLevel = this.m_level.GetGameObjectManagerAt(1).GetHighestBuildingLevel(unitData.GetProductionHouseData());

                                        if (unitData.IsUnlockedForProductionHouseLevel(highestBuildingLevel))
                                        {
                                            if (unitCount < 0)
                                            {
                                                int count = component.GetUnitCount();

                                                if (count >= -unitCount)
                                                {
                                                    component.SetUnit(unitData, LogicMath.Max(0, count + unitCount));
                                                    unitCount += count;
                                                }
                                                else
                                                {
                                                    component.SetUnit(unitData, 0);
                                                    unitCount += count;
                                                }
                                            }
                                            else
                                            {
                                                int maxUnits = component.GetMaxUnitsInCamp(unitData);
                                                int addCount = LogicMath.Min(maxUnits, unitCount);

                                                component.SetUnit(unitData, addCount);
                                                unitCount -= addCount;
                                            }

                                            component.TrainUnit(unitData);
                                        }
                                        else
                                        {
                                            component.RemoveUnits();
                                        }
                                    }

                                    if (unitCount == 0)
                                    {
                                        break;
                                    }
                                }

                                if (unitCount > 0)
                                {
                                    homeOwnerAvatar.SetUnitCountVillage2(unitData, 0);
                                    homeOwnerAvatar.GetChangeListener().CommodityCountChanged(7, unitData, 0);
                                }
                            }
                        }
                    }
                    else
                    {
                        LogicArrayList<LogicDataSlot> unitsNew = homeOwnerAvatar.GetUnitsNewVillage2();

                        for (int i = 0; i < unitsNew.Size(); i++)
                        {
                            LogicDataSlot slot = unitsNew[i];
                            LogicCharacterData data = (LogicCharacterData) slot.GetData();

                            int count = slot.GetCount();
                            int index = -1;

                            for (int j = 0; j < this.m_units.Size(); j++)
                            {
                                if (this.m_units[j].GetData() == data)
                                {
                                    index = j;
                                    break;
                                }
                            }

                            if (count > 0)
                            {
                                if (index != -1)
                                {
                                    this.m_units[index].SetCount(this.m_units[index].GetCount() + count);
                                    this.m_units.Add(new LogicDataSlot(data, count));
                                }
                                else
                                {
                                    this.m_units.Add(new LogicDataSlot(data, count));
                                }
                            }
                        }

                        for (int i = 0; i < this.m_units.Size(); i++)
                        {
                            homeOwnerAvatar.CommodityCountChangeHelper(8, this.m_units[i].GetData(), -this.m_units[i].GetCount());
                        }

                        for (int i = 0; i < this.m_units.Size(); i++)
                        {
                            LogicDataSlot slot = this.m_units[i];
                            LogicCharacterData data = (LogicCharacterData) slot.GetData();

                            int count = slot.GetCount();

                            if (count > 0)
                            {
                                for (int j = 0; j < components.Size(); j++)
                                {
                                    LogicVillage2UnitComponent village2UnitComponent = (LogicVillage2UnitComponent) components[j];

                                    int maxUnitsInCamp = village2UnitComponent.GetMaxUnitsInCamp(data);
                                    int addCount = LogicMath.Min(count, maxUnitsInCamp);

                                    village2UnitComponent.SetUnit(data, addCount);
                                    village2UnitComponent.TrainUnit(data);

                                    count -= addCount;

                                    if (count <= 0)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int i = this.m_units.Size() - 1; i >= 0; i--)
                    {
                        this.m_units[i].Destruct();
                        this.m_units.Remove(i);
                    }

                    LogicArrayList<LogicComponent> components = this.m_components[(int) LogicComponentType.UNIT_STORAGE];

                    for (int i = 0; i < components.Size(); i++)
                    {
                        LogicUnitStorageComponent storageComponent = (LogicUnitStorageComponent) components[i];

                        for (int j = 0; j < storageComponent.GetUnitTypeCount(); j++)
                        {
                            LogicCombatItemData unitType = storageComponent.GetUnitType(j);
                            int unitCount = storageComponent.GetUnitCount(j);
                            int index = -1;

                            for (int k = 0; k < this.m_units.Size(); k++)
                            {
                                LogicDataSlot tmp = this.m_units[k];

                                if (tmp.GetData() == unitType)
                                {
                                    index = k;
                                    break;
                                }
                            }

                            if (index != -1)
                            {
                                this.m_units[index].SetCount(this.m_units[index].GetCount() - unitCount);
                            }
                            else
                            {
                                this.m_units.Add(new LogicDataSlot(unitType, -unitCount));
                            }
                        }
                    }

                    LogicArrayList<LogicDataSlot> units = this.m_level.GetHomeOwnerAvatar().GetUnits();

                    for (int i = 0; i < units.Size(); i++)
                    {
                        LogicDataSlot slot = units[i];
                        int index = -1;

                        for (int j = 0; j < this.m_units.Size(); j++)
                        {
                            LogicDataSlot tmp = this.m_units[j];

                            if (tmp.GetData() == slot.GetData())
                            {
                                index = j;
                                break;
                            }
                        }

                        if (index != -1)
                        {
                            this.m_units[index].SetCount(this.m_units[index].GetCount() + slot.GetCount());
                        }
                        else
                        {
                            this.m_units.Add(new LogicDataSlot(slot.GetData(), slot.GetCount()));
                        }
                    }

                    LogicArrayList<LogicDataSlot> spells = this.m_level.GetHomeOwnerAvatar().GetSpells();

                    for (int i = 0; i < spells.Size(); i++)
                    {
                        LogicDataSlot slot = spells[i];
                        int index = -1;

                        for (int j = 0; j < this.m_units.Size(); j++)
                        {
                            LogicDataSlot tmp = this.m_units[j];

                            if (tmp.GetData() == slot.GetData())
                            {
                                index = j;
                                break;
                            }
                        }

                        if (index != -1)
                        {
                            this.m_units[index].SetCount(this.m_units[index].GetCount() + slot.GetCount());
                        }
                        else
                        {
                            this.m_units.Add(new LogicDataSlot(slot.GetData(), slot.GetCount()));
                        }
                    }

                    for (int i = 0; i < this.m_units.Size(); i++)
                    {
                        LogicDataSlot slot = this.m_units[i];
                        LogicCombatItemData data = (LogicCombatItemData) slot.GetData();
                        int unitCount = slot.GetCount();

                        if (unitCount != 0)
                        {
                            for (int j = 0; j < components.Size(); j++)
                            {
                                LogicUnitStorageComponent unitStorageComponent = (LogicUnitStorageComponent) components[j];

                                if (unitCount >= 0)
                                {
                                    while (unitStorageComponent.CanAddUnit(data))
                                    {
                                        unitStorageComponent.AddUnit(data);

                                        if (--unitCount <= 0)
                                        {
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    int idx = unitStorageComponent.GetUnitTypeIndex(data);

                                    if (idx != -1)
                                    {
                                        int count = unitStorageComponent.GetUnitCount(idx);

                                        if (count < -unitCount)
                                        {
                                            unitStorageComponent.RemoveUnits(data, count);
                                            unitCount += count;
                                        }
                                        else
                                        {
                                            unitStorageComponent.RemoveUnits(data, -unitCount);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool IsHeroUnlocked(LogicHeroData data)
        {
            LogicArrayList<LogicComponent> components = this.m_components[(int) LogicComponentType.HERO_BASE];

            for (int i = 0; i < components.Size(); i++)
            {
                LogicHeroBaseComponent component = (LogicHeroBaseComponent) components[i];

                if (component.GetHeroData() == data)
                {
                    LogicBuilding building = (LogicBuilding) component.GetParent();

                    if (!building.IsLocked())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void DebugVillage2UnitAdded(bool updateComponents)
        {
            LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();

            if (homeOwnerAvatar != null)
            {
                if (updateComponents)
                {
                    LogicArrayList<LogicComponent> components = this.m_components[(int) LogicComponentType.VILLAGE2_UNIT];
                    LogicArrayList<LogicDataSlot> units = homeOwnerAvatar.GetUnitsVillage2();

                    for (int i = 0; i < LogicMath.Min(components.Size(), units.Size()); i++)
                    {
                        LogicVillage2UnitComponent component = (LogicVillage2UnitComponent) components[i];
                        LogicDataSlot unitSlot = units[i];
                        LogicCharacterData characterData = (LogicCharacterData) unitSlot.GetData();

                        component.RemoveUnits();
                        component.SetUnit(characterData, component.GetMaxUnitsInCamp(characterData));
                    }
                }
            }
        }

        public void Tick()
        {
            bool isInCombatState = this.m_level.IsInCombatState();

            for (int i = 0; i < LogicComponent.COMPONENT_TYPE_COUNT; i++)
            {
                LogicArrayList<LogicComponent> components = this.m_components[i];

                for (int j = 0, size = components.Size(); j < size; j++)
                {
                    LogicComponent component = components[j];

                    if (component.IsEnabled())
                    {
                        component.Tick();
                    }
                }

                if (i == 0 && !isInCombatState)
                {
                    i = 1;
                }
            }
        }

        public void SubTick()
        {
        }
    }
}