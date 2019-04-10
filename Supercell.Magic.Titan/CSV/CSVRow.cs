namespace Supercell.Magic.Titan.CSV
{
    using Supercell.Magic.Titan.Math;

    public class CSVRow
    {
        private readonly int m_rowOffset;
        private readonly CSVTable m_table;

        public CSVRow(CSVTable table)
        {
            this.m_table = table;
            this.m_rowOffset = table.GetColumnRowCount();
        }

        public int GetArraySize(string column)
        {
            int columnIndex = this.GetColumnIndexByName(column);

            if (columnIndex == -1)
            {
                return 0;
            }

            return this.m_table.GetArraySizeAt(this, columnIndex);
        }

        public int GetBiggestArraySize()
        {
            int columnCount = this.m_table.GetColumnCount();
            int maxSize = 1;

            for (int i = columnCount - 1; i >= 0; i--)
            {
                maxSize = LogicMath.Max(this.m_table.GetArraySizeAt(this, i), maxSize);
            }

            return maxSize;
        }

        public int GetColumnCount()
        {
            return this.m_table.GetColumnCount();
        }

        public int GetColumnIndexByName(string name)
        {
            return this.m_table.GetColumnIndexByName(name);
        }

        public bool GetBooleanValue(string columnName, int index)
        {
            return this.m_table.GetBooleanValue(columnName, this.m_rowOffset + index);
        }

        public bool GetBooleanValueAt(int columnIndex, int index)
        {
            return this.m_table.GetBooleanValueAt(columnIndex, this.m_rowOffset + index);
        }

        public bool GetClampedBooleanValue(string columnName, int index)
        {
            int columnIndex = this.GetColumnIndexByName(columnName);

            if (columnIndex != -1)
            {
                int arraySize = this.m_table.GetArraySizeAt(this, columnIndex);

                if (index >= arraySize || arraySize < 1)
                {
                    index = LogicMath.Max(arraySize - 1, 0);
                }

                return this.m_table.GetBooleanValueAt(columnIndex, this.m_rowOffset + index);
            }

            return false;
        }

        public int GetIntegerValue(string columnName, int index)
        {
            return this.m_table.GetIntegerValue(columnName, this.m_rowOffset + index);
        }

        public int GetIntegerValueAt(int columnIndex, int index)
        {
            return this.m_table.GetIntegerValueAt(columnIndex, this.m_rowOffset + index);
        }

        public int GetClampedIntegerValue(string columnName, int index)
        {
            int columnIndex = this.GetColumnIndexByName(columnName);

            if (columnIndex != -1)
            {
                int arraySize = this.m_table.GetArraySizeAt(this, columnIndex);

                if (index >= arraySize || arraySize < 1)
                {
                    index = LogicMath.Max(arraySize - 1, 0);
                }

                return this.m_table.GetIntegerValueAt(columnIndex, this.m_rowOffset + index);
            }

            return 0;
        }

        public string GetValue(string columnName, int index)
        {
            return this.m_table.GetValue(columnName, this.m_rowOffset + index);
        }

        public string GetValueAt(int columnIndex, int index)
        {
            return this.m_table.GetValueAt(columnIndex, this.m_rowOffset + index);
        }

        public string GetClampedValue(string columnName, int index)
        {
            int columnIndex = this.GetColumnIndexByName(columnName);

            if (columnIndex != -1)
            {
                int arraySize = this.m_table.GetArraySizeAt(this, columnIndex);

                if (index >= arraySize || arraySize < 1)
                {
                    index = LogicMath.Max(arraySize - 1, 0);
                }

                return this.m_table.GetValueAt(columnIndex, this.m_rowOffset + index);
            }

            return string.Empty;
        }

        public string GetName()
        {
            return this.m_table.GetValueAt(0, this.m_rowOffset);
        }

        public int GetRowOffset()
        {
            return this.m_rowOffset;
        }
    }
}