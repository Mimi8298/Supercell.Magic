namespace Supercell.Magic.Titan.CSV
{
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Util;

    public class CSVColumn
    {
        public const int BOOLEAN_VALUE_NOT_SET = 0x2;
        public const int INT_VALUE_NOT_SET = 0x7FFFFFFF;

        private readonly LogicArrayList<byte> m_booleanValues;
        private readonly LogicArrayList<int> m_integerValues;
        private readonly LogicArrayList<string> m_stringValues;

        private readonly int m_columnType;

        public CSVColumn(int type, int size)
        {
            this.m_columnType = type;

            this.m_integerValues = new LogicArrayList<int>();
            this.m_booleanValues = new LogicArrayList<byte>();
            this.m_stringValues = new LogicArrayList<string>();

            switch (type)
            {
                case -1:
                case 0:
                    this.m_stringValues.EnsureCapacity(size);
                    break;
                case 1:
                    this.m_integerValues.EnsureCapacity(size);
                    break;
                case 2:
                    this.m_booleanValues.EnsureCapacity(size);
                    break;
                default:
                    Debugger.Error("Invalid CSVColumn type");
                    break;
            }
        }

        public void AddEmptyValue()
        {
            switch (this.m_columnType)
            {
                case -1:
                case 0:
                {
                    this.m_stringValues.Add(string.Empty);
                    break;
                }

                case 1:
                {
                    this.m_integerValues.Add(CSVColumn.INT_VALUE_NOT_SET);
                    break;
                }

                case 2:
                {
                    this.m_booleanValues.Add(CSVColumn.BOOLEAN_VALUE_NOT_SET);
                    break;
                }
            }
        }

        public void AddBooleanValue(bool value)
        {
            this.m_booleanValues.Add((byte) (value ? 1 : 0));
        }

        public void AddIntegerValue(int value)
        {
            this.m_integerValues.Add(value);
        }

        public void AddStringValue(string value)
        {
            this.m_stringValues.Add(value);
        }

        public int GetArraySize(int startOffset, int endOffset)
        {
            switch (this.m_columnType)
            {
                default:
                    for (int i = endOffset - 1; i + 1 > startOffset; i--)
                    {
                        if (this.m_stringValues[i].Length > 0)
                        {
                            return i - startOffset + 1;
                        }
                    }

                    break;
                case 1:
                    for (int i = endOffset - 1; i + 1 > startOffset; i--)
                    {
                        if (this.m_integerValues[i] != CSVColumn.INT_VALUE_NOT_SET)
                        {
                            return i - startOffset + 1;
                        }
                    }

                    break;

                case 2:
                    for (int i = endOffset - 1; i + 1 > startOffset; i--)
                    {
                        if (this.m_booleanValues[i] != CSVColumn.BOOLEAN_VALUE_NOT_SET)
                        {
                            return i - startOffset + 1;
                        }
                    }

                    break;
            }

            return 0;
        }

        public bool GetBooleanValue(int index)
        {
            return this.m_booleanValues[index] == 1;
        }

        public int GetIntegerValue(int index)
        {
            return this.m_integerValues[index];
        }

        public string GetStringValue(int index)
        {
            return this.m_stringValues[index];
        }

        public int GetSize()
        {
            switch (this.m_columnType)
            {
                case -1:
                case 0:
                    return this.m_stringValues.Size();
                case 1:
                    return this.m_integerValues.Size();
                case 2:
                    return this.m_booleanValues.Size();
                default:
                    return 0;
            }
        }

        public int GetColumnType()
        {
            return this.m_columnType;
        }
    }
}