namespace Supercell.Magic.Servers.Core.Settings
{
    using System;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public static class EnvironmentSettings
    {
        public const int SERVER_TYPE_COUNT = 30;

        public static ServerConnectionEntry[][] Servers { get; private set; }
        public static CouchbaseSettings Couchbase { get; private set; }
        public static RedisSettings Redis { get; private set; }
        public static ServerSettings Settings { get; private set; }

        public static string Environment { get; private set; }
        public static int HigherIdCounterSize { get; private set; }
        public static string[] SupportedAppVersions { get; private set; }
        public static string[] SupportedCountries { get; private set; }
        public static string[] DeveloperIPs { get; private set; }

        public static void Init()
        {
            EnvironmentSettings.Servers = new ServerConnectionEntry[30][];
            EnvironmentSettings.SupportedAppVersions = new string[0];
            EnvironmentSettings.SupportedCountries = new string[0];
            EnvironmentSettings.DeveloperIPs = new string[0];

            for (int i = 0; i < EnvironmentSettings.SERVER_TYPE_COUNT; i++)
            {
                EnvironmentSettings.Servers[i] = new ServerConnectionEntry[0];
            }

            EnvironmentSettings.Load(ServerHttpClient.DownloadJSON("environment.json"));
        }

        public static bool IsSupportedAppVersion(string appVersion)
        {
            if (EnvironmentSettings.SupportedAppVersions.Length > 0)
                return Array.IndexOf(EnvironmentSettings.SupportedAppVersions, appVersion) != -1;
            return true;
        }

        public static bool IsSupportedCountry(string country)
        {
            if (EnvironmentSettings.SupportedCountries.Length > 0)
                return Array.IndexOf(EnvironmentSettings.SupportedCountries, country) != -1;
            return true;
        }

        public static bool IsDeveloperIP(string ip)
        {
            return Array.IndexOf(EnvironmentSettings.DeveloperIPs, ip) != -1;
        }

        public static bool IsStageServer()
        {
            return EnvironmentSettings.Environment.StartsWith("stage");
        }

        public static bool IsIntegrationServer()
        {
            return EnvironmentSettings.Environment.StartsWith("integration");
        }

        public static bool IsProductionServer()
        {
            return EnvironmentSettings.Environment.Equals("prod");
        }

        public static void Load(LogicJSONObject jsonObject)
        {
            EnvironmentSettings.Environment = jsonObject.GetJSONString("environment").GetStringValue();

            LogicJSONObject serverObject = jsonObject.GetJSONObject("servers");

            if (serverObject != null)
            {
                EnvironmentSettings.Settings = new ServerSettings(serverObject.GetJSONObject("settings"));

                EnvironmentSettings.LoadServerArray(serverObject.GetJSONArray("admin"), 0);
                EnvironmentSettings.LoadServerArray(serverObject.GetJSONArray("proxy"), 1);
                EnvironmentSettings.LoadServerArray(serverObject.GetJSONArray("account"), 2);
                EnvironmentSettings.LoadServerArray(serverObject.GetJSONArray("friend"), 3);
                EnvironmentSettings.LoadServerArray(serverObject.GetJSONArray("chat"), 6);
                EnvironmentSettings.LoadServerArray(serverObject.GetJSONArray("game"), 9);
                EnvironmentSettings.LoadServerArray(serverObject.GetJSONArray("home"), 10);
                EnvironmentSettings.LoadServerArray(serverObject.GetJSONArray("stream"), 11);
                EnvironmentSettings.LoadServerArray(serverObject.GetJSONArray("league"), 13);
                EnvironmentSettings.LoadServerArray(serverObject.GetJSONArray("war"), 25);
                EnvironmentSettings.LoadServerArray(serverObject.GetJSONArray("battle"), 27);
                EnvironmentSettings.LoadServerArray(serverObject.GetJSONArray("scoring"), 28);
                EnvironmentSettings.LoadServerArray(serverObject.GetJSONArray("search"), 29);
            }

            LogicJSONObject databaseObject = jsonObject.GetJSONObject("database");

            if (databaseObject != null)
            {
                EnvironmentSettings.HigherIdCounterSize = databaseObject.GetJSONNumber("higher_id_counter_size").GetIntValue();
                EnvironmentSettings.Couchbase = new CouchbaseSettings(databaseObject.GetJSONObject("couchbase"));
                EnvironmentSettings.Redis = new RedisSettings(databaseObject.GetJSONObject("redis"));
            }
            
            EnvironmentSettings.SupportedAppVersions = EnvironmentSettings.LoadStringArray(jsonObject.GetJSONArray("supported_app_versions"));
            EnvironmentSettings.SupportedCountries = EnvironmentSettings.LoadStringArray(jsonObject.GetJSONArray("supported_countries"));
            EnvironmentSettings.DeveloperIPs = EnvironmentSettings.LoadStringArray(jsonObject.GetJSONArray("developer_ips"));
        }

        private static void LoadServerArray(LogicJSONArray jsonArray, int type)
        {
            ServerConnectionEntry[] entries;

            if (jsonArray != null)
            {
                entries = new ServerConnectionEntry[jsonArray.Size()];

                for (int i = 0; i < jsonArray.Size(); i++)
                {
                    entries[i] = new ServerConnectionEntry(jsonArray.GetJSONString(i));
                }
            }
            else
            {
                entries = new ServerConnectionEntry[0];
            }

            EnvironmentSettings.Servers[type] = entries;
        }

        private static string[] LoadStringArray(LogicJSONArray jsonArray)
        {
            string[] stringArray;

            if (jsonArray != null)
            {
                stringArray = new string[jsonArray.Size()];

                for (int i = 0; i < jsonArray.Size(); i++)
                {
                    stringArray[i] = jsonArray.GetJSONString(i).GetStringValue();
                }
            }
            else
            {
                stringArray = new string[0];
            }

            return stringArray;
        }
        
        public static LogicJSONObject Save()
        {
            LogicJSONObject jsonObject = new LogicJSONObject();

            jsonObject.Put("environment", new LogicJSONString(EnvironmentSettings.Environment));

            LogicJSONObject serverObject = new LogicJSONObject();

            serverObject.Put("settings", EnvironmentSettings.Settings.Save());
            serverObject.Put("admin", EnvironmentSettings.SaveServerArray(0));
            serverObject.Put("proxy", EnvironmentSettings.SaveServerArray(1));
            serverObject.Put("account", EnvironmentSettings.SaveServerArray(2));
            serverObject.Put("friend", EnvironmentSettings.SaveServerArray(3));
            serverObject.Put("chat", EnvironmentSettings.SaveServerArray(6));
            serverObject.Put("avatar", EnvironmentSettings.SaveServerArray(9));
            serverObject.Put("home", EnvironmentSettings.SaveServerArray(10));
            serverObject.Put("stream", EnvironmentSettings.SaveServerArray(11));
            serverObject.Put("league", EnvironmentSettings.SaveServerArray(13));
            serverObject.Put("war", EnvironmentSettings.SaveServerArray(25));
            serverObject.Put("battle", EnvironmentSettings.SaveServerArray(27));
            serverObject.Put("scoring", EnvironmentSettings.SaveServerArray(28));
            serverObject.Put("search", EnvironmentSettings.SaveServerArray(29));

            jsonObject.Put("servers", serverObject);
            
            LogicJSONObject databaseObject = new LogicJSONObject();

            databaseObject.Put("higher_id_counter_size", new LogicJSONNumber(EnvironmentSettings.HigherIdCounterSize));
            databaseObject.Put("couchbase", EnvironmentSettings.Couchbase.Save());
            databaseObject.Put("redis", EnvironmentSettings.Redis.Save());

            jsonObject.Put("supported_app_versions", EnvironmentSettings.SaveStringArray(EnvironmentSettings.SupportedAppVersions));
            jsonObject.Put("supported_countries", EnvironmentSettings.SaveStringArray(EnvironmentSettings.SupportedCountries));
            jsonObject.Put("developer_ips", EnvironmentSettings.SaveStringArray(EnvironmentSettings.DeveloperIPs));

            return jsonObject;
        }
        
        private static LogicJSONArray SaveServerArray(int type)
        {
            LogicJSONArray jsonArray = new LogicJSONArray();

            if (EnvironmentSettings.Servers[type] != null)
            {
                ServerConnectionEntry[] connectionEntryArray = EnvironmentSettings.Servers[type];

                for (int i = 0; i < connectionEntryArray.Length; i++)
                {
                    jsonArray.Add(connectionEntryArray[i].Save());
                }
            }

            return jsonArray;
        }

        private static LogicJSONArray SaveStringArray(string[] stringArray)
        {
            LogicJSONArray jsonArray = new LogicJSONArray();

            if (stringArray != null)
            {
                for (int i = 0; i < stringArray.Length; i++)
                {
                    jsonArray.Add(new LogicJSONString(stringArray[i]));
                }
            }

            return jsonArray;
        }
        
        public struct RedisSettings
        {
            public LogicArrayList<RedisDatabaseEntry> Databases { get; }

            public RedisSettings(LogicJSONObject jsonObject)
            {
                LogicJSONArray databases = jsonObject.GetJSONArray("databases");
                this.Databases = new LogicArrayList<RedisDatabaseEntry>(databases.Size());

                for (int i = 0; i < databases.Size(); i++)
                {
                    this.Databases.Add(new RedisDatabaseEntry(databases.GetJSONObject(i)));
                }
            }

            public LogicJSONObject Save()
            {
                LogicJSONObject jsonObject = new LogicJSONObject();
                LogicJSONArray databaseArray = new LogicJSONArray();

                for (int i = 0; i < this.Databases.Size(); i++)
                {
                    databaseArray.Add(this.Databases[i].Save());
                }

                jsonObject.Put("databases", databaseArray);

                return jsonObject;
            }

            public bool TryGetDatabase(string name, out RedisDatabaseEntry entry)
            {
                for (int i = 0; i < this.Databases.Size(); i++)
                {
                    if (this.Databases[i].Name == name)
                    {
                        entry = this.Databases[i];
                        return true;
                    } 
                }

                entry = default(RedisDatabaseEntry);
                return false;
            }

            public struct RedisDatabaseEntry
            {
                public string Name { get; }
                public string ConnectionString { get; }

                public RedisDatabaseEntry(LogicJSONObject jsonObject)
                {
                    this.Name = jsonObject.GetJSONString("name").GetStringValue();
                    this.ConnectionString = jsonObject.GetJSONString("connectionString").GetStringValue();
                }

                public LogicJSONObject Save()
                {
                    LogicJSONObject jsonObject = new LogicJSONObject();

                    jsonObject.Put("name", new LogicJSONString(this.Name));
                    jsonObject.Put("connectionString", new LogicJSONString(this.ConnectionString));

                    return jsonObject;
                }
            }
        }
        
        public struct ServerSettings
        {
            public ProxySettings Proxy { get; }
            public AdminSettings Admin { get; }

            public bool ContentValidationModeEnabled { get; }

            public ServerSettings(LogicJSONObject jsonObject)
            {
                this.ContentValidationModeEnabled = jsonObject.GetJSONBoolean("contentValidationModeEnabled").IsTrue();
                this.Proxy = new ProxySettings(jsonObject.GetJSONObject("proxy"));
                this.Admin = new AdminSettings(jsonObject.GetJSONObject("admin"));
            }

            public LogicJSONObject Save()
            {
                LogicJSONObject jsonObject = new LogicJSONObject();

                jsonObject.Put("contentValidationModeEnabled", new LogicJSONBoolean(this.ContentValidationModeEnabled));
                jsonObject.Put("proxy", this.Proxy.Save());
                jsonObject.Put("admin", this.Admin.Save());

                return jsonObject;
            }

            public struct ProxySettings
            {
                public int SessionCapacity { get; }

                public ProxySettings(LogicJSONObject jsonObject)
                {
                    this.SessionCapacity = LogicMath.Clamp(jsonObject.GetJSONNumber("sessionCapacity").GetIntValue(), 0, 16000);
                }

                public LogicJSONObject Save()
                {
                    LogicJSONObject jsonObject = new LogicJSONObject();
                    jsonObject.Put("sessionCapacity", new LogicJSONNumber(this.SessionCapacity));
                    return jsonObject;
                }
            }

            public struct AdminSettings
            {
                public LogicArrayList<string> PresetLevelFiles { get; }

                public AdminSettings(LogicJSONObject jsonObject)
                {
                    LogicJSONArray presetLevelFileArray = jsonObject.GetJSONArray("presetLevelFiles");

                    if (presetLevelFileArray != null)
                    {
                        this.PresetLevelFiles = new LogicArrayList<string>(presetLevelFileArray.Size());

                        for (int i = 0; i < presetLevelFileArray.Size(); i++)
                        {
                            this.PresetLevelFiles.Add(presetLevelFileArray.GetJSONString(i).GetStringValue());
                        }
                    }
                    else
                    {
                        this.PresetLevelFiles = null;
                    }
                }

                public LogicJSONObject Save()
                {
                    LogicJSONObject jsonObject = new LogicJSONObject();

                    if (this.PresetLevelFiles != null)
                    {
                        LogicJSONArray presetLevelFileArray = new LogicJSONArray();

                        for (int i = 0; i < this.PresetLevelFiles.Size(); i++)
                        {
                            presetLevelFileArray.Add(new LogicJSONString(this.PresetLevelFiles[i]));
                        }

                        jsonObject.Put("presetLevelFiles", presetLevelFileArray);
                    }

                    return jsonObject;
                }
            }
        }
        
        public struct ServerConnectionEntry
        {
            public string ServerIP { get; }
            public int ServerPort { get; }
            
            public ServerConnectionEntry(LogicJSONString jsonString)
            {
                string[] connectionString = jsonString.GetStringValue().Split(':');
                Logging.Assert(connectionString.Length == 2, "Malformed connection string!");
                this.ServerIP = connectionString[0];
                this.ServerPort = int.Parse(connectionString[1]);
            }

            public ServerConnectionEntry(string ip, int port)
            {
                this.ServerIP = ip;
                this.ServerPort = port;
            }

            public LogicJSONString Save()
            {
                return new LogicJSONString(string.Format("{0}:{1}", this.ServerIP, this.ServerPort));
            }
        }

        public struct CouchbaseSettings
        {
            public LogicArrayList<CouchbaseServerEntry> Servers { get; }
            public LogicArrayList<CouchbaseBucketEntry> Buckets { get; }

            public CouchbaseSettings(LogicJSONObject jsonObject)
            {
                LogicJSONArray serverArray = jsonObject.GetJSONArray("servers");
                LogicJSONArray bucketArray = jsonObject.GetJSONArray("buckets");

                this.Servers = new LogicArrayList<CouchbaseServerEntry>(serverArray.Size());
                this.Buckets = new LogicArrayList<CouchbaseBucketEntry>(bucketArray.Size());

                for (int i = 0; i < serverArray.Size(); i++)
                {
                    this.Servers.Add(new CouchbaseServerEntry(serverArray.GetJSONObject(i)));
                }

                for (int i = 0; i < bucketArray.Size(); i++)
                {
                    CouchbaseBucketEntry entry = new CouchbaseBucketEntry(bucketArray.GetJSONString(i));

                    if (this.GetBucketIdx(entry.Name) != -1)
                    {
                        Logging.Warning("EnvironmentSettings::CouchbaseSettings.ctr: bucket with the same name already exists.");
                        continue;
                    }

                    if ((uint)entry.ServerIndex >= this.Servers.Size())
                    {
                        Logging.Warning(string.Format("EnvironmentSettings::CouchbaseSettings.ctr: server index is out of bounds (bucket name: {0})", entry.ServerIndex));
                        continue;
                    }

                    this.Buckets.Add(entry);
                }
            }

            private int GetBucketIdx(string name)
            {
                for (int i = 0; i < this.Buckets.Size(); i++)
                {
                    if (this.Buckets[i].Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return i;
                    }
                }

                return -1;
            }

            public bool TryGetBucketData(string name, out CouchbaseServerEntry serverEntry, out CouchbaseBucketEntry bucketEntry)
            {
                int index = this.GetBucketIdx(name);

                if (index != -1)
                {
                    bucketEntry = this.Buckets[index];
                    serverEntry = this.Servers[bucketEntry.ServerIndex];

                    return true;
                }

                serverEntry = default(CouchbaseServerEntry);
                bucketEntry = null;

                return false;
            }

            public LogicJSONObject Save()
            {
                LogicJSONObject jsonObject = new LogicJSONObject();
                LogicJSONArray serverArray = new LogicJSONArray();
                LogicJSONArray bucketArray = new LogicJSONArray();

                for (int i = 0; i < this.Servers.Size(); i++)
                {
                    serverArray.Add(this.Servers[i].Save());
                }

                for (int i = 0; i < this.Buckets.Size(); i++)
                {
                    bucketArray.Add(this.Buckets[i].Save());
                }

                jsonObject.Put("servers", serverArray);
                jsonObject.Put("buckets", bucketArray);

                return jsonObject;
            }
        }

        public struct CouchbaseServerEntry
        {
            public Uri[] Hosts { get; }
            public string Username { get; }
            public string Password { get; }

            public CouchbaseServerEntry(LogicJSONObject jsonObject)
            {
                LogicJSONArray hostArray = jsonObject.GetJSONArray("hosts");

                this.Hosts = new Uri[hostArray.Size()];

                for (int i = 0; i < hostArray.Size(); i++)
                {
                    this.Hosts[i] = new Uri("http://" + hostArray.GetJSONString(i).GetStringValue());
                }

                this.Username = jsonObject.GetJSONString("username").GetStringValue();
                this.Password = jsonObject.GetJSONString("password").GetStringValue();
            }

            public LogicJSONObject Save()
            {
                LogicJSONObject jsonObject = new LogicJSONObject();
                LogicJSONArray hostArray = new LogicJSONArray();

                for (int i = 0; i < this.Hosts.Length; i++)
                {
                    hostArray.Add(new LogicJSONString(this.Hosts[i].ToString()));
                }

                jsonObject.Put("hosts", hostArray);
                jsonObject.Put("username", new LogicJSONString(this.Username));
                jsonObject.Put("password", new LogicJSONString(this.Password));

                return jsonObject;
            }
        }

        public class CouchbaseBucketEntry
        {
            public string Name { get; }
            public int ServerIndex { get; }

            public CouchbaseBucketEntry(LogicJSONString str)
            {
                string[] name = str.GetStringValue().Split(':');

                this.Name = name[0];
                this.ServerIndex = int.Parse(name[1]);
            }

            public LogicJSONString Save()
            {
                return new LogicJSONString(string.Format("{0}:{1}", this.Name, this.ServerIndex));
            }
        }
    }
}