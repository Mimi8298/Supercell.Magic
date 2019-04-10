namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Util;

    public class LogicAchievementData : LogicData
    {
        public const int ACTION_TYPE_NPC_STARS = 0;
        public const int ACTION_TYPE_UPGRADE = 1;
        public const int ACTION_TYPE_VICTORY_POINTS = 2;
        public const int ACTION_TYPE_UNIT_UNLOCK = 3;
        public const int ACTION_TYPE_CLEAR_OBSTACLES = 4;
        public const int ACTION_TYPE_DONATE_UNITS = 5;
        public const int ACTION_TYPE_LOOT = 6;
        public const int ACTION_TYPE_DESTROY = 9;
        public const int ACTION_TYPE_WIN_PVP_DEFENSE = 10;
        public const int ACTION_TYPE_WIN_PVP_ATTACK = 11;
        public const int ACTION_TYPE_LEAGUE = 12;
        public const int ACTION_TYPE_WAR_STARS = 13;
        public const int ACTION_TYPE_WAR_LOOT = 14;
        public const int ACTION_TYPE_DONATE_SPELLS = 15;
        public const int ACTION_TYPE_ACCOUNT_BOUND = 16;
        public const int ACTION_TYPE_VERSUS_BATTLE_TROPHIES = 17;
        public const int ACTION_TYPE_GEAR_UP = 18;
        public const int ACTION_TYPE_REPAIR_BUILDING = 19;

        private bool m_showValue;

        private int m_actionType;
        private int m_diamondReward;
        private int m_expReward;
        private int m_actionCount;
        private int m_level;
        private int m_levelCount;
        private int m_villageType;

        private string m_completedTID;
        private string m_androidId;

        private LogicBuildingData m_buildingData;
        private LogicResourceData m_resourceData;
        private LogicCharacterData m_characterData;
        private LogicArrayList<LogicAchievementData> m_achievementLevel;

        public LogicAchievementData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicAchievementData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_villageType = this.GetIntegerValue("UIGroup", 0);
            this.m_diamondReward = this.GetIntegerValue("DiamondReward", 0);
            this.m_expReward = this.GetIntegerValue("ExpReward", 0);
            this.m_actionCount = this.GetIntegerValue("ActionCount", 0);
            this.m_level = this.GetIntegerValue("Level", 0);
            this.m_levelCount = this.GetIntegerValue("LevelCount", 0);

            this.m_completedTID = this.GetValue("CompletedTID", 0);
            this.m_showValue = this.GetBooleanValue("ShowValue", 0);
            this.m_androidId = this.GetValue("AndroidID", 0);

            if (this.m_actionCount == 0)
            {
                Debugger.Error("Achievement has invalid ActionCount 0");
            }

            string action = this.GetValue("Action", 0);

            switch (action)
            {
                case "npc_stars":
                    this.m_actionType = LogicAchievementData.ACTION_TYPE_NPC_STARS;
                    break;
                case "upgrade":
                    this.m_actionType = LogicAchievementData.ACTION_TYPE_UPGRADE;
                    this.m_buildingData = LogicDataTables.GetBuildingByName(this.GetValue("ActionData", 0), this);

                    if (this.m_buildingData == null)
                    {
                        Debugger.Error("LogicAchievementData - Building data is NULL for upgrade achievement");
                    }

                    break;
                case "victory_points":
                    this.m_actionType = LogicAchievementData.ACTION_TYPE_VICTORY_POINTS;
                    break;
                case "unit_unlock":
                    this.m_actionType = LogicAchievementData.ACTION_TYPE_UNIT_UNLOCK;
                    this.m_characterData = LogicDataTables.GetCharacterByName(this.GetValue("ActionData", 0), this);

                    if (this.m_characterData == null)
                    {
                        Debugger.Error("LogicCharacterData - Character data is NULL for unit_unlock achievement");
                    }

                    break;
                case "clear_obstacles":
                    this.m_actionType = LogicAchievementData.ACTION_TYPE_CLEAR_OBSTACLES;
                    break;
                case "donate_units":
                    this.m_actionType = LogicAchievementData.ACTION_TYPE_DONATE_UNITS;
                    break;
                case "loot":
                    this.m_actionType = LogicAchievementData.ACTION_TYPE_LOOT;
                    this.m_resourceData = LogicDataTables.GetResourceByName(this.GetValue("ActionData", 0), this);

                    if (this.m_resourceData == null)
                    {
                        Debugger.Error("LogicAchievementData - Resource data is NULL for loot achievement");
                    }

                    break;
                case "destroy":
                    this.m_actionType = LogicAchievementData.ACTION_TYPE_DESTROY;
                    this.m_buildingData = LogicDataTables.GetBuildingByName(this.GetValue("ActionData", 0), this);

                    if (this.m_buildingData == null)
                    {
                        Debugger.Error("LogicAchievementData - Building data is NULL for destroy achievement");
                    }

                    break;
                case "win_pvp_attack":
                    this.m_actionType = LogicAchievementData.ACTION_TYPE_WIN_PVP_ATTACK;
                    break;
                case "win_pvp_defense":
                    this.m_actionType = LogicAchievementData.ACTION_TYPE_WIN_PVP_DEFENSE;
                    break;
                case "league":
                    this.m_actionType = LogicAchievementData.ACTION_TYPE_LEAGUE;
                    break;
                case "war_stars":
                    this.m_actionType = LogicAchievementData.ACTION_TYPE_WAR_STARS;
                    break;
                case "war_loot":
                    this.m_actionType = LogicAchievementData.ACTION_TYPE_WAR_LOOT;
                    break;
                case "donate_spells":
                    this.m_actionType = LogicAchievementData.ACTION_TYPE_DONATE_SPELLS;
                    break;
                case "account_bound":
                    this.m_actionType = LogicAchievementData.ACTION_TYPE_ACCOUNT_BOUND;
                    break;
                case "vs_battle_trophies":
                    this.m_actionType = LogicAchievementData.ACTION_TYPE_VERSUS_BATTLE_TROPHIES;
                    break;
                case "gear_up":
                    this.m_actionType = LogicAchievementData.ACTION_TYPE_GEAR_UP;
                    break;
                case "repair_building":
                    this.m_actionType = LogicAchievementData.ACTION_TYPE_REPAIR_BUILDING;
                    this.m_buildingData = LogicDataTables.GetBuildingByName(this.GetValue("ActionData", 0), this);

                    if (this.m_buildingData == null)
                    {
                        Debugger.Error("LogicAchievementData - Building data is NULL for repair_building achievement");
                    }

                    break;
                default:
                    Debugger.Error(string.Format("Unknown Action in achievements {0}", action));
                    break;
            }

            this.m_achievementLevel = new LogicArrayList<LogicAchievementData>();

            string achievementName = this.GetName().Substring(0, this.GetName().Length - 1);
            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.ACHIEVEMENT);

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicAchievementData achievementData = (LogicAchievementData) table.GetItemAt(i);

                if (achievementData.GetName().Contains(achievementName))
                {
                    if (achievementData.GetName().Substring(0, achievementData.GetName().Length - 1).Equals(achievementName))
                    {
                        this.m_achievementLevel.Add(achievementData);
                    }
                }
            }

            Debugger.DoAssert(this.m_achievementLevel.Size() == this.m_levelCount, string.Format(
                                  "Expected same amount of achievements named {0}X to be same as LevelCount={1} for {2}.",
                                  achievementName,
                                  this.m_levelCount,
                                  this.GetName()));
        }

        public int GetVillageType()
        {
            return this.m_villageType;
        }

        public int GetActionType()
        {
            return this.m_actionType;
        }

        public int GetDiamondReward()
        {
            return this.m_diamondReward;
        }

        public int GetExpReward()
        {
            return this.m_expReward;
        }

        public int GetActionCount()
        {
            return this.m_actionCount;
        }

        public int GetLevel()
        {
            return this.m_level;
        }

        public string GetCompletedTID()
        {
            return this.m_completedTID;
        }

        public string GetAndroidID()
        {
            return this.m_androidId;
        }

        public LogicArrayList<LogicAchievementData> GetAchievementLevels()
        {
            return this.m_achievementLevel;
        }

        public LogicBuildingData GetBuildingData()
        {
            return this.m_buildingData;
        }

        public LogicResourceData GetResourceData()
        {
            return this.m_resourceData;
        }

        public LogicCharacterData GetCharacterData()
        {
            return this.m_characterData;
        }
    }
}