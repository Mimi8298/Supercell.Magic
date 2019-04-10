namespace Supercell.Magic.Titan.Json
{
    using System.Text;

    public class LogicJSONBoolean : LogicJSONNode
    {
        private readonly bool m_value;

        public LogicJSONBoolean(bool value)
        {
            this.m_value = value;
        }

        public bool IsTrue()
        {
            return this.m_value;
        }

        public override LogicJSONNodeType GetJSONNodeType()
        {
            return LogicJSONNodeType.BOOLEAN;
        }

        public override void WriteToString(StringBuilder builder)
        {
            builder.Append(this.m_value ? "true" : "false");
        }
    }
}