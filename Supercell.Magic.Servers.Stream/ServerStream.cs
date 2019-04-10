namespace Supercell.Magic.Servers.Stream
{
    using Supercell.Magic.Servers.Stream.Logic;
    using Supercell.Magic.Servers.Stream.Session;

    using Supercell.Magic.Servers.Core.Database;
    using Supercell.Magic.Servers.Core.Util;

    public static class ServerStream
    {
        public static CouchbaseDatabase AllianceDatabase { get; private set; }
        public static CouchbaseDatabase StreamDatabase { get; private set; }

        public static void Init()
        {
            ServerStream.AllianceDatabase = new CouchbaseDatabase("magic-alliances", "data");
            ServerStream.StreamDatabase = new CouchbaseDatabase("magic-streams", "stream");

            AllianceManager.Init();
            StreamManager.Init();
            AllianceSessionManager.Init();
            WordCensorUtil.Init();
        }
    }
}