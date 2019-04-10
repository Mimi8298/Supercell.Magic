namespace Supercell.Magic.Titan.CSV
{
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Util;

    public class CSVTable
    {
        private readonly LogicArrayList<string> m_columnNameList;
        private readonly LogicArrayList<CSVColumn> m_columnList;
        private readonly LogicArrayList<CSVRow> m_rowList;

        private readonly CSVNode m_node;

        private readonly int m_size;

        public CSVTable(CSVNode node, int size)
        {
            this.m_columnNameList = new LogicArrayList<string>();
            this.m_columnList = new LogicArrayList<CSVColumn>();
            this.m_rowList = new LogicArrayList<CSVRow>();

            this.m_node = node;
            this.m_size = size;
        }

        public void AddAndConvertValue(string value, int columnIndex)
        {
            CSVColumn column = this.m_columnList[columnIndex];

            if (!string.IsNullOrEmpty(value))
            {
                switch (column.GetColumnType())
                {
                    case -1:
                    case 0:
                        column.AddStringValue(value);
                        break;
                    case 1:
                        column.AddIntegerValue(int.Parse(value));
                        break;
                    case 2:
                        if (bool.TryParse(value, out bool booleanValue))
                        {
                            column.AddBooleanValue(booleanValue);
                        }
                        else
                        {
                            Debugger.Warning(string.Format("CSVTable::addAndConvertValue invalid value '{0}' in Boolean column '{1}', {2}", value,
                                                           this.m_columnNameList[columnIndex], this.GetFileName()));
                            column.AddBooleanValue(false);
                        }

                        break;
                }
            }
            else
            {
                column.AddEmptyValue();
            }
        }

        public void AddColumn(string name)
        {
            this.m_columnNameList.Add(name);
        }

        public void AddColumnType(int type)
        {
            this.m_columnList.Add(new CSVColumn(type, this.m_size));
        }

        public void AddRow(CSVRow row)
        {
            this.m_rowList.Add(row);
        }

        public void ColumnNamesLoaded()
        {
            this.m_columnList.EnsureCapacity(this.m_columnNameList.Size());
        }

        public void CreateRow()
        {
            this.m_rowList.Add(new CSVRow(this));
        }

        public int GetArraySizeAt(CSVRow row, int columnIdx)
        {
            if (this.m_rowList.Size() > 0)
            {
                int rowIdx = this.m_rowList.IndexOf(row);

                if (rowIdx != -1)
                {
                    CSVColumn column = this.m_columnList[columnIdx];
                    return column.GetArraySize(this.m_rowList[rowIdx].GetRowOffset(),
                                               rowIdx + 1 >= this.m_rowList.Size() ? column.GetSize() : this.m_rowList[rowIdx + 1].GetRowOffset());
                }
            }

            return 0;
        }

        public string GetColumnName(int idx)
        {
            return this.m_columnNameList[idx];
        }

        public int GetColumnIndexByName(string name)
        {
            return this.m_columnNameList.IndexOf(name);
        }

        public int GetColumnCount()
        {
            return this.m_columnNameList.Size();
        }

        public int GetColumnRowCount()
        {
            return this.m_columnList[0].GetSize();
        }

        public int GetColumnTypeCount()
        {
            return this.m_columnList.Size();
        }

        public string GetFileName()
        {
            return this.m_node.GetFileName();
        }

        public bool GetBooleanValue(string name, int index)
        {
            return this.GetBooleanValueAt(this.m_columnNameList.IndexOf(name), index);
        }

        public bool GetBooleanValueAt(int columnIndex, int index)
        {
            if (columnIndex != -1)
            {
                return this.m_columnList[columnIndex].GetBooleanValue(index);
            }

            return false;
        }

        public int GetIntegerValue(string name, int index)
        {
            return this.GetIntegerValueAt(this.m_columnNameList.IndexOf(name), index);
        }

        public int GetIntegerValueAt(int columnIndex, int index)
        {
            if (columnIndex != -1)
            {
                int value = this.m_columnList[columnIndex].GetIntegerValue(index);

                if (value == 0x7fffffff)
                {
                    value = 0;
                }

                return value;
            }

            return 0;
        }

        public string GetValue(string name, int index)
        {
            return this.GetValueAt(this.m_columnNameList.IndexOf(name), index);
        }

        public string GetValueAt(int columnIndex, int index)
        {
            if (columnIndex != -1)
            {
                return this.m_columnList[columnIndex].GetStringValue(index);
            }

            return string.Empty;
        }

        public CSVRow GetRowAt(int index)
        {
            return this.m_rowList[index];
        }

        public CSVColumn GetCSVColumn(int index)
        {
            return this.m_columnList[index];
        }

        public int GetRowCount()
        {
            return this.m_rowList.Size();
        }

        public void ValidateColumnTypes()
        {
            if (this.m_columnNameList.Size() != this.m_columnList.Size())
            {
                Debugger.Warning($"Column name count {this.m_columnNameList.Size()}, column type count {this.m_columnList.Size()}, file {this.GetFileName()}");
            }
        }
    }
}