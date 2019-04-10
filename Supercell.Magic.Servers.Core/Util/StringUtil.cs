namespace Supercell.Magic.Servers.Core.Util
{
    public static class StringUtil
    {
        public static unsafe string RemoveMultipleSpaces(string str)
        {
            int length = str.Length;

            if (length > 1)
            {
                char[] strChars = str.ToCharArray();
                char[] output = new char[strChars.Length];

                int offset = 0;

                fixed (char* cp = strChars)
                {
                    char* c = cp;

                    output[offset++] = *c;
                    c += 1;
                    length -= 1;

                    while (length > 0)
                    {
                        if (*c > ' ' || *(c - 1) > ' ')
                        {
                            if (*c < ' ')
                            {
                                *c = ' ';
                            }

                            output[offset++] = *c;
                        }

                        c += 1;
                        length -= 1;
                    }
                }

                return new string(output, 0, offset);
            }

            return str;
        }
    }
}