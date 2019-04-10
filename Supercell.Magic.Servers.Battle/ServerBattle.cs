namespace Supercell.Magic.Servers.Battle
{
    using Supercell.Magic.Servers.Battle.Cluster;

    public static class ServerBattle
    {
        public static void Init()
        {
            GameModeClusterManager.Init();
        }
    }
}