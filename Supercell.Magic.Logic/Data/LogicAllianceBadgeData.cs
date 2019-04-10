namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicAllianceBadgeData : LogicData
    {
        private string m_iconLayer0;
        private string m_iconLayer1;
        private string m_iconLayer2;

        public LogicAllianceBadgeData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicAllianceBadgeData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_iconLayer0 = this.GetValue("IconLayer0", 0);
            this.m_iconLayer1 = this.GetValue("IconLayer1", 0);
            this.m_iconLayer2 = this.GetValue("IconLayer2", 0);
        }
    }
}