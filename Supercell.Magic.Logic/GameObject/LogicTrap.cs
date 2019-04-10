namespace Supercell.Magic.Logic.GameObject
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Time;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicTrap : LogicGameObject
    {
        private LogicTimer m_constructionTimer;

        private bool m_upgrading;
        private bool m_disarmed;

        private int m_upgLevel;
        private int m_numSpawns;
        private int m_fadeTime;
        private int m_spawnInitDelay;
        private int m_hitTime; // 132
        private int m_hitCount; // 136
        private int m_actionTime; // 128

        private readonly bool[] m_useAirMode;
        private readonly bool[] m_draftUseAirMode;

        private readonly int[] m_direction;
        private readonly int[] m_draftDirection;

        public LogicTrap(LogicGameObjectData data, LogicLevel level, int villageType) : base(data, level, villageType)
        {
            this.m_fadeTime = -1;
            this.m_hitTime = -1;

            this.m_useAirMode = new bool[8];
            this.m_draftUseAirMode = new bool[8];
            this.m_direction = new int[8];
            this.m_draftDirection = new int[8];

            LogicTrapData trapData = (LogicTrapData) data;

            this.AddComponent(new LogicTriggerComponent(this, trapData.GetTriggerRadius(), trapData.GetAirTrigger(), trapData.GetGroundTrigger(), trapData.GetHealerTrigger(),
                                                        trapData.GetMinTriggerHousingLimit()));
            this.AddComponent(new LogicLayoutComponent(this));

            this.m_numSpawns = trapData.GetNumSpawns(0);
            this.m_spawnInitDelay = trapData.GetSpawnInitialDelayMS() / 64;
        }

        public LogicTrapData GetTrapData()
        {
            return (LogicTrapData) this.m_data;
        }

        public override void Destruct()
        {
            base.Destruct();

            if (this.m_constructionTimer != null)
            {
                this.m_constructionTimer.Destruct();
                this.m_constructionTimer = null;
            }
        }

        public override void GetChecksum(ChecksumHelper checksum, bool includeGameObjects)
        {
            checksum.StartObject("LogicTrap");

            base.GetChecksum(checksum, includeGameObjects);

            if (this.m_constructionTimer != null)
            {
                checksum.WriteValue("remainingMS", this.m_constructionTimer.GetRemainingMS(this.m_level.GetLogicTime()));
            }

            checksum.EndObject();
        }

        public override bool ShouldDestruct()
        {
            if (this.m_fadeTime >= 1000)
            {
                int trapCount = this.GetGameObjectManager().GetGameObjectCountByData(this.GetData());
                int townHallLevel = this.m_level.GetTownHallLevel(this.m_level.GetVillageType());

                LogicTownhallLevelData townhallLevelData = LogicDataTables.GetTownHallLevel(townHallLevel);

                return townhallLevelData.GetUnlockedTrapCount(this.GetTrapData()) < trapCount;
            }

            return false;
        }

        public void FinishConstruction(bool ignoreState)
        {
            int state = this.m_level.GetState();

            if (state == 1 || !LogicDataTables.GetGlobals().CompleteConstructionOnlyHome() && ignoreState)
            {
                LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();

                if (homeOwnerAvatar != null && homeOwnerAvatar.IsClientAvatar())
                {
                    LogicTrapData data = this.GetTrapData();

                    if (this.m_constructionTimer != null)
                    {
                        this.m_constructionTimer.Destruct();
                        this.m_constructionTimer = null;
                    }

                    this.m_level.GetWorkerManagerAt(this.m_data.GetVillageType()).DeallocateWorker(this);

                    if (this.m_upgLevel != 0 || this.m_upgrading)
                    {
                        if (this.m_upgLevel >= data.GetUpgradeLevelCount() - 1)
                        {
                            Debugger.Warning("LogicTrap - Trying to upgrade to level that doesn't exist! - " + data.GetName());
                            this.m_upgLevel = data.GetUpgradeLevelCount() - 1;
                        }
                        else
                        {
                            this.m_upgLevel += 1;
                        }
                    }

                    if (!ignoreState && !this.m_disarmed)
                    {
                        if (this.GetListener() != null)
                        {
                            // Listener.
                        }
                    }

                    this.XpGainHelper(LogicGamePlayUtil.TimeToExp(data.GetBuildTime(this.m_upgLevel)), homeOwnerAvatar, ignoreState);

                    if (this.m_disarmed)
                    {
                        // Listener.
                    }

                    this.m_fadeTime = 0;
                    this.m_disarmed = false;
                    this.m_upgrading = false;

                    if (this.m_listener != null)
                    {
                        this.m_listener.RefreshState();
                    }

                    if (state == 1)
                    {
                        this.m_level.GetAchievementManager().RefreshStatus();
                    }
                }
                else
                {
                    Debugger.Warning("LogicTrap::finishCostruction failed - Avatar is null or not client avatar");
                }
            }
        }

        public bool IsFadingOut()
        {
            return this.m_fadeTime > 0;
        }

        public bool IsDisarmed()
        {
            return this.m_disarmed;
        }

        public void RepairTrap()
        {
            this.m_disarmed = false;
            this.m_fadeTime = 0;
        }

        public void DisarmTrap()
        {
            this.m_disarmed = true;
            this.m_fadeTime = 0;
        }

        public void SetUpgradeLevel(int upgLevel)
        {
            LogicTrapData data = this.GetTrapData();

            this.m_upgLevel = LogicMath.Clamp(upgLevel, 0, data.GetUpgradeLevelCount() - 1);
            this.m_numSpawns = data.GetNumSpawns(this.m_upgLevel);
        }

        public void CreateProjectile(LogicProjectileData data)
        {
            LogicTrapData trapData = this.GetTrapData();

            LogicVector2 position = new LogicVector2();
            LogicArrayList<LogicGameObject> characters = this.GetGameObjectManager().GetGameObjects(LogicGameObjectType.CHARACTER);

            LogicGameObject closestGameObject = null;

            for (int i = 0, minDistance = 0; i < characters.Size(); i++)
            {
                LogicCharacter character = (LogicCharacter) characters[i];
                LogicHitpointComponent hitpointComponent = character.GetHitpointComponent();

                if (hitpointComponent != null && hitpointComponent.GetTeam() == 0)
                {
                    if (character.IsFlying() && character.IsAlive())
                    {
                        int housingSpace = character.GetCharacterData().GetHousingSpace();

                        if (housingSpace >= trapData.GetMinTriggerHousingLimit() && character.GetChildTroops() == null)
                        {
                            if (trapData.GetHealerTrigger() || character.GetCombatComponent() == null || !character.GetCombatComponent().IsHealer())
                            {
                                position.m_x = character.GetPosition().m_x - this.GetMidX();
                                position.m_y = character.GetPosition().m_y - this.GetMidY();

                                int lengthSquared = position.GetLengthSquared();

                                if (minDistance == 0 || lengthSquared < minDistance)
                                {
                                    minDistance = lengthSquared;
                                    closestGameObject = character;
                                }
                            }
                        }
                    }
                }
            }

            position.Destruct();

            if (closestGameObject != null)
            {
                LogicProjectile projectile = (LogicProjectile) LogicGameObjectFactory.CreateGameObject(data, this.m_level, this.m_villageType);

                projectile.SetInitialPosition(null, this.GetMidX(), this.GetMidY());
                projectile.SetTarget(this.GetMidX(), this.GetMidY(), 0, closestGameObject, data.GetRandomHitPosition());
                projectile.SetDamage(trapData.GetDamage(this.m_upgLevel));
                projectile.SetDamageRadius(trapData.GetDamageRadius(this.m_upgLevel));
                projectile.SetPushBack(trapData.GetPushback(), !trapData.GetDoNotScalePushByDamage());
                projectile.SetMyTeam(1);
                projectile.SetHitEffect(trapData.GetDamageEffect(), null);

                this.GetGameObjectManager().AddGameObject(projectile, -1);
            }
        }

        public void EjectCharacters()
        {
            LogicArrayList<LogicComponent> components = this.GetComponentManager().GetComponents(LogicComponentType.MOVEMENT);

            int ejectHousingSpace = this.GetTrapData().GetEjectHousingLimit(this.m_upgLevel);
            int radius = this.GetTrapData().GetTriggerRadius();

            for (int i = 0; i < components.Size(); i++)
            {
                LogicMovementComponent movementComponent = (LogicMovementComponent) components[i];
                LogicGameObject parent = movementComponent.GetParent();

                if (parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                {
                    LogicCharacter character = (LogicCharacter) parent;

                    if (character.GetHitpointComponent() != null && character.GetHitpointComponent().GetTeam() == 0 &&
                        character.GetCharacterData().GetHousingSpace() <= ejectHousingSpace)
                    {
                        int distanceX = character.GetX() - this.GetMidX();
                        int distanceY = character.GetY() - this.GetMidY();

                        if (LogicMath.Abs(distanceX) <= radius &&
                            LogicMath.Abs(distanceY) <= radius)
                        {
                            if (character.GetCombatComponent() == null || character.GetCombatComponent().GetUndergroundTime() <= 0)
                            {
                                int distanceSquared = distanceX * distanceX + distanceY * distanceY;

                                if (distanceSquared < (uint) (radius * radius))
                                {
                                    character.Eject(null);
                                    ejectHousingSpace -= character.GetCharacterData().GetHousingSpace();
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ThrowCharacters()
        {
            LogicArrayList<LogicComponent> components = this.GetComponentManager().GetComponents(LogicComponentType.MOVEMENT);

            int ejectHousingSpace = this.GetTrapData().GetEjectHousingLimit(this.m_upgLevel);
            int radius = this.GetTrapData().GetTriggerRadius();

            for (int i = 0; i < components.Size(); i++)
            {
                LogicMovementComponent movementComponent = (LogicMovementComponent) components[i];
                LogicGameObject parent = movementComponent.GetParent();

                if (parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                {
                    LogicCharacter character = (LogicCharacter) parent;

                    if (character.GetHitpointComponent() != null && character.GetHitpointComponent().GetTeam() == 0 &&
                        character.GetCharacterData().GetHousingSpace() <= ejectHousingSpace)
                    {
                        int distanceX = character.GetX() - this.GetMidX();
                        int distanceY = character.GetY() - this.GetMidY();

                        if (LogicMath.Abs(distanceX) <= radius &&
                            LogicMath.Abs(distanceY) <= radius)
                        {
                            if (character.GetCombatComponent() == null || character.GetCombatComponent().GetUndergroundTime() <= 0)
                            {
                                int distanceSquared = distanceX * distanceX + distanceY * distanceY;

                                if (distanceSquared < (uint) (radius * radius))
                                {
                                    int activeLayout = this.m_level.GetActiveLayout();
                                    int direction = activeLayout <= 7 ? this.m_direction[activeLayout] : 0;
                                    int pushBackX = 0;
                                    int pushBackY = 0;

                                    switch (direction)
                                    {
                                        case 0:
                                            pushBackX = 256;
                                            break;
                                        case 1:
                                            pushBackY = 256;
                                            break;
                                        case 2:
                                            pushBackX = -256;
                                            break;
                                        case 3:
                                            pushBackY = -256;
                                            break;
                                    }

                                    this.m_level.AreaPushBack(this.GetMidX(), this.GetMidY(), 600, 1000, 1, 1, pushBackX, pushBackY, this.GetTrapData().GetThrowDistance(),
                                                              ejectHousingSpace);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void FastForwardTime(int secs)
        {
            if (this.m_constructionTimer != null)
            {
                if (this.m_constructionTimer.GetEndTimestamp() == -1)
                {
                    int remainingTime = this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime());

                    if (remainingTime > secs)
                    {
                        base.FastForwardTime(secs);
                        this.m_constructionTimer.StartTimer(remainingTime - secs, this.m_level.GetLogicTime(), false, -1);
                    }
                    else
                    {
                        if (LogicDataTables.GetGlobals().CompleteConstructionOnlyHome())
                        {
                            base.FastForwardTime(secs);
                            this.m_constructionTimer.StartTimer(0, this.m_level.GetLogicTime(), false, -1);
                        }
                        else
                        {
                            base.FastForwardTime(remainingTime);
                            this.FinishConstruction(true);
                            base.FastForwardTime(secs - remainingTime);
                        }

                        return;
                    }
                }
                else
                {
                    this.m_constructionTimer.AdjustEndSubtick(this.m_level);

                    if (this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime()) == 0)
                    {
                        if (LogicDataTables.GetGlobals().CompleteConstructionOnlyHome())
                        {
                            base.FastForwardTime(secs);
                            this.m_constructionTimer.StartTimer(0, this.m_level.GetLogicTime(), false, -1);
                        }
                        else
                        {
                            base.FastForwardTime(0);
                            this.FinishConstruction(true);
                            base.FastForwardTime(secs);
                        }

                        return;
                    }

                    base.FastForwardTime(secs);
                }

                int maxClockTowerFastForward = this.m_level.GetUpdatedClockTowerBoostTime();

                if (maxClockTowerFastForward > 0 && !this.m_level.IsClockTowerBoostPaused())
                {
                    if (this.m_data.GetVillageType() == 1)
                    {
                        this.m_constructionTimer.SetFastForward(this.m_constructionTimer.GetFastForward() +
                                                                60 * LogicMath.Min(secs, maxClockTowerFastForward) *
                                                                (LogicDataTables.GetGlobals().GetClockTowerBoostMultiplier() - 1));
                    }
                }
            }
            else
            {
                base.FastForwardTime(secs);
            }
        }

        public override bool IsPassable()
        {
            return true;
        }

        public override void Tick()
        {
            base.Tick();

            if (this.m_constructionTimer != null)
            {
                if (this.m_level.GetRemainingClockTowerBoostTime() > 0 &&
                    this.m_data.GetVillageType() == 1)
                {
                    this.m_constructionTimer.SetFastForward(this.m_constructionTimer.GetFastForward() + 4 * LogicDataTables.GetGlobals().GetClockTowerBoostMultiplier() - 4);
                }

                if (this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime()) <= 0)
                {
                    this.FinishConstruction(false);
                }
            }

            if (this.m_disarmed)
            {
                if (this.m_fadeTime >= 0)
                {
                    this.m_fadeTime = LogicMath.Min(this.m_fadeTime + 64, 1000);
                }
            }

            LogicTriggerComponent triggerComponent = this.GetTriggerComponent();

            if (triggerComponent.IsTriggered() && !this.m_disarmed && !this.m_upgrading)
            {
                LogicTrapData data = this.GetTrapData();

                if (this.m_numSpawns > 0)
                {
                    if (this.m_spawnInitDelay != 0)
                    {
                        this.m_spawnInitDelay -= 1;
                    }
                    else
                    {
                        this.SpawnUnit(1);
                        this.m_numSpawns -= 1;
                        this.m_spawnInitDelay = this.GetTrapData().GetTimeBetweenSpawnsMS() / 64;
                    }
                }

                if (this.m_actionTime >= 0)
                {
                    this.m_actionTime += 64;
                }

                if (this.m_hitTime >= 0)
                {
                    this.m_hitTime += 64;
                }

                if (this.m_actionTime > data.GetActionFrame())
                {
                    this.m_hitTime = data.GetHitDelayMS();
                    this.m_actionTime = -1;
                }
                else if (this.m_hitTime > data.GetHitDelayMS())
                {
                    if (data.GetSpell() != null)
                    {
                        LogicSpell spell = (LogicSpell) LogicGameObjectFactory.CreateGameObject(data.GetSpell(), this.m_level, this.m_villageType);

                        spell.SetUpgradeLevel(0);
                        spell.SetInitialPosition(this.GetMidX(), this.GetMidY());
                        spell.SetTeam(1);

                        this.GetGameObjectManager().AddGameObject(spell, -1);
                    }
                    else if (data.GetProjectile(this.m_upgLevel) != null)
                    {
                        this.CreateProjectile(data.GetProjectile(this.m_upgLevel));
                    }
                    else if (data.GetDamageMod() != 0)
                    {
                        this.m_level.AreaBoost(this.GetMidX(), this.GetMidY(), data.GetDamageRadius(this.m_upgLevel), -data.GetSpeedMod(), -data.GetSpeedMod(), data.GetDamageMod(),
                                               0, data.GetDurationMS() / 16, 0, false);
                    }
                    else if (data.GetEjectVictims())
                    {
                        if (data.GetThrowDistance() <= 0)
                        {
                            this.EjectCharacters();
                        }
                        else
                        {
                            this.ThrowCharacters();
                        }
                    }
                    else
                    {
                        bool defaultMode = true;

                        if (data.GetSpawnedCharAir() != null && data.GetSpawnedCharGround() != null || data.HasAlternativeMode())
                        {
                            int activeLayout = this.m_level.GetActiveLayout();

                            if (activeLayout <= 7)
                            {
                                defaultMode = this.m_useAirMode[activeLayout] ^ true;
                            }
                        }

                        this.m_level.AreaDamage(0, this.GetMidX(), this.GetMidY(), data.GetDamageRadius(this.m_upgLevel), data.GetDamage(this.m_upgLevel),
                                                data.GetPreferredTarget(),
                                                data.GetPreferredTargetDamageMod(), data.GetDamageEffect(), 1, null, defaultMode ? 1 : 0, 0, 100, true, false, 100, 0, this, 100,
                                                0);
                    }

                    this.m_hitTime = 0;
                    this.m_hitCount += 1;

                    if (this.m_hitCount >= data.GetHitCount() && this.m_numSpawns == 0)
                    {
                        this.m_fadeTime = 1;
                        this.m_hitTime = -1;
                        this.m_disarmed = true;
                        this.m_numSpawns = data.GetNumSpawns(this.m_upgLevel);
                        this.m_spawnInitDelay = data.GetSpawnInitialDelayMS() / 64;
                    }
                }
            }
        }

        public override void Save(LogicJSONObject jsonObject, int villageType)
        {
            if (this.m_upgLevel != 0 || this.m_constructionTimer == null || this.m_upgrading)
            {
                jsonObject.Put("lvl", new LogicJSONNumber(this.m_upgLevel));
            }
            else
            {
                jsonObject.Put("lvl", new LogicJSONNumber(-1));
            }

            if (this.m_constructionTimer != null)
            {
                jsonObject.Put("const_t", new LogicJSONNumber(this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime())));

                if (this.m_constructionTimer.GetEndTimestamp() != -1)
                {
                    jsonObject.Put("const_t_end", new LogicJSONNumber(this.m_constructionTimer.GetEndTimestamp()));
                }

                if (this.m_constructionTimer.GetFastForward() != -1)
                {
                    jsonObject.Put("con_ff", new LogicJSONNumber(this.m_constructionTimer.GetFastForward()));
                }
            }

            if (this.m_disarmed && this.GetTrapData().GetVillageType() != 1)
            {
                jsonObject.Put("needs_repair", new LogicJSONBoolean(true));
            }

            LogicTrapData data = this.GetTrapData();

            if (data.HasAlternativeMode() || data.GetSpawnedCharAir() != null && data.GetSpawnedCharGround() != null)
            {
                LogicLayoutComponent layoutComponent = (LogicLayoutComponent) this.GetComponent(LogicComponentType.LAYOUT);

                if (layoutComponent != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        jsonObject.Put(layoutComponent.GetLayoutVariableNameAirMode(i, false), new LogicJSONBoolean(this.m_useAirMode[i]));
                        jsonObject.Put(layoutComponent.GetLayoutVariableNameAirMode(i, true), new LogicJSONBoolean(this.m_draftUseAirMode[i]));
                    }
                }
            }

            if (data.GetDirectionCount() > 0)
            {
                LogicLayoutComponent layoutComponent = (LogicLayoutComponent) this.GetComponent(LogicComponentType.LAYOUT);

                if (layoutComponent != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        jsonObject.Put(layoutComponent.GetLayoutVariableNameTrapDirection(i, false), new LogicJSONNumber(this.m_direction[i]));
                        jsonObject.Put(layoutComponent.GetLayoutVariableNameTrapDirection(i, true), new LogicJSONNumber(this.m_draftDirection[i]));
                    }
                }
            }

            base.Save(jsonObject, villageType);
        }

        public override void SaveToSnapshot(LogicJSONObject jsonObject, int layoutId)
        {
            if (this.m_upgLevel != 0 || this.m_constructionTimer == null || this.m_upgrading)
            {
                jsonObject.Put("lvl", new LogicJSONNumber(this.m_upgLevel));
            }
            else
            {
                jsonObject.Put("lvl", new LogicJSONNumber(-1));
            }

            if (this.m_constructionTimer != null)
            {
                jsonObject.Put("const_t", new LogicJSONNumber(this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime())));
            }

            LogicTrapData data = this.GetTrapData();

            if (data.HasAlternativeMode() || data.GetSpawnedCharAir() != null && data.GetSpawnedCharGround() != null)
            {
                LogicLayoutComponent layoutComponent = (LogicLayoutComponent) this.GetComponent(LogicComponentType.LAYOUT);

                if (layoutComponent != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        jsonObject.Put(layoutComponent.GetLayoutVariableNameAirMode(i, false), new LogicJSONBoolean(this.m_useAirMode[i]));
                    }
                }
            }

            if (data.GetDirectionCount() > 0)
            {
                LogicLayoutComponent layoutComponent = (LogicLayoutComponent) this.GetComponent(LogicComponentType.LAYOUT);

                if (layoutComponent != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        jsonObject.Put(layoutComponent.GetLayoutVariableNameTrapDirection(i, false), new LogicJSONNumber(this.m_direction[i]));
                    }
                }
            }

            base.SaveToSnapshot(jsonObject, layoutId);
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            this.LoadUpgradeLevel(jsonObject);

            LogicTrapData data = this.GetTrapData();

            if (data.HasAlternativeMode() || data.GetSpawnedCharAir() != null && data.GetSpawnedCharGround() != null)
            {
                LogicLayoutComponent layoutComponent = (LogicLayoutComponent) this.GetComponent(LogicComponentType.LAYOUT);

                if (layoutComponent != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        LogicJSONBoolean airModeObject = jsonObject.GetJSONBoolean(layoutComponent.GetLayoutVariableNameAirMode(i, false));

                        if (airModeObject != null)
                        {
                            this.m_useAirMode[i] = airModeObject.IsTrue();
                        }

                        LogicJSONBoolean draftAirModeObject = jsonObject.GetJSONBoolean(layoutComponent.GetLayoutVariableNameAirMode(i, true));

                        if (draftAirModeObject != null)
                        {
                            this.m_draftUseAirMode[i] = draftAirModeObject.IsTrue();
                        }
                    }
                }

                LogicTriggerComponent triggerComponent = this.GetTriggerComponent();

                int layoutId = this.m_level.GetCurrentLayout();
                bool airMode = this.m_useAirMode[layoutId];

                triggerComponent.SetAirTrigger(airMode);
                triggerComponent.SetGroundTrigger(!airMode);
            }

            if (data.GetDirectionCount() > 0)
            {
                LogicLayoutComponent layoutComponent = (LogicLayoutComponent) this.GetComponent(LogicComponentType.LAYOUT);

                if (layoutComponent != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        LogicJSONNumber trapDistanceObject = jsonObject.GetJSONNumber(layoutComponent.GetLayoutVariableNameTrapDirection(i, false));

                        if (trapDistanceObject != null)
                        {
                            this.m_direction[i] = trapDistanceObject.GetIntValue();
                        }

                        LogicJSONNumber draftTrapDistanceObject = jsonObject.GetJSONNumber(layoutComponent.GetLayoutVariableNameTrapDirection(i, true));

                        if (draftTrapDistanceObject != null)
                        {
                            this.m_draftDirection[i] = draftTrapDistanceObject.GetIntValue();
                        }
                    }
                }
            }

            this.m_level.GetWorkerManagerAt(this.m_villageType).DeallocateWorker(this);

            if (this.m_constructionTimer != null)
            {
                this.m_constructionTimer.Destruct();
                this.m_constructionTimer = null;
            }

            LogicJSONNumber constTimeObject = jsonObject.GetJSONNumber("const_t");

            if (constTimeObject != null)
            {
                int constTime = constTimeObject.GetIntValue();

                if (!LogicDataTables.GetGlobals().ClampBuildingTimes())
                {
                    if (this.m_upgLevel < data.GetUpgradeLevelCount() - 1)
                    {
                        constTime = LogicMath.Min(constTime, data.GetBuildTime(this.m_upgLevel + 1));
                    }
                }

                this.m_constructionTimer = new LogicTimer();
                this.m_constructionTimer.StartTimer(constTime, this.m_level.GetLogicTime(), false, -1);

                LogicJSONNumber constTimeEndObject = jsonObject.GetJSONNumber("const_t_end");

                if (constTimeEndObject != null)
                {
                    this.m_constructionTimer.SetEndTimestamp(constTimeEndObject.GetIntValue());
                }

                LogicJSONNumber conffObject = jsonObject.GetJSONNumber("con_ff");

                if (conffObject != null)
                {
                    this.m_constructionTimer.SetFastForward(conffObject.GetIntValue());
                }

                this.m_level.GetWorkerManagerAt(this.m_villageType).AllocateWorker(this);
                this.m_upgrading = this.m_upgLevel != -1;
            }

            LogicJSONBoolean disarmed = jsonObject.GetJSONBoolean("needs_repair");

            if (disarmed != null)
            {
                this.m_disarmed = disarmed.IsTrue();
            }

            this.SetUpgradeLevel(this.m_upgLevel);
            base.Load(jsonObject);
        }

        public override void LoadFromSnapshot(LogicJSONObject jsonObject)
        {
            if (this.m_data.GetVillageType() == 1)
            {
                this.Load(jsonObject);
                return;
            }

            LogicTrapData data = this.GetTrapData();

            this.LoadUpgradeLevel(jsonObject);
            this.m_level.GetWorkerManagerAt(this.m_villageType).DeallocateWorker(this);

            if (this.m_constructionTimer != null)
            {
                this.m_constructionTimer.Destruct();
                this.m_constructionTimer = null;
            }

            if (data.HasAlternativeMode() || data.GetSpawnedCharAir() != null && data.GetSpawnedCharGround() != null)
            {
                LogicLayoutComponent layoutComponent = (LogicLayoutComponent) this.GetComponent(LogicComponentType.LAYOUT);

                if (layoutComponent != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        LogicJSONBoolean airModeObject = jsonObject.GetJSONBoolean(layoutComponent.GetLayoutVariableNameAirMode(i, false));

                        if (airModeObject != null)
                        {
                            this.m_useAirMode[i] = airModeObject.IsTrue();
                        }
                    }
                }

                LogicTriggerComponent triggerComponent = this.GetTriggerComponent();

                bool airMode = this.m_useAirMode[this.m_level.GetWarLayout()];

                triggerComponent.SetAirTrigger(airMode);
                triggerComponent.SetGroundTrigger(!airMode);
            }

            if (data.GetDirectionCount() > 0)
            {
                LogicLayoutComponent layoutComponent = (LogicLayoutComponent) this.GetComponent(LogicComponentType.LAYOUT);

                if (layoutComponent != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        LogicJSONNumber trapDistanceObject = jsonObject.GetJSONNumber(layoutComponent.GetLayoutVariableNameTrapDirection(i, false));

                        if (trapDistanceObject != null)
                        {
                            this.m_direction[i] = trapDistanceObject.GetIntValue();
                        }
                    }
                }
            }

            this.m_level.GetWorkerManagerAt(this.m_data.GetVillageType()).DeallocateWorker(this);

            if (this.m_constructionTimer != null)
            {
                this.m_constructionTimer.Destruct();
                this.m_constructionTimer = null;
            }

            this.SetUpgradeLevel(this.m_upgLevel);
            base.LoadFromSnapshot(jsonObject);
        }

        public void LoadUpgradeLevel(LogicJSONObject jsonObject)
        {
            LogicJSONNumber lvlObject = jsonObject.GetJSONNumber("lvl");
            LogicTrapData data = this.GetTrapData();

            if (lvlObject != null)
            {
                this.m_upgLevel = lvlObject.GetIntValue();
                int maxLvl = data.GetUpgradeLevelCount();

                if (this.m_upgLevel >= maxLvl)
                {
                    Debugger.Warning(string.Format("LogicTrap::load() - Loaded upgrade level {0} is over max! (max = {1}) id {2} data id {3}",
                                                   lvlObject.GetIntValue(),
                                                   maxLvl,
                                                   this.m_globalId,
                                                   data.GetGlobalID()));
                    this.m_upgLevel = maxLvl - 1;
                }
                else
                {
                    if (this.m_upgLevel < -1)
                    {
                        Debugger.Error("LogicTrap::load() - Loaded an illegal upgrade level!");
                    }
                }
            }
        }

        public override void LoadingFinished()
        {
            base.LoadingFinished();

            if (LogicDataTables.GetGlobals().ClampBuildingTimes())
            {
                if (this.m_constructionTimer != null)
                {
                    if (this.m_upgLevel < this.GetTrapData().GetUpgradeLevelCount() - 1)
                    {
                        int remainingSecs = this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime());
                        int totalSecs = this.GetTrapData().GetBuildTime(this.m_upgLevel + 1);

                        if (remainingSecs > totalSecs)
                        {
                            this.m_constructionTimer.StartTimer(totalSecs, this.m_level.GetLogicTime(), true,
                                                                this.m_level.GetHomeOwnerAvatarChangeListener().GetCurrentTimestamp());
                        }
                    }
                }
            }

            if (this.m_listener != null)
            {
                this.m_listener.LoadedFromJSON();
            }
        }

        public override LogicGameObjectType GetGameObjectType()
        {
            return LogicGameObjectType.TRAP;
        }

        public override int GetWidthInTiles()
        {
            return this.GetTrapData().GetWidth();
        }

        public override int GetHeightInTiles()
        {
            return this.GetTrapData().GetHeight();
        }

        public int GetUpgradeLevel()
        {
            return this.m_upgLevel;
        }

        public bool IsConstructing()
        {
            return this.m_constructionTimer != null;
        }

        public bool IsUpgrading()
        {
            return this.m_constructionTimer != null && this.m_upgrading;
        }

        public int GetRemainingConstructionTime()
        {
            if (this.m_constructionTimer != null)
            {
                return this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime());
            }

            return 0;
        }

        public int GetRemainingConstructionTimeMS()
        {
            if (this.m_constructionTimer != null)
            {
                return this.m_constructionTimer.GetRemainingMS(this.m_level.GetLogicTime());
            }

            return 0;
        }

        public void StartUpgrading()
        {
            if (this.m_constructionTimer != null)
            {
                this.m_constructionTimer.Destruct();
                this.m_constructionTimer = null;
            }

            LogicTrapData data = this.GetTrapData();

            this.m_upgrading = true;
            int buildTime = data.GetBuildTime(this.m_upgLevel + 1);

            if (buildTime <= 0)
            {
                this.FinishConstruction(false);
            }
            else
            {
                this.m_level.GetWorkerManagerAt(data.GetVillageType()).AllocateWorker(this);

                this.m_constructionTimer = new LogicTimer();
                this.m_constructionTimer.StartTimer(buildTime, this.m_level.GetLogicTime(), true, this.m_level.GetHomeOwnerAvatarChangeListener().GetCurrentTimestamp());
            }
        }

        public void CancelConstruction()
        {
            LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();

            if (homeOwnerAvatar != null && homeOwnerAvatar.IsClientAvatar())
            {
                if (this.m_constructionTimer != null)
                {
                    this.m_constructionTimer.Destruct();
                    this.m_constructionTimer = null;

                    int upgLevel = this.m_upgLevel;

                    if (this.m_upgrading)
                    {
                        this.SetUpgradeLevel(this.m_upgLevel);
                        upgLevel += 1;
                    }

                    LogicTrapData data = this.GetTrapData();
                    LogicResourceData buildResourceData = data.GetBuildResource();
                    int buildCost = data.GetBuildCost(upgLevel);
                    int refundedCount = LogicMath.Max(LogicDataTables.GetGlobals().GetBuildCancelMultiplier() * buildCost / 100, 0);

                    homeOwnerAvatar.CommodityCountChangeHelper(0, buildResourceData, refundedCount);

                    this.m_level.GetWorkerManagerAt(this.m_data.GetVillageType()).DeallocateWorker(this);

                    if (upgLevel != 0)
                    {
                        if (this.m_listener != null)
                        {
                            this.m_listener.RefreshState();
                        }
                    }
                    else
                    {
                        this.GetGameObjectManager().RemoveGameObject(this);
                    }
                }
            }
        }

        public int GetRequiredTownHallLevelForUpgrade()
        {
            return this.GetTrapData().GetRequiredTownHallLevel(LogicMath.Min(this.m_upgLevel + 1, this.GetTrapData().GetUpgradeLevelCount() - 1));
        }

        public bool CanUpgrade(bool canCallListener)
        {
            if (this.m_constructionTimer == null && !this.IsMaxUpgradeLevel())
            {
                if (this.m_level.GetTownHallLevel(this.m_villageType) >= this.GetRequiredTownHallLevelForUpgrade())
                {
                    return true;
                }

                if (canCallListener)
                {
                    this.m_level.GetGameListener().TownHallLevelTooLow(this.GetRequiredTownHallLevelForUpgrade());
                }
            }

            return false;
        }

        public bool IsMaxUpgradeLevel()
        {
            LogicTrapData trapData = this.GetTrapData();
            if (trapData.GetVillageType() != 1 || this.GetRequiredTownHallLevelForUpgrade() < this.m_level.GetGameMode().GetConfiguration().GetMaxTownHallLevel())
            {
                return this.m_upgLevel >= trapData.GetUpgradeLevelCount() - 1;
            }

            return true;
        }

        public bool SpeedUpConstruction()
        {
            if (this.m_constructionTimer != null)
            {
                LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();

                int remainingSecs = this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime());
                int speedUpCost = LogicGamePlayUtil.GetSpeedUpCost(remainingSecs, 0, this.m_villageType);

                if (playerAvatar.HasEnoughDiamonds(speedUpCost, true, this.m_level))
                {
                    playerAvatar.UseDiamonds(speedUpCost);
                    playerAvatar.GetChangeListener().DiamondPurchaseMade(4, this.m_data.GetGlobalID(), this.m_upgLevel + (this.m_upgrading ? 2 : 1), speedUpCost,
                                                                         this.m_level.GetVillageType());

                    this.FinishConstruction(false);

                    return true;
                }
            }

            return false;
        }

        public void SpawnUnit(int count)
        {
            LogicTrapData data = this.GetTrapData();
            LogicCharacterData spawnData = this.m_useAirMode[this.m_level.GetActiveLayout(this.m_villageType)] ? data.GetSpawnedCharAir() : data.GetSpawnedCharGround();

            if (spawnData != null)
            {
                LogicVector2 position = new LogicVector2();

                for (int i = 0, j = 59, k = 0, l = 0; i < count; i++, j += 59, k += 128, l += 360)
                {
                    int random = l / data.GetNumSpawns(this.m_upgLevel) + j * this.m_numSpawns % 360;
                    int randomX = (byte) (k & 0x80) ^ 0x180;
                    int posX = this.GetMidX() + LogicMath.GetRotatedX(randomX, 0, random);
                    int posY = this.GetMidY() + LogicMath.GetRotatedY(randomX, 0, random);

                    if (spawnData.IsFlying())
                    {
                        position.m_x = posX;
                        position.m_y = posY;
                    }
                    else
                    {
                        if (!this.m_level.GetTileMap().GetNearestPassablePosition(posX, posY, position, 1536))
                        {
                            continue;
                        }
                    }

                    LogicCharacter character = (LogicCharacter) LogicGameObjectFactory.CreateGameObject(spawnData, this.m_level, this.m_villageType);

                    character.GetHitpointComponent().SetTeam(1);
                    character.GetMovementComponent().EnableJump(3600000);
                    character.SetInitialPosition(position.m_x, position.m_y);
                    character.SetSpawnTime(200);

                    this.GetGameObjectManager().AddGameObject(character, -1);
                }

                position.Destruct();
            }
        }

        public bool IsAirMode(int layout, bool draft)
        {
            if (draft)
            {
                return this.m_draftUseAirMode[layout];
            }

            return this.m_useAirMode[layout];
        }

        public void ToggleAirMode(int layout, bool draft)
        {
            bool[] array = this.m_useAirMode;

            if (draft)
            {
                array = this.m_draftUseAirMode;
            }

            array[layout] ^= true;
        }

        public void ToggleDirection(int layout, bool draft)
        {
            int[] array = this.m_direction;

            if (draft)
            {
                array = this.m_draftDirection;
            }

            int direction = 0;

            if (array[layout] + 1 < this.GetTrapData().GetDirectionCount())
            {
                direction = array[layout] + 1;
            }

            array[layout] = direction;
        }

        public void SetDirection(int layout, bool draft, int count)
        {
            int[] array = this.m_direction;

            if (draft)
            {
                array = this.m_draftDirection;
            }

            array[layout] = count;
        }

        public int GetDirection(int layout, bool draft)
        {
            if (layout <= 7)
            {
                if (draft)
                {
                    return this.m_draftDirection[layout];
                }

                return this.m_direction[layout];
            }

            return 0;
        }

        public bool HasAirMode()
        {
            LogicTrapData data = (LogicTrapData) this.m_data;

            if (data.GetSpawnedCharAir() == null || data.GetSpawnedCharGround() == null)
            {
                return data.HasAlternativeMode();
            }

            return true;
        }
    }
}