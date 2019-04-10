namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;

    public class LogicAttackerItemData
    {
        private LogicData m_data;
        private CSVRow m_row;

        private int m_index;

        private LogicEffectData m_hitEffectData;
        private LogicEffectData m_hitEffect2Data;
        private LogicEffectData m_hitEffectActiveData;
        private LogicEffectData m_attackEffectData;
        private LogicEffectData m_attackEffect2Data;
        private LogicEffectData m_altAttackEffectData;
        private LogicEffectData m_attackEffectLv2Data;
        private LogicEffectData m_attackEffectLv3Data;
        private LogicEffectData m_transitionEffectLv2Data;
        private LogicEffectData m_transitionEffectLv3Data;
        private LogicEffectData m_preAttackEffectData;
        private LogicEffectData m_becomesTargetableEffectData;
        private LogicEffectData m_hideEffectData;
        private LogicEffectData m_summonEffectData;
        private LogicEffectData m_attackEffectSharedData;
        private LogicProjectileData m_projectileData;
        private LogicProjectileData m_altProjectileData;
        private LogicSpellData m_hitSpellData;
        private LogicData m_preferredTargetData;
        private LogicCharacterData m_summonTroopData;
        private LogicProjectileData m_rageProjectileData;

        private int m_pushBack;
        private int m_damage;
        private int m_attackSpeed;
        private int m_altAttackSpeed;
        private int m_cooldownOverride;
        private int m_prepareSpeed;
        private int m_damageMulti;
        private int m_damageLvl2;
        private int m_damageLvl3;
        private int m_altDamage;
        private int m_attackRange;
        private int m_damageRadius;
        private int m_ammoCount;
        private int m_altNumMultiTargets;
        private int m_switchTimeLv2;
        private int m_switchTimeLv3;
        private int m_statusEffectTime;
        private int m_speedMod;
        private int m_altAttackRange;
        private int m_shockwavePushStrength;
        private int m_hitSpellLevel;
        private int m_damage2;
        private int m_damage2Radius;
        private int m_damage2Delay;
        private int m_damage2Min;
        private int m_damage2FalloffStart;
        private int m_damage2FalloffEnd;
        private int m_alternatePickNewTargetDelay;
        private int m_shockwaveArcLength;
        private int m_shockwaveExpandRadius;
        private int m_targetingConeAngle;
        private int m_penetratingRadius;
        private int m_penetratingExtraRange;
        private int m_targetGroupsRadius;
        private int m_targetGroupsRange;
        private int m_targetGroupsMinWeight;
        private int m_wakeUpSpace;
        private int m_wakeUpSpeed;
        private int m_preferredTargetDamageMod;
        private int m_minAttackRange;
        private int m_summonTroopCount;
        private int m_summonCooldown;
        private int m_summonLimit;
        private int m_projectileBounces;
        private int m_burstCount;
        private int m_burstDelay;
        private int m_altBurstCount;
        private int m_altBurstDelay;
        private int m_dummyProjectileCount;
        private int m_chainAttackDistance;
        private int m_newTargetAttackDelay;

        private bool m_airTargets;
        private bool m_groundTargets;
        private bool m_altAttackMode;
        private bool m_selfAsAoeCenter;
        private bool m_increasingDamage;
        private bool m_preventsHealing;
        private bool m_penetratingProjectile;
        private bool m_targetGroups;
        private bool m_fightWithGroups;
        private bool m_preferredTargetNoTargeting;
        private bool m_altAirTargets;
        private bool m_altGroundTargets;
        private bool m_altMultiTargets;
        private bool m_spawnOnAttack;

        public void CreateReferences(CSVRow row, LogicData data, int idx)
        {
            this.m_row = row;
            this.m_data = data;
            this.m_index = idx;

            this.m_pushBack = row.GetClampedIntegerValue("PushBack", idx);
            this.m_airTargets = row.GetClampedBooleanValue("AirTargets", idx);
            this.m_groundTargets = row.GetClampedBooleanValue("GroundTargets", idx);
            this.m_altAttackMode = row.GetClampedBooleanValue("AltAttackMode", idx);
            this.m_damage = 100 * row.GetClampedIntegerValue("Damage", idx);

            int dps = row.GetClampedIntegerValue("DPS", idx);
            int attackSpeed = row.GetClampedIntegerValue("AttackSpeed", idx);
            int altDps = row.GetClampedIntegerValue("AltDPS", idx);
            int altAttackSpeed = row.GetClampedIntegerValue("AltAttackSpeed", idx);

            if (this.m_altAttackMode && altAttackSpeed == 0)
            {
                altAttackSpeed = attackSpeed;
            }

            int cooldownOverride = row.GetClampedIntegerValue("CoolDownOverride", idx);

            if (cooldownOverride == 0)
            {
                int tmp = (int) (((dps | this.m_damage) >> 31) & 0xFFFFFAEC) + 1500;

                if (attackSpeed > tmp)
                {
                    cooldownOverride = attackSpeed - tmp;
                }
            }

            this.m_prepareSpeed = row.GetClampedIntegerValue("PrepareSpeed", idx);

            this.m_attackSpeed = attackSpeed - cooldownOverride;
            this.m_altAttackSpeed = altAttackSpeed - cooldownOverride;
            this.m_cooldownOverride = cooldownOverride;

            this.m_damageMulti = 100 * row.GetClampedIntegerValue("DamageMulti", idx);
            this.m_damageLvl2 = 100 * row.GetClampedIntegerValue("DamageLv2", idx);
            this.m_damageLvl3 = 100 * row.GetClampedIntegerValue("DamageLv3", idx);

            this.m_altDamage = this.m_damage;

            if (dps != 0)
            {
                if (altDps == 0)
                {
                    altDps = dps;
                }

                this.m_damage = LogicGamePlayUtil.DPSToSingleHit(dps, this.m_attackSpeed + this.m_cooldownOverride);
                this.m_altDamage = LogicGamePlayUtil.DPSToSingleHit(altDps, this.m_altAttackSpeed + this.m_cooldownOverride);
                this.m_damageMulti = LogicGamePlayUtil.DPSToSingleHit(row.GetClampedIntegerValue("DPSMulti", idx), this.m_attackSpeed + this.m_cooldownOverride);
                this.m_damageLvl2 = LogicGamePlayUtil.DPSToSingleHit(row.GetClampedIntegerValue("DPSLv2", idx), this.m_attackSpeed + this.m_cooldownOverride);
                this.m_damageLvl3 = LogicGamePlayUtil.DPSToSingleHit(row.GetClampedIntegerValue("DPSLv3", idx), this.m_attackSpeed + this.m_cooldownOverride);
            }

            this.m_hitEffectData = LogicDataTables.GetEffectByName(row.GetClampedValue("HitEffect", idx), data);
            this.m_hitEffect2Data = LogicDataTables.GetEffectByName(row.GetClampedValue("HitEffect2", idx), data);
            this.m_hitEffectActiveData = LogicDataTables.GetEffectByName(row.GetClampedValue("HitEffectActive", idx), data);

            this.m_attackRange = (row.GetClampedIntegerValue("AttackRange", idx) << 9) / 100;
            this.m_damageRadius = (row.GetClampedIntegerValue("DamageRadius", idx) << 9) / 100;

            this.m_attackEffectData = LogicDataTables.GetEffectByName(row.GetClampedValue("AttackEffect", idx), data);
            this.m_altAttackEffectData = LogicDataTables.GetEffectByName(row.GetClampedValue("AttackEffectAlt", idx), data);
            this.m_ammoCount = row.GetClampedIntegerValue("AmmoCount", idx);
            this.m_attackEffect2Data = LogicDataTables.GetEffectByName(row.GetClampedValue("AttackEffect2", idx), data);
            this.m_attackEffectLv2Data = LogicDataTables.GetEffectByName(row.GetClampedValue("AttackEffectLv2", idx), data);
            this.m_attackEffectLv3Data = LogicDataTables.GetEffectByName(row.GetClampedValue("AttackEffectLv3", idx), data);
            this.m_transitionEffectLv2Data = LogicDataTables.GetEffectByName(row.GetClampedValue("TransitionEffectLv2", idx), data);
            this.m_transitionEffectLv3Data = LogicDataTables.GetEffectByName(row.GetClampedValue("TransitionEffectLv3", idx), data);
            this.m_altNumMultiTargets = row.GetClampedIntegerValue("AltNumMultiTargets", idx);
            this.m_switchTimeLv2 = row.GetClampedIntegerValue("Lv2SwitchTime", idx);
            this.m_switchTimeLv3 = row.GetClampedIntegerValue("Lv3SwitchTime", idx);
            this.m_statusEffectTime = row.GetClampedIntegerValue("StatusEffectTime", idx);
            this.m_speedMod = row.GetClampedIntegerValue("SpeedMod", idx);
            this.m_altAttackRange = (row.GetClampedIntegerValue("AltAttackRange", idx) << 9) / 100;
            this.m_projectileData = LogicDataTables.GetProjectileByName(row.GetClampedValue("Projectile", idx), data);
            this.m_altProjectileData = LogicDataTables.GetProjectileByName(row.GetClampedValue("AltProjectile", idx), data);
            this.m_shockwavePushStrength = row.GetClampedIntegerValue("ShockwavePushStrength", idx);
            this.m_hitSpellData = LogicDataTables.GetSpellByName(row.GetClampedValue("HitSpell", idx), data);
            this.m_hitSpellLevel = row.GetClampedIntegerValue("HitSpellLevel", idx);
            this.m_damage2 = 100 * row.GetClampedIntegerValue("Damage2", idx);
            this.m_damage2Radius = (row.GetClampedIntegerValue("Damage2Radius", idx) << 9) / 100;
            this.m_damage2Delay = row.GetClampedIntegerValue("Damage2Delay", idx);
            this.m_damage2Min = 100 * row.GetClampedIntegerValue("Damage2Min", idx);
            this.m_damage2FalloffStart = (row.GetClampedIntegerValue("Damage2FalloffStart", idx) << 9) / 100;
            this.m_damage2FalloffEnd = (row.GetClampedIntegerValue("Damage2FalloffStart", idx) << 9) / 100;

            if (this.m_damage2FalloffEnd < this.m_damage2FalloffStart)
            {
                Debugger.Error("Building " + row.GetName() + " has falloff end less than falloff start!");
            }

            if (this.m_damage2FalloffEnd > this.m_damage2Radius)
            {
                Debugger.Error("Building " + row.GetName() + " has falloff end greater than the damage radius!");
            }

            this.m_preAttackEffectData = LogicDataTables.GetEffectByName(row.GetClampedValue("PreAttackEffect", idx), data);
            this.m_becomesTargetableEffectData = LogicDataTables.GetEffectByName(row.GetClampedValue("BecomesTargetableEffect", idx), data);
            this.m_increasingDamage = row.GetClampedBooleanValue("IncreasingDamage", idx);
            this.m_preventsHealing = row.GetClampedBooleanValue("PreventsHealing", idx);
            this.m_alternatePickNewTargetDelay = row.GetClampedIntegerValue("AlternatePickNewTargetDelay", idx);
            this.m_shockwaveArcLength = row.GetClampedIntegerValue("ShockwaveArcLength", idx);
            this.m_shockwaveExpandRadius = row.GetClampedIntegerValue("ShockwaveExpandRadius", idx);
            this.m_targetingConeAngle = row.GetClampedIntegerValue("TargetingConeAngle", idx);
            this.m_penetratingProjectile = row.GetClampedBooleanValue("PenetratingProjectile", idx);
            this.m_penetratingRadius = (row.GetClampedIntegerValue("PenetratingRadius", idx) << 9) / 100;
            this.m_penetratingExtraRange = (row.GetClampedIntegerValue("PenetratingExtraRange", idx) << 9) / 100;
            this.m_targetGroups = row.GetClampedBooleanValue("TargetGroups", idx);
            this.m_fightWithGroups = row.GetClampedBooleanValue("FightWithGroups", idx);
            this.m_targetGroupsRadius = (row.GetClampedIntegerValue("TargetGroupsRadius", idx) << 9) / 100;
            this.m_targetGroupsRange = (row.GetClampedIntegerValue("TargetGroupsRange", idx) << 9) / 100;
            this.m_targetGroupsMinWeight = row.GetClampedIntegerValue("TargetGroupsMinWeight", idx);
            this.m_wakeUpSpace = row.GetClampedIntegerValue("WakeUpSpace", idx);
            this.m_wakeUpSpeed = row.GetClampedIntegerValue("WakeUpSpeed", idx);
            this.m_preferredTargetData = LogicDataTables.GetCharacterByName(row.GetClampedValue("PreferredTarget", idx), data);
            this.m_preferredTargetDamageMod = row.GetClampedIntegerValue("PreferredTargetDamageMod", idx);
            this.m_preferredTargetNoTargeting = row.GetClampedBooleanValue("PreferredTargetNoTargeting", idx);
            this.m_altAirTargets = row.GetClampedBooleanValue("AltAirTargets", idx);
            this.m_altGroundTargets = row.GetClampedBooleanValue("AltGroundTargets", idx);
            this.m_altMultiTargets = row.GetClampedBooleanValue("AltMultiTargets", idx);
            this.m_minAttackRange = (row.GetClampedIntegerValue("MinAttackRange", idx) << 9) / 100;

            if (this.m_preferredTargetData == null)
            {
                this.m_preferredTargetData = LogicDataTables.GetBuildingClassByName(row.GetClampedValue("PreferedTargetBuildingClass", idx), data);

                if (this.m_preferredTargetData == null)
                {
                    this.m_preferredTargetData = LogicDataTables.GetBuildingByName(row.GetClampedValue("PreferedTargetBuilding", idx), data);
                }

                this.m_preferredTargetDamageMod = row.GetClampedIntegerValue("PreferedTargetDamageMod", idx);

                if (this.m_preferredTargetDamageMod == 0)
                {
                    this.m_preferredTargetDamageMod = 100;
                }
            }

            this.m_summonTroopCount = row.GetClampedIntegerValue("SummonTroopCount", idx);
            this.m_summonCooldown = row.GetClampedIntegerValue("SummonCooldown", idx);
            this.m_summonEffectData = LogicDataTables.GetEffectByName(row.GetClampedValue("SummonEffect", idx), data);
            this.m_summonLimit = row.GetClampedIntegerValue("SummonLimit", idx);
            this.m_summonTroopData = LogicDataTables.GetCharacterByName(row.GetClampedValue("SummonTroop", idx), data);
            this.m_spawnOnAttack = row.GetClampedBooleanValue("SpawnOnAttack", idx);
            this.m_hideEffectData = LogicDataTables.GetEffectByName(row.GetClampedValue("HideEffect", idx), data);
            this.m_rageProjectileData = LogicDataTables.GetProjectileByName(row.GetClampedValue("RageProjectile", idx), data);
            this.m_projectileBounces = row.GetClampedIntegerValue("ProjectileBounces", idx);
            this.m_selfAsAoeCenter = row.GetClampedBooleanValue("SelfAsAoeCenter", idx);

            if (this.m_damage2Delay > this.m_cooldownOverride + this.m_attackSpeed)
            {
                Debugger.Error(row.GetName() + " has Damage2Delay greater than the attack speed!");
            }

            if (this.m_ammoCount > 0 && (this.m_attackSpeed & 63) != 0)
            {
                Debugger.Error(string.Format("Invalid attack speed {0} (must be multiple of 64)", this.m_attackSpeed));
            }

            this.m_burstCount = row.GetClampedIntegerValue("BurstCount", idx);
            this.m_burstDelay = row.GetClampedIntegerValue("BurstDelay", idx);
            this.m_altBurstCount = row.GetClampedIntegerValue("AltBurstCount", idx);
            this.m_altBurstDelay = row.GetClampedIntegerValue("AltBurstDelay", idx);
            this.m_dummyProjectileCount = row.GetClampedIntegerValue("DummyProjectileCount", idx);
            this.m_attackEffectSharedData = LogicDataTables.GetEffectByName(row.GetClampedValue("AttackEffectShared", idx), data);
            this.m_chainAttackDistance = row.GetClampedIntegerValue("ChainAttackDistance", idx);
            this.m_newTargetAttackDelay = row.GetClampedIntegerValue("NewTargetAttackDelay", idx);

            if (this.m_newTargetAttackDelay > 0)
            {
                this.m_newTargetAttackDelay = LogicMath.Clamp(attackSpeed - this.m_newTargetAttackDelay, 0, attackSpeed);
            }
        }

        public int GetDamage(int type, bool multi)
        {
            if (multi)
            {
                return this.m_damageMulti;
            }

            Debugger.DoAssert(type < 3, string.Empty);

            switch (type)
            {
                case 1:
                    return this.m_damageLvl2;
                case 2:
                    return this.m_damageLvl3;
                default:
                    return this.m_damage;
            }
        }

        public int GetAltDamage(int level, bool alt)
        {
            Debugger.DoAssert(level < 3, string.Empty);

            if (alt)
            {
                return this.m_damageMulti;
            }

            return this.m_altDamage;
        }

        public int GetDamagePerMS(int type, bool multi)
        {
            int damage = this.GetDamage(type, multi);
            int cooldown = this.m_attackSpeed + this.m_cooldownOverride;

            if (cooldown != 0)
            {
                return 1000 * damage / (100 * cooldown);
            }

            return 0;
        }

        public int GetDamage2()
        {
            return this.m_damage2;
        }

        public int GetDamage2Delay()
        {
            return this.m_damage2Delay;
        }

        public int GetAttackCooldownOverride()
        {
            return this.m_cooldownOverride;
        }

        public int GetShockwavePushStrength()
        {
            return this.m_shockwavePushStrength;
        }

        public LogicSpellData GetHitSpell()
        {
            return this.m_hitSpellData;
        }

        public bool HasAlternativeAttackMode()
        {
            return this.m_altAttackMode;
        }

        public int GetAmmoCount()
        {
            return this.m_ammoCount;
        }

        public int GetTargetingConeAngle()
        {
            return this.m_targetingConeAngle;
        }

        public LogicData GetPreferredTargetData()
        {
            return this.m_preferredTargetData;
        }

        public LogicEffectData GetHitEffect()
        {
            return this.m_hitEffectData;
        }

        public LogicEffectData GetHitEffect2()
        {
            return this.m_hitEffect2Data;
        }

        public LogicEffectData GetHitEffectActive()
        {
            return this.m_hitEffectActiveData;
        }

        public LogicEffectData GetAttackEffect()
        {
            return this.m_attackEffectData;
        }

        public LogicEffectData GetAttackEffect2()
        {
            return this.m_attackEffect2Data;
        }

        public LogicEffectData GetAltAttackEffect()
        {
            return this.m_altAttackEffectData;
        }

        public LogicEffectData GetAttackEffectLv2()
        {
            return this.m_attackEffectLv2Data;
        }

        public LogicEffectData GetAttackEffectLv3()
        {
            return this.m_attackEffectLv3Data;
        }

        public LogicEffectData GetTransitionEffectLv2()
        {
            return this.m_transitionEffectLv2Data;
        }

        public LogicEffectData GetTransitionEffectLv3()
        {
            return this.m_transitionEffectLv3Data;
        }

        public LogicEffectData GetPreAttackEffect()
        {
            return this.m_preAttackEffectData;
        }

        public LogicEffectData GetBecomesTargetableEffect()
        {
            return this.m_becomesTargetableEffectData;
        }

        public LogicEffectData GetHideEffect()
        {
            return this.m_hideEffectData;
        }

        public LogicEffectData GetSummonEffect()
        {
            return this.m_summonEffectData;
        }

        public LogicEffectData GetAttackEffectShared()
        {
            return this.m_attackEffectSharedData;
        }

        public LogicProjectileData GetProjectile(bool alt)
        {
            if (alt)
            {
                return this.m_altProjectileData;
            }

            return this.m_projectileData;
        }

        public LogicCharacterData GetSummonTroop()
        {
            return this.m_summonTroopData;
        }

        public LogicProjectileData GetRageProjectile()
        {
            return this.m_rageProjectileData;
        }

        public int GetPushBack()
        {
            return this.m_pushBack;
        }

        public int GetAttackSpeed()
        {
            return this.m_attackSpeed;
        }

        public int GetAltAttackSpeed()
        {
            return this.m_altAttackSpeed;
        }

        public int GetPrepareSpeed()
        {
            return this.m_prepareSpeed;
        }

        public int GetAttackRange(bool alt)
        {
            if (alt)
            {
                return this.m_altAttackRange;
            }

            return this.m_attackRange;
        }

        public int GetDamageRadius()
        {
            return this.m_damageRadius;
        }

        public int GetDamage2Radius()
        {
            return this.m_damage2Radius;
        }

        public int GetAltNumMultiTargets()
        {
            return this.m_altNumMultiTargets;
        }

        public int GetSwitchTimeLv2()
        {
            return this.m_switchTimeLv2;
        }

        public int GetSwitchTimeLv3()
        {
            return this.m_switchTimeLv3;
        }

        public int GetStatusEffectTime()
        {
            return this.m_statusEffectTime;
        }

        public int GetSpeedMod()
        {
            return this.m_speedMod;
        }

        public int GetHitSpellLevel()
        {
            return this.m_hitSpellLevel;
        }

        public int GetDamage2Min()
        {
            return this.m_damage2Min;
        }

        public int GetAlternatePickNewTargetDelay()
        {
            return this.m_alternatePickNewTargetDelay;
        }

        public int GetShockwaveArcLength()
        {
            return this.m_shockwaveArcLength;
        }

        public int GetShockwaveExpandRadius()
        {
            return this.m_shockwaveExpandRadius;
        }

        public int GetPenetratingRadius()
        {
            return this.m_penetratingRadius;
        }

        public int GetPenetratingExtraRange()
        {
            return this.m_penetratingExtraRange;
        }

        public int GetTargetGroupsRadius()
        {
            return this.m_targetGroupsRadius;
        }

        public int GetTargetGroupsRange()
        {
            return this.m_targetGroupsRange;
        }

        public int GetTargetGroupsMinWeight()
        {
            return this.m_targetGroupsMinWeight;
        }

        public int GetWakeUpSpace()
        {
            return this.m_wakeUpSpace;
        }

        public int GetWakeUpSpeed()
        {
            return this.m_wakeUpSpeed;
        }

        public int GetPreferredTargetDamageMod()
        {
            return this.m_preferredTargetDamageMod;
        }

        public int GetMinAttackRange()
        {
            return this.m_minAttackRange;
        }

        public int GetSummonTroopCount()
        {
            return this.m_summonTroopCount;
        }

        public int GetSummonCooldown()
        {
            return this.m_summonCooldown;
        }

        public int GetSummonLimit()
        {
            return this.m_summonLimit;
        }

        public int GetProjectileBounces()
        {
            return this.m_projectileBounces;
        }

        public int GetBurstCount()
        {
            return this.m_burstCount;
        }

        public int GetBurstDelay()
        {
            return this.m_burstDelay;
        }

        public int GetAltBurstCount()
        {
            return this.m_altBurstCount;
        }

        public int GetAltBurstDelay()
        {
            return this.m_altBurstDelay;
        }

        public int GetDummyProjectileCount()
        {
            return this.m_dummyProjectileCount;
        }

        public int GetChainAttackDistance()
        {
            return this.m_chainAttackDistance;
        }

        public int GetNewTargetAttackDelay()
        {
            return this.m_newTargetAttackDelay;
        }

        public bool GetTrackAirTargets(bool alt)
        {
            if (alt)
            {
                return this.m_altAirTargets;
            }

            return this.m_airTargets;
        }

        public bool GetTrackGroundTargets(bool alt)
        {
            if (alt)
            {
                return this.m_groundTargets;
            }

            return this.m_groundTargets;
        }

        public bool IsSelfAsAoeCenter()
        {
            return this.m_selfAsAoeCenter;
        }

        public bool IsIncreasingDamage()
        {
            return this.m_increasingDamage;
        }

        public bool GetPreventsHealing()
        {
            return this.m_preventsHealing;
        }

        public bool IsPenetratingProjectile()
        {
            return this.m_penetratingProjectile;
        }

        public bool GetTargetGroups()
        {
            return this.m_targetGroups;
        }

        public bool GetFightWithGroups()
        {
            return this.m_fightWithGroups;
        }

        public bool GetPreferredTargetNoTargeting()
        {
            return this.m_preferredTargetNoTargeting;
        }

        public bool GetSpawnOnAttack()
        {
            return this.m_spawnOnAttack;
        }

        public bool GetMultiTargets(bool alt)
        {
            if (alt)
            {
                return this.m_altMultiTargets;
            }

            return false;
        }

        public LogicAttackerItemData Clone()
        {
            LogicAttackerItemData cloned = new LogicAttackerItemData();
            cloned.CreateReferences(this.m_row, this.m_data, this.m_index);
            return cloned;
        }

        public void AddAttackRange(int value)
        {
            this.m_attackRange += value;
            this.m_altAttackRange += value;
        }
    }
}