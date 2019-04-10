namespace Supercell.Magic.Logic.Command.Debug
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Calendar;
    using Supercell.Magic.Logic.Command.Battle;
    using Supercell.Magic.Logic.Command.Home;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Mode;
    using Supercell.Magic.Logic.Unit;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicDebugCommand : LogicCommand
    {
        private LogicDebugActionType m_debugAction;
        private int m_globalId;
        private int m_debugInt;
        private string m_debugString;

        private LogicArrayList<int> m_globalIds;
        private LogicArrayList<int> m_intArrayArg2;

        public LogicDebugCommand()
        {
            // LogicDebugCommand.
        }

        public LogicDebugCommand(LogicDebugActionType actionType) : this()
        {
            this.m_debugAction = actionType;
        }

        public void SetDebugString(string str)
        {
            this.m_debugString = str;
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            switch (this.m_debugAction)
            {
                case LogicDebugActionType.FAST_FORWARD_1_HOUR:
                    level.FastForwardTime(3600);
                    level.GetGameMode().SetDebugFastForwardSecs(level.GetGameMode().GetDebugFastForwardSecs() + 3600);

                    break;
                case LogicDebugActionType.FAST_FORWARD_24_HOUR:
                    level.FastForwardTime(86400);
                    level.GetGameMode().SetDebugFastForwardSecs(level.GetGameMode().GetDebugFastForwardSecs() + 86400);

                    break;
                case LogicDebugActionType.ADD_UNITS:
                    LogicDebugCommand.AddUnits(level, 300, 300, false);
                    break;
                case LogicDebugActionType.ADD_RESOURCES:
                {
                    int diamondCount = playerAvatar.GetDiamonds();

                    if (diamondCount < 50000)
                    {
                        playerAvatar.SetDiamonds(50000);
                        playerAvatar.GetChangeListener().FreeDiamondsAdded(50000 - diamondCount, 3);
                    }

                    LogicDataTable resourceTable = LogicDataTables.GetTable(LogicDataType.RESOURCE);

                    for (int i = 0; i < resourceTable.GetItemCount(); i++)
                    {
                        LogicResourceData data = (LogicResourceData) resourceTable.GetItemAt(i);

                        if (!data.IsPremiumCurrency())
                        {
                            if (data.GetWarResourceReferenceData() == null)
                            {
                                if (data != LogicDataTables.GetDarkElixirData())
                                {
                                    playerAvatar.SetResourceCount(data, 10000000);
                                    playerAvatar.GetChangeListener().CommodityCountChanged(0, data, playerAvatar.GetResourceCount(data));
                                }
                                else if (playerAvatar.IsDarkElixirUnlocked())
                                {
                                    playerAvatar.SetResourceCount(data, 200000);
                                    playerAvatar.GetChangeListener().CommodityCountChanged(0, data, playerAvatar.GetResourceCount(data));
                                }
                            }
                        }
                    }

                    break;
                }
                case LogicDebugActionType.INCREASE_XP_LEVEL:
                {
                    int expLevel = playerAvatar.GetExpLevel();

                    if (expLevel < LogicDataTables.GetExperienceLevelCount())
                    {
                        playerAvatar.SetExpLevel(expLevel + 1);
                        playerAvatar.SetExpPoints(0);
                        playerAvatar.GetChangeListener().ExpLevelGained(1);
                    }

                    break;
                }
                case LogicDebugActionType.UPGRADE_ALL_BUILDINGS:
                {
                    LogicGameObjectManager gameObjectManager = level.GetGameObjectManager();
                    LogicArrayList<LogicGameObject> buildings = gameObjectManager.GetGameObjects(LogicGameObjectType.BUILDING);
                    LogicArrayList<LogicGameObject> traps = gameObjectManager.GetGameObjects(LogicGameObjectType.TRAP);

                    for (int i = 0; i < buildings.Size(); i++)
                    {
                        LogicBuilding building = (LogicBuilding) buildings[i];

                        if (!building.IsMaxUpgradeLevel())
                        {
                            if (gameObjectManager.GetAvailableBuildingUpgradeCount(building) > 0 && !building.IsLocked())
                            {
                                if (!building.IsConstructing())
                                    building.StartUpgrading(true, false);
                                if (building.IsConstructing())
                                    building.FinishConstruction(false, true);
                            }
                        }
                    }

                    for (int i = 0; i < traps.Size(); i++)
                    {
                        LogicTrap trap = (LogicTrap) traps[i];

                        if (!trap.IsMaxUpgradeLevel())
                        {
                            if (!trap.IsConstructing())
                                trap.StartUpgrading();
                            if (trap.IsConstructing())
                                trap.FinishConstruction(false);
                        }

                        trap.RepairTrap();
                    }

                    break;
                }
                case LogicDebugActionType.COMPLETE_TUTORIAL:
                    level.GetMissionManager().DebugCompleteAllTutorials(false, true, true);
                    break;
                case LogicDebugActionType.UNLOCK_MAP:
                {
                    LogicDataTable npcTable = LogicDataTables.GetTable(LogicDataType.NPC);

                    for (int i = 0; i < npcTable.GetItemCount(); i++)
                    {
                        playerAvatar.SetNpcStars((LogicNpcData) npcTable.GetItemAt(i), 1);
                    }

                    break;
                }
                case LogicDebugActionType.SHIELD_TO_HALF:
                    if (level.GetState() == 1)
                    {
                        LogicGameMode gameMode = level.GetGameMode();

                        int shieldRemainingSecs = gameMode.GetShieldRemainingSeconds();
                        int guardRemainingSecs = gameMode.GetGuardRemainingSeconds();

                        if (shieldRemainingSecs < 122)
                        {
                            if ((uint) (shieldRemainingSecs + 1) <= 2 && guardRemainingSecs >= 122)
                            {
                                guardRemainingSecs /= 2;

                                gameMode.SetPersonalBreakCooldownSeconds(gameMode.GetPersonalBreakCooldownSeconds() - shieldRemainingSecs);
                                gameMode.SetGuardRemainingSeconds(guardRemainingSecs);

                                level.GetHome().GetChangeListener().ShieldActivated(0, guardRemainingSecs);
                            }
                        }
                        else
                        {
                            shieldRemainingSecs /= 2;

                            gameMode.SetShieldRemainingSeconds(shieldRemainingSecs);
                            level.GetHome().GetChangeListener().ShieldActivated(shieldRemainingSecs, 900);
                            gameMode.SetPersonalBreakCooldownSeconds(LogicDataTables.GetGlobals().GetPersonalBreakLimitSeconds() + shieldRemainingSecs);
                        }

                        Debugger.HudPrint(string.Format("Debug setting VillageGuard: duration:{0}, ends in:{1}, league level:{2}", gameMode.GetGuardRemainingSeconds() / 60,
                                                        gameMode.GetShieldRemainingSeconds() / 60, level.GetHomeLeagueData()));
                    }

                    break;
                case LogicDebugActionType.FAST_FORWARD_1_MIN:
                    level.FastForwardTime(60);
                    level.GetGameMode().SetDebugFastForwardSecs(level.GetGameMode().GetDebugFastForwardSecs() + 60);
                    break;
                case LogicDebugActionType.INCREASE_TROPHIES:
                    LogicDebugCommand.ScoreChangeHelper(level, playerAvatar, 5);
                    break;
                case LogicDebugActionType.DECREASE_TROPHIES:
                    LogicDebugCommand.ScoreChangeHelper(level, playerAvatar, -5);
                    break;
                case LogicDebugActionType.ADD_ALLIANCE_UNITS:
                    if (level.GetState() == 1)
                    {
                        if (playerAvatar.HasAllianceCastle())
                        {
                            LogicDataTable characterTable = LogicDataTables.GetTable(LogicDataType.CHARACTER);

                            for (int i = 0; i < characterTable.GetItemCount(); i++)
                            {
                                LogicCharacterData data = (LogicCharacterData) characterTable.GetItemAt(i);

                                for (int j = 0; j < data.GetUpgradeLevelCount(); j++)
                                {
                                    playerAvatar.SetAllianceUnitCount(data, j, 0);
                                    playerAvatar.GetChangeListener().AllianceUnitCountChanged(data, j, 0);
                                }
                            }

                            int seed = level.GetLogicTime().GetTick() & 0x7fffffff;
                            int totalCapacity = playerAvatar.GetAllianceCastleTotalCapacity();
                            int freeCapacity = totalCapacity;

                            for (int i = 0; i < 10; i++)
                            {
                                if (freeCapacity > 0)
                                {
                                    LogicCharacterData data = (LogicCharacterData) characterTable.GetItemAt(seed % characterTable.GetItemCount());

                                    seed = ((1103515245 * seed + 12345) >> 16) & 0x7fffffff;

                                    if (freeCapacity >= data.GetHousingSpace())
                                    {
                                        LogicAttackerItemData attackerItemData = data.GetAttackerItemData(0);
                                        LogicData preferredTargetData = attackerItemData.GetPreferredTargetData();

                                        if (preferredTargetData != null && preferredTargetData.GetDataType() == LogicDataType.BUILDING_CLASS)
                                        {
                                            LogicBuildingClassData buildingClassData = (LogicBuildingClassData) preferredTargetData;

                                            if (buildingClassData.IsWall())
                                                continue;
                                        }

                                        if (attackerItemData.GetDamage(0, false) < 0 || !level.GetGameMode().GetCalendar().IsProductionEnabled(data) || data.IsDonationDisabled())
                                            continue;

                                        int count = totalCapacity / data.GetHousingSpace() / 3 + 1;

                                        if (count > 0)
                                        {
                                            int upgLevel = data.GetUpgradeLevelCount() / 2;
                                            int housingSpace = data.GetHousingSpace();

                                            do
                                            {
                                                if (freeCapacity < housingSpace)
                                                    break;

                                                playerAvatar.SetAllianceUnitCount(data, upgLevel, playerAvatar.GetAllianceUnitCount(data, upgLevel) + 1);
                                                playerAvatar.GetChangeListener().AllianceUnitAdded(data, upgLevel);

                                                freeCapacity -= housingSpace;
                                            } while (--count > 0);
                                        }
                                    }
                                }
                            }

                            level.GetComponentManager().AddAvatarAllianceUnitsToCastle();
                        }
                    }

                    break;
                case LogicDebugActionType.INCREASE_HERO_LEVELS:
                    if (level.GetState() == 1)
                    {
                        LogicDataTable heroTable = LogicDataTables.GetTable(LogicDataType.HERO);

                        for (int i = 0; i < heroTable.GetItemCount(); i++)
                        {
                            LogicHeroData data = (LogicHeroData) heroTable.GetItemAt(i);

                            if (playerAvatar.GetHeroState(data) != 0)
                                playerAvatar.CommodityCountChangeHelper(1, data, 1);
                        }
                    }

                    break;
                case LogicDebugActionType.REMOVE_RESOURCES:
                {
                    int diamonds = playerAvatar.GetDiamonds();

                    playerAvatar.SetDiamonds(0);
                    playerAvatar.GetChangeListener().FreeDiamondsAdded(-diamonds, 3);

                    LogicDataTable resourceTable = LogicDataTables.GetTable(LogicDataType.RESOURCE);

                    for (int i = 0; i < resourceTable.GetItemCount(); i++)
                    {
                        LogicResourceData data = (LogicResourceData) resourceTable.GetItemAt(i);

                        if (!data.IsPremiumCurrency() && data.GetWarResourceReferenceData() == null)
                        {
                            playerAvatar.SetResourceCount(data, 0);
                            playerAvatar.GetChangeListener().CommodityCountChanged(0, data, playerAvatar.GetResourceCount(data));
                        }
                    }

                    break;
                }
                case LogicDebugActionType.RESET_MAP_PROGRESS:
                {
                    LogicDataTable npcTable = LogicDataTables.GetTable(LogicDataType.NPC);

                    for (int i = 0; i < npcTable.GetItemCount(); i++)
                    {
                        playerAvatar.SetNpcStars((LogicNpcData) npcTable.GetItemAt(i), 0);
                    }

                    break;
                }
                case LogicDebugActionType.DEPLOY_ALL_TROOPS:
                {
                    LogicRandom random = new LogicRandom(level.GetLogicTime().GetTick());

                    int villageType = level.GetVillageType();
                    int levelWidth = level.GetWidthInTiles();
                    int levelHeight = level.GetHeightInTiles();

                    LogicArrayList<LogicDataSlot> units = villageType == 1 ? playerAvatar.GetUnitsVillage2() : playerAvatar.GetUnits();
                    LogicRect playArea = new LogicRect(0, 0, 3, levelHeight);
                    LogicTileMap tileMap = level.GetTileMap();

                    if (random.Rand(255) >= 128)
                    {
                        playArea = new LogicRect(levelWidth - playArea.GetEndX(), playArea.GetStartY(), levelWidth - playArea.GetStartX(), playArea.GetEndY());
                    }

                    if (random.Rand(255) >= 128)
                    {
                        playArea = new LogicRect(playArea.GetStartY(), playArea.GetStartX(), playArea.GetEndY(), playArea.GetEndX());
                    }

                    for (int i = 0; i < units.Size(); i++)
                    {
                        LogicDataSlot slot = units[i];
                        LogicCharacterData data = (LogicCharacterData) slot.GetData();

                        if (data.GetVillageType() == level.GetVillageType())
                        {
                            for (int j = slot.GetCount(); j > 0; j--)
                            {
                                int x = 0;
                                int y = 0;

                                bool success = false;

                                for (int k = 0; k < 10; k++)
                                {
                                    x = playArea.GetStartX() + random.Rand(playArea.GetEndX() - playArea.GetStartX());
                                    y = playArea.GetStartY() + random.Rand(playArea.GetEndY() - playArea.GetStartY());

                                    LogicTile tile = tileMap.GetTile(x, y);

                                    if (tile != null &&
                                        tile.IsPassableFlag() &&
                                        tileMap.IsValidAttackPos(x, y))
                                    {
                                        success = true;
                                        break;
                                    }
                                }

                                if (!success)
                                    continue;

                                x = x << 9;
                                y = y << 9;

                                if (!tileMap.IsPassablePathFinder(x >> 8, y >> 8))
                                    return -1;

                                if ((villageType == 1 ? playerAvatar.GetUnitCountVillage2(data) : playerAvatar.GetUnitCount(data)) > 0)
                                {
                                    if (level.GetGameMode().IsInAttackPreparationMode())
                                        level.GetGameMode().EndAttackPreparation();

                                    LogicPlaceAttackerCommand.PlaceAttacker(playerAvatar, data, level, x, y);
                                }
                            }
                        }
                    }

                    break;
                }
                case LogicDebugActionType.ADD_100_TROPHIES:
                    LogicDebugCommand.ScoreChangeHelper(level, playerAvatar, 100);
                    break;
                case LogicDebugActionType.REMOVE_100_TROPHIES:
                    LogicDebugCommand.ScoreChangeHelper(level, playerAvatar, -100);
                    break;
                case LogicDebugActionType.UPGRADE_TO_MAX_FOR_TH:
                {
                    LogicGameObjectManager gameObjectManager = level.GetGameObjectManager();
                    LogicArrayList<LogicGameObject> buildings = gameObjectManager.GetGameObjects(LogicGameObjectType.BUILDING);
                    LogicArrayList<LogicGameObject> traps = gameObjectManager.GetGameObjects(LogicGameObjectType.TRAP);

                    int townHallLevel = 1;

                    for (int i = 0; i < buildings.Size(); i++)
                    {
                        LogicBuilding building = (LogicBuilding) buildings[i];
                        LogicBuildingData data = building.GetBuildingData();

                        if (data.IsTownHall() || data.IsTownHallVillage2())
                        {
                            townHallLevel = building.GetUpgradeLevel();
                        }
                    }

                    for (int i = 0; i < buildings.Size(); i++)
                    {
                        LogicBuilding building = (LogicBuilding) buildings[i];
                        LogicBuildingData data = building.GetBuildingData();

                        if (!data.IsTownHall() && !data.IsTownHallVillage2())
                        {
                            int maxUpgLevel = data.GetMaxUpgradeLevelForTownHallLevel(townHallLevel);

                            if (building.IsLocked())
                            {
                                if (!building.CanUnlock(false))
                                    continue;

                                building.StartConstructing(false);
                            }

                            if (maxUpgLevel == -1)
                                maxUpgLevel = 0;

                            if (maxUpgLevel > 0)
                            {
                                if (data.GetAmountCanBeUpgraded(maxUpgLevel) != 0)
                                {
                                    if (building.IsConstructing())
                                        building.FinishConstruction(false, true);

                                    building.SetUpgradeLevel(maxUpgLevel - 1);

                                    if (gameObjectManager.GetAvailableBuildingUpgradeCount(building) == 0)
                                        --maxUpgLevel;
                                }
                            }

                            if (building.IsConstructing())
                                building.FinishConstruction(false, true);

                            building.SetUpgradeLevel(maxUpgLevel);
                        }
                    }

                    for (int i = 0; i < traps.Size(); i++)
                    {
                        LogicTrap trap = (LogicTrap) traps[i];
                        LogicTrapData data = trap.GetTrapData();

                        int maxUpgLevel = data.GetMaxUpgradeLevelForTownHallLevel(townHallLevel);

                        if (maxUpgLevel == -1)
                            maxUpgLevel = 0;
                        if (trap.IsConstructing())
                            trap.FinishConstruction(false);

                        trap.SetUpgradeLevel(maxUpgLevel);
                        trap.RepairTrap();
                    }

                    LogicDebugCommand.UpgradeComponentsHelper(level);
                    break;
                }
                case LogicDebugActionType.REMOVE_UNITS:
                    LogicDebugCommand.AddUnits(level, 0, 0, false);
                    break;
                case LogicDebugActionType.DISARM_TRAPS:
                {
                    LogicArrayList<LogicGameObject> traps = level.GetGameObjectManager().GetGameObjects(LogicGameObjectType.TRAP);

                    for (int i = 0; i < traps.Size(); i++)
                    {
                        LogicTrap trap = (LogicTrap) traps[i];

                        if (trap.IsConstructing())
                            trap.FinishConstruction(true);
                        if (!trap.IsDisarmed())
                            trap.DisarmTrap();
                    }

                    break;
                }
                case LogicDebugActionType.REMOVE_OBSTACLES:
                {
                    LogicGameObjectManager gameObjectManager = level.GetGameObjectManager();
                    LogicArrayList<LogicGameObject> obstacles = gameObjectManager.GetGameObjects(LogicGameObjectType.OBSTACLE);

                    while (obstacles.Size() > 0)
                        gameObjectManager.RemoveGameObject(obstacles[0]);

                    break;
                }

                case LogicDebugActionType.RESET_HERO_LEVELS:
                    if (level.GetState() == 1)
                    {
                        LogicDataTable heroTable = LogicDataTables.GetTable(LogicDataType.HERO);

                        for (int i = 0; i < heroTable.GetItemCount(); i++)
                        {
                            LogicHeroData data = (LogicHeroData) heroTable.GetItemAt(i);

                            if (playerAvatar.GetHeroState(data) != 0)
                            {
                                playerAvatar.CommodityCountChangeHelper(1, data, -99);
                            }
                        }
                    }

                    break;
                case LogicDebugActionType.COLLECT_WAR_RESOURCES:
                {
                    LogicArrayList<LogicGameObject> buildings = level.GetGameObjectManager().GetGameObjects(LogicGameObjectType.BUILDING);

                    for (int i = 0; i < buildings.Size(); i++)
                    {
                        LogicBuilding building = (LogicBuilding) buildings[i];

                        if (building.GetBuildingData() == LogicDataTables.GetAllianceCastleData())
                        {
                            building.GetWarResourceStorageComponent().CollectResources();
                        }
                    }

                    break;
                }

                case LogicDebugActionType.SET_RANDOM_TROPHIES:
                    if (level.GetVillageType() == 0)
                    {
                        LogicRandom random = new LogicRandom(level.GetLogicTime().GetTick());

                        int prevScore = playerAvatar.GetScore();
                        int score = random.Rand(1000) + 2000;

                        playerAvatar.SetScore(score);

                        LogicLeagueData prevLeagueData = playerAvatar.GetLeagueTypeData();
                        LogicGamePlayUtil.UpdateLeagueRank(playerAvatar, score, false);

                        playerAvatar.GetChangeListener()
                                    .ScoreChanged(playerAvatar.GetAllianceId(), score - prevScore, 0, true, playerAvatar.GetLeagueTypeData(), prevLeagueData, 0);
                    }


                    break;
                case LogicDebugActionType.COMPLETE_WAR_TUTORIAL:
                    level.GetMissionManager().DebugCompleteAllTutorials(false, false, true);
                    break;
                case LogicDebugActionType.ADD_WAR_RESOURCES:
                {
                    LogicDataTable resourceTable = LogicDataTables.GetTable(LogicDataType.RESOURCE);

                    for (int i = 0; i < resourceTable.GetItemCount(); i++)
                    {
                        LogicResourceData data = (LogicResourceData) resourceTable.GetItemAt(i);

                        if (!data.IsPremiumCurrency() && data.GetWarResourceReferenceData() != null)
                        {
                            if (data.GetWarResourceReferenceData() != LogicDataTables.GetDarkElixirData())
                            {
                                playerAvatar.SetResourceCount(data, 1500000);
                                playerAvatar.GetChangeListener().CommodityCountChanged(0, data, playerAvatar.GetResourceCount(data));
                            }
                            else if (playerAvatar.IsDarkElixirUnlocked())
                            {
                                playerAvatar.SetResourceCount(data, 10000);
                                playerAvatar.GetChangeListener().CommodityCountChanged(0, data, playerAvatar.GetResourceCount(data));
                            }
                        }
                    }

                    break;
                }
                case LogicDebugActionType.REMOVE_WAR_RESOURCES:
                {
                    LogicDataTable resourceTable = LogicDataTables.GetTable(LogicDataType.RESOURCE);

                    for (int i = 0; i < resourceTable.GetItemCount(); i++)
                    {
                        LogicResourceData data = (LogicResourceData) resourceTable.GetItemAt(i);

                        if (!data.IsPremiumCurrency() && data.GetWarResourceReferenceData() != null)
                        {
                            playerAvatar.SetResourceCount(data, 0);
                            playerAvatar.GetChangeListener().CommodityCountChanged(0, data, playerAvatar.GetResourceCount(data));
                        }
                    }

                    break;
                }
                case LogicDebugActionType.RESET_WAR_TUTORIAL:
                    level.DebugResetWarTutorials();
                    break;
                case LogicDebugActionType.ADD_UNIT:
                    LogicDebugCommand.AddUnits(level, 1, 0, true);
                    break;
                case LogicDebugActionType.SET_MAX_UNIT_SPELL_LEVELS:
                    LogicDebugCommand.UpgradeUnitsToMaxUpgradeLevel(level);
                    break;
                case LogicDebugActionType.REMOVE_ALL_AMMO:
                {
                    LogicArrayList<LogicGameObject> buildings = level.GetGameObjectManager().GetGameObjects(LogicGameObjectType.BUILDING);

                    for (int i = 0; i < buildings.Size(); i++)
                    {
                        LogicBuilding building = (LogicBuilding) buildings[i];
                        LogicCombatComponent combatComponent = building.GetCombatComponent(false);

                        if (combatComponent != null)
                        {
                            if (combatComponent.UseAmmo())
                            {
                                combatComponent.RemoveAmmo();
                            }
                        }
                    }

                    break;
                }
                case LogicDebugActionType.RESET_ALL_LAYOUTS:
                    level.SetWarBase(false);
                    level.SetActiveLayout(0, 0);
                    level.SetActiveLayout(0, 1);
                    level.SetActiveWarLayout(0);

                    for (int i = 0; i < 7; i++)
                    {
                        LogicArrayList<LogicGameObject> gameObjects = new LogicArrayList<LogicGameObject>(500);
                        LogicGameObjectFilter filter = new LogicGameObjectFilter();

                        filter.AddGameObjectType(LogicGameObjectType.BUILDING);
                        filter.AddGameObjectType(LogicGameObjectType.TRAP);
                        filter.AddGameObjectType(LogicGameObjectType.DECO);

                        level.GetGameObjectManager().GetGameObjects(gameObjects, filter);

                        for (int j = 0; j < gameObjects.Size(); j++)
                        {
                            LogicGameObject gameObject = gameObjects[j];

                            if (i != 0)
                                gameObject.SetPositionLayoutXY(-1, -1, i, false);
                            gameObject.SetPositionLayoutXY(-1, -1, i, true);
                        }

                        filter.Destruct();
                        gameObjects.Destruct();

                        level.SetLayoutState(i, level.GetVillageType(), 0);
                    }

                    break;
                case LogicDebugActionType.LOCK_CLAN_CASTLE:
                    level.GetGameObjectManagerAt(0).GetAllianceCastle().Lock();
                    break;
                case LogicDebugActionType.RANDOM_RESOURCES_TROPHY_XP:
                {
                    LogicRandom rnd = new LogicRandom(level.GetLogicTime().GetTick());

                    int prevDiamonds = playerAvatar.GetDiamonds();
                    int genDiamonds = rnd.Rand(400) + 100;

                    playerAvatar.SetDiamonds(genDiamonds);

                    if (prevDiamonds != genDiamonds)
                    {
                        playerAvatar.GetChangeListener().FreeDiamondsAdded(genDiamonds - prevDiamonds, 3);
                    }

                    LogicDataTable resourceTable = LogicDataTables.GetTable(LogicDataType.RESOURCE);

                    for (int i = 0; i < resourceTable.GetItemCount(); i++)
                    {
                        LogicResourceData data = (LogicResourceData) resourceTable.GetItemAt(i);

                        if (!data.IsPremiumCurrency())
                        {
                            if (data == LogicDataTables.GetDarkElixirData())
                            {
                                if (!playerAvatar.IsDarkElixirUnlocked())
                                {
                                    continue;
                                }
                            }

                            int count = rnd.Rand(playerAvatar.GetResourceCap(data));

                            playerAvatar.SetResourceCount(data, count);
                            playerAvatar.GetChangeListener().CommodityCountChanged(0, data, count);
                        }
                    }

                    break;
                }
                case LogicDebugActionType.LOAD_LEVEL:
                    LogicDebugUtil.LoadDebugJSON(level, this.m_debugString);
                    break;
                case LogicDebugActionType.UPGRADE_BUILDING:
                    LogicDebugUtil.SetBuildingUpgradeLevel(level, this.m_globalId, this.m_debugInt, level.GetVillageType());
                    break;
                case LogicDebugActionType.UPGRADE_BUILDINGS:
                    if (this.m_globalIds != null && this.m_intArrayArg2 != null)
                    {
                        for (int i = 0, count = LogicMath.Min(this.m_globalIds.Size(), this.m_intArrayArg2.Size()); i < count; i++)
                        {
                            LogicDebugUtil.SetBuildingUpgradeLevel(level, this.m_globalIds[i], this.m_intArrayArg2[i], level.GetVillageType());
                        }
                    }

                    break;
                case LogicDebugActionType.ADD_1000_CLAN_XP:
                    playerAvatar.GetChangeListener().AllianceXpGained(1000);
                    break;
                case LogicDebugActionType.RESET_ALL_TUTORIALS:
                    level.GetMissionManager().DebugResetAllTutorials();
                    break;
                case LogicDebugActionType.ADD_1000_TROPHIES:
                    LogicDebugCommand.ScoreChangeHelper(level, playerAvatar, 1000);
                    break;
                case LogicDebugActionType.REMOVE_1000_TROPHIES:
                    LogicDebugCommand.ScoreChangeHelper(level, playerAvatar, -1000);
                    break;
                case LogicDebugActionType.CAUSE_DAMAGE:
                {
                    LogicArrayList<LogicGameObject> buildings = level.GetGameObjectManager().GetGameObjects(0);

                    for (int i = 0; i < buildings.Size(); i++)
                    {
                        LogicBuilding building = (LogicBuilding) buildings[i];
                        LogicHitpointComponent hitpointComponent = building.GetHitpointComponent();

                        if (hitpointComponent != null)
                            hitpointComponent.CauseDamage(1, 0, null);
                    }

                    break;
                }
                case LogicDebugActionType.SET_MAX_HERO_LEVELS:
                    if (level.GetState() == 1)
                    {
                        LogicDataTable heroTable = LogicDataTables.GetTable(LogicDataType.HERO);

                        for (int i = 0; i < heroTable.GetItemCount(); i++)
                        {
                            LogicHeroData data = (LogicHeroData)heroTable.GetItemAt(i);

                            if (playerAvatar.GetHeroState(data) != 0)
                            {
                                playerAvatar.CommodityCountChangeHelper(1, data, 99);
                            }
                        }
                    }

                    break;
                case LogicDebugActionType.ADD_PRESET_TROOPS:
                    LogicDebugUtil.AddDebugTroopsPreset(level, playerAvatar.GetTownHallLevel(), playerAvatar);
                    break;
                case LogicDebugActionType.TOGGLE_INVULNERABILITY:
                    bool on = !level.GetInvulnerabilityEnabled();
                    level.SetInvulnerabilityEnabled(on);
                    Debugger.HudPrint(on ? "Invulnerability is ON" : "Invulnerability is OFF");
                    break;
                case LogicDebugActionType.ADD_GEMS:
                {
                    int addCount = 50000 - playerAvatar.GetDiamonds();

                    if (addCount > 0)
                    {
                        playerAvatar.SetDiamonds(50000);
                        playerAvatar.GetChangeListener().FreeDiamondsAdded(addCount, 3);
                    }

                    break;
                }
                case LogicDebugActionType.PAUSE_ALL_BOOSTS:
                    if (LogicDataTables.GetGlobals().UseNewTraining())
                    {
                        LogicGameObjectManager goManager = level.GetGameObjectManager();
                        LogicUnitProduction unitProduction = goManager.GetUnitProduction();
                        LogicUnitProduction spellProduction = goManager.GetSpellProduction();

                        if (unitProduction.GetRemainingBoostTimeSecs() > 0)
                            unitProduction.SetBoostPause(true);
                        if (spellProduction.GetRemainingBoostTimeSecs() > 0)
                            spellProduction.SetBoostPause(true);
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        LogicArrayList<LogicGameObject> gameObjects = level.GetGameObjectManagerAt(i).GetGameObjects(LogicGameObjectType.BUILDING);

                        for (int j = 0; j < gameObjects.Size(); j++)
                        {
                            LogicBuilding building = (LogicBuilding) gameObjects[j];

                            if (building.CanStopBoost() && building.GetRemainingBoostTime() > 0)
                                building.SetBoostPause(true);
                        }
                    }

                    break;
                case LogicDebugActionType.TRAVEL:
                {
                    LogicVillageObject villageObject = level.GetGameObjectManagerAt(0).GetShipyard();

                    if (villageObject != null && villageObject.GetUpgradeLevel() == 0)
                    {
                        villageObject.StartUpgrading(false);
                        villageObject.FinishConstruction(false, false);
                    }

                    LogicSetCurrentVillageCommand setCurrentVillageCommand = new LogicSetCurrentVillageCommand(level.GetVillageType() == 0 ? 1 : 0);

                    setCurrentVillageCommand.ChangeVillage(level, true);
                    setCurrentVillageCommand.Destruct();

                    break;
                }
                case LogicDebugActionType.TOGGLE_RED:
                {
                    int redPackageCount = playerAvatar.GetRedPackageCount();
                    int state;

                    if (redPackageCount == 3)
                    {
                        state = 0;
                    }
                    else
                    {
                        state = 19;

                        for (int i = 0, offset = 4; i < 3; i++)
                        {
                            if (redPackageCount > offset - 4)
                                state |= offset;
                            offset *= 2;
                        }
                    }

                    playerAvatar.SetRedPackageState(state);
                    
                    break;
                }
                case LogicDebugActionType.COMPLETE_HOME_TUTORIALS:
                    level.GetMissionManager().DebugCompleteAllTutorials(true, true, true);
                    break;
                case LogicDebugActionType.UNLOCK_SHIPYARD:
                {
                    LogicVillageObject shipyard = level.GetGameObjectManagerAt(0).GetShipyard();

                    if (shipyard == null)
                        return -31;

                    if (shipyard.GetUpgradeLevel() <= 0)
                    {
                        shipyard.StartUpgrading(false);

                        if (!shipyard.SpeedUpCostruction())
                            return -32;

                        level.GetMissionManager().DebugCompleteAllTutorials(false, true, true);
                    }

                    break;
                }
                case LogicDebugActionType.GIVE_REENGAGEMENT_LOOT_FOR_30_DAYS:
                    level.ReengageLootCart(2592000);
                    break;
                case LogicDebugActionType.ADD_FREE_UNITS:
                    LogicDebugCommand.AddFreeUnit(level, level.GetVillageType(), playerAvatar);
                    break;
                case LogicDebugActionType.RANDOM_ALLIANCE_EXP_LEVEL:
                    playerAvatar.SetAllianceLevel(level.GetLogicTime().GetTotalMS() % 25);
                    break;
            }

            return 0;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_debugAction = (LogicDebugActionType) stream.ReadInt();
            this.m_debugString = stream.ReadString(900000);
            this.m_globalId = stream.ReadInt();
            this.m_debugInt = stream.ReadInt();

            int intArrayArg1Size = stream.ReadInt();

            if (intArrayArg1Size > 0)
            {
                this.m_globalIds.EnsureCapacity(intArrayArg1Size);

                for (int i = 0; i < intArrayArg1Size; i++)
                {
                    this.m_globalIds[i] = stream.ReadInt();
                }
            }

            int intArrayArg2Size = stream.ReadInt();

            if (intArrayArg2Size > 0)
            {
                this.m_intArrayArg2.EnsureCapacity(intArrayArg2Size);

                for (int i = 0; i < intArrayArg2Size; i++)
                {
                    this.m_intArrayArg2[i] = stream.ReadInt();
                }
            }

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt((int) this.m_debugAction);
            encoder.WriteString(this.m_debugString);
            encoder.WriteInt(this.m_globalId);
            encoder.WriteInt(this.m_debugInt);

            if (this.m_globalIds != null)
            {
                encoder.WriteInt(this.m_globalIds.Size());

                for (int i = 0; i < this.m_globalIds.Size(); i++)
                {
                    encoder.WriteInt(this.m_globalIds[i]);
                }
            }
            else
            {
                encoder.WriteInt(0);
            }

            if (this.m_intArrayArg2 != null)
            {
                encoder.WriteInt(this.m_intArrayArg2.Size());

                for (int i = 0; i < this.m_intArrayArg2.Size(); i++)
                {
                    encoder.WriteInt(this.m_intArrayArg2[i]);
                }
            }
            else
            {
                encoder.WriteInt(0);
            }

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.DEBUG;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_debugString = null;
            this.m_globalIds = null;
            this.m_intArrayArg2 = null;
        }

        public override LogicJSONObject GetJSONForReplay()
        {
            LogicJSONObject jsonObject = new LogicJSONObject();

            jsonObject.Put("base", base.GetJSONForReplay());
            jsonObject.Put("dA", new LogicJSONNumber((int) this.m_debugAction));
            jsonObject.Put("gi", new LogicJSONNumber(this.m_globalId));
            jsonObject.Put("int", new LogicJSONNumber(this.m_debugInt));

            if (!string.IsNullOrEmpty(this.m_debugString))
            {
                Debugger.Warning("DebugCommand m_pString not saved for replay");
            }

            if (this.m_globalIds != null && this.m_globalIds.Size() > 0)
            {
                Debugger.Warning("DebugCommand globalsID's not saved for replay");
            }

            return jsonObject;
        }

        public override void LoadFromJSON(LogicJSONObject jsonRoot)
        {
            LogicJSONObject baseObject = jsonRoot.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("Replay LogicDebugCommand load failed! Base missing!");
            }

            base.LoadFromJSON(baseObject);

            LogicJSONNumber debugActionObject = jsonRoot.GetJSONNumber("dA");

            if (debugActionObject != null)
            {
                this.m_debugAction = (LogicDebugActionType) debugActionObject.GetIntValue();
            }

            LogicJSONNumber globalIdNumber = jsonRoot.GetJSONNumber("gi");

            if (globalIdNumber != null)
            {
                this.m_globalId = globalIdNumber.GetIntValue();
            }

            LogicJSONNumber intArgNumber = jsonRoot.GetJSONNumber("int");

            if (intArgNumber != null)
            {
                this.m_debugInt = intArgNumber.GetIntValue();
            }
        }

        public static void AddUnits(LogicLevel level, int unitCount, int spellCount, bool requireDonation)
        {
            LogicCalendar calendar = level.GetGameMode().GetCalendar();
            LogicDataTable characterTable = LogicDataTables.GetTable(LogicDataType.CHARACTER);
            LogicDataTable spellTable = LogicDataTables.GetTable(LogicDataType.SPELL);
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            int villageType = level.GetVillageType();
            int commodityType = villageType == 1 ? 7 : 0;

            for (int i = 0; i < characterTable.GetItemCount(); i++)
            {
                LogicCharacterData data = (LogicCharacterData) characterTable.GetItemAt(i);

                if (level.GetVillageType() == 1)
                {
                    unitCount = data.GetUnitsInCamp(playerAvatar.GetUnitUpgradeLevel(data));
                }

                if (!data.IsSecondaryTroop())
                {
                    if (calendar.IsProductionEnabled(data))
                    {
                        if (data.GetVillageType() == villageType && (!requireDonation || !data.IsDonationDisabled()))
                        {
                            playerAvatar.SetCommodityCount(commodityType, data, unitCount);
                            playerAvatar.GetChangeListener().CommodityCountChanged(commodityType, data, unitCount);
                        }
                    }
                }
            }

            for (int i = 0; i < spellTable.GetItemCount(); i++)
            {
                LogicSpellData data = (LogicSpellData) spellTable.GetItemAt(i);

                if (calendar.IsProductionEnabled(data) && data.GetVillageType() == 0)
                {
                    playerAvatar.SetCommodityCount(commodityType, data, spellCount);
                    playerAvatar.GetChangeListener().CommodityCountChanged(commodityType, data, spellCount);
                }
            }

            if (villageType == 1)
            {
                level.GetComponentManagerAt(1).DebugVillage2UnitAdded(true);
            }
        }

        public static void ScoreChangeHelper(LogicLevel level, LogicClientAvatar playerAvatar, int gain)
        {
            if (level.GetVillageType() == 1)
            {
                if (gain + playerAvatar.GetDuelScore() > 6000)
                    gain = 6000 - playerAvatar.GetDuelScore();
                if (gain + playerAvatar.GetDuelScore() < 0)
                    gain = -playerAvatar.GetDuelScore();

                playerAvatar.SetDuelScore(playerAvatar.GetDuelScore() + gain);
                playerAvatar.GetChangeListener().DuelScoreChanged(playerAvatar.GetAllianceId(), gain, -1, false);
            }
            else
            {
                if (gain + playerAvatar.GetScore() > 6000)
                    gain = 6000 - playerAvatar.GetScore();
                if (gain + playerAvatar.GetScore() < 0)
                    gain = -playerAvatar.GetScore();

                int score = playerAvatar.GetScore() + gain;

                playerAvatar.SetScore(score);

                LogicLeagueData prevLeagueData = playerAvatar.GetLeagueTypeData();
                LogicGamePlayUtil.UpdateLeagueRank(playerAvatar, score, false);

                playerAvatar.GetChangeListener().ScoreChanged(playerAvatar.GetAllianceId(), gain, 0, true, playerAvatar.GetLeagueTypeData(), prevLeagueData, 0);
            }
        }

        public static void UpgradeUnitsToMaxUpgradeLevel(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();
            LogicDataTable characterTable = LogicDataTables.GetTable(LogicDataType.CHARACTER);
            LogicDataTable spellTable = LogicDataTables.GetTable(LogicDataType.SPELL);

            for (int i = 0; i < characterTable.GetItemCount(); i++)
            {
                LogicCharacterData data = (LogicCharacterData) characterTable.GetItemAt(i);

                if (!data.IsSecondaryTroop())
                {
                    int maxUpgLevel = data.GetUpgradeLevelCount() - 1;
                    int upgLevel = playerAvatar.GetUnitUpgradeLevel(data);

                    playerAvatar.CommodityCountChangeHelper(1, data, maxUpgLevel - upgLevel);
                }
            }

            for (int i = 0; i < spellTable.GetItemCount(); i++)
            {
                LogicSpellData data = (LogicSpellData) spellTable.GetItemAt(i);

                if (data.IsProductionEnabled())
                {
                    int maxUpgLevel = data.GetUpgradeLevelCount() - 1;
                    int upgLevel = playerAvatar.GetUnitUpgradeLevel(data);

                    playerAvatar.CommodityCountChangeHelper(1, data, maxUpgLevel - upgLevel);
                }
            }
        }

        public static void UpgradeComponentsHelper(LogicLevel level)
        {
            int villageType = level.GetVillageType();

            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();
            LogicGameObjectManager gameObjectManager = level.GetGameObjectManagerAt(villageType);
            LogicComponentManager componentManager = level.GetComponentManagerAt(villageType);

            LogicBuilding laboratory = gameObjectManager.GetLaboratory();

            if (laboratory != null)
            {
                LogicUnitUpgradeComponent unitUpgradeComponent = laboratory.GetUnitUpgradeComponent();

                if (unitUpgradeComponent != null)
                {
                    LogicDataTable characterTable = LogicDataTables.GetTable(LogicDataType.CHARACTER);
                    LogicDataTable spellTable = LogicDataTables.GetTable(LogicDataType.SPELL);

                    for (int i = 0; i < characterTable.GetItemCount(); i++)
                    {
                        LogicCharacterData data = (LogicCharacterData) characterTable.GetItemAt(i);

                        if (data.GetVillageType() == villageType && !data.IsSecondaryTroop())
                        {
                            while (unitUpgradeComponent.CanStartUpgrading(data))
                            {
                                playerAvatar.CommodityCountChangeHelper(1, data, 1);
                            }
                        }
                    }

                    for (int i = 0; i < spellTable.GetItemCount(); i++)
                    {
                        LogicSpellData data = (LogicSpellData) spellTable.GetItemAt(i);

                        if (data.GetVillageType() == villageType)
                        {
                            while (unitUpgradeComponent.CanStartUpgrading(data))
                            {
                                playerAvatar.CommodityCountChangeHelper(1, data, 1);
                            }
                        }
                    }
                }
            }

            LogicArrayList<LogicComponent> heroBaseComponents = componentManager.GetComponents(LogicComponentType.HERO_BASE);

            for (int i = 0; i < heroBaseComponents.Size(); i++)
            {
                LogicHeroBaseComponent heroBaseComponent = (LogicHeroBaseComponent) heroBaseComponents[i];

                while (heroBaseComponent.CanStartUpgrading(false))
                {
                    playerAvatar.CommodityCountChangeHelper(1, heroBaseComponent.GetHeroData(), 1);
                }
            }
        }

        public static void AddFreeUnit(LogicLevel level, int villageType, LogicClientAvatar playerAvatar)
        {
            LogicArrayList<LogicDataType> dataTypes = new LogicArrayList<LogicDataType>();

            dataTypes.Add(LogicDataType.CHARACTER);
            dataTypes.Add(LogicDataType.SPELL);
            dataTypes.Add(LogicDataType.HERO);

            for (int i = 0; i < dataTypes.Size(); i++)
            {
                LogicDataTable table = LogicDataTables.GetTable(dataTypes[i]);

                for (int j = 0; j < table.GetItemCount(); j++)
                {
                    LogicCombatItemData combatItemData = (LogicCombatItemData) table.GetItemAt(j);

                    if (combatItemData.IsProductionEnabled() && combatItemData.GetVillageType() == villageType)
                        playerAvatar.SetFreeUnitCount(combatItemData, 1000000, level);
                }
            }
        }
    }

    public enum LogicDebugActionType
    {
        FAST_FORWARD_1_HOUR,
        FAST_FORWARD_24_HOUR,
        ADD_UNITS,
        ADD_RESOURCES,
        INCREASE_XP_LEVEL,
        UPGRADE_ALL_BUILDINGS,
        COMPLETE_TUTORIAL,
        UNLOCK_MAP,
        SHIELD_TO_HALF,
        FAST_FORWARD_1_MIN,
        INCREASE_TROPHIES,
        DECREASE_TROPHIES,
        ADD_ALLIANCE_UNITS,
        INCREASE_HERO_LEVELS,
        REMOVE_RESOURCES,
        RESET_MAP_PROGRESS,
        UNK_16,
        DEPLOY_ALL_TROOPS,
        UNK_18,
        ADD_100_TROPHIES,
        REMOVE_100_TROPHIES,
        UPGRADE_TO_MAX_FOR_TH,
        REMOVE_UNITS,
        UNK_23,
        DISARM_TRAPS,
        REMOVE_OBSTACLES,
        RESET_HERO_LEVELS,
        // PREVIOUS_ADD_WAR_RESOURCES
        COLLECT_WAR_RESOURCES = 28,
        SET_RANDOM_TROPHIES,
        COMPLETE_WAR_TUTORIAL,
        ADD_WAR_RESOURCES,
        REMOVE_WAR_RESOURCES,
        RESET_WAR_TUTORIAL,
        ADD_UNIT,
        SET_MAX_UNIT_SPELL_LEVELS,
        REMOVE_ALL_AMMO,
        RESET_ALL_LAYOUTS,
        LOCK_CLAN_CASTLE,
        UNK_39,
        RANDOM_RESOURCES_TROPHY_XP,
        UNK_41,
        LOAD_LEVEL,
        UPGRADE_BUILDING,
        UPGRADE_BUILDINGS,
        ADD_1000_CLAN_XP,
        RESET_ALL_TUTORIALS,
        ADD_1000_TROPHIES,
        REMOVE_1000_TROPHIES,
        CAUSE_DAMAGE = 50,
        SET_MAX_HERO_LEVELS,
        ADD_PRESET_TROOPS,
        TOGGLE_INVULNERABILITY,
        ADD_GEMS,
        PAUSE_ALL_BOOSTS,
        UNK_56,
        TRAVEL,
        TOGGLE_RED,
        COMPLETE_HOME_TUTORIALS,
        UNLOCK_SHIPYARD,
        GIVE_REENGAGEMENT_LOOT_FOR_30_DAYS,
        ADD_FREE_UNITS,
        RANDOM_ALLIANCE_EXP_LEVEL
    }
}