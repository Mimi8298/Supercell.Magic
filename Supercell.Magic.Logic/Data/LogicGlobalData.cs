namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicGlobalData : LogicData
    {
        private int m_numberValue;
        private bool m_booleanValue;
        private string m_textValue;

        private int[] m_numberArray;
        private int[] m_altNumberArray;

        private string[] m_stringArray;

        public LogicGlobalData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicGlobalData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            int size = this.m_row.GetBiggestArraySize();

            this.m_numberArray = new int[size];
            this.m_altNumberArray = new int[size];
            this.m_stringArray = new string[size];

            this.m_numberValue = this.GetIntegerValue("NumberValue", 0);
            this.m_booleanValue = this.GetBooleanValue("BooleanValue", 0);
            this.m_textValue = this.GetValue("TextValue", 0);

            for (int i = 0; i < size; i++)
            {
                this.m_numberArray[i] = this.GetIntegerValue("NumberArray", i);
                this.m_altNumberArray[i] = this.GetIntegerValue("AltNumberArray", i);
                this.m_stringArray[i] = this.GetValue("StringArray", i);
            }
        }

        public int GetNumberValue()
        {
            return this.m_numberValue;
        }

        public bool GetBooleanValue()
        {
            return this.m_booleanValue;
        }

        public string GetTextValue()
        {
            return this.m_textValue;
        }

        public int GetNumberArraySize()
        {
            return this.m_numberArray.Length;
        }

        public int GetNumberArray(int index)
        {
            return this.m_numberArray[index];
        }

        public int GetAltNumberArray(int index)
        {
            return this.m_altNumberArray[index];
        }
    }
}