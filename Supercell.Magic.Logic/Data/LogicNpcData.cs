namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Util;

    public class LogicNpcData : LogicData
    {
        private string m_mapInstanceName;
        private string m_levelFile;
        private string m_playerName;
        private string m_allianceName;

        private int m_expLevel;
        private int m_goldCount;
        private int m_elixirCount;
        private int m_allianceBadge;

        private bool m_alwaysUnlocked;
        private bool m_singlePlayer;

        private readonly LogicArrayList<LogicNpcData> m_dependencies;
        private readonly LogicArrayList<LogicDataSlot> m_unitCount;

        public LogicNpcData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            this.m_dependencies = new LogicArrayList<LogicNpcData>();
            this.m_unitCount = new LogicArrayList<LogicDataSlot>();
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_mapInstanceName = this.GetValue("MapInstanceName", 0);
            this.m_expLevel = this.GetIntegerValue("ExpLevel", 0);
            this.m_levelFile = this.GetValue("LevelFile", 0);
            this.m_goldCount = this.GetIntegerValue("Gold", 0);
            this.m_elixirCount = this.GetIntegerValue("Elixir", 0);
            this.m_alwaysUnlocked = this.GetBooleanValue("AlwaysUnlocked", 0);
            this.m_playerName = this.GetValue("PlayerName", 0);
            this.m_allianceName = this.GetValue("AllianceName", 0);
            this.m_allianceBadge = this.GetIntegerValue("AllianceBadge", 0);
            this.m_singlePlayer = this.GetBooleanValue("SinglePlayer", 0);

            int unitCountSize = this.GetArraySize("UnitType");

            if (unitCountSize > 0)
            {
                this.m_unitCount.EnsureCapacity(unitCountSize);

                for (int i = 0; i < unitCountSize; i++)
                {
                    int count = this.GetIntegerValue("UnitCount", i);

                    if (count > 0)
                    {
                        this.m_unitCount.Add(new LogicDataSlot(LogicDataTables.GetCharacterByName(this.GetValue("UnitType", i), this), count));
                    }
                }
            }

            int mapDependencySize = this.GetArraySize("MapDependencies");

            for (int i = 0; i < mapDependencySize; i++)
            {
                LogicNpcData data = LogicDataTables.GetNpcByName(this.GetValue("MapDependencies", i), this);

                if (data != null)
                {
                    this.m_dependencies.Add(data);
                }
            }
        }

        public LogicArrayList<LogicDataSlot> GetClonedUnits()
        {
            LogicArrayList<LogicDataSlot> units = new LogicArrayList<LogicDataSlot>();

            for (int i = 0; i < this.m_unitCount.Size(); i++)
            {
                units.Add(this.m_unitCount[i].Clone());
            }

            return units;
        }


        public bool IsUnlockedInMap(LogicClientAvatar avatar)
        {
            if (!this.m_alwaysUnlocked)
            {
                if (!string.IsNullOrEmpty(this.m_mapInstanceName))
                {
                    if (this.m_dependencies != null)
                    {
                        for (int i = 0; i < this.m_dependencies.Size(); i++)
                        {
                            if (avatar.GetNpcStars(this.m_dependencies[i]) > 0)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            return true;
        }

        public string GetMapInstanceName()
        {
            return this.m_mapInstanceName;
        }

        public int GetExpLevel()
        {
            return this.m_expLevel;
        }

        public string GetLevelFile()
        {
            return this.m_levelFile;
        }

        public int GetGoldCount()
        {
            return this.m_goldCount;
        }

        public int GetElixirCount()
        {
            return this.m_elixirCount;
        }

        public bool IsAlwaysUnlocked()
        {
            return this.m_alwaysUnlocked;
        }

        public string GetPlayerName()
        {
            return this.m_playerName;
        }

        public string GetAllianceName()
        {
            return this.m_allianceName;
        }

        public int GetAllianceBadge()
        {
            return this.m_allianceBadge;
        }

        public bool IsSinglePlayer()
        {
            return this.m_singlePlayer;
        }
    }
}