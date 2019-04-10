namespace Supercell.Magic.Servers.Admin.Logic
{
    using System.Text;
    using System.Threading.Tasks;
    using Couchbase;
    using Couchbase.N1QL;
    using Newtonsoft.Json.Linq;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Database.Document;
    using Supercell.Magic.Servers.Core.Settings;
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public static class UserManager
    {
        private static LogicArrayList<string> m_presetLevels;
        private static LogicRandom m_presetRandom;

        public static void Init()
        {
            UserManager.m_presetLevels = new LogicArrayList<string>();
            UserManager.m_presetRandom = new LogicRandom(TimeUtil.GetTimestamp());

            LogicArrayList<string> presetLevelFiles = EnvironmentSettings.Settings.Admin.PresetLevelFiles;

            for (int i = 0; i < presetLevelFiles.Size(); i++)
            {
                string json = ServerHttpClient.DownloadString("data/" + presetLevelFiles[i]);

                if (json != null)
                    UserManager.m_presetLevels.Add(json);
            }
        }

        public static async Task<AccountDocument> GetAccount(long id)
        {
            IOperationResult<string> result = await ServerAdmin.AccountDatabase.Get(id);

            if (result.Success)
                return CouchbaseDocument.Load<AccountDocument>(result.Value);
            return null;
        }

        public static async void SaveAccount(AccountDocument document)
        {
            await ServerAdmin.AccountDatabase.Update(document.Id, CouchbaseDocument.Save(document));
        }

        public static async Task<GameDocument> GetAvatar(long id)
        {
            IOperationResult<string> result = await ServerAdmin.GameDatabase.Get(id);

            if (result.Success)
                return CouchbaseDocument.Load<GameDocument>(result.Value);
            return null;
        }

        public static string GetPresetLevel()
        {
            return UserManager.m_presetLevels[UserManager.m_presetRandom.Rand(UserManager.m_presetLevels.Size())];
        }

        public static async Task<JArray> Search(string name, int expLevel, int score, string allianceName)
        {
            StringBuilder builder = new StringBuilder("SELECT id_hi,id_lo,name,xp_level,score,alliance_name FROM `%BUCKET%` WHERE meta().id LIKE '%KEY_PREFIX%%'");

            if (!string.IsNullOrEmpty(name))
                builder.Append(" AND LOWER(name) LIKE '%" + name.ToLowerInvariant() + "%'");
            if (expLevel != 0)
                builder.Append(" AND xp_level == " + expLevel);
            if (score != 0)
                builder.Append(" AND score == " + score);
            if (!string.IsNullOrEmpty(allianceName))
                builder.Append(" AND LOWER(alliance_name) LIKE '%" + allianceName.ToLowerInvariant() + "%'");
            builder.Append(" LIMIT 100");

            IQueryResult<JObject> result = await ServerAdmin.GameDatabase.ExecuteCommand<JObject>(builder.ToString());

            if (result.Success)
            {
                JArray jArray = new JArray();

                for (int i = 0; i < result.Rows.Count; i++)
                {
                    JObject original = result.Rows[i];
                    JObject jObject = new JObject();

                    LogicLong id = new LogicLong((int) original["id_hi"], (int) original["id_lo"]);

                    jObject.Add("id", (long) id);
                    jObject.Add("tag", HashTagCodeGenerator.m_instance.ToCode(id));
                    jObject.Add("name", original["name"]);
                    jObject.Add("level", original["xp_level"]);
                    jObject.Add("score", original["score"]);

                    if (original.ContainsKey("alliance_name")) jObject.Add("allianceName", original["alliance_name"]);

                    jArray.Add(jObject);
                }

                return jArray;
            }

            return null;
        }
    }
}