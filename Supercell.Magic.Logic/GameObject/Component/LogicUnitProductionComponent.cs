namespace Supercell.Magic.Logic.GameObject.Component
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Time;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Util;

    public class LogicUnitProductionComponent : LogicComponent
    {
        private LogicTimer m_timer;
        private readonly LogicArrayList<LogicDataSlot> m_slots;

        private bool m_mode;
        private int m_productionType;

        public LogicUnitProductionComponent(LogicGameObject gameObject) : base(gameObject)
        {
            this.m_slots = new LogicArrayList<LogicDataSlot>();
            this.SetProductionType(gameObject);
        }

        public void SetProductionType(LogicGameObject gameObject)
        {
            this.m_productionType = 0;

            if (gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
            {
                this.m_productionType = ((LogicBuilding) gameObject).GetBuildingData().IsForgesSpells() ? 1 : 0;
            }
        }

        public int GetProductionType()
        {
            return this.m_productionType;
        }

        public int GetRemainingSeconds()
        {
            if (this.m_timer != null)
            {
                return this.m_timer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());
            }

            return 0;
        }

        public LogicCombatItemData GetCurrentlyTrainedUnit()
        {
            if (this.m_slots.Size() > 0)
            {
                return (LogicCombatItemData) this.m_slots[0].GetData();
            }

            return null;
        }

        public bool ContainsUnit(LogicCombatItemData data)
        {
            for (int i = 0; i < this.m_slots.Size(); i++)
            {
                if (this.m_slots[i].GetData() == data)
                {
                    return true;
                }
            }

            return false;
        }

        public override void Load(LogicJSONObject root)
        {
            if (this.m_timer != null)
            {
                this.m_timer.Destruct();
                this.m_timer = null;
            }

            for (int i = this.m_slots.Size() - 1; i >= 0; i--)
            {
                this.m_slots[i].Destruct();
                this.m_slots.Remove(i);
            }

            LogicJSONObject jsonObject = root.GetJSONObject("unit_prod");

            if (jsonObject != null)
            {
                LogicJSONNumber modeObject = jsonObject.GetJSONNumber("m");

                if (modeObject != null)
                {
                    this.m_mode = true;
                }

                LogicJSONNumber unitTypeObject = jsonObject.GetJSONNumber("unit_type");

                if (unitTypeObject != null)
                {
                    this.m_productionType = unitTypeObject.GetIntValue();
                }

                LogicJSONArray slotArray = jsonObject.GetJSONArray("slots");

                if (slotArray != null)
                {
                    for (int i = 0; i < slotArray.Size(); i++)
                    {
                        LogicJSONObject slotObject = slotArray.GetJSONObject(i);

                        if (slotObject != null)
                        {
                            LogicJSONNumber idObject = slotObject.GetJSONNumber("id");

                            if (idObject != null)
                            {
                                LogicData data = LogicDataTables.GetDataById(idObject.GetIntValue(),
                                                                             this.m_productionType == 0 ? LogicDataType.CHARACTER : LogicDataType.SPELL);

                                if (data != null)
                                {
                                    LogicJSONNumber countObject = slotObject.GetJSONNumber("cnt");

                                    if (countObject != null)
                                    {
                                        if (countObject.GetIntValue() > 0)
                                        {
                                            this.m_slots.Add(new LogicDataSlot(data, countObject.GetIntValue()));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (this.m_slots.Size() > 0)
                {
                    LogicJSONNumber timeObject = jsonObject.GetJSONNumber("t");

                    if (timeObject != null)
                    {
                        this.m_timer = new LogicTimer();
                        this.m_timer.StartTimer(timeObject.GetIntValue(), this.m_parent.GetLevel().GetLogicTime(), false, -1);
                    }
                    else
                    {
                        LogicCombatItemData data = (LogicCombatItemData) this.m_slots[0].GetData();
                        LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();
                        int upgLevel = homeOwnerAvatar != null ? homeOwnerAvatar.GetUnitUpgradeLevel(data) : 0;

                        this.m_timer = new LogicTimer();
                        this.m_timer.StartTimer(data.GetTrainingTime(upgLevel, this.m_parent.GetLevel(), 0), this.m_parent.GetLevel().GetLogicTime(), false, -1);
                    }
                }
            }
            else
            {
                this.m_productionType = 0;

                if (this.m_parent.GetVillageType() == 0)
                {
                    Debugger.Warning("LogicUnitProductionComponent::load - Component wasn't found from the JSON");
                }
            }
        }

        public override void Save(LogicJSONObject root, int villageType)
        {
            LogicJSONObject jsonObject = new LogicJSONObject();

            jsonObject.Put("m", new LogicJSONNumber(1));
            jsonObject.Put("unit_type", new LogicJSONNumber(this.m_productionType));

            if (this.m_timer != null)
            {
                jsonObject.Put("t", new LogicJSONNumber(this.m_timer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime())));
            }

            if (this.m_slots.Size() > 0)
            {
                LogicJSONArray slotArray = new LogicJSONArray();

                for (int i = 0; i < this.m_slots.Size(); i++)
                {
                    LogicDataSlot slot = this.m_slots[i];
                    LogicJSONObject slotObject = new LogicJSONObject();

                    slotObject.Put("id", new LogicJSONNumber(slot.GetData().GetGlobalID()));
                    slotObject.Put("cnt", new LogicJSONNumber(slot.GetCount()));

                    slotArray.Add(slotObject);
                }

                jsonObject.Put("slots", slotArray);
            }

            root.Put("unit_prod", jsonObject);
        }

        public override LogicComponentType GetComponentType()
        {
            return LogicComponentType.UNIT_PRODUCTION;
        }
    }
}