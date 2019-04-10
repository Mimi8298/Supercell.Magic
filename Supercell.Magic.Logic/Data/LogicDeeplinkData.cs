namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicDeeplinkData : LogicData
    {
        private string[] m_parameterType;
        private string[] m_parameterName;
        private string[] m_description;

        public LogicDeeplinkData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicDeeplinkData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            int size = this.GetArraySize("ParameterType");

            this.m_parameterType = new string[size];
            this.m_parameterName = new string[size];
            this.m_description = new string[size];

            for (int i = 0; i < size; i++)
            {
                this.m_parameterType[i] = this.GetValue("ParameterType", i);
                this.m_parameterName[i] = this.GetValue("ParameterName", i);
                this.m_description[i] = this.GetValue("Description", i);
            }
        }

        public string GetParameterType(int index)
        {
            return this.m_parameterType[index];
        }

        public string GetParameterName(int index)
        {
            return this.m_parameterName[index];
        }

        public string GetDescription(int index)
        {
            return this.m_description[index];
        }
    }
}