namespace Supercell.Magic.Titan.CSV
{
    using System;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Util;

    public class CSVNode
    {
        private readonly string m_fileName;
        private CSVTable m_table;

        public CSVNode(string[] lines, string fileName)
        {
            this.m_fileName = fileName;
            this.Load(lines);
        }

        public void Load(string[] lines)
        {
            this.m_table = new CSVTable(this, lines.Length);

            if (lines.Length >= 2)
            {
                LogicArrayList<string> columnNames = this.ParseLine(lines[0]);
                LogicArrayList<string> columnTypes = this.ParseLine(lines[1]);

                for (int i = 0; i < columnNames.Size(); i++)
                {
                    this.m_table.AddColumn(columnNames[i]);
                }

                for (int i = 0; i < columnTypes.Size(); i++)
                {
                    string type = columnTypes[i];
                    int columnType = -1;

                    if (!string.IsNullOrEmpty(type))
                    {
                        if (string.Equals(type, "string", StringComparison.OrdinalIgnoreCase))
                        {
                            columnType = 0;
                        }
                        else if (string.Equals(type, "int", StringComparison.OrdinalIgnoreCase))
                        {
                            columnType = 1;
                        }
                        else if (string.Equals(type, "boolean", StringComparison.OrdinalIgnoreCase))
                        {
                            columnType = 2;
                        }
                        else
                        {
                            Debugger.Error(string.Format("Invalid column type '{0}', column name {1}, file {2}. Expecting: int/string/boolean.", columnTypes[i], columnNames[i], this.m_fileName));
                        }
                    }

                    this.m_table.AddColumnType(columnType);
                }

                this.m_table.ValidateColumnTypes();

                if (lines.Length > 2)
                {
                    for (int i = 2; i < lines.Length; i++)
                    {
                        LogicArrayList<string> values = this.ParseLine(lines[i]);

                        if (values.Size() > 0)
                        {
                            if (!string.IsNullOrEmpty(values[0]))
                            {
                                this.m_table.CreateRow();
                            }

                            for (int j = 0; j < values.Size(); j++)
                            {
                                this.m_table.AddAndConvertValue(values[j], j);
                            }
                        }
                    }
                }
            }
        }

        public LogicArrayList<string> ParseLine(string line)
        {
            bool inQuote = false;
            string readField = string.Empty;

            LogicArrayList<string> fields = new LogicArrayList<string>();

            for (int i = 0; i < line.Length; i++)
            {
                char currentChar = line[i];

                if (currentChar == '"')
                {
                    if (inQuote)
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            readField += currentChar;
                        }
                        else
                        {
                            inQuote = false;
                        }
                    }
                    else
                    {
                        inQuote = true;
                    }
                }
                else if (currentChar == ',' && !inQuote)
                {
                    fields.Add(readField);
                    readField = string.Empty;
                }
                else
                {
                    readField += currentChar;
                }
            }

            fields.Add(readField);

            return fields;
        }

        public string GetFileName()
        {
            return this.m_fileName;
        }

        public CSVTable GetTable()
        {
            return this.m_table;
        }
    }
}