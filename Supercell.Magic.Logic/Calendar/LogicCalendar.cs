namespace Supercell.Magic.Logic.Calendar
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Util;

    public class LogicCalendar
    {
        private int m_activeTimestamp;
        private int m_eventSeenTime;

        private readonly LogicArrayList<int> m_startedEvents;
        private readonly LogicArrayList<LogicCalendarEvent> m_calendarEvents;
        private LogicArrayList<LogicCalendarEvent> m_activeCalendarEvents;

        public LogicCalendar()
        {
            this.m_activeTimestamp = -1;
            this.m_startedEvents = new LogicArrayList<int>();
            this.m_calendarEvents = new LogicArrayList<LogicCalendarEvent>();
            this.m_activeCalendarEvents = new LogicArrayList<LogicCalendarEvent>();
        }

        public void Destruct()
        {
            this.DestructCalendarEvents();

            if (this.m_activeCalendarEvents != null)
            {
                this.m_activeCalendarEvents.Destruct();
                this.m_activeCalendarEvents = null;
            }
        }

        public void DestructCalendarEvents()
        {
            if (this.m_calendarEvents != null)
            {
                while (this.m_calendarEvents.Size() > 0)
                {
                    this.m_calendarEvents[0].Destruct();
                    this.m_calendarEvents.Remove(0);
                }
            }
        }

        public void GetChecksum(ChecksumHelper checksum)
        {
            checksum.StartObject("LogicCalendar");
            checksum.StartArray("m_pActiveCalendarEvents");

            for (int i = 0; i < this.m_activeCalendarEvents.Size(); i++)
            {
                checksum.StartObject("LogicCalendarEvent");
                checksum.EndObject();
            }

            checksum.EndArray();
            checksum.WriteValue("m_activeTimestamp", this.m_activeTimestamp);
            checksum.EndObject();
        }

        public void Load(string json, int activeTimestamp)
        {
            Debugger.DoAssert(json != null, "Event json NULL");

            if (json.Length > 0)
            {
                LogicJSONObject jsonObject = (LogicJSONObject) LogicJSONParser.Parse(json);

                if (jsonObject != null)
                {
                    LogicArrayList<LogicCalendarEvent> events = new LogicArrayList<LogicCalendarEvent>();
                    LogicJSONArray eventArray = jsonObject.GetJSONArray("events");

                    if (eventArray != null)
                    {
                        for (int i = 0; i < eventArray.Size(); i++)
                        {
                            LogicJSONObject calendarObject = eventArray.GetJSONObject(i);

                            if (calendarObject == null)
                            {
                                Debugger.Error("Events json malformed!");
                            }

                            events.Add(LogicCalendarEventFactory.LoadFromJSON(calendarObject, null));
                        }
                    }

                    this.LoadingFinished(events, activeTimestamp);
                }
                else
                {
                    Debugger.Error("Events json malformed!");
                }
            }
        }

        public LogicJSONObject Save()
        {
            Debugger.DoAssert(this.m_calendarEvents != null, "Cannot create events, array is NULL.");

            LogicJSONObject jsonObject = new LogicJSONObject();
            LogicJSONArray eventArray = new LogicJSONArray(this.m_calendarEvents.Size());

            for (int i = 0; i < this.m_calendarEvents.Size(); i++)
            {
                eventArray.Add(this.m_calendarEvents[i].Save());
            }

            jsonObject.Put("events", eventArray);

            return jsonObject;
        }

        public void LoadProgress(LogicJSONObject jsonObject)
        {
            LogicJSONArray eventArray = jsonObject.GetJSONArray("events");

            if (eventArray != null)
            {
                for (int i = 0; i < eventArray.Size(); i++)
                {
                    this.m_startedEvents.Add(eventArray.GetJSONNumber(i).GetIntValue());
                }
            }

            LogicJSONNumber endSessionNumber = jsonObject.GetJSONNumber("es");

            if (endSessionNumber != null)
            {
                this.m_eventSeenTime = endSessionNumber.GetIntValue();
            }
        }

        public void SaveProgress(LogicJSONObject jsonObject)
        {
            if (this.m_startedEvents.Size() > 0)
            {
                LogicJSONArray eventArray = new LogicJSONArray();

                for (int i = 0; i < this.m_startedEvents.Size(); i++)
                {
                    eventArray.Add(new LogicJSONNumber(this.m_startedEvents[i]));
                }

                jsonObject.Put("events", eventArray);
            }

            jsonObject.Put("es", new LogicJSONNumber(this.m_eventSeenTime));
        }

        public void SetEventSeenTime(int timestamp)
        {
            this.m_eventSeenTime = timestamp;
        }

        public void Update(int activeTimestamp, LogicAvatar homeOwnerAvatar, LogicLevel level)
        {
            Debugger.DoAssert(activeTimestamp != -1, "You must set a valid time for calendar.");

            if (this.m_activeTimestamp != activeTimestamp)
            {
                this.m_activeTimestamp = activeTimestamp;

                if (this.HasNewActiveCalendarEvents(this.m_activeCalendarEvents, activeTimestamp, activeTimestamp))
                {
                    this.m_activeCalendarEvents = this.GetActiveCalendarEvents(activeTimestamp, activeTimestamp);

                    if (homeOwnerAvatar != null && level != null)
                    {
                        this.UpdateUseTroopEvent(homeOwnerAvatar, level);
                    }
                }
            }
        }

        public void UpdateUseTroopEvent(LogicAvatar homeOwnerAvatar, LogicLevel level)
        {
            for (int i = 0; i < this.m_activeCalendarEvents.Size(); i++)
            {
                int idx = this.m_startedEvents.IndexOf(this.m_activeCalendarEvents[i].GetId());

                if (idx == -1)
                {
                    this.m_activeCalendarEvents[i].StartUseTroopEvent(homeOwnerAvatar, level);
                }
            }

            while (this.m_startedEvents.Size() > 0)
            {
                this.m_startedEvents.Remove(0);
            }

            for (int i = 0; i < this.m_activeCalendarEvents.Size(); i++)
            {
                this.m_startedEvents.Add(this.m_activeCalendarEvents[i].GetId());
            }
        }

        public LogicArrayList<LogicCalendarEvent> GetCalendarEvents()
        {
            return this.m_calendarEvents;
        }

        public LogicArrayList<LogicCalendarEvent> GetActiveCalendarEvents(int minTimestamp, int maxTimestamp)
        {
            LogicArrayList<LogicCalendarEvent> activeCalendarEvents = new LogicArrayList<LogicCalendarEvent>();

            for (int i = 0; i < this.m_calendarEvents.Size(); i++)
            {
                LogicCalendarEvent calendarEvent = this.m_calendarEvents[i];

                if (calendarEvent.GetStartTime() <= minTimestamp && calendarEvent.GetEndTime() >= maxTimestamp)
                {
                    activeCalendarEvents.Add(calendarEvent);
                }
            }

            return activeCalendarEvents;
        }

        public bool HasNewActiveCalendarEvents(LogicArrayList<LogicCalendarEvent> currentActiveCalendarEvents, int minTimestamp, int maxTimestamp)
        {
            int enableCalendarCount = 0;

            for (int i = 0, j = 0; i < this.m_calendarEvents.Size(); i++)
            {
                LogicCalendarEvent calendarEvent = this.m_calendarEvents[i];

                if (calendarEvent.GetStartTime() <= minTimestamp && calendarEvent.GetEndTime() >= maxTimestamp)
                {
                    if (j < currentActiveCalendarEvents.Size())
                    {
                        if (!calendarEvent.IsEqual(currentActiveCalendarEvents[j++]))
                        {
                            return true;
                        }
                    }

                    ++enableCalendarCount;
                }
            }

            return enableCalendarCount != currentActiveCalendarEvents.Size();
        }

        public void LoadingFinished(LogicArrayList<LogicCalendarEvent> events, int activeTimestamp)
        {
            this.DestructCalendarEvents();
            this.m_calendarEvents.AddAll(events);
            this.Update(activeTimestamp, null, null);
        }

        public LogicCalendarUseTroop GetUseTroopEvents(LogicCombatItemData data)
        {
            for (int i = this.m_activeCalendarEvents.Size() - 1; i >= 0; i--)
            {
                LogicArrayList<LogicCalendarUseTroop> arrayList = this.m_activeCalendarEvents[i].GetUseTroops();

                for (int j = 0; j < arrayList.Size(); j++)
                {
                    if (arrayList[j].GetData() == data)
                    {
                        return arrayList[j];
                    }
                }
            }

            return null;
        }

        public LogicCalendarBuildingDestroyedSpawnUnit GetBuildingDestroyedSpawnUnit()
        {
            for (int i = this.m_activeCalendarEvents.Size() - 1; i >= 0; i--)
            {
                LogicCalendarBuildingDestroyedSpawnUnit buildingDestroyedSpawnUnit = this.m_activeCalendarEvents[i].GetBuildingDestroyedSpawnUnit();

                if (buildingDestroyedSpawnUnit != null)
                {
                    return buildingDestroyedSpawnUnit;
                }
            }

            return null;
        }

        public int GetBuildingBoostCost(LogicBuildingData data, int upgLevel)
        {
            int buildingBoostCost = data.GetBoostCost(upgLevel);

            for (int i = this.m_activeCalendarEvents.Size() - 1; i >= 0; i--)
            {
                int cost = this.m_activeCalendarEvents[i].GetBuildingBoostCost(data, upgLevel);

                if (cost <= buildingBoostCost)
                {
                    buildingBoostCost = cost;
                }
            }

            return buildingBoostCost;
        }

        public int GetUnitProductionBoostCost()
        {
            int unitProductionBoostCost = LogicDataTables.GetGlobals().GetNewTrainingBoostBarracksCost();

            for (int i = this.m_activeCalendarEvents.Size() - 1; i >= 0; i--)
            {
                LogicCalendarEvent calendarEvent = this.m_activeCalendarEvents[i];

                if (calendarEvent.GetNewTrainingBoostBarracksCost() <= unitProductionBoostCost)
                {
                    unitProductionBoostCost = calendarEvent.GetNewTrainingBoostBarracksCost();
                }
            }

            return unitProductionBoostCost;
        }

        public int GetSpellProductionBoostCost()
        {
            int spellProductionBoostCost = LogicDataTables.GetGlobals().GetNewTrainingBoostLaboratoryCost();

            for (int i = this.m_activeCalendarEvents.Size() - 1; i >= 0; i--)
            {
                LogicCalendarEvent calendarEvent = this.m_activeCalendarEvents[i];

                if (calendarEvent.GetNewTrainingBoostSpellCost() <= spellProductionBoostCost)
                {
                    spellProductionBoostCost = calendarEvent.GetNewTrainingBoostSpellCost();
                }
            }

            return spellProductionBoostCost;
        }

        public int GetTrainingCost(LogicCombatItemData data, int upgLevel)
        {
            int trainingCost = data.GetTrainingCost(upgLevel);

            for (int i = this.m_activeCalendarEvents.Size() - 1; i >= 0; i--)
            {
                int cost = this.m_activeCalendarEvents[i].GetTrainingCost(data, upgLevel);

                if (cost <= trainingCost)
                {
                    trainingCost = cost;
                }
            }

            return trainingCost;
        }

        public int GetStarBonusMultiplier()
        {
            int multiplier = 1;

            for (int i = this.m_activeCalendarEvents.Size() - 1; i >= 0; i--)
            {
                int starBonusMultiplier = this.m_activeCalendarEvents[i].GetStarBonusMultiplier();

                if (starBonusMultiplier >= multiplier)
                {
                    multiplier = starBonusMultiplier;
                }
            }

            return multiplier;
        }

        public bool IsProductionEnabled(LogicCombatItemData data)
        {
            if (this.IsEnabled(data))
            {
                return data.IsProductionEnabled();
            }

            return false;
        }

        public bool IsEnabled(LogicData data)
        {
            if (data.IsEnableByCalendar())
            {
                for (int i = this.m_activeCalendarEvents.Size() - 1; i >= 0; i--)
                {
                    if (this.m_activeCalendarEvents[i].IsEnabled(data))
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        public static int GetDuelLootLimitCooldownInMinutes(LogicCalendar instance, LogicConfiguration configuration)
        {
            if (instance == null)
            {
                Debugger.Warning("LogicCalender is NULL for getDuelLootLimitCooldownInMinutes call");

                if (configuration != null)
                {
                    return configuration.GetDuelLootLimitCooldownInMinutes();
                }

                return 1320;
            }

            int cost = -1;

            for (int i = 0; i < instance.m_activeCalendarEvents.Size(); i++)
            {
                LogicCalendarEvent calendarEvent = instance.m_activeCalendarEvents[i];

                if (calendarEvent.GetCalendarEventType() == LogicCalendarEvent.EVENT_TYPE_DUEL_LOOT_LIMIT)
                {
                    LogicDuelLootLimitCalendarEvent duelLootLimitCalendarEvent = (LogicDuelLootLimitCalendarEvent) calendarEvent;

                    if (cost == -1 || duelLootLimitCalendarEvent.GetDuelLootLimitCooldownInMinutes() <= cost)
                    {
                        cost = duelLootLimitCalendarEvent.GetDuelLootLimitCooldownInMinutes();
                    }
                }
            }

            if (cost == -1)
            {
                if (configuration != null)
                {
                    return configuration.GetDuelLootLimitCooldownInMinutes();
                }

                return 1320;
            }

            return 0;
        }

        public static int GetDuelBonusMaxDiamondCostPercent(LogicCalendar instance, LogicConfiguration configuration)
        {
            if (instance == null)
            {
                Debugger.Warning("LogicCalender is NULL for getDuelBonusMaxDiamondCostPercent call");

                if (configuration != null)
                {
                    return configuration.GetDuelBonusMaxDiamondCostPercent();
                }

                return 100;
            }

            int percent = -1;

            for (int i = 0; i < instance.m_activeCalendarEvents.Size(); i++)
            {
                LogicCalendarEvent calendarEvent = instance.m_activeCalendarEvents[i];

                if (calendarEvent.GetCalendarEventType() == LogicCalendarEvent.EVENT_TYPE_DUEL_LOOT_LIMIT)
                {
                    LogicDuelLootLimitCalendarEvent duelLootLimitCalendarEvent = (LogicDuelLootLimitCalendarEvent) calendarEvent;

                    if (percent == -1 || duelLootLimitCalendarEvent.GetDuelBonusMaxDiamondCostPercent() <= percent)
                    {
                        percent = duelLootLimitCalendarEvent.GetDuelBonusMaxDiamondCostPercent();
                    }
                }
            }

            if (percent == -1)
            {
                if (configuration != null)
                {
                    return configuration.GetDuelBonusMaxDiamondCostPercent();
                }

                return 100;
            }

            return 0;
        }

        public static int GetDuelBonusPercentWin(LogicCalendar instance, LogicConfiguration configuration)
        {
            if (instance == null)
            {
                Debugger.Warning("LogicCalender is NULL for getDuelBonusPercentWin call");

                if (configuration != null)
                {
                    return configuration.GetDuelBonusPercentWin();
                }

                return 100;
            }

            int percent = -1;

            for (int i = 0; i < instance.m_activeCalendarEvents.Size(); i++)
            {
                LogicCalendarEvent calendarEvent = instance.m_activeCalendarEvents[i];

                if (calendarEvent.GetCalendarEventType() == LogicCalendarEvent.EVENT_TYPE_DUEL_LOOT_LIMIT)
                {
                    LogicDuelLootLimitCalendarEvent duelLootLimitCalendarEvent = (LogicDuelLootLimitCalendarEvent) calendarEvent;

                    if (percent == -1 || duelLootLimitCalendarEvent.GetDuelBonusPercentWin() <= percent)
                    {
                        percent = duelLootLimitCalendarEvent.GetDuelBonusPercentWin();
                    }
                }
            }

            if (percent == -1)
            {
                if (configuration != null)
                {
                    return configuration.GetDuelBonusPercentWin();
                }

                return 100;
            }

            return 0;
        }

        public static int GetDuelBonusPercentDraw(LogicCalendar instance, LogicConfiguration configuration)
        {
            if (instance == null)
            {
                Debugger.Warning("LogicCalender is NULL for getDuelBonusPercentDraw call");

                if (configuration != null)
                {
                    return configuration.GetDuelBonusPercentDraw();
                }

                return 100;
            }

            int percent = -1;

            for (int i = 0; i < instance.m_activeCalendarEvents.Size(); i++)
            {
                LogicCalendarEvent calendarEvent = instance.m_activeCalendarEvents[i];

                if (calendarEvent.GetCalendarEventType() == LogicCalendarEvent.EVENT_TYPE_DUEL_LOOT_LIMIT)
                {
                    LogicDuelLootLimitCalendarEvent duelLootLimitCalendarEvent = (LogicDuelLootLimitCalendarEvent) calendarEvent;

                    if (percent == -1 || duelLootLimitCalendarEvent.GetDuelBonusPercentDraw() <= percent)
                    {
                        percent = duelLootLimitCalendarEvent.GetDuelBonusPercentDraw();
                    }
                }
            }

            if (percent == -1)
            {
                if (configuration != null)
                {
                    return configuration.GetDuelBonusPercentDraw();
                }

                return 100;
            }

            return 0;
        }

        public static int GetDuelBonusPercentLose(LogicCalendar instance, LogicConfiguration configuration)
        {
            if (instance == null)
            {
                Debugger.Warning("LogicCalender is NULL for getDuelBonusPercentLose call");

                if (configuration != null)
                {
                    return configuration.GetDuelBonusPercentLose();
                }

                return 100;
            }

            int percent = -1;

            for (int i = 0; i < instance.m_activeCalendarEvents.Size(); i++)
            {
                LogicCalendarEvent calendarEvent = instance.m_activeCalendarEvents[i];

                if (calendarEvent.GetCalendarEventType() == LogicCalendarEvent.EVENT_TYPE_DUEL_LOOT_LIMIT)
                {
                    LogicDuelLootLimitCalendarEvent duelLootLimitCalendarEvent = (LogicDuelLootLimitCalendarEvent) calendarEvent;

                    if (percent == -1 || duelLootLimitCalendarEvent.GetDuelBonusPercentLose() <= percent)
                    {
                        percent = duelLootLimitCalendarEvent.GetDuelBonusPercentLose();
                    }
                }
            }

            if (percent == -1)
            {
                if (configuration != null)
                {
                    return configuration.GetDuelBonusPercentLose();
                }

                return 100;
            }

            return 0;
        }
    }
}