namespace Supercell.Magic.Logic.GameObject.Component
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Battle;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Time;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicBunkerComponent : LogicUnitStorageComponent
    {
        public const int PATROL_PATHS = 16;

        private readonly LogicGameObjectFilter m_filter;
        private LogicTimer m_requestCooldownTimer;
        private LogicTimer m_clanMailCooldownTimer;
        private LogicTimer m_replayShareCooldownTimer;
        private LogicTimer m_elderKickCooldownTimer;
        private LogicTimer m_challengeCooldownTimer;
        private LogicTimer m_arrangeWarCooldownTimer;
        private LogicArrayList<LogicVector2> m_patrolPath;

        private int m_team;
        private int m_updateAvatarCooldown;
        private int m_bunkerSearchTime;
        private int m_troopSpawnOffset;

        public LogicBunkerComponent(LogicGameObject gameObject, int capacity) : base(gameObject, capacity)
        {
            this.m_team = 1;

            this.m_filter = new LogicGameObjectFilter();
            this.m_filter.AddGameObjectType(LogicGameObjectType.CHARACTER);
            this.m_filter.PassEnemyOnly(gameObject);
        }

        public int GetTeam()
        {
            return this.m_team;
        }

        public void SetComponentMode(int value)
        {
            this.m_team = value;
        }

        public override LogicComponentType GetComponentType()
        {
            return LogicComponentType.BUNKER;
        }

        public void StartRequestCooldownTime()
        {
            if (this.m_requestCooldownTimer == null)
            {
                this.m_requestCooldownTimer = new LogicTimer();
            }

            this.m_requestCooldownTimer.StartTimer(this.GetTotalRequestCooldownTime(), this.m_parent.GetLevel().GetLogicTime(), false, -1);
        }

        public void StartClanMailCooldownTime()
        {
            if (this.m_clanMailCooldownTimer == null)
            {
                this.m_clanMailCooldownTimer = new LogicTimer();
            }

            this.m_clanMailCooldownTimer.StartTimer(LogicDataTables.GetGlobals().GetClanMailCooldown(), this.m_parent.GetLevel().GetLogicTime(), false, -1);
        }

        public void StartChallengeCooldownTime()
        {
            if (this.m_challengeCooldownTimer == null)
            {
                this.m_challengeCooldownTimer = new LogicTimer();
            }

            this.m_challengeCooldownTimer.StartTimer(LogicDataTables.GetGlobals().GetChallengeCooldown(), this.m_parent.GetLevel().GetLogicTime(), false, -1);
        }

        public void StartReplayShareCooldownTime()
        {
            if (this.m_replayShareCooldownTimer == null)
            {
                this.m_replayShareCooldownTimer = new LogicTimer();
            }

            this.m_replayShareCooldownTimer.StartTimer(LogicDataTables.GetGlobals().GetReplayShareCooldown(), this.m_parent.GetLevel().GetLogicTime(), false, -1);
        }

        public void StartArrangedWarCooldownTime()
        {
            if (this.m_arrangeWarCooldownTimer == null)
            {
                this.m_arrangeWarCooldownTimer = new LogicTimer();
            }

            this.m_arrangeWarCooldownTimer.StartTimer(LogicDataTables.GetGlobals().GetArrangeWarCooldown(), this.m_parent.GetLevel().GetLogicTime(), false, -1);
        }

        public void StopRequestCooldownTime()
        {
            if (this.m_requestCooldownTimer != null)
            {
                this.m_requestCooldownTimer.Destruct();
                this.m_requestCooldownTimer = null;
            }
        }

        public int GetRequestCooldownTime()
        {
            if (this.m_requestCooldownTimer != null)
            {
                return this.m_requestCooldownTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());
            }

            return 0;
        }

        public void StartElderKickCooldownTime()
        {
            if (this.m_elderKickCooldownTimer == null)
            {
                this.m_elderKickCooldownTimer = new LogicTimer();
            }

            this.m_elderKickCooldownTimer.StartTimer(LogicDataTables.GetGlobals().GetElderKickCooldown(), this.m_parent.GetLevel().GetLogicTime(), false, -1);
        }

        public int GetElderCooldownTime()
        {
            if (this.m_elderKickCooldownTimer != null)
            {
                return this.m_elderKickCooldownTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());
            }

            return 0;
        }

        public int GetReplayShareCooldownTime()
        {
            if (this.m_replayShareCooldownTimer != null)
            {
                return this.m_replayShareCooldownTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());
            }

            return 0;
        }

        public int GetArrangedWarCooldownTime()
        {
            if (this.m_arrangeWarCooldownTimer != null)
            {
                return this.m_arrangeWarCooldownTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());
            }

            return 0;
        }

        public int GetTotalRequestCooldownTime()
        {
            LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();

            if (homeOwnerAvatar != null)
            {
                return ((LogicClientAvatar) homeOwnerAvatar).GetTroopRequestCooldown();
            }

            return LogicDataTables.GetGlobals().GetAllianceTroopRequestCooldown();
        }

        public int GetClanMailCooldownTime()
        {
            if (this.m_clanMailCooldownTimer != null)
            {
                return this.m_clanMailCooldownTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());
            }

            return 0;
        }

        public int GetChallengeCooldownTime()
        {
            if (this.m_challengeCooldownTimer != null)
            {
                return this.m_challengeCooldownTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());
            }

            return 0;
        }

        public override void LoadingFinished()
        {
            if (this.m_parent.GetLevel().IsInCombatState())
            {
                this.m_patrolPath = this.CreatePatrolPath();
            }
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            if (this.m_requestCooldownTimer != null)
            {
                this.m_requestCooldownTimer.Destruct();
                this.m_requestCooldownTimer = null;
            }

            if (this.m_clanMailCooldownTimer != null)
            {
                this.m_clanMailCooldownTimer.Destruct();
                this.m_clanMailCooldownTimer = null;
            }

            if (this.m_replayShareCooldownTimer != null)
            {
                this.m_replayShareCooldownTimer.Destruct();
                this.m_replayShareCooldownTimer = null;
            }

            if (this.m_elderKickCooldownTimer != null)
            {
                this.m_elderKickCooldownTimer.Destruct();
                this.m_elderKickCooldownTimer = null;
            }

            if (this.m_challengeCooldownTimer != null)
            {
                this.m_challengeCooldownTimer.Destruct();
                this.m_challengeCooldownTimer = null;
            }

            if (this.m_arrangeWarCooldownTimer != null)
            {
                this.m_arrangeWarCooldownTimer.Destruct();
                this.m_arrangeWarCooldownTimer = null;
            }

            LogicJSONNumber unitRequestTimeNumber = jsonObject.GetJSONNumber("unit_req_time");

            if (unitRequestTimeNumber != null)
            {
                this.m_requestCooldownTimer = new LogicTimer();
                this.m_requestCooldownTimer.StartTimer(LogicMath.Min(unitRequestTimeNumber.GetIntValue(), this.GetTotalRequestCooldownTime()),
                                                       this.m_parent.GetLevel().GetLogicTime(), false, -1);
            }

            LogicJSONNumber clanMailTimeNumber = jsonObject.GetJSONNumber("clan_mail_time");

            if (clanMailTimeNumber != null)
            {
                this.m_clanMailCooldownTimer = new LogicTimer();
                this.m_clanMailCooldownTimer.StartTimer(LogicMath.Min(clanMailTimeNumber.GetIntValue(), LogicDataTables.GetGlobals().GetClanMailCooldown()),
                                                        this.m_parent.GetLevel().GetLogicTime(), false, -1);
            }

            LogicJSONNumber shareReplayTimeNumber = jsonObject.GetJSONNumber("share_replay_time");

            if (shareReplayTimeNumber != null)
            {
                this.m_replayShareCooldownTimer = new LogicTimer();
                this.m_replayShareCooldownTimer.StartTimer(LogicMath.Min(shareReplayTimeNumber.GetIntValue(), LogicDataTables.GetGlobals().GetReplayShareCooldown()),
                                                           this.m_parent.GetLevel().GetLogicTime(), false, -1);
            }

            LogicJSONNumber elderKickTimeNumber = jsonObject.GetJSONNumber("elder_kick_time");

            if (elderKickTimeNumber != null)
            {
                this.m_elderKickCooldownTimer = new LogicTimer();
                this.m_elderKickCooldownTimer.StartTimer(LogicMath.Min(elderKickTimeNumber.GetIntValue(), LogicDataTables.GetGlobals().GetElderKickCooldown()),
                                                         this.m_parent.GetLevel().GetLogicTime(), false, -1);
            }

            LogicJSONNumber challengeTimeNumber = jsonObject.GetJSONNumber("challenge_time");

            if (challengeTimeNumber != null)
            {
                this.m_challengeCooldownTimer = new LogicTimer();
                this.m_challengeCooldownTimer.StartTimer(LogicMath.Min(challengeTimeNumber.GetIntValue(), LogicDataTables.GetGlobals().GetChallengeCooldown()),
                                                         this.m_parent.GetLevel().GetLogicTime(), false, -1);
            }

            LogicJSONNumber arrangeWarTimeNumber = jsonObject.GetJSONNumber("arrwar_time");

            if (arrangeWarTimeNumber != null)
            {
                this.m_arrangeWarCooldownTimer = new LogicTimer();
                this.m_arrangeWarCooldownTimer.StartTimer(LogicMath.Min(arrangeWarTimeNumber.GetIntValue(), LogicDataTables.GetGlobals().GetArrangeWarCooldown()),
                                                          this.m_parent.GetLevel().GetLogicTime(), false, -1);
            }
        }

        public override void Save(LogicJSONObject jsonObject, int villageType)
        {
            if (this.m_requestCooldownTimer != null)
            {
                int remainingSecs = this.m_requestCooldownTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());

                if (remainingSecs > 0)
                {
                    jsonObject.Put("unit_req_time", new LogicJSONNumber(remainingSecs));
                }
            }

            if (this.m_clanMailCooldownTimer != null)
            {
                int remainingSecs = this.m_clanMailCooldownTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());

                if (remainingSecs > 0)
                {
                    jsonObject.Put("clan_mail_time", new LogicJSONNumber(remainingSecs));
                }
            }

            if (this.m_replayShareCooldownTimer != null)
            {
                int remainingSecs = this.m_replayShareCooldownTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());

                if (remainingSecs > 0)
                {
                    jsonObject.Put("share_replay_time", new LogicJSONNumber(remainingSecs));
                }
            }

            if (this.m_elderKickCooldownTimer != null)
            {
                int remainingSecs = this.m_elderKickCooldownTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());

                if (remainingSecs > 0)
                {
                    jsonObject.Put("elder_kick_time", new LogicJSONNumber(remainingSecs));
                }
            }

            if (this.m_challengeCooldownTimer != null)
            {
                int remainingSecs = this.m_challengeCooldownTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());

                if (remainingSecs > 0)
                {
                    jsonObject.Put("challenge_time", new LogicJSONNumber(remainingSecs));
                }
            }

            if (this.m_arrangeWarCooldownTimer != null)
            {
                int remainingSecs = this.m_arrangeWarCooldownTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());

                if (remainingSecs > 0)
                {
                    jsonObject.Put("arrwar_time", new LogicJSONNumber(remainingSecs));
                }
            }
        }

        public LogicCharacter ClosestAttacker(bool flyingTroop)
        {
            LogicGameObjectManager gameObjectManager = this.m_parent.GetLevel().GetGameObjectManagerAt(0);
            LogicArrayList<LogicGameObject> gameObjects = gameObjectManager.GetGameObjects(LogicGameObjectType.CHARACTER);

            int closestDistance = 0x7fffffff;
            LogicCharacter closestCharacter = null;

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                LogicCharacter character = (LogicCharacter) gameObjects[i];
                LogicHitpointComponent hitpointComponent = character.GetHitpointComponent();
                LogicCombatComponent combatComponent = character.GetCombatComponent();

                bool deployTime = combatComponent != null && combatComponent.GetUndergroundTime() > 0;

                if (!deployTime && (LogicDataTables.GetGlobals().SkeletonOpenClanCastle() || !LogicDataTables.IsSkeleton(character.GetCharacterData())))
                {
                    if (hitpointComponent != null)
                    {
                        if (character.IsAlive() && character.IsFlying() == flyingTroop && hitpointComponent.GetTeam() == 0)
                        {
                            int distance = character.GetPosition().GetDistanceSquaredTo(this.m_parent.GetMidX(), this.m_parent.GetMidY());

                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                closestCharacter = character;
                            }
                        }
                    }
                }
            }

            return closestCharacter;
        }

        public override void Tick()
        {
            LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();

            if (homeOwnerAvatar != null)
            {
                this.m_updateAvatarCooldown += 64;

                if (this.m_updateAvatarCooldown > 1000)
                {
                    homeOwnerAvatar.UpdateStarBonusLimitCooldown();
                    homeOwnerAvatar.UpdateLootLimitCooldown();

                    this.m_updateAvatarCooldown -= 1000;
                }
            }

            if (this.m_parent.IsAlive())
            {
                if (!this.IsEmpty())
                {
                    if (this.m_bunkerSearchTime > 0)
                    {
                        this.m_bunkerSearchTime -= 64;
                    }
                    else
                    {
                        bool airTriggered = false;
                        bool groundLocked = false;

                        if (this.m_team == 1)
                        {
                            bool inAirDistance = false;
                            bool inGroundDistance = false;

                            int clanCastleRadius = LogicDataTables.GetGlobals().GetClanCastleRadius();

                            if (LogicDataTables.GetGlobals().CastleTroopTargetFilter())
                            {
                                LogicCharacter closestGroundAttacker = this.ClosestAttacker(false);
                                LogicCharacter closestAirAttacker = this.ClosestAttacker(true);

                                if (closestAirAttacker != null)
                                {
                                    inAirDistance = closestAirAttacker.GetPosition().GetDistanceSquaredTo(this.m_parent.GetX(), this.m_parent.GetY()) <
                                                    clanCastleRadius * clanCastleRadius;
                                }

                                if (closestGroundAttacker != null)
                                {
                                    inGroundDistance = closestGroundAttacker.GetPosition().GetDistanceSquaredTo(this.m_parent.GetX(), this.m_parent.GetY()) <
                                                       clanCastleRadius * clanCastleRadius;
                                }
                            }
                            else
                            {
                                LogicCharacter closestAttacker =
                                    (LogicCharacter) this.m_parent.GetLevel().GetGameObjectManager()
                                                         .GetClosestGameObject(this.m_parent.GetX(), this.m_parent.GetY(), this.m_filter);

                                if (closestAttacker != null)
                                {
                                    inAirDistance = inGroundDistance = closestAttacker.GetPosition().GetDistanceSquaredTo(this.m_parent.GetX(), this.m_parent.GetY()) <
                                                                       clanCastleRadius * clanCastleRadius;
                                }
                            }

                            groundLocked = !inGroundDistance;
                            airTriggered = inAirDistance;

                            if (!airTriggered && groundLocked)
                            {
                                this.m_bunkerSearchTime = LogicDataTables.GetGlobals().GetBunkerSearchTime();
                                return;
                            }
                        }
                        else
                        {
                            airTriggered = true;
                        }

                        LogicCharacterData spawnData = null;
                        int spawnLevel = -1;

                        for (int i = 0; i < this.GetUnitTypeCount(); i++)
                        {
                            LogicCombatItemData data = this.GetUnitType(i);

                            if (data != null)
                            {
                                int count = this.GetUnitCount(i);

                                if (count > 0)
                                {
                                    int upgLevel = this.GetUnitLevel(i);

                                    if (data.GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
                                    {
                                        LogicCharacterData characterData = (LogicCharacterData) data;
                                        LogicAttackerItemData attackerItemData = characterData.GetAttackerItemData(upgLevel);

                                        if (!(airTriggered & groundLocked) || attackerItemData.GetTrackAirTargets(false))
                                        {
                                            if (airTriggered | groundLocked || attackerItemData.GetTrackGroundTargets(false))
                                            {
                                                this.RemoveUnits(data, upgLevel, 1);

                                                spawnData = characterData;
                                                spawnLevel = upgLevel;
                                            }
                                        }
                                    }
                                }
                            }

                            if (spawnData != null)
                            {
                                break;
                            }
                        }

                        if (spawnData != null)
                        {
                            LogicCharacter character =
                                (LogicCharacter) LogicGameObjectFactory.CreateGameObject(spawnData, this.m_parent.GetLevel(), this.m_parent.GetVillageType());

                            character.GetHitpointComponent().SetTeam(this.m_team);

                            if (character.GetChildTroops() != null)
                            {
                                LogicArrayList<LogicCharacter> childrens = character.GetChildTroops();

                                for (int i = 0; i < childrens.Size(); i++)
                                {
                                    childrens[i].GetHitpointComponent().SetTeam(this.m_team);
                                }
                            }

                            character.SetUpgradeLevel(spawnLevel == -1 ? 0 : spawnLevel);
                            character.SetAllianceUnit();

                            if (character.GetCharacterData().IsJumper())
                            {
                                character.GetMovementComponent().EnableJump(3600000);
                            }

                            if (this.m_team == 1)
                            {
                                if (LogicDataTables.GetGlobals().EnableDefendingAllianceTroopJump())
                                {
                                    character.GetMovementComponent().EnableJump(3600000);
                                }

                                if (LogicDataTables.GetGlobals().AllianceTroopsPatrol())
                                {
                                    character.GetCombatComponent().SetSearchRadius(LogicDataTables.GetGlobals().GetClanCastleRadius() >> 9);

                                    if (this.m_parent.GetGameObjectType() == LogicGameObjectType.BUILDING)
                                    {
                                        character.GetMovementComponent().SetBaseBuilding((LogicBuilding) this.m_parent);
                                    }
                                }
                            }
                            else
                            {
                                LogicAvatar visitorAvatar = this.m_parent.GetLevel().GetVisitorAvatar();

                                visitorAvatar.RemoveAllianceUnit(spawnData, spawnLevel);
                                visitorAvatar.GetChangeListener().AllianceUnitRemoved(spawnData, spawnLevel);

                                LogicBattleLog battleLog = this.m_parent.GetLevel().GetBattleLog();

                                battleLog.IncrementDeployedAllianceUnits(spawnData, 1, spawnLevel);
                                battleLog.SetAllianceUsed(true);
                            }

                            if (this.m_team == 1)
                            {
                                int spawnOffsetX = 0;
                                int spawnOffsetY = 0;

                                switch (this.m_troopSpawnOffset)
                                {
                                    case 0:
                                        spawnOffsetX = 1;
                                        spawnOffsetY = 0;
                                        break;
                                    case 1:
                                        spawnOffsetX = -1;
                                        spawnOffsetY = 0;
                                        break;
                                    case 2:
                                        spawnOffsetX = 0;
                                        spawnOffsetY = 1;
                                        break;
                                    case 3:
                                        spawnOffsetX = 0;
                                        spawnOffsetY = -1;
                                        break;
                                }

                                character.SetInitialPosition(this.m_parent.GetMidX() + ((this.m_parent.GetWidthInTiles() << 8) - 128) * spawnOffsetX,
                                                             this.m_parent.GetMidY() + ((this.m_parent.GetHeightInTiles() << 8) - 128) * spawnOffsetY);

                                if (++this.m_troopSpawnOffset > 3)
                                {
                                    this.m_troopSpawnOffset = 0;
                                }
                            }
                            else if (LogicDataTables.GetGlobals().AllowClanCastleDeployOnObstacles())
                            {
                                int posX = this.m_parent.GetX() + (this.m_parent.GetWidthInTiles() << 9) - 128;
                                int posY = this.m_parent.GetY() + (this.m_parent.GetHeightInTiles() << 8);

                                if (LogicGamePlayUtil.GetNearestValidAttackPos(this.m_parent.GetLevel(), posX, posY, out int outputX, out int outputY))
                                {
                                    character.SetInitialPosition(outputX, outputY);
                                }
                                else
                                {
                                    character.SetInitialPosition(posX, posY);
                                }
                            }
                            else
                            {
                                character.SetInitialPosition(this.m_parent.GetX() + (this.m_parent.GetWidthInTiles() << 9) - 128,
                                                             this.m_parent.GetY() + (this.m_parent.GetHeightInTiles() << 8));
                            }

                            this.m_parent.GetGameObjectManager().AddGameObject(character, -1);
                        }

                        this.m_bunkerSearchTime = LogicDataTables.GetGlobals().GetBunkerSearchTime();
                    }
                }
            }
        }

        public override void FastForwardTime(int time)
        {
            if (this.m_requestCooldownTimer != null)
            {
                this.m_requestCooldownTimer.FastForward(time);
            }

            if (this.m_replayShareCooldownTimer != null)
            {
                this.m_replayShareCooldownTimer.FastForward(time);
            }

            if (this.m_elderKickCooldownTimer != null)
            {
                this.m_elderKickCooldownTimer.FastForward(time);
            }

            if (this.m_clanMailCooldownTimer != null)
            {
                this.m_clanMailCooldownTimer.FastForward(time);
            }

            if (this.m_challengeCooldownTimer != null)
            {
                this.m_challengeCooldownTimer.FastForward(time);
            }

            if (this.m_arrangeWarCooldownTimer != null)
            {
                this.m_arrangeWarCooldownTimer.FastForward(time);
            }

            LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();

            if (homeOwnerAvatar != null)
            {
                homeOwnerAvatar.FastForwardStarBonusLimit(time);
                homeOwnerAvatar.FastForwardLootLimit(time);
            }
        }

        public LogicArrayList<LogicVector2> GetPatrolPath()
        {
            return this.m_patrolPath;
        }

        public LogicArrayList<LogicVector2> CreatePatrolPath()
        {
            int width = this.m_parent.GetWidthInTiles() << 8;
            int height = this.m_parent.GetHeightInTiles() << 8;

            if (width * width + height * height <= 0x240000)
            {
                int midX = this.m_parent.GetMidX();
                int midY = this.m_parent.GetMidY();

                LogicVector2 tmp1 = new LogicVector2();
                LogicVector2 tmp2 = new LogicVector2();
                LogicVector2 tmp3 = new LogicVector2();
                LogicVector2 tmp4 = new LogicVector2();

                tmp2.Set(midX, midY);

                LogicArrayList<LogicVector2> wayPoints = new LogicArrayList<LogicVector2>(LogicBunkerComponent.PATROL_PATHS);

                for (int i = 0, j = 360; i < LogicBunkerComponent.PATROL_PATHS; i++, j += 720)
                {
                    tmp1.Set(midX + LogicMath.Cos(j >> 5, 1536), midY + LogicMath.Sin(j >> 5, 1536));
                    LogicHeroBaseComponent.FindPoint(this.m_parent.GetLevel().GetTileMap(), tmp3, tmp2, tmp1, tmp4);
                    wayPoints.Add(new LogicVector2(tmp4.m_x, tmp4.m_y));
                }

                tmp1.Destruct();
                tmp2.Destruct();
                tmp3.Destruct();
                tmp4.Destruct();

                return wayPoints;
            }
            else
            {
                int startX = this.m_parent.GetX() + (this.m_parent.GetWidthInTiles() << 9) - 128;
                int startY = this.m_parent.GetY() + (this.m_parent.GetWidthInTiles() << 9) - 128;
                int endX = this.m_parent.GetX() + 128;
                int endY = this.m_parent.GetY() + 128;

                LogicArrayList<LogicVector2> wayPoints = new LogicArrayList<LogicVector2>(4);

                wayPoints.Add(new LogicVector2(startX, startY));
                wayPoints.Add(new LogicVector2(endX, startY));
                wayPoints.Add(new LogicVector2(endX, endY));
                wayPoints.Add(new LogicVector2(startX, endY));

                return wayPoints;
            }
        }
    }
}