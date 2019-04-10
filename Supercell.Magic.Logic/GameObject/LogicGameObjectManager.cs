namespace Supercell.Magic.Logic.GameObject
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.GameObject.Listener;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Mode;
    using Supercell.Magic.Logic.Time;
    using Supercell.Magic.Logic.Unit;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicGameObjectManager
    {
        private readonly int m_villageType;
        private readonly int[] m_gameObjectIds;

        private LogicLevel m_level;
        private LogicTileMap m_tileMap;
        private LogicUnitProduction m_unitProduction;
        private LogicUnitProduction m_spellProduction;
        private LogicGameObjectManagerListener m_listener;
        private LogicRandom m_obstacleRespawnRandom;
        private LogicRandom m_tallGrassRespawnRandom;
        private readonly LogicComponentManager m_componentManager;
        private readonly LogicArrayList<LogicGameObject>[] m_gameObjects;
        private readonly LogicArrayList<LogicBuilding> m_barracks;
        private readonly LogicArrayList<LogicBuilding> m_darkBarracks;
        private readonly LogicArrayList<int> m_obstacleDiamondsReward;
        private readonly LogicArrayList<int> m_obstacleDiamondsRewardVillage2;

        private LogicBuilding m_allianceCastle;
        private LogicBuilding m_clockTower;
        private LogicBuilding m_townHall;
        private LogicBuilding m_laboratory;
        private LogicBuilding m_spellForge;
        private LogicBuilding m_darkSpellForge;

        private LogicAlliancePortal m_alliancePortal;
        private LogicObstacle m_lootCart;
        private LogicVillageObject m_shipyard;
        private LogicVillageObject m_rowBoat;
        private LogicVillageObject m_clanGate;
        private readonly LogicObstacleData m_lootCartData;
        private LogicObstacleData m_bonusGemboxData;
        private LogicObstacleData m_specialObstacleData;

        private int m_secondsFromLastRespawn;
        private int m_secondsFromLastTallGrassRespawn;
        private int m_specialObstacleDropSecs;
        private int m_specialObstaclePeriodSecs;
        private int m_obstacleClearCounter;
        private int m_gemBoxDropSecs;
        private int m_gemBoxPeriodSecs;
        private int m_unitProductionCount;

        public LogicGameObjectManager(LogicTileMap tileMap, LogicLevel level, int villageType)
        {
            this.m_level = level;
            this.m_tileMap = tileMap;
            this.m_villageType = villageType;

            this.m_gameObjectIds = new int[LogicGameObject.GAMEOBJECT_TYPE_COUNT];
            this.m_gameObjects = new LogicArrayList<LogicGameObject>[LogicGameObject.GAMEOBJECT_TYPE_COUNT];

            for (int i = 0; i < LogicGameObject.GAMEOBJECT_TYPE_COUNT; i++)
            {
                this.m_gameObjects[i] = new LogicArrayList<LogicGameObject>(32);
            }

            this.m_obstacleDiamondsReward = new LogicArrayList<int>(20);
            this.m_obstacleDiamondsRewardVillage2 = new LogicArrayList<int>(20);

            for (int i = 0; i < 20; i++)
            {
                this.m_obstacleDiamondsReward.Add(0);
                this.m_obstacleDiamondsRewardVillage2.Add(0);
            }

            this.m_obstacleDiamondsReward[1] = 3;
            this.m_obstacleDiamondsReward[3] = 1;
            this.m_obstacleDiamondsReward[4] = 2;
            this.m_obstacleDiamondsReward[6] = 1;
            this.m_obstacleDiamondsReward[7] = 1;
            this.m_obstacleDiamondsReward[10] = 3;
            this.m_obstacleDiamondsReward[11] = 1;
            this.m_obstacleDiamondsReward[13] = 2;
            this.m_obstacleDiamondsReward[14] = 2;
            this.m_obstacleDiamondsReward[17] = 3;
            this.m_obstacleDiamondsReward[19] = 1;
            this.m_obstacleDiamondsRewardVillage2[17] = -1;
            this.m_obstacleDiamondsRewardVillage2[10] = -1;
            this.m_obstacleDiamondsRewardVillage2[11] = -1;
            this.m_obstacleDiamondsRewardVillage2[3] = 2;
            this.m_obstacleDiamondsRewardVillage2[4] = 1;
            this.m_obstacleDiamondsRewardVillage2[5] = 1;
            this.m_obstacleDiamondsRewardVillage2[6] = 1;
            this.m_obstacleDiamondsRewardVillage2[13] = -1;
            this.m_obstacleDiamondsRewardVillage2[19] = -1;

            this.m_barracks = new LogicArrayList<LogicBuilding>();
            this.m_darkBarracks = new LogicArrayList<LogicBuilding>();
            this.m_bonusGemboxData = LogicDataTables.GetObstacleByName("Bonus Gembox", null);
            this.m_lootCartData = LogicDataTables.GetObstacleByName("LootCart", null);
            this.m_componentManager = new LogicComponentManager(level);
            this.m_listener = new LogicGameObjectManagerListener();
            this.m_obstacleRespawnRandom = new LogicRandom();
            this.m_tallGrassRespawnRandom = new LogicRandom();

            if (LogicDataTables.GetGlobals().UseNewTraining())
            {
                this.m_unitProduction = new LogicUnitProduction(level, LogicDataType.CHARACTER, this.m_villageType);
                this.m_spellProduction = new LogicUnitProduction(level, LogicDataType.SPELL, this.m_villageType);
            }
        }

        public void Destruct()
        {
            for (int i = 0; i < LogicGameObject.GAMEOBJECT_TYPE_COUNT; i++)
            {
                LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[i];

                if (gameObjects != null)
                {
                    for (int j = gameObjects.Size() - 1; j >= 0; j--)
                    {
                        gameObjects[j].Destruct();
                        gameObjects.Remove(j);
                    }

                    this.m_gameObjects[i] = null;
                }
            }

            this.m_barracks.Clear();
            this.m_darkBarracks.Clear();

            if (this.m_unitProduction != null)
            {
                this.m_unitProduction.Destruct();
                this.m_unitProduction = null;
            }

            if (this.m_spellProduction != null)
            {
                this.m_spellProduction.Destruct();
                this.m_spellProduction = null;
            }

            this.m_listener = null;
            this.m_obstacleRespawnRandom = null;
            this.m_tallGrassRespawnRandom = null;
            this.m_level = null;
            this.m_tileMap = null;
            this.m_allianceCastle = null;
            this.m_clockTower = null;
            this.m_townHall = null;
            this.m_laboratory = null;
            this.m_spellForge = null;
            this.m_darkSpellForge = null;
            this.m_lootCart = null;
            this.m_shipyard = null;
            this.m_rowBoat = null;
            this.m_clanGate = null;
            this.m_bonusGemboxData = null;
            this.m_specialObstacleData = null;
        }

        public void Init(LogicLevel level, int villageType)
        {
            // SetAttackValues.
        }

        public void AddGameObject(LogicGameObject gameObject, int globalId)
        {
            LogicGameObjectType gameObjectType = gameObject.GetGameObjectType();

            if (!gameObject.GetData().IsEnabledInVillageType(this.m_villageType))
            {
                Debugger.Error(string.Format("Invalid item in level for villageType {0} DataId: {1}", this.m_villageType, gameObject.GetData().GetGlobalID()));
            }

            if (globalId == -1)
            {
                globalId = this.GenerateGameObjectGlobalID(gameObject);
            }
            else
            {
                int table = GlobalID.GetClassID(globalId);
                int idx = GlobalID.GetInstanceID(globalId);

                if (table - 500 != (int) gameObjectType)
                {
                    Debugger.Error(string.Format("LogicGameObjectManager::addGameObject with global ID {0}, doesn't have right index", globalId));
                }

                if (this.GetGameObjectByID(globalId) != null)
                {
                    Debugger.Error(string.Format("LogicGameObjectManager::addGameObject with global ID {0}, global ID already taken", globalId));
                }

                if (this.m_gameObjectIds[(int) gameObjectType] <= idx)
                {
                    this.m_gameObjectIds[(int) gameObjectType] = idx + 1;
                }
            }

            gameObject.SetGlobalID(globalId);

            LogicRandom random = new LogicRandom(this.m_level.GetGameMode().GetStartTime() + globalId);
            random.Rand(0x7fffffff);
            random.Rand(0x7fffffff);
            random.Rand(0x7fffffff);
            gameObject.SetSeed(random.Rand(0x7fffffff));

            if (gameObjectType == LogicGameObjectType.BUILDING)
            {
                LogicBuilding building = (LogicBuilding) gameObject;
                LogicBuildingData buildingData = building.GetBuildingData();

                if (buildingData.IsAllianceCastle())
                {
                    this.m_allianceCastle = building;
                }

                if (buildingData.GetUnitProduction(0) >= 1)
                {
                    if (!buildingData.IsForgesSpells())
                    {
                        if (buildingData.GetProducesUnitsOfType() == 1)
                        {
                            this.m_barracks.Add(building);
                        }
                        else if (buildingData.GetProducesUnitsOfType() == 2)
                        {
                            this.m_darkBarracks.Add(building);
                        }
                    }
                }

                if (buildingData.IsClockTower())
                {
                    this.m_clockTower = building;
                }

                if (buildingData.IsTownHall() || buildingData.IsTownHallVillage2())
                {
                    this.m_townHall = building;
                }

                if (buildingData.IsWorkerBuilding() || buildingData.IsTownHallVillage2())
                {
                    this.m_level.GetWorkerManagerAt(this.m_villageType).IncreaseWorkerCount();
                }

                if (buildingData.IsLaboratory())
                {
                    this.m_laboratory = building;
                }

                if (buildingData.GetUnitProduction(0) >= 1)
                {
                    this.m_unitProductionCount += 1;
                }

                if (buildingData.IsForgesSpells())
                {
                    int unitsOfType = buildingData.GetProducesUnitsOfType();

                    if (unitsOfType == 1)
                    {
                        this.m_darkSpellForge = building;
                    }
                    else
                    {
                        this.m_spellForge = building;
                    }
                }
            }
            else if (gameObjectType == LogicGameObjectType.OBSTACLE)
            {
                LogicObstacle obstacleObject = (LogicObstacle) gameObject;
                LogicObstacleData obstacleObjectData = obstacleObject.GetObstacleData();

                if (obstacleObjectData.IsLootCart())
                {
                    this.m_lootCart = obstacleObject;
                }
            }
            else if (gameObjectType == LogicGameObjectType.BUILDING)
            {
                this.m_alliancePortal = (LogicAlliancePortal) gameObject;
            }
            else if (gameObjectType == LogicGameObjectType.VILLAGE_OBJECT)
            {
                LogicVillageObject villageObject = (LogicVillageObject) gameObject;
                LogicVillageObjectData villageObjectData = villageObject.GetVillageObjectData();

                if (villageObjectData.IsShipyard())
                {
                    this.m_shipyard = villageObject;
                }

                if (villageObjectData.IsRowBoat())
                {
                    this.m_rowBoat = villageObject;
                }

                if (villageObjectData.IsClanGate())
                {
                    this.m_clanGate = villageObject;
                }
            }

            this.m_gameObjects[(int) gameObject.GetGameObjectType()].Add(gameObject);

            if (this.m_level.GetVillageType() == this.m_villageType)
            {
                this.m_tileMap.AddGameObject(gameObject);

                bool success = true;

                if (LogicDataTables.GetGlobals().UseTeslaTriggerCommand())
                {
                    if (gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING &&
                        gameObject.IsHidden() &&
                        this.m_level.GetState() != 1 &&
                        this.m_level.GetState() != 4)
                    {
                        success = false;
                    }
                }

                if (LogicDataTables.GetGlobals().UseTrapTriggerCommand())
                {
                    if (gameObject.GetGameObjectType() == LogicGameObjectType.TRAP &&
                        this.m_level.GetState() != 1 &&
                        this.m_level.GetState() != 4)
                    {
                        success = false;
                    }
                }

                if (success)
                {
                    this.m_listener.AddGameObject(gameObject);
                }
            }
        }

        public void AddLootCart()
        {
            if (this.m_lootCartData != null)
            {
                if (this.CreateSpecialObstacle(this.m_lootCartData, true))
                {
                    LogicArrayList<LogicGameObject> obstacles = this.m_gameObjects[(int) LogicGameObjectType.OBSTACLE];

                    for (int i = 0; i < obstacles.Size(); i++)
                    {
                        LogicObstacle obstacle = (LogicObstacle) obstacles[i];

                        if (obstacle.GetData() == this.m_lootCartData)
                        {
                            this.m_lootCart = obstacle;
                        }
                    }
                }
                else
                {
                    Debugger.Warning("LogicGameObjectManager::addLootCart failed");
                }
            }
        }

        public void RemoveGameObject(LogicGameObject gameObject)
        {
            int index = -1;
            LogicGameObjectType gameObjectType = gameObject.GetGameObjectType();

            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[(int) gameObjectType];

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                if (gameObjects[i].GetGlobalID() == gameObject.GetGlobalID())
                {
                    index = i;
                    break;
                }
            }

            gameObjects.Remove(index);

            this.m_level.GetTileMap().RemoveGameObject(gameObject);
            this.m_listener.RemoveGameObject(gameObject);

            if (this.m_townHall == gameObject)
            {
                this.m_townHall = null;
            }

            if (this.m_clockTower == gameObject)
            {
                this.m_clockTower = null;
            }

            if (gameObjectType == LogicGameObjectType.BUILDING)
            {
                LogicBuildingData buildingData = ((LogicBuilding) gameObject).GetBuildingData();

                if (buildingData.IsWorkerBuilding() || buildingData.IsTownHallVillage2())
                {
                    this.m_level.GetWorkerManagerAt(this.m_villageType).DecreaseWorkerCount();
                }

                if (buildingData.GetUnitProduction(0) > 0)
                {
                    this.m_unitProductionCount -= 1;

                    if (!buildingData.IsForgesSpells())
                    {
                        if (buildingData.GetProducesUnitsOfType() == 1)
                        {
                            for (int i = 0; i < this.m_barracks.Size(); i++)
                            {
                                if (this.m_barracks[i] == gameObject)
                                {
                                    this.m_barracks.Remove(i);
                                    break;
                                }
                            }
                        }
                        else if (buildingData.GetProducesUnitsOfType() == 2)
                        {
                            for (int i = 0; i < this.m_darkBarracks.Size(); i++)
                            {
                                if (this.m_darkBarracks[i] == gameObject)
                                {
                                    this.m_darkBarracks.Remove(i);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (this.m_allianceCastle == gameObject)
            {
                this.m_allianceCastle = null;
            }

            if (this.m_laboratory == gameObject)
            {
                this.m_laboratory = null;
            }

            if (this.m_spellForge == gameObject)
            {
                this.m_spellForge = null;
            }

            if (this.m_darkSpellForge == gameObject)
            {
                this.m_darkSpellForge = null;
            }

            gameObject.Destruct();

            this.RemoveGameObjectReferences(gameObject);
        }

        private void RemoveGameObjectReferences(LogicGameObject gameObject)
        {
            for (int i = 0; i < LogicGameObject.GAMEOBJECT_TYPE_COUNT; i++)
            {
                LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[i];

                for (int j = 0; j < gameObjects.Size(); j++)
                {
                    gameObjects[j].RemoveGameObjectReferences(gameObject);
                }
            }

            for (int i = 0; i < 2; i++)
            {
                this.m_level.GetWorkerManagerAt(i).RemoveGameObjectReference(gameObject);
            }

            this.m_componentManager.RemoveGameObjectReferences(gameObject);

            if (this.m_lootCart == gameObject)
            {
                this.m_lootCart = null;
            }
        }

        public int GenerateGameObjectGlobalID(LogicGameObject gameObject)
        {
            LogicGameObjectType type = gameObject.GetGameObjectType();

            if ((int) type >= LogicGameObject.GAMEOBJECT_TYPE_COUNT)
            {
                Debugger.Error("LogicGameObjectManager::generateGameObjectGlobalID(). Index is out of bounds.");
            }

            return GlobalID.CreateGlobalID((int) type + 500, this.m_gameObjectIds[(int) type]++);
        }

        public LogicArrayList<LogicGameObject> GetGameObjects(LogicGameObjectType index)
        {
            return this.m_gameObjects[(int) index];
        }

        public LogicGameObject GetGameObjectByID(int globalId)
        {
            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[GlobalID.GetClassID(globalId) - 500];

            for (int i = 0, cnt = gameObjects.Size(); i < cnt; i++)
            {
                LogicGameObject gameObject = gameObjects[i];

                if (gameObject.GetGlobalID() == globalId)
                {
                    return gameObject;
                }
            }

            return null;
        }

        public LogicGameObject GetGameObjectByIndex(int idx)
        {
            for (int i = 0, sum = 0; i < LogicGameObject.GAMEOBJECT_TYPE_COUNT; i++)
            {
                LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[i];

                if (sum + gameObjects.Size() > idx)
                {
                    return gameObjects[idx - sum];
                }

                sum += gameObjects.Size();
            }

            return null;
        }

        public int GetNumGameObjects()
        {
            int count = 0;

            for (int i = 0; i < LogicGameObject.GAMEOBJECT_TYPE_COUNT; i++)
            {
                count += this.m_gameObjects[i].Size();
            }

            return count;
        }

        public int GetGameObjectCountByData(LogicData data)
        {
            int cnt = 0;

            for (int i = 0; i < LogicGameObject.GAMEOBJECT_TYPE_COUNT; i++)
            {
                LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[i];

                if (gameObjects.Size() > 0)
                {
                    if (gameObjects[0].GetData().GetDataType() == data.GetDataType())
                    {
                        for (int j = 0; j < gameObjects.Size(); j++)
                        {
                            LogicGameObject gameObject = gameObjects[j];

                            if (gameObject.GetData() == data)
                            {
                                ++cnt;
                            }
                        }
                    }
                }
            }

            return cnt;
        }

        public int GetGearUpBuildingCount()
        {
            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[(int) LogicGameObjectType.BUILDING];
            int cnt = 0;

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                cnt += ((LogicBuilding) gameObjects[i]).GetGearLevel() > 0 ? 1 : 0;
            }

            return cnt;
        }

        public int GetGearUpBuildingCount(LogicBuildingData data)
        {
            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[(int) LogicGameObjectType.BUILDING];
            int cnt = 0;

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                LogicBuilding building = (LogicBuilding) gameObjects[i];

                if (building.GetData() == data && (building.GetGearLevel() > 0 || building.IsGearing()))
                {
                    cnt += 1;
                }
            }

            return cnt;
        }

        public int GetTallGrassCount()
        {
            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[(int) LogicGameObjectType.OBSTACLE];
            int count = 0;

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                LogicObstacle obstacle = (LogicObstacle) gameObjects[i];

                if (obstacle.GetObstacleData().IsTallGrass())
                {
                    ++count;
                }
            }

            return count;
        }

        public int GetHighestWallIndex(LogicBuildingData data)
        {
            int count = 1;
            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[(int) LogicGameObjectType.BUILDING];

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                count += gameObjects[i].GetData() == data ? 1 : 0;
            }

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                LogicBuilding building = (LogicBuilding) gameObjects[i];

                if (building.GetWallIndex() == count)
                {
                    count += 1;
                    i = -1;
                }
            }

            return count;
        }

        public int GetHighestBuildingLevel(LogicBuildingData data)
        {
            return this.GetHighestBuildingLevel(data, true);
        }

        public int GetBarrackCount()
        {
            if (this.m_barracks != null)
            {
                return this.m_barracks.Size();
            }

            return 0;
        }

        public int GetDarkBarrackCount()
        {
            if (this.m_darkBarracks != null)
            {
                return this.m_darkBarracks.Size();
            }

            return 0;
        }

        public LogicGameObject GetBarrack(int idx)
        {
            return this.m_barracks[idx];
        }

        public LogicGameObject GetDarkBarrack(int idx)
        {
            return this.m_darkBarracks[idx];
        }

        public int GetHighestBuildingLevel(LogicBuildingData data, bool completeProduction)
        {
            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[(int) LogicGameObjectType.BUILDING];
            int maxLevel = -1;

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                if (gameObjects[i].GetData() == data)
                {
                    LogicBuilding building = (LogicBuilding) gameObjects[i];

                    if (building.IsConstructing())
                    {
                        if (!building.IsUpgrading())
                        {
                            continue;
                        }
                    }

                    if (!building.IsLocked())
                    {
                        int upgLevel = building.GetUpgradeLevel();

                        if (completeProduction && building.IsConstructing())
                        {
                            ++upgLevel;
                        }

                        maxLevel = LogicMath.Max(maxLevel, upgLevel);
                    }
                }
            }

            return maxLevel;
        }

        public LogicBuilding GetHighestBuilding(LogicBuildingData data)
        {
            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[(int) LogicGameObjectType.BUILDING];
            LogicBuilding highestBuilding = null;

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                if (gameObjects[i].GetData() == data)
                {
                    LogicBuilding building = (LogicBuilding) gameObjects[i];

                    if (building.IsConstructing())
                    {
                        if (!building.IsUpgrading())
                        {
                            continue;
                        }
                    }

                    if (!building.IsLocked())
                    {
                        int upgLevel = building.GetUpgradeLevel();

                        if (highestBuilding != null)
                        {
                            if (upgLevel > highestBuilding.GetUpgradeLevel())
                            {
                                highestBuilding = building;
                            }
                        }
                        else
                        {
                            highestBuilding = building;
                        }
                    }
                }
            }

            return highestBuilding;
        }

        public LogicComponentManager GetComponentManager()
        {
            return this.m_componentManager;
        }

        public LogicBuilding GetTownHall()
        {
            return this.m_townHall;
        }

        public LogicUnitProduction GetSpellProduction()
        {
            return this.m_spellProduction;
        }

        public LogicUnitProduction GetUnitProduction()
        {
            return this.m_unitProduction;
        }

        public LogicObstacle GetLootCart()
        {
            return this.m_lootCart;
        }

        public void GetChecksum(ChecksumHelper checksum, bool includeGameObjects)
        {
            checksum.StartObject("LogicGameObjectManager");
            checksum.WriteValue("numGameObjects", this.GetNumGameObjects());

            if (includeGameObjects)
            {
                for (int i = 0; i < LogicGameObject.GAMEOBJECT_TYPE_COUNT; i++)
                {
                    checksum.StartArray("type" + i);

                    for (int j = 0; j < this.m_gameObjects[i].Size(); j++)
                    {
                        this.m_gameObjects[i][j].GetChecksum(checksum, true);
                    }

                    checksum.EndArray();
                }
            }
            else
            {
                checksum.StartArray("type0");

                LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[(int) LogicGameObjectType.BUILDING];

                for (int i = 0; i < gameObjects.Size(); i++)
                {
                    gameObjects[i].GetChecksum(checksum, false);
                }

                checksum.EndArray();
            }

            checksum.EndObject();
        }

        public LogicGameObjectManagerListener GetListener()
        {
            return this.m_listener;
        }

        public void SetListener(LogicGameObjectManagerListener listener)
        {
            this.m_listener = listener;
        }

        public void Village2TownHallFixed()
        {
            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[(int) LogicGameObjectType.BUILDING];

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                LogicBuilding building = (LogicBuilding) gameObjects[i];

                if (building.IsLocked())
                {
                    if (building.GetBuildingData().GetRequiredTownHallLevel(0) <= 1)
                    {
                        building.FinishConstruction(true, true);

                        LogicVillage2UnitComponent village2UnitComponent = building.GetVillage2UnitComponent();

                        if (village2UnitComponent != null)
                        {
                            village2UnitComponent.TrainUnit(LogicDataTables.GetGlobals().GetVillage2StartUnitData());
                            village2UnitComponent.ProductionCompleted();
                        }
                    }
                }
            }
        }

        public void LoadVillageObjects()
        {
            LogicDataTable dataTable = LogicDataTables.GetTable(LogicDataType.VILLAGE_OBJECT);

            for (int i = 0; i < dataTable.GetItemCount(); i++)
            {
                LogicVillageObjectData data = (LogicVillageObjectData) dataTable.GetItemAt(i);

                if (data.IsEnabledInVillageType(this.m_villageType) && !data.IsDisabled() && this.GetGameObjectCountByData(data) == 0)
                {
                    LogicVillageObject villageObject = (LogicVillageObject) LogicGameObjectFactory.CreateGameObject(data, this.m_level, this.m_villageType);
                    villageObject.SetInitialPosition((data.GetTileX100() << 9) / 100, (data.GetTileY100() << 9) / 100);
                    this.AddGameObject(villageObject, -1);
                }
            }
        }

        public void LoadingFinished()
        {
            this.RespawnObstacles();

            LogicArrayList<LogicGameObject> buildings = this.m_gameObjects[(int) LogicGameObjectType.BUILDING];

            for (int i = 0; i < buildings.Size(); i++)
            {
                buildings[i].LoadingFinished();
            }

            LogicArrayList<LogicGameObject> obstacles = this.m_gameObjects[(int) LogicGameObjectType.OBSTACLE];

            for (int i = 0; i < obstacles.Size(); i++)
            {
                obstacles[i].LoadingFinished();
            }

            LogicArrayList<LogicGameObject> traps = this.m_gameObjects[(int) LogicGameObjectType.TRAP];

            for (int i = 0; i < traps.Size(); i++)
            {
                traps[i].LoadingFinished();
            }

            LogicArrayList<LogicGameObject> decos = this.m_gameObjects[(int) LogicGameObjectType.DECO];

            for (int i = 0; i < decos.Size(); i++)
            {
                decos[i].LoadingFinished();
            }

            LogicArrayList<LogicGameObject> villageObjects = this.m_gameObjects[(int) LogicGameObjectType.VILLAGE_OBJECT];

            for (int i = 0; i < villageObjects.Size(); i++)
            {
                villageObjects[i].LoadingFinished();
            }

            if (LogicDataTables.GetGlobals().UseNewTraining())
            {
                this.m_unitProduction.LoadingFinished();
                this.m_spellProduction.LoadingFinished();
            }
        }

        public void FastForwardTime(int secs)
        {
            this.m_secondsFromLastRespawn += secs;
            this.m_secondsFromLastTallGrassRespawn += secs;
            this.m_gemBoxDropSecs -= secs;
            this.m_specialObstacleDropSecs -= secs;

            if (secs > 0)
            {
                int secsSinceLastMaintenance = this.m_level.GetGameMode().GetSecondsSinceLastMaintenance();
                int offlineServerTime = -1;

                if (secsSinceLastMaintenance > 0)
                {
                    offlineServerTime = secs - secsSinceLastMaintenance;
                }

                int idx = 0;

                do
                {
                    bool stopBoost;

                    int maxFastForwardTime = secs;
                    int fastForwardTime = 1;

                    if (idx == 999)
                    {
                        Debugger.Warning("LogicGameObjectManager::fastForwardTime - Pass limit reached");
                    }
                    else
                    {
                        for (int i = 0; i < LogicGameObject.GAMEOBJECT_TYPE_COUNT; i++)
                        {
                            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[i];

                            for (int j = 0; j < gameObjects.Size(); j++)
                            {
                                int tmp = gameObjects[j].GetMaxFastForwardTime();

                                if (tmp >= 0)
                                {
                                    maxFastForwardTime = LogicMath.Min(maxFastForwardTime, tmp);
                                }
                            }
                        }
                    }

                    if (offlineServerTime < 1)
                    {
                        stopBoost = false;

                        if (offlineServerTime == 0)
                        {
                            stopBoost = true;
                            offlineServerTime = -1;
                        }
                    }
                    else
                    {
                        maxFastForwardTime = LogicMath.Min(maxFastForwardTime, offlineServerTime);
                        offlineServerTime -= maxFastForwardTime;
                        stopBoost = false;
                    }

                    if (maxFastForwardTime > 0)
                    {
                        fastForwardTime = maxFastForwardTime;
                    }

                    if (fastForwardTime > secs)
                    {
                        fastForwardTime = secs;
                    }

                    for (int i = 0; i < LogicGameObject.GAMEOBJECT_TYPE_COUNT; i++)
                    {
                        LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[i];

                        for (int j = 0; j < gameObjects.Size(); j++)
                        {
                            LogicGameObject gameObject = gameObjects[j];

                            if (stopBoost)
                            {
                                gameObject.StopBoost();
                            }

                            gameObjects[j].FastForwardTime(fastForwardTime);
                        }
                    }

                    LogicArrayList<LogicGameObject> buildings = this.m_gameObjects[(int) LogicGameObjectType.BUILDING];

                    for (int i = 0; i < buildings.Size(); i++)
                    {
                        buildings[i].FastForwardBoost(fastForwardTime);
                    }

                    if (LogicDataTables.GetGlobals().UseNewTraining())
                    {
                        if (stopBoost)
                        {
                            this.m_unitProduction.StopBoost();
                            this.m_spellProduction.StopBoost();
                        }

                        this.m_unitProduction.FastForwardTime(fastForwardTime);
                        this.m_spellProduction.FastForwardTime(fastForwardTime);
                    }

                    if (idx++ > 998)
                    {
                        break;
                    }

                    secs -= fastForwardTime;
                } while (secs > 0);
            }

            this.RespawnObstacles();
        }

        public int IncreaseObstacleClearCounter(int lootMultiplier)
        {
            this.m_obstacleClearCounter = (this.m_obstacleClearCounter + 1) % this.m_obstacleDiamondsReward.Size();
            int diamondsReward = this.m_obstacleDiamondsReward[this.m_obstacleClearCounter];

            if (lootMultiplier >= 2)
            {
                diamondsReward = this.m_obstacleDiamondsRewardVillage2[this.m_obstacleClearCounter] + lootMultiplier * diamondsReward;
            }

            return diamondsReward;
        }

        public void GenerateNextGemboxDropTime(bool clamp)
        {
            int random = this.m_obstacleRespawnRandom.Rand(this.m_bonusGemboxData.GetAppearancePeriodHours());
            int timeToGemboxDrop = this.m_gemBoxPeriodSecs + 3600 * random;

            if (clamp)
            {
                int minTime = 3600 * this.m_bonusGemboxData.GetMinRespawnTimeHours();

                if (timeToGemboxDrop < minTime)
                {
                    timeToGemboxDrop = minTime;
                }
            }

            this.m_gemBoxDropSecs = timeToGemboxDrop;
            this.m_gemBoxPeriodSecs = 3600 * (this.m_bonusGemboxData.GetAppearancePeriodHours() - random);
        }

        public void RespawnObstacles()
        {
            int villageType = this.m_level.GetVillageType();
            int matchType = this.m_level.GetMatchType();

            if (villageType == this.m_villageType && matchType != 3 && matchType != 7)
            {
                if (villageType == 0 && matchType == 0)
                {
                    if (this.m_level.GetMatchType() == 0)
                    {
                        this.Village1RespawnObstacle();

                        if (this.m_bonusGemboxData != null && this.m_gemBoxDropSecs <= 0 && this.m_bonusGemboxData.GetLootCount() > 0)
                        {
                            this.CreateSpecialObstacle(this.m_bonusGemboxData, true);
                            this.GenerateNextGemboxDropTime(true);
                        }

                        if (this.m_specialObstacleData != null && this.m_specialObstacleDropSecs <= 0 && this.m_specialObstacleData.GetLootCount() > 0)
                        {
                            this.CreateSpecialObstacle(this.m_specialObstacleData, false);

                            int rnd = this.m_obstacleRespawnRandom.Rand(this.m_specialObstacleData.GetAppearancePeriodHours());

                            this.m_specialObstacleDropSecs = 3600 * rnd + this.m_specialObstaclePeriodSecs;
                            this.m_specialObstaclePeriodSecs = 3600 * (this.m_specialObstacleData.GetAppearancePeriodHours() - rnd);
                        }
                    }
                }
                else if (villageType == 1 && matchType != 5)
                {
                    this.Village2RespawnObstacles();
                }
            }
        }

        public void Village1RespawnObstacle()
        {
            if (this.m_villageType == 0 && this.m_level.GetVillageType() == 0)
            {
                int obstacleRespawnTime = LogicDataTables.GetGlobals().GetObstacleRespawnSecs();
                int obstacleMaxCount = LogicDataTables.GetGlobals().GetObstacleMaxCount();

                int tombStoneCount = this.m_level.GetTombStoneCount();
                int tallGrassCount = this.m_level.GetTallGrassCount();

                if (this.m_secondsFromLastRespawn > obstacleRespawnTime)
                {
                    int ignoreCount = tombStoneCount + tallGrassCount;

                    do
                    {
                        int count = this.m_gameObjects[(int) LogicGameObjectType.OBSTACLE].Size() - ignoreCount;

                        if (count >= obstacleMaxCount)
                        {
                            this.m_secondsFromLastRespawn = 0;
                            break;
                        }

                        this.Village1CreateObstacle();
                        this.m_secondsFromLastRespawn -= obstacleRespawnTime;
                    } while (this.m_secondsFromLastRespawn > obstacleRespawnTime);
                }
            }
        }

        public void Village2RespawnObstacles()
        {
            if (this.m_villageType == 1 && this.m_level.GetVillageType() == 1)
            {
                int respawnObstacleSecs = LogicDataTables.GetGlobals().GetObstacleRespawnSecs();

                while (this.m_secondsFromLastRespawn > respawnObstacleSecs)
                {
                    this.Village2CreateObstacle();
                    this.m_secondsFromLastRespawn -= respawnObstacleSecs;
                }

                this.RespawnTallGrass();
            }
        }

        public void RespawnTallGrass()
        {
            int oldBarbarianStatueCount = this.GetGameObjectCountByData(LogicDataTables.GetDecoByName("Old Barbarian Statue", null));
            int maxTallGrassCount = oldBarbarianStatueCount > 0 ? 50 : 25;
            int tallGrassRespawnSecs = LogicDataTables.GetGlobals().GetTallGrassRespawnSecs();

            while (this.m_secondsFromLastTallGrassRespawn > tallGrassRespawnSecs)
            {
                if (this.GetTallGrassCount() >= maxTallGrassCount)
                {
                    this.m_secondsFromLastTallGrassRespawn = 0;
                    break;
                }

                this.CreateTallGrass();
                this.m_secondsFromLastTallGrassRespawn -= tallGrassRespawnSecs;
            }
        }

        public bool CreateSpecialObstacle(LogicObstacleData data, bool oneOnly)
        {
            if (oneOnly)
            {
                LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[(int) LogicGameObjectType.OBSTACLE];

                for (int i = 0; i < gameObjects.Size(); i++)
                {
                    if (gameObjects[i].GetData() == data)
                    {
                        return false;
                    }
                }
            }

            bool created = this.RandomlyPlaceObstacle(data);

            if (!created)
            {
                created = this.CreateObstacleIfAnyPlaceExists(data, data.GetWidth(), data.GetHeight());
            }

            return created;
        }

        public bool PlaceGameObjectIfAnyFreeSpaceExists(LogicGameObject gameObject, int width, int height)
        {
            int levelWidth = this.m_level.GetWidthInTiles();
            int levelHeight = this.m_level.GetHeightInTiles();
            int possibility = levelWidth * levelHeight;
            int x = levelWidth / 2;
            int y = levelHeight / 2;

            if (possibility > 0)
            {
                do
                {
                    if (!this.m_level.IsValidPlaceForBuilding(x, y, width, height, null))
                    {
                        if (++x + width > levelWidth)
                        {
                            if (++y + height > levelHeight)
                            {
                                y = 0;
                            }

                            x = 0;
                        }
                    }
                    else
                    {
                        gameObject.SetInitialPosition(x << 9, y << 9);
                        return true;
                    }
                } while (--possibility > 0);
            }

            return false;
        }

        public void Village1CreateObstacle()
        {
            if (this.m_villageType != 0)
            {
                Debugger.Warning("invalid village type home!");
            }

            if (this.m_level.GetVillageType() != 0)
            {
                Debugger.Warning("invalid village type home (2)!");
            }

            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.OBSTACLE);
            int respawnWeights = 0;

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicObstacleData obstacleData = (LogicObstacleData) table.GetItemAt(i);

                if (obstacleData.IsEnabledInVillageType(this.m_villageType))
                {
                    respawnWeights += obstacleData.GetRespawnWeight();
                }
            }

            int rnd = this.m_obstacleRespawnRandom.Rand(respawnWeights);
            LogicObstacleData respawnObstacleData = null;

            for (int i = 0, weights = 0; i < table.GetItemCount(); i++)
            {
                LogicObstacleData obstacleData = (LogicObstacleData) table.GetItemAt(i);

                if (obstacleData.IsEnabledInVillageType(this.m_villageType))
                {
                    weights += obstacleData.GetRespawnWeight();

                    if (weights > rnd)
                    {
                        respawnObstacleData = obstacleData;
                        break;
                    }
                }
            }

            if (respawnObstacleData != null && respawnObstacleData.IsEnabledInVillageType(this.m_villageType))
            {
                this.RandomlyPlaceObstacle(respawnObstacleData);
            }
        }

        public void Village2CreateObstacle()
        {
            if (this.m_villageType != 1)
            {
                Debugger.Warning("invalid village type home!");
            }

            if (this.m_level.GetVillageType() != 1)
            {
                Debugger.Warning("invalid village type home (2)!");
            }

            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.OBSTACLE);
            int respawnWeights = 0;

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicObstacleData obstacleData = (LogicObstacleData) table.GetItemAt(i);

                if (obstacleData.IsEnabledInVillageType(this.m_villageType) &&
                    obstacleData.GetVillage2RespawnCount() > this.GetGameObjectCountByData(obstacleData))
                {
                    respawnWeights += obstacleData.GetRespawnWeight();
                }
            }

            if (respawnWeights > 0)
            {
                int rnd = this.m_obstacleRespawnRandom.Rand(respawnWeights);
                LogicObstacleData respawnObstacleData = null;

                for (int i = 0, weights = 0; i < table.GetItemCount(); i++)
                {
                    LogicObstacleData obstacleData = (LogicObstacleData) table.GetItemAt(i);

                    if (obstacleData.IsEnabledInVillageType(this.m_villageType) &&
                        obstacleData.GetVillage2RespawnCount() > this.GetGameObjectCountByData(obstacleData))
                    {
                        weights += obstacleData.GetRespawnWeight();

                        if (weights > rnd)
                        {
                            respawnObstacleData = obstacleData;
                            break;
                        }
                    }
                }

                if (respawnObstacleData != null && respawnObstacleData.IsEnabledInVillageType(this.m_villageType))
                {
                    this.RandomlyPlaceObstacle(respawnObstacleData);
                }
            }
        }

        public void CreateTallGrass()
        {
            LogicDataTable obstacleTable = LogicDataTables.GetTable(LogicDataType.OBSTACLE);
            int respawnWeights = 0;

            for (int i = 0; i < obstacleTable.GetItemCount(); i++)
            {
                LogicObstacleData obstacleData = (LogicObstacleData) obstacleTable.GetItemAt(i);

                if (obstacleData.IsTallGrass())
                {
                    respawnWeights += obstacleData.GetRespawnWeight();
                }
            }

            int rnd = this.m_obstacleRespawnRandom.Rand(respawnWeights);
            LogicObstacleData tallGrassData = null;

            for (int i = 0, weights = 0; i < obstacleTable.GetItemCount(); i++)
            {
                LogicObstacleData obstacleData = (LogicObstacleData) obstacleTable.GetItemAt(i);

                if (obstacleData.IsTallGrass())
                {
                    weights += obstacleData.GetRespawnWeight();

                    if (weights > rnd)
                    {
                        tallGrassData = obstacleData;
                        break;
                    }
                }
            }

            if (this.GetTallGrassCount() == 0 || this.m_tallGrassRespawnRandom.Rand(100) <= 80)
            {
                LogicArrayList<int> validX = new LogicArrayList<int>();
                LogicArrayList<int> validY = new LogicArrayList<int>();

                LogicDecoData oldBarbarianStatueData = LogicDataTables.GetDecoByName("Old Barbarian Statue", null);

                if (this.GetGameObjectCountByData(oldBarbarianStatueData) > 0)
                {
                    LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[(int) LogicGameObjectType.DECO];

                    for (int i = 0; i < gameObjects.Size(); i++)
                    {
                        LogicDeco deco = (LogicDeco) gameObjects[i];

                        if (deco.GetData() == oldBarbarianStatueData)
                        {
                            int tileX = deco.GetTileX();
                            int tileY = deco.GetTileY();
                            int width = deco.GetWidthInTiles();
                            int height = deco.GetHeightInTiles();

                            for (int j = 0; j < 2; j++)
                            {
                                for (int k = 0; k < width; k++)
                                {
                                    int x = tileX + k;
                                    int y = tileY - 1;

                                    if (this.m_level.IsValidPlaceForObstacle(x, y, 1, 1, false, false))
                                    {
                                        validX.Add(x);
                                        validY.Add(y);
                                    }

                                    y = tileY + height;

                                    if (this.m_level.IsValidPlaceForObstacle(x, y, 1, 1, false, false))
                                    {
                                        validX.Add(x);
                                        validY.Add(y);
                                    }
                                }

                                for (int k = 0; k < height; k++)
                                {
                                    int x = tileX - 1;
                                    int y = tileY + k;

                                    if (this.m_level.IsValidPlaceForObstacle(x, y, 1, 1, false, false))
                                    {
                                        validX.Add(x);
                                        validY.Add(y);
                                    }

                                    x = tileX + width;

                                    if (this.m_level.IsValidPlaceForObstacle(x, y, 1, 1, false, false))
                                    {
                                        validX.Add(x);
                                        validY.Add(y);
                                    }
                                }
                            }
                        }
                    }
                }

                LogicArrayList<LogicGameObject> obstacles = this.m_gameObjects[(int) LogicGameObjectType.OBSTACLE];

                for (int i = 0; i < obstacles.Size(); i++)
                {
                    LogicObstacle obstacle = (LogicObstacle) obstacles[i];

                    if (obstacle.GetObstacleData().IsTallGrassSpawnPoint())
                    {
                        int tileX = obstacle.GetTileX();
                        int tileY = obstacle.GetTileY();
                        int width = obstacle.GetWidthInTiles();
                        int height = obstacle.GetHeightInTiles();

                        for (int j = 0; j < width; j++)
                        {
                            int x = tileX + j;
                            int y = tileY - 1;

                            if (this.m_level.IsValidPlaceForObstacle(x, y, 1, 1, false, false))
                            {
                                validX.Add(x);
                                validY.Add(y);
                            }

                            y = tileY + height;

                            if (this.m_level.IsValidPlaceForObstacle(x, y, 1, 1, false, false))
                            {
                                validX.Add(x);
                                validY.Add(y);
                            }
                        }

                        for (int j = 0; j < height; j++)
                        {
                            int x = tileX - 1;
                            int y = tileY + j;

                            if (this.m_level.IsValidPlaceForObstacle(x, y, 1, 1, false, false))
                            {
                                validX.Add(x);
                                validY.Add(y);
                            }

                            x = tileX + width;

                            if (this.m_level.IsValidPlaceForObstacle(x, y, 1, 1, false, false))
                            {
                                validX.Add(x);
                                validY.Add(y);
                            }
                        }
                    }
                }

                if (validX.Size() > 0)
                {
                    int rndIdx = this.m_tallGrassRespawnRandom.Rand(validX.Size());
                    int x = validX[rndIdx];
                    int y = validY[rndIdx];

                    if (!this.m_level.IsValidPlaceForObstacle(x, y, 1, 1, false, true))
                    {
                        Debugger.Warning("Trying to spawn units on non empty area");
                    }

                    LogicObstacle obstacle = (LogicObstacle) LogicGameObjectFactory.CreateGameObject(tallGrassData, this.m_level, this.m_villageType);
                    obstacle.SetInitialPosition(x << 9, y << 9);
                    this.AddGameObject(obstacle, -1);
                }
            }
        }

        public bool RandomlyPlaceObstacle(LogicObstacleData data)
        {
            if (data.IsEnabledInVillageType(this.m_villageType))
            {
                if (this.m_level.GetVillageType() != this.m_villageType && data.GetLootDefensePercentage() == 0)
                {
                    Debugger.Warning("invalid village type for randomlyPlaceObstacle");
                }

                for (int i = 0; i <= 20; i++)
                {
                    int widthInTiles = this.m_level.GetWidthInTiles();
                    int heightInTiles = this.m_level.GetHeightInTiles();
                    int x = this.m_obstacleRespawnRandom.Rand(widthInTiles - data.GetWidth() + 1);
                    int y = this.m_obstacleRespawnRandom.Rand(heightInTiles - data.GetHeight() + 1);

                    if (this.m_level.IsValidPlaceForObstacle(x, y, data.GetWidth(), data.GetHeight(), true, true))
                    {
                        LogicObstacle obstacle = (LogicObstacle) LogicGameObjectFactory.CreateGameObject(data, this.m_level, this.m_villageType);

                        if (data.GetLootCount() > 0)
                        {
                            obstacle.SetLootMultiplyVersion(2);
                        }

                        obstacle.SetInitialPosition(x << 9, y << 9);

                        this.AddGameObject(obstacle, -1);

                        return true;
                    }
                }
            }
            else
            {
                Debugger.Warning("randomlyPlaceObstacle; trying to place obstacle in wrong village");
            }

            return false;
        }

        public bool CreateObstacleIfAnyPlaceExists(LogicObstacleData data, int width, int height)
        {
            int levelWidth = this.m_level.GetWidthInTiles();
            int levelHeight = this.m_level.GetHeightInTiles();
            int possibility = levelWidth * levelHeight;
            int y = this.m_obstacleRespawnRandom.Rand(levelHeight);
            int x = this.m_obstacleRespawnRandom.Rand(levelWidth);

            while (possibility-- > 0)
            {
                if (!this.m_level.IsValidPlaceForObstacle(x, y, width, height, false, true))
                {
                    if (++x + width > levelWidth)
                    {
                        if (++y + height > levelHeight)
                        {
                            y = 0;
                        }

                        x = 0;
                    }
                }
                else
                {
                    LogicObstacle obstacle = (LogicObstacle) LogicGameObjectFactory.CreateGameObject(data, this.m_level, this.m_villageType);
                    obstacle.SetInitialPosition(x << 9, y << 9);
                    this.AddGameObject(obstacle, -1);

                    return true;
                }
            }

            return false;
        }

        public void RefreshArmyCampSize()
        {
            LogicArrayList<LogicComponent> components = this.m_componentManager.GetComponents(LogicComponentType.VILLAGE2_UNIT);

            for (int i = 0; i < components.Size(); i++)
            {
                ((LogicVillage2UnitComponent) components[i]).RefreshArmyCampSize(true);
            }
        }

        public LogicGameObject GetClosestGameObject(int x, int y, LogicGameObjectFilter filter)
        {
            if (filter.IsComponentFilter())
            {
                return this.m_componentManager.GetClosestComponent(x, y, (LogicComponentFilter) filter)?.GetParent();
            }

            LogicGameObject closestGameObject = null;
            int minDistance = 0;

            for (int i = 0; i < 8; i++)
            {
                if (filter.ContainsGameObjectType(i))
                {
                    LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[i];

                    for (int j = 0; j < gameObjects.Size(); j++)
                    {
                        LogicGameObject gameObject = gameObjects[j];

                        if (filter.TestGameObject(gameObject))
                        {
                            int distance = gameObject.GetPosition().GetDistanceSquaredTo(x, y);

                            if (closestGameObject == null || distance < minDistance)
                            {
                                closestGameObject = gameObject;
                                minDistance = distance;
                            }
                        }
                    }
                }
            }

            return closestGameObject;
        }

        public void GetGameObjects(LogicArrayList<LogicGameObject> output, LogicGameObjectFilter filter)
        {
            output.Clear();

            if (filter.IsComponentFilter())
            {
                LogicComponentFilter componentFilter = (LogicComponentFilter) filter;
                LogicArrayList<LogicComponent> components = this.m_componentManager.GetComponents(componentFilter.GetComponentType());

                for (int i = 0, size = components.Size(); i < size; i++)
                {
                    LogicGameObject parent = components[i].GetParent();

                    if (componentFilter.TestGameObject(parent))
                    {
                        output.Add(parent);
                    }
                }
            }
            else
            {
                for (int i = 0; i < LogicGameObject.GAMEOBJECT_TYPE_COUNT; i++)
                {
                    LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[i];

                    for (int j = 0, size = gameObjects.Size(); j < size; j++)
                    {
                        LogicGameObject gameObject = gameObjects[j];

                        if (filter.TestGameObject(gameObject))
                        {
                            output.Add(gameObject);
                        }
                    }
                }
            }
        }

        public int GetAvailableBuildingUpgradeCount(LogicBuilding gameObject)
        {
            LogicBuildingData data = gameObject.GetBuildingData();
            int upgradeLevel = gameObject.GetUpgradeLevel() + 1;

            if (data.GetUpgradeLevelCount() > upgradeLevel)
            {
                int amount = data.GetAmountCanBeUpgraded(upgradeLevel);

                if (amount != 0)
                {
                    int count = 0;
                    LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[(int) LogicGameObjectType.BUILDING];

                    for (int i = 0; i < gameObjects.Size(); i++)
                    {
                        LogicBuilding building = (LogicBuilding) gameObjects[i];

                        if (building.GetData() == data)
                        {
                            if (building.GetUpgradeLevel() > gameObject.GetUpgradeLevel())
                            {
                                count += 1;
                            }
                        }
                    }

                    return amount - count;
                }

                return 1;
            }

            return 0;
        }

        public void ChangeVillageType(bool enabled)
        {
            for (int i = 0; i < LogicGameObject.GAMEOBJECT_TYPE_COUNT; i++)
            {
                LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[i];

                for (int j = 0; j < gameObjects.Size(); j++)
                {
                    LogicGameObject gameObject = gameObjects[j];

                    if (enabled)
                    {
                        this.m_tileMap.AddGameObject(gameObject);

                        bool success = true;

                        if (LogicDataTables.GetGlobals().UseTeslaTriggerCommand())
                        {
                            if (gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING &&
                                gameObject.IsHidden() &&
                                this.m_level.GetState() != 1 &&
                                this.m_level.GetState() != 4)
                            {
                                success = false;
                            }
                        }

                        if (LogicDataTables.GetGlobals().UseTrapTriggerCommand())
                        {
                            if (gameObject.GetGameObjectType() == LogicGameObjectType.TRAP &&
                                this.m_level.GetState() != 1 &&
                                this.m_level.GetState() != 4)
                            {
                                success = false;
                            }
                        }

                        if (success)
                        {
                            this.m_listener.AddGameObject(gameObject);
                        }
                    }
                    else
                    {
                        this.m_tileMap.RemoveGameObject(gameObject);
                        this.m_listener.RemoveGameObject(gameObject);
                    }
                }
            }
        }

        public void DoDestucting()
        {
            bool projectileDestructed = false;

            for (int type = 0; type < LogicGameObject.GAMEOBJECT_TYPE_COUNT; type++)
            {
                LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[type];

                for (int i = 0; i < gameObjects.Size(); i++)
                {
                    LogicGameObject gameObject = gameObjects[i];

                    if (gameObject.ShouldDestruct())
                    {
                        gameObjects.Remove(i--);

                        this.RemoveGameObjectReferences(gameObject);
                        this.m_listener.RemoveGameObject(gameObject);

                        gameObject.Destruct();

                        if (type == 2)
                        {
                            projectileDestructed = true;
                        }
                    }
                }
            }

            if (projectileDestructed)
            {
                if (this.m_level.GetConfiguration().GetBattleWaitForProjectileDestruction())
                {
                    if (this.m_gameObjects[(int) LogicGameObjectType.PROJECTILE].Size() == 0)
                    {
                        this.m_level.UpdateBattleStatus();
                    }
                }
            }
        }

        public void SubTick()
        {
            this.m_componentManager.SubTick();

            if (LogicDataTables.GetGlobals().UseNewTraining())
            {
                this.m_unitProduction.SubTick();
                this.m_spellProduction.SubTick();
            }

            LogicArrayList<LogicGameObject> buildings = this.m_gameObjects[(int) LogicGameObjectType.BUILDING];
            LogicArrayList<LogicGameObject> characters = this.m_gameObjects[(int) LogicGameObjectType.CHARACTER];
            LogicArrayList<LogicGameObject> projectiles = this.m_gameObjects[(int) LogicGameObjectType.PROJECTILE];
            LogicArrayList<LogicGameObject> spells = this.m_gameObjects[(int) LogicGameObjectType.SPELL];
            LogicArrayList<LogicGameObject> vObjects = this.m_gameObjects[(int) LogicGameObjectType.VILLAGE_OBJECT];

            for (int i = 0, j = characters.Size(); i < j; i++)
            {
                characters[i].SubTick();
            }

            for (int i = 0, j = spells.Size(); i < j; i++)
            {
                spells[i].SubTick();
            }

            for (int i = 0, j = buildings.Size(); i < j; i++)
            {
                buildings[i].SubTick();
            }

            for (int i = 0, j = projectiles.Size(); i < j; i++)
            {
                projectiles[i].SubTick();
            }

            for (int i = 0, j = vObjects.Size(); i < j; i++)
            {
                vObjects[i].SubTick();
            }
        }

        public void Tick()
        {
            this.DoDestucting();

            if (LogicDataTables.GetGlobals().UseNewTraining())
            {
                this.m_unitProduction.Tick();
                this.m_spellProduction.Tick();
            }

            this.m_componentManager.Tick();

            for (int i = 0; i < LogicGameObject.GAMEOBJECT_TYPE_COUNT; i++)
            {
                LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[i];

                for (int j = 0, size = gameObjects.Size(); j < size; j++)
                {
                    gameObjects[j].Tick();
                }
            }
        }

        public void Load(LogicJSONObject jsonObject)
        {
            this.Load(jsonObject, false, true);
        }

        public void LoadFromSnapshot(LogicJSONObject jsonObject)
        {
            LogicGameMode gameMode = this.m_level.GetGameMode();

            if (gameMode.GetVisitType() != 1 &&
                gameMode.GetVisitType() != 4 &&
                gameMode.GetVisitType() != 5)
            {
                this.Load(jsonObject, true, true);
            }
            else
            {
                this.Load(jsonObject, true, false);

                int layout = 7;

                if (gameMode.GetVisitType() != 4 && !this.m_level.IsArrangedWar() || !this.m_level.IsArrangedWar())
                {
                    int warLayout = this.m_level.GetWarLayout();

                    if (warLayout < 0 || !this.m_level.IsWarBase())
                    {
                        layout = this.m_level.GetActiveLayout();
                    }
                    else
                    {
                        layout = warLayout;
                    }
                }

                bool useDraft = false;

                for (int i = 0; i < LogicGameObject.GAMEOBJECT_TYPE_COUNT; i++)
                {
                    LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[i];

                    for (int j = 0; j < gameObjects.Size(); j++)
                    {
                        LogicGameObject gameObject = gameObjects[j];

                        if (gameObject.GetComponent(LogicComponentType.LAYOUT) != null &&
                            gameObject.GetPositionLayout(layout, true).m_x != -1)
                        {
                            useDraft = true;
                        }
                    }
                }

                if (useDraft)
                {
                    for (int i = 0; i < LogicGameObject.GAMEOBJECT_TYPE_COUNT; i++)
                    {
                        LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[i];

                        for (int j = 0; j < gameObjects.Size(); j++)
                        {
                            LogicGameObject gameObject = gameObjects[j];

                            if (gameObject.GetComponent(LogicComponentType.LAYOUT) != null)
                            {
                                LogicVector2 editModePosition = gameObject.GetPositionLayout(layout, true);

                                if (editModePosition.m_x == -1 || editModePosition.m_y == -1)
                                {
                                    gameObject.SetPositionLayoutXY(-1, -1, layout, false);
                                }
                                else
                                {
                                    gameObject.SetInitialPosition(editModePosition.m_x << 9, editModePosition.m_y << 9);
                                    gameObject.SetPositionLayoutXY(editModePosition.m_x, editModePosition.m_y, layout, false);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Load(LogicJSONObject jsonObject, bool snapshot, bool loadObstacle)
        {
            this.m_specialObstacleData = this.m_level.GetGameMode().GetConfiguration().GetSpecialObstacleData();

            if (this.GetNumGameObjects() != 0)
            {
                Debugger.Error("LogicGameObjectManager::load - numGameObjects is non zero!");
                return;
            }

            if (this.m_villageType == 1)
            {
                LogicJSONArray buildingArray = jsonObject.GetJSONArray("buildings2");
                LogicJSONArray trapArray = jsonObject.GetJSONArray("traps2");
                LogicJSONArray decoArray = jsonObject.GetJSONArray("decos2");

                if (buildingArray != null)
                {
                    this.LoadGameObjectsJsonArray(buildingArray, snapshot);
                }

                if (loadObstacle)
                {
                    LogicJSONArray vObjArray = jsonObject.GetJSONArray("vobjs2");
                    LogicJSONArray obstacleArray = jsonObject.GetJSONArray("obstacles2");

                    if (obstacleArray != null)
                    {
                        this.LoadGameObjectsJsonArray(obstacleArray, snapshot);
                    }

                    if (vObjArray != null)
                    {
                        this.LoadGameObjectsJsonArray(vObjArray, snapshot);
                    }
                }

                if (trapArray != null)
                {
                    this.LoadGameObjectsJsonArray(trapArray, snapshot);
                }

                if (decoArray != null)
                {
                    this.LoadGameObjectsJsonArray(decoArray, snapshot);
                }

                if (!snapshot)
                {
                    LogicJSONNumber respawnSecondsObject = jsonObject.GetJSONNumber("v2rs");

                    if (respawnSecondsObject != null)
                    {
                        this.m_secondsFromLastRespawn = respawnSecondsObject.GetIntValue();
                    }

                    LogicJSONNumber respawnSeedObject = jsonObject.GetJSONNumber("v2rseed");

                    if (respawnSeedObject != null)
                    {
                        this.m_obstacleRespawnRandom.SetIteratedRandomSeed(respawnSeedObject.GetIntValue());
                    }
                    else
                    {
                        this.m_obstacleRespawnRandom.SetIteratedRandomSeed(112);
                    }

                    LogicJSONNumber respawnClearCounterObject = jsonObject.GetJSONNumber("v2ccounter");

                    if (respawnClearCounterObject != null)
                    {
                        this.m_obstacleClearCounter = respawnClearCounterObject.GetIntValue();
                    }

                    LogicJSONNumber respawnTallGrassSecondsObject = jsonObject.GetJSONNumber("tgsec");

                    if (respawnTallGrassSecondsObject != null)
                    {
                        this.m_secondsFromLastTallGrassRespawn = respawnTallGrassSecondsObject.GetIntValue();
                    }

                    LogicJSONNumber respawnTallGrassSeedObject = jsonObject.GetJSONNumber("tgseed");

                    if (respawnTallGrassSeedObject != null)
                    {
                        this.m_tallGrassRespawnRandom.SetIteratedRandomSeed(respawnTallGrassSeedObject.GetIntValue());
                    }
                }
            }
            else
            {
                LogicJSONArray buildingArray = jsonObject.GetJSONArray("buildings");
                LogicJSONArray trapArray = jsonObject.GetJSONArray("traps");
                LogicJSONArray decoArray = jsonObject.GetJSONArray("decos");

                if (buildingArray != null)
                {
                    this.LoadGameObjectsJsonArray(buildingArray, snapshot);
                }
                else
                {
                    Debugger.Error("LogicGameObjectManager::load - Building array is NULL!");
                    return;
                }

                if (loadObstacle)
                {
                    LogicJSONArray obstacleArray = jsonObject.GetJSONArray("obstacles");
                    LogicJSONArray vObjArray = jsonObject.GetJSONArray("vobjs");

                    if (obstacleArray != null)
                    {
                        this.LoadGameObjectsJsonArray(obstacleArray, snapshot);
                    }

                    if (vObjArray != null)
                    {
                        this.LoadGameObjectsJsonArray(vObjArray, snapshot);
                    }
                }

                if (trapArray != null)
                {
                    this.LoadGameObjectsJsonArray(trapArray, snapshot);
                }

                if (decoArray != null)
                {
                    this.LoadGameObjectsJsonArray(decoArray, snapshot);
                }

                if (!snapshot)
                {
                    LogicJSONObject respawnVarsObject = jsonObject.GetJSONObject("respawnVars");

                    if (respawnVarsObject != null)
                    {
                        this.m_secondsFromLastRespawn = respawnVarsObject.GetJSONNumber("secondsFromLastRespawn").GetIntValue();
                        this.m_obstacleRespawnRandom.SetIteratedRandomSeed(respawnVarsObject.GetJSONNumber("respawnSeed").GetIntValue());
                        this.m_obstacleClearCounter = respawnVarsObject.GetJSONNumber("obstacleClearCounter").GetIntValue();

                        LogicJSONNumber timeToGemboxDropObject = respawnVarsObject.GetJSONNumber("time_to_gembox_drop");

                        if (timeToGemboxDropObject != null)
                        {
                            this.m_gemBoxDropSecs = timeToGemboxDropObject.GetIntValue();
                        }
                        else
                        {
                            if (this.m_bonusGemboxData != null)
                            {
                                int random = this.m_obstacleRespawnRandom.Rand(this.m_bonusGemboxData.GetAppearancePeriodHours());

                                this.m_gemBoxDropSecs = 3600 * random + this.m_gemBoxPeriodSecs;
                                this.m_gemBoxPeriodSecs = 3600 * (this.m_bonusGemboxData.GetAppearancePeriodHours() - random);
                            }
                        }

                        LogicJSONNumber timeToGemboxPeriodObject = respawnVarsObject.GetJSONNumber("time_in_gembox_period");

                        if (timeToGemboxPeriodObject != null)
                        {
                            this.m_gemBoxPeriodSecs = timeToGemboxPeriodObject.GetIntValue();
                        }

                        if (this.m_specialObstacleData != null)
                        {
                            LogicJSONNumber timeToSpecialDropObject = respawnVarsObject.GetJSONNumber("time_to_special_drop");

                            if (timeToSpecialDropObject != null)
                            {
                                this.m_specialObstacleDropSecs = timeToSpecialDropObject.GetIntValue();
                            }
                            else
                            {
                                this.m_specialObstaclePeriodSecs = 0;

                                int random = this.m_obstacleRespawnRandom.Rand(this.m_specialObstacleData.GetAppearancePeriodHours());

                                this.m_specialObstacleDropSecs = 3600 * random + this.m_specialObstaclePeriodSecs;
                                this.m_specialObstaclePeriodSecs = 3600 * (this.m_specialObstacleData.GetAppearancePeriodHours() - random);
                            }

                            LogicJSONNumber timeToSpecialPeriodObject = respawnVarsObject.GetJSONNumber("time_to_special_period");

                            if (timeToSpecialPeriodObject != null)
                            {
                                this.m_specialObstaclePeriodSecs = timeToSpecialPeriodObject.GetIntValue();
                            }
                        }
                    }
                    else
                    {
                        Debugger.Warning("Can't find respawn variables");

                        this.m_obstacleRespawnRandom.SetIteratedRandomSeed(112);
                        this.m_gemBoxDropSecs = 604800;
                        this.m_specialObstacleDropSecs = 604800;
                        this.m_specialObstaclePeriodSecs = 0;

                        if (this.m_bonusGemboxData != null)
                        {
                            this.m_gemBoxDropSecs = 3600 * this.m_bonusGemboxData.GetAppearancePeriodHours();
                        }

                        if (this.m_specialObstacleData != null)
                        {
                            this.m_specialObstacleDropSecs = 3600 * this.m_specialObstacleData.GetAppearancePeriodHours();
                        }
                    }
                }

                if (LogicDataTables.GetGlobals().UseNewTraining())
                {
                    LogicJSONObject unitsObject = jsonObject.GetJSONObject("units");
                    LogicJSONObject spellsObjects = jsonObject.GetJSONObject("spells");

                    if (unitsObject != null)
                    {
                        this.m_unitProduction.Load(unitsObject);
                    }

                    if (spellsObjects != null)
                    {
                        this.m_spellProduction.Load(spellsObjects);
                    }
                }
            }

            this.m_tileMap.EnableRoomIndices(true);
        }

        public void LoadGameObjectsJsonArray(LogicJSONArray jsonArray, bool snapshot)
        {
            for (int i = 0, size = jsonArray.Size(); i < size; i++)
            {
                LogicJSONObject jsonObject = (LogicJSONObject) jsonArray.Get(i);

                if (jsonObject != null)
                {
                    LogicJSONNumber dataObject = jsonObject.GetJSONNumber("data");

                    if (dataObject != null)
                    {
                        LogicGameObjectData data = (LogicGameObjectData) LogicDataTables.GetDataById(dataObject.GetIntValue());

                        if (data != null)
                        {
                            LogicDataType dataType = data.GetDataType();

                            if (dataType != LogicDataType.BUILDING &&
                                dataType != LogicDataType.OBSTACLE &&
                                dataType != LogicDataType.TRAP &&
                                dataType != LogicDataType.DECO &&
                                dataType != LogicDataType.VILLAGE_OBJECT)
                            {
                                return;
                            }

                            LogicGameObject gameObject = LogicGameObjectFactory.CreateGameObject(data, this.m_level, this.m_villageType);

                            if (gameObject != null)
                            {
                                LogicJSONNumber idObject = jsonObject.GetJSONNumber("id");
                                int globalId = -1;

                                if (idObject != null)
                                {
                                    globalId = idObject.GetIntValue();
                                }

                                if (snapshot)
                                {
                                    gameObject.LoadFromSnapshot(jsonObject);
                                }
                                else
                                {
                                    gameObject.Load(jsonObject);
                                }

                                bool needDestruct = false;

                                if (data.IsEnableByCalendar())
                                {
                                    int matchType = this.m_level.GetMatchType();

                                    if (!(matchType <= 7 && (matchType == 3 || matchType == 5 || matchType == 7)) && this.m_level.GetGameMode().GetVisitType() != 1)
                                    {
                                        needDestruct = !this.m_level.GetGameMode().GetCalendar().IsEnabled(data);
                                    }
                                }

                                if (dataType == LogicDataType.OBSTACLE)
                                {
                                    LogicObstacleData obstacleData = (LogicObstacleData) data;

                                    if (obstacleData.GetLootDefensePercentage() > 0 && this.m_townHall != null)
                                    {
                                        if (LogicDataTables.GetGlobals().GetLootCartEnabledTownHall() >= this.m_townHall.GetUpgradeLevel())
                                        {
                                            needDestruct = true;
                                        }
                                    }
                                }
                                else if (dataType == LogicDataType.VILLAGE_OBJECT)
                                {
                                    LogicVillageObjectData villageObjectData = (LogicVillageObjectData) data;

                                    if (villageObjectData.IsDisabled())
                                    {
                                        needDestruct = true;
                                    }
                                }

                                if (snapshot)
                                {
                                    if (gameObject.GetTileX() == -1 && gameObject.GetTileY() == -1)
                                    {
                                        needDestruct = true;
                                    }
                                }

                                if (needDestruct)
                                {
                                    gameObject.Destruct();
                                    gameObject = null;
                                }
                                else
                                {
                                    this.AddGameObject(gameObject, globalId);
                                }
                            }
                        }
                        else
                        {
                            Debugger.Error("LogicGameObjectManager::load - Data is NULL!");
                        }
                    }
                    else
                    {
                        Debugger.Error("LogicGameObjectManager::load - Data id was not found!");
                    }
                }
                else
                {
                    Debugger.Error("LogicGameObjectManager::load - Building is NULL!");
                }
            }
        }

        public void Save(LogicJSONObject jsonObject)
        {
            if (this.m_villageType == 1)
            {
                jsonObject.Put("buildings2", this.SaveGameObjects(LogicGameObjectType.BUILDING));
                jsonObject.Put("obstacles2", this.SaveObstacles());
                jsonObject.Put("traps2", this.SaveGameObjects(LogicGameObjectType.TRAP));
                jsonObject.Put("decos2", this.SaveGameObjects(LogicGameObjectType.DECO));

                if (!this.m_level.IsNpcVillage())
                {
                    if (LogicDataTables.GetGlobals().SaveVillageObjects())
                    {
                        jsonObject.Put("vobjs2", this.SaveGameObjects(LogicGameObjectType.VILLAGE_OBJECT));
                    }

                    int passedSecs = LogicTime.GetTicksInSeconds(this.m_level.GetLogicTime().GetTick());

                    jsonObject.Put("v2rs", new LogicJSONNumber(passedSecs + this.m_secondsFromLastRespawn));
                    jsonObject.Put("v2rseed", new LogicJSONNumber(this.m_obstacleRespawnRandom.GetIteratedRandomSeed()));
                    jsonObject.Put("v2ccounter", new LogicJSONNumber(this.m_obstacleClearCounter));
                    jsonObject.Put("tgsec", new LogicJSONNumber(this.m_secondsFromLastTallGrassRespawn));
                    jsonObject.Put("tgseed", new LogicJSONNumber(this.m_tallGrassRespawnRandom.GetIteratedRandomSeed()));
                }
            }
            else
            {
                jsonObject.Put("buildings", this.SaveGameObjects(LogicGameObjectType.BUILDING));
                jsonObject.Put("obstacles", this.SaveObstacles());
                jsonObject.Put("traps", this.SaveGameObjects(LogicGameObjectType.TRAP));
                jsonObject.Put("decos", this.SaveGameObjects(LogicGameObjectType.DECO));

                if (!this.m_level.IsNpcVillage())
                {
                    if (LogicDataTables.GetGlobals().SaveVillageObjects())
                    {
                        jsonObject.Put("vobjs", this.SaveGameObjects(LogicGameObjectType.VILLAGE_OBJECT));
                    }

                    LogicJSONObject respawnObject = new LogicJSONObject();

                    int passedSecs = LogicTime.GetTicksInSeconds(this.m_level.GetLogicTime().GetTick());

                    respawnObject.Put("secondsFromLastRespawn", new LogicJSONNumber(passedSecs + this.m_secondsFromLastRespawn));
                    respawnObject.Put("respawnSeed", new LogicJSONNumber(this.m_obstacleRespawnRandom.GetIteratedRandomSeed()));
                    respawnObject.Put("obstacleClearCounter", new LogicJSONNumber(this.m_obstacleClearCounter));

                    int maxGemBoxRespawnSecs = this.m_bonusGemboxData != null ? 7200 * this.m_bonusGemboxData.GetAppearancePeriodHours() : 1209600;

                    if (this.m_gemBoxDropSecs > maxGemBoxRespawnSecs)
                    {
                        this.m_gemBoxDropSecs = 0;
                        this.m_gemBoxPeriodSecs = 0;
                    }

                    respawnObject.Put("time_to_gembox_drop", new LogicJSONNumber(this.m_gemBoxDropSecs - passedSecs));
                    respawnObject.Put("time_in_gembox_period", new LogicJSONNumber(this.m_gemBoxPeriodSecs - passedSecs));

                    if (this.m_specialObstacleData != null)
                    {
                        respawnObject.Put("time_to_special_drop", new LogicJSONNumber(this.m_specialObstacleDropSecs - passedSecs));
                        respawnObject.Put("time_to_special_period", new LogicJSONNumber(this.m_specialObstaclePeriodSecs - passedSecs));
                    }

                    jsonObject.Put("respawnVars", respawnObject);

                    if (LogicDataTables.GetGlobals().UseNewTraining())
                    {
                        LogicJSONObject unitObject = new LogicJSONObject();
                        LogicJSONObject spellObject = new LogicJSONObject();

                        this.m_unitProduction.Save(unitObject);
                        this.m_spellProduction.Save(spellObject);

                        jsonObject.Put("units", unitObject);
                        jsonObject.Put("spells", spellObject);
                    }
                }
            }
        }

        public void SaveToSnapshot(LogicJSONObject jsonObject, int layoutId)
        {
            if (this.m_villageType == 1)
            {
                jsonObject.Put("buildings2", this.SaveGameObjects(LogicGameObjectType.BUILDING, layoutId));
                jsonObject.Put("obstacles2", this.SaveObstacles(layoutId));
                jsonObject.Put("traps2", this.SaveGameObjects(LogicGameObjectType.TRAP, layoutId));
                jsonObject.Put("decos2", this.SaveGameObjects(LogicGameObjectType.DECO, layoutId));

                if (!this.m_level.IsNpcVillage())
                {
                    if (LogicDataTables.GetGlobals().SaveVillageObjects())
                    {
                        jsonObject.Put("vobjs2", this.SaveGameObjects(LogicGameObjectType.VILLAGE_OBJECT, layoutId));
                    }

                    int passedSecs = LogicTime.GetTicksInSeconds(this.m_level.GetLogicTime().GetTick());

                    jsonObject.Put("v2rs", new LogicJSONNumber(passedSecs + this.m_secondsFromLastRespawn));
                    jsonObject.Put("v2rseed", new LogicJSONNumber(this.m_obstacleRespawnRandom.GetIteratedRandomSeed()));
                    jsonObject.Put("v2ccounter", new LogicJSONNumber(this.m_obstacleClearCounter));
                    jsonObject.Put("tgsec", new LogicJSONNumber(this.m_secondsFromLastTallGrassRespawn));
                    jsonObject.Put("tgseed", new LogicJSONNumber(this.m_tallGrassRespawnRandom.GetIteratedRandomSeed()));
                }
            }
            else
            {
                jsonObject.Put("buildings", this.SaveGameObjects(LogicGameObjectType.BUILDING, layoutId));
                jsonObject.Put("obstacles", this.SaveObstacles(layoutId));
                jsonObject.Put("traps", this.SaveGameObjects(LogicGameObjectType.TRAP, layoutId));
                jsonObject.Put("decos", this.SaveGameObjects(LogicGameObjectType.DECO, layoutId));

                if (!this.m_level.IsNpcVillage())
                {
                    if (LogicDataTables.GetGlobals().SaveVillageObjects())
                    {
                        jsonObject.Put("vobjs", this.SaveGameObjects(LogicGameObjectType.VILLAGE_OBJECT, layoutId));
                    }

                    LogicJSONObject respawnObject = new LogicJSONObject();

                    int passedSecs = LogicTime.GetTicksInSeconds(this.m_level.GetLogicTime().GetTick());

                    respawnObject.Put("secondsFromLastRespawn", new LogicJSONNumber(passedSecs + this.m_secondsFromLastRespawn));
                    respawnObject.Put("respawnSeed", new LogicJSONNumber(this.m_obstacleRespawnRandom.GetIteratedRandomSeed()));
                    respawnObject.Put("obstacleClearCounter", new LogicJSONNumber(this.m_obstacleClearCounter));

                    int maxGemBoxRespawnSecs = this.m_bonusGemboxData != null ? 7200 * this.m_bonusGemboxData.GetAppearancePeriodHours() : 1209600;

                    if (this.m_gemBoxDropSecs > maxGemBoxRespawnSecs)
                    {
                        this.m_gemBoxDropSecs = 0;
                        this.m_gemBoxPeriodSecs = 0;
                    }

                    respawnObject.Put("time_to_gembox_drop", new LogicJSONNumber(this.m_gemBoxDropSecs - passedSecs));
                    respawnObject.Put("time_in_gembox_period", new LogicJSONNumber(this.m_gemBoxPeriodSecs - passedSecs));

                    if (this.m_specialObstacleData != null)
                    {
                        respawnObject.Put("time_to_special_drop", new LogicJSONNumber(this.m_specialObstacleDropSecs - passedSecs));
                        respawnObject.Put("time_to_special_period", new LogicJSONNumber(this.m_specialObstaclePeriodSecs - passedSecs));
                    }

                    jsonObject.Put("respawnVars", respawnObject);
                }
            }
        }

        public LogicJSONArray SaveGameObjects(LogicGameObjectType gameObjectType)
        {
            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[(int) gameObjectType];
            LogicJSONArray jsonArray = new LogicJSONArray(gameObjects.Size());

            for (int i = 0, cnt = gameObjects.Size(); i < cnt; i++)
            {
                LogicGameObject gameObject = gameObjects[i];
                LogicJSONObject jsonObject = new LogicJSONObject();

                jsonObject.Put("data", new LogicJSONNumber(gameObject.GetData().GetGlobalID()));
                jsonObject.Put("id", new LogicJSONNumber(gameObject.GetGlobalID()));
                gameObject.Save(jsonObject, this.m_villageType);
                jsonArray.Add(jsonObject);
            }

            return jsonArray;
        }

        public LogicJSONArray SaveObstacles()
        {
            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[(int) LogicGameObjectType.OBSTACLE];
            LogicJSONArray jsonArray = new LogicJSONArray(gameObjects.Size());

            for (int i = 0, cnt = gameObjects.Size(); i < cnt; i++)
            {
                LogicObstacle gameObject = (LogicObstacle) gameObjects[i];

                if (!gameObject.IsFadingOut())
                {
                    LogicJSONObject jsonObject = new LogicJSONObject();

                    jsonObject.Put("data", new LogicJSONNumber(gameObject.GetData().GetGlobalID()));
                    jsonObject.Put("id", new LogicJSONNumber(gameObject.GetGlobalID()));
                    gameObject.Save(jsonObject, this.m_villageType);
                    jsonArray.Add(jsonObject);
                }
            }

            return jsonArray;
        }

        public LogicJSONArray SaveGameObjects(LogicGameObjectType gameObjectType, int layoutId)
        {
            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[(int) gameObjectType];
            LogicJSONArray jsonArray = new LogicJSONArray(gameObjects.Size());

            for (int i = 0, cnt = gameObjects.Size(); i < cnt; i++)
            {
                LogicGameObject gameObject = gameObjects[i];
                LogicJSONObject jsonObject = new LogicJSONObject();

                jsonObject.Put("data", new LogicJSONNumber(gameObject.GetData().GetGlobalID()));
                jsonObject.Put("id", new LogicJSONNumber(gameObject.GetGlobalID()));
                gameObject.SaveToSnapshot(jsonObject, layoutId);
                jsonArray.Add(jsonObject);
            }

            return jsonArray;
        }

        public LogicJSONArray SaveObstacles(int layoutId)
        {
            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjects[(int) LogicGameObjectType.OBSTACLE];
            LogicJSONArray jsonArray = new LogicJSONArray(gameObjects.Size());

            for (int i = 0, cnt = gameObjects.Size(); i < cnt; i++)
            {
                LogicObstacle gameObject = (LogicObstacle) gameObjects[i];

                if (!gameObject.IsFadingOut())
                {
                    LogicJSONObject jsonObject = new LogicJSONObject();

                    jsonObject.Put("data", new LogicJSONNumber(gameObject.GetData().GetGlobalID()));
                    jsonObject.Put("id", new LogicJSONNumber(gameObject.GetGlobalID()));
                    gameObject.SaveToSnapshot(jsonObject, layoutId);
                    jsonArray.Add(jsonObject);
                }
            }

            return jsonArray;
        }

        public LogicBuilding GetClockTower()
        {
            return this.m_clockTower;
        }

        public LogicBuilding GetAllianceCastle()
        {
            return this.m_allianceCastle;
        }

        public LogicAlliancePortal GetAlliancePortal()
        {
            return this.m_alliancePortal;
        }

        public LogicVillageObject GetShipyard()
        {
            return this.m_shipyard;
        }

        public LogicBuilding GetLaboratory()
        {
            return this.m_laboratory;
        }
    }
}