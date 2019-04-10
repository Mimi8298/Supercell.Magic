namespace Supercell.Magic.Servers.Core.Database
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    using StackExchange.Redis;

    using Supercell.Magic.Servers.Core.Settings;

    public class RedisDatabase
    {
        private readonly IDatabase m_database;

        private readonly ConnectionMultiplexer m_redis;
        private readonly byte[] m_removeIfScript;

        public RedisDatabase(string name)
        {
            if (!EnvironmentSettings.Redis.TryGetDatabase(name, out EnvironmentSettings.RedisSettings.RedisDatabaseEntry database))
                throw new Exception("Unknown redis database: " + name);

            this.m_redis = ConnectionMultiplexer.Connect(database.ConnectionString);
            this.m_database = this.m_redis.GetDatabase();

            EndPoint[] endPoints = this.m_redis.GetEndPoints();
            LuaScript luaScript = LuaScript.Prepare(@"
                local value = redis.call('GET', KEYS[1])
                if value == ARGV[1] then
                    return redis.call('DEL', KEYS[1]);
                end
                return false;");

            for (int i = 0; i < endPoints.Length; i++)
            {
                byte[] hash = luaScript.Load(this.m_redis.GetServer(endPoints[i])).Hash;

                if (hash.Length != 20)
                    throw new Exception("RedisDatabase.ctor: hash length != 20");

                if (this.m_removeIfScript != null)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        if (this.m_removeIfScript[j] != hash[j])
                            throw new Exception("RedisDatabase.ctor: hash mismatch");
                    }
                }
                else
                {
                    this.m_removeIfScript = hash;
                }
            }
        }

        public Task<bool> Exists(long id)
        {
            return this.m_database.KeyExistsAsync(id.ToString());
        }

        public Task<bool> Delete(long id)
        {
            return this.m_database.KeyDeleteAsync(id.ToString());
        }

        public Task<bool> Set(long id, string value)
        {
            return this.m_database.StringSetAsync(id.ToString(), value);
        }

        public Task<RedisValue> GetSet(long id, string value)
        {
            return this.m_database.StringGetSetAsync(id.ToString(), value);
        }

        public Task<RedisValue> Get(long id)
        {
            return this.m_database.StringGetAsync(id.ToString());
        }

        public Task<RedisValue[]> Get(RedisKey[] ids)
        {
            return this.m_database.StringGetAsync(ids);
        }

        public async void DeleteIfEquals(long id, string value)
        {
            await this.m_database.ScriptEvaluateAsync(this.m_removeIfScript, new RedisKey[]
            {
                id.ToString()
            }, new RedisValue[]
            {
                value
            });
        }
    }
}