namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Util;

    public class LogicMissionData : LogicData
    {
        private int m_missionType;
        private int m_missionCategory;
        private int m_buildBuildingCount;
        private int m_buildBuildingLevel;
        private int m_trainTroopCount;
        private int m_villagers;
        private int m_rewardResourceCount;
        private int m_customData;
        private int m_rewardXp;
        private int m_rewardCharacterCount;
        private int m_delay;
        private int m_villageType;

        private bool m_openAchievements;
        private bool m_showMap;
        private bool m_changeName;
        private bool m_switchSides;
        private bool m_showWarBase;
        private bool m_showStates;
        private bool m_openInfo;
        private bool m_showDonate;
        private bool m_warStates;
        private bool m_forceCamera;
        private bool m_deprecated;
        private bool m_firstStep;

        private string m_action;
        private string m_tutorialText;

        private LogicNpcData m_defendNpcData;
        private LogicNpcData m_attackNpcData;
        private LogicCharacterData m_characterData;
        private LogicBuildingData m_buildBuildingData;
        private LogicVillageObjectData m_fixVillageObjectData;
        private LogicCharacterData m_rewardCharacterData;
        private LogicResourceData m_rewardResourceData;
        private readonly LogicArrayList<LogicMissionData> m_missionDependencies;

        public LogicMissionData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            this.m_missionType = -1;
            this.m_missionDependencies = new LogicArrayList<LogicMissionData>();
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            for (int i = 0; i < this.GetArraySize("Dependencies"); i++)
            {
                LogicMissionData dependency = LogicDataTables.GetMissionByName(this.GetValue("Dependencies", i), this);

                if (dependency != null)
                {
                    this.m_missionDependencies.Add(dependency);
                }
            }

            this.m_action = this.GetValue("Action", 0);
            this.m_deprecated = this.GetBooleanValue("Deprecated", 0);
            this.m_missionCategory = this.GetIntegerValue("MissionCategory", 0);
            this.m_fixVillageObjectData = LogicDataTables.GetVillageObjectByName(this.GetValue("FixVillageObject", 0), this);

            if (this.m_fixVillageObjectData != null)
            {
                this.m_buildBuildingLevel = this.GetIntegerValue("BuildBuildingLevel", 0);
                this.m_missionType = 13;
            }

            if (string.Equals(this.m_action, "travel"))
            {
                this.m_missionType = 14;
            }
            else if (string.Equals(this.m_action, "upgrade2"))
            {
                this.m_characterData = LogicDataTables.GetCharacterByName(this.GetValue("Character", 0), this);
                this.m_missionType = 17;
            }
            else if (string.Equals(this.m_action, "duel"))
            {
                this.m_attackNpcData = LogicDataTables.GetNpcByName(this.GetValue("AttackNPC", 0), this);
                this.m_missionType = 18;
            }
            else if (string.Equals(this.m_action, "duel_end"))
            {
                this.m_attackNpcData = LogicDataTables.GetNpcByName(this.GetValue("AttackNPC", 0), this);
                this.m_missionType = 19;
            }
            else if (string.Equals(this.m_action, "duel_end2"))
            {
                this.m_missionType = 20;
            }
            else if (string.Equals(this.m_action, "show_builder_menu"))
            {
                this.m_missionType = 21;
            }

            this.m_buildBuildingData = LogicDataTables.GetBuildingByName(this.GetValue("BuildBuilding", 0), this);

            if (this.m_buildBuildingData != null)
            {
                this.m_buildBuildingCount = this.GetIntegerValue("BuildBuildingCount", 0);
                this.m_buildBuildingLevel = this.GetIntegerValue("BuildBuildingLevel", 0) - 1;
                this.m_missionType = string.Equals(this.m_action, "unlock") ? 15 : 5;

                if (this.m_buildBuildingCount < 0)
                {
                    Debugger.Error("missions.csv: BuildBuildingCount is invalid!");
                }
            }
            else
            {
                if (this.m_missionType == -1)
                {
                    this.m_openAchievements = this.GetBooleanValue("OpenAchievements", 0);

                    if (this.m_openAchievements)
                    {
                        this.m_missionType = 7;
                    }
                    else
                    {
                        this.m_defendNpcData = LogicDataTables.GetNpcByName(this.GetValue("DefendNPC", 0), this);

                        if (this.m_defendNpcData != null)
                        {
                            this.m_missionType = 1;
                        }
                        else
                        {
                            this.m_attackNpcData = LogicDataTables.GetNpcByName(this.GetValue("AttackNPC", 0), this);

                            if (this.m_attackNpcData != null)
                            {
                                this.m_missionType = 2;
                                this.m_showMap = this.GetBooleanValue("ShowMap", 0);
                            }
                            else
                            {
                                this.m_changeName = this.GetBooleanValue("ChangeName", 0);

                                if (this.m_changeName)
                                {
                                    this.m_missionType = 6;
                                }
                                else
                                {
                                    this.m_trainTroopCount = this.GetIntegerValue("TrainTroops", 0);

                                    if (this.m_trainTroopCount > 0)
                                    {
                                        this.m_missionType = 4;
                                    }
                                    else
                                    {
                                        this.m_switchSides = this.GetBooleanValue("SwitchSides", 0);

                                        if (this.m_switchSides)
                                        {
                                            this.m_missionType = 8;
                                        }
                                        else
                                        {
                                            this.m_showWarBase = this.GetBooleanValue("ShowWarBase", 0);

                                            if (this.m_showWarBase)
                                            {
                                                this.m_missionType = 9;
                                            }
                                            else
                                            {
                                                this.m_openInfo = this.GetBooleanValue("OpenInfo", 0);

                                                if (this.m_openInfo)
                                                {
                                                    this.m_missionType = 11;
                                                }
                                                else
                                                {
                                                    this.m_showDonate = this.GetBooleanValue("ShowDonate", 0);

                                                    if (this.m_showDonate)
                                                    {
                                                        this.m_missionType = 10;
                                                    }
                                                    else
                                                    {
                                                        this.m_showStates = this.GetBooleanValue("WarStates", 0);

                                                        if (this.m_showStates)
                                                        {
                                                            this.m_missionType = 12;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            this.m_villagers = this.GetIntegerValue("Villagers", 0);

            if (this.m_villagers > 0)
            {
                this.m_missionType = 16;
            }

            this.m_forceCamera = this.GetBooleanValue("ForceCamera", 0);

            if (this.m_missionType == -1)
            {
                Debugger.Error(string.Format("missions.csv: invalid mission ({0})", this.GetName()));
            }

            this.m_rewardResourceData = LogicDataTables.GetResourceByName(this.GetValue("RewardResource", 0), this);
            this.m_rewardResourceCount = this.GetIntegerValue("RewardResourceCount", 0);

            if (this.m_rewardResourceData != null)
            {
                if (this.m_rewardResourceCount != 0)
                {
                    if (this.m_rewardResourceCount < 0)
                    {
                        Debugger.Error("missions.csv: RewardResourceCount is negative!");

                        this.m_rewardResourceData = null;
                        this.m_rewardResourceCount = 0;
                    }
                }
                else
                {
                    this.m_rewardResourceData = null;
                }
            }
            else if (this.m_rewardResourceCount != 0)
            {
                Debugger.Warning("missions.csv: RewardResourceCount defined but RewardResource is not!");
                this.m_rewardResourceCount = 0;
            }

            this.m_customData = this.GetIntegerValue("CustomData", 0);
            this.m_rewardXp = this.GetIntegerValue("RewardXP", 0);

            if (this.m_rewardXp < 0)
            {
                Debugger.Warning("missions.csv: RewardXP is negative!");
                this.m_rewardXp = 0;
            }

            this.m_rewardCharacterData = LogicDataTables.GetCharacterByName(this.GetValue("RewardTroop", 0), this);
            this.m_rewardCharacterCount = this.GetIntegerValue("RewardTroopCount", 0);

            if (this.m_rewardCharacterData != null)
            {
                if (this.m_rewardCharacterCount != 0)
                {
                    if (this.m_rewardCharacterCount < 0)
                    {
                        Debugger.Error("missions.csv: RewardTroopCount is negative!");

                        this.m_rewardCharacterData = null;
                        this.m_rewardCharacterCount = 0;
                    }
                }
                else
                {
                    this.m_rewardCharacterData = null;
                }
            }
            else if (this.m_rewardCharacterCount != 0)
            {
                Debugger.Warning("missions.csv: RewardTroopCount defined but RewardTroop is not!");
                this.m_rewardCharacterCount = 0;
            }

            this.m_delay = this.GetIntegerValue("Delay", 0);
            this.m_villageType = this.GetIntegerValue("VillageType", 0);
            this.m_firstStep = this.GetBooleanValue("FirstStep", 0);
            this.m_tutorialText = this.GetValue("TutorialText", 0);

            if (this.m_tutorialText.Length > 0)
            {
                // BLABLABLA
            }
        }

        public bool IsOpenForAvatar(LogicClientAvatar avatar)
        {
            if (!avatar.IsMissionCompleted(this))
            {
                if (avatar.GetExpLevel() >= 10)
                {
                    if ((uint) (this.m_missionCategory - 1) > 1)
                    {
                        return false;
                    }
                }

                if (!this.m_deprecated)
                {
                    for (int i = 0; i < this.m_missionDependencies.Size(); i++)
                    {
                        if (!avatar.IsMissionCompleted(this.m_missionDependencies[i]))
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public int GetMissionType()
        {
            return this.m_missionType;
        }

        public int GetCustomData()
        {
            return this.m_customData;
        }

        public LogicCharacterData GetCharacterData()
        {
            return this.m_characterData;
        }

        public LogicCharacterData GetRewardCharacterData()
        {
            return this.m_rewardCharacterData;
        }

        public LogicVillageObjectData GetFixVillageObjectData()
        {
            return this.m_fixVillageObjectData;
        }

        public LogicBuildingData GetBuildBuildingData()
        {
            return this.m_buildBuildingData;
        }

        public LogicResourceData GetRewardResourceData()
        {
            return this.m_rewardResourceData;
        }

        public LogicNpcData GetAttackNpcData()
        {
            return this.m_attackNpcData;
        }

        public LogicNpcData GetDefendNpcData()
        {
            return this.m_defendNpcData;
        }

        public int GetRewardResourceCount()
        {
            return this.m_rewardResourceCount;
        }

        public int GetRewardCharacterCount()
        {
            return this.m_rewardCharacterCount;
        }

        public int GetRewardXp()
        {
            return this.m_rewardXp;
        }

        public int GetBuildBuildingLevel()
        {
            return this.m_buildBuildingLevel;
        }

        public int GetBuildBuildingCount()
        {
            return this.m_buildBuildingCount;
        }

        public int GetTrainTroopCount()
        {
            return this.m_trainTroopCount;
        }

        public int GetMissionCategory()
        {
            return this.m_missionCategory;
        }

        public int GetVillageType()
        {
            return this.m_villageType;
        }
    }
}