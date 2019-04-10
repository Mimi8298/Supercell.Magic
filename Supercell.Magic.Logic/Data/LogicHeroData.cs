namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Math;

    public class LogicHeroData : LogicCharacterData
    {
        private bool m_noDefence;
        private bool m_abilityOnce;
        private bool m_abilityStealth;
        private bool m_abilityAffectsHero;
        private bool m_hasAltMode;
        private bool m_altModeFlying;

        private int m_maxSearchRadiusForDefender;
        private int m_abilityCooldown;
        private int m_activationTime;
        private int m_activeDuration;
        private int m_alertRadius;
        private int m_abilityRadius;
        private int m_sleepOffsetX;
        private int m_sleepOffsetY;
        private int m_patrolRadius;
        private int m_abilityDelay;

        private int[] m_abilityTime;
        private int[] m_abilitySpeedBoost;
        private int[] m_abilitySpeedBoost2;
        private int[] m_abilityDamageBoostPercent;
        private int[] m_abilitySummonTroopCount;
        private int[] m_abilityHealthIncrease;
        private int[] m_abilityShieldProjectileSpeed;
        private int[] m_abilityShieldProjectileDamageMod;
        private int[] m_abilitySpellLevel;
        private int[] m_abilityDamageBoostOffset;
        private int[] m_abilityDamageBoost;
        private int[] m_regenerationTimeSecs;
        private int[] m_requiredTownHallLevel;
        private int[] m_strengthWeight2;

        private string m_smallPictureSWF;
        private string m_smallPicture;
        private string m_abilityTID;
        private string m_abilityDescTID;
        private string m_abilityIcon;
        private string m_abilityBigPictureExportName;

        private LogicCharacterData m_abilityAffectsCharacterData;
        private LogicCharacterData m_abilitySummonTroopData;
        private LogicEffectData m_abilityTriggerEffectData;
        private LogicEffectData m_specialAbilityEffectData;
        private LogicEffectData m_celebreateEffectData;

        private LogicSpellData[] m_abilitySpellData;

        public LogicHeroData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicHeroData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_maxSearchRadiusForDefender = (this.GetIntegerValue("MaxSearchRadiusForDefender", 0) << 9) / 100;

            int biggestArraySize = this.m_row.GetBiggestArraySize();

            this.m_abilitySpellData = new LogicSpellData[biggestArraySize];
            this.m_regenerationTimeSecs = new int[biggestArraySize];
            this.m_requiredTownHallLevel = new int[biggestArraySize];
            this.m_abilityTime = new int[biggestArraySize];
            this.m_abilitySpeedBoost = new int[biggestArraySize];
            this.m_abilitySpeedBoost2 = new int[biggestArraySize];
            this.m_abilityDamageBoostPercent = new int[biggestArraySize];
            this.m_abilitySummonTroopCount = new int[biggestArraySize];
            this.m_abilityHealthIncrease = new int[biggestArraySize];
            this.m_abilityShieldProjectileSpeed = new int[biggestArraySize];
            this.m_abilityShieldProjectileDamageMod = new int[biggestArraySize];
            this.m_abilitySpellLevel = new int[biggestArraySize];
            this.m_abilityDamageBoostOffset = new int[biggestArraySize];
            this.m_abilityDamageBoost = new int[biggestArraySize];
            this.m_strengthWeight2 = new int[biggestArraySize];

            for (int i = 0; i < biggestArraySize; i++)
            {
                this.m_regenerationTimeSecs[i] = 60 * this.GetClampedIntegerValue("RegenerationTimeMinutes", i);
                this.m_requiredTownHallLevel[i] = this.GetClampedIntegerValue("RequiredTownHallLevel", i) - 1;
                this.m_abilityTime[i] = this.GetClampedIntegerValue("AbilityTime", i);
                this.m_abilitySpeedBoost[i] = this.GetClampedIntegerValue("AbilitySpeedBoost", i);
                this.m_abilitySpeedBoost2[i] = this.GetClampedIntegerValue("AbilitySpeedBoost2", i);
                this.m_abilityDamageBoostPercent[i] = this.GetClampedIntegerValue("AbilityDamageBoostPercent", i);
                this.m_abilitySummonTroopCount[i] = this.GetClampedIntegerValue("AbilitySummonTroopCount", i);
                this.m_abilityHealthIncrease[i] = this.GetClampedIntegerValue("AbilityHealthIncrease", i);
                this.m_abilityShieldProjectileSpeed[i] = this.GetClampedIntegerValue("AbilityShieldProjectileSpeed", i);
                this.m_abilityShieldProjectileDamageMod[i] = this.GetClampedIntegerValue("AbilityShieldProjectileDamageMod", i);
                this.m_abilitySpellData[i] = LogicDataTables.GetSpellByName(this.GetClampedValue("AbilitySpell", i), this);
                this.m_abilitySpellLevel[i] = this.GetClampedIntegerValue("AbilitySpellLevel", i);
                this.m_abilityDamageBoostOffset[i] = this.GetClampedIntegerValue("AbilityDamageBoostOffset", i);

                int damage = this.m_attackerItemData[i].GetDamagePerMS(0, false);
                int offset = this.m_abilityDamageBoostOffset[i];

                this.m_abilityDamageBoost[i] = (100 * (damage + offset) + damage / 2) / damage - 100;
                this.m_strengthWeight2[i] = this.GetClampedIntegerValue("StrengthWeight2", i);
            }

            this.m_alertRadius = (this.GetIntegerValue("AlertRadius", 0) << 9) / 100;
            this.m_abilityStealth = this.GetBooleanValue("AbilityStealth", 0);
            this.m_abilityRadius = this.GetIntegerValue("AbilityRadius", 0);
            this.m_abilityAffectsHero = this.GetBooleanValue("AbilityAffectsHero", 0);
            this.m_abilityAffectsCharacterData = LogicDataTables.GetCharacterByName(this.GetValue("AbilityAffectsCharacter", 0), this);
            this.m_abilityTriggerEffectData = LogicDataTables.GetEffectByName(this.GetValue("AbilityTriggerEffect", 0), this);
            this.m_abilityOnce = this.GetBooleanValue("AbilityOnce", 0);
            this.m_abilityCooldown = this.GetIntegerValue("AbilityCooldown", 0);
            this.m_abilitySummonTroopData = LogicDataTables.GetCharacterByName(this.GetValue("AbilitySummonTroop", 0), this);
            this.m_specialAbilityEffectData = LogicDataTables.GetEffectByName(this.GetValue("SpecialAbilityEffect", 0), this);
            this.m_celebreateEffectData = LogicDataTables.GetEffectByName(this.GetValue("CelebrateEffect", 0), this);
            this.m_sleepOffsetX = (this.GetIntegerValue("SleepOffsetX", 0) << 9) / 100;
            this.m_sleepOffsetY = (this.GetIntegerValue("SleepOffsetY", 0) << 9) / 100;
            this.m_patrolRadius = (this.GetIntegerValue("PatrolRadius", 0) << 9) / 100;
            this.m_smallPictureSWF = this.GetValue("SmallPictureSWF", 0);
            this.m_smallPicture = this.GetValue("SmallPicture", 0);
            this.m_abilityTID = this.GetValue("AbilityTID", 0);
            this.m_abilityDescTID = this.GetValue("AbilityDescTID", 0);
            this.m_abilityIcon = this.GetValue("AbilityIcon", 0);
            this.m_abilityDelay = this.GetIntegerValue("AbilityDelay", 0);
            this.m_abilityBigPictureExportName = this.GetValue("AbilityBigPictureExportName", 0);
            this.m_hasAltMode = this.GetBooleanValue("HasAltMode", 0);
            this.m_altModeFlying = this.GetBooleanValue("AltModeFlying", 0);
            this.m_noDefence = this.GetBooleanValue("NoDefence", 0);
            this.m_activationTime = this.GetIntegerValue("ActivationTime", 0);
            this.m_activeDuration = this.GetIntegerValue("ActiveDuration", 0);
        }

        public bool GetAbilityStealth()
        {
            return this.m_abilityStealth;
        }

        public bool GetAbilityAffectsHero()
        {
            return this.m_abilityAffectsHero;
        }

        public int GetAbilityHealthIncrease(int upgLevel)
        {
            return this.m_abilityHealthIncrease[upgLevel];
        }

        public bool HasHasAltMode()
        {
            return this.m_hasAltMode;
        }

        public bool GetAltModeFlying()
        {
            return this.m_altModeFlying;
        }

        public int GetMaxSearchRadiusForDefender()
        {
            return this.m_maxSearchRadiusForDefender;
        }

        public int GetActivationTime()
        {
            return this.m_activationTime;
        }

        public int GetActiveDuration()
        {
            return this.m_activeDuration;
        }

        public int GetAlertRadius()
        {
            return this.m_alertRadius;
        }

        public int GetAbilityRadius()
        {
            return this.m_abilityRadius;
        }

        public int GetSleepOffsetX()
        {
            return this.m_sleepOffsetX;
        }

        public int GetSleepOffsetY()
        {
            return this.m_sleepOffsetY;
        }

        public int GetPatrolRadius()
        {
            return this.m_patrolRadius;
        }

        public int GetAbilityDelay()
        {
            return this.m_abilityDelay;
        }

        public string GetSmallPictureSWF()
        {
            return this.m_smallPictureSWF;
        }

        public string GetSmallPicture()
        {
            return this.m_smallPicture;
        }

        public string GetAbilityTID()
        {
            return this.m_abilityTID;
        }

        public string GetAbilityDescTID()
        {
            return this.m_abilityDescTID;
        }

        public string GetAbilityIcon()
        {
            return this.m_abilityIcon;
        }

        public string GetAbilityBigPictureExportName()
        {
            return this.m_abilityBigPictureExportName;
        }

        public LogicCharacterData GetAbilityAffectsCharacter()
        {
            return this.m_abilityAffectsCharacterData;
        }

        public LogicCharacterData GetAbilitySummonTroop()
        {
            return this.m_abilitySummonTroopData;
        }

        public LogicEffectData GetAbilityTriggerEffect()
        {
            return this.m_abilityTriggerEffectData;
        }

        public LogicEffectData GetSpecialAbilityEffect()
        {
            return this.m_specialAbilityEffectData;
        }

        public LogicEffectData GetCelebreateEffect()
        {
            return this.m_celebreateEffectData;
        }

        public LogicSpellData GetAbilitySpell(int upgLevel)
        {
            return this.m_abilitySpellData[upgLevel];
        }

        public int GetAbilitySpellLevel(int upgLevel)
        {
            return this.m_abilitySpellLevel[upgLevel];
        }

        public int GetRequiredTownHallLevel(int index)
        {
            return this.m_requiredTownHallLevel[index];
        }

        public int GetAbilityShieldProjectileSpeed(int upgLevel)
        {
            return this.m_abilityShieldProjectileSpeed[upgLevel];
        }

        public int GetAbilityShieldProjectileDamageMod(int upgLevel)
        {
            return this.m_abilityShieldProjectileDamageMod[upgLevel];
        }

        public int GetAbilityTime(int index)
        {
            return this.m_abilityTime[index];
        }

        public int GetAbilityCooldown()
        {
            return this.m_abilityCooldown;
        }

        public int GetAbilitySpeedBoost(int index)
        {
            return this.m_abilitySpeedBoost[index];
        }

        public int GetAbilitySpeedBoost2(int index)
        {
            return this.m_abilitySpeedBoost2[index];
        }

        public int GetAbilityDamageBoostPercent(int index)
        {
            return this.m_abilityDamageBoostPercent[index];
        }

        public int GetAbilityDamageBoost(int index)
        {
            return this.m_abilityDamageBoost[index];
        }

        public int GetAbilitySummonTroopCount(int index)
        {
            return this.m_abilitySummonTroopCount[index];
        }

        public bool HasNoDefence()
        {
            return this.m_noDefence;
        }

        public bool HasOnceAbility()
        {
            return this.m_abilityOnce;
        }

        public int GetHeroHitpoints(int hp, int upgLevel)
        {
            hp = LogicMath.Max(0, hp);

            int regenTime = this.m_regenerationTimeSecs[upgLevel];
            int hitpoints = this.m_hitpoints[upgLevel];

            if (regenTime != 0)
            {
                hitpoints = hitpoints * (LogicMath.Max(regenTime - hp, 0) / 60) / (regenTime / 60);
            }

            return hitpoints;
        }

        public bool HasEnoughHealthForAttack(int hp, int upgLevel)
        {
            return this.m_hitpoints[upgLevel] == hp;
        }

        public bool HasAbility(int upgLevel)
        {
            if (this.m_abilityTime[upgLevel] <= 0)
            {
                return this.m_abilitySpellData[upgLevel] != null;
            }

            return true;
        }

        public int GetFullRegenerationTimeSec(int upgLevel)
        {
            return this.m_regenerationTimeSecs[upgLevel];
        }

        public int GetSecondsToFullHealth(int hp, int upgLevel)
        {
            return 60 * (this.m_regenerationTimeSecs[upgLevel] / 60 * (this.m_hitpoints[upgLevel] - LogicMath.Clamp(hp, 0, this.m_hitpoints[upgLevel])) /
                         this.m_hitpoints[upgLevel]);
        }

        public int GetStrengthWeight2(int upgLevel)
        {
            return this.m_strengthWeight2[upgLevel];
        }

        public bool IsFlying(int mode)
        {
            if (mode == 1)
            {
                if (this.m_hasAltMode)
                {
                    return this.m_altModeFlying;
                }
            }

            return this.IsFlying();
        }

        public override int GetCombatItemType()
        {
            return LogicCombatItemData.COMBAT_ITEM_TYPE_HERO;
        }
    }
}