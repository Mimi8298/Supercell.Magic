namespace Supercell.Magic.Titan.Json
{
    using System.Text;

    public class LogicJSONNumber : LogicJSONNode
    {
        private int m_value;

        public LogicJSONNumber()
        {
            this.m_value = 0;
        }

        public LogicJSONNumber(int value)
        {
            this.m_value = value;
        }

        public int GetIntValue()
        {
            return this.m_value;
        }

        public void SetIntValue(int value)
        {
            this.m_value = value;
        }

        public override LogicJSONNodeType GetJSONNodeType()
        {
            return LogicJSONNodeType.NUMBER;
        }

        public override void WriteToString(StringBuilder builder)
        {
            builder.Append(this.m_value);
        }
    }
}