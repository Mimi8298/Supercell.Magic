namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Debug;

    public class LogicSpellData : LogicCombatItemData
    {
        private int[] m_damage;
        private int[] m_troopDamagePermil;
        private int[] m_buildingDamagePermil;
        private int[] m_poisonDamage;
        private int[] m_executeHealthPermil;
        private int[] m_damagePermilMin;
        private int[] m_damageBoostPercent;
        private int[] m_preferredDamagePermilMin;
        private int[] m_boostTimeMS;
        private int[] m_speedBoost;
        private int[] m_speedBoost2;
        private int[] m_duplicateLifetime;
        private int[] m_jumpBoostMS;
        private int[] m_jumpHousingLimit;
        private int[] m_freezeTimeMS;
        private int[] m_duplicateHousing;
        private int[] m_unitsToSpawn;
        private int[] m_strengthWeight;
        private int[] m_radius;
        private int[] m_randomRadius;
        private int[] m_timeBetweenHitsMS;
        private int[] m_buildingDamageBoostPercent;
        private int[] m_shieldProjectileSpeed;
        private int[] m_shieldProjectileDamageMod;
        private int[] m_extraHealthPermil;
        private int[] m_extraHealthMin;
        private int[] m_extraHealthMax;
        private int[] m_attackSpeedBoost;
        private int[] m_invulnerabilityTime;
        private int[] m_maxUnitsHit;
        private int[] m_numberOfHits;
        private int[] m_spawnDuration;

        private bool m_randomRadiusAffectsOnlyGfx;
        private bool m_poisonIncreaseSlowly;
        private bool m_poisonAffectAir;
        private bool m_scaleByTownHall;
        private bool m_troopsOnly;
        private bool m_snapToGrid;
        private bool m_boostDefenders;
        private bool m_boostLinkedToPoison;
        private bool m_scaleDeployEffects;

        private int m_spawnFirstGroupSize;
        private int m_pauseCombatComponentMs;
        private int m_damageTHPercent;
        private int m_shrinkReduceSpeedRatio;
        private int m_shrinkHitpointsRatio;
        private int m_deployEffect2Delay;
        private int m_hitTimeMS;
        private int m_deployTimeMS;
        private int m_chargingTimeMS;
        private int m_spellForgeLevel;
        private int m_numObstacles;
        private int m_preferredTargetDamageMod;
        private int m_heroDamageMultiplier;

        private string m_targetInfoString;

        private LogicObstacleData m_spawnObstacle;
        private LogicData m_preferredTarget;
        private LogicCharacterData m_summonTroop;

        private LogicEffectData[] m_preDeployEffect;
        private LogicEffectData[] m_deployEffect;
        private LogicEffectData[] m_deployEffect2;
        private LogicEffectData[] m_enemyDeployEffect;
        private LogicEffectData[] m_chargingEffect;
        private LogicEffectData[] m_hitEffect;

        public LogicSpellData(CSVRow row, LogicDataTable table) : base(row, table)
        {
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_damage = new int[this.m_upgradeLevelCount];
            this.m_troopDamagePermil = new int[this.m_upgradeLevelCount];
            this.m_buildingDamagePermil = new int[this.m_upgradeLevelCount];
            this.m_executeHealthPermil = new int[this.m_upgradeLevelCount];
            this.m_damagePermilMin = new int[this.m_upgradeLevelCount];
            this.m_preferredDamagePermilMin = new int[this.m_upgradeLevelCount];
            this.m_boostTimeMS = new int[this.m_upgradeLevelCount];
            this.m_speedBoost = new int[this.m_upgradeLevelCount];
            this.m_speedBoost2 = new int[this.m_upgradeLevelCount];
            this.m_damageBoostPercent = new int[this.m_upgradeLevelCount];
            this.m_duplicateLifetime = new int[this.m_upgradeLevelCount];
            this.m_duplicateHousing = new int[this.m_upgradeLevelCount];
            this.m_radius = new int[this.m_upgradeLevelCount];
            this.m_numberOfHits = new int[this.m_upgradeLevelCount];
            this.m_randomRadius = new int[this.m_upgradeLevelCount];
            this.m_timeBetweenHitsMS = new int[this.m_upgradeLevelCount];
            this.m_jumpBoostMS = new int[this.m_upgradeLevelCount];
            this.m_jumpHousingLimit = new int[this.m_upgradeLevelCount];
            this.m_freezeTimeMS = new int[this.m_upgradeLevelCount];
            this.m_strengthWeight = new int[this.m_upgradeLevelCount];
            this.m_buildingDamageBoostPercent = new int[this.m_upgradeLevelCount];
            this.m_shieldProjectileSpeed = new int[this.m_upgradeLevelCount];
            this.m_shieldProjectileDamageMod = new int[this.m_upgradeLevelCount];
            this.m_extraHealthPermil = new int[this.m_upgradeLevelCount];
            this.m_extraHealthMin = new int[this.m_upgradeLevelCount];
            this.m_extraHealthMax = new int[this.m_upgradeLevelCount];
            this.m_poisonDamage = new int[this.m_upgradeLevelCount];
            this.m_attackSpeedBoost = new int[this.m_upgradeLevelCount];
            this.m_invulnerabilityTime = new int[this.m_upgradeLevelCount];
            this.m_maxUnitsHit = new int[this.m_upgradeLevelCount];
            this.m_unitsToSpawn = new int[this.m_upgradeLevelCount];
            this.m_spawnDuration = new int[this.m_upgradeLevelCount];

            this.m_preDeployEffect = new LogicEffectData[this.m_upgradeLevelCount];
            this.m_deployEffect = new LogicEffectData[this.m_upgradeLevelCount];
            this.m_deployEffect2 = new LogicEffectData[this.m_upgradeLevelCount];
            this.m_enemyDeployEffect = new LogicEffectData[this.m_upgradeLevelCount];
            this.m_chargingEffect = new LogicEffectData[this.m_upgradeLevelCount];
            this.m_hitEffect = new LogicEffectData[this.m_upgradeLevelCount];

            for (int i = 0; i < this.m_upgradeLevelCount; i++)
            {
                this.m_damage[i] = LogicGamePlayUtil.DPSToSingleHit(this.GetClampedIntegerValue("Damage", i), 1000);
                this.m_troopDamagePermil[i] = this.GetClampedIntegerValue("TroopDamagePermil", i);
                this.m_buildingDamagePermil[i] = this.GetClampedIntegerValue("BuildingDamagePermil", i);
                this.m_executeHealthPermil[i] = this.GetClampedIntegerValue("ExecuteHealthPermil", i);
                this.m_damagePermilMin[i] = this.GetClampedIntegerValue("DamagePermilMin", i);
                this.m_preferredDamagePermilMin[i] = this.GetClampedIntegerValue("PreferredDamagePermilMin", i);
                this.m_boostTimeMS[i] = this.GetClampedIntegerValue("BoostTimeMS", i);
                this.m_speedBoost[i] = this.GetClampedIntegerValue("SpeedBoost", i);
                this.m_speedBoost2[i] = this.GetClampedIntegerValue("SpeedBoost2", i);
                this.m_damageBoostPercent[i] = this.GetClampedIntegerValue("DamageBoostPercent", i);
                this.m_duplicateLifetime[i] = this.GetClampedIntegerValue("DuplicateLifetime", i);
                this.m_duplicateHousing[i] = this.GetClampedIntegerValue("DuplicateHousing", i);
                this.m_radius[i] = (this.GetClampedIntegerValue("Radius", i) << 9) / 100;
                this.m_numberOfHits[i] = this.GetClampedIntegerValue("NumberOfHits", i);
                this.m_randomRadius[i] = (this.GetClampedIntegerValue("RandomRadius", i) << 9) / 100;
                this.m_timeBetweenHitsMS[i] = this.GetClampedIntegerValue("TimeBetweenHitsMS", i);
                this.m_jumpBoostMS[i] = this.GetClampedIntegerValue("JumpBoostMS", i);
                this.m_jumpHousingLimit[i] = this.GetClampedIntegerValue("JumpHousingLimit", i);
                this.m_hitEffect[i] = LogicDataTables.GetEffectByName(this.GetClampedValue("HitEffect", i), this);
                this.m_chargingEffect[i] = LogicDataTables.GetEffectByName(this.GetClampedValue("ChargingEffect", i), this);
                this.m_preDeployEffect[i] = LogicDataTables.GetEffectByName(this.GetClampedValue("PreDeployEffect", i), this);
                this.m_deployEffect[i] = LogicDataTables.GetEffectByName(this.GetClampedValue("DeployEffect", i), this);
                this.m_enemyDeployEffect[i] = LogicDataTables.GetEffectByName(this.GetClampedValue("EnemyDeployEffect", i), this);
                this.m_deployEffect2[i] = LogicDataTables.GetEffectByName(this.GetClampedValue("DeployEffect2", i), this);
                this.m_freezeTimeMS[i] = this.GetClampedIntegerValue("FreezeTimeMS", i);
                this.m_strengthWeight[i] = this.GetClampedIntegerValue("StrengthWeight", i);
                this.m_buildingDamageBoostPercent[i] = this.GetClampedIntegerValue("BuildingDamageBoostPercent", i);
                this.m_shieldProjectileSpeed[i] = this.GetClampedIntegerValue("ShieldProjectileSpeed", i);
                this.m_shieldProjectileDamageMod[i] = this.GetClampedIntegerValue("ShieldProjectileDamageMod", i);
                this.m_extraHealthPermil[i] = this.GetClampedIntegerValue("ExtraHealthPermil", i);
                this.m_extraHealthMin[i] = this.GetClampedIntegerValue("ExtraHealthMin", i);
                this.m_extraHealthMax[i] = this.GetClampedIntegerValue("ExtraHealthMax", i);
                this.m_poisonDamage[i] = LogicGamePlayUtil.DPSToSingleHit(this.GetClampedIntegerValue("PoisonDPS", i), 1000);
                this.m_attackSpeedBoost[i] = this.GetClampedIntegerValue("AttackSpeedBoost", i);
                this.m_invulnerabilityTime[i] = this.GetClampedIntegerValue("InvulnerabilityTime", i);
                this.m_maxUnitsHit[i] = this.GetClampedIntegerValue("MaxUnitsHit", i);
                this.m_unitsToSpawn[i] = this.GetClampedIntegerValue("UnitsToSpawn", i);
                this.m_spawnDuration[i] = this.GetClampedIntegerValue("SpawnDuration", i);
            }

            this.m_poisonIncreaseSlowly = this.GetBooleanValue("PoisonIncreaseSlowly", 0);
            this.m_poisonAffectAir = this.GetBooleanValue("PoisonAffectAir", 0);
            this.m_spawnFirstGroupSize = this.GetIntegerValue("SpawnFirstGroupSize", 0);
            this.m_scaleByTownHall = this.GetBooleanValue("ScaleByTH", 0);
            this.m_pauseCombatComponentMs = this.GetIntegerValue("PauseCombatComponentsMs", 0);
            this.m_damageTHPercent = this.GetIntegerValue("DamageTHPercent", 0);

            if (this.m_damageTHPercent <= 0)
            {
                this.m_damageTHPercent = 100;
            }

            this.m_shrinkReduceSpeedRatio = this.GetIntegerValue("ShrinkReduceSpeedRatio", 0);
            this.m_shrinkHitpointsRatio = this.GetIntegerValue("ShrinkHitpointsRatio", 0);
            this.m_deployEffect2Delay = this.GetIntegerValue("DeployEffect2Delay", 0);
            this.m_hitTimeMS = this.GetIntegerValue("HitTimeMS", 0);
            this.m_deployTimeMS = this.GetIntegerValue("DeployTimeMS", 0);
            this.m_chargingTimeMS = this.GetIntegerValue("ChargingTimeMS", 0);
            this.m_spellForgeLevel = this.GetIntegerValue("SpellForgeLevel", 0) - 1;
            this.m_randomRadiusAffectsOnlyGfx = this.GetBooleanValue("RandomRadiusAffectsOnlyGfx", 0);
            this.m_spawnObstacle = LogicDataTables.GetObstacleByName(this.GetValue("SpawnObstacle", 0), this);
            this.m_numObstacles = this.GetIntegerValue("NumObstacles", 0);
            this.m_troopsOnly = this.GetBooleanValue("TroopsOnly", 0);
            this.m_targetInfoString = this.GetValue("TargetInfoString", 0);

            string preferredTarget = this.GetValue("PreferredTarget", 0);

            if (preferredTarget.Length != 0)
            {
                this.m_preferredTarget = LogicDataTables.GetBuildingClassByName(preferredTarget, null);

                if (this.m_preferredTarget == null)
                {
                    this.m_preferredTarget = LogicDataTables.GetBuildingByName(preferredTarget, null);

                    if (this.m_preferredTarget == null)
                    {
                        this.m_preferredTarget = LogicDataTables.GetCharacterByName(preferredTarget, null);

                        if (this.m_preferredTarget == null)
                        {
                            Debugger.Warning(string.Format("CSV row ({0}) has an invalid reference ({1})", this.GetName(), preferredTarget));
                        }
                    }
                }
            }

            this.m_preferredTargetDamageMod = this.GetIntegerValue("PreferredTargetDamageMod", 0);

            if (this.m_preferredTargetDamageMod == 0)
            {
                this.m_preferredTargetDamageMod = 100;
            }

            this.m_heroDamageMultiplier = this.GetIntegerValue("HeroDamageMultiplier", 0);

            if (this.m_heroDamageMultiplier == 0)
            {
                this.m_heroDamageMultiplier = 100;
            }

            this.m_snapToGrid = this.GetBooleanValue("SnapToGrid", 0);
            this.m_boostDefenders = this.GetBooleanValue("BoostDefenders", 0);
            this.m_boostLinkedToPoison = this.GetBooleanValue("BoostLinkedToPoison", 0);
            this.m_scaleDeployEffects = this.GetBooleanValue("ScaleDeployEffects", 0);
            this.m_summonTroop = LogicDataTables.GetCharacterByName(this.GetValue("SummonTroop", 0), null);
        }

        public override int GetRequiredProductionHouseLevel()
        {
            return this.m_spellForgeLevel;
        }

        public override bool IsUnlockedForProductionHouseLevel(int level)
        {
            return level >= this.m_spellForgeLevel;
        }

        public override LogicBuildingData GetProductionHouseData()
        {
            return LogicDataTables.GetBuildingByName(this.GetUnitOfType() == 1 ? "Spell Forge" : "Mini Spell Factory", null);
        }

        public bool IsDamageSpell()
        {
            return this.m_damage[0] > 0 || this.m_buildingDamagePermil[0] > 0 || this.m_troopDamagePermil[0] > 0 || this.m_poisonDamage[0] > 0;
        }

        public bool IsBuildingDamageSpell()
        {
            return this.m_damage[0] > 0 || this.m_buildingDamagePermil[0] > 0;
        }

        public bool GetRandomRadiusAffectsOnlyGfx()
        {
            return this.m_randomRadiusAffectsOnlyGfx;
        }

        public bool GetPoisonIncreaseSlowly()
        {
            return this.m_poisonIncreaseSlowly;
        }

        public bool GetPoisonAffectAir()
        {
            return this.m_poisonAffectAir;
        }

        public bool IsScaleByTownHall()
        {
            return this.m_scaleByTownHall;
        }

        public bool GetTroopsOnly()
        {
            return this.m_troopsOnly;
        }

        public bool GetSnapToGrid()
        {
            return this.m_snapToGrid;
        }

        public bool GetBoostDefenders()
        {
            return this.m_boostDefenders;
        }

        public bool GetBoostLinkedToPoison()
        {
            return this.m_boostLinkedToPoison;
        }

        public bool GetScaleDeployEffects()
        {
            return this.m_scaleDeployEffects;
        }

        public int GetDamage(int upgLevel)
        {
            return this.m_damage[upgLevel];
        }

        public int GetSpawnFirstGroupSize()
        {
            return this.m_spawnFirstGroupSize;
        }

        public int GetPauseCombatComponentMs()
        {
            return this.m_pauseCombatComponentMs;
        }

        public int GetDamageTHPercent()
        {
            return this.m_damageTHPercent;
        }

        public int GetShrinkReduceSpeedRatio()
        {
            return this.m_shrinkReduceSpeedRatio;
        }

        public int GetShrinkHitpointsRatio()
        {
            return this.m_shrinkHitpointsRatio;
        }

        public int GetDeployEffect2Delay()
        {
            return this.m_deployEffect2Delay;
        }

        public int GetHitTimeMS()
        {
            return this.m_hitTimeMS;
        }

        public int GetDeployTimeMS()
        {
            return this.m_deployTimeMS;
        }

        public int GetChargingTimeMS()
        {
            return this.m_chargingTimeMS;
        }

        public int GetSpellForgeLevel()
        {
            return this.m_spellForgeLevel;
        }

        public int GetNumObstacles()
        {
            return this.m_numObstacles;
        }

        public int GetPreferredTargetDamageMod()
        {
            return this.m_preferredTargetDamageMod;
        }

        public int GetHeroDamageMultiplier()
        {
            return this.m_heroDamageMultiplier;
        }

        public string GetTargetInfoString()
        {
            return this.m_targetInfoString;
        }

        public int GetBuildingDamagePermil(int upgLevel)
        {
            return this.m_buildingDamagePermil[upgLevel];
        }

        public int GetTroopDamagePermil(int upgLevel)
        {
            return this.m_troopDamagePermil[upgLevel];
        }

        public int GetExecuteHealthPermil(int upgLevel)
        {
            return this.m_executeHealthPermil[upgLevel];
        }

        public int GetDamagePermilMin(int upgLevel)
        {
            return this.m_damagePermilMin[upgLevel];
        }

        public int GetPreferredDamagePermilMin(int upgLevel)
        {
            return this.m_preferredDamagePermilMin[upgLevel];
        }

        public int GetPoisonDamage(int upgLevel)
        {
            return this.m_poisonDamage[upgLevel];
        }

        public int GetDamageBoostPercent(int upgLevel)
        {
            return this.m_damageBoostPercent[upgLevel];
        }

        public int GetBuildingDamageBoostPercent(int upgLevel)
        {
            return this.m_buildingDamageBoostPercent[upgLevel];
        }

        public int GetSpeedBoost(int upgLevel)
        {
            return this.m_speedBoost[upgLevel];
        }

        public int GetSpeedBoost2(int upgLevel)
        {
            return this.m_speedBoost2[upgLevel];
        }

        public int GetJumpBoostMS(int upgLevel)
        {
            return this.m_jumpBoostMS[upgLevel];
        }

        public int GetJumpHousingLimit(int upgLevel)
        {
            return this.m_jumpHousingLimit[upgLevel];
        }

        public int GetFreezeTimeMS(int upgLevel)
        {
            return this.m_freezeTimeMS[upgLevel];
        }

        public int GetDuplicateHousing(int upgLevel)
        {
            return this.m_duplicateHousing[upgLevel];
        }

        public int GetDuplicateLifetime(int upgLevel)
        {
            return this.m_duplicateLifetime[upgLevel];
        }

        public int GetUnitsToSpawn(int upgLevel)
        {
            return this.m_unitsToSpawn[upgLevel];
        }

        public int GetSpawnDuration(int upgLevel)
        {
            return this.m_spawnDuration[upgLevel];
        }

        public int GetStrengthWeight(int upgLevel)
        {
            return this.m_strengthWeight[upgLevel];
        }

        public int GetRandomRadius(int upgLevel)
        {
            return this.m_randomRadius[upgLevel];
        }

        public int GetRadius(int upgLevel)
        {
            return this.m_radius[upgLevel];
        }

        public int GetTimeBetweenHitsMS(int upgLevel)
        {
            return this.m_timeBetweenHitsMS[upgLevel];
        }

        public int GetMaxUnitsHit(int upgLevel)
        {
            return this.m_maxUnitsHit[upgLevel];
        }

        public int GetNumberOfHits(int upgLevel)
        {
            return this.m_numberOfHits[upgLevel];
        }

        public int GetExtraHealthPermil(int upgLevel)
        {
            return this.m_extraHealthPermil[upgLevel];
        }

        public int GetExtraHealthMin(int upgLevel)
        {
            return this.m_extraHealthMin[upgLevel];
        }

        public int GetExtraHealthMax(int upgLevel)
        {
            return this.m_extraHealthMax[upgLevel];
        }

        public int GetInvulnerabilityTime(int upgLevel)
        {
            return this.m_invulnerabilityTime[upgLevel];
        }

        public int GetShieldProjectileSpeed(int upgLevel)
        {
            return this.m_shieldProjectileSpeed[upgLevel];
        }

        public int GetShieldProjectileDamageMod(int upgLevel)
        {
            return this.m_shieldProjectileDamageMod[upgLevel];
        }

        public int GetAttackSpeedBoost(int upgLevel)
        {
            return this.m_attackSpeedBoost[upgLevel];
        }

        public int GetBoostTimeMS(int upgLevel)
        {
            return this.m_boostTimeMS[upgLevel];
        }

        public LogicObstacleData GetSpawnObstacle()
        {
            return this.m_spawnObstacle;
        }

        public LogicEffectData GetPreDeployEffect(int upgLevel)
        {
            return this.m_preDeployEffect[upgLevel];
        }

        public LogicEffectData GetDeployEffect(int upgLevel)
        {
            return this.m_deployEffect[upgLevel];
        }

        public LogicEffectData GetEnemyDeployEffect(int upgLevel)
        {
            return this.m_enemyDeployEffect[upgLevel];
        }

        public LogicEffectData GetDeployEffect2(int upgLevel)
        {
            return this.m_deployEffect2[upgLevel];
        }

        public LogicEffectData GetChargingEffect(int upgLevel)
        {
            return this.m_chargingEffect[upgLevel];
        }

        public LogicEffectData GetHitEffect(int upgLevel)
        {
            return this.m_hitEffect[upgLevel];
        }

        public LogicData GetPreferredTarget()
        {
            return this.m_preferredTarget;
        }

        public LogicCharacterData GetSummonTroop()
        {
            return this.m_summonTroop;
        }

        public override int GetCombatItemType()
        {
            return 1;
        }
    }
}