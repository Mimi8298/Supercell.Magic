namespace Supercell.Magic.Servers.Home.Util
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Home;
    using Supercell.Magic.Servers.Core.Helper;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Util;

    public static class GameUtil
    {
        public static void SaveHomeOwnerAvatarInHome(LogicClientAvatar logicClientAvatar, LogicClientHome logicClientHome)
        {
            CompressibleStringHelper.Uncompress(logicClientHome.GetCompressibleHomeJSON());
            LogicJSONObject jsonObject = LogicJSONParser.ParseObject(logicClientHome.GetCompressibleHomeJSON().Get());
            logicClientAvatar.SaveToDirect(jsonObject);
            logicClientHome.SetHomeJSON(LogicJSONParser.CreateJSONString(jsonObject, 512));
            CompressibleStringHelper.Compress(logicClientHome.GetCompressibleHomeJSON());
        }

        public static LogicClientHome CloneHome(LogicClientHome home)
        {
            LogicClientHome copy = new LogicClientHome();
            ByteStream stream = new ByteStream(512);
            home.Encode(stream);
            copy.Decode(stream);
            return copy;
        }

        public static LogicClientAvatar LoadHomeOwnerAvatarFromHome(LogicClientHome home)
        {
            string json = home.GetCompressibleHomeJSON().Get();

            if (json == null)
            {
                ZLibHelper.DecompressInMySQLFormat(home.GetCompressibleHomeJSON().GetCompressed(), out byte[] output);
                json = LogicStringUtil.CreateString(output, 0, output.Length);
            }
            
            LogicClientAvatar logicClientAvatar = new LogicClientAvatar();
            logicClientAvatar.LoadForReplay(LogicJSONParser.ParseObject(json), true);
            return logicClientAvatar;
        }
    }
}