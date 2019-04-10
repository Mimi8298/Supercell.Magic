namespace Supercell.Magic.Logic.Data
{
    public class LogicDataTableResource
    {
        private string m_fileName;

        private LogicDataType m_tableIndex;
        private int m_type;

        public LogicDataTableResource(string fileName, LogicDataType tableIndex, int type)
        {
            this.m_fileName = fileName;
            this.m_tableIndex = tableIndex;
            this.m_type = type;
        }

        public void Destruct()
        {
            this.m_fileName = null;
            this.m_tableIndex = 0;
            this.m_type = 0;
        }

        public string GetFileName()
        {
            return this.m_fileName;
        }

        public LogicDataType GetTableIndex()
        {
            return this.m_tableIndex;
        }

        public int GetTableType()
        {
            return this.m_type;
        }
    }
}