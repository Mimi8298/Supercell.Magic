namespace Supercell.Magic.Titan.Util
{
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;

    public class LogicLongToCodeConverterUtil
    {
        private readonly string m_hashTag;
        private readonly string m_conversionChars;

        public LogicLongToCodeConverterUtil(string hashTag, string conversionChars)
        {
            this.m_hashTag = hashTag;
            this.m_conversionChars = conversionChars;
        }

        public string ToCode(LogicLong logicLong)
        {
            int highValue = logicLong.GetHigherInt();

            if (highValue < 256)
            {
                return this.m_hashTag + this.Convert(((long) logicLong.GetLowerInt() << 8) | (uint) highValue);
            }

            Debugger.Warning("Cannot convert the code to string. Higher int value too large");
            return null;
        }

        public LogicLong ToId(string code)
        {
            if (code.Length < 14)
            {
                string idCode = code.Substring(1);
                long id = this.ConvertCode(idCode);

                if (id != -1)
                {
                    return new LogicLong((int) (id % 256), (int) ((id >> 8) & 0x7FFFFFFF));
                }
            }
            else
            {
                Debugger.Warning("Cannot convert the string to code. String is too long.");
            }

            return new LogicLong(-1, -1);
        }

        private long ConvertCode(string code)
        {
            long id = 0;
            int conversionCharsCount = this.m_conversionChars.Length;
            int codeCharsCount = code.Length;

            for (int i = 0; i < codeCharsCount; i++)
            {
                int charIndex = this.m_conversionChars.IndexOf(code[i]);

                if (charIndex == -1)
                {
                    Debugger.Warning("Cannot convert the string to code. String contains invalid character(s).");
                    id = -1;
                    break;
                }

                id = id * conversionCharsCount + charIndex;
            }

            return id;
        }

        private string Convert(long value)
        {
            char[] code = new char[12];

            if (value > -1)
            {
                int conversionCharsCount = this.m_conversionChars.Length;

                for (int i = 11; i >= 0; i--)
                {
                    code[i] = this.m_conversionChars[(int) (value % conversionCharsCount)];
                    value /= conversionCharsCount;

                    if (value == 0)
                    {
                        return new string(code, i, 12 - i);
                    }
                }

                return new string(code);
            }

            Debugger.Warning("LogicLongToCodeConverter: value to convert cannot be negative");

            return null;
        }
    }
}