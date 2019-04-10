namespace Supercell.Magic.Titan.Util
{
    using System.Text;
    using Supercell.Magic.Titan.Debug;

    public static class LogicStringUtil
    {
        public static byte[] GetBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static string CreateString(byte[] value, int offset, int length)
        {
            return Encoding.UTF8.GetString(value, offset, length);
        }

        public static int ConvertToInt(string value)
        {
            return LogicStringUtil.ConvertToInt(value, 0, value.Length);
        }

        public static int ConvertToInt(string value, int startIndex, int endIndex)
        {
            int length = endIndex - startIndex;

            if (length <= 0)
            {
                Debugger.Warning("LogicStringUtil::convertToInt empty String");
            }
            else
            {
                if (length < 12)
                {
                    if (value[startIndex] == '-')
                    {
                        ++startIndex;
                    }

                    for (int i = 0; i < length; i++)
                    {
                        if (value[startIndex + i] < '0' || value[startIndex + i] > '9')
                        {
                            Debugger.Warning("LogicStringUtil::convertToInt invalid value: : " + value.Substring(startIndex, length));
                            return 0;
                        }
                    }

                    return int.Parse(value.Substring(startIndex, length));
                }

                Debugger.Warning("LogicStringUtil::convertToInt too long value: " + value.Substring(startIndex, length));
            }

            return 0;
        }
    }
}