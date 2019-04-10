namespace Supercell.Magic.Servers.Game
{
    using Supercell.Magic.Servers.Core.Database;
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Servers.Game.Logic;
    using Supercell.Magic.Servers.Game.Logic.Live;
    using Supercell.Magic.Servers.Game.Session;

    public static class ServerGame
    {
        public static CouchbaseDatabase GameDatabase { get; private set; }

        public static void Init()
        {
            ServerGame.GameDatabase = new CouchbaseDatabase("magic-players", "game");

            GameResourceManager.Init();
            GameMatchmakingManager.Init();
            GameDuelMatchmakingManager.Init();
            GameAvatarManager.Init();
            GameSessionManager.Init();
            LiveReplayManager.Init();
            GameBaseGenerator.Init();
            WordCensorUtil.Init();
        }
    }
}