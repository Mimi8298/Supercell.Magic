namespace Supercell.Magic.Servers.Core.Util
{
    using System;
    using System.Collections.Generic;

    public static class WordCensorUtil
    {
        private static HashSet<string> m_badWords;

        public static void Init()
        {
            WordCensorUtil.m_badWords = new HashSet<string>(ServerHttpClient.DownloadString("data/badwords.txt").Split(new string[] {"\r\n", "\n" }, StringSplitOptions.None));
        }

        public static string FilterMessage(string message)
        {
            string[] array = message.Split(' ');

            for (int i = 0; i < array.Length; i++)
            {
                string str = array[i];
                string toLower = str.ToLower();

                if (WordCensorUtil.m_badWords.Contains(toLower))
                    array[i] = "***";
            }

            return string.Join(" ", array);
        }

        public static bool IsValidMessage(string message)
        {
            string[] array = message.Split(' ');

            for (int i = 0; i < array.Length; i++)
            {
                string str = array[i];
                string toLower = str.ToLower();

                if (WordCensorUtil.m_badWords.Contains(toLower))
                    return false;
            }

            return true;
        }
    }
}