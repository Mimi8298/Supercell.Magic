namespace Supercell.Magic.Logic.Calendar
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class LogicCalendarEvent
    {
        public const int EVENT_TYPE_BASE = 0;
        public const int EVENT_TYPE_OFFER = 1;
        public const int EVENT_TYPE_DUEL_LOOT_LIMIT = 4;

        private int m_id;
        private int m_version;

        private int m_startTime;
        private int m_endTime;
        private int m_visibleTime;
        private int m_visibleEndTime;

        private int m_inboxEntryId;

        private int m_newTrainingBoostBarracksCost;
        private int m_newTrainingBoostSpellCost;
        private int m_allianceXpMultiplier;
        private int m_starBonusMultiplier;
        private int m_allianceWarWinLootMultiplier;
        private int m_allianceWarDrawLootMultiplier;
        private int m_allianceWarLooseLootMultiplier;

        private string m_clashBoxEntryName;
        private string m_notificationTid;
        private string m_image;
        private string m_sc;
        private string m_localization;

        private LogicArrayList<LogicCalendarFunction> m_functions;
        private readonly LogicArrayList<LogicDataSlot> m_buildingBoostCost;
        private LogicArrayList<LogicDataSlot> m_troopDiscount;
        private LogicArrayList<LogicData> m_enabledData;
        private LogicArrayList<LogicDataSlot> m_freeTroops;
        private LogicArrayList<LogicCalendarUseTroop> m_useTroops;

        private LogicCalendarBuildingDestroyedSpawnUnit m_buildingDestroyedSpawnUnit;
        private LogicEventEntryData m_eventEntryData;
        private LogicCalendarTargeting m_targeting;
        private LogicCalendarErrorHandler m_errorHandler;

        public LogicCalendarEvent()
        {
            this.m_errorHandler = new LogicCalendarErrorHandler();

            this.m_functions = new LogicArrayList<LogicCalendarFunction>();
            this.m_buildingBoostCost = new LogicArrayList<LogicDataSlot>();
            this.m_troopDiscount = new LogicArrayList<LogicDataSlot>();
            this.m_enabledData = new LogicArrayList<LogicData>();
            this.m_freeTroops = new LogicArrayList<LogicDataSlot>();
            this.m_useTroops = new LogicArrayList<LogicCalendarUseTroop>();

            this.m_allianceXpMultiplier = 1;
            this.m_starBonusMultiplier = 1;
            this.m_allianceWarWinLootMultiplier = 1;
            this.m_allianceWarDrawLootMultiplier = 1;
            this.m_allianceWarLooseLootMultiplier = 1;
        }

        public virtual void Destruct()
        {
            if (this.m_functions != null)
            {
                for (int i = this.m_functions.Size() - 1; i >= 0; i--)
                {
                    this.m_functions[i].Destruct();
                    this.m_functions.Remove(i);
                }

                this.m_functions = null;
            }

            if (this.m_buildingBoostCost != null)
            {
                while (this.m_buildingBoostCost.Size() > 0)
                {
                    this.m_buildingBoostCost[0].Destruct();
                    this.m_buildingBoostCost.Remove(0);
                }

                this.m_troopDiscount = null;
            }

            if (this.m_troopDiscount != null)
            {
                while (this.m_troopDiscount.Size() > 0)
                {
                    this.m_troopDiscount[0].Destruct();
                    this.m_troopDiscount.Remove(0);
                }

                this.m_troopDiscount = null;
            }

            if (this.m_freeTroops != null)
            {
                while (this.m_freeTroops.Size() > 0)
                {
                    this.m_freeTroops[0].Destruct();
                    this.m_freeTroops.Remove(0);
                }

                this.m_freeTroops = null;
            }

            this.m_enabledData = null;
            this.m_useTroops = null;

            this.m_clashBoxEntryName = null;
            this.m_notificationTid = null;
            this.m_image = null;
            this.m_sc = null;
            this.m_localization = null;

            this.m_eventEntryData = null;
            this.m_targeting = null;
            this.m_errorHandler = null;
        }

        public void SetErrorHandler(LogicCalendarErrorHandler errorHandler)
        {
            this.m_errorHandler = errorHandler;
        }

        public void Init(LogicJSONObject jsonObject)
        {
            this.Load(jsonObject);
            this.ApplyFunctions();
        }

        public virtual void Load(LogicJSONObject jsonObject)
        {
            if (jsonObject == null)
            {
                this.m_errorHandler.Error(this, "Json cannot be null");
            }

            this.m_id = LogicJSONHelper.GetInt(jsonObject, "id", -1);
            this.m_version = LogicJSONHelper.GetInt(jsonObject, "version", 0);

            this.m_newTrainingBoostBarracksCost = LogicDataTables.GetGlobals().GetNewTrainingBoostBarracksCost();
            this.m_newTrainingBoostSpellCost = LogicDataTables.GetGlobals().GetNewTrainingBoostLaboratoryCost();

            this.m_startTime = LogicCalendarEvent.ConvertStringToTimestamp(LogicJSONHelper.GetString(jsonObject, "startTime"), false);
            this.m_endTime = LogicCalendarEvent.ConvertStringToTimestamp(LogicJSONHelper.GetString(jsonObject, "endTime"), true);

            if (this.m_startTime >= this.m_endTime)
            {
                this.m_errorHandler.ErrorField(this, "endTime", "End time must be after start time.");
            }

            LogicJSONString visibleTimeString = jsonObject.GetJSONString("visibleTime");

            if (visibleTimeString != null)
            {
                this.m_visibleTime = LogicCalendarEvent.ConvertStringToTimestamp(visibleTimeString.GetStringValue(), false);

                if (this.m_visibleTime > this.m_startTime)
                {
                    this.m_errorHandler.ErrorField(this, "visibleTime", "Visible time must be before or at start time.");
                }
            }
            else
            {
                this.m_visibleTime = 0;
            }

            this.m_clashBoxEntryName = jsonObject.GetJSONString("clashBoxEntryName").GetStringValue();

            LogicJSONString eventEntryNameString = jsonObject.GetJSONString("eventEntryName");

            this.m_eventEntryData = LogicDataTables.GetEventEntryByName(eventEntryNameString.GetStringValue(), null);

            if (eventEntryNameString.GetStringValue().Length > 0)
            {
                if (this.m_eventEntryData == null)
                {
                    this.m_errorHandler.ErrorField(this, "eventEntryName", string.Format("Invalid event entry name: {0}.", eventEntryNameString.GetStringValue()));
                }

                if (this.m_visibleTime == 0)
                {
                    this.m_errorHandler.ErrorField(this, "visibleTime", "Visible time must be set if event entry name is set.");
                }
            }

            if (this.m_visibleTime != 0)
            {
                if (this.m_eventEntryData == null)
                {
                    this.m_errorHandler.ErrorField(this, "eventEntryName", "Event entry name must be set if visible time is set.");
                }
            }

            this.m_inboxEntryId = LogicJSONHelper.GetInt(jsonObject, "inboxEntryId", -1);
            this.m_notificationTid = LogicJSONHelper.GetString(jsonObject, "notificationTid");
            this.m_image = LogicJSONHelper.GetString(jsonObject, "image");
            this.m_sc = LogicJSONHelper.GetString(jsonObject, "sc");
            this.m_localization = LogicJSONHelper.GetString(jsonObject, "localization");

            LogicJSONObject targetingObject = jsonObject.GetJSONObject("targeting");

            if (targetingObject != null)
            {
                this.m_targeting = new LogicCalendarTargeting(jsonObject);
            }

            LogicJSONArray functionArray = jsonObject.GetJSONArray("functions");

            if (functionArray != null)
            {
                for (int i = 0; i < functionArray.Size(); i++)
                {
                    this.m_functions.Add(new LogicCalendarFunction(this, i, functionArray.GetJSONObject(i), this.m_errorHandler));
                }
            }
        }

        public virtual LogicJSONObject Save()
        {
            LogicJSONObject jsonObject = new LogicJSONObject();

            jsonObject.Put("type", new LogicJSONNumber(this.GetCalendarEventType()));
            jsonObject.Put("id", new LogicJSONNumber(this.m_id));
            jsonObject.Put("version", new LogicJSONNumber(this.m_version));
            jsonObject.Put("visibleTime", new LogicJSONString(LogicCalendarEvent.ConvertTimestampToString(this.m_visibleTime)));
            jsonObject.Put("startTime", new LogicJSONString(LogicCalendarEvent.ConvertTimestampToString(this.m_startTime)));
            jsonObject.Put("endTime", new LogicJSONString(LogicCalendarEvent.ConvertTimestampToString(this.m_endTime)));
            jsonObject.Put("clashBoxEntryName", new LogicJSONString(this.m_clashBoxEntryName));
            jsonObject.Put("eventEntryName", new LogicJSONString(this.m_eventEntryData != null ? this.m_eventEntryData.GetName() : string.Empty));
            jsonObject.Put("inboxEntryId", new LogicJSONNumber(this.m_inboxEntryId));
            jsonObject.Put("notificationTid", new LogicJSONString(this.m_notificationTid));
            jsonObject.Put("image", new LogicJSONString(this.m_image));
            jsonObject.Put("sc", new LogicJSONString(this.m_sc));
            jsonObject.Put("localization", new LogicJSONString(this.m_localization));

            LogicJSONArray functionArray = new LogicJSONArray(this.m_functions.Size());

            for (int i = 0; i < this.m_functions.Size(); i++)
            {
                functionArray.Add(this.m_functions[i].Save());
            }

            jsonObject.Put("functions", functionArray);

            if (this.m_targeting != null)
            {
                LogicJSONObject targetingObject = new LogicJSONObject();
                this.m_targeting.Save(jsonObject);
                jsonObject.Put("targeting", targetingObject);
            }

            return jsonObject;
        }

        public void ApplyFunctions()
        {
            for (int i = 0; i < this.m_functions.Size(); i++)
            {
                this.m_functions[i].ApplyToEvent(this);
            }
        }

        private static int ConvertStringToTimestamp(string time, bool round)
        {
            int spliter = time.IndexOf("T");

            Debugger.DoAssert(spliter == 8, "Unable to convert time. ISO8601 expected.");
            LogicGregDate date = new LogicGregDate(LogicStringUtil.ConvertToInt(time, 0, 4),
                                                   LogicStringUtil.ConvertToInt(time, 4, 6),
                                                   LogicStringUtil.ConvertToInt(time, 6, 8));

            date.Validate();

            int totalSecs = date.GetIndex() * 86400;
            string dayTime = time.Substring(spliter + 1);

            if (dayTime.Length < 2)
            {
                if (round)
                {
                    return totalSecs + 82800;
                }

                return totalSecs;
            }

            totalSecs += 3600 * LogicStringUtil.ConvertToInt(dayTime, 0, 2);

            if (dayTime.Length < 4)
            {
                if (round)
                {
                    return totalSecs + 3540;
                }

                return totalSecs;
            }

            totalSecs += 60 * LogicStringUtil.ConvertToInt(dayTime, 2, 4);

            if (dayTime.Length < 6)
            {
                if (round)
                {
                    return totalSecs + 59;
                }

                return totalSecs;
            }

            return totalSecs + LogicStringUtil.ConvertToInt(dayTime, 4, 6);
        }

        private static string ConvertTimestampToString(int timestamp)
        {
            LogicGregDate gregDate = new LogicGregDate(timestamp / 86400);
            return string.Format("{0:D4}{1:D2}{2:D2}T{3:D2}{4:D2}{5:D2}.000Z", gregDate.GetYear(), gregDate.GetMonth(), gregDate.GetDay(),
                                 timestamp % 86400 / 3600,
                                 timestamp % 86400 % 3600 / 60,
                                 timestamp % 86400 % 3600 % 60);
        }

        public virtual int GetCalendarEventType()
        {
            return LogicCalendarEvent.EVENT_TYPE_BASE;
        }

        public int GetId()
        {
            return this.m_id;
        }

        public void SetId(int value)
        {
            this.m_id = value;
        }

        public int GetVersion()
        {
            return this.m_version;
        }

        public void SetVersion(int value)
        {
            this.m_version = value;
        }

        public int GetStartTime()
        {
            return this.m_startTime;
        }

        public void SetStartTime(int value)
        {
            this.m_startTime = value;
        }

        public int GetEndTime()
        {
            return this.m_endTime;
        }

        public void SetEndTime(int value)
        {
            this.m_endTime = value;
        }

        public int GetVisibleTime()
        {
            return this.m_visibleTime;
        }

        public void SetVisibleTime(int value)
        {
            this.m_visibleTime = value;
        }

        public int GetVisibleEndTime()
        {
            return this.m_visibleEndTime;
        }

        public void SetVisibleEndTime(int value)
        {
            this.m_visibleEndTime = value;
        }

        public int GetInboxEntryId()
        {
            return this.m_inboxEntryId;
        }

        public void SetInboxEntryId(int value)
        {
            this.m_inboxEntryId = value;
        }

        public string GetClashBoxEntryName()
        {
            return this.m_clashBoxEntryName;
        }

        public void SetClashBoxEntryName(string value)
        {
            this.m_clashBoxEntryName = value;
        }

        public string GetNotificationTid()
        {
            return this.m_notificationTid;
        }

        public void SetNotificationTid(string value)
        {
            this.m_notificationTid = value;
        }

        public string GetImage()
        {
            return this.m_image;
        }

        public void SetImage(string value)
        {
            this.m_image = value;
        }

        public string GetSc()
        {
            return this.m_sc;
        }

        public void SetSc(string value)
        {
            this.m_sc = value;
        }

        public string GetLocalization()
        {
            return this.m_localization;
        }

        public void SetLocalization(string value)
        {
            this.m_localization = value;
        }

        public LogicEventEntryData GetEventEntryData()
        {
            return this.m_eventEntryData;
        }

        public void SetEventEntryData(LogicEventEntryData value)
        {
            this.m_eventEntryData = value;
        }

        public int GetNewTrainingBoostBarracksCost()
        {
            return this.m_newTrainingBoostBarracksCost;
        }

        public void SetNewTrainingBoostBarracksCost(int value)
        {
            this.m_newTrainingBoostBarracksCost = value;
        }

        public int GetNewTrainingBoostSpellCost()
        {
            return this.m_newTrainingBoostSpellCost;
        }

        public void SetNewTrainingBoostSpellCost(int value)
        {
            this.m_newTrainingBoostSpellCost = value;
        }

        public void AddBuildingBoost(LogicData data, int count)
        {
            this.m_buildingBoostCost.Add(new LogicDataSlot(data, count));
        }

        public int GetBuildingBoostCost(LogicBuildingData data, int upgLevel)
        {
            for (int i = 0; i < this.m_buildingBoostCost.Size(); i++)
            {
                LogicDataSlot slot = this.m_buildingBoostCost[i];

                if (slot.GetData() == data)
                {
                    return slot.GetCount();
                }
            }

            return data.GetBoostCost(upgLevel);
        }

        public void AddTroopDiscount(LogicData data, int count)
        {
            this.m_troopDiscount.Add(new LogicDataSlot(data, count));
        }

        public int GetTrainingCost(LogicCombatItemData data, int upgLevel)
        {
            int trainingCost = data.GetTrainingCost(upgLevel);

            for (int i = 0; i < this.m_troopDiscount.Size(); i++)
            {
                LogicDataSlot slot = this.m_troopDiscount[i];

                if (slot.GetData() == data)
                {
                    return (slot.GetCount() * trainingCost + 99) / 100;
                }
            }

            return trainingCost;
        }

        public void AddEnabledData(LogicData data)
        {
            this.m_enabledData.Add(data);
        }

        public bool IsEnabled(LogicData data)
        {
            if (data.IsEnableByCalendar())
            {
                if (this.m_enabledData.IndexOf(data) != -1)
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        public void AddFreeTroop(LogicCombatItemData data, int count)
        {
            if (!data.IsProductionEnabled())
            {
                Debugger.Error(data.GetName() + " cannot be produced!");
            }

            this.m_freeTroops.Add(new LogicDataSlot(data, count));
        }

        public void AddUseTroop(LogicCombatItemData data, int count, int ratioOfHousing, int rewardDiamonds, int rewardXp)
        {
            LogicCalendarUseTroop calendarUseTroop = new LogicCalendarUseTroop(data);

            calendarUseTroop.AddParameter(count);
            calendarUseTroop.AddParameter(ratioOfHousing);
            calendarUseTroop.AddParameter(rewardDiamonds);
            calendarUseTroop.AddParameter(rewardXp);

            this.m_useTroops.Add(calendarUseTroop);
        }

        public void AddBuildingDestroyedSpawnUnit(LogicBuildingData data, LogicCharacterData spawnData, int count)
        {
            this.SetBuildingDestroyedSpawnUnit(new LogicCalendarBuildingDestroyedSpawnUnit(data, spawnData, count));
        }

        public void SetBuildingDestroyedSpawnUnit(LogicCalendarBuildingDestroyedSpawnUnit buildingDestroyedSpawnUnit)
        {
            this.m_buildingDestroyedSpawnUnit = buildingDestroyedSpawnUnit;
        }

        public int GetAllianceXpMultiplier()
        {
            return this.m_allianceXpMultiplier;
        }

        public void SetAllianceXpMultiplier(int value)
        {
            this.m_allianceXpMultiplier = value;
        }

        public int GetStarBonusMultiplier()
        {
            return this.m_starBonusMultiplier;
        }

        public void SetStarBonusMultiplier(int value)
        {
            this.m_starBonusMultiplier = value;
        }

        public int GetAllianceWarWinLootMultiplier()
        {
            return this.m_allianceWarWinLootMultiplier;
        }

        public void SetAllianceWarWinLootMultiplier(int value)
        {
            this.m_allianceWarWinLootMultiplier = value;
        }

        public int GetAllianceWarDrawLootMultiplier()
        {
            return this.m_allianceWarDrawLootMultiplier;
        }

        public void SetAllianceWarDrawLootMultiplier(int value)
        {
            this.m_allianceWarDrawLootMultiplier = value;
        }

        public int GetAllianceWarLooseLootMultiplier()
        {
            return this.m_allianceWarLooseLootMultiplier;
        }

        public void SetAllianceWarLooseLootMultiplier(int value)
        {
            this.m_allianceWarLooseLootMultiplier = value;
        }

        public bool IsEqual(LogicCalendarEvent calendarEvent)
        {
            if (this.m_id == calendarEvent.m_id)
            {
                return this.m_version == calendarEvent.m_version;
            }

            return false;
        }

        public void StartUseTroopEvent(LogicAvatar homeOwnerAvatar, LogicLevel level)
        {
            if (homeOwnerAvatar != null)
            {
                for (int i = 0; i < this.m_useTroops.Size(); i++)
                {
                    LogicCalendarUseTroop calendarUseTroop = this.m_useTroops[i];
                    LogicCombatItemData data = calendarUseTroop.GetData();

                    int housingSpace;
                    int totalMaxHousing;
                    int unitCount;

                    if (data.GetCombatItemType() != LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
                    {
                        housingSpace = data.GetHousingSpace() * 2;
                        totalMaxHousing = data.GetHousingSpace() + 2 * (level.GetComponentManagerAt(data.GetVillageType()).GetTotalMaxHousing(data.GetCombatItemType()) *
                                                                        calendarUseTroop.GetParameter(1) / 100);
                        unitCount = totalMaxHousing / housingSpace;
                    }
                    else
                    {
                        LogicBuildingData troopHousingData = LogicDataTables.GetBuildingByName("Troop Housing", null);
                        LogicBuildingData barrackData = LogicDataTables.GetBuildingByName("Barrack", null);
                        LogicBuildingData darkElixirBarrackData = LogicDataTables.GetBuildingByName("Dark Elixir Barrack", null);

                        int townHallLevel = homeOwnerAvatar.GetTownHallLevel();
                        int maxUpgradeLevelForTH = troopHousingData.GetMaxUpgradeLevelForTownHallLevel(townHallLevel);
                        int unitStorageCapacity = troopHousingData.GetUnitStorageCapacity(maxUpgradeLevelForTH);

                        housingSpace = data.GetHousingSpace();

                        if (data.GetUnitOfType() == 1 && barrackData.GetRequiredTownHallLevel(data.GetRequiredProductionHouseLevel()) <= townHallLevel ||
                            data.GetUnitOfType() == 2 && darkElixirBarrackData.GetRequiredTownHallLevel(data.GetRequiredProductionHouseLevel()) <= townHallLevel)
                        {
                            int totalHousing = (int) ((long) LogicDataTables.GetTownHallLevel(townHallLevel).GetUnlockedBuildingCount(troopHousingData) *
                                                      calendarUseTroop.GetParameter(1) *
                                                      unitStorageCapacity);
                            unitCount = (int) ((housingSpace * 0.5f + totalHousing / 100) / housingSpace);
                        }
                        else
                        {
                            LogicBuildingData allianceCastleData = LogicDataTables.GetBuildingByName("Alliance Castle", null);

                            totalMaxHousing = allianceCastleData.GetUnitStorageCapacity(allianceCastleData.GetMaxUpgradeLevelForTownHallLevel(townHallLevel));
                            unitCount = totalMaxHousing / housingSpace;
                        }
                    }

                    int eventCounter = LogicMath.Max(1, unitCount) << 16;

                    homeOwnerAvatar.SetCommodityCount(6, data, eventCounter);
                    homeOwnerAvatar.GetChangeListener().CommodityCountChanged(6, data, eventCounter);

                    Debugger.HudPrint("EVENT: Use troop/spell event started!");
                }
            }
        }

        public LogicCalendarBuildingDestroyedSpawnUnit GetBuildingDestroyedSpawnUnit()
        {
            return this.m_buildingDestroyedSpawnUnit;
        }

        public LogicArrayList<LogicCalendarUseTroop> GetUseTroops()
        {
            return this.m_useTroops;
        }

        public LogicArrayList<LogicCalendarFunction> GetFunctions()
        {
            return this.m_functions;
        }
    }
}