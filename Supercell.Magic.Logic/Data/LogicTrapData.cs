namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;

    public class LogicTrapData : LogicGameObjectData
    {
        private bool m_enableByCalendar;
        private bool m_hasAltMode;
        private bool m_ejectVictims;
        private bool m_doNotScalePushByDamage;
        private bool m_airTrigger;
        private bool m_groundTrigger;
        private bool m_healerTrigger;

        private int m_upgradeLevelCount;
        private int m_preferredTargetDamageMod;
        private int m_minTriggerHousingLimit;
        private int m_timeBetweenSpawnsMS;
        private int m_spawnInitialDelayMS;
        private int m_throwDistance;
        private int m_triggerRadius;
        private int m_directionCount;
        private int m_actionFrame;
        private int m_pushback;
        private int m_speedMod;
        private int m_damageMod;
        private int m_durationMS;
        private int m_hitDelayMS;
        private int m_hitCount;

        private int[] m_constructionTimes;
        private int[] m_townHallLevel;
        private int[] m_buildCost;
        private int[] m_rearmCost;
        private int[] m_strenghtWeight;
        private int[] m_damage;
        private int[] m_damageRadius;
        private int[] m_ejectHousingLimit;
        private int[] m_numSpawns;

        private int m_width;
        private int m_height;

        private LogicProjectileData[] m_projectileData;

        private LogicSpellData m_spell;
        private LogicEffectData m_effectData;
        private LogicEffectData m_effect2Data;
        private LogicEffectData m_effectBrokenData;
        private LogicEffectData m_damageEffectData;
        private LogicEffectData m_pickUpEffectData;
        private LogicEffectData m_placingEffectData;
        private LogicEffectData m_appearEffectData;
        private LogicEffectData m_toggleAttackModeEffectData;
        private LogicCharacterData m_preferredTargetData;
        private LogicCharacterData m_spawnedCharGroundData;
        private LogicCharacterData m_spawnedCharAirData;
        private LogicResourceData m_buildResourceData;

        public LogicTrapData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicTrapData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_width = this.GetIntegerValue("Width", 0);
            this.m_height = this.GetIntegerValue("Height", 0);

            this.m_upgradeLevelCount = this.m_row.GetBiggestArraySize();

            this.m_buildCost = new int[this.m_upgradeLevelCount];
            this.m_rearmCost = new int[this.m_upgradeLevelCount];
            this.m_townHallLevel = new int[this.m_upgradeLevelCount];
            this.m_strenghtWeight = new int[this.m_upgradeLevelCount];
            this.m_damage = new int[this.m_upgradeLevelCount];
            this.m_damageRadius = new int[this.m_upgradeLevelCount];
            this.m_ejectHousingLimit = new int[this.m_upgradeLevelCount];
            this.m_numSpawns = new int[this.m_upgradeLevelCount];
            this.m_constructionTimes = new int[this.m_upgradeLevelCount];

            this.m_projectileData = new LogicProjectileData[this.m_upgradeLevelCount];

            for (int i = 0; i < this.m_upgradeLevelCount; i++)
            {
                this.m_buildCost[i] = this.GetClampedIntegerValue("BuildCost", i);
                this.m_rearmCost[i] = this.GetClampedIntegerValue("RearmCost", i);
                this.m_townHallLevel[i] = LogicMath.Max(this.GetClampedIntegerValue("TownHallLevel", i) - 1, 0);
                this.m_strenghtWeight[i] = this.GetClampedIntegerValue("StrengthWeight", i);
                this.m_damage[i] = LogicGamePlayUtil.DPSToSingleHit(this.GetClampedIntegerValue("Damage", i), 1000);
                this.m_damageRadius[i] = (this.GetClampedIntegerValue("DamageRadius", i) << 9) / 100;
                this.m_ejectHousingLimit[i] = this.GetIntegerValue("EjectHousingLimit", i);
                this.m_numSpawns[i] = this.GetClampedIntegerValue("NumSpawns", i);
                this.m_constructionTimes[i] = 86400 * this.GetClampedIntegerValue("BuildTimeD", i) +
                                              3600 * this.GetClampedIntegerValue("BuildTimeH", i) +
                                              60 * this.GetClampedIntegerValue("BuildTimeM", i) +
                                              this.GetClampedIntegerValue("BuildTimeS", i);
                this.m_projectileData[i] = LogicDataTables.GetProjectileByName(this.GetValue("Projectile", i), this);
            }

            this.m_preferredTargetData = LogicDataTables.GetCharacterByName(this.GetValue("PreferredTarget", 0), this);
            this.m_preferredTargetDamageMod = this.GetIntegerValue("PreferredTargetDamageMod", 0);

            if (this.m_preferredTargetDamageMod == 0)
            {
                this.m_preferredTargetDamageMod = 100;
            }

            this.m_buildResourceData = LogicDataTables.GetResourceByName(this.GetValue("BuildResource", 0), this);

            if (this.m_buildResourceData == null)
            {
                Debugger.Error("build resource is not defined for trap: " + this.GetName());
            }

            this.m_ejectVictims = this.GetBooleanValue("EjectVictims", 0);
            this.m_actionFrame = 1000 * this.GetIntegerValue("ActionFrame", 0) / 24;
            this.m_pushback = this.GetIntegerValue("Pushback", 0);
            this.m_doNotScalePushByDamage = this.GetBooleanValue("DoNotScalePushByDamage", 0);
            this.m_effectData = LogicDataTables.GetEffectByName(this.GetValue("Effect", 0), this);
            this.m_effect2Data = LogicDataTables.GetEffectByName(this.GetValue("Effect2", 0), this);
            this.m_effectBrokenData = LogicDataTables.GetEffectByName(this.GetValue("EffectBroken", 0), this);
            this.m_damageEffectData = LogicDataTables.GetEffectByName(this.GetValue("DamageEffect", 0), this);
            this.m_pickUpEffectData = LogicDataTables.GetEffectByName(this.GetValue("PickUpEffect", 0), this);
            this.m_placingEffectData = LogicDataTables.GetEffectByName(this.GetValue("PlacingEffect", 0), this);
            this.m_appearEffectData = LogicDataTables.GetEffectByName(this.GetValue("AppearEffect", 0), this);
            this.m_toggleAttackModeEffectData = LogicDataTables.GetEffectByName(this.GetValue("ToggleAttackModeEffect", 0), this);
            this.m_triggerRadius = (this.GetIntegerValue("TriggerRadius", 0) << 9) / 100;
            this.m_directionCount = this.GetIntegerValue("DirectionCount", 0);
            this.m_spell = LogicDataTables.GetSpellByName(this.GetValue("Spell", 0), this);
            this.m_airTrigger = this.GetBooleanValue("AirTrigger", 0);
            this.m_groundTrigger = this.GetBooleanValue("GroundTrigger", 0);
            this.m_healerTrigger = this.GetBooleanValue("HealerTrigger", 0);
            this.m_speedMod = this.GetIntegerValue("SpeedMod", 0);
            this.m_damageMod = this.GetIntegerValue("DamageMod", 0);
            this.m_durationMS = this.GetIntegerValue("DurationMS", 0);
            this.m_hitDelayMS = this.GetIntegerValue("HitDelayMS", 0);
            this.m_hitCount = this.GetIntegerValue("HitCnt", 0);
            this.m_minTriggerHousingLimit = this.GetIntegerValue("MinTriggerHousingLimit", 0);
            this.m_spawnedCharGroundData = LogicDataTables.GetCharacterByName(this.GetValue("SpawnedCharGround", 0), this);
            this.m_spawnedCharAirData = LogicDataTables.GetCharacterByName(this.GetValue("SpawnedCharAir", 0), this);
            this.m_timeBetweenSpawnsMS = this.GetIntegerValue("TimeBetweenSpawnsMs", 0);
            this.m_spawnInitialDelayMS = this.GetIntegerValue("SpawnInitialDelayMs", 0);
            this.m_throwDistance = this.GetIntegerValue("ThrowDistance", 0);
            this.m_hasAltMode = this.GetBooleanValue("HasAltMode", 0);
            this.m_enableByCalendar = this.GetBooleanValue("EnabledByCalendar", 0);

            if (this.m_enableByCalendar)
            {
                if (this.m_upgradeLevelCount > 1)
                {
                    Debugger.Error("Temporary traps should not have upgrade levels!");
                }
            }
        }

        public int GetWidth()
        {
            return this.m_width;
        }

        public int GetHeight()
        {
            return this.m_height;
        }

        public int GetUpgradeLevelCount()
        {
            return this.m_upgradeLevelCount;
        }

        public int GetSpawnInitialDelayMS()
        {
            return this.m_spawnInitialDelayMS;
        }

        public int GetNumSpawns(int upgLevel)
        {
            return this.m_numSpawns[upgLevel];
        }

        public int GetBuildTime(int upgLevel)
        {
            return this.m_constructionTimes[upgLevel];
        }

        public LogicResourceData GetBuildResource()
        {
            return this.m_buildResourceData;
        }

        public int GetBuildCost(int upgLevel)
        {
            return this.m_buildCost[upgLevel];
        }

        public int GetRequiredTownHallLevel(int upgLevel)
        {
            return this.m_townHallLevel[upgLevel];
        }

        public LogicCharacterData GetSpawnedCharAir()
        {
            return this.m_spawnedCharAirData;
        }

        public LogicCharacterData GetSpawnedCharGround()
        {
            return this.m_spawnedCharGroundData;
        }

        public bool HasAlternativeMode()
        {
            return this.m_hasAltMode;
        }

        public int GetThrowDistance()
        {
            return this.m_throwDistance;
        }

        public int GetDirectionCount()
        {
            return this.m_directionCount;
        }

        public int GetDamage(int idx)
        {
            return this.m_damage[idx];
        }

        public int GetDamageRadius(int idx)
        {
            return this.m_damageRadius[idx];
        }

        public override bool IsEnableByCalendar()
        {
            return this.m_enableByCalendar;
        }

        public int GetRearmCost(int idx)
        {
            return this.m_rearmCost[idx];
        }

        public int GetStrengthWeight(int idx)
        {
            return this.m_strenghtWeight[idx];
        }

        public int GetEjectHousingLimit(int idx)
        {
            return this.m_ejectHousingLimit[idx];
        }

        public int GetPushback()
        {
            return this.m_pushback;
        }

        public int GetMaxUpgradeLevelForTownHallLevel(int townHallLevel)
        {
            int count = this.m_upgradeLevelCount;

            while (count > 0)
            {
                if (this.GetRequiredTownHallLevel(--count) <= townHallLevel)
                {
                    return count;
                }
            }

            return -1;
        }

        public bool GetEjectVictims()
        {
            return this.m_ejectVictims;
        }

        public bool GetDoNotScalePushByDamage()
        {
            return this.m_doNotScalePushByDamage;
        }

        public bool GetAirTrigger()
        {
            return this.m_airTrigger;
        }

        public bool GetGroundTrigger()
        {
            return this.m_groundTrigger;
        }

        public bool GetHealerTrigger()
        {
            return this.m_healerTrigger;
        }

        public int GetMinTriggerHousingLimit()
        {
            return this.m_minTriggerHousingLimit;
        }

        public int GetTimeBetweenSpawnsMS()
        {
            return this.m_timeBetweenSpawnsMS;
        }

        public int GetTriggerRadius()
        {
            return this.m_triggerRadius;
        }

        public int GetActionFrame()
        {
            return this.m_actionFrame;
        }

        public int GetSpeedMod()
        {
            return this.m_speedMod;
        }

        public int GetDamageMod()
        {
            return this.m_damageMod;
        }

        public int GetDurationMS()
        {
            return this.m_durationMS;
        }

        public int GetHitDelayMS()
        {
            return this.m_hitDelayMS;
        }

        public int GetHitCount()
        {
            return this.m_hitCount;
        }

        public LogicSpellData GetSpell()
        {
            return this.m_spell;
        }

        public LogicProjectileData GetProjectile(int idx)
        {
            return this.m_projectileData[idx];
        }

        public LogicEffectData GetEffect()
        {
            return this.m_effectData;
        }

        public LogicEffectData GetEffect2()
        {
            return this.m_effect2Data;
        }

        public LogicEffectData GetEffectBroken()
        {
            return this.m_effectBrokenData;
        }

        public LogicEffectData GetDamageEffect()
        {
            return this.m_damageEffectData;
        }

        public LogicEffectData GetPickUpEffect()
        {
            return this.m_pickUpEffectData;
        }

        public LogicEffectData GetPlacingEffect()
        {
            return this.m_placingEffectData;
        }

        public LogicEffectData GetAppearEffect()
        {
            return this.m_appearEffectData;
        }

        public LogicEffectData GetToggleAttackModeEffect()
        {
            return this.m_toggleAttackModeEffectData;
        }

        public LogicCharacterData GetPreferredTarget()
        {
            return this.m_preferredTargetData;
        }

        public int GetPreferredTargetDamageMod()
        {
            return this.m_preferredTargetDamageMod;
        }
    }
}