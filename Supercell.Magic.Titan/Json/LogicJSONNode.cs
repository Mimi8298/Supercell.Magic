namespace Supercell.Magic.Titan.Json
{
    using System.Text;

    public abstract class LogicJSONNode
    {
        public abstract LogicJSONNodeType GetJSONNodeType();
        public abstract void WriteToString(StringBuilder builder);
    }

    public enum LogicJSONNodeType
    {
        ARRAY = 1,
        OBJECT,
        NUMBER,
        STRING,
        BOOLEAN,
        NULL
    }
}