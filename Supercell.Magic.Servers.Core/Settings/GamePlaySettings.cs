namespace Supercell.Magic.Servers.Core.Settings
{
    using Supercell.Magic.Titan.Json;

    public static class GamePlaySettings
    {
        public static bool FriendlyChallengeEnabled { get; private set; }
        public static bool OpAttackEnabled { get; private set; }
        public static bool OpRankingEnabled { get; private set; }

        public static void Init()
        {
            GamePlaySettings.Load(ServerHttpClient.DownloadJSON("gameplay.json"));
        }

        public static void Load(LogicJSONObject jsonObject)
        {
            GamePlaySettings.FriendlyChallengeEnabled = jsonObject.GetJSONBoolean("friendlyChallengeEnabled").IsTrue();
            GamePlaySettings.OpAttackEnabled = jsonObject.GetJSONBoolean("opAttackEnabled").IsTrue();
            GamePlaySettings.OpRankingEnabled = jsonObject.GetJSONBoolean("opRankingEnabled").IsTrue();
        }

        public static LogicJSONObject Save()
        {
            LogicJSONObject jsonObject = new LogicJSONObject();
            
            jsonObject.Put("friendlyChallengeEnabled", new LogicJSONBoolean(GamePlaySettings.FriendlyChallengeEnabled));
            jsonObject.Put("opAttackEnabled", new LogicJSONBoolean(GamePlaySettings.OpAttackEnabled));
            jsonObject.Put("opRankingEnabled", new LogicJSONBoolean(GamePlaySettings.OpRankingEnabled));

            return jsonObject;
        }
    }
}