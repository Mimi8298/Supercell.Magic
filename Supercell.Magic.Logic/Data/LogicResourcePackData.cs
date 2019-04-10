namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicResourcePackData : LogicData
    {
        private LogicResourceData m_resourceData;
        private int m_capacityPercentage;

        public LogicResourcePackData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicResourcePackData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_resourceData = LogicDataTables.GetResourceByName(this.GetValue("Resource", 0), this);
            this.m_capacityPercentage = this.GetIntegerValue("CapacityPercentage", 0);
        }

        public LogicResourceData GetResourceData()
        {
            return this.m_resourceData;
        }

        public int GetCapacityPercentage()
        {
            return this.m_capacityPercentage;
        }
    }
}