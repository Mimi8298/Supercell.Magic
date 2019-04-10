namespace Supercell.Magic.Logic.Data
{
    using System;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Debug;

    public class LogicCharacterData : LogicCombatItemData
    {
        public const int SPECIAL_ABILITY_TYPE_START_RAGE = 0;
        public const int SPECIAL_ABILITY_TYPE_BIG_FIRST_HIT = 1;
        public const int SPECIAL_ABILITY_TYPE_START_CLOAK = 2;
        public const int SPECIAL_ABILITY_TYPE_SPEED_BOOST = 3;
        public const int SPECIAL_ABILITY_TYPE_DIE_DAMAGE = 4;
        public const int SPECIAL_ABILITY_TYPE_SPAWN_UNITS = 5;
        public const int SPECIAL_ABILITY_TYPE_SPECIAL_PROJECTILE = 6;
        public const int SPECIAL_ABILITY_TYPE_RAGE_ALONE = 7;
        public const int SPECIAL_ABILITY_TYPE_RESPAWN_AS_CANNON = 8;

        private LogicCharacterData m_childTroopData;
        private LogicCharacterData m_secondaryTroopData;
        private LogicSpellData m_specialAbilitySpellData;
        private LogicObstacleData m_tombstoneData;
        private LogicEffectData m_moveStartsEffect;

        private LogicEffectData[] m_specialAbilityEffectData;
        private LogicEffectData[] m_deployEffectData;
        private LogicEffectData[] m_dieEffectData;
        private LogicEffectData[] m_dieEffect2Data;
        private LogicEffectData[] m_dieDamageEffectData;
        private LogicEffectData[] m_moveTrailEffectData;
        private LogicSpellData[] m_auraSpellData;
        private LogicSpellData[] m_retributionSpellData;

        private bool m_flying;
        private bool m_jumper;
        private bool m_isUnderground;
        private bool m_disableDonate;
        private bool m_isSecondaryTroop;
        private bool m_attackMultipleBuildings;
        private bool m_balloonGoblin;
        private bool m_randomizeSecSpawnDist;
        private bool m_attackOverWalls;
        private bool m_smoothJump;
        private bool m_scaleByTH;
        private bool m_triggersTraps;
        private bool m_boostedIfAlone;
        private bool m_pickNewTargetAfterPushback;

        private int m_speed;
        private int m_speedDecreasePerChildTroopLost;
        private int m_pushbackSpeed;
        private int m_hitEffectOffset;
        private int m_targetedEffectOffset;
        private int m_specialAbilityType;
        private int m_unlockedBarrackLevel;
        private int m_childTroopCount;
        private int m_dieDamageRadius;
        private int m_dieDamageDelay;
        private int m_secondarySpawnDistance;
        private int m_secondarySpawnOffset;
        private int m_maxTrainingCount;
        private int m_movementOffsetAmount;
        private int m_movementOffsetSpeed;
        private int m_spawnIdle;
        private int m_autoMergeDistance;
        private int m_autoMergeGroupSize;
        private int m_invisibilityRadius;
        private int m_healthReductionPerSecond;
        private int m_friendlyGroupWeight;
        private int m_enemyGroupWeight;
        private int m_chainShootingDistance;
        private int m_boostRadius;
        private int m_boostDamagePerfect;
        private int m_boostAttackSpeed;
        private int m_loseHpPerTick;
        private int m_loseHpInterval;

        protected int[] m_hitpoints;
        protected int[] m_unitsInCamp;
        protected int[] m_strengthWeight;
        protected int[] m_specialAbilityLevel;
        protected int[] m_specialAbilityAttribute;
        protected int[] m_specialAbilityAttribute2;
        protected int[] m_specialAbilityAttribute3;

        private int[] m_secondaryTroopCount;
        private int[] m_childTroopX;
        private int[] m_childTroopY;
        private int[] m_attackCount;
        private int[] m_abilityAttackCount;
        private int[] m_dieDamage;
        private int[] m_scale;
        private int[] m_auraSpellLevel;
        private int[] m_retributionSpellLevel;
        private int[] m_retributionSpellTriggerHealth;

        private string m_swf;
        private string m_specialAbilityName;
        private string m_specialAbilityInfo;
        private string m_bigPictureSWF;
        private string m_customDefenderIcon;
        private string m_auraTID;
        private string m_auraDescTID;
        private string m_auraBigPictureExportName;

        protected LogicAttackerItemData[] m_attackerItemData;

        public LogicCharacterData(CSVRow row, LogicDataTable table) : base(row, table)
        {
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            int upgradeLevelCount = this.GetUpgradeLevelCount();

            this.m_attackerItemData = new LogicAttackerItemData[upgradeLevelCount];
            this.m_specialAbilityEffectData = new LogicEffectData[upgradeLevelCount];
            this.m_auraSpellData = new LogicSpellData[upgradeLevelCount];
            this.m_retributionSpellData = new LogicSpellData[upgradeLevelCount];
            this.m_deployEffectData = new LogicEffectData[upgradeLevelCount];
            this.m_dieEffectData = new LogicEffectData[upgradeLevelCount];
            this.m_dieEffect2Data = new LogicEffectData[upgradeLevelCount];
            this.m_dieDamageEffectData = new LogicEffectData[upgradeLevelCount];
            this.m_moveTrailEffectData = new LogicEffectData[upgradeLevelCount];

            this.m_hitpoints = new int[upgradeLevelCount];
            this.m_secondaryTroopCount = new int[upgradeLevelCount];
            this.m_unitsInCamp = new int[upgradeLevelCount];
            this.m_strengthWeight = new int[upgradeLevelCount];
            this.m_specialAbilityLevel = new int[upgradeLevelCount];
            this.m_specialAbilityAttribute = new int[upgradeLevelCount];
            this.m_specialAbilityAttribute2 = new int[upgradeLevelCount];
            this.m_specialAbilityAttribute3 = new int[upgradeLevelCount];
            this.m_scale = new int[upgradeLevelCount];
            this.m_attackCount = new int[upgradeLevelCount];
            this.m_abilityAttackCount = new int[upgradeLevelCount];
            this.m_dieDamage = new int[upgradeLevelCount];
            this.m_auraSpellLevel = new int[upgradeLevelCount];
            this.m_retributionSpellLevel = new int[upgradeLevelCount];
            this.m_retributionSpellTriggerHealth = new int[upgradeLevelCount];

            this.m_childTroopX = new int[3];
            this.m_childTroopY = new int[3];

            for (int i = 0; i < this.m_hitpoints.Length; i++)
            {
                this.m_attackerItemData[i] = new LogicAttackerItemData();
                this.m_attackerItemData[i].CreateReferences(this.m_row, this, i);

                this.m_hitpoints[i] = this.GetClampedIntegerValue("Hitpoints", i);
                this.m_secondaryTroopCount[i] = this.GetClampedIntegerValue("SecondaryTroopCnt", i);
                this.m_unitsInCamp[i] = this.GetClampedIntegerValue("UnitsInCamp", i);
                this.m_specialAbilityLevel[i] = this.GetClampedIntegerValue("SpecialAbilityLevel", i);
                this.m_specialAbilityEffectData[i] = LogicDataTables.GetEffectByName(this.GetClampedValue("SpecialAbilityEffect", i), this);
                this.m_deployEffectData[i] = LogicDataTables.GetEffectByName(this.GetClampedValue("DeployEffect", i), this);
                this.m_dieEffectData[i] = LogicDataTables.GetEffectByName(this.GetClampedValue("DieEffect", i), this);
                this.m_dieEffect2Data[i] = LogicDataTables.GetEffectByName(this.GetClampedValue("DieEffect2", i), this);
                this.m_dieDamageEffectData[i] = LogicDataTables.GetEffectByName(this.GetClampedValue("DieDamageEffect", i), this);
                this.m_moveTrailEffectData[i] = LogicDataTables.GetEffectByName(this.GetClampedValue("MoveTrailEffect", i), this);
                this.m_attackCount[i] = this.GetClampedIntegerValue("AttackCount", i);
                this.m_abilityAttackCount[i] = this.GetClampedIntegerValue("AbilityAttackCount", i);
                this.m_dieDamage[i] = this.GetClampedIntegerValue("DieDamage", i);
                this.m_strengthWeight[i] = this.GetClampedIntegerValue("StrengthWeight", i);
                this.m_scale[i] = this.GetClampedIntegerValue("Scale", i);

                if (this.m_scale[i] == 0)
                {
                    this.m_scale[i] = 100;
                }

                this.m_auraSpellData[i] = LogicDataTables.GetSpellByName(this.GetClampedValue("AuraSpell", i), this);
                this.m_auraSpellLevel[i] = this.GetClampedIntegerValue("AuraSpellLevel", i);
                this.m_retributionSpellData[i] = LogicDataTables.GetSpellByName(this.GetClampedValue("RetributionSpell", i), this);
                this.m_retributionSpellLevel[i] = this.GetClampedIntegerValue("RetributionSpellLevel", i);
                this.m_retributionSpellTriggerHealth[i] = this.GetClampedIntegerValue("RetributionSpellTriggerHealth", i);
                this.m_specialAbilityAttribute[i] = this.GetClampedIntegerValue("SpecialAbilityAttribute", i);
                this.m_specialAbilityAttribute2[i] = this.GetClampedIntegerValue("SpecialAbilityAttribute2", i);
                this.m_specialAbilityAttribute3[i] = this.GetClampedIntegerValue("SpecialAbilityAttribute3", i);
            }

            this.m_moveStartsEffect = LogicDataTables.GetEffectByName(this.GetValue("MoveStartsEffect", 0), this);
            this.m_specialAbilityType = this.GetSpecialAbilityTypeFromCSV();

            string specialAbilitySpell = this.GetValue("SpecialAbilitySpell", 0);

            if (specialAbilitySpell.Length > 0)
            {
                this.m_specialAbilitySpellData = LogicDataTables.GetSpellByName(specialAbilitySpell, this);
            }

            this.m_swf = this.GetValue("SWF", 0);
            this.m_specialAbilityName = this.GetValue("SpecialAbilityName", 0);
            this.m_specialAbilityInfo = this.GetValue("SpecialAbilityInfo", 0);
            this.m_bigPictureSWF = this.GetValue("BigPictureSWF", 0);
            this.m_dieDamageRadius = (this.GetIntegerValue("DieDamageRadius", 0) << 9) / 100;
            this.m_dieDamageDelay = this.GetIntegerValue("DieDamageDelay", 0);

            if (this.m_dieDamageDelay > 4000)
            {
                Debugger.Warning("m_dieDamageDelay too big");
                this.m_dieDamageDelay = 4000;
            }

            this.m_secondaryTroopData = LogicDataTables.GetCharacterByName(this.GetValue("SecondaryTroop", 0), this);
            this.m_isSecondaryTroop = this.GetBooleanValue("IsSecondaryTroop", 0);
            this.m_secondarySpawnDistance = (this.GetIntegerValue("SecondarySpawnDist", 0) << 9) / 100;
            this.m_secondarySpawnOffset = (this.GetIntegerValue("SecondarySpawnOffset", 0) << 9) / 100;
            this.m_tombstoneData = LogicDataTables.GetObstacleByName(this.GetValue("TombStone", 0), this);
            this.m_maxTrainingCount = this.GetIntegerValue("MaxTrainingCount", 0);
            this.m_unlockedBarrackLevel = this.GetIntegerValue("BarrackLevel", 0) - 1;
            this.m_flying = this.GetBooleanValue("IsFlying", 0);
            this.m_jumper = this.GetBooleanValue("IsJumper", 0);
            this.m_movementOffsetAmount = this.GetIntegerValue("MovementOffsetAmount", 0);
            this.m_movementOffsetSpeed = this.GetIntegerValue("MovementOffsetSpeed", 0);
            this.m_balloonGoblin = this.GetName().Equals("Balloon Goblin");
            this.m_spawnIdle = this.GetIntegerValue("SpawnIdle", 0);
            this.m_childTroopData = LogicDataTables.GetCharacterByName(this.GetValue("ChildTroop", 0), this);
            this.m_childTroopCount = this.GetIntegerValue("ChildTroopCount", 0);

            for (int i = 0; i < 3; i++)
            {
                this.m_childTroopX[i] = this.GetIntegerValue(string.Format("ChildTroop{0}_X", i), 0);
                this.m_childTroopY[i] = this.GetIntegerValue(string.Format("ChildTroop{0}_Y", i), 0);
            }

            this.m_attackMultipleBuildings = this.GetBooleanValue("AttackMultipleBuildings", 0);

            if (!this.m_attackerItemData[0].IsSelfAsAoeCenter())
            {
                this.m_attackMultipleBuildings = false;
            }

            this.m_speed = (this.GetIntegerValue("Speed", 0) << 9) / 100;
            this.m_speedDecreasePerChildTroopLost = (this.GetIntegerValue("SpeedDecreasePerChildTroopLost", 0) << 9) / 100;
            this.m_pickNewTargetAfterPushback = this.GetBooleanValue("PickNewTargetAfterPushback", 0);
            this.m_pushbackSpeed = this.GetIntegerValue("PushbackSpeed", 0);
            this.m_hitEffectOffset = this.GetIntegerValue("HitEffectOffset", 0);
            this.m_targetedEffectOffset = (this.GetIntegerValue("TargetedEffectOffset", 0) << 9) / 100;
            this.m_randomizeSecSpawnDist = this.GetBooleanValue("RandomizeSecSpawnDist", 0);
            this.m_customDefenderIcon = this.GetValue("CustonDefenderIcon", 0);
            this.m_autoMergeDistance = (this.GetIntegerValue("AutoMergeDistance", 0) << 9) / 100;
            this.m_autoMergeGroupSize = this.GetIntegerValue("AutoMergeGroupSize", 0);
            this.m_invisibilityRadius = (this.GetIntegerValue("InvisibilityRadius", 0) << 9) / 100;
            this.m_healthReductionPerSecond = this.GetIntegerValue("HealthReductionPerSecond", 0);
            this.m_isUnderground = this.GetBooleanValue("IsUnderground", 0);
            this.m_attackOverWalls = this.GetBooleanValue("NoAttackOverWalls", 0) ^ true;
            this.m_smoothJump = this.GetBooleanValue("SmoothJump", 0);
            this.m_auraTID = this.GetValue("AuraTID", 0);
            this.m_auraDescTID = this.GetValue("AuraDescTID", 0);
            this.m_auraBigPictureExportName = this.GetValue("AuraBigPictureExportName", 0);
            this.m_friendlyGroupWeight = this.GetIntegerValue("FriendlyGroupWeight", 0);
            this.m_enemyGroupWeight = this.GetIntegerValue("EnemyGroupWeight", 0);
            this.m_scaleByTH = this.GetBooleanValue("ScaleByTH", 0);
            this.m_disableDonate = this.GetBooleanValue("DisableDonate", 0);
            this.m_loseHpPerTick = this.GetIntegerValue("LoseHpPerTick", 0);
            this.m_loseHpInterval = this.GetIntegerValue("LoseHpInterval", 0);
            this.m_triggersTraps = this.GetBooleanValue("TriggersTraps", 0);
            this.m_chainShootingDistance = this.GetIntegerValue("ChainShootingDistance", 0);
            this.m_boostedIfAlone = this.GetBooleanValue("BoostedIfAlone", 0);
            this.m_boostRadius = (this.GetIntegerValue("BoostRadius", 0) << 9) / 100;
            this.m_boostDamagePerfect = this.GetIntegerValue("BoostDmgPerfect", 0);
            this.m_boostAttackSpeed = this.GetIntegerValue("BoostAttackSpeed", 0);
        }

        public override void CreateReferences2()
        {
            if (this.m_tombstoneData != null)
            {
                if (!this.m_tombstoneData.IsEnabledInVillageType(this.GetVillageType()))
                {
                    Debugger.Error(string.Format("invalid tombstone for unit '{0}' villageType's do not match", this.GetName()));
                }
            }
        }

        private int GetSpecialAbilityTypeFromCSV()
        {
            string name = this.GetValue("SpecialAbilityType", 0);

            if (string.Equals(name, "StartRage", StringComparison.InvariantCultureIgnoreCase))
            {
                return LogicCharacterData.SPECIAL_ABILITY_TYPE_START_RAGE;
            }

            if (string.Equals(name, "BigFirstHit", StringComparison.InvariantCultureIgnoreCase))
            {
                return LogicCharacterData.SPECIAL_ABILITY_TYPE_BIG_FIRST_HIT;
            }

            if (string.Equals(name, "StartCloak", StringComparison.InvariantCultureIgnoreCase))
            {
                return LogicCharacterData.SPECIAL_ABILITY_TYPE_START_CLOAK;
            }

            if (string.Equals(name, "SpeedBoost", StringComparison.InvariantCultureIgnoreCase))
            {
                return LogicCharacterData.SPECIAL_ABILITY_TYPE_SPEED_BOOST;
            }

            if (string.Equals(name, "DieDamage", StringComparison.InvariantCultureIgnoreCase))
            {
                return LogicCharacterData.SPECIAL_ABILITY_TYPE_DIE_DAMAGE;
            }

            if (string.Equals(name, "SpawnUnits", StringComparison.InvariantCultureIgnoreCase))
            {
                return LogicCharacterData.SPECIAL_ABILITY_TYPE_SPAWN_UNITS;
            }

            if (string.Equals(name, "SpecialProjectile", StringComparison.InvariantCultureIgnoreCase))
            {
                return LogicCharacterData.SPECIAL_ABILITY_TYPE_SPECIAL_PROJECTILE;
            }

            if (string.Equals(name, "RageAlone", StringComparison.InvariantCultureIgnoreCase))
            {
                return LogicCharacterData.SPECIAL_ABILITY_TYPE_RAGE_ALONE;
            }

            if (string.Equals(name, "RespawnAsCannon", StringComparison.InvariantCultureIgnoreCase))
            {
                return LogicCharacterData.SPECIAL_ABILITY_TYPE_RESPAWN_AS_CANNON;
            }

            return -1;
        }

        public int GetSpecialAbilityType()
        {
            return this.m_specialAbilityType;
        }

        public override int GetRequiredProductionHouseLevel()
        {
            return this.m_unlockedBarrackLevel;
        }

        public override bool IsDonationDisabled()
        {
            return this.m_disableDonate;
        }

        public override bool IsUnlockedForProductionHouseLevel(int level)
        {
            return level >= this.m_unlockedBarrackLevel;
        }

        public override LogicBuildingData GetProductionHouseData()
        {
            string buildingName = this.GetVillageType() == 1 ? this.GetUnitOfType() == 1 ? "Barrack2" : "Dark Elixir Barrack2" :
                this.GetUnitOfType() == 1 ? "Barrack" : "Dark Elixir Barrack";

            return LogicDataTables.GetBuildingByName(buildingName, null);
        }

        public LogicAttackerItemData GetAttackerItemData(int idx)
        {
            return this.m_attackerItemData[idx];
        }

        public override bool IsUnderground()
        {
            return this.m_isUnderground;
        }

        public int GetHitpoints(int index)
        {
            return this.m_hitpoints[index];
        }

        public int GetUnitsInCamp(int index)
        {
            return this.m_unitsInCamp[index];
        }

        public bool IsSecondaryTroop()
        {
            return this.m_isSecondaryTroop;
        }

        public int GetSpeed()
        {
            return this.m_speed;
        }

        public int GetSpecialAbilityLevel(int upgLevel)
        {
            return this.m_specialAbilityLevel[upgLevel];
        }

        public int GetSpecialAbilityAttribute(int upgLevel)
        {
            return this.m_specialAbilityAttribute[upgLevel];
        }

        public int GetSpecialAbilityAttribute2(int upgLevel)
        {
            return this.m_specialAbilityAttribute2[upgLevel];
        }

        public int GetSpecialAbilityAttribute3(int upgLevel)
        {
            return this.m_specialAbilityAttribute3[upgLevel];
        }

        public int GetAbilityAttackCount(int upgLevel)
        {
            return this.m_abilityAttackCount[upgLevel];
        }

        public LogicCharacterData GetChildTroop()
        {
            return this.m_childTroopData;
        }

        public int GetChildTroopCount()
        {
            return this.m_childTroopCount;
        }

        public int GetChildTroopX(int idx)
        {
            return this.m_childTroopX[idx];
        }

        public int GetChildTroopY(int idx)
        {
            return this.m_childTroopY[idx];
        }

        public bool GetAttackMultipleBuildings()
        {
            return this.m_attackMultipleBuildings;
        }

        public LogicCharacterData GetSecondaryTroop()
        {
            return this.m_secondaryTroopData;
        }

        public LogicSpellData GetSpecialAbilitySpell()
        {
            return this.m_specialAbilitySpellData;
        }

        public LogicObstacleData GetTombstone()
        {
            return this.m_tombstoneData;
        }

        public LogicEffectData GetMoveStartsEffect()
        {
            return this.m_moveStartsEffect;
        }

        public bool GetRandomizeSecSpawnDist()
        {
            return this.m_randomizeSecSpawnDist;
        }

        public bool GetAttackOverWalls()
        {
            return this.m_attackOverWalls;
        }

        public bool GetSmoothJump()
        {
            return this.m_smoothJump;
        }

        public bool GetScaleByTH()
        {
            return this.m_scaleByTH;
        }

        public bool GetTriggersTraps()
        {
            return this.m_triggersTraps;
        }

        public bool GetBoostedIfAlone()
        {
            return this.m_boostedIfAlone;
        }

        public bool GetPickNewTargetAfterPushback()
        {
            return this.m_pickNewTargetAfterPushback;
        }

        public int GetSpeedDecreasePerChildTroopLost()
        {
            return this.m_speedDecreasePerChildTroopLost;
        }

        public int GetPushbackSpeed()
        {
            return this.m_pushbackSpeed;
        }

        public int GetHitEffectOffset()
        {
            return this.m_hitEffectOffset;
        }

        public int GetTargetedEffectOffset()
        {
            return this.m_targetedEffectOffset;
        }

        public int GetDieDamageRadius()
        {
            return this.m_dieDamageRadius;
        }

        public int GetDieDamage(int upgLevel)
        {
            return LogicGamePlayUtil.DPSToSingleHit(this.m_dieDamage[upgLevel], 1000);
        }

        public int GetDieDamageDelay()
        {
            return this.m_dieDamageDelay;
        }

        public LogicEffectData GetDieEffect(int upgLevel)
        {
            return this.m_dieEffectData[upgLevel];
        }

        public LogicEffectData GetDieEffect2(int upgLevel)
        {
            return this.m_dieEffect2Data[upgLevel];
        }

        public LogicEffectData GetSpecialAbilityEffect(int upgLevel)
        {
            return this.m_specialAbilityEffectData[upgLevel];
        }

        public int GetSecondarySpawnDistance()
        {
            return this.m_secondarySpawnDistance;
        }

        public int GetSecondarySpawnOffset()
        {
            return this.m_secondarySpawnOffset;
        }

        public int GetMaxTrainingCount()
        {
            return this.m_maxTrainingCount;
        }

        public int GetMovementOffsetAmount()
        {
            return this.m_movementOffsetAmount;
        }

        public int GetMovementOffsetSpeed()
        {
            return this.m_movementOffsetSpeed;
        }

        public int GetSpawnIdle()
        {
            return this.m_spawnIdle;
        }

        public int GetAutoMergeDistance()
        {
            return this.m_autoMergeDistance;
        }

        public int GetAutoMergeGroupSize()
        {
            return this.m_autoMergeGroupSize;
        }

        public int GetInvisibilityRadius()
        {
            return this.m_invisibilityRadius;
        }

        public int GetHealthReductionPerSecond()
        {
            return this.m_healthReductionPerSecond;
        }

        public int GetFriendlyGroupWeight()
        {
            return this.m_friendlyGroupWeight;
        }

        public int GetEnemyGroupWeight()
        {
            return this.m_enemyGroupWeight;
        }

        public int GetChainShootingDistance()
        {
            return this.m_chainShootingDistance;
        }

        public int GetBoostRadius()
        {
            return this.m_boostRadius;
        }

        public int GetBoostDamagePerfect()
        {
            return this.m_boostDamagePerfect;
        }

        public int GetBoostAttackSpeed()
        {
            return this.m_boostAttackSpeed;
        }

        public int GetLoseHpPerTick()
        {
            return this.m_loseHpPerTick;
        }

        public int GetLoseHpInterval()
        {
            return this.m_loseHpInterval;
        }

        public string GetSwf()
        {
            return this.m_swf;
        }

        public string GetSpecialAbilityName()
        {
            return this.m_specialAbilityName;
        }

        public string GetSpecialAbilityInfo()
        {
            return this.m_specialAbilityInfo;
        }

        public string GetBigPictureSWF()
        {
            return this.m_bigPictureSWF;
        }

        public string GetCustomDefenderIcon()
        {
            return this.m_customDefenderIcon;
        }

        public string GetAuraTID()
        {
            return this.m_auraTID;
        }

        public LogicSpellData GetAuraSpell(int upgLevel)
        {
            return this.m_auraSpellData[upgLevel];
        }

        public int GetAuraSpellLevel(int upgLevel)
        {
            return this.m_auraSpellLevel[upgLevel];
        }

        public LogicSpellData GetRetributionSpell(int upgLevel)
        {
            return this.m_retributionSpellData[upgLevel];
        }

        public int GetRetributionSpellLevel(int upgLevel)
        {
            return this.m_retributionSpellLevel[upgLevel];
        }

        public int GetRetributionSpellTriggerHealth(int upgLevel)
        {
            return this.m_retributionSpellTriggerHealth[upgLevel];
        }

        public string GetAuraDescTID()
        {
            return this.m_auraDescTID;
        }

        public string GetAuraBigPictureExportName()
        {
            return this.m_auraBigPictureExportName;
        }

        public bool IsFlying()
        {
            return this.m_flying;
        }

        public bool IsJumper()
        {
            return this.m_jumper;
        }

        public bool IsBalloonGoblin()
        {
            return this.m_balloonGoblin;
        }

        public bool IsUnlockedForBarrackLevel(int barrackLevel)
        {
            return this.m_unlockedBarrackLevel != -1 && this.m_unlockedBarrackLevel <= barrackLevel;
        }

        public int GetDonateXP()
        {
            return this.GetHousingSpace();
        }

        public int GetStrengthWeight(int upgLevel)
        {
            return this.m_strengthWeight[upgLevel];
        }

        public int GetSecondaryTroopCount(int upgLevel)
        {
            return this.m_secondaryTroopCount[upgLevel];
        }

        public int GetAttackCount(int upgLevel)
        {
            return this.m_attackCount[upgLevel];
        }

        public override int GetCombatItemType()
        {
            return LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER;
        }
    }
}