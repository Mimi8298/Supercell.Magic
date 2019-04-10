namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Debug;

    public class LogicObstacleData : LogicGameObjectData
    {
        private LogicResourceData m_clearResourceData;
        private LogicResourceData m_lootResourceData;
        private LogicEffectData m_clearEffect;
        private LogicEffectData m_pickUpEffect;
        private LogicObstacleData m_spawnObstacle;

        private string m_exportName;
        private string m_exportNameBase;
        private string m_highlightExportName;

        private bool m_passable;
        private bool m_isTombstone;
        private bool m_lightsOn;
        private bool m_tallGrass;
        private bool m_tallGrassSpawnPoint;

        private int m_lootCount;
        private int m_clearCost;
        private int m_clearTimeSecs;
        private int m_respawnWeight;
        private int m_lootMultiplierVersion2;
        private int m_width;
        private int m_height;
        private int m_tombGroup;
        private int m_appearancePeriodHours;
        private int m_minRespawnTimeHours;
        private int m_spawnRadius;
        private int m_spawnIntervalSeconds;
        private int m_spawnCount;
        private int m_maxSpawned;
        private int m_maxLifetimeSpawns;
        private int m_lootDefensePercentage;
        private int m_redMul;
        private int m_greenMul;
        private int m_blueMul;
        private int m_redAdd;
        private int m_greenAdd;
        private int m_blueAdd;
        private int m_village2RespawnCount;
        private int m_variationCount;
        private int m_lootHighlightPercentage;

        public LogicObstacleData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicObstacleData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_exportName = this.GetValue("ExportName", 0);
            this.m_exportNameBase = this.GetValue("ExportNameBase", 0);
            this.m_width = this.GetIntegerValue("Width", 0);
            this.m_height = this.GetIntegerValue("Height", 0);
            this.m_passable = this.GetBooleanValue("Passable", 0);
            this.m_clearEffect = LogicDataTables.GetEffectByName(this.GetValue("ClearEffect", 0), this);
            this.m_pickUpEffect = LogicDataTables.GetEffectByName(this.GetValue("PickUpEffect", 0), this);
            this.m_isTombstone = this.GetBooleanValue("IsTombstone", 0);
            this.m_tombGroup = this.GetIntegerValue("TombGroup", 0);
            this.m_appearancePeriodHours = this.GetIntegerValue("AppearancePeriodHours", 0);
            this.m_minRespawnTimeHours = this.GetIntegerValue("MinRespawnTimeHours", 0);
            this.m_lootDefensePercentage = this.GetIntegerValue("LootDefensePercentage", 0);
            this.m_redMul = this.GetIntegerValue("RedMul", 0);
            this.m_greenMul = this.GetIntegerValue("GreenMul", 0);
            this.m_blueMul = this.GetIntegerValue("BlueMul", 0);
            this.m_redAdd = this.GetIntegerValue("RedAdd", 0);
            this.m_greenAdd = this.GetIntegerValue("GreenAdd", 0);
            this.m_blueAdd = this.GetIntegerValue("BlueAdd", 0);
            this.m_lightsOn = this.GetBooleanValue("LightsOn", 0);
            this.m_village2RespawnCount = this.GetIntegerValue("Village2RespawnCount", 0);
            this.m_variationCount = this.GetIntegerValue("VariationCount", 0);
            this.m_tallGrass = this.GetBooleanValue("TallGrass", 0);
            this.m_tallGrassSpawnPoint = this.GetBooleanValue("TallGrassSpawnPoint", 0);
            this.m_lootHighlightPercentage = this.GetIntegerValue("LootHighlightPercentage", 0);
            this.m_highlightExportName = this.GetValue("HighlightExportName", 0);

            this.m_clearResourceData = LogicDataTables.GetResourceByName(this.GetValue("ClearResource", 0), this);

            if (this.m_clearResourceData == null)
            {
                Debugger.Error("Clear resource is not defined for obstacle: " + this.GetName());
            }

            this.m_clearCost = this.GetIntegerValue("ClearCost", 0);
            this.m_clearTimeSecs = this.GetIntegerValue("ClearTimeSeconds", 0);
            this.m_respawnWeight = this.GetIntegerValue("RespawnWeight", 0);

            string lootResourceName = this.GetValue("LootResource", 0);

            if (lootResourceName.Length <= 0)
            {
                this.m_respawnWeight = 0;
            }
            else
            {
                this.m_lootResourceData = LogicDataTables.GetResourceByName(lootResourceName, this);
                this.m_lootCount = this.GetIntegerValue("LootCount", 0);
            }

            this.m_lootMultiplierVersion2 = this.GetIntegerValue("LootMultiplierForVersion2", 0);

            if (this.m_lootMultiplierVersion2 == 0)
            {
                this.m_lootMultiplierVersion2 = 1;
            }

            string spawnObstacle = this.GetValue("SpawnObstacle", 0);

            if (spawnObstacle.Length > 0)
            {
                this.m_spawnObstacle = LogicDataTables.GetObstacleByName(spawnObstacle, this);
                this.m_spawnRadius = this.GetIntegerValue("SpawnRadius", 0);
                this.m_spawnIntervalSeconds = this.GetIntegerValue("SpawnIntervalSeconds", 0);
                this.m_spawnCount = this.GetIntegerValue("SpawnCount", 0);
                this.m_maxSpawned = this.GetIntegerValue("MaxSpawned", 0);
                this.m_maxLifetimeSpawns = this.GetIntegerValue("MaxLifetimeSpawns", 0);
            }
        }

        public override void CreateReferences2()
        {
            if (this.m_lootResourceData != null)
            {
                if (this.m_lootResourceData.GetVillageType() != this.GetVillageType() && !this.m_lootResourceData.IsPremiumCurrency())
                {
                    Debugger.Error("invalid resource");
                }
            }

            if (this.m_clearResourceData.GetVillageType() != this.GetVillageType() && !this.m_clearResourceData.IsPremiumCurrency())
            {
                Debugger.Error("invalid clear resource");
            }
        }

        public int GetRespawnWeight()
        {
            return this.m_respawnWeight;
        }

        public int GetClearTime()
        {
            return this.m_clearTimeSecs;
        }

        public LogicResourceData GetClearResourceData()
        {
            return this.m_clearResourceData;
        }

        public LogicResourceData GetLootResourceData()
        {
            return this.m_lootResourceData;
        }

        public int GetLootCount()
        {
            return this.m_lootCount;
        }

        public int GetClearCost()
        {
            return this.m_clearCost;
        }

        public int GetLootMultiplierVersion2()
        {
            return this.m_lootMultiplierVersion2;
        }

        public bool IsLootCart()
        {
            return this.m_lootDefensePercentage > 0;
        }

        public string GetExportName()
        {
            return this.m_exportName;
        }

        public string GetExportNameBase()
        {
            return this.m_exportNameBase;
        }

        public int GetWidth()
        {
            return this.m_width;
        }

        public int GetHeight()
        {
            return this.m_height;
        }

        public bool IsPassable()
        {
            return this.m_passable;
        }

        public int GetTombGroup()
        {
            return this.m_tombGroup;
        }

        public LogicEffectData GetClearEffect()
        {
            return this.m_clearEffect;
        }

        public LogicEffectData GetPickUpEffect()
        {
            return this.m_pickUpEffect;
        }

        public LogicObstacleData GetSpawnObstacle()
        {
            return this.m_spawnObstacle;
        }

        public bool IsTombstone()
        {
            return this.m_isTombstone;
        }

        public int GetAppearancePeriodHours()
        {
            return this.m_appearancePeriodHours;
        }

        public int GetMinRespawnTimeHours()
        {
            return this.m_minRespawnTimeHours;
        }

        public int GetSpawnRadius()
        {
            return this.m_spawnRadius;
        }

        public int GetSpawnIntervalSeconds()
        {
            return this.m_spawnIntervalSeconds;
        }

        public int GetSpawnCount()
        {
            return this.m_spawnCount;
        }

        public int GetMaxSpawned()
        {
            return this.m_maxSpawned;
        }

        public int GetMaxLifetimeSpawns()
        {
            return this.m_maxLifetimeSpawns;
        }

        public int GetLootDefensePercentage()
        {
            return this.m_lootDefensePercentage;
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

        public bool IsLightsOn()
        {
            return this.m_lightsOn;
        }

        public int GetVillage2RespawnCount()
        {
            return this.m_village2RespawnCount;
        }

        public int GetVariationCount()
        {
            return this.m_variationCount;
        }

        public bool IsTallGrass()
        {
            return this.m_tallGrass;
        }

        public bool IsTallGrassSpawnPoint()
        {
            return this.m_tallGrassSpawnPoint;
        }

        public int GetLootHighlightPercentage()
        {
            return this.m_lootHighlightPercentage;
        }

        public string GetHighlightExportName()
        {
            return this.m_highlightExportName;
        }
    }
}