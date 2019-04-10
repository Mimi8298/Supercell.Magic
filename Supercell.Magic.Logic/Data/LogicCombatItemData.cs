namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;

    public class LogicCombatItemData : LogicGameObjectData
    {
        public const int COMBAT_ITEM_TYPE_CHARACTER = 0;
        public const int COMBAT_ITEM_TYPE_SPELL = 1;
        public const int COMBAT_ITEM_TYPE_HERO = 2;

        private LogicResourceData[] m_upgradeResourceData;
        private LogicResourceData m_trainingResourceData;

        protected int m_upgradeLevelCount;

        private int[] m_upgradeTime;
        private int[] m_upgradeCost;
        private int[] m_trainingTime;
        private int[] m_trainingCost;
        private int[] m_laboratoryLevel;
        private int[] m_upgradeLevelByTownHall;

        private int m_housingSpace;
        private int m_unitType;
        private int m_donateCost;

        private bool m_productionEnabled;
        private bool m_enableByCalendar;

        public LogicCombatItemData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicCombatItemData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            int size = this.m_upgradeLevelCount = this.m_row.GetBiggestArraySize();

            this.m_upgradeLevelByTownHall = new int[size];
            this.m_upgradeTime = new int[size];
            this.m_upgradeCost = new int[size];
            this.m_trainingTime = new int[size];
            this.m_trainingCost = new int[size];
            this.m_laboratoryLevel = new int[size];

            this.m_upgradeResourceData = new LogicResourceData[size];

            for (int i = 0; i < size; i++)
            {
                this.m_upgradeLevelByTownHall[i] = this.GetClampedIntegerValue("UpgradeLevelByTH", i);
                this.m_upgradeTime[i] = 3600 * this.GetClampedIntegerValue("UpgradeTimeH", i) + 60 * this.GetClampedIntegerValue("UpgradeTimeM", i);
                this.m_upgradeCost[i] = this.GetClampedIntegerValue("UpgradeCost", i);
                this.m_trainingTime[i] = this.GetClampedIntegerValue("TrainingTime", i);
                this.m_trainingCost[i] = this.GetClampedIntegerValue("TrainingCost", i);
                this.m_laboratoryLevel[i] = this.GetClampedIntegerValue("LaboratoryLevel", i) - 1;

                this.m_upgradeResourceData[i] = LogicDataTables.GetResourceByName(this.GetClampedValue("UpgradeResource", i), this);

                if (this.m_upgradeResourceData[i] == null && this.GetCombatItemType() != LogicCombatItemData.COMBAT_ITEM_TYPE_HERO)
                {
                    Debugger.Error("UpgradeResource is not defined for " + this.GetName());
                }
            }

            if (this.GetName().Equals("Barbarian2"))
            {
                if (this.m_upgradeTime[0] == 0)
                {
                    this.m_upgradeTime[0] = 30;
                }
            }

            this.m_trainingResourceData = LogicDataTables.GetResourceByName(this.GetValue("TrainingResource", 0), this);
            this.m_housingSpace = this.GetIntegerValue("HousingSpace", 0);
            this.m_productionEnabled = !this.GetBooleanValue("DisableProduction", 0);
            this.m_enableByCalendar = this.GetBooleanValue("EnabledByCalendar", 0);
            this.m_unitType = this.GetIntegerValue("UnitOfType", 0);
            this.m_donateCost = this.GetIntegerValue("DonateCost", 0);

            if (this.m_trainingResourceData == null && this.GetCombatItemType() != LogicCombatItemData.COMBAT_ITEM_TYPE_HERO)
            {
                Debugger.Error("TrainingResource is not defined for " + this.GetName());
            }
        }

        public virtual bool IsDonationDisabled()
        {
            return false;
        }

        public int GetDonateCost()
        {
            return this.m_donateCost;
        }

        public int GetUpgradeLevelCount()
        {
            return this.m_upgradeLevelCount;
        }

        public int GetUpgradeTime(int idx)
        {
            return this.m_upgradeTime[idx];
        }

        public LogicResourceData GetUpgradeResource(int idx)
        {
            return this.m_upgradeResourceData[idx];
        }

        public int GetUpgradeCost(int idx)
        {
            return this.m_upgradeCost[idx];
        }

        public LogicResourceData GetTrainingResource()
        {
            return this.m_trainingResourceData;
        }

        public int GetTrainingCost(int idx)
        {
            return this.m_trainingCost[idx];
        }

        public int GetUnitOfType()
        {
            return this.m_unitType;
        }

        public int GetRequiredLaboratoryLevel(int idx)
        {
            return this.m_laboratoryLevel[idx];
        }

        public virtual int GetRequiredProductionHouseLevel()
        {
            return 0;
        }

        public virtual bool IsUnlockedForProductionHouseLevel(int level)
        {
            return false;
        }

        public virtual LogicBuildingData GetProductionHouseData()
        {
            return null;
        }

        public virtual bool IsUnderground()
        {
            return false;
        }

        public int GetHousingSpace()
        {
            return this.m_housingSpace;
        }

        public int GetUpgradeLevelByTownHall(int townHallLevel)
        {
            int levelCount = this.m_upgradeLevelCount;

            if (levelCount >= 2)
            {
                int index = 1;

                while (townHallLevel + 1 >= this.m_upgradeLevelByTownHall[index])
                {
                    if (++index >= levelCount)
                    {
                        return levelCount - 1;
                    }
                }

                levelCount = index;
            }

            return levelCount - 1;
        }

        public bool UseUpgradeLevelByTownHall()
        {
            return this.m_upgradeLevelByTownHall[0] > 0;
        }

        public int GetTrainingTime(int index, LogicLevel level, int additionalBarrackCount)
        {
            int trainingTime = this.m_trainingTime[index];

            if (LogicDataTables.GetGlobals().UseNewTraining() &&
                this.GetVillageType() != 1 &&
                this.GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
            {
                if (level != null)
                {
                    LogicGameObjectManager gameObjectManager = level.GetGameObjectManagerAt(0);

                    switch (this.m_unitType)
                    {
                        case 1:
                            int barrackCount = gameObjectManager.GetBarrackCount();
                            int productionLevel = this.GetRequiredProductionHouseLevel();
                            int barrackFound = 0;

                            for (int i = 0; i < barrackCount; i++)
                            {
                                LogicBuilding barrack = (LogicBuilding) gameObjectManager.GetBarrack(i);

                                if (barrack != null)
                                {
                                    if (barrack.GetBuildingData().GetProducesUnitsOfType() == this.GetUnitOfType())
                                    {
                                        if (barrack.GetUpgradeLevel() >= productionLevel)
                                        {
                                            if (!barrack.IsConstructing())
                                            {
                                                barrackFound += 1;
                                            }
                                        }
                                    }
                                }
                            }

                            if (barrackFound + additionalBarrackCount <= 0)
                            {
                                return trainingTime;
                            }

                            int[] barrackDivisor = LogicDataTables.GetGlobals().GetBarrackReduceTrainingDevisor();
                            int divisor = barrackDivisor[LogicMath.Min(barrackDivisor.Length - 1, barrackFound + additionalBarrackCount - 1)];

                            if (divisor > 0)
                            {
                                return trainingTime / divisor;
                            }

                            return trainingTime;
                        case 2:
                            barrackCount = gameObjectManager.GetDarkBarrackCount();
                            productionLevel = this.GetRequiredProductionHouseLevel();
                            barrackFound = 0;

                            for (int i = 0; i < barrackCount; i++)
                            {
                                LogicBuilding barrack = (LogicBuilding) gameObjectManager.GetDarkBarrack(i);

                                if (barrack != null)
                                {
                                    if (barrack.GetBuildingData().GetProducesUnitsOfType() == this.GetUnitOfType())
                                    {
                                        if (barrack.GetUpgradeLevel() >= productionLevel)
                                        {
                                            if (!barrack.IsConstructing())
                                            {
                                                barrackFound += 1;
                                            }
                                        }
                                    }
                                }
                            }

                            if (barrackFound + additionalBarrackCount <= 0)
                            {
                                return trainingTime;
                            }

                            barrackDivisor = LogicDataTables.GetGlobals().GetDarkBarrackReduceTrainingDevisor();
                            divisor = barrackDivisor[LogicMath.Min(barrackDivisor.Length - 1, barrackFound + additionalBarrackCount - 1)];

                            if (divisor > 0)
                            {
                                return trainingTime / divisor;
                            }

                            return trainingTime;
                        default:
                            Debugger.Error("invalid type for unit");
                            break;
                    }
                }
                else
                {
                    Debugger.Error("level was null in getTrainingTime()");
                }
            }

            return trainingTime;
        }

        public bool IsProductionEnabled()
        {
            return this.m_productionEnabled;
        }

        public override bool IsEnableByCalendar()
        {
            return this.m_enableByCalendar;
        }

        public virtual int GetCombatItemType()
        {
            return -1;
        }
    }
}