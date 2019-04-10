namespace Supercell.Magic.Logic.Unit
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Time;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class LogicUnitProduction
    {
        public const int TUTORIAL_MAX_CAPACITY = 20;

        private LogicLevel m_level;
        private LogicTimer m_timer;
        private LogicTimer m_boostTimer;
        private readonly LogicArrayList<LogicUnitProductionSlot> m_slots;

        private readonly int m_villageType;
        private readonly LogicDataType m_unitProductionType;
        private int m_nextProduction;

        private bool m_locked;
        private bool m_boostPause;

        public LogicUnitProduction(LogicLevel level, LogicDataType unitProductionType, int villageType)
        {
            this.m_level = level;
            this.m_villageType = villageType;
            this.m_unitProductionType = unitProductionType;

            this.m_slots = new LogicArrayList<LogicUnitProductionSlot>();
        }

        public void Destruct()
        {
            for (int i = this.m_slots.Size() - 1; i >= 0; i--)
            {
                this.m_slots[i].Destruct();
                this.m_slots.Remove(i);
            }

            if (this.m_timer != null)
            {
                this.m_timer.Destruct();
                this.m_timer = null;
            }

            if (this.m_boostTimer != null)
            {
                this.m_boostTimer.Destruct();
                this.m_boostTimer = null;
            }

            this.m_level = null;
        }

        public LogicDataType GetUnitProductionType()
        {
            return this.m_unitProductionType;
        }

        public bool IsLocked()
        {
            return this.m_locked;
        }

        public void SetLocked(bool state)
        {
            this.m_locked = state;
        }

        public bool IsBoostPaused()
        {
            return this.m_boostPause;
        }

        public void SetBoostPause(bool state)
        {
            this.m_boostPause = state;
        }

        public int GetRemainingBoostTimeSecs()
        {
            if (this.m_boostTimer != null)
            {
                return this.m_boostTimer.GetRemainingSeconds(this.m_level.GetLogicTime());
            }

            return 0;
        }

        public int GetMaxBoostTimeSecs()
        {
            if (this.m_unitProductionType == LogicDataType.SPELL)
            {
                return LogicDataTables.GetGlobals().GetSpellFactoryBoostSecs();
            }

            if (this.m_unitProductionType == LogicDataType.CHARACTER)
            {
                return LogicDataTables.GetGlobals().GetBarracksBoostSecs();
            }

            return 0;
        }


        public int GetRemainingSeconds()
        {
            if (this.m_timer != null)
            {
                return this.m_timer.GetRemainingSeconds(this.m_level.GetLogicTime());
            }

            return 0;
        }

        public int GetRemainingMS()
        {
            if (this.m_timer != null)
            {
                return this.m_timer.GetRemainingMS(this.m_level.GetLogicTime());
            }

            return 0;
        }

        public int GetTotalSeconds()
        {
            LogicUnitProductionSlot slot = null;

            for (int i = 0; i < this.m_slots.Size(); i++)
            {
                LogicUnitProductionSlot tmp = this.m_slots[i];

                if (!tmp.IsTerminate())
                {
                    slot = tmp;
                    break;
                }
            }

            if (slot != null)
            {
                LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();
                LogicCombatItemData data = (LogicCombatItemData) slot.GetData();

                return data.GetTrainingTime(homeOwnerAvatar.GetUnitUpgradeLevel(data), this.m_level, 0);
            }

            return 0;
        }

        public int GetTotalRemainingSeconds()
        {
            LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();
            LogicComponentManager componentManager = this.m_level.GetComponentManagerAt(this.m_villageType);

            int totalMaxHousing = componentManager.GetTotalMaxHousing(this.m_unitProductionType != LogicDataType.CHARACTER ? 1 : 0);
            int totalUsedCapacity = this.m_unitProductionType == LogicDataType.CHARACTER ? homeOwnerAvatar.GetUnitsTotalCapacity() : homeOwnerAvatar.GetSpellsTotalCapacity();
            int freeCapacity = totalMaxHousing - totalUsedCapacity;
            int remainingSecs = 0;

            for (int i = 0; i < this.m_slots.Size(); i++)
            {
                LogicUnitProductionSlot slot = this.m_slots[i];
                LogicCombatItemData data = (LogicCombatItemData) slot.GetData();
                int housingSpace = data.GetHousingSpace();
                int count = slot.GetCount();

                if (count > 0)
                {
                    if (i == 0)
                    {
                        if (!slot.IsTerminate() && freeCapacity - housingSpace >= 0)
                        {
                            if (this.m_timer != null)
                            {
                                remainingSecs += this.m_timer.GetRemainingSeconds(this.m_level.GetLogicTime());
                            }
                        }

                        freeCapacity -= housingSpace;
                        count -= 1;
                    }

                    for (int j = 0; j < count; j++)
                    {
                        if (!slot.IsTerminate() && freeCapacity - housingSpace >= 0)
                        {
                            remainingSecs += data.GetTrainingTime(homeOwnerAvatar.GetUnitUpgradeLevel(data), this.m_level, 0);
                        }

                        freeCapacity -= housingSpace;
                    }
                }
            }

            return remainingSecs;
        }

        public bool IsTutorialCapacityFull()
        {
            return this.m_level.GetHomeOwnerAvatar().GetUnitsTotalCapacity() + this.GetTotalCount() >= LogicUnitProduction.TUTORIAL_MAX_CAPACITY;
        }

        public int GetMaxTrainCount()
        {
            return this.m_level.GetComponentManagerAt(this.m_villageType).GetTotalMaxHousing(this.m_unitProductionType != LogicDataType.CHARACTER ? 1 : 0) * 2;
        }

        public int GetTutorialMax()
        {
            return LogicUnitProduction.TUTORIAL_MAX_CAPACITY;
        }

        public int GetTutorialCount()
        {
            return this.m_level.GetHomeOwnerAvatar().GetUnitsTotalCapacity() + this.GetTotalCount();
        }

        public int GetSlotCount()
        {
            return this.m_slots.Size();
        }

        public int GetTrainingCount(int idx)
        {
            return this.m_slots[idx].GetCount();
        }

        public LogicCombatItemData GetCurrentlyTrainedUnit()
        {
            for (int i = 0; i < this.m_slots.Size(); i++)
            {
                if (!this.m_slots[i].IsTerminate())
                {
                    return (LogicCombatItemData) this.m_slots[i].GetData();
                }
            }

            return null;
        }

        public int GetCurrentlyTrainedIndex()
        {
            for (int i = 0; i < this.m_slots.Size(); i++)
            {
                if (!this.m_slots[i].IsTerminate())
                {
                    return i;
                }
            }

            return -1;
        }

        public int GetWaitingForSpaceUnitCount(LogicCombatItemData data)
        {
            int count = 0;

            for (int i = 0; i < this.m_slots.Size(); i++)
            {
                LogicUnitProductionSlot slot = this.m_slots[i];

                if (slot.GetData() == data)
                {
                    if (slot.IsTerminate())
                    {
                        count += slot.GetCount();
                    }
                }
            }

            return count;
        }

        public LogicUnitProductionSlot GetCurrentlyTrainedSlot()
        {
            for (int i = 0; i < this.m_slots.Size(); i++)
            {
                if (!this.m_slots[i].IsTerminate())
                {
                    return this.m_slots[i];
                }
            }

            return null;
        }

        public LogicCombatItemData GetWaitingForSpaceUnit()
        {
            if (this.m_slots.Size() > 0)
            {
                LogicUnitProductionSlot slot = this.m_slots[0];

                if (slot.IsTerminate() || this.m_timer != null && this.m_timer.GetRemainingSeconds(this.m_level.GetLogicTime()) == 0)
                {
                    return (LogicCombatItemData) slot.GetData();
                }
            }

            return null;
        }

        public int GetBoostMultiplier()
        {
            if (this.m_unitProductionType == LogicDataType.SPELL)
            {
                if (LogicDataTables.GetGlobals().UseNewTraining())
                {
                    return LogicDataTables.GetGlobals().GetSpellFactoryBoostNewMultiplier();
                }

                return LogicDataTables.GetGlobals().GetSpellFactoryBoostMultiplier();
            }

            if (this.m_unitProductionType != LogicDataType.CHARACTER)
            {
                return 1;
            }

            if (LogicDataTables.GetGlobals().UseNewTraining())
            {
                return LogicDataTables.GetGlobals().GetBarracksBoostNewMultiplier();
            }

            return LogicDataTables.GetGlobals().GetBarracksBoostMultiplier();
        }

        public bool ProductionCompleted(bool speedUp)
        {
            bool success = false;

            if (!this.m_locked)
            {
                LogicComponentFilter filter = new LogicComponentFilter();

                filter.SetComponentType(0);

                while (true)
                {
                    LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();
                    LogicComponentManager componentManager = this.m_level.GetComponentManagerAt(this.m_villageType);
                    LogicCombatItemData productionData = this.GetWaitingForSpaceUnit();

                    if (speedUp)
                    {
                        if (this.m_slots.Size() <= 0)
                        {
                            return false;
                        }

                        productionData = (LogicCombatItemData) this.m_slots[0].GetData();
                    }

                    if (productionData == null)
                    {
                        filter.Destruct();
                        return false;
                    }

                    bool productionTerminate = this.m_slots[0].IsTerminate();
                    LogicBuildingData buildingProductionData = productionData.GetProductionHouseData();
                    LogicGameObjectManager gameObjectManager = this.m_level.GetGameObjectManagerAt(this.m_villageType);
                    LogicBuilding productionHouse = gameObjectManager.GetHighestBuilding(buildingProductionData);

                    if (LogicDataTables.GetGlobals().UseTroopWalksOutFromTraining())
                    {
                        int gameObjectCount = gameObjectManager.GetNumGameObjects();

                        for (int i = 0; i < gameObjectCount; i++)
                        {
                            LogicGameObject gameObject = gameObjectManager.GetGameObjectByIndex(i);

                            if (gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                            {
                                LogicBuilding building = (LogicBuilding) gameObject;
                                LogicUnitProductionComponent unitProductionComponent = building.GetUnitProductionComponent();

                                if (unitProductionComponent != null)
                                {
                                    if (unitProductionComponent.GetProductionType() == productionData.GetCombatItemType())
                                    {
                                        if (building.GetBuildingData().GetProducesUnitsOfType() == productionData.GetUnitOfType() &&
                                            !building.IsUpgrading() &&
                                            !building.IsConstructing())
                                        {
                                            if (productionData.IsUnlockedForProductionHouseLevel(building.GetUpgradeLevel()))
                                            {
                                                if (productionHouse != null)
                                                {
                                                    int seed = this.m_level.GetPlayerAvatar().GetExpPoints();

                                                    if (building.Rand(seed) % 1000 > 750)
                                                    {
                                                        productionHouse = building;
                                                    }
                                                }
                                                else
                                                {
                                                    productionHouse = building;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (productionHouse != null)
                    {
                        LogicUnitStorageComponent unitStorageComponent =
                            (LogicUnitStorageComponent) componentManager.GetClosestComponent(productionHouse.GetX(), productionHouse.GetY(), filter);

                        if (unitStorageComponent != null)
                        {
                            if (unitStorageComponent.CanAddUnit(productionData))
                            {
                                homeOwnerAvatar.CommodityCountChangeHelper(0, productionData, 1);
                                unitStorageComponent.AddUnit(productionData);

                                if (productionTerminate)
                                {
                                    this.RemoveUnit(productionData, -1);
                                }
                                else
                                {
                                    this.StartProducingNextUnit();
                                }

                                success = true;

                                if (this.m_slots.Size() > 0 && this.m_slots[0].IsTerminate() && this.m_slots[0].GetCount() > 0)
                                {
                                    continue;
                                }

                                break;
                            }

                            filter.AddIgnoreObject(unitStorageComponent.GetParent());
                        }
                        else
                        {
                            if (this.m_timer != null && this.m_timer.GetRemainingSeconds(this.m_level.GetLogicTime()) == 0)
                            {
                                success = this.TrainingFinished();
                            }

                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                filter.Destruct();

                if (success)
                {
                    this.m_nextProduction = 0;
                }
                else
                {
                    this.m_nextProduction = 2000;
                }
            }

            return success;
        }

        public void RemoveTrainedUnit(LogicCombatItemData data)
        {
            int idx = -1;

            for (int i = 0; i < this.m_slots.Size(); i++)
            {
                if (this.m_slots[i].GetData() == data)
                {
                    if (this.m_slots[i].IsTerminate())
                    {
                        idx = i;
                        break;
                    }
                }
            }

            if (idx != -1)
            {
                LogicUnitProductionSlot slot = this.m_slots[idx];

                if (slot.GetCount() > 1)
                {
                    slot.SetCount(slot.GetCount() - 1);
                }
                else
                {
                    this.m_slots.Remove(idx);

                    slot.Destruct();
                    slot = null;
                }

                this.MergeSlots();

                if (this.GetWaitingForSpaceUnit() != null)
                {
                    this.ProductionCompleted(false);
                }
            }
        }

        public bool RemoveUnit(LogicCombatItemData data, int index)
        {
            LogicUnitProductionSlot slot = null;
            bool removed = false;

            if (index > -1 &&
                this.m_slots.Size() > index &&
                this.m_slots[index].GetData() == data)
            {
                slot = this.m_slots[index];
            }
            else
            {
                index = -1;

                for (int i = 0; i < this.m_slots.Size(); i++)
                {
                    LogicUnitProductionSlot tmp = this.m_slots[i];

                    if (tmp.GetData() == data)
                    {
                        index = i;
                        break;
                    }
                }

                if (index == -1)
                {
                    return false;
                }

                slot = this.m_slots[index];
            }

            int count = slot.GetCount();

            if (count > 0)
            {
                removed = true;
                slot.SetCount(count - 1);

                if (count == 1)
                {
                    int prodIdx = this.GetCurrentlyTrainedIndex();

                    if (prodIdx == index)
                    {
                        if (this.m_timer != null)
                        {
                            this.m_timer.Destruct();
                            this.m_timer = null;
                        }
                    }

                    this.m_slots[index].Destruct();
                    this.m_slots.Remove(index);
                }
            }

            if (this.m_slots.Size() > 0)
            {
                LogicUnitProductionSlot productionSlot = this.GetCurrentlyTrainedSlot();

                if (productionSlot == null || this.m_timer != null)
                {
                    if (!removed)
                    {
                        return false;
                    }

                    this.MergeSlots();
                }
                else
                {
                    LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();
                    LogicCombatItemData productionData = (LogicCombatItemData) productionSlot.GetData();

                    this.m_timer = new LogicTimer();
                    this.m_timer.StartTimer(productionData.GetTrainingTime(homeOwnerAvatar.GetUnitUpgradeLevel(productionData), this.m_level, 0), this.m_level.GetLogicTime(),
                                            false,
                                            -1);

                    if (removed)
                    {
                        this.MergeSlots();
                    }
                }
            }
            else
            {
                if (!removed)
                {
                    return false;
                }

                this.MergeSlots();
            }

            return true;
        }

        public void SpeedUp()
        {
            LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();
            LogicComponentManager componentManager = this.m_level.GetComponentManagerAt(this.m_villageType);

            int totalMaxHousing = componentManager.GetTotalMaxHousing(this.m_unitProductionType != LogicDataType.CHARACTER ? 1 : 0);
            int totalUsedCapacity = this.m_unitProductionType == LogicDataType.CHARACTER ? homeOwnerAvatar.GetUnitsTotalCapacity() : homeOwnerAvatar.GetSpellsTotalCapacity();
            int freeCapacity = totalMaxHousing - totalUsedCapacity;

            bool armyCampFull = false;

            while (!armyCampFull && this.m_slots.Size() > 0)
            {
                LogicUnitProductionSlot slot = this.m_slots[0];
                LogicCombatItemData data = (LogicCombatItemData) slot.GetData();

                int count = slot.GetCount();

                if (count <= 0)
                {
                    break;
                }

                armyCampFull = true;

                do
                {
                    freeCapacity -= data.GetHousingSpace();

                    if (freeCapacity >= 0)
                    {
                        this.ProductionCompleted(true);
                        armyCampFull = false;
                    }
                } while (--count > 0);
            }
        }

        public LogicUnitProductionSlot GetUnit(int index)
        {
            if (index > -1 && this.m_slots.Size() > index)
            {
                return this.m_slots[index];
            }

            return null;
        }

        public void MergeSlots()
        {
            LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();

            if (this.m_slots.Size() > 0)
            {
                if (this.m_slots.Size() > 1)
                {
                    for (int i = 1; i < this.m_slots.Size(); i++)
                    {
                        LogicUnitProductionSlot slot1 = this.m_slots[i];
                        LogicUnitProductionSlot slot2 = this.m_slots[i - 1];

                        if (slot1.GetData() == slot2.GetData())
                        {
                            if (slot1.IsTerminate() == slot2.IsTerminate())
                            {
                                this.m_slots.Remove(i--);

                                slot2.SetCount(slot2.GetCount() + slot1.GetCount());
                                slot1.Destruct();
                                slot1 = null;
                            }
                        }
                    }
                }
            }

            LogicComponentManager componentManager = this.m_level.GetComponentManagerAt(this.m_villageType);

            int usedCapacity = this.m_unitProductionType == LogicDataType.SPELL ? homeOwnerAvatar.GetSpellsTotalCapacity() : homeOwnerAvatar.GetUnitsTotalCapacity();
            int totalCapacity = componentManager.GetTotalMaxHousing(this.m_unitProductionType != LogicDataType.CHARACTER ? 1 : 0);
            int freeCapacity = totalCapacity - usedCapacity;

            for (int i = 0, j = freeCapacity; i < this.m_slots.Size(); i++)
            {
                LogicUnitProductionSlot slot = this.m_slots[i];
                LogicCombatItemData data = (LogicCombatItemData) slot.GetData();

                int count = slot.GetCount();
                int housingSpace = data.GetHousingSpace() * count;

                if (j < housingSpace)
                {
                    if (count > 1)
                    {
                        int maxInProduction = j / data.GetHousingSpace();

                        if (maxInProduction > 0)
                        {
                            int inQueue = count - maxInProduction;

                            if (inQueue > 0)
                            {
                                slot.SetCount(maxInProduction);
                                this.m_slots.Add(i + 1, new LogicUnitProductionSlot(data, inQueue, slot.IsTerminate()));
                            }
                        }
                    }

                    break;
                }

                j -= housingSpace;
            }
        }

        public void StartProducingNextUnit()
        {
            if (this.m_timer != null)
            {
                this.m_timer.Destruct();
                this.m_timer = null;
            }

            if (this.m_slots.Size() > 0)
            {
                LogicCombatItemData unitData = this.GetCurrentlyTrainedUnit();

                if (unitData != null)
                {
                    this.RemoveUnit(unitData, -1);
                }
            }
        }

        public bool TrainingFinished()
        {
            bool success = false;

            if (this.m_timer != null)
            {
                this.m_timer.Destruct();
                this.m_timer = null;
            }

            if (this.m_slots.Size() > 0)
            {
                LogicUnitProductionSlot prodSlot = this.GetCurrentlyTrainedSlot();
                int prodIdx = this.GetCurrentlyTrainedIndex();

                if (prodSlot != null)
                {
                    if (prodSlot.GetCount() == 1)
                    {
                        prodSlot.SetTerminate(true);
                    }
                    else
                    {
                        prodSlot.SetCount(prodSlot.GetCount() - 1);

                        LogicUnitProductionSlot previousSlot = this.m_slots[LogicMath.Max(prodIdx - 1, 0)];

                        if (previousSlot != null &&
                            previousSlot.IsTerminate() &&
                            previousSlot.GetData().GetGlobalID() == prodSlot.GetData().GetGlobalID())
                        {
                            previousSlot.SetCount(previousSlot.GetCount() + 1);
                        }
                        else
                        {
                            this.m_slots.Add(prodIdx, new LogicUnitProductionSlot(prodSlot.GetData(), 1, true));
                        }
                    }
                }

                if (this.m_slots.Size() > 0)
                {
                    LogicCombatItemData nextProductionData = this.GetCurrentlyTrainedUnit();

                    if (nextProductionData != null && this.m_timer == null)
                    {
                        this.m_timer = new LogicTimer();
                        this.m_timer.StartTimer(nextProductionData.GetTrainingTime(this.m_level.GetHomeOwnerAvatar().GetUnitUpgradeLevel(nextProductionData), this.m_level, 0),
                                                this.m_level.GetLogicTime(), false, -1);
                        success = true;
                    }
                }
            }

            this.MergeSlots();

            return success;
        }

        public bool DragSlot(int slotIdx, int dragIdx)
        {
            this.m_locked = false;

            if (slotIdx > -1 && slotIdx < this.m_slots.Size())
            {
                LogicCombatItemData productionData = this.GetCurrentlyTrainedUnit();
                LogicUnitProductionSlot slot = this.m_slots[slotIdx];

                this.m_slots.Remove(slotIdx);

                if (slot != null)
                {
                    if (slotIdx <= dragIdx)
                    {
                        dragIdx -= 1;
                    }

                    if (dragIdx >= 0 && dragIdx <= this.m_slots.Size())
                    {
                        this.m_slots.Add(dragIdx, slot);
                        this.MergeSlots();

                        LogicCombatItemData prodData = this.GetCurrentlyTrainedUnit();
                        int prodIdx = this.GetCurrentlyTrainedIndex();

                        if (productionData != prodData && (dragIdx >= prodIdx || prodIdx == slotIdx || prodIdx == dragIdx + 1))
                        {
                            if (this.m_timer != null)
                            {
                                this.m_timer.Destruct();
                                this.m_timer = null;
                            }

                            LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();

                            this.m_timer = new LogicTimer();
                            this.m_timer.StartTimer(prodData.GetTrainingTime(homeOwnerAvatar.GetUnitUpgradeLevel(prodData), this.m_level, 0),
                                                    this.m_level.GetLogicTime(), false, -1);
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public int AddUnitToQueue(LogicCombatItemData data)
        {
            if (data != null)
            {
                if (this.CanAddUnitToQueue(data, false))
                {
                    LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();

                    for (int i = this.m_slots.Size() - 1; i >= 0; i--)
                    {
                        LogicUnitProductionSlot tmp = this.m_slots[i];

                        if (tmp != null)
                        {
                            if (tmp.GetData() == data)
                            {
                                tmp.SetCount(tmp.GetCount() + 1);
                                this.MergeSlots();

                                return i;
                            }

                            break;
                        }
                    }

                    this.m_slots.Add(new LogicUnitProductionSlot(data, 1, false));
                    this.MergeSlots();

                    if (this.m_slots.Size() > 0)
                    {
                        LogicCombatItemData productionData = this.GetCurrentlyTrainedUnit();

                        if (productionData != null && this.m_timer == null)
                        {
                            this.m_timer = new LogicTimer();
                            this.m_timer.StartTimer(productionData.GetTrainingTime(homeOwnerAvatar.GetUnitUpgradeLevel(productionData), this.m_level, 0),
                                                    this.m_level.GetLogicTime(), false, -1);
                        }
                    }
                }
            }
            else
            {
                Debugger.Error("LogicUnitProduction - Trying to add NULL character!");
            }

            return -1;
        }

        public int AddUnitToQueue(LogicCombatItemData data, int index, bool ignoreCapacity)
        {
            if (data != null)
            {
                if (this.CanAddUnitToQueue(data, ignoreCapacity))
                {
                    LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();
                    LogicCombatItemData productionData = this.GetCurrentlyTrainedUnit();

                    this.m_slots.Add(index, new LogicUnitProductionSlot(data, 1, false));
                    this.MergeSlots();

                    if (productionData != null)
                    {
                        if (this.GetCurrentlyTrainedUnit() == data || this.GetCurrentlyTrainedIndex() != index)
                        {
                            return index;
                        }
                    }
                    else
                    {
                        productionData = this.GetCurrentlyTrainedUnit();
                    }

                    if (this.m_timer != null)
                    {
                        this.m_timer.Destruct();
                        this.m_timer = null;
                    }

                    this.m_timer = new LogicTimer();
                    this.m_timer.StartTimer(productionData.GetTrainingTime(homeOwnerAvatar.GetUnitUpgradeLevel(productionData), this.m_level, 0),
                                            this.m_level.GetLogicTime(), false, -1);

                    return index;
                }
            }
            else
            {
                Debugger.Error("LogicUnitProduction - Trying to add NULL character!");
            }

            return -1;
        }

        public bool CanAddUnitToQueue(LogicCombatItemData data, bool ignoreCapacity)
        {
            if (data != null)
            {
                if (data.GetDataType() == this.m_unitProductionType)
                {
                    LogicGameObjectManager gameObjectManager = this.m_level.GetGameObjectManagerAt(0);
                    LogicBuilding productionHouse = gameObjectManager.GetHighestBuilding(data.GetProductionHouseData());

                    if (productionHouse != null)
                    {
                        if (!data.IsUnlockedForProductionHouseLevel(productionHouse.GetUpgradeLevel()))
                        {
                            return false;
                        }

                        if (data.GetUnitOfType() != productionHouse.GetBuildingData().GetProducesUnitsOfType())
                        {
                            return false;
                        }
                    }

                    if (this.m_level.GetMissionManager().IsTutorialFinished() ||
                        this.m_level.GetHomeOwnerAvatar().GetUnitsTotalCapacity() + this.GetTotalCount() < LogicUnitProduction.TUTORIAL_MAX_CAPACITY)
                    {
                        if (ignoreCapacity)
                        {
                            return true;
                        }

                        LogicAvatar avatar = this.m_level.GetHomeOwnerAvatar();
                        LogicComponentManager componentManager = this.m_level.GetComponentManagerAt(this.m_villageType);
                        int totalMaxHousing = componentManager.GetTotalMaxHousing(this.m_unitProductionType != LogicDataType.CHARACTER ? 1 : 0) * 2;
                        int totalUsedCapacity = this.GetTotalCount() + data.GetHousingSpace() + (this.m_unitProductionType == LogicDataType.CHARACTER
                                                    ? avatar.GetUnitsTotalCapacity()
                                                    : avatar.GetSpellsTotalCapacity());

                        return totalMaxHousing >= totalUsedCapacity;
                    }
                }
                else
                {
                    Debugger.Error("Trying to add wrong unit type to UnitProduction");
                }
            }
            else
            {
                Debugger.Error("Trying to add NULL troop to UnitProduction");
            }

            return false;
        }

        public bool CanBeBoosted()
        {
            if (!this.m_boostPause && this.GetBoostMultiplier() > 0)
                return this.GetBoostCost() > 0;
            return false;
        }

        public int GetBoostCost()
        {
            if (this.m_unitProductionType == LogicDataType.CHARACTER)
                return this.m_level.GetGameMode().GetCalendar().GetUnitProductionBoostCost();
            return this.m_level.GetGameMode().GetCalendar().GetSpellProductionBoostCost();
        }

        public void Boost()
        {
            if (this.m_boostTimer != null)
            {
                this.m_boostTimer.Destruct();
                this.m_boostTimer = null;
            }

            this.m_boostTimer = new LogicTimer();
            this.m_boostTimer.StartTimer(this.GetMaxBoostTimeSecs(), this.m_level.GetLogicTime(), false, -1);
        }

        public void StopBoost()
        {
            if (this.m_boostTimer != null && !this.m_boostPause)
            {
                this.m_boostPause = true;
            }
        }

        public int GetTotalCount()
        {
            int count = 0;

            for (int i = 0; i < this.m_slots.Size(); i++)
            {
                LogicUnitProductionSlot slot = this.m_slots[i];
                LogicCombatItemData data = (LogicCombatItemData) slot.GetData();

                count += data.GetHousingSpace() * slot.GetCount();
            }

            return count;
        }

        public void SubTick()
        {
            if (this.m_boostTimer != null && this.m_boostPause)
            {
                this.m_boostTimer.StartTimer(this.m_boostTimer.GetRemainingSeconds(this.m_level.GetLogicTime()), this.m_level.GetLogicTime(), false, -1);
            }
        }

        public void Tick()
        {
            if (this.GetRemainingBoostTimeSecs() > 0)
            {
                if (this.m_timer != null)
                {
                    if (!this.IsBoostPaused())
                    {
                        this.m_timer.FastForwardSubticks(4 * this.GetBoostMultiplier() - 4);
                    }
                }
            }

            bool productionCompleted = false;

            if (this.m_timer != null)
            {
                productionCompleted = this.m_timer.GetRemainingSeconds(this.m_level.GetLogicTime()) == 0;
            }

            if (this.m_nextProduction > 0)
            {
                this.m_nextProduction = productionCompleted ? 0 : LogicMath.Max(this.m_nextProduction - 64, 0);
            }

            if (this.m_boostTimer != null && this.m_boostTimer.GetRemainingSeconds(this.m_level.GetLogicTime()) <= 0)
            {
                this.m_boostTimer.Destruct();
                this.m_boostTimer = null;
            }

            if ((productionCompleted || this.GetWaitingForSpaceUnit() != null) && this.m_nextProduction == 0)
            {
                this.ProductionCompleted(false);
            }
        }

        public void FastForwardTime(int secs)
        {
            if (this.m_boostTimer != null && !this.m_boostPause)
            {
                int remainingSecs = this.m_boostTimer.GetRemainingSeconds(this.m_level.GetLogicTime());

                if (remainingSecs <= secs)
                {
                    this.m_boostTimer.Destruct();
                    this.m_boostTimer = null;
                }
                else
                {
                    this.m_boostTimer.StartTimer(remainingSecs - secs, this.m_level.GetLogicTime(), false, -1);
                }
            }

            if (this.GetRemainingBoostTimeSecs() > 0)
            {
                if (this.GetBoostMultiplier() >= 2 && !this.IsBoostPaused())
                {
                    secs = LogicMath.Min(secs, this.GetRemainingBoostTimeSecs()) * (this.GetBoostMultiplier() - 1) + secs;
                }

                if (this.m_timer != null)
                {
                    if (!this.IsBoostPaused())
                    {
                        this.m_timer.FastForwardSubticks(4 * this.GetBoostMultiplier() - 4);
                    }
                }
            }

            do
            {
                if (secs <= 0)
                {
                    break;
                }

                LogicUnitProductionSlot productionSlot = this.GetCurrentlyTrainedSlot();

                if (productionSlot == null)
                {
                    break;
                }

                if (this.m_timer == null)
                {
                    LogicCombatItemData productionData = (LogicCombatItemData) productionSlot.GetData();

                    this.m_timer = new LogicTimer();
                    this.m_timer.StartTimer(productionData.GetTrainingTime(this.m_level.GetHomeOwnerAvatar().GetUnitUpgradeLevel(productionData), this.m_level, 0),
                                            this.m_level.GetLogicTime(), false, -1);
                }

                int remainingSecs = this.m_timer.GetRemainingSeconds(this.m_level.GetLogicTime());

                if (secs < remainingSecs)
                {
                    this.m_timer.StartTimer(remainingSecs - secs, this.m_level.GetLogicTime(), false, -1);
                    break;
                }

                secs -= remainingSecs;
                this.m_timer.StartTimer(0, this.m_level.GetLogicTime(), false, -1);
            } while (this.ProductionCompleted(false));
        }

        public void LoadingFinished()
        {
            LogicCombatItemData unitData = this.GetWaitingForSpaceUnit();

            if (unitData != null)
            {
                this.ProductionCompleted(false);
            }
        }

        public void UnitRemoved()
        {
            LogicCombatItemData unitData = this.GetWaitingForSpaceUnit();

            if (unitData != null)
            {
                this.ProductionCompleted(false);
            }
        }

        public void Load(LogicJSONObject root)
        {
            if (this.m_timer != null)
            {
                this.m_timer.Destruct();
                this.m_timer = null;
            }

            if (this.m_boostTimer != null)
            {
                this.m_boostTimer.Destruct();
                this.m_boostTimer = null;
            }

            for (int i = this.m_slots.Size() - 1; i >= 0; i--)
            {
                this.m_slots[i].Destruct();
                this.m_slots.Remove(i);
            }

            LogicJSONObject jsonObject = root.GetJSONObject("unit_prod");

            if (jsonObject != null)
            {
                LogicJSONArray slotArray = jsonObject.GetJSONArray("slots");

                if (slotArray != null)
                {
                    for (int i = 0; i < slotArray.Size(); i++)
                    {
                        LogicJSONObject slotObject = slotArray.GetJSONObject(i);

                        if (slotObject != null)
                        {
                            LogicJSONNumber dataObject = slotObject.GetJSONNumber("id");

                            if (dataObject != null)
                            {
                                LogicData data = LogicDataTables.GetDataById(dataObject.GetIntValue());

                                if (data != null)
                                {
                                    LogicJSONNumber countObject = slotObject.GetJSONNumber("cnt");
                                    LogicJSONBoolean termineObject = slotObject.GetJSONBoolean("t");

                                    if (countObject != null)
                                    {
                                        if (countObject.GetIntValue() > 0)
                                        {
                                            LogicUnitProductionSlot slot = new LogicUnitProductionSlot(data, countObject.GetIntValue(), false);

                                            if (termineObject != null)
                                            {
                                                slot.SetTerminate(termineObject.IsTrue());
                                            }

                                            this.m_slots.Add(slot);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (this.m_slots.Size() > 0)
                {
                    LogicUnitProductionSlot slot = this.GetCurrentlyTrainedSlot();

                    if (slot != null)
                    {
                        LogicJSONNumber timeObject = jsonObject.GetJSONNumber("t");

                        if (timeObject != null)
                        {
                            this.m_timer = new LogicTimer();
                            this.m_timer.StartTimer(timeObject.GetIntValue(), this.m_level.GetLogicTime(), false, -1);
                        }
                        else
                        {
                            LogicCombatItemData combatItemData = (LogicCombatItemData) slot.GetData();
                            LogicAvatar avatar = this.m_level.GetHomeOwnerAvatar();
                            int upgradeLevel = 0;

                            if (avatar != null)
                            {
                                upgradeLevel = avatar.GetUnitUpgradeLevel(combatItemData);
                            }

                            this.m_timer = new LogicTimer();
                            this.m_timer.StartTimer(combatItemData.GetTrainingTime(upgradeLevel, this.m_level, 0), this.m_level.GetLogicTime(), false, -1);

                            Debugger.Print("LogicUnitProduction::load null timer, restart: " + this.m_timer.GetRemainingSeconds(this.m_level.GetLogicTime()));
                        }
                    }
                }

                LogicJSONNumber boostTimeObject = jsonObject.GetJSONNumber("boost_t");

                if (boostTimeObject != null)
                {
                    this.m_boostTimer = new LogicTimer();
                    this.m_boostTimer.StartTimer(boostTimeObject.GetIntValue(), this.m_level.GetLogicTime(), false, -1);
                }

                LogicJSONBoolean boostPauseObject = jsonObject.GetJSONBoolean("boost_pause");

                if (boostPauseObject != null)
                {
                    this.m_boostPause = boostPauseObject.IsTrue();
                }
            }
            else
            {
                Debugger.Warning("LogicUnitProduction::load - Component wasn't found from the JSON");
            }
        }

        public void Save(LogicJSONObject root)
        {
            LogicJSONObject jsonObject = new LogicJSONObject();

            if (this.m_timer != null)
            {
                jsonObject.Put("t", new LogicJSONNumber(this.m_timer.GetRemainingSeconds(this.m_level.GetLogicTime())));
            }

            if (this.m_slots.Size() > 0)
            {
                LogicJSONArray slotArray = new LogicJSONArray();

                for (int i = 0; i < this.m_slots.Size(); i++)
                {
                    LogicUnitProductionSlot slot = this.m_slots[i];

                    if (slot != null)
                    {
                        LogicJSONObject slotObject = new LogicJSONObject();

                        slotObject.Put("id", new LogicJSONNumber(slot.GetData().GetGlobalID()));
                        slotObject.Put("cnt", new LogicJSONNumber(slot.GetCount()));

                        if (slot.IsTerminate())
                        {
                            slotObject.Put("t", new LogicJSONBoolean(true));
                        }

                        slotArray.Add(slotObject);
                    }
                }

                jsonObject.Put("slots", slotArray);
            }

            if (this.m_boostTimer != null)
            {
                jsonObject.Put("boost_t", new LogicJSONNumber(this.m_boostTimer.GetRemainingSeconds(this.m_level.GetLogicTime())));
            }

            if (this.m_boostPause)
            {
                jsonObject.Put("boost_pause", new LogicJSONBoolean(true));
            }

            root.Put("unit_prod", jsonObject);
        }
    }
}