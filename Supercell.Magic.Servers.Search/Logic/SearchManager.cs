namespace Supercell.Magic.Servers.Search.Logic
{
    using System.Text;
    using System.Threading.Tasks;
    using Couchbase;
    using Couchbase.N1QL;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Servers.Core.Database;
    using Supercell.Magic.Servers.Core.Database.Document;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public static class SearchManager
    {
        public const string QUERY_ALLIANCES_LIST_SELECT = CouchbaseDocument.JSON_ATTRIBUTE_ID_HIGH + "," + CouchbaseDocument.JSON_ATTRIBUTE_ID_LOW + "," +
                                                                   "alliance_name,badge_id,type,member_count,score,duel_score,xp_level";

        public const string QUERY_JOINABLE_ALLIANCES_LIST_FILTER =
            "meta().id LIKE '%KEY_PREFIX%%' AND type == 1 AND member_count BETWEEN 1 AND 49 AND required_score <= %AVATAR_SCORE% AND required_duel_score <= %AVATAR_DUEL_SCORE%";

        public const string QUERY_JOINABLE_ALLIANCES_LIST = "SELECT " + SearchManager.QUERY_ALLIANCES_LIST_SELECT + " FROM `%BUCKET%` WHERE " +
                                                            SearchManager.QUERY_JOINABLE_ALLIANCES_LIST_FILTER + " ORDER BY score DESC LIMIT 200";

        public static async Task<LogicArrayList<AllianceHeaderEntry>> GetJoinableAlliancesList(int score, int duelScore)
        {
            LogicArrayList<AllianceHeaderEntry> allianceList = new LogicArrayList<AllianceHeaderEntry>();
            IQueryResult<JObject> result =
                await ServerSearch.AllianceDatabase.ExecuteCommand<JObject>(SearchManager.QUERY_JOINABLE_ALLIANCES_LIST.Replace("%AVATAR_SCORE%", score.ToString())
                                                                                         .Replace("%AVATAR_DUEL_SCORE%", duelScore.ToString()));

            if (result.Success)
            {
                allianceList.EnsureCapacity(result.Rows.Count);
                result.Rows.ForEach(jsonObject => { allianceList.Add(SearchManager.LoadAllianceHeaderEntry(jsonObject)); });
            }

            return allianceList;
        }

        public static async Task<LogicArrayList<AllianceHeaderEntry>> GetAllianceHeaderList(LogicArrayList<LogicLong> ids)
        {
            LogicArrayList<AllianceHeaderEntry> allianceList = new LogicArrayList<AllianceHeaderEntry>(ids.Size());
            Task<IOperationResult<string>>[] results = new Task<IOperationResult<string>>[ids.Size()];

            for (int i = 0; i < ids.Size(); i++)
            {
                results[i] = ServerSearch.AllianceDatabase.Get(ids[i]);
            }

            for (int i = 0; i < ids.Size(); i++)
            {
                IOperationResult<string> result = await results[i];

                if (result.Success)
                {
                    LogicJSONObject jsonObject = LogicJSONParser.ParseObject(result.Value);
                    AllianceHeaderEntry allianceHeaderEntry = new AllianceHeaderEntry();

                    allianceHeaderEntry.Load(jsonObject);
                    allianceHeaderEntry.SetAllianceId(new LogicLong(jsonObject.GetJSONNumber(CouchbaseDocument.JSON_ATTRIBUTE_ID_HIGH).GetIntValue(),
                                                                    jsonObject.GetJSONNumber(CouchbaseDocument.JSON_ATTRIBUTE_ID_LOW).GetIntValue()));
                    allianceList.Add(allianceHeaderEntry);
                }
            }

            return allianceList;
        }
        
        public static async Task<LogicArrayList<AllianceHeaderEntry>> SearchAlliances(SearchAlliancesMessage searchAlliancesMessage, LogicClientAvatar playerAvatar)
        {
            LogicArrayList<AllianceHeaderEntry> allianceList = new LogicArrayList<AllianceHeaderEntry>();
            StringBuilder builder = new StringBuilder();

            builder.Append("SELECT ");
            builder.Append(SearchManager.QUERY_ALLIANCES_LIST_SELECT);
            builder.Append(" FROM `%BUCKET%` WHERE meta().id LIKE '%KEY_PREFIX%%' AND (");

            string searchString = searchAlliancesMessage.GetSearchString();

            if (!string.IsNullOrEmpty(searchString))
            {
                if (searchString.Length >= 16)
                    searchString = searchString.Substring(0, 15);

                if (searchString.StartsWith(HashTagCodeGenerator.CONVERSION_TAG))
                {
                    LogicLong allianceId = HashTagCodeGenerator.m_instance.ToId(searchString.Trim().ToUpperInvariant());

                    if (allianceId != null)
                    {
                        builder.Append(CouchbaseDocument.JSON_ATTRIBUTE_ID_HIGH);
                        builder.Append(" == ");
                        builder.Append(allianceId.GetHigherInt().ToString());
                        builder.Append(" AND ");
                        builder.Append(CouchbaseDocument.JSON_ATTRIBUTE_ID_LOW);
                        builder.Append(" == ");
                        builder.Append(allianceId.GetLowerInt().ToString());
                        builder.Append(" OR ");
                    }
                }

                builder.Append("LOWER(alliance_name) LIKE '%");
                builder.Append(searchString.ToLowerInvariant());
                builder.Append("%' AND ");
            }

            builder.Append("member_count BETWEEN ");
            builder.Append(searchAlliancesMessage.GetMinimumMembers().ToString());
            builder.Append(" AND ");
            builder.Append(searchAlliancesMessage.GetMaximumMembers().ToString());

            if (searchAlliancesMessage.GetWarFrequency() != 0)
            {
                builder.Append(" AND war_freq == ");
                builder.Append(searchAlliancesMessage.GetWarFrequency().ToString());
            }

            if (searchAlliancesMessage.GetOrigin() != null)
            {
                builder.Append(" AND origin == ");
                builder.Append(searchAlliancesMessage.GetOrigin().GetGlobalID().ToString());
            }

            if (searchAlliancesMessage.GetRequiredScore() != 0)
            {
                builder.Append(" AND score >= ");
                builder.Append(searchAlliancesMessage.GetRequiredScore().ToString());
            }

            if (searchAlliancesMessage.GetRequiredDuelScore() != 0)
            {
                builder.Append(" AND duel_score >= ");
                builder.Append(searchAlliancesMessage.GetRequiredDuelScore().ToString());
            }

            if (searchAlliancesMessage.GetMinimumExpLevel() != 0)
            {
                builder.Append(" AND xp_level >= ");
                builder.Append(searchAlliancesMessage.GetMinimumExpLevel().ToString());
            }

            if (searchAlliancesMessage.IsJoinableOnly())
            {
                builder.Append(" AND required_score <= ");
                builder.Append(playerAvatar.GetScore().ToString());
                builder.Append(" AND required_duel_score <= ");
                builder.Append(playerAvatar.GetDuelScore().ToString());
            }

            builder.AppendLine(") ORDER BY score DESC LIMIT 200");

            IQueryResult<JObject> result = await ServerSearch.AllianceDatabase.ExecuteCommand<JObject>(builder.ToString());

            if (result.Success)
            {
                allianceList.EnsureCapacity(result.Rows.Count);
                result.Rows.ForEach(jsonObject => { allianceList.Add(SearchManager.LoadAllianceHeaderEntry(jsonObject)); });
            }

            return allianceList;
        }

        public static AllianceHeaderEntry LoadAllianceHeaderEntry(JObject jsonObject)
        {
            AllianceHeaderEntry allianceHeaderEntry = new AllianceHeaderEntry();

            allianceHeaderEntry.SetAllianceId(new LogicLong((int) jsonObject[CouchbaseDocument.JSON_ATTRIBUTE_ID_HIGH],
                                                            (int) jsonObject[CouchbaseDocument.JSON_ATTRIBUTE_ID_LOW]));
            allianceHeaderEntry.SetAllianceName((string) jsonObject["alliance_name"]);
            allianceHeaderEntry.SetAllianceBadgeId((int) jsonObject["badge_id"]);
            allianceHeaderEntry.SetAllianceType((AllianceType) (int) jsonObject["type"]);
            allianceHeaderEntry.SetNumberOfMembers((int) jsonObject["member_count"]);
            allianceHeaderEntry.SetScore((int) jsonObject["score"]);
            allianceHeaderEntry.SetDuelScore((int) jsonObject["duel_score"]);
            allianceHeaderEntry.SetAllianceLevel((int) jsonObject["xp_level"]);

            return allianceHeaderEntry;
        }
    }
}