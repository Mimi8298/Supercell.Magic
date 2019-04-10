namespace Supercell.Magic.Servers.Home
{
    using Supercell.Magic.Servers.Home.Cluster;

    public static class ServerHome
    {
        public static void Init()
        {
            GameModeClusterManager.Init();
        }
    }
}