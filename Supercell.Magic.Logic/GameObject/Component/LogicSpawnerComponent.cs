namespace Supercell.Magic.Logic.GameObject.Component
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Time;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicSpawnerComponent : LogicComponent
    {
        private LogicArrayList<int> m_spawned;
        private LogicMersenneTwisterRandom m_randomizer;
        private LogicTimer m_timer;
        private LogicGameObjectData m_spawnData;

        private readonly int m_radius;
        private readonly int m_intervalSeconds;
        private readonly int m_spawnCount;
        private readonly int m_maxSpawned;
        private readonly int m_maxLifetimeSpawns;
        private int m_lifeTimeSpawns;

        private bool m_initialSpawnDone;

        public LogicSpawnerComponent(LogicGameObject gameObject, LogicObstacleData spawnData, int radius, int intervalSeconds, int spawnCount, int maxSpawned,
                                   int maxLifetimeSpawns) : base(gameObject)
        {
            this.m_spawned = new LogicArrayList<int>();
            this.m_randomizer = new LogicMersenneTwisterRandom();

            this.m_spawnData = spawnData;
            this.m_radius = radius;
            this.m_intervalSeconds = intervalSeconds;
            this.m_spawnCount = spawnCount;
            this.m_maxSpawned = maxSpawned;
            this.m_maxLifetimeSpawns = maxLifetimeSpawns;

            this.m_randomizer.Initialize(this.m_parent.GetGlobalID());
            this.m_spawned.EnsureCapacity(maxSpawned);
        }

        public override void Destruct()
        {
            base.Destruct();

            if (this.m_timer != null)
            {
                this.m_timer.Destruct();
                this.m_timer = null;
            }

            this.m_spawnData = null;
            this.m_spawned = null;
            this.m_randomizer = null;
        }

        public override LogicComponentType GetComponentType()
        {
            return LogicComponentType.SPAWNER;
        }

        public override void Tick()
        {
            if (!this.m_initialSpawnDone)
            {
                this.Spawn();
                this.m_initialSpawnDone = true;
            }
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONBoolean initialSpawnDoneBoolean = jsonObject.GetJSONBoolean("initial_spawn_done");

            if (initialSpawnDoneBoolean != null)
            {
                this.m_initialSpawnDone = initialSpawnDoneBoolean.IsTrue();
            }

            if (this.m_timer != null)
            {
                this.m_timer.Destruct();
                this.m_timer = null;
            }

            this.m_timer = LogicTimer.GetLogicTimer(jsonObject, this.m_parent.GetLevel().GetLogicTime(), "spawn_timer", this.m_intervalSeconds);

            LogicJSONNumber lifetimeSpawnsNumber = jsonObject.GetJSONNumber("lifetime_spawns");

            if (lifetimeSpawnsNumber != null)
            {
                this.m_lifeTimeSpawns = lifetimeSpawnsNumber.GetIntValue();
            }

            LogicJSONArray spawnedArray = jsonObject.GetJSONArray("spawned");

            if (spawnedArray != null)
            {
                for (int i = 0; i < spawnedArray.Size(); i++)
                {
                    this.m_spawned.Add(spawnedArray.GetJSONNumber(i).GetIntValue());
                }
            }
        }

        public override void LoadFromSnapshot(LogicJSONObject jsonObject)
        {
            if (this.m_timer != null)
            {
                this.m_timer.Destruct();
                this.m_timer = null;
            }
        }

        public override void Save(LogicJSONObject jsonObject, int villageType)
        {
            LogicTimer.SetLogicTimer(jsonObject, this.m_timer, this.m_parent.GetLevel(), "spawn_timer");

            jsonObject.Put("initial_spawn_done", new LogicJSONBoolean(this.m_initialSpawnDone));
            jsonObject.Put("lifetime_spawns", new LogicJSONNumber(this.m_lifeTimeSpawns));

            LogicJSONArray jsonArray = new LogicJSONArray();

            for (int i = 0; i < this.m_spawned.Size(); i++)
            {
                jsonArray.Add(new LogicJSONNumber(this.m_spawned[i]));
            }

            jsonObject.Put("spawned", jsonArray);
        }

        public override void FastForwardTime(int time)
        {
            while (time > 0)
            {
                LogicGameObjectManager gameObjectManager = this.m_parent.GetGameObjectManager();

                for (int i = 0; i < this.m_spawned.Size(); i++)
                {
                    if (gameObjectManager.GetGameObjectByID(this.m_spawned[i]) == null)
                    {
                        this.m_spawned.Remove(i--);
                    }
                }

                if (this.m_lifeTimeSpawns < this.m_maxLifetimeSpawns && this.m_spawned.Size() < this.m_maxSpawned)
                {
                    if (this.m_timer == null)
                    {
                        this.m_timer = new LogicTimer();
                        this.m_timer.StartTimer(this.m_intervalSeconds, this.m_parent.GetLevel().GetLogicTime(), false, -1);
                    }

                    int remainingSeconds = this.m_timer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());

                    if (time < remainingSeconds)
                    {
                        break;
                    }

                    this.m_timer.FastForward(remainingSeconds);
                    this.Spawn();
                    this.m_timer.StartTimer(this.m_intervalSeconds, this.m_parent.GetLevel().GetLogicTime(), false, -1);

                    time -= remainingSeconds;
                }
                else
                {
                    if (this.m_timer != null)
                    {
                        this.m_timer.Destruct();
                        this.m_timer = null;
                    }

                    break;
                }
            }
        }

        private void Spawn()
        {
            int free = LogicMath.Min(LogicMath.Min(this.m_spawnCount, this.m_maxSpawned - this.m_spawned.Size()), this.m_maxLifetimeSpawns - this.m_lifeTimeSpawns);

            if (free > 0)
            {
                int x = this.m_parent.GetX();
                int y = this.m_parent.GetY();
                int tileX = this.m_parent.GetTileX();
                int tileY = this.m_parent.GetTileY();
                int width = this.m_parent.GetWidthInTiles();
                int height = this.m_parent.GetHeightInTiles();
                int levelWidth = this.m_parent.GetLevel().GetWidthInTiles();
                int levelHeight = this.m_parent.GetLevel().GetHeightInTiles();

                int startTileX = LogicMath.Clamp(tileX - this.m_radius, 0, levelWidth);
                int startTileY = LogicMath.Clamp(tileY - this.m_radius, 0, levelHeight);
                int endTileX = LogicMath.Clamp(tileX + this.m_radius + width, 0, levelWidth);
                int endTileY = LogicMath.Clamp(tileY + this.m_radius + height, 0, levelHeight);

                int radius = (this.m_radius << 9) * (this.m_radius << 9);
                int possibility = (endTileX - startTileX) * (endTileY - startTileY);

                LogicArrayList<LogicTile> spawnPoints = new LogicArrayList<LogicTile>(possibility);
                LogicTileMap tileMap = this.m_parent.GetLevel().GetTileMap();

                int spawnPointUpStartX = x + (width << 9);
                int spawnPointUpStartY = y + (height << 9);

                int tmp4 = y - 256 - (startTileY << 9);

                int startMidX = (startTileX << 9) | 256;
                int startMidY = (startTileY << 9) | 256;

                for (int i = startTileX, j = startMidX; i < endTileX; i++, j += 512)
                {
                    int tmp1 = j >= spawnPointUpStartX ? -spawnPointUpStartX + j + 1 : 0;
                    int tmp2 = j >= x ? tmp1 : x - j;

                    tmp2 *= tmp2;

                    for (int k = startTileY, l = startMidY, m = tmp4; k < endTileY; k++, l += 512, m -= 512)
                    {
                        LogicTile tile = tileMap.GetTile(i, k);

                        if (tile.GetGameObjectCount() == 0)
                        {
                            int tmp3 = y <= l ? l < spawnPointUpStartY ? 0 : -spawnPointUpStartY + l + 1 : m;

                            tmp3 *= tmp3;

                            if (tmp2 + tmp3 <= radius)
                            {
                                spawnPoints.Add(tile);
                            }
                        }
                    }
                }

                for (int i = free; i > 0 && spawnPoints.Size() > 0; i--, ++this.m_lifeTimeSpawns)
                {
                    int idx = this.m_randomizer.Rand(spawnPoints.Size());

                    LogicTile tile = spawnPoints[idx];
                    LogicGameObject gameObject = LogicGameObjectFactory.CreateGameObject(this.m_spawnData, this.m_parent.GetLevel(), this.m_parent.GetVillageType());

                    gameObject.SetInitialPosition(tile.GetX() << 9, tile.GetY() << 9);

                    this.m_parent.GetGameObjectManager().AddGameObject(gameObject, -1);
                    this.m_spawned.Add(gameObject.GetGlobalID());

                    spawnPoints.Remove(idx);
                }
            }
        }
    }
}