namespace Supercell.Magic.Servers.Scoring
{
    using Supercell.Magic.Servers.Core.Database;
    using Supercell.Magic.Servers.Scoring.Logic;

    public static class ServerScoring
    {
        public static CouchbaseDatabase GameDatabase { get; private set; }
        public static CouchbaseDatabase AllianceDatabase { get; private set; }
        public static CouchbaseDatabase SeasonDatabase { get; private set; }

        public static void Init()
        {
            ServerScoring.GameDatabase = new CouchbaseDatabase("magic-players", "game");
            ServerScoring.AllianceDatabase = new CouchbaseDatabase("magic-alliances", "data");
            ServerScoring.SeasonDatabase = new CouchbaseDatabase("magic-seasons", "season");

            ScoringManager.Init();
        }
    }
}