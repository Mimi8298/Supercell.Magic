namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicWarData : LogicData
    {
        private int m_teamSize;
        private int m_preparationMinutes;
        private int m_warMinutes;

        private bool m_disableProduction;

        public LogicWarData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicWarData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_teamSize = this.GetIntegerValue("TeamSize", 0);
            this.m_preparationMinutes = this.GetIntegerValue("PreparationMinutes", 0);
            this.m_warMinutes = this.GetIntegerValue("WarMinutes", 0);
            this.m_disableProduction = this.GetBooleanValue("DisableProduction", 0);
        }

        public int GetTeamSize()
        {
            return this.m_teamSize;
        }

        public int GetPreparationMinutes()
        {
            return this.m_preparationMinutes;
        }

        public int GetWarMinutes()
        {
            return this.m_warMinutes;
        }

        public bool IsDisableProduction()
        {
            return this.m_disableProduction;
        }
    }
}