namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicAlliancePortalData : LogicGameObjectData
    {
        private string m_swf;
        private string m_exportName;

        private int m_width;
        private int m_height;

        public LogicAlliancePortalData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicAlliancePortalData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_swf = this.GetValue("SWF", 0);
            this.m_exportName = this.GetValue("ExportName", 0);
            this.m_width = this.GetIntegerValue("Width", 0);
            this.m_height = this.GetIntegerValue("Height", 0);
        }

        public int GetWidth()
        {
            return this.m_width;
        }

        public int GetHeight()
        {
            return this.m_height;
        }
    }
}