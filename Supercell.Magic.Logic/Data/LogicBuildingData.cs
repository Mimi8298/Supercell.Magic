namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class LogicBuildingData : LogicGameObjectData
    {
        private LogicBuildingData m_gearUpBuildingData;
        private LogicBuildingClassData m_buildingClass;
        private LogicBuildingClassData m_secondaryTargetingClass;
        private LogicBuildingClassData m_shopBuildingClass;
        private LogicResourceData[] m_buildResourceData;
        private LogicResourceData[] m_altBuildResourceData;
        private LogicResourceData[] m_ammoResourceData;
        private LogicResourceData m_gearUpResourceData;
        private LogicResourceData m_produceResourceData;
        private LogicHeroData m_heroData;
        private LogicArrayList<int>[] m_storedResourceCounts;
        private LogicArrayList<int>[] m_percentageStoredResourceCounts;
        private LogicArrayList<LogicAttackerItemData> m_attackItemData;
        private LogicSpellData m_areaOfEffectSpellData;
        private LogicSpellData m_alternativeAreaOfEffectSpellData;
        private LogicEffectData m_loadAmmoEffect;
        private LogicEffectData m_noAmmoEffect;
        private LogicEffectData m_toggleAttackModeEffect;
        private LogicEffectData m_pickUpEffect;
        private LogicEffectData m_placingEffect;
        private LogicEffectData m_appearEffect;
        private LogicEffectData m_dieDamageEffect;
        private LogicCharacterData[] m_defenceTroopCharacter;
        private LogicCharacterData[] m_defenceTroopCharacter2;

        private int[] m_buildCost;
        private int[] m_constructionTimes;
        private int[] m_townHallLevel;
        private int[] m_townHallVillage2Level;
        private int[] m_wallBlockX;
        private int[] m_wallBlockY;
        private int[] m_gearUpTime;
        private int[] m_gearUpCost;
        private int[] m_boostCost;
        private int[] m_ammoCost;
        private int[] m_housingSpace;
        private int[] m_housingSpaceAlt;
        private int[] m_resourcePer100Hours;
        private int[] m_resourceMax;
        private int[] m_resourceIconLimit;
        private int[] m_hitpoints;
        private int[] m_regenTime;
        private int[] m_amountCanBeUpgraded;
        private int[] m_unitProduction;
        private int[] m_strengthWeight;
        private int[] m_destructionXP;
        private int[] m_defenceTroopLevel;
        private int[] m_defenceTroopCount;
        private int[] m_dieDamage;

        private int m_width;
        private int m_height;
        private int m_village2Housing;
        private int m_producesUnitsOfType;
        private int m_chainAttackDistance;
        private int m_buildingW;
        private int m_buildingH;
        private int m_baseGfx;
        private int m_startingHomeCount;
        private int m_triggerRadius;
        private int m_aimRotateStep;
        private int m_turnSpeed;
        private int m_dieDamageRadius;
        private int m_dieDamageDelay;
        private int m_redMul;
        private int m_greenMul;
        private int m_blueMul;
        private int m_redAdd;
        private int m_greenAdd;
        private int m_blueAdd;
        private int m_newTargetAttackDelay;
        private int m_gearUpLevelRequirement;
        private int m_upgradeLevelCount;
        private int m_targetingConeAngle;

        private bool m_lootOnDestruction;
        private bool m_bunker;
        private bool m_upgradesUnits;
        private bool m_freeBoost;
        private bool m_randomHitPosition;
        private bool m_canNotSellLast;
        private bool m_locked;
        private bool m_hidden;
        private bool m_forgesSpells;
        private bool m_forgesMiniSpells;
        private bool m_isHeroBarrack;
        private bool m_needsAim;
        private bool m_shareHeroCombatData;
        private bool m_isRed;
        private bool m_selfAsAoeCenter;
        private bool m_isClockTower;
        private bool m_isFlamer;
        private bool m_isBarrackVillage2;

        private string m_exportNameNpc;
        private string m_exportNameConstruction;
        private string m_exportNameLocked;
        private string m_exportNameBeamStart;
        private string m_exportNameBeamEnd;

        public LogicBuildingData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicBuildingData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_upgradeLevelCount = this.m_row.GetBiggestArraySize();
            this.m_buildingClass = LogicDataTables.GetBuildingClassByName(this.GetValue("BuildingClass", 0), this);

            if (this.m_buildingClass == null)
            {
                Debugger.Error("Building class is not defined for " + this.GetName());
            }

            this.m_secondaryTargetingClass = LogicDataTables.GetBuildingClassByName(this.GetValue("SecondaryTargetingClass", 0), this);
            this.m_shopBuildingClass = LogicDataTables.GetBuildingClassByName(this.GetValue("ShopBuildingClass", 0), this);

            if (this.m_shopBuildingClass == null)
            {
                this.m_shopBuildingClass = this.m_buildingClass;
            }

            this.m_exportNameNpc = this.GetValue("ExportNameNpc", 0);
            this.m_exportNameConstruction = this.GetValue("ExportNameConstruction", 0);
            this.m_exportNameLocked = this.GetValue("ExportNameLocked", 0);
            this.m_width = this.GetIntegerValue("Width", 0);
            this.m_height = this.GetIntegerValue("Height", 0);
            this.m_lootOnDestruction = this.GetBooleanValue("LootOnDestruction", 0);
            this.m_bunker = this.GetBooleanValue("Bunker", 0);
            this.m_village2Housing = this.GetIntegerValue("Village2Housing", 0);
            this.m_upgradesUnits = this.GetBooleanValue("UpgradesUnits", 0);
            this.m_producesUnitsOfType = this.GetIntegerValue("ProducesUnitsOfType", 0);
            this.m_freeBoost = this.GetBooleanValue("FreeBoost", 0);
            this.m_randomHitPosition = this.GetBooleanValue("RandomHitPosition", 0);
            this.m_chainAttackDistance = this.GetIntegerValue("ChainAttackDistance", 0);
            this.m_buildingW = this.GetIntegerValue("BuildingW", 0);
            this.m_buildingH = this.GetIntegerValue("BuildingH", 0);

            if (this.m_buildingW == 0)
            {
                this.m_buildingW = this.m_width;
            }

            if (this.m_buildingH == 0)
            {
                this.m_buildingH = this.m_height;
            }

            this.m_baseGfx = this.GetIntegerValue("BaseGfx", 0);
            this.m_loadAmmoEffect = LogicDataTables.GetEffectByName(this.GetValue("LoadAmmoEffect", 0), this);
            this.m_noAmmoEffect = LogicDataTables.GetEffectByName(this.GetValue("NoAmmoEffect", 0), this);
            this.m_toggleAttackModeEffect = LogicDataTables.GetEffectByName(this.GetValue("ToggleAttackModeEffect", 0), this);
            this.m_pickUpEffect = LogicDataTables.GetEffectByName(this.GetValue("PickUpEffect", 0), this);
            this.m_placingEffect = LogicDataTables.GetEffectByName(this.GetValue("PlacingEffect", 0), this);
            this.m_canNotSellLast = this.GetBooleanValue("CanNotSellLast", 0);
            this.m_locked = this.GetBooleanValue("Locked", 0);
            this.m_startingHomeCount = this.GetIntegerValue("StartingHomeCount", 0);
            this.m_hidden = this.GetBooleanValue("Hidden", 0);
            this.m_triggerRadius = (this.GetIntegerValue("TriggerRadius", 0) << 9) / 100;
            this.m_appearEffect = LogicDataTables.GetEffectByName(this.GetValue("AppearEffect", 0), this);
            this.m_forgesSpells = this.GetBooleanValue("ForgesSpells", 0);
            this.m_forgesMiniSpells = this.GetBooleanValue("ForgesMiniSpells", 0);
            this.m_isHeroBarrack = this.GetBooleanValue("IsHeroBarrack", 0);
            this.m_aimRotateStep = this.GetIntegerValue("AimRotateStep", 0);
            this.m_turnSpeed = this.GetIntegerValue("TurnSpeed", 0);

            if (this.m_turnSpeed == 0)
            {
                this.m_turnSpeed = 500;
            }

            this.m_needsAim = this.GetBooleanValue("NeedsAim", 0);
            this.m_exportNameBeamStart = this.GetValue("ExportNameBeamStart", 0);
            this.m_exportNameBeamEnd = this.GetValue("ExportNameBeamEnd", 0);
            this.m_shareHeroCombatData = this.GetBooleanValue("ShareHeroCombatData", 0);
            this.m_dieDamageRadius = (this.GetIntegerValue("DieDamageRadius", 0) << 9) / 100;
            this.m_dieDamageEffect = LogicDataTables.GetEffectByName(this.GetValue("DieDamageEffect", 0), this);
            this.m_dieDamageDelay = this.GetIntegerValue("DieDamageDelay", 0);

            if (this.m_dieDamageDelay > 4000)
            {
                Debugger.Warning("m_dieDamageDelay too big");
                this.m_dieDamageDelay = 4000;
            }

            this.m_isRed = this.GetBooleanValue("IsRed", 0);
            this.m_redMul = this.GetIntegerValue("RedMul", 0);
            this.m_greenMul = this.GetIntegerValue("GreenMul", 0);
            this.m_blueMul = this.GetIntegerValue("BlueMul", 0);
            this.m_redAdd = this.GetIntegerValue("RedAdd", 0);
            this.m_greenAdd = this.GetIntegerValue("GreenAdd", 0);
            this.m_blueAdd = this.GetIntegerValue("BlueAdd", 0);

            this.m_selfAsAoeCenter = this.GetBooleanValue("SelfAsAoeCenter", 0);
            this.m_newTargetAttackDelay = this.GetIntegerValue("NewTargetAttackDelay", 0);
            this.m_gearUpLevelRequirement = this.GetIntegerValue("GearUpLevelRequirement", 0);
            this.m_bunker = this.GetBooleanValue("Bunker", 0);

            int longestArraySize = this.m_row.GetBiggestArraySize();

            this.m_buildResourceData = new LogicResourceData[longestArraySize];
            this.m_altBuildResourceData = new LogicResourceData[longestArraySize];
            this.m_storedResourceCounts = new LogicArrayList<int>[longestArraySize];
            this.m_percentageStoredResourceCounts = new LogicArrayList<int>[longestArraySize];
            this.m_ammoResourceData = new LogicResourceData[longestArraySize];
            this.m_attackItemData = new LogicArrayList<LogicAttackerItemData>(longestArraySize);
            this.m_defenceTroopCharacter = new LogicCharacterData[longestArraySize];
            this.m_defenceTroopCharacter2 = new LogicCharacterData[longestArraySize];

            this.m_buildCost = new int[longestArraySize];
            this.m_ammoCost = new int[longestArraySize];
            this.m_townHallLevel = new int[longestArraySize];
            this.m_townHallVillage2Level = new int[longestArraySize];
            this.m_constructionTimes = new int[longestArraySize];
            this.m_gearUpTime = new int[longestArraySize];
            this.m_gearUpCost = new int[longestArraySize];
            this.m_boostCost = new int[longestArraySize];
            this.m_housingSpace = new int[longestArraySize];
            this.m_housingSpaceAlt = new int[longestArraySize];
            this.m_resourcePer100Hours = new int[longestArraySize];
            this.m_resourceMax = new int[longestArraySize];
            this.m_resourceIconLimit = new int[longestArraySize];
            this.m_hitpoints = new int[longestArraySize];
            this.m_regenTime = new int[longestArraySize];
            this.m_amountCanBeUpgraded = new int[longestArraySize];
            this.m_unitProduction = new int[longestArraySize];
            this.m_strengthWeight = new int[longestArraySize];
            this.m_destructionXP = new int[longestArraySize];
            this.m_defenceTroopCount = new int[longestArraySize];
            this.m_defenceTroopLevel = new int[longestArraySize];
            this.m_dieDamage = new int[longestArraySize];
            this.m_wallBlockX = new int[0];
            this.m_wallBlockY = new int[0];

            for (int i = 0; i < longestArraySize; i++)
            {
                LogicAttackerItemData itemData = new LogicAttackerItemData();
                itemData.CreateReferences(this.m_row, this, i);
                this.m_attackItemData.Add(itemData);

                this.m_dieDamage[i] = this.GetClampedIntegerValue("DieDamage", i);
                this.m_buildCost[i] = this.GetClampedIntegerValue("BuildCost", i);
                this.m_housingSpace[i] = this.GetClampedIntegerValue("HousingSpace", i);
                this.m_housingSpaceAlt[i] = this.GetClampedIntegerValue("HousingSpaceAlt", i);
                this.m_unitProduction[i] = this.GetClampedIntegerValue("UnitProduction", i);
                this.m_gearUpCost[i] = this.GetClampedIntegerValue("GearUpCost", i);
                this.m_boostCost[i] = this.GetClampedIntegerValue("BoostCost", i);
                this.m_resourcePer100Hours[i] = this.GetClampedIntegerValue("ResourcePer100Hours", i);
                this.m_resourceMax[i] = this.GetClampedIntegerValue("ResourceMax", i);
                this.m_resourceIconLimit[i] = this.GetClampedIntegerValue("ResourceIconLimit", i);
                this.m_hitpoints[i] = this.GetClampedIntegerValue("Hitpoints", i);
                this.m_regenTime[i] = this.GetClampedIntegerValue("RegenTime", i);
                this.m_amountCanBeUpgraded[i] = this.GetClampedIntegerValue("AmountCanBeUpgraded", i);
                this.m_buildResourceData[i] = LogicDataTables.GetResourceByName(this.GetClampedValue("BuildResource", i), this);
                this.m_altBuildResourceData[i] = LogicDataTables.GetResourceByName(this.GetClampedValue("AltBuildResource", i), this);
                this.m_townHallLevel[i] = LogicMath.Max(this.GetClampedIntegerValue("TownHallLevel", i) - 1, 0);
                this.m_townHallVillage2Level[i] = LogicMath.Max(this.GetClampedIntegerValue("TownHallLevel2", i) - 1, 0);
                this.m_storedResourceCounts[i] = new LogicArrayList<int>();
                this.m_percentageStoredResourceCounts[i] = new LogicArrayList<int>();

                LogicDataTable table = LogicDataTables.GetTable(LogicDataType.RESOURCE);

                for (int j = 0; j < table.GetItemCount(); j++)
                {
                    this.m_storedResourceCounts[i].Add(this.GetClampedIntegerValue("MaxStored" + table.GetItemAt(j).GetName(), i));
                    this.m_percentageStoredResourceCounts[i].Add(this.GetClampedIntegerValue("PercentageStored" + table.GetItemAt(j).GetName(), i));
                }

                this.m_gearUpTime[i] = 60 * this.GetClampedIntegerValue("GearUpTime", i);
                this.m_constructionTimes[i] = 86400 * this.GetClampedIntegerValue("BuildTimeD", i) +
                                              3600 * this.GetClampedIntegerValue("BuildTimeH", i) +
                                              60 * this.GetClampedIntegerValue("BuildTimeM", i) +
                                              this.GetIntegerValue("BuildTimeS", i);
                this.m_destructionXP[i] = this.GetClampedIntegerValue("DestructionXP", i);
                this.m_ammoResourceData[i] = LogicDataTables.GetResourceByName(this.GetClampedValue("AmmoResource", i), this);
                this.m_ammoCost[i] = this.GetClampedIntegerValue("AmmoCost", i);
                this.m_strengthWeight[i] = this.GetClampedIntegerValue("StrengthWeight", i);

                string defenceTroopCharacter = this.GetClampedValue("DefenceTroopCharacter", i);

                if (defenceTroopCharacter.Length > 0)
                {
                    this.m_defenceTroopCharacter[i] = LogicDataTables.GetCharacterByName(defenceTroopCharacter, this);
                }

                string defenceTroopCharacter2 = this.GetClampedValue("DefenceTroopCharacter2", i);

                if (defenceTroopCharacter2.Length > 0)
                {
                    this.m_defenceTroopCharacter2[i] = LogicDataTables.GetCharacterByName(defenceTroopCharacter2, this);
                }

                this.m_defenceTroopCount[i] = this.GetIntegerValue("DefenceTroopCount", i);
                this.m_defenceTroopLevel[i] = this.GetIntegerValue("DefenceTroopLevel", i);

                if (i > 0 && this.m_housingSpace[i] < this.m_housingSpace[i - 1])
                    Debugger.Error("Building " + this.GetName() + " unit storage space decreases by upgrade level!");
                if (this.m_gearUpCost[i] > 0 && this.m_gearUpTime[i] <= 0 || this.m_gearUpCost[i] <= 0 && this.m_gearUpTime[i] > 0)
                    Debugger.Error("invalid gear up settings. gear up time and cost must be set for levels where available");
            }

            this.m_areaOfEffectSpellData = LogicDataTables.GetSpellByName(this.GetValue("AOESpell", 0), this);
            this.m_alternativeAreaOfEffectSpellData = LogicDataTables.GetSpellByName(this.GetValue("AOESpellAlternate", 0), this);
            this.m_produceResourceData = LogicDataTables.GetResourceByName(this.GetValue("ProducesResource", 0), this);
            this.m_gearUpResourceData = LogicDataTables.GetResourceByName(this.GetValue("GearUpResource", 0), this);

            string heroType = this.GetValue("HeroType", 0);

            if (!string.IsNullOrEmpty(heroType))
            {
                this.m_heroData = LogicDataTables.GetHeroByName(heroType, this);
            }

            string wallBlockX = this.GetValue("WallBlockX", 0);

            if (wallBlockX.Length > 0)
            {
                this.LoadWallBlock(wallBlockX, out this.m_wallBlockX);
                this.LoadWallBlock(this.GetValue("WallBlockY", 0), out this.m_wallBlockY);

                if (this.m_wallBlockX.Length != this.m_wallBlockY.Length)
                {
                    Debugger.Error("LogicBuildingData: Error parsing wall offsets");
                }

                if (this.m_wallBlockX.Length > 10)
                {
                    Debugger.Error("LogicBuildingData: Too many wall blocks");
                }
            }

            string gearUpBuilding = this.GetValue("GearUpBuilding", 0);

            if (gearUpBuilding.Length > 0)
            {
                this.m_gearUpBuildingData = LogicDataTables.GetBuildingByName(gearUpBuilding, this);
            }

            this.m_isClockTower = this.GetName().Equals("Clock Tower");
            this.m_isFlamer = this.GetName().Equals("Flamer");
            this.m_isBarrackVillage2 = this.GetName().Equals("Barrack2");
        }

        public void LoadWallBlock(string value, out int[] wallBlock)
        {
            string[] tmp = value.Split(',');
            wallBlock = new int[tmp.Length];

            for (int i = 0; i < tmp.Length; i++)
            {
                wallBlock[i] = int.Parse(tmp[i]);
            }
        }

        public int GetWallBlockIndex(int x, int y, int idx)
        {
            int wallBlockX = this.m_wallBlockX[idx];
            int wallBlockY = this.m_wallBlockY[idx];

            for (int i = 0; i < 4; i++)
            {
                if (x == wallBlockX && wallBlockY == y)
                {
                    return i;
                }

                int tmp = x;

                x = -y;
                y = tmp;
            }

            return -1;
        }

        public LogicBuildingClassData GetBuildingClass()
        {
            return this.m_buildingClass;
        }

        public LogicAttackerItemData GetAttackerItemData(int idx)
        {
            return this.m_attackItemData[idx];
        }

        public int GetUpgradeLevelCount()
        {
            return this.m_upgradeLevelCount;
        }

        public int GetConstructionTime(int upgLevel, LogicLevel level, int ignoreBuildingCnt)
        {
            if (this.GetVillage2Housing() < 1)
            {
                return this.m_constructionTimes[upgLevel];
            }

            return LogicDataTables.GetGlobals().GetTroopHousingBuildTimeVillage2(level, ignoreBuildingCnt);
        }

        public bool IsTownHall()
        {
            return this.m_buildingClass.IsTownHall();
        }

        public bool IsTownHallVillage2()
        {
            return this.m_buildingClass.IsTownHall2();
        }

        public bool IsWorkerBuilding()
        {
            return this.m_buildingClass.IsWorker();
        }

        public bool IsWall()
        {
            return this.m_buildingClass.IsWall();
        }

        public bool IsAllianceCastle()
        {
            return this.m_bunker;
        }

        public bool IsLaboratory()
        {
            return this.m_upgradesUnits;
        }

        public bool IsLocked()
        {
            return this.m_locked;
        }

        public bool IsClockTower()
        {
            return this.m_isClockTower;
        }

        public bool IsFlamer()
        {
            return this.m_isFlamer;
        }

        public bool IsBarrackVillage2()
        {
            return this.m_isBarrackVillage2;
        }

        public int GetUnitStorageCapacity(int level)
        {
            return this.m_housingSpace[level];
        }

        public int GetAltUnitStorageCapacity(int level)
        {
            return this.m_housingSpaceAlt[level];
        }

        public LogicResourceData GetGearUpResource()
        {
            return this.m_gearUpResourceData;
        }

        public LogicResourceData GetBuildResource(int idx)
        {
            return this.m_buildResourceData[idx];
        }

        public LogicResourceData GetAltBuildResource(int idx)
        {
            return this.m_altBuildResourceData[idx];
        }

        public LogicResourceData GetProduceResource()
        {
            return this.m_produceResourceData;
        }

        public LogicHeroData GetHeroData()
        {
            return this.m_heroData;
        }

        public LogicBuildingData GetGearUpBuildingData()
        {
            return this.m_gearUpBuildingData;
        }

        public LogicSpellData GetAreaOfEffectSpell()
        {
            return this.m_areaOfEffectSpellData;
        }

        public LogicSpellData GetAltAreaOfEffectSpell()
        {
            return this.m_alternativeAreaOfEffectSpellData;
        }

        public int GetBuildCost(int index, LogicLevel level)
        {
            if (this.GetVillage2Housing() <= 0)
            {
                if (this.m_buildingClass.IsWorker())
                {
                    return LogicDataTables.GetGlobals().GetWorkerCost(level);
                }

                return this.m_buildCost[index];
            }

            return LogicDataTables.GetGlobals().GetTroopHousingBuildCostVillage2(level);
        }

        public int GetRequiredTownHallLevel(int index)
        {
            if (index != 0 || LogicDataTables.GetTownHallLevelCount() < 1)
            {
                return this.m_townHallLevel[index];
            }

            for (int i = 0; i < LogicDataTables.GetTownHallLevelCount(); i++)
            {
                LogicTownhallLevelData townHallLevel = LogicDataTables.GetTownHallLevel(i);

                if (townHallLevel.GetUnlockedBuildingCount(this) > 0)
                {
                    return i;
                }
            }

            return this.m_townHallLevel[index];
        }

        public int GetTownHallLevel2(int index)
        {
            return this.m_townHallVillage2Level[index];
        }

        public int GetWidth()
        {
            return this.m_width;
        }

        public int GetHeight()
        {
            return this.m_height;
        }

        public bool StoresResources()
        {
            LogicArrayList<int> storeCount = this.m_storedResourceCounts[0];

            for (int i = 0; i < storeCount.Size(); i++)
            {
                if (storeCount[i] > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetMaxStoredGold(int upgLevel)
        {
            return this.m_storedResourceCounts[upgLevel][LogicDataTables.GetGoldData().GetInstanceID()];
        }

        public int GetMaxStoredElixir(int upgLevel)
        {
            return this.m_storedResourceCounts[upgLevel][LogicDataTables.GetElixirData().GetInstanceID()];
        }

        public int GetMaxStoredDarkElixir(int upgLevel)
        {
            return this.m_storedResourceCounts[upgLevel][LogicDataTables.GetDarkElixirData().GetInstanceID()];
        }

        public int GetMaxUpgradeLevelForTownHallLevel(int townHallLevel)
        {
            int count = this.m_upgradeLevelCount;

            while (count > 0)
            {
                if (this.GetRequiredTownHallLevel(--count) <= townHallLevel)
                {
                    return count;
                }
            }

            return -1;
        }

        public int GetMinUpgradeLevelForGearUp()
        {
            int count = this.m_upgradeLevelCount;

            for (int i = 0; i < count; i++)
            {
                if (this.m_gearUpCost[i] > 0)
                {
                    return i;
                }
            }

            return -1;
        }

        public LogicArrayList<int> GetMaxStoredResourceCounts(int idx)
        {
            return this.m_storedResourceCounts[idx];
        }

        public LogicArrayList<int> GetMaxPercentageStoredResourceCounts(int idx)
        {
            return this.m_percentageStoredResourceCounts[idx];
        }

        public int GetResourcePer100Hours(int index)
        {
            return this.m_resourcePer100Hours[index];
        }

        public int GetResourceMax(int index)
        {
            return this.m_resourceMax[index];
        }

        public int GetResourceIconLimit(int index)
        {
            return this.m_resourceIconLimit[index];
        }

        public int GetBoostCost(int index)
        {
            return this.m_boostCost[index];
        }

        public int GetHitpoints(int index)
        {
            return this.m_hitpoints[index];
        }

        public int GetRegenerationTime(int index)
        {
            return this.m_regenTime[index];
        }

        public int GetAmmoCost(int index, int count)
        {
            if (count < 1)
            {
                return 0;
            }

            return LogicMath.Max(this.m_ammoCost[index] * count / this.m_attackItemData[index].GetAmmoCount(), 1);
        }

        public LogicResourceData GetAmmoResourceData(int idx)
        {
            return this.m_ammoResourceData[idx];
        }

        public int GetAmountCanBeUpgraded(int index)
        {
            return this.m_amountCanBeUpgraded[index];
        }

        public int GetGearUpCost(int index)
        {
            return this.m_gearUpCost[index];
        }

        public int GetGearUpTime(int index)
        {
            return this.m_gearUpTime[index];
        }

        public int GetWallBlockX(int index)
        {
            return this.m_wallBlockX[index];
        }

        public int GetWallBlockY(int index)
        {
            return this.m_wallBlockY[index];
        }

        public int GetWallBlockCount()
        {
            return this.m_wallBlockX.Length;
        }

        public string GetExportNameNpc()
        {
            return this.m_exportNameNpc;
        }

        public string GetExportNameConstruction()
        {
            return this.m_exportNameConstruction;
        }

        public string GetExportNameLocked()
        {
            return this.m_exportNameLocked;
        }

        public bool IsLootOnDestruction()
        {
            return this.m_lootOnDestruction;
        }

        public int GetVillage2Housing()
        {
            return this.m_village2Housing;
        }

        public bool IsFreeBoost()
        {
            return this.m_freeBoost;
        }

        public bool IsRandomHitPosition()
        {
            return this.m_randomHitPosition;
        }

        public int GetChainAttackDistance()
        {
            return this.m_chainAttackDistance;
        }

        public int GetBuildingW()
        {
            return this.m_buildingW;
        }

        public int GetBuildingH()
        {
            return this.m_buildingH;
        }

        public int GetBaseGfx()
        {
            return this.m_baseGfx;
        }

        public LogicEffectData GetLoadAmmoEffect()
        {
            return this.m_loadAmmoEffect;
        }

        public LogicEffectData GetNoAmmoEffect()
        {
            return this.m_noAmmoEffect;
        }

        public LogicEffectData GetToggleAttackModeEffect()
        {
            return this.m_toggleAttackModeEffect;
        }

        public LogicEffectData GetPickUpEffect()
        {
            return this.m_pickUpEffect;
        }

        public LogicEffectData GetPlacingEffect()
        {
            return this.m_placingEffect;
        }

        public bool IsCanNotSellLast()
        {
            return this.m_canNotSellLast;
        }

        public int GetStartingHomeCount()
        {
            return this.m_startingHomeCount;
        }

        public bool IsHidden()
        {
            return this.m_hidden;
        }

        public int GetTriggerRadius()
        {
            return this.m_triggerRadius;
        }

        public LogicEffectData GetAppearEffect()
        {
            return this.m_appearEffect;
        }

        public bool IsForgesSpells()
        {
            return this.m_forgesSpells;
        }

        public bool IsForgesMiniSpells()
        {
            return this.m_forgesMiniSpells;
        }

        public int GetAimRotateStep()
        {
            return this.m_aimRotateStep;
        }

        public int GetTurnSpeed()
        {
            return this.m_turnSpeed;
        }

        public bool IsNeedsAim()
        {
            return this.m_needsAim;
        }

        public string GetExportNameBeamStart()
        {
            return this.m_exportNameBeamStart;
        }

        public string GetExportNameBeamEnd()
        {
            return this.m_exportNameBeamEnd;
        }

        public bool GetShareHeroCombatData()
        {
            return this.m_shareHeroCombatData;
        }

        public int GetDieDamageRadius()
        {
            return this.m_dieDamageRadius;
        }

        public LogicEffectData GetDieDamageEffect()
        {
            return this.m_dieDamageEffect;
        }

        public int GetDieDamage(int upgLevel)
        {
            return LogicGamePlayUtil.DPSToSingleHit(this.m_dieDamage[upgLevel], 1000);
        }

        public int GetDieDamageDelay()
        {
            return this.m_dieDamageDelay;
        }

        public int GetRedMul()
        {
            return this.m_redMul;
        }

        public int GetGreenMul()
        {
            return this.m_greenMul;
        }

        public int GetBlueMul()
        {
            return this.m_blueMul;
        }

        public int GetRedAdd()
        {
            return this.m_redAdd;
        }

        public int GetGreenAdd()
        {
            return this.m_greenAdd;
        }

        public int GetBlueAdd()
        {
            return this.m_blueAdd;
        }

        public LogicCharacterData GetDefenceTroopCharacter(int upgLevel)
        {
            return this.m_defenceTroopCharacter[upgLevel];
        }

        public LogicCharacterData GetDefenceTroopCharacter2(int upgLevel)
        {
            return this.m_defenceTroopCharacter2[upgLevel];
        }

        public int GetDefenceTroopCount(int upgLevel)
        {
            return this.m_defenceTroopCount[upgLevel];
        }

        public int GetDefenceTroopLevel(int upgLevel)
        {
            return this.m_defenceTroopLevel[upgLevel];
        }

        public bool IsSelfAsAoeCenter()
        {
            return this.m_selfAsAoeCenter;
        }

        public int GetNewTargetAttackDelay()
        {
            return this.m_newTargetAttackDelay;
        }

        public int GetGearUpLevelRequirement()
        {
            return this.m_gearUpLevelRequirement;
        }

        public int GetProducesUnitsOfType()
        {
            return this.m_producesUnitsOfType;
        }

        public bool IsHeroBarrack()
        {
            return this.m_isHeroBarrack;
        }

        public bool IsRed()
        {
            return this.m_isRed;
        }

        public int GetUnitProduction(int index)
        {
            return this.m_unitProduction[index];
        }

        public int GetStrengthWeight(int index)
        {
            return this.m_strengthWeight[index];
        }

        public int GetDestructionXP(int index)
        {
            return this.m_destructionXP[index];
        }

        public LogicBuildingClassData GetSecondaryTargetingClass()
        {
            return this.m_secondaryTargetingClass;
        }

        public LogicBuildingClassData GetShopBuildingClass()
        {
            return this.m_shopBuildingClass;
        }
    }
}