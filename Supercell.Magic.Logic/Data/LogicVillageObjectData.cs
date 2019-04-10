namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicVillageObjectData : LogicGameObjectData
    {
        private int m_upgradeLevelCount;
        private int m_tileX100;
        private int m_tileY100;
        private int m_requiredTH;
        private int m_animX;
        private int m_animY;
        private int m_animID;
        private int m_animDir;
        private int m_animVisibilityOdds;
        private int m_unitHousing;

        private int[] m_buildTime;
        private int[] m_buildCost;
        private int[] m_requiredTownHallLevel;

        private bool m_shipyard;
        private bool m_rowBoat;
        private bool m_clanGate;
        private bool m_disabled;
        private bool m_automaticUpgrades;
        private bool m_requiresBuilder;
        private bool m_hasInfoScreen;
        private bool m_housesUnits;

        private LogicResourceData m_buildResourceData;
        private LogicEffectData m_pickUpEffect;

        public LogicVillageObjectData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicVillageObjectData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_disabled = this.GetBooleanValue("Disabled", 0);
            this.m_tileX100 = this.GetIntegerValue("TileX100", 0);
            this.m_tileY100 = this.GetIntegerValue("TileY100", 0);
            this.m_requiredTH = this.GetIntegerValue("RequiredTH", 0);
            this.m_automaticUpgrades = this.GetBooleanValue("AutomaticUpgrades", 0);
            this.m_requiresBuilder = this.GetBooleanValue("RequiresBuilder", 0);
            this.m_pickUpEffect = LogicDataTables.GetEffectByName(this.GetValue("PickUpEffect", 0), this);
            this.m_animX = this.GetIntegerValue("AnimX", 0);
            this.m_animY = this.GetIntegerValue("AnimY", 0);
            this.m_animID = this.GetIntegerValue("AnimID", 0);
            this.m_animDir = this.GetIntegerValue("AnimDir", 0);
            this.m_animVisibilityOdds = this.GetIntegerValue("AnimVisibilityOdds", 0);
            this.m_hasInfoScreen = this.GetBooleanValue("HasInfoScreen", 0);
            this.m_unitHousing = this.GetIntegerValue("UnitHousing", 0);
            this.m_housesUnits = this.GetBooleanValue("HousesUnits", 0);

            this.m_shipyard = string.Equals("Shipyard", this.GetName());

            if (!this.m_shipyard)
            {
                this.m_shipyard = string.Equals("Shipyard2", this.GetName());
            }

            this.m_rowBoat = string.Equals("Rowboat", this.GetName());

            if (!this.m_rowBoat)
            {
                this.m_rowBoat = string.Equals("Rowboat2", this.GetName());
            }

            this.m_clanGate = string.Equals("ClanGate", this.GetName());

            this.m_upgradeLevelCount = this.m_row.GetBiggestArraySize();
            this.m_buildCost = new int[this.m_row.GetBiggestArraySize()];
            this.m_buildTime = new int[this.m_row.GetBiggestArraySize()];
            this.m_requiredTownHallLevel = new int[this.m_row.GetBiggestArraySize()];

            for (int i = 0; i < this.m_upgradeLevelCount; i++)
            {
                this.m_requiredTownHallLevel[i] = this.GetClampedIntegerValue("RequiredTownHall", i);
                this.m_buildCost[i] = this.GetClampedIntegerValue("BuildCost", i);
                this.m_buildTime[i] = 86400 * this.GetClampedIntegerValue("BuildTimeD", i) +
                                      3600 * this.GetClampedIntegerValue("BuildTimeH", i) +
                                      60 * this.GetClampedIntegerValue("BuildTimeM", i) +
                                      this.GetClampedIntegerValue("BuildTimeS", i);
            }

            this.m_buildResourceData = LogicDataTables.GetResourceByName(this.GetValue("BuildResource", 0), this);
        }

        public bool IsShipyard()
        {
            return this.m_shipyard;
        }

        public bool IsRowBoat()
        {
            return this.m_rowBoat;
        }

        public bool IsClanGate()
        {
            return this.m_clanGate;
        }

        public int GetBuildTime(int index)
        {
            return this.m_buildTime[index];
        }

        public int GetBuildCost(int index)
        {
            return this.m_buildCost[index];
        }

        public int GetRequiredTownHallLevel(int index)
        {
            return this.m_requiredTownHallLevel[index];
        }

        public int GetUpgradeLevelCount()
        {
            return this.m_upgradeLevelCount;
        }

        public LogicResourceData GetBuildResource()
        {
            return this.m_buildResourceData;
        }

        public bool IsDisabled()
        {
            return this.m_disabled;
        }

        public int GetTileX100()
        {
            return this.m_tileX100;
        }

        public int GetTileY100()
        {
            return this.m_tileY100;
        }

        public int GetRequiredTH()
        {
            return this.m_requiredTH;
        }

        public bool IsAutomaticUpgrades()
        {
            return this.m_automaticUpgrades;
        }

        public bool IsRequiresBuilder()
        {
            return this.m_requiresBuilder;
        }

        public LogicEffectData GetPickUpEffect()
        {
            return this.m_pickUpEffect;
        }

        public int GetAnimX()
        {
            return this.m_animX;
        }

        public int GetAnimY()
        {
            return this.m_animY;
        }

        public int GetAnimID()
        {
            return this.m_animID;
        }

        public int GetAnimDir()
        {
            return this.m_animDir;
        }

        public int GetAnimVisibilityOdds()
        {
            return this.m_animVisibilityOdds;
        }

        public bool IsHasInfoScreen()
        {
            return this.m_hasInfoScreen;
        }

        public int GetUnitHousing()
        {
            return this.m_unitHousing;
        }

        public bool IsHousesUnits()
        {
            return this.m_housesUnits;
        }
    }
}