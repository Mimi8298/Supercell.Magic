namespace Supercell.Magic.Titan.Json
{
    using System.Text;

    public class LogicJSONNull : LogicJSONNode
    {
        public override LogicJSONNodeType GetJSONNodeType()
        {
            return LogicJSONNodeType.NULL;
        }

        public override void WriteToString(StringBuilder builder)
        {
            builder.Append("null");
        }
    }
}