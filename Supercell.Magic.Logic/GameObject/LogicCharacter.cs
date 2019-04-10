namespace Supercell.Magic.Logic.GameObject
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicCharacter : LogicGameObject
    {
        private int m_upgradeLevel;
        private int m_abilityTriggerTime; // 156
        private int m_abilityTime; // 160
        private int m_abilityCooldown; // 164
        private int m_abilityAttackCount; // 172
        private int m_summonSpawnCount; // 168
        private int m_troopChildX;
        private int m_troopChildY;
        private int m_duplicateLifeTime; // 228
        private int m_dieTime;
        private int m_spawnTime; // 132 
        private int m_spawnIdleTime; // 136
        private int m_secondaryTroopTeam;
        private int m_autoMergeSize;
        private int m_autoMergeTime;
        private int m_loseHpTime;
        private int m_rageAloneTime; // 232
        private int m_activationTimeState; // 236
        private int m_activationTime; // 240

        private bool m_flying;
        private bool m_duplicate; // 225
        private bool m_abilityUsed; // 211
        private bool m_troopChild;
        private bool m_allianceUnit;
        private bool m_ejected;
        private bool m_hasSpawnDelay; // 209
        private bool m_retributionSpellCreated; // 224

        private LogicSpell m_auraSpell; // 212
        private LogicSpell m_abilitySpell; // 216
        private LogicSpell m_retributionSpell; // 220

        private LogicCharacter m_parent;
        private LogicCharacterData m_summoner; // 152
        private readonly LogicVector2 m_ejectPosition;
        private readonly LogicArrayList<LogicCharacter> m_summonTroops;
        private readonly LogicArrayList<LogicCharacter> m_childrens;

        public LogicCharacter(LogicGameObjectData data, LogicLevel level, int villageType) : base(data, level, villageType)
        {
            LogicCharacterData characterData = (LogicCharacterData) data;

            this.m_flying = characterData.IsFlying();

            this.AddComponent(new LogicHitpointComponent(this, characterData.GetHitpoints(0), 0));
            this.AddComponent(new LogicCombatComponent(this));
            this.AddComponent(new LogicMovementComponent(this, characterData.GetSpeed(), characterData.IsFlying(), characterData.IsUnderground()));
            this.SetUpgradeLevel(0);

            int childTroopCount = characterData.GetChildTroopCount();

            if (childTroopCount > 0)
            {
                this.m_childrens = new LogicArrayList<LogicCharacter>(childTroopCount);

                for (int i = 0; i < childTroopCount; i++)
                {
                    LogicCharacter character = new LogicCharacter(characterData.GetChildTroop(), level, villageType);

                    character.SetTroopChild(this, i);
                    character.GetCombatComponent().SetTroopChild(true);

                    this.m_childrens.Add(character);
                    this.GetGameObjectManager().AddGameObject(character, -1);
                }
            }

            this.m_ejectPosition = new LogicVector2();
            this.m_summonTroops = new LogicArrayList<LogicCharacter>();

            if (characterData.IsUnderground())
            {
                this.GetCombatComponent().SetUndergroundTime(3600000);
            }
        }

        public LogicCharacterData GetCharacterData()
        {
            return (LogicCharacterData) this.m_data;
        }

        public LogicAttackerItemData GetAttackerItemData()
        {
            return this.GetCharacterData().GetAttackerItemData(this.m_upgradeLevel);
        }

        public override LogicGameObjectType GetGameObjectType()
        {
            return LogicGameObjectType.CHARACTER;
        }

        public override bool IsHero()
        {
            return this.GetCharacterData().GetDataType() == LogicDataType.HERO;
        }

        public override bool IsFlying()
        {
            return this.m_flying;
        }

        public int GetSecondaryTroopTeam()
        {
            return this.m_secondaryTroopTeam;
        }

        public void SetSecondaryTroopTeam(int value)
        {
            this.m_secondaryTroopTeam = value;
        }

        public bool GetWaitDieDamage()
        {
            return this.GetCharacterData().GetDieDamageDelay() > this.m_dieTime;
        }

        public int GetSpawnDelay()
        {
            return this.m_spawnTime;
        }

        public void SetSpawnTime(int time)
        {
            this.m_spawnTime = time;

            if (time > 0)
            {
                this.m_hasSpawnDelay = true;
            }
        }

        public int GetSpawnIdleTime()
        {
            return this.m_spawnIdleTime;
        }

        public int GetSummonTroopCount()
        {
            return this.m_summonTroops.Size();
        }

        public LogicArrayList<LogicCharacter> GetChildTroops()
        {
            return this.m_childrens;
        }

        public override void SetInitialPosition(int x, int y)
        {
            base.SetInitialPosition(x, y);

            LogicMovementComponent movementComponent = this.GetMovementComponent();

            if (movementComponent != null)
            {
                movementComponent.GetMovementSystem().Reset(x, y);
            }

            if (this.m_childrens != null)
            {
                for (int i = 0; i < this.m_childrens.Size(); i++)
                {
                    this.m_childrens[i].SetInitialPosition(x, y);
                }
            }
        }

        public override bool ShouldDestruct()
        {
            if (this.m_level.IsInCombatState())
            {
                int fadingOutTime = 5000;

                if (this.m_duplicate)
                {
                    fadingOutTime += (this.m_duplicateLifeTime >> 31) & -4999;
                }

                return this.m_dieTime > fadingOutTime;
            }

            return true;
        }

        public bool HasSpecialAbility()
        {
            return this.GetCharacterData().GetSpecialAbilityLevel(this.m_upgradeLevel) > 0;
        }

        public int GetUpgradeLevel()
        {
            return this.m_upgradeLevel;
        }

        public void SetUpgradeLevel(int upgLevel)
        {
            this.m_upgradeLevel = upgLevel;

            LogicCharacterData data = this.GetCharacterData();
            LogicHitpointComponent hitpointComponent = this.GetHitpointComponent();
            LogicCombatComponent combatComponent = this.GetCombatComponent();

            int hp = data.GetHitpoints(upgLevel);
            int damagePercentage = 100;

            if (data.GetScaleByTH())
            {
                LogicAvatar avatar = this.m_level.GetHomeOwnerAvatar();

                if (hitpointComponent != null && hitpointComponent.GetTeam() == 0)
                {
                    avatar = this.m_level.GetVisitorAvatar();
                }

                int tmp1 = 700 * avatar.GetTownHallLevel() / (LogicDataTables.GetTownHallLevelCount() - 1);

                damagePercentage = tmp1 / 10 + 30;
                hp = damagePercentage * hp / 100;

                if (damagePercentage * hp < 200)
                {
                    hp = 1;
                }

                if (tmp1 < -289)
                {
                    damagePercentage = 1;
                }
            }

            hitpointComponent.SetMaxHitpoints(hp);
            hitpointComponent.SetHitpoints(data.GetHitpoints(upgLevel));
            hitpointComponent.SetDieEffect(data.GetDieEffect(upgLevel), data.GetDieEffect2(upgLevel));

            if (combatComponent != null)
            {
                combatComponent.SetAttackValues(data.GetAttackerItemData(upgLevel), damagePercentage);
            }

            if (this.m_childrens != null)
            {
                for (int i = 0; i < this.m_childrens.Size(); i++)
                {
                    this.m_childrens[i].SetUpgradeLevel(upgLevel);
                }
            }

            if (this.IsHero())
            {
                LogicHeroData heroData = (LogicHeroData) this.m_data;
                LogicAvatar avatar = this.m_level.GetHomeOwnerAvatar();

                if (hitpointComponent.GetTeam() == 0)
                {
                    avatar = this.m_level.GetVisitorAvatar();
                }

                this.m_flying = heroData.IsFlying(avatar.GetHeroMode(heroData));
                this.GetMovementComponent().SetFlying(this.m_flying);
            }

            if (data.GetAutoMergeDistance() > 0)
            {
                this.m_autoMergeTime = 2000;
            }

            int speed = data.GetSpeed();

            if (data.GetSpecialAbilityLevel(this.m_upgradeLevel) > 0 &&
                data.GetSpecialAbilityType() == LogicCharacterData.SPECIAL_ABILITY_TYPE_SPEED_BOOST)
            {
                speed = speed * data.GetSpecialAbilityAttribute(this.m_upgradeLevel) / 100;
            }

            this.GetMovementComponent().SetSpeed(speed);
        }

        public LogicCharacter GetParent()
        {
            return this.m_parent;
        }

        public void SetTroopChild(LogicCharacter character, int idx)
        {
            this.m_parent = character;

            if (character != null)
            {
                this.m_troopChildX = (character.GetCharacterData().GetChildTroopX(idx) << 9) / 100;
                this.m_troopChildY = (character.GetCharacterData().GetChildTroopY(idx) << 9) / 100;
                this.m_troopChild = true;
            }
        }

        public void SetDuplicate(bool clone, int lifetime)
        {
            this.m_duplicate = clone;
            this.m_duplicateLifeTime = lifetime;
        }

        public bool TileOkForTombstone(LogicTile tile)
        {
            if (tile != null && !tile.IsFullyNotPassable())
            {
                for (int i = 0; i < tile.GetGameObjectCount(); i++)
                {
                    LogicGameObject gameObject = tile.GetGameObject(i);

                    if (gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING ||
                        gameObject.GetGameObjectType() == LogicGameObjectType.OBSTACLE ||
                        gameObject.GetGameObjectType() == LogicGameObjectType.TRAP ||
                        gameObject.GetGameObjectType() == LogicGameObjectType.DECO)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public void Eject(LogicVector2 pos)
        {
            if (!this.IsFlying())
            {
                if (pos != null)
                {
                    this.m_ejectPosition.Set(pos.m_x, pos.m_y);
                }
                else
                {
                    this.m_ejectPosition.Set(0, 0);
                }

                this.m_ejected = true;
                this.GetHitpointComponent().Kill();
            }
        }

        public void SetAllianceUnit()
        {
            this.m_allianceUnit = true;
        }

        public int GetAbilityCooldown()
        {
            return this.m_abilityCooldown;
        }

        public int GetAbilityTime()
        {
            return this.m_abilityTime;
        }

        public bool IsAbilityUsed()
        {
            return this.m_abilityUsed;
        }

        public void StartAbility()
        {
            if (this.IsHero())
            {
                LogicHeroData heroData = (LogicHeroData) this.m_data;

                this.m_abilityCooldown = 60 * heroData.GetAbilityCooldown() / 4000;
                this.m_abilityTriggerTime = 5;
                this.m_abilityTime = 60 * heroData.GetAbilityTime(this.m_upgradeLevel) / 20000;
                this.m_summonSpawnCount = 0;

                this.GetHitpointComponent().CauseDamage(-100 * heroData.GetAbilityHealthIncrease(this.m_upgradeLevel), 0, this);

                this.m_abilityAttackCount = this.GetCombatComponent().GetHitCount() + heroData.GetAbilityAttackCount(this.m_upgradeLevel);

                if (heroData.GetAbilityDelay() > 0)
                {
                    this.GetCombatComponent().SetAttackDelay(0, heroData.GetAbilityDelay());
                    // Listener.
                }

                LogicSpellData abilitySpellData = heroData.GetAbilitySpell(this.m_upgradeLevel);

                if (abilitySpellData != null)
                {
                    this.m_abilitySpell = (LogicSpell) LogicGameObjectFactory.CreateGameObject(abilitySpellData, this.m_level, this.m_villageType);
                    this.m_abilitySpell.SetUpgradeLevel(heroData.GetAbilitySpellLevel(this.m_upgradeLevel));
                    this.m_abilitySpell.SetInitialPosition(this.GetX(), this.GetY());
                    this.m_abilitySpell.AllowDestruction(false);

                    this.GetGameObjectManager().AddGameObject(this.m_abilitySpell, -1);
                }

                if (heroData.GetActivationTime() > 0)
                {
                    this.m_activationTimeState = 1;
                    this.m_activationTime = heroData.GetActivationTime();

                    this.GetMovementComponent().GetMovementSystem().SetFreezeTime(this.m_activationTime);
                    this.GetCombatComponent().SetActivationTime(this.m_activationTime);

                    // Listener.
                }

                this.m_abilityUsed = true;
            }
        }

        public bool GetSpecialAbilityAvailable()
        {
            LogicCharacterData data = this.GetCharacterData();

            if (data.GetSpecialAbilityLevel(this.m_upgradeLevel) <= 0)
            {
                return false;
            }

            switch (data.GetSpecialAbilityType())
            {
                case LogicCharacterData.SPECIAL_ABILITY_TYPE_BIG_FIRST_HIT:
                    return this.GetCombatComponent().GetHitCount() <= 0;
                case LogicCharacterData.SPECIAL_ABILITY_TYPE_SPECIAL_PROJECTILE:
                    return this.GetCombatComponent().GetHitCount() < data.GetSpecialAbilityAttribute(this.m_upgradeLevel);
            }

            return true;
        }

        public bool IsWallBreaker()
        {
            LogicCombatComponent combatComponent = this.GetCombatComponent();

            if (combatComponent != null)
            {
                return combatComponent.IsWallBreaker();
            }

            return false;
        }

        public int GetEjectTime()
        {
            if (this.m_ejected)
            {
                return this.m_dieTime + 1;
            }

            return 0;
        }

        public override int GetHitEffectOffset()
        {
            return this.GetCharacterData().GetHitEffectOffset();
        }

        public override void DeathEvent()
        {
            LogicHitpointComponent hitpointComponent = this.GetHitpointComponent();
            LogicCharacterData data = this.GetCharacterData();

            if (hitpointComponent != null && hitpointComponent.GetTeam() == 1 && !this.IsHero() && !data.IsSecondaryTroop() &&
                this.m_level.GetVillageType() == 0 && this.m_allianceUnit)
            {
                LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();

                homeOwnerAvatar.RemoveAllianceUnit(data, this.m_upgradeLevel);
                homeOwnerAvatar.GetChangeListener().AllianceUnitRemoved(data, this.m_upgradeLevel);
            }

            if (data.GetSpecialAbilityType() != LogicCharacterData.SPECIAL_ABILITY_TYPE_RESPAWN_AS_CANNON ||
                data.GetSpecialAbilityLevel(this.m_upgradeLevel) <= 0)
            {
                if (data.GetSpecialAbilityType() == LogicCharacterData.SPECIAL_ABILITY_TYPE_SPAWN_UNITS)
                {
                    if (data.GetSpecialAbilityLevel(this.m_upgradeLevel) > 0)
                    {
                        this.CheckSpawning(null, data.GetSpecialAbilityAttribute(this.m_upgradeLevel), 0, 0);
                    }
                }
                else if (data.GetSecondaryTroop() != null)
                {
                    this.CheckSpawning(null, 0, 0, 0);
                }
            }
            else if (!this.m_ejected)
            {
                this.CheckSpawning(LogicDataTables.GetCharacterByName("MovingCannonSecondary", null), 1, data.GetSpecialAbilityAttribute(this.m_upgradeLevel), 500);
            }

            this.AddTombstoneIfNeeded();

            if (this.m_parent != null)
            {
                this.m_parent.RemoveChildren(this);
                this.m_parent = null;
            }

            base.DeathEvent();
        }

        public void RemoveChildren(LogicCharacter children)
        {
            if (this.m_childrens.Size() > 0)
            {
                int idx = this.m_childrens.IndexOf(children);

                if (idx != -1)
                {
                    this.m_childrens.Remove(idx);

                    if (this.m_childrens.Size() != 0)
                    {
                        int childTroopLost = this.GetCharacterData().GetChildTroopCount() - this.m_childrens.Size();
                        this.GetMovementComponent().GetMovementSystem()
                            .SetSpeed(this.GetCharacterData().GetSpeed() - this.GetCharacterData().GetSpeedDecreasePerChildTroopLost() * childTroopLost);
                    }
                    else
                    {
                        this.GetHitpointComponent().Kill();
                    }
                }
            }
        }

        public void AddTombstoneIfNeeded()
        {
            if (!this.m_ejected && this.m_level.GetTombStoneCount() < 40)
            {
                int tileX = this.GetTileX();
                int tileY = this.GetTileY();

                LogicTileMap tileMap = this.m_level.GetTileMap();
                LogicTile tile = tileMap.GetTile(tileX, tileY);

                if (!this.TileOkForTombstone(tile))
                {
                    int minDistance = 0;
                    int closestTileX = -1;
                    int closestTileY = -1;

                    for (int i = -1; i < 2; i++)
                    {
                        int offsetX = ((i + tileX) << 9) | 256;
                        int offsetY = 256 - (tileY << 9);

                        for (int j = -1; j < 2; j++, offsetY -= 512)
                        {
                            tile = tileMap.GetTile(tileX + i, tileY + j);

                            if (this.TileOkForTombstone(tile))
                            {
                                int distanceX = this.GetX() - offsetX;
                                int distanceY = this.GetY() + offsetY;
                                int distance = distanceX * distanceX + distanceY * distanceY;

                                if (minDistance == 0 || distance < minDistance)
                                {
                                    minDistance = distance;
                                    closestTileX = tileX + i;
                                    closestTileY = tileY + j;
                                }
                            }
                        }
                    }

                    if (minDistance == 0)
                    {
                        return;
                    }

                    tileX = closestTileX;
                    tileY = closestTileY;
                }

                LogicObstacleData tombstoneData = this.GetCharacterData().GetTombstone();

                if (tombstoneData != null)
                {
                    LogicObstacle tombstone = (LogicObstacle) LogicGameObjectFactory.CreateGameObject(tombstoneData, this.m_level, this.m_villageType);
                    tombstone.SetInitialPosition(tileX << 9, tileY << 9);
                    this.GetGameObjectManager().AddGameObject(tombstone, -1);
                }
            }
        }

        public void CheckSpawning(LogicCharacterData spawnCharacterData, int spawnCount, int spawnUpgradeLevel, int invulnerabilityTime)
        {
            LogicCharacterData data = this.GetCharacterData();

            if (spawnCharacterData == null)
            {
                spawnCharacterData = data.GetSecondaryTroop();

                if (spawnCharacterData == null)
                {
                    spawnCharacterData = data.GetAttackerItemData(this.m_upgradeLevel).GetSummonTroop();

                    if (spawnCharacterData == null)
                    {
                        return;
                    }
                }
            }

            if (spawnCharacterData.IsSecondaryTroop() || this.IsHero())
            {
                int totalSpawnCount = spawnCount;
                int upgLevel = this.m_upgradeLevel;

                if (upgLevel >= spawnCharacterData.GetUpgradeLevelCount())
                {
                    upgLevel = spawnCharacterData.GetUpgradeLevelCount() - 1;
                }

                if (this.IsHero())
                {
                    if (this.m_summonSpawnCount >= spawnCount)
                    {
                        return;
                    }

                    upgLevel = spawnUpgradeLevel;
                    totalSpawnCount = LogicMath.Max(0, LogicMath.Min(3, spawnCount - this.m_summonSpawnCount));
                }
                else
                {
                    if (data.GetSecondaryTroopCount(this.m_upgradeLevel) != 0)
                    {
                        totalSpawnCount = data.GetSecondaryTroopCount(this.m_upgradeLevel);
                    }
                    else if (spawnCount == 0)
                    {
                        totalSpawnCount = data.GetAttackerItemData(this.m_upgradeLevel).GetSummonTroopCount();

                        if (this.m_summonTroops.Size() + totalSpawnCount > data.GetAttackerItemData(this.m_upgradeLevel).GetSummonLimit())
                        {
                            totalSpawnCount = data.GetAttackerItemData(this.m_upgradeLevel).GetSummonLimit() - this.m_summonTroops.Size();
                        }
                    }
                }

                if (totalSpawnCount > 0)
                {
                    LogicVector2 position = new LogicVector2();
                    LogicRandom random = new LogicRandom(this.m_globalId);

                    int team = this.GetHitpointComponent().GetTeam();
                    bool randomizeSecSpawnDist = this.GetCharacterData().GetRandomizeSecSpawnDist();

                    for (int i = 0, j = 0, k = 0; i < totalSpawnCount; i++, j += 360, k += 100)
                    {
                        int seed = j / totalSpawnCount;

                        if (this.IsHero())
                        {
                            seed = 360 * (i + this.m_summonSpawnCount) / LogicMath.Max(1, LogicMath.Min(6, spawnCount));
                        }

                        int rnd = 59 * this.m_globalId % 360 + seed;

                        if (spawnCharacterData.IsFlying())
                        {
                            LogicCharacterData parentData = this.GetCharacterData();

                            position.Set(this.GetX() + LogicMath.GetRotatedX(parentData.GetSecondarySpawnOffset(), 0, rnd),
                                         this.GetY() + LogicMath.GetRotatedY(parentData.GetSecondarySpawnOffset(), 0, rnd));
                        }
                        else if (spawnCharacterData.GetSpeed() == 0)
                        {
                            position.Set(this.GetX(), this.GetY());
                        }
                        else
                        {
                            if (!this.m_level.GetTileMap().GetNearestPassablePosition(this.GetX(), this.GetY(), position, 1536))
                            {
                                continue;
                            }
                        }

                        LogicCharacter spawnGameObject = (LogicCharacter) LogicGameObjectFactory.CreateGameObject(spawnCharacterData, this.m_level, this.m_villageType);

                        if (this.GetCharacterData().GetAttackerItemData(this.m_upgradeLevel).GetSummonTroop() != null || this.IsHero())
                        {
                            this.m_summonTroops.Add(spawnGameObject);
                        }

                        spawnGameObject.GetHitpointComponent().SetTeam(team);
                        spawnGameObject.SetUpgradeLevel(upgLevel);

                        spawnGameObject.SetInitialPosition(position.m_x, position.m_y);

                        if (this.m_duplicate)
                        {
                            spawnGameObject.m_duplicateLifeTime = this.m_duplicateLifeTime;
                            spawnGameObject.m_duplicate = true;
                        }

                        if (!this.IsHero())
                        {
                            spawnGameObject.m_summoner = (LogicCharacterData) this.m_data;
                        }

                        if (invulnerabilityTime > 0)
                        {
                            spawnGameObject.GetHitpointComponent().SetInvulnerabilityTime(invulnerabilityTime);
                        }

                        int secondarySpawnDistance = this.IsHero() ? 768 : this.GetCharacterData().GetSecondarySpawnDistance();

                        if (secondarySpawnDistance > 0)
                        {
                            if (randomizeSecSpawnDist)
                            {
                                secondarySpawnDistance = (int) (random.Rand(secondarySpawnDistance) + ((uint) secondarySpawnDistance >> 1));
                            }

                            position.Set(LogicMath.Cos(rnd, secondarySpawnDistance),
                                         LogicMath.Sin(rnd, secondarySpawnDistance));

                            int pushBackSpeed = spawnGameObject.GetCharacterData().GetPushbackSpeed();

                            if (pushBackSpeed <= 0)
                            {
                                pushBackSpeed = 1;
                            }

                            int pushBackTime = 2 * secondarySpawnDistance / (3 * pushBackSpeed);

                            if (this.GetHitpointComponent().GetHitpoints() > 0)
                            {
                                if (this.GetAttackerItemData().GetSummonTroop() != null)
                                {
                                    spawnGameObject.SetSpawnTime(pushBackTime);
                                }
                                else if (this.IsHero())
                                {
                                    spawnGameObject.SetSpawnTime(pushBackTime + k);
                                }
                            }

                            spawnGameObject.GetMovementComponent().GetMovementSystem().PushTrap(position, pushBackTime, 0, false, false);
                        }

                        if (team == 1 || spawnGameObject.GetCharacterData().IsJumper())
                        {
                            spawnGameObject.GetMovementComponent().EnableJump(3600000);
                            spawnGameObject.GetCombatComponent().RefreshTarget(true);
                        }

                        if (team == 1)
                        {
                            if (LogicDataTables.GetGlobals().AllianceTroopsPatrol())
                            {
                                spawnGameObject.GetCombatComponent().SetSearchRadius(LogicDataTables.GetGlobals().GetClanCastleRadius() >> 9);

                                if (this.GetMovementComponent().GetBaseBuilding() != null)
                                {
                                    spawnGameObject.GetMovementComponent().SetBaseBuilding(this.GetMovementComponent().GetBaseBuilding());
                                }
                            }
                        }

                        this.GetGameObjectManager().AddGameObject(spawnGameObject, -1);

                        if (this.IsHero())
                        {
                            ++this.m_summonSpawnCount;
                        }
                    }

                    position.Destruct();
                }
            }
            else
            {
                Debugger.Warning("checkSpawning: trying to spawn normal troops!");
            }
        }

        public override void SpawnEvent(LogicCharacterData data, int count, int upgLevel)
        {
            this.CheckSpawning(data, count, upgLevel, 0);
        }

        public override void SubTick()
        {
            base.SubTick();

            LogicCombatComponent combatComponent = this.GetCombatComponent();
            LogicMovementComponent movementComponent = this.GetMovementComponent();

            if (combatComponent != null)
            {
                combatComponent.SubTick();
            }

            if (movementComponent != null)
            {
                movementComponent.SubTick();

                LogicMovementSystem movementSystem = movementComponent.GetMovementSystem();
                LogicVector2 movementPosition = movementSystem.GetPosition();

                this.SetPositionXY(movementPosition.m_x, movementPosition.m_y);
            }
            else if (this.m_troopChild)
            {
                LogicVector2 tmp = new LogicVector2(this.m_troopChildX, this.m_troopChildY);

                tmp.Rotate(this.m_parent.GetDirection());

                LogicMovementSystem movementSystem = this.m_parent.GetMovementComponent().GetMovementSystem();
                LogicVector2 position = movementSystem.GetPosition();

                this.SetPositionXY(tmp.m_x + position.m_x, tmp.m_y + position.m_y);
            }

            if (this.m_childrens != null)
            {
                for (int i = 0; i < this.m_childrens.Size(); i++)
                {
                    this.m_childrens[i].SubTick();
                }
            }

            int distanceX = this.GetX() + (this.GetWidthInTiles() << 8);
            int distanceY = this.GetY() + (this.GetHeightInTiles() << 8);

            if (this.m_auraSpell != null)
            {
                this.m_auraSpell.SetPositionXY(distanceX, distanceY);
            }

            if (this.m_abilitySpell != null)
            {
                this.m_abilitySpell.SetPositionXY(distanceX, distanceY);
            }

            if (this.m_retributionSpell != null)
            {
                this.m_retributionSpell.SetPositionXY(distanceX, distanceY);
            }
        }

        public override void Tick()
        {
            base.Tick();

            LogicCharacterData data = this.GetCharacterData();

            if (!this.IsAlive())
            {
                if (!this.IsHero())
                {
                    int dieDamageDelay = this.GetCharacterData().GetDieDamageDelay();
                    int prevDieTime = this.m_dieTime;

                    this.m_dieTime += 64;

                    if (dieDamageDelay >= prevDieTime && dieDamageDelay < this.m_dieTime && (!this.m_duplicate || this.m_duplicateLifeTime >= 0))
                    {
                        this.CheckDieDamage(data.GetDieDamage(this.m_upgradeLevel), data.GetDieDamageRadius());
                        this.m_level.UpdateBattleStatus();
                    }
                }

                this.m_spawnTime = 0;
                this.m_spawnIdleTime = 0;

                if (this.m_auraSpell != null)
                {
                    this.GetGameObjectManager().RemoveGameObject(this.m_auraSpell);
                    this.m_auraSpell = null;
                }

                if (this.m_abilitySpell != null)
                {
                    this.GetGameObjectManager().RemoveGameObject(this.m_abilitySpell);
                    this.m_abilitySpell = null;
                }

                if (this.m_retributionSpell != null)
                {
                    this.GetGameObjectManager().RemoveGameObject(this.m_retributionSpell);
                    this.m_retributionSpell = null;
                }
            }
            else
            {
                if (data.GetLoseHpPerTick() > 0)
                {
                    this.m_loseHpTime += 64;

                    if (this.m_loseHpTime > data.GetLoseHpInterval())
                    {
                        LogicHitpointComponent hitpointComponent = this.GetHitpointComponent();

                        if (hitpointComponent != null)
                        {
                            hitpointComponent.CauseDamage(100 * data.GetLoseHpPerTick(), this.m_globalId, this);
                            // Listener.
                        }

                        this.m_loseHpTime = 0;
                    }
                }

                if (data.GetAttackCount(this.m_upgradeLevel) > 0 && this.GetCombatComponent() != null && this.GetHitpointComponent() != null &&
                    this.GetCombatComponent().GetHitCount() >= data.GetAttackCount(this.m_upgradeLevel))
                {
                    this.GetHitpointComponent().Kill();
                }

                this.m_spawnTime = LogicMath.Max(this.m_spawnTime - 64, 0);
                this.m_spawnIdleTime = LogicMath.Max(this.m_spawnIdleTime - 64, 0);

                if (this.m_spawnTime == 0 && this.m_hasSpawnDelay)
                {
                    this.m_spawnIdleTime = LogicMath.Max(10, data.GetSpawnIdle());
                    this.m_hasSpawnDelay = false;
                }

                if (data.GetBoostedIfAlone() || data.GetSpecialAbilityType() == LogicCharacterData.SPECIAL_ABILITY_TYPE_RAGE_ALONE && this.GetSpecialAbilityAvailable())
                {
                    if (++this.m_rageAloneTime >= 5)
                    {
                        this.m_level.AreaBoostAlone(this, 6);
                        this.m_rageAloneTime = 0;
                    }
                }

                if (this.IsHero())
                {
                    LogicHeroData heroData = (LogicHeroData) data;

                    if (this.m_abilityTime > 0)
                    {
                        if (heroData.GetAbilityAttackCount(this.m_upgradeLevel) > 0 && this.GetCombatComponent().GetHitCount() >= this.m_abilityAttackCount)
                        {
                            Debugger.HudPrint("Hero ability: No more attacks left!");

                            this.m_abilityTime = 0;
                            this.m_abilityTriggerTime = 0;
                            this.m_activationTime = 0;
                        }
                        else
                        {
                            if (++this.m_abilityTriggerTime >= 5)
                            {
                                this.m_abilityTime -= 1;
                                this.m_abilityTriggerTime = 0;

                                this.m_level.AreaAbilityBoost(this, 5);
                            }
                        }
                    }

                    if (this.m_abilityCooldown > 0)
                    {
                        this.m_abilityCooldown -= 1;
                    }

                    if (this.m_abilitySpell != null && this.m_abilitySpell.GetHitsCompleted())
                    {
                        this.GetGameObjectManager().RemoveGameObject(this.m_abilitySpell);
                        this.m_abilitySpell = null;
                    }
                }

                if (this.m_auraSpell == null || this.m_auraSpell.GetHitsCompleted())
                {
                    if (this.m_auraSpell != null)
                    {
                        this.GetGameObjectManager().RemoveGameObject(this.m_auraSpell);
                        this.m_auraSpell = null;
                    }

                    if (data.GetAuraSpell(this.m_upgradeLevel) != null)
                    {
                        LogicHitpointComponent hitpointComponent = this.GetHitpointComponent();

                        if (hitpointComponent != null && hitpointComponent.GetTeam() == 0)
                        {
                            this.m_auraSpell = (LogicSpell) LogicGameObjectFactory.CreateGameObject(data.GetAuraSpell(this.m_upgradeLevel), this.m_level, this.m_villageType);
                            this.m_auraSpell.SetUpgradeLevel(data.GetAuraSpellLevel(this.m_upgradeLevel));
                            this.m_auraSpell.SetInitialPosition(this.GetX(), this.GetY());
                            this.m_auraSpell.AllowDestruction(false);
                            this.m_auraSpell.SetTeam(hitpointComponent.GetTeam());

                            this.GetGameObjectManager().AddGameObject(this.m_auraSpell, -1);
                        }
                    }
                }

                if (!this.m_retributionSpellCreated)
                {
                    if (data.GetRetributionSpell(this.m_upgradeLevel) != null)
                    {
                        LogicHitpointComponent hitpointComponent = this.GetHitpointComponent();

                        if (hitpointComponent.GetHitpoints() <=
                            hitpointComponent.GetMaxHitpoints() * data.GetRetributionSpellTriggerHealth(this.m_upgradeLevel) / 100)
                        {
                            this.m_retributionSpellCreated = true;
                            this.m_retributionSpell =
                                (LogicSpell) LogicGameObjectFactory.CreateGameObject(data.GetRetributionSpell(this.m_upgradeLevel), this.m_level, this.m_villageType);
                            this.m_retributionSpell.SetUpgradeLevel(data.GetRetributionSpellLevel(this.m_upgradeLevel));
                            this.m_retributionSpell.SetPositionXY(this.GetX(), this.GetY());
                            this.m_retributionSpell.AllowDestruction(false);
                            this.m_retributionSpell.SetTeam(hitpointComponent.GetTeam());

                            this.GetGameObjectManager().AddGameObject(this.m_retributionSpell, -1);
                        }
                    }
                }

                if (this.m_activationTimeState == 2)
                {
                    this.m_activationTime -= 64;

                    if (this.m_activationTime < 0)
                    {
                        this.m_activationTimeState = 0;
                        this.m_activationTime = 0;
                    }
                }
                else if (this.m_activationTimeState == 1)
                {
                    this.m_activationTime -= 64;

                    if (this.m_activationTime < 0)
                    {
                        this.m_activationTimeState = 2;
                        this.m_activationTime = ((LogicHeroData) this.m_data).GetActiveDuration();
                    }
                }
            }

            this.CheckSummons();

            if (this.IsAlive())
            {
                if (data.GetAutoMergeDistance() > 0)
                {
                    this.m_autoMergeTime = LogicMath.Max(this.m_autoMergeTime - 64, 0);
                }

                if (data.GetInvisibilityRadius() > 0)
                {
                    this.m_level.AreaInvisibility(this.GetMidX(), this.GetMidY(), data.GetInvisibilityRadius(), 4, this.GetHitpointComponent().GetTeam());
                }

                if (data.GetHealthReductionPerSecond() > 0)
                {
                    this.GetHitpointComponent().CauseDamage(100 * data.GetHealthReductionPerSecond() / 15, 0, this);
                }
            }

            if (this.m_duplicate)
            {
                if (this.m_duplicateLifeTime-- <= 0)
                {
                    LogicHitpointComponent hitpointComponent = this.GetHitpointComponent();

                    if (hitpointComponent != null)
                    {
                        hitpointComponent.SetHitpoints(0);
                        this.m_level.UpdateBattleStatus();
                    }
                }
            }
        }

        public void CheckDieDamage(int damage, int radius)
        {
            LogicCharacterData data = (LogicCharacterData) this.m_data;

            if (data.GetSpecialAbilityType() == LogicCharacterData.SPECIAL_ABILITY_TYPE_DIE_DAMAGE)
            {
                if (data.GetSpecialAbilityLevel(this.m_upgradeLevel) <= 0)
                {
                    return;
                }
            }

            if (damage > 0 && radius > 0)
            {
                LogicHitpointComponent hitpointComponent = this.GetHitpointComponent();

                if (hitpointComponent != null)
                {
                    this.m_level.AreaDamage(0, this.GetX(), this.GetY(), radius, damage, null, 0, null, hitpointComponent.GetTeam(), null, 1, 0, 0, true, false, 100,
                                            0, this, 100, 0);
                }
            }
        }

        public void CheckSummons()
        {
            for (int i = 0; i < this.m_summonTroops.Size(); i++)
            {
                LogicCharacter character = this.m_summonTroops[i];

                if (!character.IsAlive())
                {
                    this.m_summonTroops.Remove(i--);
                }
                else
                {
                    if (character.m_spawnTime > 0 && !this.IsAlive())
                    {
                        this.m_summonTroops.Remove(i--);
                        this.m_hasSpawnDelay = false;

                        LogicHitpointComponent hitpointComponent = character.GetHitpointComponent();

                        if (hitpointComponent != null)
                        {
                            hitpointComponent.SetHitpoints(0);
                        }

                        this.m_level.UpdateBattleStatus();
                    }
                }
            }
        }

        public void UpdateAutoMerge()
        {
            if (this.m_autoMergeTime > 0)
            {
                int autoMergeGroupSize = this.GetCharacterData().GetAutoMergeGroupSize();
                int autoMergeDistance = this.GetCharacterData().GetAutoMergeDistance();

                if (autoMergeGroupSize > 0)
                {
                    LogicArrayList<LogicGameObject> characters = this.GetGameObjectManager().GetGameObjects(LogicGameObjectType.CHARACTER);
                    LogicCharacter closestCharacter = null;

                    for (int i = 0; i < characters.Size(); i++)
                    {
                        LogicCharacter character = (LogicCharacter) characters[i];

                        if (character != this)
                        {
                            if (character.GetData() == this.GetData())
                            {
                                if (this.m_autoMergeSize == 0 && character.m_autoMergeSize >= autoMergeGroupSize)
                                {
                                    if (character.GetHitpointComponent().GetTeam() == this.GetHitpointComponent().GetTeam() && character.IsAlive())
                                    {
                                        if (character.m_autoMergeTime > 0)
                                        {
                                            int distanceSquared = this.GetPosition().GetDistanceSquared(character.GetPosition());

                                            if (distanceSquared <= autoMergeDistance * autoMergeDistance)
                                            {
                                                closestCharacter = character;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (closestCharacter != null)
                    {
                        closestCharacter.m_autoMergeSize += 1;
                        closestCharacter.GetCombatComponent().SetMergeDamage(90 * closestCharacter.m_autoMergeSize);
                        closestCharacter.GetHitpointComponent()
                                        .SetMaxHitpoints(closestCharacter.GetCharacterData().GetHitpoints(this.m_upgradeLevel) * (closestCharacter.m_autoMergeSize + 1));
                        closestCharacter.GetHitpointComponent()
                                        .SetHitpoints(closestCharacter.GetCharacterData().GetHitpoints(this.m_upgradeLevel) * (closestCharacter.m_autoMergeSize + 1));

                        this.GetGameObjectManager().RemoveGameObject(this);
                    }
                }
            }
        }

        public override bool IsStaticObject()
        {
            return false;
        }

        public override int GetWidthInTiles()
        {
            return 1;
        }

        public override int GetHeightInTiles()
        {
            return 1;
        }

        public override int GetMidX()
        {
            return this.GetX();
        }

        public override int GetMidY()
        {
            return this.GetY();
        }

        public override int GetDirection()
        {
            LogicMovementComponent movementComponent = this.GetMovementComponent();

            if (movementComponent != null)
            {
                return movementComponent.GetMovementSystem().GetDirection();
            }

            if (this.m_troopChild && this.m_parent != null)
            {
                return this.m_parent.GetDirection();
            }

            return 0;
        }
    }
}