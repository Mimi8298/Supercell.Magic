namespace Supercell.Magic.Logic.GameObject.Component
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Time;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public sealed class LogicUnitUpgradeComponent : LogicComponent
    {
        private LogicTimer m_timer;
        private LogicCombatItemData m_unit;
        private int m_unitType;

        public LogicUnitUpgradeComponent(LogicGameObject gameObject) : base(gameObject)
        {
            // LogicUnitUpgradeComponent.
        }

        public override void Destruct()
        {
            base.Destruct();

            if (this.m_timer != null)
            {
                this.m_timer.Destruct();
                this.m_timer = null;
            }
        }

        public override void Tick()
        {
            if (this.m_timer != null)
            {
                if (this.m_parent.GetLevel().GetRemainingClockTowerBoostTime() > 0)
                {
                    LogicGameObjectData data = this.m_parent.GetData();

                    if (data.GetDataType() == LogicDataType.BUILDING && data.GetVillageType() == 1)
                    {
                        this.m_timer.SetFastForward(this.m_timer.GetFastForward() + 4 * LogicDataTables.GetGlobals().GetClockTowerBoostMultiplier() - 4);
                    }
                }

                if (this.m_timer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime()) == 0)
                {
                    this.FinishUpgrading(true);
                }
            }
        }

        public override LogicComponentType GetComponentType()
        {
            return LogicComponentType.UNIT_UPGRADE;
        }

        public void FinishUpgrading(bool tick)
        {
            if (this.m_unit != null)
            {
                this.m_parent.GetLevel().GetHomeOwnerAvatar().CommodityCountChangeHelper(1, this.m_unit, 1);

                if (this.m_unit.GetVillageType() == 1 && this.m_unit.GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
                {
                    this.m_parent.GetLevel().GetGameObjectManagerAt(1).RefreshArmyCampSize();
                }

                if (this.m_parent.GetLevel().GetState() == 1)
                {
                    this.m_parent.GetLevel().GetGameListener()
                        .UnitUpgradeCompleted(this.m_unit, this.m_parent.GetLevel().GetHomeOwnerAvatar().GetUnitUpgradeLevel(this.m_unit), tick);
                }

                this.m_unit = null;
            }
            else
            {
                Debugger.Warning("LogicUnitUpgradeComponent::finishUpgrading called and m_pUnit is NULL");
            }

            if (this.m_timer != null)
            {
                this.m_timer.Destruct();
                this.m_timer = null;
            }
        }

        public int GetRemainingSeconds()
        {
            if (this.m_timer != null)
            {
                return this.m_timer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());
            }

            return 0;
        }

        public int GetRemainingMS()
        {
            if (this.m_timer != null)
            {
                return this.m_timer.GetRemainingMS(this.m_parent.GetLevel().GetLogicTime());
            }

            return 0;
        }

        public int GetTotalSeconds()
        {
            if (this.m_unit != null)
            {
                return this.m_unit.GetUpgradeTime(this.m_parent.GetLevel().GetHomeOwnerAvatar().GetUnitUpgradeLevel(this.m_unit));
            }

            return 0;
        }

        public override void Save(LogicJSONObject root, int villageType)
        {
            if (this.m_timer != null && this.m_unit != null)
            {
                LogicJSONObject jsonObject = new LogicJSONObject();

                jsonObject.Put("unit_type", new LogicJSONNumber(this.m_unitType));
                jsonObject.Put("t", new LogicJSONNumber(this.m_timer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime())));
                jsonObject.Put("id", new LogicJSONNumber(this.m_unit.GetGlobalID()));

                if (this.m_timer.GetEndTimestamp() != -1)
                {
                    jsonObject.Put("t_end", new LogicJSONNumber(this.m_timer.GetEndTimestamp()));
                }

                if (this.m_timer.GetFastForward() > 0)
                {
                    jsonObject.Put("t_ff", new LogicJSONNumber(this.m_timer.GetFastForward()));
                }

                root.Put("unit_upg", jsonObject);
            }
        }

        public override void Load(LogicJSONObject root)
        {
            if (this.m_timer != null)
            {
                this.m_timer.Destruct();
                this.m_timer = null;
            }

            this.m_unit = null;

            LogicJSONObject jsonObject = root.GetJSONObject("unit_upg");

            if (jsonObject != null)
            {
                LogicJSONNumber unitTypeObject = jsonObject.GetJSONNumber("unit_type");
                LogicJSONNumber idObject = jsonObject.GetJSONNumber("id");
                LogicJSONNumber timerObject = jsonObject.GetJSONNumber("t");
                LogicJSONNumber timerEndObject = jsonObject.GetJSONNumber("t_end");
                LogicJSONNumber timerFastForwardObject = jsonObject.GetJSONNumber("t_ff");

                this.m_unitType = unitTypeObject != null ? unitTypeObject.GetIntValue() : 0;

                if (idObject != null)
                {
                    if (timerObject != null)
                    {
                        LogicData data = LogicDataTables.GetDataById(idObject.GetIntValue(), this.m_unitType == 0 ? LogicDataType.CHARACTER : LogicDataType.SPELL);

                        if (data != null)
                        {
                            this.m_unit = (LogicCombatItemData) data;

                            this.m_timer = new LogicTimer();
                            this.m_timer.StartTimer(timerObject.GetIntValue(), this.m_parent.GetLevel().GetLogicTime(), false, -1);

                            if (timerEndObject != null)
                            {
                                this.m_timer.SetEndTimestamp(timerEndObject.GetIntValue());
                            }

                            if (timerFastForwardObject != null)
                            {
                                this.m_timer.SetFastForward(timerFastForwardObject.GetIntValue());
                            }
                        }
                    }
                }
            }
        }

        public override void LoadingFinished()
        {
            if (this.m_timer != null)
            {
                int remainingSecs = this.m_timer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());
                int totalSecs = this.GetTotalSeconds();

                if (LogicDataTables.GetGlobals().ClampUpgradeTimes())
                {
                    if (remainingSecs > totalSecs)
                    {
                        this.m_timer.StartTimer(totalSecs, this.m_parent.GetLevel().GetLogicTime(), true,
                                                this.m_parent.GetLevel().GetHomeOwnerAvatarChangeListener().GetCurrentTimestamp());
                    }
                }
                else
                {
                    this.m_timer.StartTimer(LogicMath.Min(remainingSecs, totalSecs), this.m_parent.GetLevel().GetLogicTime(), false, -1);
                }
            }
        }

        public override void FastForwardTime(int time)
        {
            if (this.m_timer != null)
            {
                if (this.m_timer.GetEndTimestamp() == -1)
                {
                    this.m_timer.StartTimer(this.m_timer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime()) - time, this.m_parent.GetLevel().GetLogicTime(), false, -1);
                }
                else
                {
                    this.m_timer.AdjustEndSubtick(this.m_parent.GetLevel());
                }

                int clockTowerBoostTime = this.m_parent.GetLevel().GetUpdatedClockTowerBoostTime();

                if (clockTowerBoostTime > 0 && this.m_parent.GetLevel().IsClockTowerBoostPaused())
                {
                    LogicGameObjectData data = this.m_parent.GetData();

                    if (data.GetDataType() == LogicDataType.BUILDING)
                    {
                        if (data.GetVillageType() == 1)
                        {
                            this.m_timer.SetFastForward(this.m_timer.GetFastForward() +
                                                        60 * LogicMath.Min(time, clockTowerBoostTime) * (LogicDataTables.GetGlobals().GetClockTowerBoostMultiplier() - 1));
                        }
                    }
                }
            }
        }

        public bool SpeedUp()
        {
            if (this.m_unit != null)
            {
                int remainingSecs = this.m_timer != null ? this.m_timer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime()) : 0;
                int speedUpCost = LogicGamePlayUtil.GetSpeedUpCost(remainingSecs, 0, this.m_parent.GetVillageType());

                LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();

                if (this.m_parent.GetLevel().GetHomeOwnerAvatar().IsClientAvatar())
                {
                    LogicClientAvatar clientAvatar = (LogicClientAvatar) homeOwnerAvatar;

                    if (clientAvatar.HasEnoughDiamonds(speedUpCost, true, this.m_parent.GetLevel()))
                    {
                        clientAvatar.UseDiamonds(speedUpCost);
                        clientAvatar.GetChangeListener().DiamondPurchaseMade(4, this.m_unit.GetGlobalID(), clientAvatar.GetUnitUpgradeLevel(this.m_unit) + 1, speedUpCost,
                                                                             this.m_parent.GetLevel().GetVillageType());
                        this.FinishUpgrading(true);

                        return true;
                    }
                }
            }

            return false;
        }

        public LogicCombatItemData GetCurrentlyUpgradedUnit()
        {
            return this.m_unit;
        }

        public void CancelUpgrade()
        {
            if (this.m_unit != null)
            {
                LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();
                int upgLevel = homeOwnerAvatar.GetUnitUpgradeLevel(this.m_unit);

                homeOwnerAvatar.CommodityCountChangeHelper(0, this.m_unit.GetUpgradeResource(upgLevel), this.m_unit.GetUpgradeCost(upgLevel));

                this.m_unit = null;
            }

            if (this.m_timer != null)
            {
                this.m_timer.Destruct();
                this.m_timer = null;
            }
        }

        public bool CanStartUpgrading(LogicCombatItemData data)
        {
            if (data != null && this.m_unit == null)
            {
                if (this.m_parent.GetLevel().GetGameMode().GetCalendar().IsProductionEnabled(data))
                {
                    if (data.GetCombatItemType() != LogicCombatItemData.COMBAT_ITEM_TYPE_HERO)
                    {
                        if (this.m_parent.GetVillageType() == data.GetVillageType())
                        {
                            int upgLevel = this.m_parent.GetLevel().GetHomeOwnerAvatar().GetUnitUpgradeLevel(data);

                            if (data.GetUpgradeLevelCount() - 1 > upgLevel)
                            {
                                int maxProductionHouseLevel;

                                if (data.GetVillageType() == 1)
                                {
                                    maxProductionHouseLevel = this.m_parent.GetComponentManager().GetMaxBarrackLevel();
                                }
                                else
                                {
                                    LogicComponentManager componentManager = this.m_parent.GetComponentManager();

                                    if (data.GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
                                    {
                                        maxProductionHouseLevel = data.GetUnitOfType() != 1 ? componentManager.GetMaxDarkBarrackLevel() : componentManager.GetMaxBarrackLevel();
                                    }
                                    else
                                    {
                                        maxProductionHouseLevel =
                                            data.GetUnitOfType() == 1 ? componentManager.GetMaxSpellForgeLevel() : componentManager.GetMaxMiniSpellForgeLevel();
                                    }
                                }

                                if (maxProductionHouseLevel >= data.GetRequiredProductionHouseLevel())
                                {
                                    LogicBuilding building = (LogicBuilding) this.m_parent;

                                    if (!building.IsLocked())
                                    {
                                        return building.GetUpgradeLevel() >= data.GetRequiredLaboratoryLevel(upgLevel + 1);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public void StartUpgrading(LogicCombatItemData data)
        {
            if (this.CanStartUpgrading(data))
            {
                this.m_unit = data;

                if (this.m_timer != null)
                {
                    this.m_timer.Destruct();
                    this.m_timer = null;
                }

                this.m_timer = new LogicTimer();
                this.m_timer.StartTimer(this.GetTotalSeconds(), this.m_parent.GetLevel().GetLogicTime(), true,
                                        this.m_parent.GetLevel().GetHomeOwnerAvatarChangeListener().GetCurrentTimestamp());
                this.m_unitType = this.m_unit.GetCombatItemType();
            }
        }
    }
}