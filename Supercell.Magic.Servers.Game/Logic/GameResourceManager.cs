namespace Supercell.Magic.Servers.Game.Logic
{
    using System.Collections.Generic;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Home;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Helper;

    public static class GameResourceManager
    {
        public static byte[] CompressedStartingHomeJSON { get; private set; }
        public static LogicClientHome[] NpcHomes { get; private set; }
        
        public static void Init()
        {
            GameResourceManager.CompressedStartingHomeJSON = GameResourceManager.Compress(ServerHttpClient.DownloadBytes("data/level/starting_home.json"));
            GameResourceManager.NpcHomes = new LogicClientHome[LogicDataTables.GetTable(LogicDataType.NPC).GetItemCount()];

            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.NPC);

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicNpcData data = (LogicNpcData)table.GetItemAt(i);
                LogicClientHome logicClientHome = new LogicClientHome();

                logicClientHome.GetCompressibleHomeJSON().Set(GameResourceManager.Compress(ServerHttpClient.DownloadBytes("data/" + data.GetLevelFile())));

                GameResourceManager.NpcHomes[i] = logicClientHome;
            }
        }
        
        private static byte[] Compress(byte[] json)
        {
            ZLibHelper.CompressInZLibFormat(json, out byte[] output);
            return output;
        }
    }
}