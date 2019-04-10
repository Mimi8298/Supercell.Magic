namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicDecoData : LogicGameObjectData
    {
        private int m_width;
        private int m_height;
        private int m_buildCost;
        private int m_maxCount;
        private int m_requiredExpLevel;

        private bool m_inShop;
        private bool m_passable;

        private LogicResourceData m_buildResourceData;

        public LogicDecoData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicDecoData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_width = this.GetIntegerValue("Width", 0);
            this.m_height = this.GetIntegerValue("Height", 0);
            this.m_maxCount = this.GetIntegerValue("MaxCount", 0);
            this.m_inShop = !this.GetBooleanValue("NotInShop", 0);
            this.m_buildCost = this.GetIntegerValue("BuildCost", 0);
            this.m_requiredExpLevel = this.GetIntegerValue("RequiredExpLevel", 0);
            this.m_passable = this.GetBooleanValue("DecoPath", 0);

            this.m_buildResourceData = LogicDataTables.GetResourceByName(this.GetValue("BuildResource", 0), this);
        }

        public bool IsInShop()
        {
            return this.m_inShop;
        }

        public int GetMaxCount()
        {
            return this.m_maxCount;
        }

        public int GetRequiredExpLevel()
        {
            return this.m_requiredExpLevel;
        }

        public int GetSellPrice()
        {
            return this.m_buildCost / 10;
        }

        public int GetBuildCost()
        {
            return this.m_buildCost;
        }

        public int GetWidth()
        {
            return this.m_width;
        }

        public int GetHeight()
        {
            return this.m_height;
        }

        public LogicResourceData GetBuildResource()
        {
            return this.m_buildResourceData;
        }

        public bool IsPassable()
        {
            return this.m_passable;
        }
    }
}