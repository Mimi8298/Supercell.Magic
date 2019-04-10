namespace Supercell.Magic.Logic.GameObject.Component
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject.Listener;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicHitpointComponent : LogicComponent
    {
        private bool m_regenerationEnabled;

        private int m_team;
        private int m_hp;
        private int m_maxHp;
        private int m_originalHp;
        private int m_maxRegenerationTime;
        private int m_lastDamageTime;
        private int m_regenTime;
        private int m_shrinkTime;
        private int m_extraHpTime;
        private int m_poisonInitTime;
        private int m_poisonDamage;
        private int m_poisonTime;
        private int m_invulnerabilityTime; // 76
        private int m_damagePermilCount;

        private readonly int[] m_healingTime; // 80
        private readonly int[] m_healingId; // 112

        private LogicEffectData m_dieEffect;
        private LogicEffectData m_dieEffect2;

        public LogicHitpointComponent(LogicGameObject gameObject, int hp, int team) : base(gameObject)
        {
            this.m_team = team;

            this.m_healingTime = new int[8];
            this.m_healingId = new int[8];

            this.m_lastDamageTime = 60;
            this.m_hp = 100 * hp;
            this.m_maxHp = 100 * hp;
            this.m_originalHp = 100 * hp;
        }

        public void SetMaxRegenerationTime(int time)
        {
            this.m_maxRegenerationTime = time;
        }

        public void EnableRegeneration(bool state)
        {
            this.m_regenerationEnabled = state;
        }

        public void SetPoisonDamage(int damage, bool increaseSlowly)
        {
            int time = 8;

            if (this.m_poisonTime >= 80)
            {
                time = 24;

                if (this.m_poisonTime >= 320)
                {
                    time = 136;

                    if (this.m_poisonTime >= 1000)
                    {
                        time = 0;
                    }
                }
            }

            if (this.m_poisonDamage != 0)
            {
                if (this.m_poisonDamage >= damage)
                {
                    if (this.m_poisonDamage > damage)
                    {
                        time = damage * time / this.m_poisonDamage;
                    }
                }
                else
                {
                    this.m_poisonTime = this.m_poisonDamage * this.m_poisonTime / damage;
                    this.m_poisonDamage = damage;
                }
            }
            else
            {
                this.m_poisonDamage = damage;
            }

            this.m_poisonTime = increaseSlowly ? LogicMath.Min(time + this.m_poisonTime, 1000) : 1000;
            this.m_poisonInitTime = 640;
        }

        public void CauseDamage(int damage, int gameObjectId, LogicGameObject gameObject)
        {
            if (damage >= 0 || this.m_hp != 0)
            {
                if (this.m_parent == null)
                {
                    if (damage > 0 && this.m_invulnerabilityTime > 0)
                    {
                        return;
                    }
                }
                else
                {
                    LogicCombatComponent combatComponent = this.m_parent.GetCombatComponent();

                    if (combatComponent != null)
                    {
                        if (combatComponent.GetUndergroundTime() > 0 && damage > 0)
                        {
                            damage = 0;
                        }
                    }

                    if (!this.m_parent.GetLevel().GetInvulnerabilityEnabled())
                    {
                        if (damage > 0 && this.m_invulnerabilityTime > 0)
                        {
                            return;
                        }
                    }
                    else
                    {
                        damage = 0;
                    }

                    if (this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                    {
                        LogicCharacter character = (LogicCharacter) this.m_parent;
                        LogicArrayList<LogicCharacter> childTroops = character.GetChildTroops();

                        if (childTroops != null && childTroops.Size() > 0 || character.GetSpawnDelay() > 0)
                        {
                            return;
                        }
                    }
                }

                if (gameObjectId != 0 && damage < 0)
                {
                    int prevHealingIdx = -1;
                    int healingIdx = -1;

                    for (int i = 0; i < 8; i++)
                    {
                        if (this.m_healingId[i] == gameObjectId)
                        {
                            prevHealingIdx = i;
                        }
                        else if (healingIdx == -1)
                        {
                            healingIdx = i;

                            if (this.m_healingTime[i] > 0)
                            {
                                healingIdx = -1;
                            }
                        }
                    }

                    if (healingIdx < prevHealingIdx && prevHealingIdx != -1 && healingIdx != -1)
                    {
                        this.m_healingId[healingIdx] = gameObjectId;
                        this.m_healingTime[healingIdx] = 1000;
                        this.m_healingId[prevHealingIdx] = 0;
                        this.m_healingTime[prevHealingIdx] = 0;
                    }
                    else if (prevHealingIdx == -1)
                    {
                        if (healingIdx != -1)
                        {
                            this.m_healingId[healingIdx] = gameObjectId;
                            this.m_healingTime[healingIdx] = 1000;
                        }
                        else
                        {
                            healingIdx = 8;
                        }
                    }
                    else
                    {
                        healingIdx = prevHealingIdx;
                        this.m_healingTime[prevHealingIdx] = 1000;
                    }

                    damage = damage * LogicDataTables.GetGlobals().GetHealStackPercent(healingIdx) / 100;
                }

                int prevHp = (this.m_hp + 99) / 100;
                int prevAccurateHp = this.m_hp;
                this.m_hp = LogicMath.Clamp(this.m_hp - damage, 0, this.m_maxHp);
                int hp = (this.m_hp + 99) / 100;

                if (prevHp > hp)
                {
                    LogicResourceStorageComponent resourceStorageComponent =
                        (LogicResourceStorageComponent) this.m_parent.GetComponent(LogicComponentType.RESOURCE_STORAGE);
                    LogicResourceProductionComponent resourceProductionComponent =
                        (LogicResourceProductionComponent) this.m_parent.GetComponent(LogicComponentType.RESOURCE_PRODUCTION);
                    LogicWarResourceStorageComponent warResourceStorageComponent =
                        (LogicWarResourceStorageComponent) this.m_parent.GetComponent(LogicComponentType.WAR_RESOURCE_STORAGE);

                    if (this.m_parent.GetGameObjectType() == LogicGameObjectType.BUILDING)
                    {
                        LogicBuilding building = (LogicBuilding) this.m_parent;

                        if (!building.GetBuildingData().IsLootOnDestruction() || prevAccurateHp > 0 && (this.m_hp == 0 || (uint) this.m_hp >= 0xFFFFFF3A))
                        {
                            if (resourceStorageComponent != null)
                                resourceStorageComponent.ResourcesStolen(prevHp - hp, prevHp);
                            if (resourceProductionComponent != null)
                                resourceProductionComponent.ResourcesStolen(prevHp - hp, prevHp);
                            if (warResourceStorageComponent != null)
                                warResourceStorageComponent.ResourcesStolen(prevHp - hp, prevHp);
                        }
                    }

                    if (this.m_parent.IsWall())
                        this.m_parent.RefreshPassable();
                    this.m_lastDamageTime = 0;
                }

                this.UpdateHeroHealthToAvatar(hp);

                if (damage <= 0)
                {
                    if (damage < 0)
                    {
                        // Listener
                    }
                }
                else
                {
                    if (this.m_parent.GetMovementComponent() != null)
                        this.m_parent.GetMovementComponent().SetPatrolFreeze();
                }

                if (prevAccurateHp > 0 && this.m_hp == 0)
                {
                    this.m_parent.DeathEvent();
                    this.m_parent.GetLevel().UpdateBattleStatus();

                    if (this.m_parent.IsWall())
                        this.WallRemoved();
                }
            }
        }

        public void Kill()
        {
            this.m_invulnerabilityTime = 0;
            this.CauseDamage(this.m_maxHp, 0, null);
        }

        public void CauseDamagePermil(int hp)
        {
            this.CauseDamage(hp, 0, null);
            this.m_damagePermilCount += 1;
        }

        public int GetDamagePermilCount()
        {
            return this.m_damagePermilCount;
        }

        public void UpdateHeroHealthToAvatar(int hitpoint)
        {
            LogicAvatar avatar = this.m_team == 1 ? this.m_parent.GetLevel().GetHomeOwnerAvatar() : this.m_parent.GetLevel().GetVisitorAvatar();
            LogicHeroData data = null;

            int upgLevel = 0;

            if (this.m_parent.IsHero())
            {
                LogicCharacter character = (LogicCharacter) this.m_parent;

                data = (LogicHeroData) character.GetCharacterData();
                upgLevel = character.GetUpgradeLevel();
            }
            else if (this.m_parent.GetGameObjectType() == LogicGameObjectType.BUILDING)
            {
                LogicBuilding building = (LogicBuilding) this.m_parent;
                LogicHeroBaseComponent heroBaseComponent = building.GetHeroBaseComponent();

                if (heroBaseComponent == null)
                {
                    return;
                }

                LogicBuildingData buildingData = building.GetBuildingData();

                if (!buildingData.GetShareHeroCombatData())
                {
                    return;
                }

                LogicCombatComponent combatComponent = building.GetCombatComponent();

                if (combatComponent == null || !combatComponent.IsEnabled())
                {
                    return;
                }

                data = buildingData.GetHeroData();
                upgLevel = avatar.GetUnitUpgradeLevel(data);
            }

            if (data != null)
            {
                int secs = LogicMath.Min(data.GetSecondsToFullHealth(hitpoint, upgLevel), data.GetFullRegenerationTimeSec(upgLevel));

                if (avatar != null)
                {
                    avatar.GetChangeListener().CommodityCountChanged(0, data, secs);
                    avatar.SetHeroHealth(data, secs);
                }
            }
        }

        public void WallRemoved()
        {
            LogicArrayList<LogicComponent> components = this.m_parent.GetComponentManager().GetComponents(LogicComponentType.MOVEMENT);

            for (int i = 0; i < components.Size(); i++)
            {
                LogicCombatComponent combatComponent = components[i].GetParent().GetCombatComponent();

                if (combatComponent != null && combatComponent.GetTarget(0) != null)
                {
                    LogicGameObject target = combatComponent.GetTarget(0);

                    if (target.IsWall())
                    {
                        combatComponent.ForceNewTarget();
                    }
                }
            }
        }

        public void SetMaxHitpoints(int maxHp)
        {
            this.m_maxHp = 100 * maxHp;
            this.m_hp = LogicMath.Clamp(this.m_hp, 0, this.m_maxHp);
            this.m_originalHp = this.m_maxHp;
        }

        public void SetHitpoints(int hp)
        {
            this.m_hp = LogicMath.Clamp(100 * hp, 0, this.m_maxHp);
        }

        public bool IsEnemy(LogicGameObject gameObject)
        {
            LogicHitpointComponent hitpointComponent = (LogicHitpointComponent) gameObject.GetComponent(LogicComponentType.HITPOINT);

            if (hitpointComponent != null && hitpointComponent.GetTeam() != this.m_team)
            {
                return this.m_hp > 0;
            }

            return false;
        }

        public bool IsEnemyForTeam(int team)
        {
            return this.m_team != team && this.m_hp > 0;
        }

        public int GetTeam()
        {
            return this.m_team;
        }

        public void SetTeam(int team)
        {
            this.m_team = team;

            if (team != 0)
            {
                if (this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                {
                    LogicCharacter character = (LogicCharacter) this.m_parent;
                    LogicCombatComponent combatComponent = character.GetCombatComponent();

                    if (combatComponent != null)
                    {
                        if (character.GetCharacterData().IsUnderground())
                        {
                            combatComponent.SetUndergroundTime(0);
                        }

                        LogicMovementComponent movementComponent = character.GetMovementComponent();

                        if (movementComponent != null)
                        {
                            movementComponent.SetUnderground(false);
                        }
                    }
                }
            }
        }

        public int GetInvulnerabilityTime()
        {
            return this.m_invulnerabilityTime;
        }

        public void SetInvulnerabilityTime(int ms)
        {
            this.m_invulnerabilityTime = ms;
        }

        public bool HasFullHitpoints()
        {
            return this.m_hp == this.m_maxHp;
        }

        public void SetDieEffect(LogicEffectData data1, LogicEffectData data2)
        {
            this.m_dieEffect = data1;
            this.m_dieEffect = data2;
        }

        public bool IsDamagedRecently()
        {
            return this.m_lastDamageTime < 30;
        }

        public override void Tick()
        {
            if (this.m_hp < this.m_maxHp && this.m_regenerationEnabled)
            {
                int rebuildEffectHp = this.m_maxHp / 5;
                int prevHp = this.m_hp;

                if (this.m_maxRegenerationTime <= 0)
                {
                    this.m_hp = this.m_maxHp;
                    this.m_regenTime = 0;
                }
                else
                {
                    this.m_regenTime += 64;
                    int tmp = LogicMath.Max(1000 * this.m_maxRegenerationTime / (this.m_maxHp / 100), 1);
                    this.m_hp += 100 * (this.m_regenTime / tmp);

                    if (this.m_hp >= this.m_maxHp)
                    {
                        this.m_hp = this.m_maxHp;
                        this.m_regenTime = 0;
                    }
                    else
                    {
                        this.m_regenTime %= tmp;
                    }
                }

                if (prevHp < rebuildEffectHp && this.m_hp >= rebuildEffectHp)
                {
                    LogicGameObjectListener listener = this.GetParentListener();

                    if (listener != null)
                    {
                        // Listener.
                    }
                }
            }

            if (this.m_extraHpTime > 0)
            {
                this.m_extraHpTime -= 64;

                if (this.m_extraHpTime <= 0)
                {
                    this.m_extraHpTime = 0;

                    if (this.m_hp > 0)
                    {
                        if (this.m_originalHp != this.m_maxHp)
                        {
                            this.m_hp = (int) (this.m_originalHp * (long) this.m_hp / this.m_maxHp);
                            this.m_maxHp = this.m_originalHp;
                        }

                        this.m_extraHpTime = -1;
                    }
                }
            }

            if (this.m_poisonInitTime <= 0)
            {
                if (this.m_poisonTime > 0)
                {
                    this.m_poisonTime -= 10;

                    if (this.m_poisonTime <= 0)
                    {
                        this.m_poisonTime = 0;
                        this.m_poisonDamage = 0;
                    }
                }
            }
            else
            {
                this.m_poisonInitTime = LogicMath.Max(this.m_poisonInitTime - 64, 0);
            }

            if (this.m_poisonTime > 0)
            {
                this.CauseDamage((int) (((this.m_poisonDamage * this.m_poisonTime / 1000L) * 64L) / 1000L), 0, null);
            }

            this.m_invulnerabilityTime = LogicMath.Max(this.m_invulnerabilityTime - 64, 0);

            for (int i = 0; i < 8; i++)
            {
                this.m_healingTime[i] -= 64;

                if (this.m_healingTime[i] <= 0)
                {
                    this.m_healingTime[i] = 0;
                    this.m_healingId[i] = 0;
                }
            }

            this.m_lastDamageTime += 1;

            if (this.m_shrinkTime > 0)
            {
                this.m_shrinkTime -= 1;

                if (this.m_shrinkTime == 0)
                {
                    Debugger.HudPrint("HP TO ORIGINAL");

                    this.m_hp = LogicMath.Min(this.m_originalHp * (100 * this.m_hp / this.m_maxHp) / 100, this.m_originalHp);
                    this.m_maxHp = this.m_originalHp;
                }
            }
        }

        public override void FastForwardTime(int time)
        {
            if (this.m_regenerationEnabled)
            {
                this.m_regenTime += 64;

                if (this.m_maxRegenerationTime <= time)
                {
                    this.m_hp = this.m_maxHp;
                }
                else
                {
                    this.m_hp += this.m_maxHp * time / this.m_maxRegenerationTime;

                    if (this.m_hp > this.m_maxHp)
                    {
                        this.m_hp = this.m_maxHp;
                    }
                }
            }
        }

        public override LogicComponentType GetComponentType()
        {
            return LogicComponentType.HITPOINT;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            this.LoadHitpoint(jsonObject);
        }

        public override void LoadFromSnapshot(LogicJSONObject jsonObject)
        {
            this.LoadHitpoint(jsonObject);

            this.m_hp = this.m_maxHp;
            this.m_regenerationEnabled = false;
        }

        public void LoadHitpoint(LogicJSONObject jsonObject)
        {
            LogicJSONNumber hpNumber = jsonObject.GetJSONNumber("hp");
            LogicJSONBoolean regenBoolean = jsonObject.GetJSONBoolean("reg");

            if (hpNumber != null)
            {
                this.m_hp = this.m_parent.GetLevel().GetState() != 2 ? LogicMath.Clamp(hpNumber.GetIntValue(), 0, this.m_maxHp) : this.m_maxHp;
            }
            else
            {
                this.m_hp = this.m_maxHp;
            }

            this.m_regenerationEnabled = regenBoolean != null && regenBoolean.IsTrue();
        }

        public override void Save(LogicJSONObject jsonObject, int villageType)
        {
            if (this.m_hp < this.m_maxHp)
            {
                jsonObject.Put("hp", new LogicJSONNumber(this.m_hp));
                jsonObject.Put("reg", new LogicJSONBoolean(this.m_regenerationEnabled));
            }
        }

        public override void SaveToSnapshot(LogicJSONObject jsonObject, int layoutId)
        {
            if (this.m_hp < this.m_maxHp)
            {
                jsonObject.Put("hp", new LogicJSONNumber(this.m_hp));
                jsonObject.Put("reg", new LogicJSONBoolean(this.m_regenerationEnabled));
            }
        }

        public int GetHitpoints()
        {
            return this.m_hp;
        }

        public int GetMaxHitpoints()
        {
            return this.m_maxHp;
        }

        public int GetOriginalHitpoints()
        {
            return this.m_originalHp;
        }

        public void SetExtraHealth(int hp, int time)
        {
            if (this.m_hp > 0)
            {
                if (time == -1 || this.m_maxHp <= hp)
                {
                    if (this.m_maxHp != hp)
                    {
                        this.m_hp = (int) ((long) hp * this.m_hp / this.m_maxHp);
                        this.m_maxHp = hp;
                    }

                    this.m_extraHpTime = time;
                }
            }
        }

        public void SetShrinkHitpoints(int time, int hp)
        {
            this.m_shrinkTime = time;
            this.m_maxHp = hp * this.m_originalHp / 100;
            this.m_hp = LogicMath.Min(this.m_hp, this.m_maxHp);
        }

        public int GetPoisonRemainingMS()
        {
            return this.m_poisonInitTime + (this.m_poisonTime << 6) / 10;
        }
    }
}