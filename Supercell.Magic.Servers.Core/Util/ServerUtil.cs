namespace Supercell.Magic.Servers.Core.Util
{
    public static class ServerUtil
    {
        public static string GetServerName(int type)
        {
            switch (type)
            {
                case 0: return "Admin";
                case 1: return "Proxy";
                case 3: return "Friend";
                case 6: return "Chat";
                case 9: return "Game";
                case 10: return "Home";
                case 11: return "Stream";
                case 13: return "League";
                case 25: return "War";
                case 27: return "Battle";
                case 28: return "Scoring";
                case 29: return "Search";
                default: return "(unk)";
            }
        }

        public static bool IsContainerServer(int type)
        {
            return type == 6 || type == 10 || type == 12 || type == 27 || type == 28 || type == 29;
        }
    }
}