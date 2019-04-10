namespace Supercell.Magic.Logic.Level
{
    using System;
    using Supercell.Magic.Logic.Achievement;
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Avatar.Change;
    using Supercell.Magic.Logic.Battle;
    using Supercell.Magic.Logic.Calendar;
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Logic.Cooldown;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Home;
    using Supercell.Magic.Logic.Mission;
    using Supercell.Magic.Logic.Mode;
    using Supercell.Magic.Logic.Offer;
    using Supercell.Magic.Logic.Time;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Logic.Worker;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class LogicLevel
    {
        public const int VILLAGE_COUNT = 2;
        public const int EXPERIENCE_VERSION = 1;

        public const int LEVEL_WIDTH = 25600;
        public const int LEVEL_HEIGHT = 25600;
        public const int NPC_LEVEL_WIDTH = 22528;
        public const int NPC_LEVEL_HEIGHT = 22528;

        public const int TILEMAP_SIZE_X = 50;
        public const int TILEMAP_SIZE_Y = 50;

        private readonly LogicTime m_time;
        private LogicGameMode m_gameMode;
        private LogicClientHome m_home;
        private LogicAvatar m_homeOwnerAvatar;
        private LogicAvatar m_visitorAvatar;
        private LogicTileMap m_tileMap;
        private LogicNpcAttack m_npcAttack;
        private LogicRect m_playArea;
        private LogicLeagueData m_leagueData;
        private LogicLeagueData m_visitorLeagueData;
        private LogicLong m_revengeId;

        private readonly LogicGameObjectManager[] m_gameObjectManagers;
        private readonly LogicWorkerManager[] m_workerManagers;
        private LogicOfferManager m_offerManager;
        private LogicAchievementManager m_achievementManager;
        private LogicCooldownManager m_cooldownManager;
        private LogicMissionManager m_missionManager;
        private LogicBattleLog m_battleLog;
        private LogicGameListener m_gameListener;
        private LogicJSONObject m_levelJSON;

        private readonly LogicArrayList<int> m_layoutState;
        private readonly LogicArrayList<int> m_layoutCooldown;
        private readonly LogicArrayList<int> m_layoutStateVillage2;
        private readonly LogicArrayList<string> m_armyNames;
        private LogicArrayList<LogicHeroData> m_placedHeroData;
        private LogicArrayList<LogicCharacter> m_placedHero;
        private LogicArrayList<LogicDataSlot> m_unplacedObjects;

        private LogicArrayList<int> m_newShopBuildings;
        private LogicArrayList<int> m_newShopTraps;
        private LogicArrayList<int> m_newShopDecos;

        private int m_liveReplayUpdateFrequency;

        private int m_loadingVillageType;
        private int m_villageType;
        private int m_warLayout;
        private int m_activeLayout;
        private int m_activeLayoutVillage2;
        private int m_lastLeagueRank;
        private int m_lastAllianceLevel;
        private int m_lastSeasonSeen;
        private int m_lastSeenNews;
        private int m_waveNumber;
        private int m_experienceVersion;
        private int m_warTutorialsSeen;
        private int m_matchType;
        private int m_remainingClockTowerBoostTime;
        private int m_levelWidth;
        private int m_levelHeight;
        private int m_aliveBuildingCount;
        private int m_destructibleBuildingCount;
        private int m_shieldActivatedHours;
        private int m_previousAttackStars;

        private bool m_helpOpened;
        private bool m_warBase;
        private bool m_war;
        private bool m_editModeShown;
        private bool m_npcVillage;
        private bool m_androidClient;
        private bool m_battleStarted;
        private bool m_battleEndPending;
        private bool m_arrangedWarBase;
        private bool m_arrangedWar;
        private bool m_invulnerabilityEnabled;
        private bool m_lastLeagueShuffle;
        private bool m_attackShieldCostOpened;
        private bool m_layoutEditShownErase;
        private bool m_readyForAttack;
        private bool m_ignoreAttack;

        private bool m_feedbackTownHallDestroyed;
        private bool m_feedbackDestruction25;
        private bool m_feedbackDestruction50;
        private bool m_feedbackDestruction75;

        private string m_warTroopRequestMessage;
        private string m_troopRequestMessage;

        public LogicLevel(LogicGameMode gameMode)
        {
            this.m_gameMode = gameMode;

            this.m_troopRequestMessage = string.Empty;
            this.m_warTroopRequestMessage = string.Empty;
            this.m_lastSeenNews = -1;
            this.m_loadingVillageType = -1;
            this.m_readyForAttack = true;

            this.m_time = new LogicTime();
            this.m_gameListener = new LogicGameListener();
            this.m_achievementManager = new LogicAchievementManager(this);
            this.m_layoutState = new LogicArrayList<int>();
            this.m_armyNames = new LogicArrayList<string>(4);
            this.m_gameObjectManagers = new LogicGameObjectManager[LogicLevel.VILLAGE_COUNT];
            this.m_workerManagers = new LogicWorkerManager[LogicLevel.VILLAGE_COUNT];
            this.m_tileMap = new LogicTileMap(LogicLevel.TILEMAP_SIZE_X, LogicLevel.TILEMAP_SIZE_Y);

            for (int i = 0; i < LogicLevel.VILLAGE_COUNT; i++)
            {
                this.m_workerManagers[i] = new LogicWorkerManager(this);
                this.m_gameObjectManagers[i] = new LogicGameObjectManager(this.m_tileMap, this, i);
            }

            this.m_levelWidth = LogicLevel.LEVEL_WIDTH;
            this.m_levelHeight = LogicLevel.LEVEL_HEIGHT;
            this.m_offerManager = new LogicOfferManager(this);
            this.m_playArea = new LogicRect(3, 3, 47, 47);
            this.m_cooldownManager = new LogicCooldownManager();
            this.m_battleLog = new LogicBattleLog(this);
            this.m_missionManager = new LogicMissionManager(this);
            this.m_layoutState = new LogicArrayList<int>(8);
            this.m_layoutCooldown = new LogicArrayList<int>(8);
            this.m_layoutStateVillage2 = new LogicArrayList<int>(8);
            this.m_unplacedObjects = new LogicArrayList<LogicDataSlot>();
            this.m_newShopBuildings = new LogicArrayList<int>();
            this.m_newShopTraps = new LogicArrayList<int>();
            this.m_newShopDecos = new LogicArrayList<int>();

            LogicDataTable buildingTable = LogicDataTables.GetTable(LogicDataType.BUILDING);
            LogicDataTable trapTable = LogicDataTables.GetTable(LogicDataType.TRAP);
            LogicDataTable decoTable = LogicDataTables.GetTable(LogicDataType.DECO);

            this.m_newShopBuildings.EnsureCapacity(buildingTable.GetItemCount());

            for (int i = 0; i < buildingTable.GetItemCount(); i++)
            {
                this.m_newShopBuildings.Add(0);
            }

            this.m_newShopBuildings.EnsureCapacity(trapTable.GetItemCount());

            for (int i = 0; i < trapTable.GetItemCount(); i++)
            {
                this.m_newShopTraps.Add(0);
            }

            this.m_newShopBuildings.EnsureCapacity(decoTable.GetItemCount());

            for (int i = 0; i < decoTable.GetItemCount(); i++)
            {
                this.m_newShopDecos.Add(0);
            }

            if (LogicDataTables.GetGlobals().LiveReplayEnabled())
            {
                this.m_liveReplayUpdateFrequency = LogicTime.GetSecondsInTicks(LogicDataTables.GetGlobals().GetLiveReplayUpdateFrequencySecs());
            }

            for (int i = 0; i < 8; i++)
            {
                this.m_layoutState.Add(0);
            }
            
            for (int i = 0; i < 8; i++)
            {
                this.m_layoutCooldown.Add(0);
            }
            
            for (int i = 0; i < 8; i++)
            {
                this.m_layoutStateVillage2.Add(0);
            }
            
            for (int i = 0; i < 4; i++)
            {
                this.m_armyNames.Add(string.Empty);
            }
        }

        public LogicGameMode GetGameMode()
        {
            return this.m_gameMode;
        }

        public LogicCalendar GetCalendar()
        {
            return this.m_gameMode.GetCalendar();
        }

        public LogicConfiguration GetConfiguration()
        {
            return this.m_gameMode.GetConfiguration();
        }

        public LogicGameListener GetGameListener()
        {
            return this.m_gameListener;
        }

        public void SetGameListener(LogicGameListener listener)
        {
            this.m_gameListener = listener;
        }

        public bool GetInvulnerabilityEnabled()
        {
            return this.m_invulnerabilityEnabled;
        }

        public void SetInvulnerabilityEnabled(bool state)
        {
            this.m_invulnerabilityEnabled = state;
        }

        public bool IsEditModeShown()
        {
            return this.m_editModeShown;
        }

        public bool GetIgnoreAttack()
        {
            return this.m_ignoreAttack;
        }

        public void SetEditModeShown()
        {
            this.m_editModeShown = true;
        }

        public void SetShieldCostPopupShown(bool seen)
        {
            this.m_attackShieldCostOpened = seen;
        }

        public void SetHelpOpened(bool opened)
        {
            this.m_helpOpened = opened;
        }

        public void SetAttackShieldCostOpened(bool opened)
        {
            this.m_attackShieldCostOpened = opened;
        }

        public int GetPreviousAttackStars()
        {
            return this.m_previousAttackStars;
        }

        public int GetState()
        {
            if (this.m_gameMode != null)
                return this.m_gameMode.GetState();
            return 0;
        }

        public int GetMatchType()
        {
            return this.m_matchType;
        }

        public void SetMatchType(int matchType, LogicLong revengeId)
        {
            this.m_matchType = matchType;
            this.m_revengeId = revengeId;

            if (matchType == 2)
            {
                this.m_npcVillage = true;
                this.m_levelWidth = LogicLevel.NPC_LEVEL_WIDTH;
                this.m_levelHeight = LogicLevel.NPC_LEVEL_HEIGHT;
            }
        }

        public string GetTroopRequestMessage()
        {
            return this.m_troopRequestMessage;
        }

        public void SetTroopRequestMessage(string message)
        {
            this.m_troopRequestMessage = message;
        }

        public void SetWarTroopRequestMessage(string message)
        {
            this.m_warTroopRequestMessage = message;
        }

        public int GetRemainingClockTowerBoostTime()
        {
            return this.m_remainingClockTowerBoostTime;
        }

        public int GetWarLayout()
        {
            return this.m_warLayout;
        }

        public int GetActiveLayout(int villageType)
        {
            return villageType == 0 ? this.m_activeLayout : this.m_activeLayoutVillage2;
        }

        public int GetActiveLayout()
        {
            if (this.m_loadingVillageType != -1)
            {
                return this.m_loadingVillageType == 0 ? this.m_activeLayout : this.m_activeLayoutVillage2;
            }

            return this.m_villageType == 0 ? this.m_activeLayout : this.m_activeLayoutVillage2;
        }

        public int GetCurrentLayout()
        {
            if (!this.m_arrangedWar || this.m_gameMode.GetVisitType() == 5 || !this.m_arrangedWarBase)
            {
                if (this.m_matchType != 5 && this.m_gameMode.GetVisitType() != 4 && this.m_gameMode.GetVisitType() != 5 && this.m_gameMode.GetVisitType() != 1 &&
                    this.m_gameMode.GetVillageType() != 1)
                {
                    return this.m_villageType != 0 ? this.m_activeLayoutVillage2 : this.m_activeLayout;
                }

                return this.m_warLayout;
            }

            return 7;
        }

        public void SetActiveLayout(int layout, int villageType)
        {
            if (villageType == 0)
            {
                this.m_activeLayout = layout;
            }
            else
            {
                this.m_activeLayoutVillage2 = layout;
            }
        }

        public void SetActiveWarLayout(int layout)
        {
            this.m_warLayout = layout;
        }

        public void SetLayoutCooldownSecs(int index, int secs)
        {
            if ((index & 0xFFFFFFFE) != 6 && this.m_villageType == 0)
            {
                this.m_layoutCooldown[index] = 15 * secs;
            }
        }

        public int GetLayoutCooldown(int index)
        {
            return this.m_layoutCooldown[index];
        }

        public int GetLayoutState(int idx, int villageType)
        {
            return villageType != 1 ? this.m_layoutState[idx] : this.m_layoutStateVillage2[idx];
        }

        public void SetLayoutState(int layoutId, int villageType, int state)
        {
            (villageType == 0 ? this.m_layoutState : this.m_layoutStateVillage2)[layoutId] = state;
        }

        public int GetTownHallLevel(int villageType)
        {
            LogicBuilding townHall = this.m_gameObjectManagers[villageType].GetTownHall();

            if (townHall != null)
            {
                return townHall.GetUpgradeLevel();
            }

            return 0;
        }

        public int GetRequiredTownHallLevelForLayout(int layoutId, int villageType)
        {
            if (villageType <= -1)
            {
                villageType = this.m_villageType;
            }

            if (layoutId > 7)
            {
                Debugger.Warning("unknown layout in getRequiredTownHallLevelForLayout");
                return 10000;
            }

            switch (layoutId)
            {
                case 0:
                case 1:
                    return 0;
                case 2:
                case 4:
                    if (villageType == 1)
                    {
                        return LogicDataTables.GetGlobals().GetLayoutTownHallLevelVillage2Slot2();
                    }

                    return LogicDataTables.GetGlobals().GetLayoutTownHallLevelSlot2();
                case 3:
                case 5:
                    if (villageType == 1)
                    {
                        return LogicDataTables.GetGlobals().GetLayoutTownHallLevelVillage2Slot3();
                    }

                    return LogicDataTables.GetGlobals().GetLayoutTownHallLevelSlot3();
                default:
                    return 0;
            }
        }

        public void SaveLayout(int inputLayoutId, int outputLayoutId)
        {
            int villageType = this.m_villageType;

            if (outputLayoutId == 6 || outputLayoutId == 7)
            {
                villageType = 0;
            }

            LogicArrayList<LogicGameObject> gameObjects = new LogicArrayList<LogicGameObject>(500);
            LogicGameObjectFilter filter = new LogicGameObjectFilter();

            filter.AddGameObjectType(LogicGameObjectType.BUILDING);
            filter.AddGameObjectType(LogicGameObjectType.TRAP);
            filter.AddGameObjectType(LogicGameObjectType.DECO);

            this.m_gameObjectManagers[villageType].GetGameObjects(gameObjects, filter);

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                LogicGameObject gameObject = gameObjects[i];
                LogicVector2 layoutPosition = gameObject.GetPositionLayout(inputLayoutId, false);
                LogicVector2 editModePosition = gameObject.GetPositionLayout(inputLayoutId, true);

                gameObject.SetPositionLayoutXY(layoutPosition.m_x, layoutPosition.m_y, outputLayoutId, false);
                gameObject.SetPositionLayoutXY(editModePosition.m_x, editModePosition.m_y, outputLayoutId, true);

                if (gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                {
                    LogicCombatComponent combatComponent = gameObject.GetCombatComponent(false);

                    if (combatComponent != null)
                    {
                        if (combatComponent.HasAltAttackMode())
                        {
                            if (combatComponent.UseAltAttackMode(inputLayoutId, false) ^ combatComponent.UseAltAttackMode(outputLayoutId, false))
                            {
                                combatComponent.ToggleAttackMode(outputLayoutId, false);
                            }

                            if (combatComponent.UseAltAttackMode(inputLayoutId, true) ^ combatComponent.UseAltAttackMode(outputLayoutId, true))
                            {
                                combatComponent.ToggleAttackMode(outputLayoutId, true);
                            }
                        }

                        if (combatComponent.GetAttackerItemData().GetTargetingConeAngle() != 0)
                        {
                            int aimAngle1 = combatComponent.GetAimAngle(inputLayoutId, false);
                            int aimAngle2 = combatComponent.GetAimAngle(outputLayoutId, false);

                            if (aimAngle1 != aimAngle2)
                            {
                                combatComponent.ToggleAimAngle(aimAngle1 - aimAngle2, outputLayoutId, false);
                            }
                        }
                    }
                }
                else if (gameObject.GetGameObjectType() == LogicGameObjectType.TRAP)
                {
                    LogicTrap trap = (LogicTrap) gameObject;

                    if (trap.HasAirMode())
                    {
                        if (trap.IsAirMode(inputLayoutId, false) ^ trap.IsAirMode(outputLayoutId, false))
                        {
                            trap.ToggleAirMode(outputLayoutId, false);
                        }

                        if (trap.IsAirMode(inputLayoutId, true) ^ trap.IsAirMode(outputLayoutId, true))
                        {
                            trap.ToggleAirMode(outputLayoutId, true);
                        }
                    }
                }
            }

            gameObjects.Destruct();
            filter.Destruct();
        }

        public int GetBuildingCount(bool includeDestructed, bool includeLocked)
        {
            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjectManagers[this.m_villageType].GetGameObjects(LogicGameObjectType.BUILDING);
            int cnt = 0;

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                LogicBuilding building = (LogicBuilding) gameObjects[i];
                LogicBuildingData buildingData = building.GetBuildingData();
                LogicHitpointComponent hitpointComponent = building.GetHitpointComponent();

                if (includeLocked || !building.IsLocked())
                {
                    if (hitpointComponent != null)
                    {
                        if (!buildingData.IsWall())
                        {
                            if (includeDestructed)
                            {
                                ++cnt;
                            }
                            else
                            {
                                if (building.GetHitpointComponent().GetHitpoints() > 0)
                                {
                                    ++cnt;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (building.IsConstructing())
                    {
                        if (hitpointComponent != null)
                        {
                            if (!buildingData.IsWall())
                            {
                                if (includeDestructed)
                                {
                                    ++cnt;
                                }
                                else
                                {
                                    if (building.GetHitpointComponent().GetHitpoints() > 0)
                                    {
                                        ++cnt;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return cnt;
        }

        public int GetTombStoneCount()
        {
            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjectManagers[this.m_villageType].GetGameObjects(LogicGameObjectType.OBSTACLE);
            int cnt = 0;

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                if (((LogicObstacle) gameObjects[i]).GetObstacleData().IsTombstone())
                {
                    ++cnt;
                }
            }

            return cnt;
        }

        public int GetTallGrassCount()
        {
            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjectManagers[this.m_villageType].GetGameObjects(LogicGameObjectType.OBSTACLE);
            int cnt = 0;

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                if (((LogicObstacle) gameObjects[i]).GetObstacleData().IsTallGrass())
                {
                    ++cnt;
                }
            }

            return cnt;
        }

        public void DefenseStateEnded()
        {
            if (this.m_npcAttack != null)
            {
                this.m_npcAttack.Destruct();
                this.m_npcAttack = null;
            }

            this.SetVisitorAvatar(null);
        }

        public void DefenseStateStarted(LogicAvatar avatar)
        {
            this.SetVisitorAvatar(avatar);

            if (this.m_npcAttack != null)
            {
                this.m_npcAttack.Destruct();
                this.m_npcAttack = null;
            }

            this.m_npcAttack = new LogicNpcAttack(this);
            this.m_aliveBuildingCount = this.GetBuildingCount(false, true);
            this.m_destructibleBuildingCount = this.GetBuildingCount(true, false);

            if (this.m_battleLog != null)
            {
                this.m_battleLog.Destruct();
                this.m_battleLog = null;
            }

            this.m_battleLog = new LogicBattleLog(this);
            this.m_battleLog.CalculateAvailableResources(this, this.m_matchType);

            this.SetOwnerInformationToBattleLog();
        }

        public int GetUpdatedClockTowerBoostTime()
        {
            LogicBuilding clockTower = this.m_gameObjectManagers[1].GetClockTower();

            if (clockTower != null && !clockTower.IsConstructing())
            {
                return clockTower.GetRemainingBoostTime();
            }

            return 0;
        }

        public int GetUnplacedObjectCount(LogicData data)
        {
            if (this.m_unplacedObjects != null)
            {
                int cnt = 0;

                for (int i = 0; i < this.m_unplacedObjects.Size(); i++)
                {
                    if (this.m_unplacedObjects[i].GetData() == data)
                    {
                        ++cnt;
                    }
                }

                return cnt;
            }

            return 0;
        }

        public int GetUnplacedObjectCount(LogicData data, int upgradeLevel)
        {
            if (this.m_unplacedObjects != null)
            {
                int cnt = 0;

                for (int i = 0; i < this.m_unplacedObjects.Size(); i++)
                {
                    if (this.m_unplacedObjects[i].GetData() == data && this.m_unplacedObjects[i].GetCount() == upgradeLevel)
                    {
                        ++cnt;
                    }
                }

                return cnt;
            }

            return 0;
        }

        public bool RemoveUnplacedObject(LogicData data, int upgradeLevel)
        {
            if (this.m_unplacedObjects != null)
            {
                for (int i = 0; i < this.m_unplacedObjects.Size(); i++)
                {
                    LogicDataSlot slot = this.m_unplacedObjects[i];

                    if (slot.GetData() == data && slot.GetCount() == upgradeLevel)
                    {
                        slot.SetCount(slot.GetCount() - 1);
                        this.m_unplacedObjects.Remove(i);

                        return true;
                    }
                }
            }

            return false;
        }

        public int GetObjectCount(LogicGameObjectData data, int villageType)
        {
            int cnt = 0;

            if (this.m_unplacedObjects != null)
            {
                for (int i = 0; i < this.m_unplacedObjects.Size(); i++)
                {
                    if (this.m_unplacedObjects[i].GetData() == data)
                    {
                        ++cnt;
                    }
                }
            }

            return cnt + this.m_gameObjectManagers[data.GetVillageType()].GetGameObjectCountByData(data);
        }

        public LogicOfferManager GetOfferManager()
        {
            return this.m_offerManager;
        }

        public int GetWidth()
        {
            return this.m_levelWidth;
        }

        public int GetHeight()
        {
            return this.m_levelHeight;
        }

        public int GetWidthInTiles()
        {
            return this.m_tileMap.GetSizeX();
        }

        public int GetHeightInTiles()
        {
            return this.m_tileMap.GetSizeY();
        }

        public int GetExperienceVersion()
        {
            return this.m_experienceVersion;
        }

        public void SetExperienceVersion(int version)
        {
            this.m_experienceVersion = version;
        }

        public bool GetBattleEndPending()
        {
            return this.m_battleEndPending;
        }

        public void EndBattle()
        {
            this.m_battleEndPending = true;
        }

        public int GetVillageType()
        {
            return this.m_villageType;
        }

        public void SetVillageType(int villageType)
        {
            this.m_villageType = villageType;
            this.m_battleLog.SetVillageType(villageType);

            for (int i = 0; i < LogicLevel.VILLAGE_COUNT; i++)
            {
                this.m_gameObjectManagers[i].ChangeVillageType(i == villageType);
            }
        }

        public void SetLoadingVillageType(int villageType)
        {
            this.m_loadingVillageType = villageType;
        }

        public string GetArmyName(int armyId)
        {
            return this.m_armyNames[armyId];
        }

        public bool IsArrangedWar()
        {
            return this.m_arrangedWar;
        }

        public void SetArrangedWar(bool enabled)
        {
            this.m_arrangedWar = enabled;
        }

        public bool IsArrangedWarBase()
        {
            return this.m_arrangedWarBase;
        }

        public void SetArmyName(int armyId, string name)
        {
            if (name.Length > 16)
            {
                name = name.Substring(0, 16);
            }

            this.m_armyNames[armyId] = name;
        }

        public bool IsReadyForAttack()
        {
            return this.m_readyForAttack;
        }

        public LogicBattleLog GetBattleLog()
        {
            return this.m_battleLog;
        }

        public LogicTime GetLogicTime()
        {
            return this.m_time;
        }

        public LogicRect GetPlayArea()
        {
            return this.m_playArea;
        }

        public LogicAchievementManager GetAchievementManager()
        {
            return this.m_achievementManager;
        }

        public LogicMissionManager GetMissionManager()
        {
            return this.m_missionManager;
        }

        public LogicWorkerManager GetWorkerManager()
        {
            return this.m_workerManagers[this.m_villageType];
        }

        public LogicWorkerManager GetWorkerManagerAt(int index)
        {
            return this.m_workerManagers[index];
        }

        public LogicGameObjectManager GetGameObjectManager()
        {
            return this.m_gameObjectManagers[this.m_villageType];
        }

        public LogicGameObjectManager GetGameObjectManagerAt(int index)
        {
            return this.m_gameObjectManagers[index];
        }

        public LogicComponentManager GetComponentManager()
        {
            return this.m_gameObjectManagers[this.m_loadingVillageType < 0 ? this.m_villageType : this.m_loadingVillageType].GetComponentManager();
        }

        public LogicComponentManager GetComponentManagerAt(int villageType)
        {
            return this.m_gameObjectManagers[villageType].GetComponentManager();
        }

        public LogicCooldownManager GetCooldownManager()
        {
            return this.m_cooldownManager;
        }

        public LogicTileMap GetTileMap()
        {
            return this.m_tileMap;
        }

        public LogicClientAvatar GetPlayerAvatar()
        {
            if (this.GetState() != 1 && this.GetState() != 3)
            {
                return (LogicClientAvatar) this.m_visitorAvatar;
            }

            return (LogicClientAvatar) this.m_homeOwnerAvatar;
        }

        public LogicAvatar GetHomeOwnerAvatar()
        {
            return this.m_homeOwnerAvatar;
        }

        public LogicAvatarChangeListener GetHomeOwnerAvatarChangeListener()
        {
            return this.m_homeOwnerAvatar.GetChangeListener();
        }

        public LogicLeagueData GetHomeLeagueData()
        {
            if (this.m_gameMode.GetState() == 1 &&
                this.m_homeOwnerAvatar.IsClientAvatar())
            {
                return ((LogicClientAvatar) this.m_homeOwnerAvatar).GetLeagueTypeData();
            }

            return this.m_leagueData;
        }

        public LogicAvatar GetVisitorAvatar()
        {
            return this.m_visitorAvatar;
        }

        public LogicClientHome GetHome()
        {
            return this.m_home;
        }

        public void UpdateLastUsedArmy()
        {
            LogicClientAvatar playerAvatar = this.GetPlayerAvatar();

            if (this.m_villageType == 0)
            {
                playerAvatar.SetLastUsedArmy(playerAvatar.GetUnits(), playerAvatar.GetSpells());
            }
        }

        public void SetHome(LogicClientHome home, bool androidClient)
        {
            this.m_offerManager.Init();

            this.m_home = home;
            this.m_levelJSON = (LogicJSONObject) LogicJSONParser.Parse(home.GetHomeJSON());

            LogicJSONBoolean androidClientBoolean = this.m_levelJSON.GetJSONBoolean("android_client");
            LogicJSONBoolean warBaseBoolean = this.m_levelJSON.GetJSONBoolean("war_base");

            if (warBaseBoolean != null)
            {
                this.m_warBase = warBaseBoolean.IsTrue();
            }

            LogicJSONBoolean arrangedWarBoolean = this.m_levelJSON.GetJSONBoolean("arr_war_base");

            if (arrangedWarBoolean != null)
            {
                this.m_arrangedWarBase = arrangedWarBoolean.IsTrue();
            }

            LogicJSONNumber activeLayoutNumber = this.m_levelJSON.GetJSONNumber("active_layout");

            if (activeLayoutNumber != null)
            {
                this.m_activeLayout = activeLayoutNumber.GetIntValue();
            }

            LogicJSONNumber activeLayout2Number = this.m_levelJSON.GetJSONNumber("act_l2");

            if (activeLayout2Number != null)
            {
                this.m_activeLayoutVillage2 = activeLayout2Number.GetIntValue();
            }

            if (this.m_activeLayout < 0)
            {
                this.m_activeLayout = 0;
            }

            if (this.m_activeLayoutVillage2 < 0)
            {
                this.m_activeLayoutVillage2 = 0;
            }

            LogicJSONNumber warLayoutNumber = this.m_levelJSON.GetJSONNumber("war_layout");

            if (warLayoutNumber != null)
            {
                this.m_warLayout = warLayoutNumber.GetIntValue();
            }
            else if (this.m_warBase)
            {
                this.m_warLayout = 1;
            }

            if (this.m_warLayout < 0)
            {
                this.m_warLayout = 0;
            }

            for (int i = 0; i < this.m_layoutState.Size(); i++)
            {
                this.m_layoutState[i] = 0;
            }

            LogicJSONArray layoutStateArray = this.m_levelJSON.GetJSONArray("layout_state");

            if (layoutStateArray != null)
            {
                int arraySize = layoutStateArray.Size();

                for (int i = 0; i < this.m_layoutState.Size(); i++)
                {
                    if (i >= arraySize)
                    {
                        break;
                    }

                    LogicJSONNumber numObject = layoutStateArray.GetJSONNumber(i);

                    if (numObject != null)
                    {
                        int num = numObject.GetIntValue();

                        if (num > -1)
                        {
                            this.m_layoutState[i] = num;
                        }
                    }
                }
            }

            for (int i = 0; i < this.m_layoutStateVillage2.Size(); i++)
            {
                this.m_layoutStateVillage2[i] = 0;
            }

            LogicJSONArray layoutState2Array = this.m_levelJSON.GetJSONArray("layout_state2");

            if (layoutState2Array != null)
            {
                int arraySize = layoutState2Array.Size();

                for (int i = 0; i < this.m_layoutStateVillage2.Size(); i++)
                {
                    if (i >= arraySize)
                    {
                        break;
                    }

                    LogicJSONNumber numObject = layoutState2Array.GetJSONNumber(i);

                    if (numObject != null)
                    {
                        int num = numObject.GetIntValue();

                        if (num > -1)
                        {
                            this.m_layoutStateVillage2[i] = num;
                        }
                    }
                }
            }

            for (int i = 0; i < this.m_layoutCooldown.Size(); i++)
            {
                this.m_layoutCooldown[i] = 0;
            }

            LogicJSONArray layoutCooldownArray = this.m_levelJSON.GetJSONArray("layout_cooldown");

            if (layoutCooldownArray != null)
            {
                int arraySize = layoutCooldownArray.Size();

                for (int i = 0; i < this.m_layoutCooldown.Size(); i++)
                {
                    if (i >= arraySize)
                    {
                        break;
                    }

                    LogicJSONNumber numObject = layoutCooldownArray.GetJSONNumber(i);

                    if (numObject != null)
                    {
                        int num = LogicMath.Min(numObject.GetIntValue(), 15 * LogicDataTables.GetGlobals().GetChallengeBaseSaveCooldown());

                        if (num > -1)
                        {
                            this.m_layoutCooldown[i] = num;
                        }
                    }
                }
            }

            if (this.m_unplacedObjects != null)
            {
                this.m_unplacedObjects.Clear();
            }

            LogicJSONArray unplacedArray = this.m_levelJSON.GetJSONArray("unplaced");

            if (unplacedArray != null)
            {
                int arraySize = unplacedArray.Size();

                for (int i = 0; i < arraySize; i++)
                {
                    LogicDataSlot dataSlot = new LogicDataSlot(null, 0);
                    dataSlot.ReadFromJSON(unplacedArray.GetJSONObject(i));
                    this.AddUnplacedObject(dataSlot);
                }
            }

            this.m_gameMode.GetCalendar().LoadProgress(this.m_levelJSON);

            if (androidClient)
            {
                this.m_androidClient = true;
            }
            else
            {
                if (androidClientBoolean != null)
                {
                    this.m_androidClient = androidClientBoolean.IsTrue();
                }
            }

            LogicJSONNumber waveNumObject = this.m_levelJSON.GetJSONNumber("wave_num");

            if (waveNumObject != null)
            {
                if (this.GetState() != 1)
                {
                    this.m_waveNumber = waveNumObject.GetIntValue();
                }
            }

            LogicJSONBoolean arrangedWarObject = this.m_levelJSON.GetJSONBoolean("arrWar");

            if (arrangedWarObject != null)
            {
                this.m_arrangedWar = arrangedWarObject.IsTrue();
            }

            LogicJSONBoolean warObject = this.m_levelJSON.GetJSONBoolean("war");

            if (warObject != null)
            {
                this.m_war = warObject.IsTrue();
            }
            else
            {
                this.m_war = false;
            }

            LogicJSONBoolean directObject = this.m_levelJSON.GetJSONBoolean("direct");
            LogicJSONBoolean direct2Object = this.m_levelJSON.GetJSONBoolean("direct2");

            bool notDirectLevel = directObject == null || !directObject.IsTrue();

            if (!(notDirectLevel || !this.m_warBase))
            {
                this.m_matchType = 7;
                this.m_revengeId = null;
            }
            else if (direct2Object != null && direct2Object.IsTrue())
            {
                this.m_matchType = 8;
                this.m_revengeId = null;
            }
            else if (this.m_warBase || !notDirectLevel)
            {
                this.m_matchType = (notDirectLevel ? 2 : 0) + 3;
                this.m_revengeId = null;
            }

            if (LogicDataTables.GetGlobals().LoadVillage2AsSnapshot() && (this.m_matchType & 0xFFFFFFFE) == 8)
            {
                this.m_loadingVillageType = 0;

                do
                {
                    this.m_gameObjectManagers[this.m_loadingVillageType].LoadFromSnapshot(this.m_levelJSON);
                } while (++this.m_loadingVillageType < 2);

                this.m_loadingVillageType = -1;
            }
            else if (this.m_matchType == 3 || this.m_matchType == 5 || this.m_matchType == 7 ||
                     this.m_gameMode.GetVisitType() == 1 ||
                     this.m_gameMode.GetVisitType() == 2 ||
                     this.m_gameMode.GetVisitType() == 3 ||
                     this.m_gameMode.GetVisitType() == 4 ||
                     this.m_gameMode.GetVisitType() == 5)
            {
                this.m_gameObjectManagers[0].LoadFromSnapshot(this.m_levelJSON);

                if (this.m_matchType == 5 && LogicDataTables.GetGlobals().ReadyForWarAttackCheck())
                {
                    this.m_readyForAttack = false;
                }
            }
            else
            {
                this.m_loadingVillageType = 0;

                do
                {
                    this.m_gameObjectManagers[this.m_loadingVillageType].Load(this.m_levelJSON);
                } while (++this.m_loadingVillageType < 2);

                this.m_loadingVillageType = -1;
                this.m_cooldownManager.Load(this.m_levelJSON);
                this.m_offerManager.Load(this.m_levelJSON);
            }

            if (!this.m_npcVillage)
            {
                LogicJSONNumber expVerNumber = this.m_levelJSON.GetJSONNumber("exp_ver");

                if (expVerNumber != null)
                {
                    this.m_experienceVersion = expVerNumber.GetIntValue();
                }
                else
                {
                    this.m_experienceVersion = 0;
                }

                if (this.m_gameMode.GetState() != 5)
                {
                    while (this.m_experienceVersion < LogicLevel.EXPERIENCE_VERSION)
                    {
                        this.UpdateExperienceVersion(this.m_experienceVersion);
                    }
                }
            }

            this.m_aliveBuildingCount = this.GetBuildingCount(false, true);
            this.m_destructibleBuildingCount = this.GetBuildingCount(true, false);
        }

        public void SaveToJSON(LogicJSONObject jsonObject)
        {
            if (!this.m_npcVillage)
            {
                if (this.m_waveNumber > 0)
                {
                    jsonObject.Put("wave_num", new LogicJSONNumber(this.m_waveNumber));
                }

                if (this.m_experienceVersion > 0)
                {
                    jsonObject.Put("exp_ver", new LogicJSONNumber(this.m_experienceVersion));
                }

                if (this.m_androidClient)
                {
                    jsonObject.Put("android_client", new LogicJSONBoolean(true));
                }

                if (this.m_matchType == 5 || this.m_matchType == 7)
                {
                    jsonObject.Put("war", new LogicJSONBoolean(true));
                }

                if (this.m_matchType == 3 || this.m_matchType == 7)
                {
                    jsonObject.Put("direct", new LogicJSONBoolean(true));
                }

                if (this.m_matchType == 8)
                {
                    jsonObject.Put("direct2", new LogicJSONBoolean(true));
                }

                if (this.m_arrangedWar)
                {
                    jsonObject.Put("arrWar", new LogicJSONBoolean(true));
                }

                jsonObject.Put("active_layout", new LogicJSONNumber(this.m_activeLayout));
                jsonObject.Put("act_l2", new LogicJSONNumber(this.m_activeLayoutVillage2));

                if (this.m_warBase)
                {
                    if (LogicDataTables.GetGlobals().RevertBrokenWarLayouts())
                    {
                        /* if ( sub_1E436C(v22) != 1 )
                           {
                               this.m_warBase = false;
                           }
                        */
                    }

                    jsonObject.Put("war_layout", new LogicJSONNumber(this.m_warLayout));
                }

                LogicJSONArray layoutStateArray = new LogicJSONArray();

                for (int i = 0; i < this.m_layoutState.Size(); i++)
                {
                    layoutStateArray.Add(new LogicJSONNumber(this.m_layoutState[i]));
                }

                jsonObject.Put("layout_state", layoutStateArray);

                LogicJSONArray layoutState2Array = new LogicJSONArray();

                for (int i = 0; i < this.m_layoutStateVillage2.Size(); i++)
                {
                    layoutState2Array.Add(new LogicJSONNumber(this.m_layoutStateVillage2[i]));
                }

                jsonObject.Put("layout_state2", layoutState2Array);

                LogicJSONArray layoutCooldownArray = new LogicJSONArray();

                for (int i = 0; i < this.m_layoutCooldown.Size(); i++)
                {
                    layoutCooldownArray.Add(new LogicJSONNumber(this.m_layoutCooldown[i]));
                }

                jsonObject.Put("layout_cooldown", layoutCooldownArray);
            }

            for (int i = 0; i < LogicLevel.VILLAGE_COUNT; i++)
            {
                this.m_gameObjectManagers[i].Save(jsonObject);
            }

            if (!this.m_npcVillage)
            {
                this.m_cooldownManager.Save(jsonObject);
                this.SaveShopNewItems(jsonObject);

                jsonObject.Put("last_league_rank", new LogicJSONNumber(this.m_lastLeagueRank));
                jsonObject.Put("last_alliance_level", new LogicJSONNumber(this.m_lastAllianceLevel));
                jsonObject.Put("last_league_shuffle", new LogicJSONNumber(this.m_lastLeagueShuffle ? 1 : 0));
                jsonObject.Put("last_season_seen", new LogicJSONNumber(this.m_lastSeasonSeen));
                jsonObject.Put("last_news_seen", new LogicJSONNumber(this.m_lastSeenNews));

                if (this.m_troopRequestMessage.Length > 0)
                {
                    jsonObject.Put("troop_req_msg", new LogicJSONString(this.m_troopRequestMessage));
                }

                if (this.m_warTroopRequestMessage.Length > 0)
                {
                    jsonObject.Put("war_req_msg", new LogicJSONString(this.m_warTroopRequestMessage));
                }

                jsonObject.Put("war_tutorials_seen", new LogicJSONNumber(this.m_warTutorialsSeen));
                jsonObject.Put("war_base", new LogicJSONBoolean(this.m_warBase));
                jsonObject.Put("arr_war_base", new LogicJSONBoolean(this.m_arrangedWarBase));

                LogicJSONArray armyNameArray = new LogicJSONArray();

                for (int i = 0; i < this.m_armyNames.Size(); i++)
                {
                    armyNameArray.Add(new LogicJSONString(this.m_armyNames[i]));
                }

                jsonObject.Put("army_names", armyNameArray);

                int accountFlags = 0;

                if (this.m_helpOpened)
                {
                    accountFlags |= 1 << 0;
                }

                if (this.m_editModeShown)
                {
                    accountFlags |= 1 << 1;
                }

                if (this.m_attackShieldCostOpened)
                {
                    accountFlags |= 1 << 3;
                }

                jsonObject.Put("account_flags", new LogicJSONNumber(accountFlags));
                jsonObject.Put(this.GetPersistentBoolVariableName(0), new LogicJSONBoolean(this.m_layoutEditShownErase));

                if (this.m_unplacedObjects != null)
                {
                    if (this.m_unplacedObjects.Size() > 0)
                    {
                        LogicJSONArray unplacedArray = new LogicJSONArray();

                        for (int i = 0; i < this.m_unplacedObjects.Size(); i++)
                        {
                            LogicJSONObject obj = new LogicJSONObject();
                            this.m_unplacedObjects[i].WriteToJSON(obj);
                            unplacedArray.Add(obj);
                        }

                        jsonObject.Put("unplaced", unplacedArray);
                    }
                }

                this.m_gameMode.GetCalendar().SaveProgress(jsonObject);
            }
        }

        private void SaveShopNewItems(LogicJSONObject jsonObject)
        {
            LogicDataTable buildingTable = LogicDataTables.GetTable(LogicDataType.BUILDING);
            LogicDataTable trapTable = LogicDataTables.GetTable(LogicDataType.TRAP);
            LogicDataTable decoTable = LogicDataTables.GetTable(LogicDataType.DECO);

            int townHallLevelVillage2 = this.m_homeOwnerAvatar.GetVillage2TownHallLevel();
            int townHallLevel = this.m_homeOwnerAvatar.GetTownHallLevel();
            int expLevel = this.m_homeOwnerAvatar.GetExpLevel();

            LogicJSONArray newShopBuildingArray = new LogicJSONArray();

            for (int i = 0; i < this.m_newShopBuildings.Size(); i++)
            {
                LogicGameObjectData data = (LogicGameObjectData) buildingTable.GetItemAt(i);

                int currentNewItemCount = this.m_newShopBuildings[i];
                int unlockedShopItemCount = this.GetShopUnlockCount(data, data.GetVillageType() == 0 ? townHallLevel : townHallLevelVillage2);

                newShopBuildingArray.Add(new LogicJSONNumber(unlockedShopItemCount - currentNewItemCount));
            }

            jsonObject.Put("newShopBuildings", newShopBuildingArray);

            LogicJSONArray newShopTrapArray = new LogicJSONArray();

            for (int i = 0; i < this.m_newShopTraps.Size(); i++)
            {
                LogicGameObjectData data = (LogicGameObjectData) trapTable.GetItemAt(i);

                int currentNewItemCount = this.m_newShopTraps[i];
                int unlockedShopItemCount = this.GetShopUnlockCount(data, data.GetVillageType() == 0 ? townHallLevel : townHallLevelVillage2);

                newShopTrapArray.Add(new LogicJSONNumber(unlockedShopItemCount - currentNewItemCount));
            }

            jsonObject.Put("newShopTraps", newShopTrapArray);

            LogicJSONArray newShopDecoArray = new LogicJSONArray();

            for (int i = 0; i < this.m_newShopDecos.Size(); i++)
            {
                int currentNewItemCount = this.m_newShopDecos[i];
                int unlockedShopItemCount = this.GetShopUnlockCount(decoTable.GetItemAt(i), expLevel);

                newShopDecoArray.Add(new LogicJSONNumber(unlockedShopItemCount - currentNewItemCount));
            }

            jsonObject.Put("newShopDecos", newShopDecoArray);
        }

        public void LoadShopNewItems()
        {
            if (this.m_levelJSON != null)
            {
                for (int i = 0; i < this.m_newShopBuildings.Size(); i++)
                {
                    this.m_newShopBuildings[i] = 0;
                }

                for (int i = 0; i < this.m_newShopTraps.Size(); i++)
                {
                    this.m_newShopTraps[i] = 0;
                }

                for (int i = 0; i < this.m_newShopDecos.Size(); i++)
                {
                    this.m_newShopDecos[i] = 0;
                }

                LogicDataTable buildingTable = LogicDataTables.GetTable(LogicDataType.BUILDING);
                LogicDataTable trapTable = LogicDataTables.GetTable(LogicDataType.TRAP);
                LogicDataTable decoTable = LogicDataTables.GetTable(LogicDataType.DECO);

                int townHallLevelVillage2 = this.m_homeOwnerAvatar.GetVillage2TownHallLevel();
                int townHallLevel = this.m_homeOwnerAvatar.GetTownHallLevel();
                int expLevel = this.m_homeOwnerAvatar.GetExpLevel();

                LogicJSONArray buildingArray = this.m_levelJSON.GetJSONArray("newShopBuildings");

                if (buildingArray != null)
                {
                    for (int i = 0; i < this.m_newShopBuildings.Size(); i++)
                    {
                        LogicGameObjectData data = (LogicGameObjectData) buildingTable.GetItemAt(i);

                        int unlockedShopItemCount = this.GetShopUnlockCount(data, data.GetVillageType() == 0 ? townHallLevel : townHallLevelVillage2);

                        if (i < buildingArray.Size())
                        {
                            unlockedShopItemCount -= buildingArray.GetJSONNumber(i).GetIntValue();

                            if (unlockedShopItemCount < 0)
                            {
                                unlockedShopItemCount = 0;
                            }
                        }

                        this.m_newShopBuildings[i] = unlockedShopItemCount;
                    }
                }

                LogicJSONArray trapArray = this.m_levelJSON.GetJSONArray("newShopTraps");

                if (trapArray != null)
                {
                    for (int i = 0; i < this.m_newShopTraps.Size(); i++)
                    {
                        LogicGameObjectData data = (LogicGameObjectData) trapTable.GetItemAt(i);

                        int unlockedShopItemCount = this.GetShopUnlockCount(data, data.GetVillageType() == 0 ? townHallLevel : townHallLevelVillage2);

                        if (i < trapArray.Size())
                        {
                            unlockedShopItemCount -= trapArray.GetJSONNumber(i).GetIntValue();

                            if (unlockedShopItemCount < 0)
                            {
                                unlockedShopItemCount = 0;
                            }
                        }

                        this.m_newShopTraps[i] = unlockedShopItemCount;
                    }
                }

                LogicJSONArray decoArray = this.m_levelJSON.GetJSONArray("newShopDecos");

                if (decoArray != null)
                {
                    for (int i = 0; i < this.m_newShopDecos.Size(); i++)
                    {
                        int unlockedShopItemCount = this.GetShopUnlockCount(decoTable.GetItemAt(i), expLevel);

                        if (i < decoArray.Size())
                        {
                            unlockedShopItemCount -= decoArray.GetJSONNumber(i).GetIntValue();

                            if (unlockedShopItemCount < 0)
                            {
                                unlockedShopItemCount = 0;
                            }
                        }

                        this.m_newShopDecos[i] = unlockedShopItemCount;
                    }
                }
            }
        }

        public bool SetUnlockedShopItemCount(LogicGameObjectData data, int index, int count, int villageType)
        {
            if (data.GetVillageType() == villageType)
            {
                switch (data.GetDataType())
                {
                    case LogicDataType.BUILDING:
                        this.m_newShopBuildings[index] = count;
                        break;
                    case LogicDataType.TRAP:
                        this.m_newShopTraps[index] = count;
                        break;
                    case LogicDataType.DECO:
                        this.m_newShopDecos[index] = count;
                        break;
                    default: return false;
                }

                return true;
            }

            return false;
        }

        public int GetLastAllianceLevel()
        {
            return this.m_lastAllianceLevel;
        }

        public void SetLastAllianceLevel(int value)
        {
            this.m_lastAllianceLevel = value;
        }

        public void SetLastSeenNews(int lastSeenNews)
        {
            if (this.m_lastSeenNews < lastSeenNews)
            {
                this.m_lastSeenNews = lastSeenNews;
            }
        }

        public int GetLastSeenNews()
        {
            return this.m_lastSeenNews;
        }

        public int GetLastSeasonSeen()
        {
            return this.m_lastSeasonSeen;
        }

        public void SetLastSeasonSeen(int value)
        {
            this.m_lastSeasonSeen = value;
        }

        public int GetLastLeagueRank()
        {
            return this.m_lastLeagueRank;
        }

        public void SetLastLeagueRank(int value)
        {
            this.m_lastLeagueRank = value;
        }

        public bool IsLastLeagueShuffle()
        {
            return this.m_lastLeagueShuffle;
        }

        public void SetLastLeagueShuffle(bool state)
        {
            this.m_lastLeagueShuffle = state;
        }

        public void RefreshNewShopUnlocksExp()
        {
            int expLevel = this.m_homeOwnerAvatar.GetExpLevel();

            if (this.m_homeOwnerAvatar.GetExpLevel() > 0)
            {
                LogicDataTable table = LogicDataTables.GetTable(LogicDataType.DECO);

                for (int i = 0; i < this.m_newShopDecos.Size(); i++)
                {
                    LogicData data = table.GetItemAt(i);

                    int totalShopUnlock = this.GetShopUnlockCount(data, expLevel);
                    int shopUnlockCount = totalShopUnlock - this.GetShopUnlockCount(data, expLevel - 1);

                    if (shopUnlockCount > 0)
                    {
                        this.m_newShopDecos[i] += shopUnlockCount;
                    }
                }
            }
        }

        public void RefreshNewShopUnlocksTH(int villageType)
        {
            this.RefreshNewShopUnlocks(villageType);
        }

        public void RefreshNewShopUnlocks(int villageType)
        {
            LogicBuilding townHall = this.m_gameObjectManagers[villageType].GetTownHall();

            if (townHall != null)
            {
                int thUpgradeLevel = townHall.GetUpgradeLevel();

                if (thUpgradeLevel > 0)
                {
                    LogicDataTable buildingTable = LogicDataTables.GetTable(LogicDataType.BUILDING);

                    for (int i = 0; i < this.m_newShopBuildings.Size(); i++)
                    {
                        LogicGameObjectData data = (LogicGameObjectData) buildingTable.GetItemAt(i);

                        if (data.IsEnabledInVillageType(villageType))
                        {
                            int totalShopUnlock = this.GetShopUnlockCount(data, thUpgradeLevel);
                            int previousTotalShopUnlock = this.GetShopUnlockCount(data, thUpgradeLevel - 1);

                            if (totalShopUnlock > previousTotalShopUnlock)
                                this.m_newShopBuildings[i] += totalShopUnlock - previousTotalShopUnlock;
                        }
                    }

                    LogicDataTable trapTable = LogicDataTables.GetTable(LogicDataType.TRAP);

                    for (int i = 0; i < this.m_newShopTraps.Size(); i++)
                    {
                        LogicGameObjectData data = (LogicGameObjectData) trapTable.GetItemAt(i);

                        if (data.IsEnabledInVillageType(villageType))
                        {
                            int totalShopUnlock = this.GetShopUnlockCount(data, thUpgradeLevel);
                            int previousTotalShopUnlock = this.GetShopUnlockCount(data, thUpgradeLevel - 1);

                            if (totalShopUnlock > previousTotalShopUnlock)
                                this.m_newShopTraps[i] += totalShopUnlock - previousTotalShopUnlock;
                        }
                    }
                }
            }
        }

        public void RefreshResourceCaps()
        {
            if (this.m_homeOwnerAvatar != null && this.m_homeOwnerAvatar.IsClientAvatar())
            {
                LogicClientAvatar clientAvatar = (LogicClientAvatar) this.m_homeOwnerAvatar;
                LogicDataTable table = LogicDataTables.GetTable(LogicDataType.RESOURCE);

                for (int i = 0, cnt = 0; i < table.GetItemCount(); i++, cnt = 0)
                {
                    LogicResourceData data = (LogicResourceData) table.GetItemAt(i);

                    for (int j = 0; j < 2; j++)
                    {
                        if (data.GetWarResourceReferenceData() != null)
                        {
                            LogicArrayList<LogicComponent> components =
                                this.m_gameObjectManagers[j].GetComponentManager().GetComponents(LogicComponentType.WAR_RESOURCE_STORAGE);

                            for (int k = 0; k < components.Size(); k++)
                            {
                                LogicWarResourceStorageComponent resourceWarStorageComponent = (LogicWarResourceStorageComponent) components[k];

                                if (resourceWarStorageComponent.IsEnabled())
                                {
                                    cnt += resourceWarStorageComponent.GetMax(i);
                                }
                            }
                        }
                        else
                        {
                            LogicArrayList<LogicComponent> components =
                                this.m_gameObjectManagers[j].GetComponentManager().GetComponents(LogicComponentType.RESOURCE_STORAGE);

                            for (int k = 0; k < components.Size(); k++)
                            {
                                LogicResourceStorageComponent resourceStorageComponent = (LogicResourceStorageComponent) components[k];

                                if (resourceStorageComponent.IsEnabled())
                                {
                                    cnt += resourceStorageComponent.GetMax(i);
                                }
                            }
                        }
                    }

                    if (!data.IsPremiumCurrency())
                    {
                        clientAvatar.SetResourceCap(data, cnt);
                        clientAvatar.GetChangeListener().CommodityCountChanged(1, data, cnt);
                    }
                }
            }
        }

        public void SetHomeOwnerAvatar(LogicAvatar avatar)
        {
            this.m_homeOwnerAvatar = avatar;

            if (avatar != null)
            {
                avatar.SetLevel(this);

                if (avatar.IsClientAvatar())
                {
                    this.m_lastLeagueRank = ((LogicClientAvatar) avatar).GetLeagueType();
                }

                if (this.m_battleLog != null)
                {
                    if (avatar.GetName() != null)
                    {
                        this.m_battleLog.SetDefenderName(avatar.GetName());
                    }
                }

                this.m_gameObjectManagers[0].GetComponentManager().DivideAvatarUnitsToStorages(0);
                this.m_gameObjectManagers[1].GetComponentManager().DivideAvatarUnitsToStorages(1);

                if (this.m_matchType == 5)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    this.m_gameObjectManagers[0].GetComponentManager().AddAvatarAllianceUnitsToCastle();
                }
            }
        }

        public void SetVisitorAvatar(LogicAvatar avatar)
        {
            if (this.m_visitorAvatar != avatar && this.m_visitorAvatar != null)
            {
                this.m_visitorAvatar.Destruct();
                this.m_visitorAvatar = null;
            }

            this.m_visitorAvatar = avatar;

            if (avatar != null)
            {
                avatar.SetLevel(this);

                if (this.m_battleLog != null && avatar.IsClientAvatar())
                {
                    LogicClientAvatar clientAvatar = (LogicClientAvatar) this.m_visitorAvatar;

                    this.m_visitorLeagueData = clientAvatar.GetLeagueTypeData();
                    this.m_battleLog.SetAttackerStars(clientAvatar.GetStarBonusCounter());
                    this.m_battleLog.SetAttackerHomeId(clientAvatar.GetCurrentHomeId());
                    this.m_battleLog.SetAttackerName(clientAvatar.GetName());

                    if (avatar.IsInAlliance())
                    {
                        this.m_battleLog.SetAttackerAllianceId(clientAvatar.GetAllianceId());
                        this.m_battleLog.SetAttackerAllianceBadge(clientAvatar.GetAllianceBadgeId());
                        this.m_battleLog.SetAttackerAllianceLevel(clientAvatar.GetAllianceLevel());

                        string allianceName = clientAvatar.GetAllianceName();

                        if (allianceName != null)
                        {
                            this.m_battleLog.SetAttackerAllianceName(allianceName);
                        }
                    }
                    else
                    {
                        this.m_battleLog.SetAttackerAllianceBadge(-1);
                    }
                }
            }
        }

        public void SetOwnerInformationToBattleLog()
        {
            if (this.m_homeOwnerAvatar.IsClientAvatar())
            {
                LogicClientAvatar clientAvatar = (LogicClientAvatar) this.m_homeOwnerAvatar;

                if (clientAvatar.IsInAlliance())
                {
                    this.m_battleLog.SetDefenderAllianceId(clientAvatar.GetAllianceId());

                    string allianceName = clientAvatar.GetAllianceName();

                    if (allianceName != null)
                    {
                        this.m_battleLog.SetDefenderAllianceName(allianceName);
                    }

                    this.m_battleLog.SetDefenderAllianceBadge(clientAvatar.GetAllianceBadgeId());
                    this.m_battleLog.SetDefenderAllianceLevel(clientAvatar.GetAllianceLevel());
                }
                else
                {
                    this.m_battleLog.SetDefenderAllianceBadge(-1);
                }

                this.m_battleLog.SetDefenderHomeId(clientAvatar.GetCurrentHomeId());
            }
        }

        public void SetPersistentBool(int idx, bool value)
        {
            switch (idx)
            {
                case 0:
                    this.m_layoutEditShownErase = value;
                    break;
                default:
                    Debugger.Warning("setPersistentBool() index out of bounds");
                    break;
            }
        }

        public string GetPersistentBoolVariableName(int idx)
        {
            switch (idx)
            {
                case 0:
                    return "bool_layout_edit_shown_erase";
                default:
                    Debugger.Error("Boolean index out of bounds");
                    return null;
            }
        }

        public void AddUnplacedObject(LogicDataSlot obj)
        {
            if (this.m_unplacedObjects == null)
            {
                this.m_unplacedObjects = new LogicArrayList<LogicDataSlot>();
            }

            this.m_unplacedObjects.Add(obj);
        }

        public int GetShopUnlockCount(LogicData data, int arg)
        {
            int unlock = 0;

            switch (data.GetDataType())
            {
                case LogicDataType.BUILDING:
                    LogicBuildingData buildingData = (LogicBuildingData) data;

                    if (!buildingData.IsLocked())
                    {
                        unlock = LogicDataTables.GetTownHallLevel(arg).GetUnlockedBuildingCount(buildingData) - buildingData.GetStartingHomeCount();

                        if (unlock < 0)
                            unlock = 0;
                    }

                    break;
                case LogicDataType.TRAP:
                    unlock = LogicDataTables.GetTownHallLevel(arg).GetUnlockedTrapCount((LogicTrapData) data);
                    break;
                case LogicDataType.DECO:
                    LogicDecoData decoData = (LogicDecoData) data;

                    if (decoData.GetRequiredExpLevel() <= arg && decoData.IsInShop())
                    {
                        unlock = decoData.GetMaxCount();
                    }

                    break;
            }

            return unlock;
        }

        public void BattleStarted()
        {
            this.m_battleStarted = true;

            if (this.m_matchType == 4 && !LogicDataTables.GetGlobals().RemoveRevengeWhenBattleIsLoaded())
            {
                this.GetPlayerAvatar().GetChangeListener().RevengeUsed(this.m_revengeId);
            }

            if (this.GetState() != 5)
            {
                if (this.m_matchType <= 8)
                {
                    if (this.m_matchType == 1 ||
                        this.m_matchType == 3 ||
                        this.m_matchType == 4 ||
                        this.m_matchType == 7)
                    {
                        this.m_readyForAttack = true;

                        this.m_homeOwnerAvatar.GetChangeListener().AttackStarted();
                        this.GetPlayerAvatar().GetChangeListener().AttackStarted();
                    }
                    else if (this.m_matchType == 5 ||
                             this.m_matchType == 8)
                    {
                        LogicClientAvatar playerAvatar = this.GetPlayerAvatar();
                        LogicAvatarChangeListener listener = playerAvatar.GetChangeListener();

                        if (listener != null)
                        {
                            listener.BattleFeedback(4, 0);
                        }
                    }
                }
            }
        }

        public bool IsClockTowerBoostPaused()
        {
            LogicBuilding clockTower = this.m_gameObjectManagers[1].GetClockTower();

            if (clockTower != null)
            {
                return clockTower.IsBoostPaused();
            }

            return false;
        }

        public bool IsInCombatState()
        {
            int state = this.GetState();
            return state == 2 || state == 3 || state == 5;
        }

        public bool IsWarBase()
        {
            return this.m_warBase;
        }

        public void SetWarBase(bool enabled)
        {
            this.m_warBase = enabled;
        }

        public bool IsNpcVillage()
        {
            return this.m_npcVillage;
        }

        public void SetNpcVillage(bool enabled)
        {
            this.m_npcVillage = enabled;
        }

        public bool IsBuildingGearUpCapReached(LogicBuildingData data, bool canCallListener)
        {
            int townHallLevel = 0;

            if (this.m_gameObjectManagers[this.m_villageType].GetTownHall() != null)
            {
                townHallLevel = this.m_gameObjectManagers[this.m_villageType].GetTownHall().GetUpgradeLevel();
            }

            int unlockedGearupCount = LogicDataTables.GetTownHallLevel(townHallLevel).GetUnlockedBuildingGearupCount(data);
            int gearupCount = this.m_gameObjectManagers[this.m_villageType].GetGearUpBuildingCount(data);

            if (unlockedGearupCount <= gearupCount)
            {
                if (canCallListener)
                {
                    this.m_gameListener.BuildingCapReached(data);
                }

                return true;
            }

            return false;
        }

        public bool IsBuildingCapReached(LogicBuildingData data, bool canCallListener)
        {
            int townHallLevel = 0;

            if (this.m_gameObjectManagers[this.m_villageType].GetTownHall() != null)
            {
                townHallLevel = this.m_gameObjectManagers[this.m_villageType].GetTownHall().GetUpgradeLevel();
            }

            bool reached = this.m_gameObjectManagers[this.m_villageType].GetGameObjectCountByData(data) >=
                           LogicDataTables.GetTownHallLevel(townHallLevel).GetUnlockedBuildingCount(data);

            if (!reached && canCallListener)
            {
                this.m_gameListener.BuildingCapReached(data);
            }

            return reached;
        }

        public bool IsTrapCapReached(LogicTrapData data, bool canCallListener)
        {
            int townHallLevel = 0;

            if (this.m_gameObjectManagers[this.m_villageType].GetTownHall() != null)
            {
                townHallLevel = this.m_gameObjectManagers[this.m_villageType].GetTownHall().GetUpgradeLevel();
            }

            bool reached = this.m_gameObjectManagers[this.m_villageType].GetGameObjectCountByData(data) >=
                           LogicDataTables.GetTownHallLevel(townHallLevel).GetUnlockedTrapCount(data);

            if (!reached && canCallListener)
            {
                this.m_gameListener.TrapCapReached(data);
            }

            return reached;
        }

        public bool IsDecoCapReached(LogicDecoData data, bool canCallListener)
        {
            int townHallLevel = 0;

            if (this.m_gameObjectManagers[this.m_villageType].GetTownHall() != null)
            {
                townHallLevel = this.m_gameObjectManagers[this.m_villageType].GetTownHall().GetUpgradeLevel();
            }

            bool reached = true;

            if (this.m_homeOwnerAvatar.GetExpLevel() >= data.GetRequiredExpLevel())
            {
                reached = this.m_gameObjectManagers[this.m_villageType].GetGameObjectCountByData(data) >=
                          data.GetMaxCount();
            }

            if (!reached && canCallListener)
            {
                this.m_gameListener.DecoCapReached(data);
            }

            return reached;
        }

        public bool IsValidPlaceForBuilding(int x, int y, int width, int height, LogicGameObject gameObject)
        {
            if (this.m_playArea.IsInside(x, y) && this.m_playArea.IsInside(x + width, y + height))
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (!this.m_tileMap.GetTile(x + i, y + j).IsBuildable(gameObject))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public bool IsValidPlaceForObstacle(int x, int y, int width, int height, bool edge, bool ignoreTallGrass)
        {
            if (x >= 0 && y >= 0)
            {
                if (width + x <= LogicLevel.TILEMAP_SIZE_X && height + y <= LogicLevel.TILEMAP_SIZE_Y)
                {
                    if (edge)
                    {
                        x -= 1;
                        y -= 1;
                        width += 2;
                        height += 2;
                    }

                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            LogicTile tile = this.m_tileMap.GetTile(x + i, y + j);

                            if (tile != null)
                            {
                                if (!ignoreTallGrass)
                                {
                                    if (tile.GetTallGrass() != null)
                                    {
                                        return false;
                                    }
                                }

                                if (!tile.IsBuildable(null))
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public bool IsValidPlaceForBuildingWithIgnoreList(int x, int y, int width, int height, LogicGameObject[] gameObjects, int count)
        {
            if (this.m_playArea.IsInside(x, y))
            {
                if (this.m_playArea.IsInside(x + width, y + height))
                {
                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            if (!this.m_tileMap.GetTile(x + i, y + j).IsBuildableWithIgnoreList(gameObjects, count))
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public bool IsAttackerHeroPlaced(LogicHeroData data)
        {
            if (this.m_placedHeroData != null)
            {
                for (int i = 0; i < this.m_placedHeroData.Size(); i++)
                {
                    if (this.m_placedHeroData[i] == data)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void SetAttackerHeroPlaced(LogicHeroData data, LogicCharacter hero)
        {
            if (this.m_placedHeroData == null)
            {
                this.m_placedHeroData = new LogicArrayList<LogicHeroData>();
                this.m_placedHero = new LogicArrayList<LogicCharacter>();
            }

            int index = this.m_placedHeroData.IndexOf(data);

            if (index == -1)
            {
                this.m_placedHero.Add(hero);
                this.m_placedHeroData.Add(data);
            }
            else
            {
                Debugger.Warning("setAttackerHeroPlaced called twice for same hero");
            }
        }

        public int GetTotalAttackerHeroPlaced()
        {
            if (this.m_placedHeroData != null)
            {
                return this.m_placedHeroData.Size();
            }

            return 0;
        }

        public bool IsUnitsTrainedVillage2()
        {
            LogicArrayList<LogicComponent> components = this.m_gameObjectManagers[1].GetComponentManager().GetComponents(LogicComponentType.VILLAGE2_UNIT);

            for (int i = 0; i < components.Size(); i++)
            {
                LogicVillage2UnitComponent component = (LogicVillage2UnitComponent) components[i];

                if (component.IsEnabled())
                {
                    if (component.GetUnitData() == null || component.GetUnitCount() == 0 || component.GetUnitCount() < component.GetMaxUnitsInCamp(component.GetUnitData()))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool GetAreaShield(int midX, int midY, LogicVector2 output)
        {
            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjectManagers[this.m_villageType].GetGameObjects(LogicGameObjectType.SPELL);

            int speed = 0x7FFFFFFF;
            int damage = 100;

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                LogicSpell spell = (LogicSpell) gameObjects[i];

                if (spell.IsDeployed() && !spell.GetHitsCompleted())
                {
                    LogicSpellData spellData = spell.GetSpellData();

                    if (spellData.GetShieldProjectileSpeed(spell.GetUpgradeLevel()) != 0)
                    {
                        int radius = spellData.GetRadius(spellData.GetRadius(spell.GetUpgradeLevel()));
                        int distanceX = spell.GetMidX() - midX;
                        int distanceY = spell.GetMidY() - midY;

                        if (LogicMath.Abs(distanceX) <= radius && LogicMath.Abs(distanceY) <= radius)
                        {
                            if (distanceX * distanceX + distanceY * distanceY < (uint) (radius * radius))
                            {
                                speed = LogicMath.Min(speed, spellData.GetShieldProjectileSpeed(spell.GetUpgradeLevel()));
                                damage = 100 - spellData.GetShieldProjectileDamageMod(spell.GetUpgradeLevel());
                            }
                        }
                    }
                }
            }

            if (this.m_placedHero != null)
            {
                for (int i = 0; i < this.m_placedHero.Size(); i++)
                {
                    LogicCharacter hero = this.m_placedHero[i];

                    if (hero.GetAbilityTime() > 0 && hero.IsAlive())
                    {
                        LogicHeroData heroData = this.m_placedHeroData[i];

                        int upgLevel = hero.GetUpgradeLevel();
                        int abilityShieldProjectileSpeed = heroData.GetAbilityShieldProjectileSpeed(upgLevel);

                        if (abilityShieldProjectileSpeed != 0)
                        {
                            int radius = heroData.GetAbilityRadius();
                            int distanceX = hero.GetMidX() - midX;
                            int distanceY = hero.GetMidY() - midY;

                            if (LogicMath.Abs(distanceX) <= radius && LogicMath.Abs(distanceY) <= radius)
                            {
                                if (abilityShieldProjectileSpeed < speed && distanceX * distanceX + distanceY * distanceY < (uint) (radius * radius))
                                {
                                    speed = abilityShieldProjectileSpeed;
                                    damage = 100 - heroData.GetAbilityShieldProjectileDamageMod(upgLevel);
                                }
                            }
                        }
                    }
                }
            }

            output.m_x = speed;
            output.m_y = damage;

            return speed != 0x7FFFFFFF;
        }

        public void AreaPoison(int gameObjectId, int x, int y, int radius, int damage, LogicData preferredTarget, int preferredTargetDamagePecent, LogicEffectData hitEffect,
                               int team, LogicVector2 unk2,
                               int targetType, int damageType, int unk3, bool healing, int heroDamageMultiplier, bool increaseSlowly)
        {
            int villageType = this.m_loadingVillageType;

            if (villageType < 0)
            {
                villageType = this.m_villageType;
            }

            LogicArrayList<LogicComponent> components = this.GetComponentManagerAt(villageType).GetComponents(LogicComponentType.HITPOINT);
            LogicVector2 pushBackPosition = new LogicVector2();

            for (int i = 0; i < components.Size(); i++)
            {
                LogicHitpointComponent hitpointComponent = (LogicHitpointComponent) components[i];
                LogicGameObject parent = hitpointComponent.GetParent();

                if (!parent.IsHidden() && hitpointComponent.GetHitpoints() > 0)
                {
                    if (hitpointComponent.GetTeam() == team)
                    {
                        if (damage > 0 || damage < 0 && parent.IsPreventsHealing())
                        {
                            continue;
                        }
                    }
                    else if (damage < 0)
                    {
                        continue;
                    }

                    if (damageType == 2 && parent.GetGameObjectType() != LogicGameObjectType.CHARACTER)
                    {
                        continue;
                    }

                    int parentX;
                    int parentY;

                    LogicMovementComponent movementComponent = parent.GetMovementComponent();

                    if (movementComponent != null || parent.IsFlying())
                    {
                        if (parent.IsFlying())
                        {
                            if (targetType == 1)
                            {
                                continue;
                            }
                        }
                        else if (targetType == 0)
                        {
                            continue;
                        }

                        parentX = parent.GetMidX();
                        parentY = parent.GetMidY();
                    }
                    else
                    {
                        int posX = parent.GetX();
                        int posY = parent.GetY();

                        parentX = LogicMath.Clamp(x, posX, posX + (parent.GetWidthInTiles() << 9));
                        parentY = LogicMath.Clamp(y, posY, posY + (parent.GetHeightInTiles() << 9));
                    }

                    int distanceX = x - parentX;
                    int distanceY = y - parentY;

                    if (distanceX * distanceX + distanceY * distanceY < (uint) (radius * radius))
                    {
                        if (damageType == 1 && parent.GetGameObjectType() == LogicGameObjectType.BUILDING)
                        {
                            LogicBuilding building = (LogicBuilding) parent;

                            if (building.GetResourceStorageComponentComponent() != null &&
                                !building.GetBuildingData().IsTownHall() &&
                                !building.GetBuildingData().IsTownHallVillage2())
                            {
                                parent.SetDamageTime(10);
                                continue;
                            }
                        }

                        int totalDamage = LogicCombatComponent.IsPreferredTarget(preferredTarget, parent) ? damage * preferredTargetDamagePecent / 100 : damage;

                        if (parent.IsHero())
                        {
                            totalDamage = (totalDamage < 0 ? LogicDataTables.GetGlobals().GetHeroHealMultiplier() : heroDamageMultiplier) * damage / 100;
                        }

                        hitpointComponent.SetPoisonDamage(totalDamage, increaseSlowly);

                        if (healing)
                        {
                            // Listener.
                        }

                        if ((distanceX | distanceX) == 0)
                        {
                            distanceX = 1;
                        }

                        pushBackPosition.m_x = -distanceX;
                        pushBackPosition.m_y = -distanceY;

                        pushBackPosition.Normalize(512);

                        if (unk3 > 0 && movementComponent != null)
                        {
                            movementComponent.GetMovementSystem().PushBack(pushBackPosition, damage, unk3, 0, false, true);
                        }
                    }
                }
            }
        }

        public void AreaDamage(int gameObjectId, int x, int y, int radius, int damage, LogicData preferredTarget, int preferredTargetDamagePecent, LogicEffectData hitEffect,
                               int team, LogicVector2 unk2,
                               int targetType, int damageType, int unk3, bool gravity, bool healing, int heroDamageMultiplier, int maxUnitsHit, LogicGameObject gameObject,
                               int damageTHPercent, int pauseCombatComponentsMs)
        {
            int villageType = this.m_loadingVillageType;

            if (villageType < 0)
            {
                villageType = this.m_villageType;
            }

            LogicArrayList<LogicComponent> components = this.GetComponentManagerAt(villageType).GetComponents(LogicComponentType.HITPOINT);
            LogicVector2 pushBackPosition = new LogicVector2();

            int freezeTimeMS = pauseCombatComponentsMs >> 6;
            int maxHits = maxUnitsHit > 0 ? maxUnitsHit : 0x7FFFFFFF;

            for (int i = 0, j = 0; i < components.Size() && j < maxHits; i++)
            {
                LogicHitpointComponent hitpointComponent = (LogicHitpointComponent) components[i];
                LogicGameObject parent = hitpointComponent.GetParent();

                if (!parent.IsHidden() && hitpointComponent.GetHitpoints() > 0)
                {
                    if (hitpointComponent.GetTeam() == team)
                    {
                        if (damage > 0 || damage < 0 && parent.IsPreventsHealing())
                        {
                            continue;
                        }
                    }
                    else if (damage < 0)
                    {
                        continue;
                    }

                    if (damageType == 2 && parent.GetGameObjectType() != LogicGameObjectType.CHARACTER)
                    {
                        continue;
                    }

                    int parentX;
                    int parentY;

                    LogicMovementComponent movementComponent = parent.GetMovementComponent();

                    if (movementComponent != null || parent.IsFlying())
                    {
                        if (parent.IsFlying())
                        {
                            if (targetType == 1)
                            {
                                continue;
                            }
                        }
                        else if (targetType == 0)
                        {
                            continue;
                        }

                        parentX = parent.GetMidX();
                        parentY = parent.GetMidY();
                    }
                    else
                    {
                        int posX = parent.GetX();
                        int posY = parent.GetY();

                        parentX = LogicMath.Clamp(x, posX, posX + (parent.GetWidthInTiles() << 9));
                        parentY = LogicMath.Clamp(y, posY, posY + (parent.GetHeightInTiles() << 9));
                    }

                    int distanceX = x - parentX;
                    int distanceY = y - parentY;

                    if (LogicMath.Abs(distanceX) <= radius &&
                        LogicMath.Abs(distanceY) <= radius &&
                        distanceX * distanceX + distanceY * distanceY < (uint) (radius * radius))
                    {
                        if (damageType == 1 && parent.GetGameObjectType() == LogicGameObjectType.BUILDING)
                        {
                            LogicBuilding building = (LogicBuilding) parent;

                            if (building.GetResourceStorageComponentComponent() != null &&
                                !building.GetBuildingData().IsTownHall() &&
                                !building.GetBuildingData().IsTownHallVillage2())
                            {
                                parent.SetDamageTime(10);
                                continue;
                            }
                        }

                        int totalDamage = LogicCombatComponent.IsPreferredTarget(preferredTarget, parent) ? damage * preferredTargetDamagePecent / 100 : damage;

                        if (parent.GetGameObjectType() == LogicGameObjectType.BUILDING && parent.GetData() == LogicDataTables.GetTownHallData())
                            totalDamage = damageTHPercent * totalDamage / 100;
                        if (parent.IsHero())
                            totalDamage = (totalDamage < 0 ? LogicDataTables.GetGlobals().GetHeroHealMultiplier() : heroDamageMultiplier) * damage / 100;

                        hitpointComponent.CauseDamage(totalDamage, gameObjectId, gameObject);

                        if (pauseCombatComponentsMs > 0)
                        {
                            if (parent.GetCombatComponent() != null)
                                parent.Freeze(freezeTimeMS, 0);
                            if (parent.GetMovementComponent() != null)
                                parent.Freeze(freezeTimeMS, 0);
                        }

                        if (healing)
                        {
                            // Listener.
                        }

                        if ((distanceX | distanceY) == 0)
                        {
                            distanceX = 1;

                            if (unk2 != null && (unk2.m_x | unk2.m_y) != 0)
                            {
                                distanceX = -unk2.m_x;
                                distanceY = -unk2.m_y;
                            }
                        }

                        ++j;

                        pushBackPosition.m_x = -distanceX;
                        pushBackPosition.m_y = -distanceY;

                        pushBackPosition.Normalize(512);

                        if (unk3 > 0 && movementComponent != null && !this.m_invulnerabilityEnabled)
                        {
                            movementComponent.GetMovementSystem().PushBack(pushBackPosition, damage, unk3, 0, false, gravity);

                            if (parent.GetGameObjectType() == LogicGameObjectType.BUILDING)
                            {
                                LogicBuilding building = (LogicBuilding) parent;

                                if (building.GetCombatComponent() != null && building.GetCombatComponent().GetAttackerItemData().IsSelfAsAoeCenter())
                                {
                                    // Listener.
                                }
                            }
                        }
                    }
                }
            }
        }

        public void AreaFreeze(int x, int y, int radius, int time, int team)
        {
            for (int i = 0; i < LogicGameObject.GAMEOBJECT_TYPE_COUNT; i++)
            {
                LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjectManagers[this.m_villageType].GetGameObjects((LogicGameObjectType) i);

                for (int j = 0; j < gameObjects.Size(); j++)
                {
                    LogicGameObject gameObject = gameObjects[j];
                    LogicCombatComponent combatComponent = gameObject.GetCombatComponent();

                    if (combatComponent == null || combatComponent.GetUndergroundTime() <= 0)
                    {
                        LogicHitpointComponent hitpointComponent = gameObject.GetHitpointComponent();

                        if (hitpointComponent == null || hitpointComponent.GetTeam() != team)
                        {
                            int distanceX = x - gameObject.GetMidX();
                            int distanceY = y - gameObject.GetMidY();

                            if (LogicMath.Abs(distanceX) <= radius &&
                                LogicMath.Abs(distanceY) <= radius &&
                                distanceX * distanceX + distanceY * distanceY < (uint) (radius * radius))
                            {
                                int freezeTime = LogicMath.Max(0, time / 4 - (int) (((distanceX * distanceX + distanceY * distanceY) / 250000u) & 0xFFFE));
                                int freezeDelay = (int) ((distanceX * distanceX + distanceY * distanceY) / 2000000u);

                                gameObject.Freeze(freezeTime, freezeDelay);
                            }
                        }
                    }
                }
            }
        }

        public void AreaShield(int x, int y, int radius, int time, int team)
        {
            LogicArrayList<LogicComponent> components = this.m_gameObjectManagers[this.m_loadingVillageType < 0 ? this.m_villageType : this.m_loadingVillageType]
                                                            .GetComponentManager()
                                                            .GetComponents(LogicComponentType.MOVEMENT);

            for (int i = 0; i < components.Size(); i++)
            {
                LogicMovementComponent movementComponent = (LogicMovementComponent) components[i];
                LogicGameObject parent = movementComponent.GetParent();
                LogicHitpointComponent hitpointComponent = parent.GetHitpointComponent();
                LogicCombatComponent combatComponent = parent.GetCombatComponent();

                if (hitpointComponent != null && combatComponent != null && hitpointComponent.GetTeam() == team)
                {
                    int distanceX = x - parent.GetMidX();
                    int distanceY = y - parent.GetMidY();

                    if (LogicMath.Abs(distanceX) <= radius &&
                        LogicMath.Abs(distanceY) <= radius &&
                        distanceX * distanceX + distanceY * distanceY < (uint) (radius * radius))
                    {
                        hitpointComponent.SetInvulnerabilityTime(time);
                        parent.SetPreventsHealingTime(0);

                        if (parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                        {
                            LogicCharacter character = (LogicCharacter) parent;
                            LogicArrayList<LogicCharacter> childrens = character.GetChildTroops();

                            if (childrens != null)
                            {
                                for (int j = 0; j < childrens.Size(); j++)
                                {
                                    LogicGameObject children = childrens[j];

                                    children.GetHitpointComponent().SetInvulnerabilityTime(time);
                                    children.SetPreventsHealingTime(0);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void AreaShrink(int x, int y, int radius, int speedBoostRatio, int hpRatio, int time, int team)
        {
            for (int i = 0; i < LogicGameObject.GAMEOBJECT_TYPE_COUNT; i++)
            {
                LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjectManagers[this.m_villageType].GetGameObjects((LogicGameObjectType) i);

                for (int j = 0; j < gameObjects.Size(); j++)
                {
                    LogicGameObject gameObject = gameObjects[j];
                    LogicCombatComponent combatComponent = gameObject.GetCombatComponent();

                    if (combatComponent == null || combatComponent.GetUndergroundTime() <= 0)
                    {
                        LogicHitpointComponent hitpointComponent = gameObject.GetHitpointComponent();

                        if (hitpointComponent == null || hitpointComponent.GetTeam() != team)
                        {
                            int distanceX = x - gameObject.GetMidX();
                            int distanceY = y - gameObject.GetMidY();

                            if (LogicMath.Abs(distanceX) <= radius &&
                                LogicMath.Abs(distanceY) <= radius &&
                                distanceX * distanceX + distanceY * distanceY < (uint) (radius * radius))
                            {
                                gameObject.Shrink(time, speedBoostRatio);

                                if (hitpointComponent != null && hitpointComponent.GetOriginalHitpoints() == hitpointComponent.GetMaxHitpoints())
                                {
                                    hitpointComponent.SetShrinkHitpoints(time, hpRatio);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void AreaInvisibility(int x, int y, int radius, int time, int team)
        {
            for (int i = 0; i < LogicGameObject.GAMEOBJECT_TYPE_COUNT; i++)
            {
                LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjectManagers[this.m_villageType].GetGameObjects((LogicGameObjectType) i);

                for (int j = 0; j < gameObjects.Size(); j++)
                {
                    LogicGameObject gameObject = gameObjects[j];
                    LogicHitpointComponent hitpointComponent = gameObject.GetHitpointComponent();
                    LogicCombatComponent combatComponent = gameObject.GetCombatComponent();

                    if (hitpointComponent != null && combatComponent != null && hitpointComponent.GetTeam() == team)
                    {
                        int distanceX = x - gameObject.GetMidX();
                        int distanceY = y - gameObject.GetMidY();

                        if (LogicMath.Abs(distanceX) <= radius &&
                            LogicMath.Abs(distanceY) <= radius &&
                            distanceX * distanceX + distanceY * distanceY < (uint) (radius * radius))
                        {
                            combatComponent.SetUndergroundTime(time / 4);
                        }
                    }
                }
            }
        }

        public void BoostGameObject(LogicGameObject gameObject, int speedBoost, int speedBoost2, int damageBoostPercentage, int attackSpeedBoost, int boostTime,
                                    bool boostLinkedToPoison)
        {
            LogicMovementComponent movementComponent = gameObject.GetMovementComponent();
            LogicHitpointComponent hitpointComponent = gameObject.GetHitpointComponent();
            LogicCombatComponent combatComponent = gameObject.GetCombatComponent();

            if (hitpointComponent != null && combatComponent != null)
            {
                int totalDamageBoostPercentage = damageBoostPercentage;

                if (boostLinkedToPoison)
                {
                    int rmMS = hitpointComponent.GetPoisonRemainingMS();

                    if (60 * rmMS / 1000 >= boostTime)
                    {
                        boostTime = 60 * rmMS / 1000;
                    }
                }

                if (gameObject.IsHero())
                {
                    totalDamageBoostPercentage = totalDamageBoostPercentage * LogicDataTables.GetGlobals().GetHeroRageMultiplier() / 100;
                }

                combatComponent.Boost(totalDamageBoostPercentage, attackSpeedBoost, boostTime / 4);

                if (gameObject.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                {
                    LogicCharacter character = (LogicCharacter) gameObject;
                    LogicArrayList<LogicCharacter> childrens = character.GetChildTroops();

                    if (childrens != null)
                    {
                        for (int i = 0; i < childrens.Size(); i++)
                        {
                            childrens[i].GetCombatComponent().Boost(damageBoostPercentage, attackSpeedBoost, boostTime / 4);
                        }
                    }
                }

                if (gameObject.GetData().GetDataType() == LogicDataType.CHARACTER && ((LogicCharacter) gameObject).GetAttackerItemData().GetPreferredTargetData() != null)
                {
                    if (movementComponent != null)
                    {
                        movementComponent.GetMovementSystem().Boost(speedBoost2, boostTime);
                    }
                }
                else
                {
                    if (gameObject.IsHero())
                    {
                        speedBoost = (int) (speedBoost * LogicDataTables.GetGlobals().GetHeroRageSpeedMultiplier() / 100L);
                    }

                    if (movementComponent != null)
                    {
                        movementComponent.GetMovementSystem().Boost(speedBoost, boostTime);
                    }
                }
            }
        }

        public void AreaBoost(int x, int y, int radius, int speedBoost, int speedBoost2, int damageBoostPercentage, int attackSpeedBoost, int damageTime, int team,
                              bool boostLinkedToPoison)
        {
            LogicArrayList<LogicComponent> components = this.GetComponentManagerAt(this.m_loadingVillageType < 0 ? this.m_villageType : this.m_loadingVillageType)
                                                            .GetComponents(LogicComponentType.MOVEMENT);

            for (int i = 0; i < components.Size(); i++)
            {
                LogicMovementComponent movementComponent = (LogicMovementComponent) components[i];
                LogicGameObject parent = movementComponent.GetParent();
                LogicHitpointComponent hitpointComponent = parent.GetHitpointComponent();
                LogicCombatComponent combatComponent = parent.GetCombatComponent();

                if (hitpointComponent != null && combatComponent != null && hitpointComponent.GetTeam() == team && hitpointComponent.GetHitpoints() > 0)
                {
                    int distanceX = x - parent.GetMidX();
                    int distanceY = y - parent.GetMidY();

                    if (LogicMath.Abs(distanceX) <= radius &&
                        LogicMath.Abs(distanceY) <= radius &&
                        distanceX * distanceX + distanceY * distanceY < (uint) (radius * radius))
                    {
                        this.BoostGameObject(parent, speedBoost, speedBoost2, damageBoostPercentage, attackSpeedBoost, damageTime, boostLinkedToPoison);
                    }
                }
            }
        }

        public void AreaBoost(int x, int y, int radius, int damageBoostPercentage, int attackSpeedBoost, int damageTime)
        {
            LogicArrayList<LogicComponent> components = this.GetComponentManagerAt(this.m_loadingVillageType < 0 ? this.m_villageType : this.m_loadingVillageType)
                                                            .GetComponents(LogicComponentType.COMBAT);

            for (int i = 0; i < components.Size(); i++)
            {
                LogicCombatComponent combatComponent = (LogicCombatComponent) components[i];
                LogicGameObject parent = combatComponent.GetParent();

                if (parent.GetGameObjectType() == LogicGameObjectType.BUILDING)
                {
                    LogicHitpointComponent hitpointComponent = parent.GetHitpointComponent();

                    if (hitpointComponent != null && hitpointComponent.GetHitpoints() > 0)
                    {
                        int distanceX = x - parent.GetMidX();
                        int distanceY = y - parent.GetMidY();

                        if (LogicMath.Abs(distanceX) <= radius &&
                            LogicMath.Abs(distanceY) <= radius &&
                            distanceX * distanceX + distanceY * distanceY < (uint) (radius * radius))
                        {
                            combatComponent.Boost(damageBoostPercentage, attackSpeedBoost, damageTime);
                        }
                    }
                }
            }
        }

        public void AreaAbilityBoost(LogicCharacter hero, int time)
        {
            if (hero.IsHero())
            {
                LogicArrayList<LogicComponent> components = this.GetComponentManagerAt(this.m_loadingVillageType < 0 ? this.m_villageType : this.m_loadingVillageType)
                                                                .GetComponents(LogicComponentType.MOVEMENT);
                LogicHeroData data = (LogicHeroData) hero.GetData();

                int upgLevel = hero.GetUpgradeLevel();
                int x = hero.GetX();
                int y = hero.GetY();

                LogicCharacterData abilityAffectsCharacter = data.GetAbilityAffectsCharacter();
                LogicCharacter abilityAffectsHero = data.GetAbilityAffectsHero() ? hero : null;
                LogicCharacterData summonTroop = data.GetAbilitySummonTroop();

                int speedBoost = data.GetAbilitySpeedBoost(upgLevel);
                int speedBoost2 = data.GetAbilitySpeedBoost2(upgLevel);
                int damageBoostPercent = data.GetAbilityDamageBoostPercent(upgLevel);
                int damageBoost = data.GetAbilityDamageBoost(upgLevel);
                int summonTroopCount = data.GetAbilitySummonTroopCount(upgLevel);

                int radius = data.GetAbilityRadius();
                int speedTime = time * 4;

                for (int i = 0; i < components.Size(); i++)
                {
                    LogicMovementComponent movementComponent = (LogicMovementComponent) components[i];
                    LogicGameObject parent = movementComponent.GetParent();
                    LogicHitpointComponent hitpointComponent = parent.GetHitpointComponent();
                    LogicCombatComponent combatComponent = parent.GetCombatComponent();

                    if (hitpointComponent != null && combatComponent != null && hitpointComponent.GetTeam() == 0 && hitpointComponent.GetHitpoints() > 0 &&
                        parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                    {
                        LogicCharacter character = (LogicCharacter) parent;
                        LogicCharacterData characterData = character.GetCharacterData();

                        if (abilityAffectsCharacter == null || abilityAffectsHero == null || abilityAffectsCharacter == characterData || abilityAffectsHero == parent)
                        {
                            if (abilityAffectsHero != null || !character.IsHero())
                            {
                                if (abilityAffectsCharacter != null || characterData.GetDataType() == LogicDataType.HERO)
                                {
                                    if (characterData == data || !character.IsHero())
                                    {
                                        int distanceX = x - character.GetMidX();
                                        int distanceY = y - character.GetMidY();

                                        if (LogicMath.Abs(distanceX) <= radius &&
                                            LogicMath.Abs(distanceY) <= radius &&
                                            distanceX * distanceX + distanceY * distanceY < (uint) (radius * radius))
                                        {
                                            if (speedBoost > 0 && speedBoost2 > 0 && damageBoostPercent > 0)
                                            {
                                                if (character.IsHero())
                                                {
                                                    if (damageBoost != 0)
                                                    {
                                                        combatComponent.Boost(damageBoost, 0, time);
                                                    }
                                                }
                                                else
                                                {
                                                    combatComponent.Boost(damageBoostPercent, 0, time);
                                                }

                                                if (characterData.GetDataType() == LogicDataType.CHARACTER &&
                                                    character.GetAttackerItemData().GetPreferredTargetData() != null)
                                                {
                                                    movementComponent.GetMovementSystem().Boost(speedBoost2, speedTime);
                                                }
                                                else
                                                {
                                                    movementComponent.GetMovementSystem().Boost(speedBoost, speedTime);
                                                }
                                            }
                                            else if (data.GetAbilityStealth())
                                            {
                                                if (character.IsHero() && damageBoost != 0)
                                                {
                                                    combatComponent.Boost(damageBoost, 0, time);
                                                }

                                                character.SetStealthTime(time);
                                            }

                                            if (summonTroopCount > 0)
                                            {
                                                if (character.IsHero())
                                                {
                                                    character.SpawnEvent(summonTroop, summonTroopCount, this.m_visitorAvatar.GetUnitUpgradeLevel(summonTroop));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void AreaBoostAlone(LogicCharacter character, int time)
        {
            LogicArrayList<LogicComponent> components = this.GetComponentManagerAt(this.m_loadingVillageType < 0 ? this.m_villageType : this.m_loadingVillageType)
                                                            .GetComponents(LogicComponentType.MOVEMENT);
            LogicCharacterData data = character.GetCharacterData();

            int x = character.GetX();
            int y = character.GetY();
            int boostRadius = data.GetBoostRadius();
            int boostDamagePerfect = data.GetBoostDamagePerfect();
            int boostAttackSpeed = data.GetBoostAttackSpeed();

            if (data.GetSpecialAbilityType() == LogicCharacterData.SPECIAL_ABILITY_TYPE_RAGE_ALONE && character.GetSpecialAbilityAvailable())
            {
                boostDamagePerfect = data.GetSpecialAbilityAttribute(character.GetUpgradeLevel());
                boostAttackSpeed = data.GetSpecialAbilityAttribute2(character.GetUpgradeLevel());
            }

            int team = character.GetHitpointComponent() != null ? character.GetHitpointComponent().GetTeam() : -1;
            int boostRadiusSquared = boostRadius * boostRadius;
            bool flying = character.IsFlying();
            bool isAlone = true;

            for (int i = 0; i < components.Size(); i++)
            {
                LogicMovementComponent movementComponent = (LogicMovementComponent) components[i];
                LogicGameObject parent = movementComponent.GetParent();
                LogicHitpointComponent hitpointComponent = parent.GetHitpointComponent();
                LogicCombatComponent combatComponent = parent.GetCombatComponent();

                if (hitpointComponent != null && combatComponent != null && hitpointComponent.GetTeam() == team && hitpointComponent.GetHitpoints() > 0 &&
                    parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                {
                    if (parent != character && !(flying ^ parent.IsFlying()))
                    {
                        int distanceX = x - parent.GetMidX();
                        int distanceY = y - parent.GetMidY();

                        if (distanceX * distanceX + distanceY * distanceY < boostRadiusSquared)
                        {
                            isAlone = false;
                        }
                    }
                }
            }

            if (isAlone)
            {
                LogicCombatComponent combatComponent = character.GetCombatComponent();

                if (combatComponent != null)
                {
                    combatComponent.Boost(boostDamagePerfect, boostAttackSpeed, time);
                }
            }
        }

        public void AreaJump(int x, int y, int radius, int time, int housingSpaceLimit, int team)
        {
            LogicArrayList<LogicComponent> components = this.GetComponentManagerAt(this.m_loadingVillageType < 0 ? this.m_villageType : this.m_loadingVillageType)
                                                            .GetComponents(LogicComponentType.MOVEMENT);

            for (int i = 0; i < components.Size(); i++)
            {
                LogicMovementComponent movementComponent = (LogicMovementComponent) components[i];
                LogicGameObject parent = movementComponent.GetParent();
                LogicHitpointComponent hitpointComponent = parent.GetHitpointComponent();
                LogicCombatComponent combatComponent = parent.GetCombatComponent();

                if (hitpointComponent != null && combatComponent != null && hitpointComponent.GetTeam() == team && hitpointComponent.GetHitpoints() > 0)
                {
                    if (parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                    {
                        LogicCharacter character = (LogicCharacter) parent;
                        LogicCharacterData data = character.GetCharacterData();

                        if (data.GetHousingSpace() > housingSpaceLimit || data.IsJumper())
                        {
                            continue;
                        }
                    }

                    int distanceX = x - parent.GetMidX();
                    int distanceY = y - parent.GetMidY();

                    if (LogicMath.Abs(distanceX) <= radius &&
                        LogicMath.Abs(distanceY) <= radius &&
                        distanceX * distanceX + distanceY * distanceY < (uint) (radius * radius))
                    {
                        movementComponent.EnableJump(time);
                    }
                }
            }
        }

        public void AreaPushBack(int x, int y, int radius, int time, int team, int targetType, int pushBackX, int pushBackY, int distance, int housingSpaceLimit)
        {
            LogicArrayList<LogicComponent> components = this.GetComponentManagerAt(this.m_loadingVillageType < 0 ? this.m_villageType : this.m_loadingVillageType)
                                                            .GetComponents(LogicComponentType.HITPOINT);
            LogicVector2 pushBackPosition = new LogicVector2();

            for (int i = 0; i < components.Size(); i++)
            {
                LogicHitpointComponent hitpointComponent = (LogicHitpointComponent) components[i];
                LogicGameObject parent = hitpointComponent.GetParent();

                if (!parent.IsHidden() && hitpointComponent.GetHitpoints() != 0 && hitpointComponent.GetTeam() != team)
                {
                    LogicCombatItemData data = (LogicCombatItemData) parent.GetData();

                    if (housingSpaceLimit >= data.GetHousingSpace())
                    {
                        LogicMovementComponent movementComponent = parent.GetMovementComponent();

                        int posX;
                        int posY;

                        if (movementComponent != null || parent.IsFlying())
                        {
                            if (parent.IsFlying())
                            {
                                if (targetType == 1)
                                {
                                    continue;
                                }
                            }
                            else if (targetType == 0)
                            {
                                continue;
                            }

                            posX = parent.GetMidX();
                            posY = parent.GetMidY();
                        }
                        else
                        {
                            posX = LogicMath.Clamp(x, parent.GetX(), parent.GetX() + (parent.GetWidthInTiles() << 9));
                            posY = LogicMath.Clamp(y, parent.GetY(), parent.GetY() + (parent.GetHeightInTiles() << 9));
                        }

                        int distanceX = x - posX;
                        int distanceY = y - posY;

                        if (LogicMath.Abs(distanceX) <= radius &&
                            LogicMath.Abs(distanceY) <= radius &&
                            distanceX * distanceX + distanceY * distanceY < (uint) (radius * radius))
                        {
                            if (time > 0 && movementComponent != null && !this.m_invulnerabilityEnabled)
                            {
                                pushBackPosition.m_x = (int) ((2 * distance * pushBackX) & 0xFFFFFE00) / 100;
                                pushBackPosition.m_y = (int) ((2 * distance * pushBackY) & 0xFFFFFE00) / 100;

                                movementComponent.GetMovementSystem().PushTrap(pushBackPosition, 1000, 0, true, true);
                                housingSpaceLimit -= data.GetHousingSpace();
                            }
                        }
                    }
                }
            }
        }

        public bool HasFreeWorkers(LogicCommand command, int villageType)
        {
            if (villageType == -1)
            {
                villageType = this.m_villageType;
            }

            bool hasFreeWorker = this.m_workerManagers[villageType].GetFreeWorkers() > 0;

            if (!hasFreeWorker)
            {
                this.m_gameListener.NotEnoughWorkers(command, villageType);
            }

            return hasFreeWorker;
        }

        public void LoadingFinished()
        {
            for (int i = 0; i < LogicLevel.VILLAGE_COUNT; i++)
            {
                this.m_gameObjectManagers[i].GetComponentManager().DivideAvatarResourcesToStorages();
            }

            this.RefreshResourceCaps();

            for (int i = 0; i < LogicLevel.VILLAGE_COUNT; i++)
            {
                this.m_gameObjectManagers[i].GetComponentManager().CalculateLoot(true);
            }

            if (this.m_battleLog != null)
            {
                this.m_battleLog.CalculateAvailableResources(this, this.m_matchType);
                this.SetOwnerInformationToBattleLog();
            }

            if (this.m_gameMode.GetState() == 2)
            {
                if (this.m_matchType == 1)
                {
                    this.m_visitorAvatar.CommodityCountChangeHelper(0, LogicDataTables.GetGlobals().GetAttackResource(),
                                                                    -LogicDataTables.GetTownHallLevel(this.m_visitorAvatar.GetTownHallLevel()).GetAttackCost());
                }
                else if (this.m_matchType == 8)
                {
                    this.m_visitorAvatar.CommodityCountChangeHelper(0, LogicDataTables.GetGlobals().GetAttackResource(),
                                                                    -LogicDataTables.GetTownHallLevel(this.m_visitorAvatar.GetTownHallLevel()).GetAttackCostVillage2());
                }
            }

            for (int i = 0; i < LogicLevel.VILLAGE_COUNT; i++)
            {
                this.m_gameObjectManagers[i].LoadingFinished();
            }

            this.m_missionManager.LoadingFinished();
            this.LoadShopNewItems();

            if (this.m_levelJSON != null)
            {
                this.m_lastLeagueRank = 0;
                this.m_lastAllianceLevel = 1;

                LogicJSONNumber accountFlagObject = this.m_levelJSON.GetJSONNumber("account_flags");

                if (accountFlagObject != null)
                {
                    int value = accountFlagObject.GetIntValue();

                    this.m_helpOpened = (value & 1) != 0;
                    this.m_editModeShown = ((value >> 1) & 1) != 0;
                    this.m_attackShieldCostOpened = ((value >> 3) & 1) != 0;
                }

                LogicJSONNumber lastLeagueRankObject = this.m_levelJSON.GetJSONNumber("last_league_rank");

                if (lastLeagueRankObject != null)
                {
                    this.m_lastLeagueRank = lastLeagueRankObject.GetIntValue();
                }

                LogicJSONNumber lastAllianceLevelObject = this.m_levelJSON.GetJSONNumber("last_alliance_level");

                if (lastAllianceLevelObject != null)
                {
                    this.m_lastAllianceLevel = lastAllianceLevelObject.GetIntValue();
                }

                LogicJSONNumber lastLeagueShuffleObject = this.m_levelJSON.GetJSONNumber("last_league_shuffle");

                if (lastLeagueShuffleObject != null)
                {
                    this.m_lastLeagueShuffle = lastLeagueShuffleObject.GetIntValue() != 0;
                }

                LogicJSONNumber lastSeasonSeenNumber = this.m_levelJSON.GetJSONNumber("last_season_seen");

                if (lastSeasonSeenNumber != null)
                {
                    this.m_lastSeasonSeen = lastSeasonSeenNumber.GetIntValue();
                }

                LogicJSONNumber lastNewsSeenNumber = this.m_levelJSON.GetJSONNumber("last_news_seen");

                if (lastNewsSeenNumber != null)
                {
                    this.m_lastSeenNews = lastNewsSeenNumber.GetIntValue();
                }

                LogicJSONBoolean editModeShown = this.m_levelJSON.GetJSONBoolean("edit_mode_shown");

                if (editModeShown != null)
                {
                    this.m_editModeShown = editModeShown.IsTrue();
                }

                LogicJSONString troopRequestObject = this.m_levelJSON.GetJSONString("troop_req_msg");

                if (troopRequestObject != null)
                {
                    this.m_troopRequestMessage = troopRequestObject.GetStringValue();
                }

                LogicJSONString warRequestObject = this.m_levelJSON.GetJSONString("war_req_msg");

                if (warRequestObject != null)
                {
                    this.m_warTroopRequestMessage = warRequestObject.GetStringValue();
                }

                LogicJSONNumber warTutorialsSeenNumber = this.m_levelJSON.GetJSONNumber("war_tutorials_seen");

                if (warTutorialsSeenNumber != null)
                {
                    this.m_warTutorialsSeen = warTutorialsSeenNumber.GetIntValue();
                }

                LogicJSONArray armyNameArray = this.m_levelJSON.GetJSONArray("army_names");

                if (armyNameArray != null)
                {
                    int size = LogicMath.Min(armyNameArray.Size(), this.m_armyNames.Size());

                    for (int i = 0; i < size; i++)
                    {
                        this.m_armyNames[i] = armyNameArray.GetJSONString(i).GetStringValue();
                    }
                }

                LogicJSONBoolean helpOpenedBoolean = this.m_levelJSON.GetJSONBoolean("help_opened");

                if (helpOpenedBoolean != null)
                {
                    this.m_helpOpened = helpOpenedBoolean.IsTrue();
                }

                LogicJSONBoolean layoutEditShownEraseBoolean = this.m_levelJSON.GetJSONBoolean(this.GetPersistentBoolVariableName(0));

                if (layoutEditShownEraseBoolean != null)
                {
                    this.m_layoutEditShownErase = layoutEditShownEraseBoolean.IsTrue();
                }
            }

            this.m_achievementManager.RefreshStatus();

            if (LogicDataTables.GetGlobals().ValidateTroopUpgradeLevels())
            {
                for (int i = 0; i < LogicLevel.VILLAGE_COUNT; i++)
                {
                    this.m_gameObjectManagers[i].GetComponentManager().ValidateTroopUpgradeLevels();
                }
            }

            if (this.m_gameMode.GetState() == 2 && this.m_matchType == 4 && LogicDataTables.GetGlobals().RemoveRevengeWhenBattleIsLoaded())
            {
                this.GetPlayerAvatar().GetChangeListener().RevengeUsed(this.m_revengeId);
            }

            this.m_gameMode.GetCalendar().UpdateUseTroopEvent(this.m_homeOwnerAvatar, this);
            this.m_levelJSON = null;
        }

        public void LoadVillageObjects()
        {
            for (int i = 0; i < LogicLevel.VILLAGE_COUNT; i++)
            {
                this.m_gameObjectManagers[i].LoadVillageObjects();
            }
        }

        public void FastForwardTime(int totalSecs)
        {
            for (int i = 0; i < LogicLevel.VILLAGE_COUNT; i++)
            {
                this.m_gameObjectManagers[i].FastForwardTime(totalSecs);
            }

            this.m_offerManager.FastForward(totalSecs);
            this.m_cooldownManager.FastForwardTime(totalSecs);

            for (int i = 0; i < this.m_layoutCooldown.Size(); i++)
            {
                if (this.m_layoutCooldown[i] > 0)
                {
                    this.m_layoutCooldown[i] = LogicMath.Max(0, this.m_layoutCooldown[i] - 15 * totalSecs);
                }
            }
        }

        public void SubTick()
        {
            int clockTowerBoostTime = 0;

            if (this.m_gameObjectManagers[1].GetClockTower() != null)
            {
                LogicBuilding clockTower = this.m_gameObjectManagers[1].GetClockTower();

                if (!clockTower.IsBoostPaused())
                {
                    if (!clockTower.IsConstructing())
                    {
                        clockTowerBoostTime = clockTower.GetRemainingBoostTime();
                    }
                }
            }

            this.m_remainingClockTowerBoostTime = clockTowerBoostTime;

            for (int i = 0; i < LogicLevel.VILLAGE_COUNT; i++)
            {
                this.m_gameObjectManagers[i].SubTick();
            }
        }

        public void Tick()
        {
            int state = this.GetState();

            if (state == 2 && !this.m_battleStarted && this.m_battleLog.GetBattleStarted())
            {
                this.BattleStarted();
            }

            if (state <= 1)
            {
                for (int i = 0; i < LogicLevel.VILLAGE_COUNT; i++)
                {
                    this.m_gameObjectManagers[i].Tick();
                }
            }
            else
            {
                this.m_gameObjectManagers[this.m_villageType].Tick();
            }

            this.m_missionManager.Tick();
            this.m_achievementManager.Tick();
            this.m_offerManager.Tick();

            if (this.m_npcAttack != null)
            {
                this.m_npcAttack.Tick();
            }

            this.m_cooldownManager.Tick();
            this.UpdateBattleShieldStatus(true);

            for (int i = 0; i < this.m_layoutCooldown.Size(); i++)
            {
                int cooldown = this.m_layoutCooldown[i];

                if (cooldown > 0)
                {
                    this.m_layoutCooldown[i] = cooldown - 1;
                }
            }
        }

        public void UpdateExperienceVersion(int prevVersion)
        {
            if (prevVersion == 0)
            {
                LogicGameObjectManager gameObjectManager = this.m_gameObjectManagers[0];

                for (int i = 0, j = gameObjectManager.GetNumGameObjects(); i < j; i++)
                {
                    LogicGameObject gameObject = gameObjectManager.GetGameObjectByIndex(i);

                    int width = gameObject.GetWidthInTiles();
                    int height = gameObject.GetHeightInTiles();

                    for (int k = 0; k < 8; k++)
                    {
                        for (int l = 0; l < 2; l++)
                        {
                            int x = 0;
                            int y = 0;

                            if (l != 0 && k == this.m_activeLayout)
                            {
                                LogicVector2 pos = gameObject.GetPosition();

                                if ((pos.m_x & pos.m_y) >> 9 == -1)
                                {
                                    continue;
                                }

                                x = pos.m_x >> 9;
                                y = pos.m_y >> 9;
                            }
                            else
                            {
                                if (gameObject.GetLayoutComponent() == null)
                                {
                                    continue;
                                }

                                LogicVector2 pos = gameObject.GetPositionLayout(k, l == 0);

                                x = pos.m_x;
                                y = pos.m_y;

                                if ((x & y) == -1)
                                {
                                    continue;
                                }
                            }

                            int updatedX = x + 3;
                            int updatedY = y + 3;

                            if (gameObject.GetGameObjectType() == LogicGameObjectType.OBSTACLE)
                            {
                                int corrX = 0;
                                int corrY = 0;

                                if (x < 3)
                                {
                                    corrX -= 3;
                                }

                                if (y < 3)
                                {
                                    corrY -= 3;
                                }

                                if (x + width > 44)
                                {
                                    corrX += 3;
                                }

                                if (y + height > 44)
                                {
                                    corrY += 3;
                                }

                                updatedX += corrX;
                                updatedY += corrY;
                            }

                            if (l != 0 && this.m_activeLayout == k)
                            {
                                gameObject.SetPositionXY(updatedX << 9, updatedY << 9);
                            }
                            else
                            {
                                gameObject.SetPositionLayoutXY(updatedX, updatedY, k, l == 0);
                            }
                        }
                    }
                }

                this.m_experienceVersion = 1;
            }
        }

        public void UpdateBattleShieldStatus(bool unk)
        {
            if (this.m_homeOwnerAvatar.IsClientAvatar())
            {
                if (this.m_gameMode.GetState() != 5)
                {
                    if (this.m_matchType > 7 || this.m_matchType != 3 && this.m_matchType != 5 && this.m_matchType != 7)
                    {
                        LogicGlobals globals = LogicDataTables.GetGlobals();

                        int destructionPercentage = this.m_battleLog.GetDestructionPercentage();
                        int shieldHours = 0;

                        if (destructionPercentage >= globals.GetShieldTriggerPercentageHousingSpace())
                        {
                            shieldHours = globals.GetDestructionToShield(destructionPercentage);
                        }

                        if (shieldHours > 0 && !unk)
                        {
                            LogicClientAvatar homeOwnerAvatar = (LogicClientAvatar) this.m_homeOwnerAvatar;
                            LogicLeagueData leagueData = homeOwnerAvatar.GetLeagueTypeData();

                            if (leagueData == null)
                            {
                                leagueData = (LogicLeagueData) LogicDataTables.GetTable(LogicDataType.LEAGUE).GetItemAt(0);
                            }

                            int villageGuardMins = leagueData.GetVillageGuardInMins();

                            if (homeOwnerAvatar.GetAttackShieldReduceCounter() != 0)
                            {
                                homeOwnerAvatar.SetAttackShieldReduceCounter(0);
                                homeOwnerAvatar.GetChangeListener().AttackShieldReduceCounterChanged(0);
                            }

                            if (homeOwnerAvatar.GetDefenseVillageGuardCounter() != 0)
                            {
                                homeOwnerAvatar.SetDefenseVillageGuardCounter(0);
                                homeOwnerAvatar.GetChangeListener().DefenseVillageGuardCounterChanged(0);
                            }

                            this.m_home.GetChangeListener().ShieldActivated(shieldHours * 3600, villageGuardMins * 60);
                        }

                        if (shieldHours > this.m_shieldActivatedHours)
                        {
                            this.m_gameListener.ShieldActivated(shieldHours);
                            this.m_shieldActivatedHours = shieldHours;
                        }
                    }
                }
            }
        }

        public int GetDefenseShieldActivatedHours()
        {
            return this.m_shieldActivatedHours;
        }

        public void UpdateBattleStatus()
        {
            int state = this.m_gameMode.GetState();
            Debugger.DoAssert(state == 2 || state == 3 || state == 5, "updateBattleStatus in non combat state.");
            int aliveBuildingCount = this.GetBuildingCount(false, true);

            if (state == 2 || state == 5)
            {
                if (aliveBuildingCount < this.m_aliveBuildingCount)
                {
                    this.m_battleLog.SetDestructionPercentage(100 - 100 * aliveBuildingCount / this.m_destructibleBuildingCount);
                }
            }

            this.m_aliveBuildingCount = aliveBuildingCount;

            LogicArrayList<LogicGameObject> gameObjects = this.m_gameObjectManagers[this.m_villageType].GetGameObjects(LogicGameObjectType.CHARACTER);

            bool battleWaitForDieDamage = this.m_gameMode.GetConfiguration().GetBattleWaitForDieDamage();
            int damageCharacterCount = 0;

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                LogicCharacter character = (LogicCharacter) gameObjects[i];
                LogicHitpointComponent hitpointComponent = character.GetHitpointComponent();

                if (hitpointComponent != null && hitpointComponent.GetTeam() == 0)
                {
                    LogicAttackerItemData data = character.GetAttackerItemData();

                    if (data.GetDamage(0, false) > 0 && (hitpointComponent.GetHitpoints() > 0 || battleWaitForDieDamage && character.GetWaitDieDamage()))
                    {
                        damageCharacterCount += 1;
                    }
                }
            }

            int availableUnitCount = 0;
            bool containsSpells = false;
            bool containsAlliancePortals = false;

            if (this.m_villageType == 1)
            {
                availableUnitCount = this.m_visitorAvatar.GetUnitsTotalVillage2();

                LogicDataTable dataTable = LogicDataTables.GetTable(LogicDataType.HERO);

                for (int i = 0; i < dataTable.GetItemCount(); i++)
                {
                    LogicHeroData data = (LogicHeroData) dataTable.GetItemAt(i);

                    if (data.GetVillageType() == this.m_villageType && this.m_visitorAvatar.GetHeroState(data) != 0)
                    {
                        if (this.m_placedHeroData == null || this.m_placedHeroData.IndexOf(data) == -1)
                        {
                            ++availableUnitCount;
                        }
                    }
                }
            }
            else
            {
                availableUnitCount = this.m_visitorAvatar.GetUnitsTotal() + this.m_visitorAvatar.GetDamagingSpellsTotal();

                if (LogicDataTables.GetGlobals().FixClanPortalBattleNotEnding() && this.m_gameObjectManagers[0].GetAlliancePortal() != null)
                {
                    availableUnitCount += this.m_gameObjectManagers[0].GetAlliancePortal().GetBunkerComponent().GetUsedCapacity();
                }
                else
                {
                    availableUnitCount += this.m_visitorAvatar.GetAllianceCastleUsedCapacity();
                }

                LogicDataTable heroTable = LogicDataTables.GetTable(LogicDataType.HERO);

                for (int i = 0; i < heroTable.GetItemCount(); i++)
                {
                    LogicHeroData data = (LogicHeroData) heroTable.GetItemAt(i);

                    if (data.GetVillageType() == this.m_villageType && this.m_visitorAvatar.GetHeroState(data) != 0)
                    {
                        if (this.m_placedHeroData == null || this.m_placedHeroData.IndexOf(data) == -1)
                        {
                            ++availableUnitCount;
                        }
                    }
                }

                LogicArrayList<LogicGameObject> spells = this.m_gameObjectManagers[this.m_villageType].GetGameObjects(LogicGameObjectType.SPELL);

                for (int i = 0; i < spells.Size(); i++)
                {
                    LogicSpell spell = (LogicSpell) spells[i];

                    if (!spell.GetHitsCompleted() && (spell.GetSpellData().IsDamageSpell() || spell.GetSpellData().GetSummonTroop() != null))
                    {
                        containsSpells = true;
                    }
                }

                LogicArrayList<LogicGameObject> alliancePortals = this.m_gameObjectManagers[this.m_villageType].GetGameObjects(LogicGameObjectType.ALLIANCE_PORTAL);

                for (int i = 0; i < alliancePortals.Size(); i++)
                {
                    LogicAlliancePortal alliancePortal = (LogicAlliancePortal) alliancePortals[i];

                    if (alliancePortal.GetBunkerComponent().GetTeam() == 0 && !alliancePortal.GetBunkerComponent().IsEmpty())
                    {
                        containsAlliancePortals = true;
                    }
                }
            }

            if ((this.m_matchType == 5 || this.m_matchType == 8) && this.m_gameMode.GetState() != 5)
            {
                this.UpdateBattleFeedback();
            }

            if (aliveBuildingCount == 0 || !(containsSpells | containsAlliancePortals) && (damageCharacterCount | availableUnitCount) == 0 &&
                (this.m_gameObjectManagers[this.m_villageType].GetGameObjects(LogicGameObjectType.PROJECTILE).Size() == 0 ||
                 !this.m_gameMode.GetConfiguration().GetBattleWaitForProjectileDestruction()) && this.m_matchType != 6)
            {
                this.m_battleEndPending = true;
            }
        }

        public void UpdateBattleFeedback()
        {
            LogicAvatarChangeListener avatarChangeListener = this.GetPlayerAvatar().GetChangeListener();

            if (avatarChangeListener != null)
            {
                if (!this.m_feedbackTownHallDestroyed)
                {
                    if (this.m_battleLog.GetTownHallDestroyed())
                    {
                        this.m_feedbackTownHallDestroyed = true;
                        avatarChangeListener.BattleFeedback(3, this.m_battleLog.GetStars());
                    }
                }

                if (!this.m_feedbackDestruction25)
                {
                    if (this.m_battleLog.GetDestructionPercentage() >= 25)
                    {
                        this.m_feedbackDestruction25 = true;
                        avatarChangeListener.BattleFeedback(0, this.m_battleLog.GetStars());
                    }
                }

                if (!this.m_feedbackDestruction50)
                {
                    if (this.m_battleLog.GetDestructionPercentage() >= 50)
                    {
                        this.m_feedbackDestruction50 = true;
                        avatarChangeListener.BattleFeedback(1, this.m_battleLog.GetStars());
                    }
                }

                if (!this.m_feedbackDestruction75)
                {
                    if (this.m_battleLog.GetDestructionPercentage() >= 75)
                    {
                        this.m_feedbackDestruction75 = true;
                        avatarChangeListener.BattleFeedback(2, this.m_battleLog.GetStars());
                    }
                }
            }
        }

        public void ReengageLootCart(int secs)
        {
            if (this.GetState() == 1)
            {
                LogicGlobals globals = LogicDataTables.GetGlobals();

                if (globals.GetLootCartReengagementMinSeconds() < secs)
                {
                    if (globals.GetLootCartReengagementMaxSeconds() <= secs)
                    {
                        secs = globals.GetLootCartReengagementMaxSeconds();
                    }

                    int interval = globals.GetLootCartReengagementMaxSeconds() - globals.GetLootCartReengagementMinSeconds();
                    int time = 100 * (secs - globals.GetLootCartReengagementMinSeconds()) / interval;

                    if (time > 0)
                    {
                        if (this.m_homeOwnerAvatar != null)
                        {
                            if (this.m_homeOwnerAvatar.GetTownHallLevel() > 0)
                            {
                                LogicGameObjectManager gameObjectManager = this.m_gameObjectManagers[0];

                                if (gameObjectManager.GetLootCart() == null)
                                {
                                    gameObjectManager.AddLootCart();
                                }

                                gameObjectManager.GetLootCart().ReengageLootCart(time);
                            }
                        }
                    }
                }
            }
        }

        public void DebugResetWarTutorials()
        {
            this.m_warTutorialsSeen = 0;
            this.m_missionManager.DebugResetWarTutorials();
        }

        public void Destruct()
        {
            for (int i = 0; i < LogicLevel.VILLAGE_COUNT; i++)
            {
                if (this.m_gameObjectManagers[i] != null)
                {
                    this.m_gameObjectManagers[i].Destruct();
                    this.m_gameObjectManagers[i] = null;
                }

                if (this.m_workerManagers[i] != null)
                {
                    this.m_workerManagers[i].Destruct();
                    this.m_workerManagers[i] = null;
                }
            }

            if (this.m_placedHero != null)
            {
                this.m_placedHero.Destruct();
                this.m_placedHero = null;
                this.m_placedHeroData = null;
            }

            if (this.m_tileMap != null)
            {
                this.m_tileMap.Destruct();
                this.m_tileMap = null;
            }

            if (this.m_playArea != null)
            {
                this.m_playArea.Destruct();
                this.m_playArea = null;
            }

            if (this.m_offerManager != null)
            {
                this.m_offerManager.Destruct();
                this.m_offerManager = null;
            }

            if (this.m_achievementManager != null)
            {
                this.m_achievementManager.Destruct();
                this.m_achievementManager = null;
            }

            if (this.m_cooldownManager != null)
            {
                this.m_cooldownManager.Destruct();
                this.m_cooldownManager = null;
            }

            if (this.m_missionManager != null)
            {
                this.m_missionManager.Destruct();
                this.m_missionManager = null;
            }

            if (this.m_battleLog != null)
            {
                this.m_battleLog.Destruct();
                this.m_battleLog = null;
            }

            if (this.m_gameListener != null)
            {
                this.m_gameListener.Destruct();
                this.m_gameListener = null;
            }

            if (this.m_newShopBuildings != null)
            {
                this.m_newShopBuildings.Destruct();
                this.m_newShopBuildings = null;
            }

            if (this.m_newShopTraps != null)
            {
                this.m_newShopTraps.Destruct();
                this.m_newShopTraps = null;
            }

            if (this.m_newShopDecos != null)
            {
                this.m_newShopDecos.Destruct();
                this.m_newShopDecos = null;
            }

            this.m_layoutState.Destruct();
            this.m_layoutStateVillage2.Destruct();
            this.m_layoutCooldown.Destruct();
            this.m_armyNames.Destruct();

            if (this.m_unplacedObjects != null)
            {
                for (int i = this.m_unplacedObjects.Size() - 1; i >= 0; i--)
                {
                    this.m_unplacedObjects[i].Destruct();
                    this.m_unplacedObjects.Remove(i);
                }

                this.m_unplacedObjects = null;
            }

            if (this.m_homeOwnerAvatar != null)
            {
                this.m_homeOwnerAvatar.SetLevel(null);
                this.m_homeOwnerAvatar = null;
            }

            this.m_levelJSON = null;
            this.m_gameMode = null;
            this.m_home = null;
            this.m_visitorAvatar = null;
            this.m_revengeId = null;
        }
    }
}