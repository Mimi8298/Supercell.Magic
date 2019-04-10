namespace Supercell.Magic.Titan.Json
{
    using System.Text;

    public class LogicJSONString : LogicJSONNode
    {
        private string m_value;

        public LogicJSONString(string value)
        {
            this.m_value = value;
        }

        public string GetStringValue()
        {
            return this.m_value;
        }

        public void SetStringValue(string value)
        {
            this.m_value = value;
        }

        public override LogicJSONNodeType GetJSONNodeType()
        {
            return LogicJSONNodeType.STRING;
        }

        public override void WriteToString(StringBuilder builder)
        {
            LogicJSONParser.WriteString(this.m_value, builder);
        }
    }
}