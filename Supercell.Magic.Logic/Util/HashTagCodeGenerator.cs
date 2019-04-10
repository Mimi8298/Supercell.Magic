namespace Supercell.Magic.Logic.Util
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class HashTagCodeGenerator
    {
        public static readonly HashTagCodeGenerator m_instance = new HashTagCodeGenerator();

        public const string CONVERSION_TAG = "#";
        public const string CONVERSION_CHARS = "0289PYLQGRJCUV";

        private readonly LogicLongToCodeConverterUtil m_codeConverterUtil;

        private HashTagCodeGenerator()
        {
            this.m_codeConverterUtil = new LogicLongToCodeConverterUtil(HashTagCodeGenerator.CONVERSION_TAG, HashTagCodeGenerator.CONVERSION_CHARS);
        }

        public string ToCode(LogicLong logicLong)
        {
            return this.m_codeConverterUtil.ToCode(logicLong);
        }

        public LogicLong ToId(string value)
        {
            LogicLong id = this.m_codeConverterUtil.ToId(value);

            if (this.IsIdValid(id))
            {
                return id;
            }

            return null;
        }

        public bool IsIdValid(LogicLong id)
        {
            return id.GetHigherInt() != -1 && id.GetHigherInt() != -1;
        }
    }
}