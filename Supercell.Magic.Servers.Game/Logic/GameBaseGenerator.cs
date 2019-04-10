namespace Supercell.Magic.Servers.Game.Logic
{
    using System;
    using System.Text;
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Home;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Mode;
    using Supercell.Magic.Servers.Core.Helper;

    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public static class GameBaseGenerator
    {
        public static LogicClientAvatar HomeOwnerAvatar { get; private set; }

        private static LogicRandom m_random;
        private static LogicArrayList<LogicBuildingData> m_defenseBuildingArray;
        private static LogicArrayList<LogicBuildingData> m_otherBuildingArray;
        private static LogicArrayList<LogicTrapData> m_trapArray;

        public static void Init()
        {
            GameBaseGenerator.HomeOwnerAvatar = new LogicClientAvatar();
            GameBaseGenerator.HomeOwnerAvatar.SetName("Supercell - Private CoC Server");
            GameBaseGenerator.HomeOwnerAvatar.SetExpLevel(500);
            GameBaseGenerator.HomeOwnerAvatar.SetScore(5000);
            GameBaseGenerator.HomeOwnerAvatar.SetLeagueType(LogicDataTables.GetTable(LogicDataType.LEAGUE).GetItemCount() - 1);

            GameBaseGenerator.m_random = new LogicRandom(Environment.TickCount);
            GameBaseGenerator.m_defenseBuildingArray = new LogicArrayList<LogicBuildingData>();
            GameBaseGenerator.m_otherBuildingArray = new LogicArrayList<LogicBuildingData>();
            GameBaseGenerator.m_trapArray = new LogicArrayList<LogicTrapData>();

            LogicDataTable buildingTable = LogicDataTables.GetTable(LogicDataType.BUILDING);
            LogicDataTable trapTable = LogicDataTables.GetTable(LogicDataType.TRAP);

            for (int i = 0; i < buildingTable.GetItemCount(); i++)
            {
                LogicBuildingData buildingData = (LogicBuildingData) buildingTable.GetItemAt(i);

                if (buildingData.IsEnabledInVillageType(0))
                {
                    if (buildingData.GetBuildingClass().GetName() == "Defense")
                        GameBaseGenerator.m_defenseBuildingArray.Add(buildingData);
                    else
                        GameBaseGenerator.m_otherBuildingArray.Add(buildingData);
                }
            }

            for (int i = 0; i < trapTable.GetItemCount(); i++)
            {
                LogicTrapData trapData = (LogicTrapData) trapTable.GetItemAt(i);

                if (trapData.IsEnabledInVillageType(0))
                    GameBaseGenerator.m_trapArray.Add(trapData);
            }
        }

        public static LogicClientHome GenerateBase(LogicGameObjectData gameObjectData)
        {
            LogicClientHome logicClientHome = new LogicClientHome();
            LogicGameMode logicGameMode = new LogicGameMode();
            LogicLevel logicLevel = logicGameMode.GetLevel();
            LogicGameObjectManager logicGameObjectManager = logicLevel.GetGameObjectManagerAt(0);

            logicLevel.SetLoadingVillageType(-1);
            logicLevel.SetVillageType(0);
            logicLevel.SetExperienceVersion(1);
            logicLevel.SetHomeOwnerAvatar(GameBaseGenerator.HomeOwnerAvatar);

            LogicBuilding townHall = new LogicBuilding(LogicDataTables.GetTownHallData(), logicLevel, 0);

            townHall.SetInitialPosition((LogicLevel.TILEMAP_SIZE_X / 2 - townHall.GetWidthInTiles() / 2) << 9, (LogicLevel.TILEMAP_SIZE_Y / 2 - townHall.GetHeightInTiles() / 2) << 9);
            townHall.SetUpgradeLevel(townHall.GetBuildingData().GetUpgradeLevelCount() - 1);

            logicGameObjectManager.AddGameObject(townHall, -1);

            LogicTownhallLevelData townhallLevelData = LogicDataTables.GetTownHallLevel(townHall.GetUpgradeLevel());

            if (gameObjectData == null)
            {
                LogicArrayList<LogicGameObject> scrambler = new LogicArrayList<LogicGameObject>();

                for (int i = 0; i < GameBaseGenerator.m_defenseBuildingArray.Size(); i++)
                {
                    LogicBuildingData buildingData = GameBaseGenerator.m_defenseBuildingArray[i];

                    for (int j = townhallLevelData.GetUnlockedBuildingCount(buildingData); j > 0; j--)
                    {
                        LogicBuilding logicBuilding =
                            (LogicBuilding) GameBaseGenerator.CreateAndPlaceRandomlyGameObject(buildingData, logicLevel, buildingData.GetWidth(), buildingData.GetHeight(), 0);

                        if (logicBuilding != null)
                        {
                            logicBuilding.SetLocked(false);
                            logicBuilding.SetUpgradeLevel(buildingData.GetUpgradeLevelCount() - 1);
                            scrambler.Add(logicBuilding);

                            int upgradeLevel = buildingData.GetUpgradeLevelCount() - 1;
                            int minUpgradeLevelForGearUp = buildingData.GetMinUpgradeLevelForGearUp();

                            if (minUpgradeLevelForGearUp > -1 && upgradeLevel >= minUpgradeLevelForGearUp)
                            {
                                if (GameBaseGenerator.m_random.Rand(100) >= 50)
                                    logicBuilding.SetGearLevel(1);
                            }

                            if (buildingData.GetAttackerItemData(upgradeLevel).GetTargetingConeAngle() != 0)
                                logicBuilding.GetCombatComponent().ToggleAimAngle(buildingData.GetAimRotateStep() * GameBaseGenerator.m_random.Rand(360 / buildingData.GetAimRotateStep()), 0, false);

                            if (buildingData.GetAttackerItemData(upgradeLevel).HasAlternativeAttackMode())
                            {
                                if (minUpgradeLevelForGearUp > -1 && logicBuilding.GetGearLevel() != 1)
                                    continue;
                                if (GameBaseGenerator.m_random.Rand(100) >= 50)
                                    logicBuilding.GetCombatComponent().ToggleAttackMode(0, false);
                            }
                        }
                    }
                }

                for (int i = 0; i < GameBaseGenerator.m_otherBuildingArray.Size(); i++)
                {
                    LogicBuildingData buildingData = GameBaseGenerator.m_otherBuildingArray[i];

                    for (int j = townhallLevelData.GetUnlockedBuildingCount(buildingData); j > 0; j--)
                    {
                        LogicBuilding logicBuilding =
                            (LogicBuilding) GameBaseGenerator.CreateAndPlaceRandomlyGameObject(buildingData, logicLevel, buildingData.GetWidth(), buildingData.GetHeight(), 0);

                        if (logicBuilding != null)
                        {
                            logicBuilding.SetLocked(false);
                            logicBuilding.SetUpgradeLevel(buildingData.GetUpgradeLevelCount() - 1);
                            scrambler.Add(logicBuilding);
                        }
                    }
                }

                for (int i = 0; i < GameBaseGenerator.m_trapArray.Size(); i++)
                {
                    LogicTrapData trapData = (LogicTrapData)GameBaseGenerator.m_trapArray[i];

                    for (int j = townhallLevelData.GetUnlockedTrapCount(trapData); j > 0; j--)
                    {
                        LogicTrap trap = (LogicTrap)GameBaseGenerator.CreateAndPlaceRandomlyGameObject(trapData, logicLevel, trapData.GetWidth(), trapData.GetHeight(), 0);

                        if (trap != null)
                        {
                            trap.SetUpgradeLevel(trapData.GetUpgradeLevelCount() - 1);
                            scrambler.Add(trap);
                        }
                    }
                }

                for (int i = 0; i < scrambler.Size(); i++)
                {
                    LogicGameObject gameObject = scrambler[i];
                    LogicData data = gameObject.GetData();

                    int width = gameObject.GetWidthInTiles();
                    int height = gameObject.GetHeightInTiles();
                    int x = gameObject.GetX();
                    int y = gameObject.GetY();

                    LogicArrayList<LogicGameObject> available = new LogicArrayList<LogicGameObject>();

                    for (int j = i + 1; j < scrambler.Size(); j++)
                    {
                        if (data != scrambler[j].GetData())
                        {
                            if (scrambler[j].GetWidthInTiles() == width && scrambler[j].GetHeightInTiles() == height)
                                available.Add(scrambler[j]);
                        }
                    }

                    if (available.Size() != 0)
                    {
                        LogicGameObject swapObj = available[GameBaseGenerator.m_random.Rand(available.Size())];

                        gameObject.SetInitialPosition(swapObj.GetX(), swapObj.GetY());
                        swapObj.SetInitialPosition(x, y);
                    }
                }
            }
            else
            {
                int maxUpgradeLevel = 0;
                int width = 1;
                int height = 1;

                switch (gameObjectData.GetDataType())
                {
                    case LogicDataType.BUILDING:
                    {
                        LogicBuildingData logicBuildingData = (LogicBuildingData) gameObjectData;
                        maxUpgradeLevel = logicBuildingData.GetUpgradeLevelCount();
                        width = logicBuildingData.GetWidth();
                        height = logicBuildingData.GetHeight();
                        break;
                    }

                    case LogicDataType.TRAP:
                        LogicTrapData logicTrapData = (LogicTrapData)gameObjectData;
                        maxUpgradeLevel = logicTrapData.GetUpgradeLevelCount();
                        width = logicTrapData.GetWidth();
                        height = logicTrapData.GetHeight();
                        break;
                }

                int upgLevel = maxUpgradeLevel - 1;
                int x = 0;
                int y = 0;

                while (true)
                {
                    LogicBuilding building =
                        (LogicBuilding) GameBaseGenerator.CreateGameObjectIfAnyPlaceExist(gameObjectData, logicLevel, width, height, 0, x, y);

                    if (building == null)
                        break;

                    building.SetLocked(false);
                    building.SetUpgradeLevel(upgLevel != -1 ? upgLevel : GameBaseGenerator.m_random.Rand(building.GetUpgradeLevel()));
                    x = building.GetTileX();
                    y = building.GetTileY();
                }
            }

            for (int i = 0; i < 10; i++)
            {
                logicGameObjectManager.Village1CreateObstacle();
            }

            LogicJSONObject jsonObject = new LogicJSONObject();

            logicGameMode.SaveToJSON(jsonObject);
            logicGameMode.Destruct();
            
            logicClientHome.SetHomeJSON(LogicJSONParser.CreateJSONString(jsonObject, 2048));

            CompressibleStringHelper.Compress(logicClientHome.GetCompressibleHomeJSON());

            return logicClientHome;
        }

        private static LogicGameObject CreateAndPlaceRandomlyGameObject(LogicGameObjectData data, LogicLevel level, int width, int height, int villageType)
        {
            int levelEndX = level.GetPlayArea().GetEndX();
            int levelEndY = level.GetPlayArea().GetEndY();
            int midX = levelEndX / 2;
            int midY = levelEndY / 2;

            int passCount = 1;

            while (true)
            {
                int startX = LogicMath.Max(midX - passCount, 0);
                int startY = LogicMath.Max(midY - passCount, 0);
                int endX = LogicMath.Min(midX + passCount, levelEndX);
                int endY = LogicMath.Min(midY + passCount, levelEndY);

                int possibility = LogicMath.Min((endX - startX) * (endY - startY), 20);

                for (int i = 0; i < possibility; i++)
                {
                    int x = startX + GameBaseGenerator.m_random.Rand(endX - startX);
                    int y = startY + GameBaseGenerator.m_random.Rand(endY - startY);

                    if (level.IsValidPlaceForBuilding(x, y, width, height, null))
                    {
                        LogicGameObject gameObject = LogicGameObjectFactory.CreateGameObject(data, level, villageType);
                        gameObject.SetInitialPosition(x << 9, y << 9);
                        level.GetGameObjectManagerAt(villageType).AddGameObject(gameObject, -1);
                        return gameObject;
                    }
                }

                if (startX == 0 && startY == 0)
                {
                    return null;
                }

                passCount += 2;
            }
        }

        private static LogicGameObject CreateGameObjectIfAnyPlaceExist(LogicGameObjectData data, LogicLevel level, int width, int height, int villageType, int x = 0, int y = 0)
        {
            int levelEndX = level.GetPlayArea().GetEndX();
            int levelEndY = level.GetPlayArea().GetEndY();

            if (x == 0 && y == 0)
            {
                x = level.GetPlayArea().GetStartX();
                y = level.GetPlayArea().GetStartY();
            }

            while (true)
            {
                if (level.IsValidPlaceForBuilding(x, y, width, height, null))
                {
                    LogicGameObject gameObject = LogicGameObjectFactory.CreateGameObject(data, level, villageType);
                    gameObject.SetInitialPosition(x << 9, y << 9);
                    level.GetGameObjectManagerAt(villageType).AddGameObject(gameObject, -1);
                    return gameObject;
                }

                if (++x + width > levelEndX)
                {
                    if (++y + height > levelEndY)
                        break;
                    x = level.GetPlayArea().GetStartX();
                }
            }

            return null;
        }
    }
}