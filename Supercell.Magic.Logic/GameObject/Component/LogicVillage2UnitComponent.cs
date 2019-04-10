namespace Supercell.Magic.Logic.GameObject.Component
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject.Listener;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Time;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicVillage2UnitComponent : LogicComponent
    {
        private LogicDataSlot m_unit;
        private LogicTimer m_productionTimer;

        private bool m_noBarrack;
        private int m_productionType;

        public LogicVillage2UnitComponent(LogicGameObject gameObject) : base(gameObject)
        {
        }

        public void TrainUnit(LogicCombatItemData combatItemData)
        {
            if (this.m_unit != null)
            {
                this.m_unit.Destruct();
                this.m_unit = null;
            }

            if (this.m_productionTimer != null)
            {
                this.m_productionTimer.Destruct();
                this.m_productionTimer = null;
            }

            this.m_unit = new LogicDataSlot(combatItemData, 0);
            this.m_productionTimer = new LogicTimer();
            this.m_productionTimer.StartTimer(this.GetTrainingTime(combatItemData), this.m_parent.GetLevel().GetLogicTime(), false, -1);
        }

        public void RemoveUnits()
        {
            if (this.m_unit != null)
            {
                this.m_unit.Destruct();
                this.m_unit = null;
            }

            if (this.m_productionTimer != null)
            {
                this.m_productionTimer.Destruct();
                this.m_productionTimer = null;
            }
        }

        public void SetUnit(LogicCombatItemData combatItemData, int count)
        {
            if (this.m_unit != null)
            {
                this.m_unit.Destruct();
                this.m_unit = null;
            }

            this.m_unit = new LogicDataSlot(combatItemData, count);
        }

        public int GetTrainingTime(LogicCombatItemData data)
        {
            return data.GetTrainingTime(this.m_parent.GetLevel().GetHomeOwnerAvatar().GetUnitUpgradeLevel(data), this.m_parent.GetLevel(), 0);
        }

        public int GetTotalSecs()
        {
            if (this.m_unit != null)
            {
                return this.GetTrainingTime((LogicCombatItemData) this.m_unit.GetData());
            }

            return 0;
        }

        public int GetMaxUnitsInCamp(LogicCharacterData data)
        {
            LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();

            if (homeOwnerAvatar != null)
            {
                return data.GetUnitsInCamp(homeOwnerAvatar.GetUnitUpgradeLevel(data));
            }

            Debugger.Error("AVATAR = NULL");

            return 0;
        }

        public bool IsEmpty()
        {
            if (this.m_unit != null)
            {
                return this.m_unit.GetCount() <= 0;
            }

            return true;
        }

        public int GetRemainingSecs()
        {
            if (this.m_productionTimer != null)
            {
                int remainingSecs = this.m_productionTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());
                int trainingTime = this.m_unit != null ? this.GetTrainingTime((LogicCombatItemData) this.m_unit.GetData()) : 0;

                return LogicMath.Min(remainingSecs, trainingTime);
            }

            return 0;
        }

        public void ProductionCompleted()
        {
            this.m_unit.SetCount(this.GetMaxUnitsInCamp((LogicCharacterData) this.m_unit.GetData()));

            if (this.m_productionTimer != null)
            {
                this.m_productionTimer.Destruct();
                this.m_productionTimer = null;
            }

            LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();
            LogicCombatItemData combatItemData = (LogicCombatItemData) this.m_unit.GetData();

            int unitCount = this.m_parent.GetLevel().GetMissionManager().IsVillage2TutorialOpen()
                ? this.m_unit.GetCount() - homeOwnerAvatar.GetUnitCountVillage2(combatItemData)
                : this.m_unit.GetCount();

            homeOwnerAvatar.CommodityCountChangeHelper(7, this.m_unit.GetData(), unitCount);

            if (this.m_parent.GetLevel().GetGameListener() != null)
            {
                // ?
            }

            int state = this.m_parent.GetLevel().GetState();

            if (state == 1)
            {
                if (this.m_parent.GetListener() != null)
                {
                    // ?
                }
            }
        }

        public void RefreshArmyCampSize(bool unk)
        {
            if (this.m_unit != null && this.m_productionTimer == null)
            {
                int maxUnits = this.GetMaxUnitsInCamp((LogicCharacterData) this.m_unit.GetData());
                int unitCount = this.m_unit.GetCount();

                if (maxUnits > unitCount)
                {
                    LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();
                    homeOwnerAvatar.CommodityCountChangeHelper(7, this.m_unit.GetData(), maxUnits - unitCount);
                    this.m_unit.SetCount(maxUnits);

                    if (this.m_parent.GetLevel().GetState() == 1)
                    {
                        LogicGameObjectListener listener = this.m_parent.GetListener();

                        if (listener != null)
                        {
                            for (int i = unitCount; i < maxUnits; i++)
                            {
                                if (i > 25)
                                {
                                    return;
                                }

                                // TODO: Implement listener.
                            }
                        }
                    }
                }
            }
        }

        public int GetRemainingMS()
        {
            if (this.m_productionTimer != null)
            {
                return this.m_productionTimer.GetRemainingMS(this.m_parent.GetLevel().GetLogicTime());
            }

            return 0;
        }

        public LogicCharacterData GetUnitData()
        {
            if (this.m_unit != null)
            {
                return (LogicCharacterData) this.m_unit.GetData();
            }

            return null;
        }

        public LogicCharacterData GetCurrentlyTrainedUnit()
        {
            if (this.m_unit != null && this.m_unit.GetCount() == 0)
            {
                return (LogicCharacterData) this.m_unit.GetData();
            }

            return null;
        }

        public int GetUnitCount()
        {
            if (this.m_unit != null)
            {
                return this.m_unit.GetCount();
            }

            return 0;
        }

        public override LogicComponentType GetComponentType()
        {
            return LogicComponentType.VILLAGE2_UNIT;
        }

        public override void FastForwardTime(int secs)
        {
            LogicArrayList<LogicComponent> components = this.m_parent.GetComponentManager().GetComponents(LogicComponentType.VILLAGE2_UNIT);

            int barrackCount = this.m_parent.GetGameObjectManager().GetBarrackCount();
            int barrackFoundCount = 0;

            for (int i = 0; i < barrackCount; i++)
            {
                LogicBuilding building = (LogicBuilding) this.m_parent.GetGameObjectManager().GetBarrack(i);

                if (building != null && !building.IsConstructing())
                {
                    barrackFoundCount += 1;
                }
            }

            if (components.Size() <= 0 || barrackFoundCount != 0 && components[0] == this)
            {
                LogicLevel level = this.m_parent.GetLevel();

                int clockTowerBoostTime = level.GetUpdatedClockTowerBoostTime();
                int boostTime = 0;

                if (clockTowerBoostTime > 0 &&
                    !level.IsClockTowerBoostPaused())
                {
                    LogicGameObjectData data = this.m_parent.GetData();

                    if (data.GetDataType() == LogicDataType.BUILDING)
                    {
                        if (data.GetVillageType() == 1)
                        {
                            boostTime = LogicMath.Min(secs, clockTowerBoostTime) * (LogicDataTables.GetGlobals().GetClockTowerBoostMultiplier() - 1);
                        }
                    }
                }

                int remainingSecs = secs + boostTime;

                for (int i = 0; i < components.Size(); i++)
                {
                    remainingSecs = LogicMath.Max(0, remainingSecs - ((LogicVillage2UnitComponent) components[i]).FastForwardProduction(remainingSecs));
                }
            }
        }

        public int FastForwardProduction(int secs)
        {
            if (secs > 0)
            {
                if (this.m_unit != null && this.m_unit.GetCount() == 0)
                {
                    if (this.m_productionTimer == null)
                    {
                        this.m_productionTimer = new LogicTimer();
                        this.m_productionTimer.StartTimer(this.GetTrainingTime((LogicCharacterData) this.m_unit.GetData()), this.m_parent.GetLevel().GetLogicTime(), false, -1);
                    }

                    int remainingSecs = this.m_productionTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());

                    if (remainingSecs - secs <= 0)
                    {
                        this.ProductionCompleted();
                        return remainingSecs;
                    }

                    this.m_productionTimer.StartTimer(remainingSecs - secs, this.m_parent.GetLevel().GetLogicTime(), false, -1);
                    return secs;
                }
            }

            return 0;
        }

        public override void Tick()
        {
            LogicArrayList<LogicComponent> components = this.m_parent.GetComponentManager().GetComponents(LogicComponentType.VILLAGE2_UNIT);

            int barrackCount = this.m_parent.GetGameObjectManager().GetBarrackCount();
            int barrackFoundCount = 0;

            for (int i = 0; i < barrackCount; i++)
            {
                LogicBuilding building = (LogicBuilding) this.m_parent.GetGameObjectManager().GetBarrack(i);

                if (building != null && !building.IsConstructing())
                {
                    barrackFoundCount += 1;
                }
            }

            this.m_noBarrack = barrackFoundCount == 0;

            for (int i = 0; i < components.Size(); i++)
            {
                LogicVillage2UnitComponent component = (LogicVillage2UnitComponent) components[i];

                if (barrackFoundCount != 0)
                {
                    if (this == component)
                    {
                        break;
                    }
                }

                if (component != null && component.m_unit != null)
                {
                    if (component.m_unit.GetData() != null && component.m_unit.GetCount() == 0)
                    {
                        if (component.GetRemainingSecs() > 0)
                        {
                            if (this.m_productionTimer != null)
                            {
                                this.m_productionTimer.StartTimer(this.m_productionTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime()),
                                                                  this.m_parent.GetLevel().GetLogicTime(), false, -1);
                            }

                            return;
                        }
                    }
                }
            }

            if (this.m_productionTimer != null)
            {
                if (this.m_parent.GetLevel().GetRemainingClockTowerBoostTime() > 0)
                {
                    if (this.m_parent.GetData().GetDataType() == LogicDataType.BUILDING && this.m_parent.GetData().GetVillageType() == 1)
                    {
                        this.m_productionTimer.FastForwardSubticks(4 * LogicDataTables.GetGlobals().GetClockTowerBoostMultiplier() - 4);
                    }
                }
            }
            
            // TODO: Implement listener.

            if (this.m_productionTimer != null && this.m_productionTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime()) == 0)
            {
                if (this.m_unit != null)
                {
                    this.ProductionCompleted();
                }
            }
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONObject unitProductionObject = jsonObject.GetJSONObject("up2");

            if (unitProductionObject != null)
            {
                LogicJSONNumber timerObject = unitProductionObject.GetJSONNumber("t");

                if (timerObject != null)
                {
                    int time = timerObject.GetIntValue();

                    if (this.m_productionTimer != null)
                    {
                        this.m_productionTimer.Destruct();
                        this.m_productionTimer = null;
                    }

                    this.m_productionTimer = new LogicTimer();
                    this.m_productionTimer.StartTimer(time, this.m_parent.GetLevel().GetLogicTime(), false, -1);
                }

                LogicJSONArray unitArray = unitProductionObject.GetJSONArray("unit");

                if (unitArray != null)
                {
                    LogicJSONNumber dataObject = unitArray.GetJSONNumber(0);
                    LogicJSONNumber cntObject = unitArray.GetJSONNumber(1);

                    if (dataObject != null)
                    {
                        if (cntObject != null)
                        {
                            LogicData data = LogicDataTables.GetDataById(dataObject.GetIntValue(),
                                                                         this.m_productionType != 0 ? LogicDataType.SPELL : LogicDataType.CHARACTER);

                            if (data == null)
                            {
                                Debugger.Error("LogicVillage2UnitComponent::load - Character data is NULL!");
                            }

                            this.m_unit = new LogicDataSlot(data, cntObject.GetIntValue());
                        }
                    }
                }
            }
        }

        public override void Save(LogicJSONObject jsonObject, int villageType)
        {
            LogicJSONObject unitProductionObject = new LogicJSONObject();

            if (this.m_unit != null)
            {
                LogicJSONArray unitArray = new LogicJSONArray();

                unitArray.Add(new LogicJSONNumber(this.m_unit.GetData().GetGlobalID()));
                unitArray.Add(new LogicJSONNumber(this.m_unit.GetCount()));

                unitProductionObject.Put("unit", unitArray);
            }

            if (this.m_productionTimer != null)
            {
                unitProductionObject.Put("t", new LogicJSONNumber(this.m_productionTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime())));
            }

            jsonObject.Put("up2", unitProductionObject);
        }

        public override void LoadingFinished()
        {
            this.RefreshArmyCampSize(false);
        }
    }
}