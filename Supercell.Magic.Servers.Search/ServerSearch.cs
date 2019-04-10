namespace Supercell.Magic.Servers.Search
{
    using Supercell.Magic.Servers.Core.Database;
    using Supercell.Magic.Servers.Core.Database.Document;

    public static class ServerSearch
    {
        public static CouchbaseDatabase AllianceDatabase { get; private set; }

        public static void Init()
        {
            ServerSearch.AllianceDatabase = new CouchbaseDatabase("magic-alliances", "data");
            ServerSearch.AllianceDatabase.CreateIndexWithFilter("joinableAlliancesIndex", "meta().id LIKE '%KEY_PREFIX%%' AND type == 1 AND member_count BETWEEN 1 AND 49", "meta().id", "type",
                                                                "member_count", "required_score", "required_duel_score");
            ServerSearch.AllianceDatabase.CreateIndexWithFilter("searchAlliancesIndex", "meta().id LIKE '%KEY_PREFIX%%'", CouchbaseDocument.JSON_ATTRIBUTE_ID_HIGH,
                                                                CouchbaseDocument.JSON_ATTRIBUTE_ID_LOW, "alliance_name", "war_freq", "origin", "member_count", "score", "duel_score", "xp_level",
                                                                "required_score", "required_duel_score");
        }
    }
}