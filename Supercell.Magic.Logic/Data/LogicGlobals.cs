namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;

    public class LogicGlobals : LogicDataTable
    {
        private int m_speedUpDiamondCostPerMin;
        private int m_speedUpDiamondCostPerHour;
        private int m_speedUpDiamondCostPerDay;
        private int m_speedUpDiamondCostPerWeek;
        private int m_speedUpDiamondCostPerMinVillage2;
        private int m_speedUpDiamondCostPerHourVillage2;
        private int m_speedUpDiamondCostPerDayVillage2;
        private int m_speedUpDiamondCostPerWeekVillage2;

        private int m_resourceDiamondCost100;
        private int m_resourceDiamondCost1000;
        private int m_resourceDiamondCost10000;
        private int m_resourceDiamondCost100000;
        private int m_resourceDiamondCost1000000;
        private int m_resourceDiamondCost10000000;
        private int m_village2ResourceDiamondCost100;
        private int m_village2ResourceDiamondCost1000;
        private int m_village2ResourceDiamondCost10000;
        private int m_village2resourceDiamondCost100000;
        private int m_village2resourceDiamondCost1000000;
        private int m_village2ResourceDiamondCost10000000;
        private int m_darkElixirDiamondCost1;
        private int m_darkElixirDiamondCost10;
        private int m_darkElixirDiamondCost100;
        private int m_darkElixirDiamondCost1000;
        private int m_darkElixirDiamondCost10000;
        private int m_darkElixirDiamondCost100000;

        private int m_freeUnitHousingCapPercentage;
        private int m_freeHeroHealthCap;
        private int m_startingDiamonds;
        private int m_startingElixir;
        private int m_startingElixir2;
        private int m_startingGold;
        private int m_startingGold2;
        private int m_liveReplayFrequencySecs;
        private int m_challengeBaseSaveCooldown;
        private int m_allianceTroopRequestCooldown;
        private int m_arrangeWarCooldown;
        private int m_clanMailCooldown;
        private int m_replayShareCooldown;
        private int m_elderKickCooldown;
        private int m_challengeCooldown;
        private int m_allianceCreateCost;
        private int m_clockTowerBoostCooldownSecs;
        private int m_clampLongTimeStampsToDays;
        private int m_workerCostSecondBuildCost;
        private int m_workerCostThirdBuildCost;
        private int m_workerCostFourthBuildCost;
        private int m_workerCostFifthBuildCost;
        private int m_challengeBaseCooldownEnabledOnTh;
        private int m_obstacleRespawnSecs;
        private int m_tallGrassRespawnSecs;
        private int m_obstacleMaxCount;
        private int m_resourceProductionLootPercentage;
        private int m_darkElixirProductionLootPercentage;
        private int m_village2MinTownHallLevelForDestructObstacle;
        private int m_attackVillage2PreparationLengthSecs;
        private int m_attackPreparationLengthSecs;
        private int m_attackLengthSecs;
        private int m_village2StartUnitLevel;
        private int m_resourceProductionBoostSecs;
        private int m_barracksBoostSecs;
        private int m_spellFactoryBoostSecs;
        private int m_heroRestBoostSecs;
        private int m_troopTrainingSpeedUpCostTutorial;
        private int m_newTrainingBoostBarracksCost;
        private int m_newTrainingBoostLaboratoryCost;
        private int m_personalBreakLimitSeconds;
        private int m_enablePresetsTownHallLevel;
        private int m_maxAllianceFeedbackMessageLength;
        private int m_maxAllianceMailLength;
        private int m_maxMessageLength;
        private int m_maxTroopDonationCount;
        private int m_maxSpellDonationCount;
        private int m_darkSpellDonationXP;
        private int m_enableNameChangeTownHallLevel;
        private int m_starBonusCooldownMinutes;
        private int m_bunkerSearchTime;
        private int m_clanCastleRadius;
        private int m_clanDefenderSearchRadius;
        private int m_lootCartReengagementMinSecs;
        private int m_lootCartReengagementMaxSecs;
        private int m_warMaxExcludeMembers;
        private int m_minerTargetRandPercentage;
        private int m_minerSpeedRandPercentage;
        private int m_minerHideTime;
        private int m_minerHideTimeRandom;
        private int m_townHallLootPercentage;
        private int m_charVsCharRandomDistanceLimit;
        private int m_charVsCharRadiusForAttacker;
        private int m_targetListSize;
        private int m_chainedProjectileBounceCount;

        private int m_clockTowerBoostMultiplier;
        private int m_resourceProductionBoostMultiplier;
        private int m_spellTrainingCostMultiplier;
        private int m_spellSpeedUpCostMultiplier;
        private int m_heroHealthSpeedUpCostMultipler;
        private int m_troopRequestSpeedUpCostMultiplier;
        private int m_troopTrainingCostMultiplier;
        private int m_speedUpBoostCooldownCostMultiplier;
        private int m_spellHousingCostMultiplier;
        private int m_unitHousingCostMultiplier;
        private int m_heroHousingCostMultiplier;
        private int m_unitHousingCostMultiplierForTotal;
        private int m_spellHousingCostMultiplierForTotal;
        private int m_heroHousingCostMultiplierForTotal;
        private int m_allianceUnitHousingCostMultiplierForTotal;
        private int m_barracksBoostNewMultiplier;
        private int m_barracksBoostMultiplier;
        private int m_spellFactoryBoostNewMultiplier;
        private int m_spellFactoryBoostMultiplier;
        private int m_clockTowerSpeedUpMultiplier;
        private int m_heroRestBoostMultiplier;
        private int m_buildCancelMultiplier;
        private int m_spellCancelMultiplier;
        private int m_trainCancelMultiplier;
        private int m_heroUpgradeCancelMultiplier;
        private int m_village2FirstVictoryTrophies;
        private int m_village2FirstVictoryGold;
        private int m_village2FirstVictoryElixir;
        private int m_duelLootLimitFreeSpeedUps;
        private int m_newbieShieldHours;
        private int m_bookmarksMaxAlliances;
        private int m_layoutSlot2THLevel;
        private int m_layoutSlot3THLevel;
        private int m_layoutSlot2THLevelVillage2;
        private int m_layoutSlot3THLevelVillage2;
        private int m_scoreMultiplierOnAttackLose;
        private int m_eloOffsetDampeningFactor;
        private int m_eloOffsetDampeningLimit;
        private int m_eloOffsetDampeningScoreLimit;
        private int m_starBonusStarCount;
        private int m_lootCartEnabledForTH;
        private int m_shieldTriggerPercentageHousingSpace;
        private int m_defaultDefenseVillageGuard;
        private int m_wallCostBase;
        private int m_hiddenBuildingAppearDestructionPercentage;
        private int m_heroHealMultiplier;
        private int m_heroRageMultiplier;
        private int m_heroRageSpeedMultiplier;
        private int m_warLootPercentage;
        private int m_blockedAttackPositionPenalty;
        private int m_wallBreakerSmartCountLimit;
        private int m_wallBreakerSmartRadius;
        private int m_wallBreakerSmartRetargetLimit;
        private int m_selectedWallTime;
        private int m_forgetTargetTime;
        private int m_skeletonSpellStorageMultiplier;
        private int m_allianceAlertRadius;
        private int m_shrinkSpellDurationSeconds;

        private bool m_useNewTraining;
        private bool m_useTroopWalksOutFromTraining;
        private bool m_useVillageObjects;
        private bool m_useVersusBattle;
        private bool m_moreAccurateTime;
        private bool m_dragInTraining;
        private bool m_dragInTrainingFix;
        private bool m_dragInTrainingFix2;
        private bool m_useNewPathFinder;
        private bool m_liveReplayEnabled;
        private bool m_revertBrokenWarLayouts;
        private bool m_removeRevengeWhenBattleIsLoaded;
        private bool m_completeConstructionOnlyHome;
        private bool m_useNewSpeedUpCalculation;
        private bool m_clampBuildingTimes;
        private bool m_clampUpgradesTimes;
        private bool m_clampAvatarTimersToMax;
        private bool m_stopBoostPauseWhenBoostTimeZeroOnLoad;
        private bool m_fixClanPortalBattleNotEnding;
        private bool m_fixMergeOldBarrackBoostPausing;
        private bool m_saveVillageObjects;
        private bool m_startInLastUsedVillage;
        private bool m_workerForZeroBuildingTime;
        private bool m_adjustEndSubtickUseCurrentTime;
        private bool m_collectAllResourcesAtOnce;
        private bool m_useSwapBuildings;
        private bool m_treasurySizeBasedOnTawnHall;
        private bool m_useTeslaTriggerCommand;
        private bool m_useTrapTriggerCommand;
        private bool m_validateTroopUpgradeLevels;
        private bool m_allowCancelBuildingConstruction;
        private bool m_village2TrainingOnlyUseRegularStorage;
        private bool m_enableTroopDeletion;
        private bool m_enablePresets;
        private bool m_enableNameChange;
        private bool m_enableQuickDonate;
        private bool m_enableQuickDonateWar;
        private bool m_useTownHallLootPenaltyInWar;
        private bool m_allowClanCastleDeployOnObstacles;
        private bool m_skeletonTriggerTesla;
        private bool m_skeletonOpenClanCastle;
        private bool m_castleTroopTargetFilter;
        private bool m_useTroopRequestSpeedUp;
        private bool m_noCooldownFromMoveEditModeActive;
        private bool m_scoringOnlyFromMatchedMode;
        private bool m_eloOffsetDampeningEnabled;
        private bool m_enableLeagues;
        private bool m_revengeGiveLeagueBonus;
        private bool m_revengeGiveStarBonus;
        private bool m_allowStarsOverflowInStarBonus;
        private bool m_loadVillage2AsSnapshot;
        private bool m_readyForWarAttackCheck;
        private bool m_useMoreAccurateLootCap;
        private bool m_enableDefendingAllianceTroopJump;
        private bool m_useWallWeightsForJumpSpell;
        private bool m_jumpWhenHitJumpable;
        private bool m_slideAlongObstacles;
        private bool m_guardPostNotFunctionalUnderUpgrade;
        private bool m_repathDuringFly;
        private bool m_useStickToClosestUnitHealer;
        private bool m_heroUsesAttackPosRandom;
        private bool m_useAttackPosRandomOn1stTarget;
        private bool m_targetSelectionConsidersWallsOnPath;
        private bool m_valkyriePrefers4Buildings;
        private bool m_tighterAttackPosition;
        private bool m_allianceTroopsPatrol;
        private bool m_wallBreakerUseRooms;
        private bool m_rememberOriginalTarget;
        private bool m_ignoreAllianceAlertForNonValidTargets;
        private bool m_restartAttackTimerOnAreaDamageTurrets;
        private bool m_clearAlertStateIfNoTargetFound;
        private bool m_movingUnitsUseSimpleSelect;
        private bool m_morePreciseTargetSelection;
        private bool m_useSmarterHealer;
        private bool m_usePoisonAvoidance;
        private bool m_removeUntriggeredTesla;

        private int[] m_village2TroopHousingBuildCost;
        private int[] m_village2TroopHousingBuildTimeSecs;
        private int[] m_lootMultiplierByTownHallDifference;
        private int[] m_barrackReduceTrainingDivisor;
        private int[] m_darkBarrackReduceTrainingDivisor;
        private int[] m_clockTowerBoostSecs;
        private int[] m_allianceScoreLimit;
        private int[] m_leagueBonusPercentages;
        private int[] m_leagueBonusAltPercentages;
        private int[] m_destructionToShield;
        private int[] m_shieldHours;
        private int[] m_attackShieldReduceHours;
        private int[] m_healStackPercent;

        private LogicResourceData m_allianceCreateResourceData;
        private LogicCharacterData m_village2StartUnit;

        public LogicGlobals(CSVTable table, LogicDataType index) : base(table, index)
        {
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_freeUnitHousingCapPercentage = this.GetIntValue("FREE_UNIT_HOUSING_CAP_PERCENTAGE");
            this.m_freeHeroHealthCap = this.GetIntValue("FREE_HERO_HEALTH_CAP");

            this.m_speedUpDiamondCostPerMin = this.GetIntValue("SPEED_UP_DIAMOND_COST_1_MIN");
            this.m_speedUpDiamondCostPerHour = this.GetIntValue("SPEED_UP_DIAMOND_COST_1_HOUR");
            this.m_speedUpDiamondCostPerDay = this.GetIntValue("SPEED_UP_DIAMOND_COST_24_HOURS");
            this.m_speedUpDiamondCostPerWeek = this.GetIntValue("SPEED_UP_DIAMOND_COST_1_WEEK");
            this.m_speedUpDiamondCostPerMinVillage2 = this.GetIntValue("VILLAGE2_SPEED_UP_DIAMOND_COST_1_MIN");
            this.m_speedUpDiamondCostPerHourVillage2 = this.GetIntValue("VILLAGE2_SPEED_UP_DIAMOND_COST_1_HOUR");
            this.m_speedUpDiamondCostPerDayVillage2 = this.GetIntValue("VILLAGE2_SPEED_UP_DIAMOND_COST_24_HOURS");
            this.m_speedUpDiamondCostPerWeekVillage2 = this.GetIntValue("VILLAGE2_SPEED_UP_DIAMOND_COST_1_WEEK");

            this.m_resourceDiamondCost100 = this.GetIntValue("RESOURCE_DIAMOND_COST_100");
            this.m_resourceDiamondCost1000 = this.GetIntValue("RESOURCE_DIAMOND_COST_1000");
            this.m_resourceDiamondCost10000 = this.GetIntValue("RESOURCE_DIAMOND_COST_10000");
            this.m_resourceDiamondCost100000 = this.GetIntValue("RESOURCE_DIAMOND_COST_100000");
            this.m_resourceDiamondCost1000000 = this.GetIntValue("RESOURCE_DIAMOND_COST_1000000");
            this.m_resourceDiamondCost10000000 = this.GetIntValue("RESOURCE_DIAMOND_COST_10000000");
            this.m_village2ResourceDiamondCost100 = this.GetIntValue("VILLAGE2_RESOURCE_DIAMOND_COST_100");
            this.m_village2ResourceDiamondCost1000 = this.GetIntValue("VILLAGE2_RESOURCE_DIAMOND_COST_1000");
            this.m_village2ResourceDiamondCost10000 = this.GetIntValue("VILLAGE2_RESOURCE_DIAMOND_COST_10000");
            this.m_village2resourceDiamondCost100000 = this.GetIntValue("VILLAGE2_RESOURCE_DIAMOND_COST_100000");
            this.m_village2resourceDiamondCost1000000 = this.GetIntValue("VILLAGE2_RESOURCE_DIAMOND_COST_1000000");
            this.m_village2ResourceDiamondCost10000000 = this.GetIntValue("VILLAGE2_RESOURCE_DIAMOND_COST_10000000");
            this.m_darkElixirDiamondCost1 = this.GetIntValue("DARK_ELIXIR_DIAMOND_COST_1");
            this.m_darkElixirDiamondCost10 = this.GetIntValue("DARK_ELIXIR_DIAMOND_COST_10");
            this.m_darkElixirDiamondCost100 = this.GetIntValue("DARK_ELIXIR_DIAMOND_COST_100");
            this.m_darkElixirDiamondCost1000 = this.GetIntValue("DARK_ELIXIR_DIAMOND_COST_1000");
            this.m_darkElixirDiamondCost10000 = this.GetIntValue("DARK_ELIXIR_DIAMOND_COST_10000");
            this.m_darkElixirDiamondCost100000 = this.GetIntValue("DARK_ELIXIR_DIAMOND_COST_100000");

            this.m_startingDiamonds = this.GetIntValue("STARTING_DIAMONDS");
            this.m_startingGold = this.GetIntValue("STARTING_GOLD");
            this.m_startingElixir = this.GetIntValue("STARTING_ELIXIR");
            this.m_startingGold2 = this.GetIntValue("STARTING_GOLD2");
            this.m_startingElixir2 = this.GetIntValue("STARTING_ELIXIR2");
            this.m_liveReplayFrequencySecs = this.GetIntValue("LIVE_REPLAY_UPDATE_FREQUENCY_SECONDS");
            this.m_challengeBaseSaveCooldown = this.GetIntValue("CHALLENGE_BASE_SAVE_COOLDOWN");
            this.m_allianceCreateCost = this.GetIntValue("ALLIANCE_CREATE_COST");
            this.m_clockTowerBoostCooldownSecs = 60 * this.GetIntValue("CLOCK_TOWER_BOOST_COOLDOWN_MINS");
            this.m_clampLongTimeStampsToDays = this.GetIntValue("CLAMP_LONG_TIME_STAMPS_TO_DAYS");
            this.m_workerCostSecondBuildCost = this.GetIntValue("WORKER_COST_2ND");
            this.m_workerCostThirdBuildCost = this.GetIntValue("WORKER_COST_3RD");
            this.m_workerCostFourthBuildCost = this.GetIntValue("WORKER_COST_4TH");
            this.m_workerCostFifthBuildCost = this.GetIntValue("WORKER_COST_5TH");
            this.m_challengeBaseCooldownEnabledOnTh = this.GetIntValue("CHALLENGE_BASE_COOLDOWN_ENABLED_ON_TH");
            this.m_obstacleRespawnSecs = this.GetIntValue("OBSTACLE_RESPAWN_SECONDS");

            if (this.m_obstacleRespawnSecs < 3600)
            {
                Debugger.Error("Globals.csv - OBSTACLE_RESPAWN_SECONDS is smaller than 3600");
            }

            this.m_obstacleMaxCount = this.GetIntValue("OBSTACLE_COUNT_MAX");
            this.m_resourceProductionLootPercentage = this.GetIntValue("RESOURCE_PRODUCTION_LOOT_PERCENTAGE");
            this.m_darkElixirProductionLootPercentage = this.GetIntValue("RESOURCE_PRODUCTION_LOOT_PERCENTAGE_DARK_ELIXIR");
            this.m_village2MinTownHallLevelForDestructObstacle = this.GetIntValue("VILLAGE2_DO_NOT_ALLOW_CLEAR_OBSTACLE_TH");
            this.m_attackVillage2PreparationLengthSecs = this.GetIntValue("ATTACK_PREPARATION_LENGTH_VILLAGE2_SEC");
            this.m_attackPreparationLengthSecs = this.GetIntValue("ATTACK_PREPARATION_LENGTH_SEC");
            this.m_attackLengthSecs = this.GetIntValue("ATTACK_LENGTH_SEC");
            this.m_village2StartUnitLevel = this.GetIntValue("VILLAGE2_START_UNIT_LEVEL");
            this.m_resourceProductionBoostSecs = 60 * this.GetIntValue("RESOURCE_PRODUCTION_BOOST_MINS");
            this.m_barracksBoostSecs = 60 * this.GetIntValue("BARRACKS_BOOST_MINS");
            this.m_spellFactoryBoostSecs = 60 * this.GetIntValue("SPELL_FACTORY_BOOST_MINS");
            this.m_heroRestBoostSecs = 60 * this.GetIntValue("HERO_REST_BOOST_MINS");
            this.m_troopTrainingSpeedUpCostTutorial = this.GetIntValue("TROOP_TRAINING_SPEED_UP_COST_TUTORIAL");
            this.m_newTrainingBoostBarracksCost = this.GetIntValue("NEW_TRAINING_BOOST_BARRACKS_COST");
            this.m_newTrainingBoostLaboratoryCost = this.GetIntValue("NEW_TRAINING_BOOST_LABORATORY_COST");
            this.m_personalBreakLimitSeconds = this.GetIntValue("PERSONAL_BREAK_LIMIT_SECONDS");
            this.m_allianceTroopRequestCooldown = this.GetIntValue("ALLIANCE_TROOP_REQUEST_COOLDOWN");
            this.m_arrangeWarCooldown = this.GetIntValue("ARRANGE_WAR_COOLDOWN");
            this.m_clanMailCooldown = this.GetIntValue("CLAN_MAIL_COOLDOWN");
            this.m_replayShareCooldown = this.GetIntValue("REPLAY_SHARE_COOLDOWN");
            this.m_elderKickCooldown = this.GetIntValue("ELDER_KICK_COOLDOWN");
            this.m_challengeCooldown = this.GetIntValue("CHALLENGE_COOLDOWN");
            this.m_enablePresetsTownHallLevel = this.GetIntValue("ENABLE_PRESETS_TH_LEVEL") - 1;
            this.m_maxAllianceFeedbackMessageLength = this.GetIntValue("MAX_ALLIANCE_FEEDBACK_MESSAGE_LENGTH");
            this.m_maxAllianceMailLength = this.GetIntValue("MAX_ALLIANCE_MAIL_LENGTH");
            this.m_maxMessageLength = this.GetIntValue("MAX_MESSAGE_LENGTH");
            this.m_tallGrassRespawnSecs = this.GetIntValue("TALLGRASS_RESPAWN_SECONDS");
            this.m_enableNameChangeTownHallLevel = this.GetIntValue("ENABLE_NAME_CHANGE_TH_LEVEL") - 1;
            this.m_village2FirstVictoryTrophies = this.GetIntValue("VILLAGE2_FIRST_VICTORY_TROPHIES");
            this.m_village2FirstVictoryGold = this.GetIntValue("VILLAGE2_FIRST_VICTORY_GOLD");
            this.m_village2FirstVictoryElixir = this.GetIntValue("VILLAGE2_FIRST_VICTORY_ELIXIR");
            this.m_duelLootLimitFreeSpeedUps = this.GetIntValue("DUEL_LOOT_LIMIT_FREE_SPEEDUPS");
            this.m_maxTroopDonationCount = this.GetIntValue("MAX_TROOP_DONATION_COUNT");
            this.m_maxSpellDonationCount = this.GetIntValue("MAX_SPELL_DONATION_COUNT");
            this.m_darkSpellDonationXP = this.GetIntValue("DARK_SPELL_DONATION_XP");
            this.m_starBonusCooldownMinutes = this.GetIntValue("STAR_BONUS_COOLDOWN_MINUTES");
            this.m_clanCastleRadius = this.GetIntValue("CLAN_CASTLE_RADIUS") << 9;
            this.m_clanDefenderSearchRadius = this.GetIntValue("CASTLE_DEFENDER_SEARCH_RADIUS");
            this.m_bunkerSearchTime = this.GetIntValue("BUNKER_SEARCH_TIME");
            this.m_newbieShieldHours = this.GetIntValue("NEWBIE_SHIELD_HOURS");
            this.m_lootCartReengagementMinSecs = 60 * this.GetIntValue("LOOT_CART_REENGAGEMENT_MINUTES_MIN");
            this.m_lootCartReengagementMaxSecs = 60 * this.GetIntValue("LOOT_CART_REENGAGEMENT_MINUTES_MAX");
            this.m_warMaxExcludeMembers = this.GetIntValue("WAR_MAX_EXCLUDE_MEMBERS");
            this.m_shieldTriggerPercentageHousingSpace = this.GetIntValue("SHIELD_TRIGGER_PERCENTAGE_HOUSING_SPACE");
            this.m_defaultDefenseVillageGuard = this.GetIntValue("DEFAULT_DEFENSE_VILLAGE_GUARD");
            this.m_minerTargetRandPercentage = this.GetIntValue("MINER_TARGET_RAND_P");
            this.m_minerSpeedRandPercentage = this.GetIntValue("MINER_SPEED_RAND_P");
            this.m_minerHideTime = this.GetIntValue("MINER_HIDE_TIME");
            this.m_minerHideTimeRandom = this.GetIntValue("MINER_HIDE_TIME_RANDOM");

            if (this.m_minerHideTimeRandom <= 0)
            {
                this.m_minerHideTimeRandom = 1;
            }

            this.m_townHallLootPercentage = this.GetIntValue("TOWN_HALL_LOOT_PERCENTAGE");
            this.m_charVsCharRandomDistanceLimit = (this.GetIntValue("CHAR_VS_CHAR_RANDOM_DIST_LIMIT") << 9) / 100;
            this.m_charVsCharRadiusForAttacker = this.GetIntValue("CHAR_VS_CHAR_RADIUS_FOR_ATTACKER") << 9;
            this.m_hiddenBuildingAppearDestructionPercentage = this.GetIntValue("HIDDEN_BUILDING_APPEAR_DESTRUCTION_PERCENTAGE");
            this.m_heroHealMultiplier = this.GetIntValue("HERO_HEAL_MULTIPLIER");
            this.m_heroRageMultiplier = this.GetIntValue("HERO_RAGE_MULTIPLIER");
            this.m_heroRageSpeedMultiplier = this.GetIntValue("HERO_RAGE_SPEED_MULTIPLIER");
            this.m_wallCostBase = this.GetIntValue("WALL_COST_BASE");

            if (this.m_wallCostBase > 1500)
            {
                Debugger.Warning("WALL_COST_BASE is too big");
                this.m_wallCostBase = 1500;
            }
            else if (this.m_wallCostBase < 100)
            {
                this.m_wallCostBase = 100;
            }

            if (this.m_bunkerSearchTime < 100)
            {
                Debugger.Warning("m_bunkerSearchTime too small");
            }

            if (this.m_townHallLootPercentage != -1 && this.m_townHallLootPercentage > 100)
            {
                Debugger.Error("globals.csv: Invalid loot percentage!");
            }

            this.m_clockTowerBoostMultiplier = this.GetIntValue("CLOCK_TOWER_BOOST_MULTIPLIER");
            this.m_resourceProductionBoostMultiplier = this.GetIntValue("RESOURCE_PRODUCTION_BOOST_MULTIPLIER");
            this.m_spellTrainingCostMultiplier = this.GetIntValue("SPELL_TRAINING_COST_MULTIPLIER");
            this.m_spellSpeedUpCostMultiplier = this.GetIntValue("SPELL_SPEED_UP_COST_MULTIPLIER");
            this.m_heroHealthSpeedUpCostMultipler = this.GetIntValue("HERO_HEALTH_SPEED_UP_COST_MULTIPLIER");
            this.m_troopRequestSpeedUpCostMultiplier = this.GetIntValue("TROOP_REQUEST_SPEED_UP_COST_MULTIPLIER");
            this.m_troopTrainingCostMultiplier = this.GetIntValue("TROOP_TRAINING_COST_MULTIPLIER");
            this.m_speedUpBoostCooldownCostMultiplier = this.GetIntValue("SPEEDUP_BOOST_COOLDOWN_COST_MULTIPLIER");
            this.m_clockTowerSpeedUpMultiplier = this.GetIntValue("CLOCK_TOWER_SPEEDUP_MULTIPLIER");
            this.m_barracksBoostMultiplier = this.GetIntValue("BARRACKS_BOOST_MULTIPLIER");
            this.m_barracksBoostNewMultiplier = this.GetIntValue("BARRACKS_BOOST_MULTIPLIER_NEW");
            this.m_spellFactoryBoostNewMultiplier = this.GetIntValue("SPELL_FACTORY_BOOST_MULTIPLIER_NEW");
            this.m_spellFactoryBoostMultiplier = this.GetIntValue("SPELL_FACTORY_BOOST_MULTIPLIER");
            this.m_heroRestBoostMultiplier = this.GetIntValue("HERO_REST_BOOST_MULTIPLIER");
            this.m_buildCancelMultiplier = this.GetIntValue("BUILD_CANCEL_MULTIPLIER");
            this.m_trainCancelMultiplier = this.GetIntValue("TRAIN_CANCEL_MULTIPLIER");
            this.m_spellCancelMultiplier = this.GetIntValue("SPELL_CANCEL_MULTIPLIER");
            this.m_heroUpgradeCancelMultiplier = this.GetIntValue("HERO_UPGRADE_CANCEL_MULTIPLIER");
            this.m_spellHousingCostMultiplier = this.GetIntValue("SPELL_HOUSING_COST_MULTIPLIER");
            this.m_unitHousingCostMultiplier = this.GetIntValue("UNIT_HOUSING_COST_MULTIPLIER");
            this.m_heroHousingCostMultiplier = this.GetIntValue("HERO_HOUSING_COST_MULTIPLIER");
            this.m_unitHousingCostMultiplierForTotal = this.GetIntValue("UNIT_HOUSING_COST_MULTIPLIER_FOR_TOTAL");
            this.m_spellHousingCostMultiplierForTotal = this.GetIntValue("SPELL_HOUSING_COST_MULTIPLIER_FOR_TOTAL");
            this.m_heroHousingCostMultiplierForTotal = this.GetIntValue("HERO_HOUSING_COST_MULTIPLIER_FOR_TOTAL");
            this.m_allianceUnitHousingCostMultiplierForTotal = this.GetIntValue("ALLIANCE_UNIT_HOUSING_COST_MULTIPLIER_FOR_TOTAL");
            this.m_bookmarksMaxAlliances = this.GetIntValue("BOOKMARKS_MAX_ALLIANCES");
            this.m_layoutSlot2THLevel = this.GetIntValue("LAYOUT_SLOT_2_TH_LEVEL") - 1;
            this.m_layoutSlot3THLevel = this.GetIntValue("LAYOUT_SLOT_3_TH_LEVEL") - 1;
            this.m_layoutSlot2THLevelVillage2 = this.GetIntValue("LAYOUT_SLOT_2_TH_LEVEL_VILLAGE2") - 1;
            this.m_layoutSlot3THLevelVillage2 = this.GetIntValue("LAYOUT_SLOT_3_TH_LEVEL_VILLAGE2") - 1;
            this.m_scoreMultiplierOnAttackLose = this.GetIntValue("SCORE_MULTIPLIER_ON_ATTACK_LOSE");
            this.m_eloOffsetDampeningFactor = this.GetIntValue("ELO_OFFSET_DAMPENING_FACTOR");
            this.m_eloOffsetDampeningLimit = this.GetIntValue("ELO_OFFSET_DAMPENING_LIMIT");
            this.m_eloOffsetDampeningScoreLimit = this.GetIntValue("ELO_OFFSET_DAMPENING_SCORE_LIMIT");
            this.m_starBonusStarCount = this.GetIntValue("STAR_BONUS_STAR_COUNT");
            this.m_lootCartEnabledForTH = this.GetIntValue("LOOT_CART_ENABLED_FOR_TH");
            this.m_warLootPercentage = this.GetIntValue("WAR_LOOT_PERCENTAGE");
            this.m_blockedAttackPositionPenalty = this.GetIntValue("BLOCKED_ATTACK_POSITION_PENALTY");
            this.m_targetListSize = this.GetIntValue("TARGET_LIST_SIZE");

            if (this.m_targetListSize <= 2)
            {
                Debugger.Error("TARGET_LIST_SIZE too small");
            }

            this.m_wallBreakerSmartCountLimit = this.GetIntValue("WALL_BREAKER_SMART_CNT_LIMIT");
            this.m_wallBreakerSmartRadius = (this.GetIntValue("WALL_BREAKER_SMART_RADIUS") << 9) / 100;
            this.m_wallBreakerSmartRetargetLimit = this.GetIntValue("WALL_BREAKER_SMART_RETARGET_LIMIT");
            this.m_selectedWallTime = this.GetIntValue("SELECTED_WALL_TIME");
            this.m_skeletonSpellStorageMultiplier = this.GetIntValue("SKELETON_SPELL_STORAGE_MULTIPLIER");
            this.m_allianceAlertRadius = (this.GetIntValue("ALLIANCE_ALERT_RADIUS") << 9) / 100;
            this.m_forgetTargetTime = this.GetIntValue("FORGET_TARGET_TIME");

            if (this.m_forgetTargetTime < 5000)
            {
                Debugger.Warning("FORGET_TARGET_TIME is too small");
                this.m_forgetTargetTime = 5000;
            }

            this.m_chainedProjectileBounceCount = this.GetIntValue("CHAINED_PROJECTILE_BOUNCE_COUNT");
            this.m_shrinkSpellDurationSeconds = this.GetIntValue("SHRINK_SPELL_DURATION_SECONDS");

            this.m_useNewPathFinder = this.GetBoolValue("USE_NEW_PATH_FINDER");
            this.m_useTroopWalksOutFromTraining = this.GetBoolValue("USE_TROOP_WALKS_OUT_FROM_TRAINING");
            this.m_useVillageObjects = this.GetBoolValue("USE_VILLAGE_OBJECTS");
            this.m_useVersusBattle = this.GetBoolValue("USE_VERSUS_BATTLE");
            this.m_moreAccurateTime = this.GetBoolValue("MORE_ACCURATE_TIME");
            this.m_useNewTraining = this.GetBoolValue("USE_NEW_TRAINING");
            this.m_dragInTraining = this.GetBoolValue("DRAG_IN_TRAINING");
            this.m_dragInTrainingFix = this.GetBoolValue("DRAG_IN_TRAINING_FIX");
            this.m_dragInTrainingFix2 = this.GetBoolValue("DRAG_IN_TRAINING_FIX2");
            this.m_revertBrokenWarLayouts = this.GetBoolValue("REVERT_BROKEN_WAR_LAYOUTS");
            this.m_liveReplayEnabled = this.GetBoolValue("LIVE_REPLAY_ENABLED");
            this.m_removeRevengeWhenBattleIsLoaded = this.GetBoolValue("REMOVE_REVENGE_WHEN_BATTLE_IS_LOADED");
            this.m_completeConstructionOnlyHome = this.GetBoolValue("COMPLETE_CONSTRUCTIONS_ONLY_HOME");
            this.m_useNewSpeedUpCalculation = this.GetBoolValue("USE_NEW_SPEEDUP_CALCULATION");
            this.m_clampBuildingTimes = this.GetBoolValue("CLAMP_BUILDING_TIMES");
            this.m_clampUpgradesTimes = this.GetBoolValue("CLAMP_UPGRADE_TIMES");
            this.m_clampAvatarTimersToMax = this.GetBoolValue("CLAMP_AVATAR_TIMERS_TO_MAX");
            this.m_stopBoostPauseWhenBoostTimeZeroOnLoad = this.GetBoolValue("STOP_BOOST_PAUSE_WHEN_BOOST_TIME_ZERO_ON_LOAD");
            this.m_fixClanPortalBattleNotEnding = this.GetBoolValue("FIX_CLAN_PORTAL_BATTLE_NOT_ENDING");
            this.m_fixMergeOldBarrackBoostPausing = this.GetBoolValue("FIX_MERGE_OLD_BARRACK_BOOST_PAUSING");
            this.m_saveVillageObjects = this.GetBoolValue("SAVE_VILLAGE_OBJECTS");
            this.m_workerForZeroBuildingTime = this.GetBoolValue("WORKER_FOR_ZERO_BUILD_TIME");
            this.m_adjustEndSubtickUseCurrentTime = this.GetBoolValue("ADJUST_END_SUBTICK_USE_CURRENT_TIME");
            this.m_collectAllResourcesAtOnce = this.GetBoolValue("COLLECT_ALL_RESOURCES_AT_ONCE");
            this.m_useSwapBuildings = this.GetBoolValue("USE_SWAP_BUILDINGS");
            this.m_treasurySizeBasedOnTawnHall = this.GetBoolValue("TREASURY_SIZE_BASED_ON_TH");
            this.m_startInLastUsedVillage = this.GetBoolValue("START_IN_LAST_USED_VILLAGE");
            this.m_useTeslaTriggerCommand = this.GetBoolValue("USE_TESLA_TRIGGER_CMD");
            this.m_useTrapTriggerCommand = this.GetBoolValue("USE_TRAP_TRIGGER_CMD");
            this.m_validateTroopUpgradeLevels = this.GetBoolValue("VALIDATE_TROOP_UPGRADE_LEVELS");
            this.m_allowCancelBuildingConstruction = this.GetBoolValue("ALLOW_CANCEL_BUILDING_CONSTRUCTION");
            this.m_village2TrainingOnlyUseRegularStorage = this.GetBoolValue("V2_TRAINING_ONLY_USE_REGULAR_STORAGE");
            this.m_enableTroopDeletion = this.GetBoolValue("ENABLE_TROOP_DELETION");
            this.m_enablePresets = this.GetBoolValue("ENABLE_PRESETS");
            this.m_useTownHallLootPenaltyInWar = this.GetBoolValue("USE_TOWNHALL_LOOT_PENALTY_IN_WAR");
            this.m_enableNameChange = this.GetBoolValue("ENABLE_NAME_CHANGE");
            this.m_enableQuickDonate = this.GetBoolValue("ENABLE_QUICK_DONATE");
            this.m_enableQuickDonateWar = this.GetBoolValue("ENABLE_QUICK_DONATE_WAR");
            this.m_allowClanCastleDeployOnObstacles = this.GetBoolValue("ALLOW_CLANCASTLE_DEPLOY_ON_OBSTACLES");
            this.m_skeletonTriggerTesla = this.GetBoolValue("SKELETON_TRIGGER_TESLA");
            this.m_skeletonOpenClanCastle = this.GetBoolValue("SKELETON_OPEN_CC");
            this.m_castleTroopTargetFilter = this.GetBoolValue("CASTLE_TROOP_TARGET_FILTER");
            this.m_useTroopRequestSpeedUp = this.GetBoolValue("USE_TROOP_REQUEST_SPEED_UP");
            this.m_noCooldownFromMoveEditModeActive = this.GetBoolValue("NO_COOLDOWN_FROM_MOVE_EDITMODE_ACTIVE");
            this.m_scoringOnlyFromMatchedMode = this.GetBoolValue("SCORING_ONLY_FROM_MM");
            this.m_eloOffsetDampeningEnabled = this.GetBoolValue("ELO_OFFSET_DAMPENING_ENABLED");
            this.m_enableLeagues = this.GetBoolValue("ENABLE_LEAGUES");
            this.m_revengeGiveLeagueBonus = this.GetBoolValue("REVENGE_GIVE_LEAGUE_BONUS");
            this.m_revengeGiveStarBonus = this.GetBoolValue("REVENGE_GIVE_STAR_BONUS");
            this.m_allowStarsOverflowInStarBonus = this.GetBoolValue("ALLOW_STARS_OVERFLOW_IN_STAR_BONUS");
            this.m_loadVillage2AsSnapshot = this.GetBoolValue("LOAD_V2_AS_SNAPSHOT");
            this.m_readyForWarAttackCheck = this.GetBoolValue("READY_FOR_WAR_ATTACK_CHECK");
            this.m_useMoreAccurateLootCap = this.GetBoolValue("USE_MORE_ACCURATE_LOOT_CAP");
            this.m_enableDefendingAllianceTroopJump = this.GetBoolValue("ENABLE_DEFENDING_ALLIANCE_TROOP_JUMP");
            this.m_useWallWeightsForJumpSpell = this.GetBoolValue("USE_WALL_WEIGHTS_FOR_JUMP_SPELL");
            this.m_jumpWhenHitJumpable = this.GetBoolValue("JUMP_WHEN_HIT_JUMPABLE");
            this.m_slideAlongObstacles = this.GetBoolValue("SLIDE_ALONG_OBSTACLES");
            this.m_guardPostNotFunctionalUnderUpgrade = this.GetBoolValue("GUARD_POST_NOT_FUNCTIONAL_UNDER_UGPRADE");
            this.m_repathDuringFly = this.GetBoolValue("REPATH_DURING_FLY");
            this.m_useStickToClosestUnitHealer = this.GetBoolValue("USE_STICK_TO_CLOSEST_UNIT_HEALER");
            this.m_heroUsesAttackPosRandom = this.GetBoolValue("HERO_USES_ATTACK_POS_RANDOM");
            this.m_useAttackPosRandomOn1stTarget = this.GetBoolValue("USE_ATTACK_POS_RANDOM_ON_1ST_TARGET");
            this.m_targetSelectionConsidersWallsOnPath = this.GetBoolValue("TARGET_SELECTION_CONSIDERS_WALLS_ON_PATH");
            this.m_valkyriePrefers4Buildings = this.GetBoolValue("VALKYRIE_PREFERS_4_BUILDINGS");
            this.m_tighterAttackPosition = this.GetBoolValue("TIGHTER_ATTACK_POSITION");
            this.m_allianceTroopsPatrol = this.GetBoolValue("ALLIANCE_TROOPS_PATROL");
            this.m_wallBreakerUseRooms = this.GetBoolValue("WALL_BREAKER_USE_ROOMS");
            this.m_rememberOriginalTarget = this.GetBoolValue("REMEMBER_ORIGINAL_TARGET");
            this.m_ignoreAllianceAlertForNonValidTargets = this.GetBoolValue("IGNORE_ALLIANCE_ALERT_FOR_NON_VALID_TARGETS");
            this.m_restartAttackTimerOnAreaDamageTurrets = this.GetBoolValue("RESTART_ATTACK_TIMER_ON_AREA_DAMAGE_TURRETS");
            this.m_clearAlertStateIfNoTargetFound = this.GetBoolValue("CLEAR_ALERT_STATE_IF_NO_TARGET_FOUND");
            this.m_morePreciseTargetSelection = this.GetBoolValue("MORE_PRECISE_TARGET_SELECTION");
            this.m_movingUnitsUseSimpleSelect = this.GetBoolValue("MOVING_UNITS_USE_SIMPLE_SELECT");
            this.m_useSmarterHealer = this.GetBoolValue("USE_SMARTER_HEALER");
            this.m_usePoisonAvoidance = this.GetBoolValue("USE_POISON_AVOIDANCE");
            this.m_removeUntriggeredTesla = this.GetBoolValue("REMOVE_UNTRIGGERED_TESLA");

            this.m_allianceCreateResourceData = LogicDataTables.GetResourceByName(this.GetGlobalData("ALLIANCE_CREATE_RESOURCE").GetTextValue(), null);
            this.m_village2StartUnit = LogicDataTables.GetCharacterByName(this.GetGlobalData("VILLAGE2_START_UNIT").GetTextValue(), null);

            LogicGlobalData village2TroopHousingBuildCostData = this.GetGlobalData("TROOP_HOUSING_V2_COST");

            this.m_village2TroopHousingBuildCost = new int[village2TroopHousingBuildCostData.GetNumberArraySize()];

            for (int i = 0; i < this.m_village2TroopHousingBuildCost.Length; i++)
            {
                this.m_village2TroopHousingBuildCost[i] = village2TroopHousingBuildCostData.GetNumberArray(i);
            }

            LogicGlobalData village2TroopHousingBuildTimeSecsData = this.GetGlobalData("TROOP_HOUSING_V2_BUILD_TIME_SECONDS");

            this.m_village2TroopHousingBuildTimeSecs = new int[village2TroopHousingBuildTimeSecsData.GetNumberArraySize()];

            for (int i = 0; i < this.m_village2TroopHousingBuildTimeSecs.Length; i++)
            {
                this.m_village2TroopHousingBuildTimeSecs[i] = village2TroopHousingBuildTimeSecsData.GetNumberArray(i);
            }

            LogicGlobalData lootMultiplierByTownHallDifferenceObject = this.GetGlobalData("LOOT_MULTIPLIER_BY_TH_DIFF");

            this.m_lootMultiplierByTownHallDifference = new int[lootMultiplierByTownHallDifferenceObject.GetNumberArraySize()];

            for (int i = 0; i < this.m_lootMultiplierByTownHallDifference.Length; i++)
            {
                this.m_lootMultiplierByTownHallDifference[i] = lootMultiplierByTownHallDifferenceObject.GetNumberArray(i);
            }

            LogicGlobalData barrackReduceTrainingDivisorObject = this.GetGlobalData("BARRACK_REDUCE_TRAINING_DIVISOR");

            this.m_barrackReduceTrainingDivisor = new int[barrackReduceTrainingDivisorObject.GetNumberArraySize()];

            for (int i = 0; i < this.m_barrackReduceTrainingDivisor.Length; i++)
            {
                this.m_barrackReduceTrainingDivisor[i] = barrackReduceTrainingDivisorObject.GetNumberArray(i);
            }

            LogicGlobalData darkBarrackReduceTrainingDivisorObject = this.GetGlobalData("DARK_BARRACK_REDUCE_TRAINING_DIVISOR");

            this.m_darkBarrackReduceTrainingDivisor = new int[darkBarrackReduceTrainingDivisorObject.GetNumberArraySize()];

            for (int i = 0; i < this.m_darkBarrackReduceTrainingDivisor.Length; i++)
            {
                this.m_darkBarrackReduceTrainingDivisor[i] = darkBarrackReduceTrainingDivisorObject.GetNumberArray(i);
            }

            LogicGlobalData clockTowerBoostObject = this.GetGlobalData("CLOCK_TOWER_BOOST_MINS");

            this.m_clockTowerBoostSecs = new int[clockTowerBoostObject.GetNumberArraySize()];

            for (int i = 0; i < this.m_clockTowerBoostSecs.Length; i++)
            {
                this.m_clockTowerBoostSecs[i] = clockTowerBoostObject.GetNumberArray(i) * 60;
            }

            LogicGlobalData allianceScoreLimitObject = this.GetGlobalData("ALLIANCE_SCORE_LIMIT");

            this.m_allianceScoreLimit = new int[allianceScoreLimitObject.GetNumberArraySize()];

            for (int i = 0; i < this.m_allianceScoreLimit.Length; i++)
            {
                this.m_allianceScoreLimit[i] = allianceScoreLimitObject.GetNumberArray(i);
            }

            LogicGlobalData shieldHoursObject = this.GetGlobalData("SHIELD_HOURS");

            this.m_shieldHours = new int[shieldHoursObject.GetNumberArraySize()];

            for (int i = 0; i < this.m_shieldHours.Length; i++)
            {
                this.m_shieldHours[i] = shieldHoursObject.GetNumberArray(i);
            }

            LogicGlobalData destructionToShieldObject = this.GetGlobalData("DESTRUCTION_TO_SHIELD");

            this.m_destructionToShield = new int[destructionToShieldObject.GetNumberArraySize()];

            for (int i = 0; i < this.m_destructionToShield.Length; i++)
            {
                this.m_destructionToShield[i] = destructionToShieldObject.GetNumberArray(i);
            }

            Debugger.DoAssert(this.m_shieldHours.Length == this.m_destructionToShield.Length, string.Empty);
            LogicGlobalData attackShieldReduceHoursObject = this.GetGlobalData("ATTACK_SHIELD_REDUCE_HOURS");

            this.m_attackShieldReduceHours = new int[attackShieldReduceHoursObject.GetNumberArraySize()];

            for (int i = 0; i < this.m_attackShieldReduceHours.Length; i++)
            {
                this.m_attackShieldReduceHours[i] = attackShieldReduceHoursObject.GetNumberArray(i);
            }

            LogicGlobalData healStackPercentObject = this.GetGlobalData("HEAL_STACK_PERCENT");

            this.m_healStackPercent = new int[healStackPercentObject.GetNumberArraySize()];

            for (int i = 0; i < healStackPercentObject.GetNumberArraySize(); i++)
            {
                this.m_healStackPercent[i] = healStackPercentObject.GetNumberArray(i);
            }

            LogicGlobalData leagueBonusPercentageObject = this.GetGlobalData("LEAGUE_BONUS_PERCENTAGES");

            this.m_leagueBonusPercentages = new int[leagueBonusPercentageObject.GetNumberArraySize()];
            this.m_leagueBonusAltPercentages = new int[leagueBonusPercentageObject.GetNumberArraySize()];

            for (int i = 0; i < this.m_leagueBonusPercentages.Length; i++)
            {
                this.m_leagueBonusPercentages[i] = leagueBonusPercentageObject.GetNumberArray(i);
                this.m_leagueBonusAltPercentages[i] = leagueBonusPercentageObject.GetAltNumberArray(i);
            }
        }

        private LogicGlobalData GetGlobalData(string name)
        {
            return LogicDataTables.GetGlobalByName(name, null);
        }

        private bool GetBoolValue(string name)
        {
            return this.GetGlobalData(name).GetBooleanValue();
        }

        private int GetIntValue(string name)
        {
            return this.GetGlobalData(name).GetNumberValue();
        }

        public int GetFreeUnitHousingCapPercentage()
        {
            return this.m_freeUnitHousingCapPercentage;
        }

        public int GetFreeHeroHealthCap()
        {
            return this.m_freeHeroHealthCap;
        }

        public int GetStartingDiamonds()
        {
            return this.m_startingDiamonds;
        }

        public int GetStartingGold()
        {
            return this.m_startingGold;
        }

        public int GetStartingElixir()
        {
            return this.m_startingElixir;
        }

        public int GetStartingGold2()
        {
            return this.m_startingGold2;
        }

        public int GetStartingElixir2()
        {
            return this.m_startingElixir2;
        }

        public int GetLiveReplayUpdateFrequencySecs()
        {
            return this.m_liveReplayFrequencySecs;
        }

        public int GetChallengeBaseSaveCooldown()
        {
            return this.m_challengeBaseSaveCooldown;
        }

        public int GetAllianceTroopRequestCooldown()
        {
            return this.m_allianceTroopRequestCooldown;
        }

        public int GetArrangeWarCooldown()
        {
            return this.m_arrangeWarCooldown;
        }

        public int GetClanMailCooldown()
        {
            return this.m_clanMailCooldown;
        }

        public int GetReplayShareCooldown()
        {
            return this.m_replayShareCooldown;
        }

        public int GetElderKickCooldown()
        {
            return this.m_elderKickCooldown;
        }

        public int GetAllianceCreateCost()
        {
            return this.m_allianceCreateCost;
        }

        public int GetClockTowerBoostMultiplier()
        {
            return this.m_clockTowerBoostMultiplier;
        }

        public int GetResourceProductionBoostMultiplier()
        {
            return this.m_resourceProductionBoostMultiplier;
        }

        public int GetResourceProductionBoostSecs()
        {
            return this.m_resourceProductionBoostSecs;
        }

        public int GetSpellFactoryBoostMultiplier()
        {
            return this.m_spellFactoryBoostMultiplier;
        }

        public int GetSpellFactoryBoostNewMultiplier()
        {
            return this.m_spellFactoryBoostNewMultiplier;
        }

        public int GetSpellFactoryBoostSecs()
        {
            return this.m_spellFactoryBoostSecs;
        }

        public int GetBarracksBoostNewMultiplier()
        {
            return this.m_barracksBoostNewMultiplier;
        }

        public int GetBarracksBoostMultiplier()
        {
            return this.m_barracksBoostMultiplier;
        }

        public int GetBuildCancelMultiplier()
        {
            return this.m_buildCancelMultiplier;
        }

        public int GetTrainCancelMultiplier()
        {
            return this.m_trainCancelMultiplier;
        }

        public int GetSpellCancelMultiplier()
        {
            return this.m_spellCancelMultiplier;
        }

        public int GetHeroUpgradeCancelMultiplier()
        {
            return this.m_heroUpgradeCancelMultiplier;
        }

        public int GetBarracksBoostSecs()
        {
            return this.m_barracksBoostSecs;
        }

        public int GetClockTowerBoostCooldownSecs()
        {
            return this.m_clockTowerBoostCooldownSecs;
        }

        public int GetHeroRestBoostSecs()
        {
            return this.m_heroRestBoostSecs;
        }

        public int GetClampLongTimeStampsToDays()
        {
            return this.m_clampLongTimeStampsToDays;
        }

        public int GetObstacleRespawnSecs()
        {
            return this.m_obstacleRespawnSecs;
        }

        public int GetTallGrassRespawnSecs()
        {
            return this.m_tallGrassRespawnSecs;
        }

        public int GetObstacleMaxCount()
        {
            return this.m_obstacleMaxCount;
        }

        public int GetNewTrainingBoostBarracksCost()
        {
            return this.m_newTrainingBoostBarracksCost;
        }

        public int GetNewTrainingBoostLaboratoryCost()
        {
            return this.m_newTrainingBoostLaboratoryCost;
        }

        public int GetUnitHousingCostMultiplierForTotal()
        {
            return this.m_unitHousingCostMultiplierForTotal;
        }

        public int GetSpellHousingCostMultiplierForTotal()
        {
            return this.m_spellHousingCostMultiplierForTotal;
        }

        public int GetHeroHousingCostMultiplierForTotal()
        {
            return this.m_heroHousingCostMultiplierForTotal;
        }

        public int GetAllianceUnitHousingCostMultiplierForTotal()
        {
            return this.m_allianceUnitHousingCostMultiplierForTotal;
        }

        public int GetPersonalBreakLimitSeconds()
        {
            return this.m_personalBreakLimitSeconds;
        }

        public int GetMaxTroopDonationCount()
        {
            return this.m_maxTroopDonationCount;
        }

        public int GetMaxSpellDonationCount()
        {
            return this.m_maxSpellDonationCount;
        }

        public int GetDarkSpellDonationXP()
        {
            return this.m_darkSpellDonationXP;
        }

        public int GetStarBonusCooldownMinutes()
        {
            return this.m_starBonusCooldownMinutes;
        }

        public int GetChallengeCooldown()
        {
            return this.m_challengeCooldown;
        }

        public int GetNewbieShieldHours()
        {
            return this.m_newbieShieldHours;
        }

        public int GetLayoutTownHallLevelSlot2()
        {
            return this.m_layoutSlot2THLevel;
        }

        public int GetLayoutTownHallLevelSlot3()
        {
            return this.m_layoutSlot3THLevel;
        }

        public int GetLayoutTownHallLevelVillage2Slot2()
        {
            return this.m_layoutSlot2THLevelVillage2;
        }

        public int GetLayoutTownHallLevelVillage2Slot3()
        {
            return this.m_layoutSlot3THLevelVillage2;
        }

        public int GetScoreMultiplierOnAttackLose()
        {
            return this.m_scoreMultiplierOnAttackLose;
        }

        public int GetEloDampeningFactor()
        {
            return this.m_eloOffsetDampeningFactor;
        }

        public int GetEloDampeningLimit()
        {
            return this.m_eloOffsetDampeningLimit;
        }

        public int GetEloDampeningScoreLimit()
        {
            return this.m_eloOffsetDampeningScoreLimit;
        }

        public int GetShieldTriggerPercentageHousingSpace()
        {
            return this.m_shieldTriggerPercentageHousingSpace;
        }

        public int GetDefaultDefenseVillageGuard()
        {
            return this.m_defaultDefenseVillageGuard;
        }

        public int GetMinerTargetRandomPercentage()
        {
            return this.m_minerTargetRandPercentage;
        }

        public int GetMinerSpeedRandomPercentage()
        {
            return this.m_minerSpeedRandPercentage;
        }

        public int GetMinerHideTime()
        {
            return this.m_minerHideTime;
        }

        public int GetMinerHideTimeRandom()
        {
            return this.m_minerHideTimeRandom;
        }

        public int GetTownHallLootPercentage()
        {
            return this.m_townHallLootPercentage;
        }

        public int GetTargetListSize()
        {
            return this.m_targetListSize;
        }

        public int GetWallBreakerSmartCountLimit()
        {
            return this.m_wallBreakerSmartCountLimit;
        }

        public int GetWallBreakerSmartRadius()
        {
            return this.m_wallBreakerSmartRadius;
        }

        public int GetWallBreakerSmartRetargetLimit()
        {
            return this.m_wallBreakerSmartRetargetLimit;
        }

        public int GetSelectedWallTime()
        {
            return this.m_selectedWallTime;
        }

        public int GetForgetTargetTime()
        {
            return this.m_forgetTargetTime;
        }

        public int GetSkeletonSpellStorageMultipler()
        {
            return this.m_skeletonSpellStorageMultiplier;
        }

        public int GetAllianceAlertRadius()
        {
            return this.m_allianceAlertRadius;
        }

        public int GetHiddenBuildingAppearDestructionPercentage()
        {
            return this.m_hiddenBuildingAppearDestructionPercentage;
        }

        public int GetWallCostBase()
        {
            return this.m_wallCostBase;
        }

        public int GetHeroHealMultiplier()
        {
            return this.m_heroHealMultiplier;
        }

        public int GetHeroRageMultiplier()
        {
            return this.m_heroRageMultiplier;
        }

        public int GetHeroRageSpeedMultiplier()
        {
            return this.m_heroRageSpeedMultiplier;
        }

        public int GetChainedProjectileBounceCount()
        {
            return this.m_chainedProjectileBounceCount;
        }

        public int GetShrinkSpellDurationSeconds()
        {
            return this.m_shrinkSpellDurationSeconds;
        }

        public int GetResourceProductionLootPercentage(LogicResourceData data)
        {
            if (LogicDataTables.GetDarkElixirData() == data)
            {
                return this.m_darkElixirProductionLootPercentage;
            }

            return this.m_resourceProductionLootPercentage;
        }

        public int GetLootMultiplierByTownHallDiff(int townHallLevel1, int townHallLevel2)
        {
            return this.m_lootMultiplierByTownHallDifference[LogicMath.Clamp(townHallLevel1 + 4 - townHallLevel2, 0, this.m_lootMultiplierByTownHallDifference.Length - 1)];
        }

        public int[] GetBarrackReduceTrainingDevisor()
        {
            return this.m_barrackReduceTrainingDivisor;
        }

        public int[] GetDarkBarrackReduceTrainingDevisor()
        {
            return this.m_darkBarrackReduceTrainingDivisor;
        }

        public int GetWorkerCost(LogicLevel level)
        {
            int totalWorkers = level.GetWorkerManagerAt(level.GetVillageType()).GetTotalWorkers() + level.GetUnplacedObjectCount(LogicDataTables.GetWorkerData());

            switch (totalWorkers)
            {
                case 1: return this.m_workerCostSecondBuildCost;
                case 2: return this.m_workerCostThirdBuildCost;
                case 3: return this.m_workerCostFourthBuildCost;
                case 4: return this.m_workerCostFifthBuildCost;
                default: return this.m_workerCostFifthBuildCost;
            }
        }

        public int GetChallengeBaseCooldownEnabledTownHall()
        {
            return this.m_challengeBaseCooldownEnabledOnTh;
        }

        public int GetSpellTrainingCostMultiplier()
        {
            return this.m_spellTrainingCostMultiplier;
        }

        public int GetSpellSpeedUpCostMultiplier()
        {
            return this.m_spellSpeedUpCostMultiplier;
        }

        public int GetHeroHealthSpeedUpCostMultipler()
        {
            return this.m_heroHealthSpeedUpCostMultipler;
        }

        public int GetTroopRequestSpeedUpCostMultiplier()
        {
            return this.m_troopRequestSpeedUpCostMultiplier;
        }

        public int GetTroopTrainingCostMultiplier()
        {
            return this.m_troopTrainingCostMultiplier;
        }

        public int GetSpeedUpBoostCooldownCostMultiplier()
        {
            return this.m_speedUpBoostCooldownCostMultiplier;
        }

        public int GetClockTowerSpeedUpMultiplier()
        {
            return this.m_clockTowerSpeedUpMultiplier;
        }

        public int GetMinVillage2TownHallLevelForDestructObstacle()
        {
            return this.m_village2MinTownHallLevelForDestructObstacle;
        }

        public int GetAttackPreparationLengthSecs()
        {
            return this.m_attackPreparationLengthSecs;
        }

        public int GetAttackVillage2PreparationLengthSecs()
        {
            return this.m_attackVillage2PreparationLengthSecs;
        }

        public int GetAttackLengthSecs()
        {
            return this.m_attackLengthSecs;
        }

        public int GetVillage2StartUnitLevel()
        {
            return this.m_village2StartUnitLevel;
        }

        public int GetHeroRestBoostMultiplier()
        {
            return this.m_heroRestBoostMultiplier;
        }

        public int GetEnablePresetsTownHallLevel()
        {
            return this.m_enablePresetsTownHallLevel;
        }

        public int GetMaxAllianceFeedbackMessageLength()
        {
            return this.m_maxAllianceFeedbackMessageLength;
        }

        public int GetMaxMessageLength()
        {
            return this.m_maxMessageLength;
        }

        public int GetAllianceMailLength()
        {
            return this.m_maxAllianceMailLength;
        }

        public int GetUnitHousingCostMultiplier()
        {
            return this.m_unitHousingCostMultiplier;
        }

        public int GetHeroHousingCostMultiplier()
        {
            return this.m_heroHousingCostMultiplier;
        }

        public int GetSpellHousingCostMultiplier()
        {
            return this.m_spellHousingCostMultiplier;
        }

        public int GetEnableNameChangeTownHallLevel()
        {
            return this.m_enableNameChangeTownHallLevel;
        }

        public int GetDuelLootLimitFreeSpeedUps()
        {
            return this.m_duelLootLimitFreeSpeedUps;
        }

        public int GetBunkerSearchTime()
        {
            return this.m_bunkerSearchTime;
        }

        public int GetClanCastleRadius()
        {
            return this.m_clanCastleRadius;
        }

        public int GetClanDefenderSearchRadius()
        {
            return this.m_clanDefenderSearchRadius;
        }

        public int GetLootCartReengagementMinSeconds()
        {
            return this.m_lootCartReengagementMinSecs;
        }

        public int GetLootCartReengagementMaxSeconds()
        {
            return this.m_lootCartReengagementMaxSecs;
        }

        public int GetBookmarksMaxAlliances()
        {
            return this.m_bookmarksMaxAlliances;
        }

        public int GetStarBonusStarCount()
        {
            return this.m_starBonusStarCount;
        }

        public int GetLootCartEnabledTownHall()
        {
            return this.m_lootCartEnabledForTH;
        }

        public int GetWarMaxExcludeMembers()
        {
            return this.m_warMaxExcludeMembers;
        }

        public int GetCharVersusCharRandomDistanceLimit()
        {
            return this.m_charVsCharRandomDistanceLimit;
        }

        public int GetCharVersusCharRadiusForAttacker()
        {
            return this.m_charVsCharRadiusForAttacker;
        }

        public int GetWarLootPercentage()
        {
            return this.m_warLootPercentage;
        }

        public int GetBlockedAttackPositionPenalty()
        {
            return this.m_blockedAttackPositionPenalty;
        }

        public bool CastleTroopTargetFilter()
        {
            return this.m_castleTroopTargetFilter;
        }

        public bool MoreAccurateTime()
        {
            return this.m_moreAccurateTime;
        }

        public bool UseNewTraining()
        {
            return this.m_useNewTraining;
        }

        public bool UseTroopWalksOutFromTraining()
        {
            return this.m_useTroopWalksOutFromTraining;
        }

        public bool UseVillageObjects()
        {
            return this.m_useVillageObjects;
        }

        public bool UseTownHallLootPenaltyInWar()
        {
            return this.m_useTownHallLootPenaltyInWar;
        }

        public bool UseDragInTraining()
        {
            return this.m_dragInTraining;
        }

        public bool UseDragInTrainingFix()
        {
            return this.m_dragInTrainingFix;
        }

        public bool UseDragInTrainingFix2()
        {
            return this.m_dragInTrainingFix2;
        }

        public bool RevertBrokenWarLayouts()
        {
            return this.m_revertBrokenWarLayouts;
        }

        public bool LiveReplayEnabled()
        {
            return this.m_liveReplayEnabled;
        }

        public bool RemoveRevengeWhenBattleIsLoaded()
        {
            return this.m_removeRevengeWhenBattleIsLoaded;
        }

        public bool UseNewPathFinder()
        {
            return this.m_useNewPathFinder;
        }

        public bool CompleteConstructionOnlyHome()
        {
            return this.m_completeConstructionOnlyHome;
        }

        public bool UseNewSpeedUpCalculation()
        {
            return this.m_useNewSpeedUpCalculation;
        }

        public bool ClampBuildingTimes()
        {
            return this.m_clampBuildingTimes;
        }

        public bool ClampUpgradeTimes()
        {
            return this.m_clampUpgradesTimes;
        }

        public bool ClampAvatarTimersToMax()
        {
            return this.m_clampAvatarTimersToMax;
        }

        public bool StopBoostPauseWhenBoostTimeZeroOnLoad()
        {
            return this.m_stopBoostPauseWhenBoostTimeZeroOnLoad;
        }

        public bool FixClanPortalBattleNotEnding()
        {
            return this.m_fixClanPortalBattleNotEnding;
        }

        public bool FixMergeOldBarrackBoostPausing()
        {
            return this.m_fixMergeOldBarrackBoostPausing;
        }

        public bool SaveVillageObjects()
        {
            return this.m_saveVillageObjects;
        }

        public bool StartInLastUsedVillage()
        {
            return this.m_startInLastUsedVillage;
        }

        public bool WorkerForZeroBuilTime()
        {
            return this.m_workerForZeroBuildingTime;
        }

        public bool AdjustEndSubtickUseCurrentTime()
        {
            return this.m_adjustEndSubtickUseCurrentTime;
        }

        public bool CollectAllResourcesAtOnce()
        {
            return this.m_collectAllResourcesAtOnce;
        }

        public bool UseSwapBuildings()
        {
            return this.m_useSwapBuildings;
        }

        public bool TreasurySizeBasedOnTownHall()
        {
            return this.m_treasurySizeBasedOnTawnHall;
        }

        public bool UseTeslaTriggerCommand()
        {
            return this.m_useTeslaTriggerCommand;
        }

        public bool UseTrapTriggerCommand()
        {
            return this.m_useTrapTriggerCommand;
        }

        public bool ValidateTroopUpgradeLevels()
        {
            return this.m_validateTroopUpgradeLevels;
        }

        public bool AllowCancelBuildingConstruction()
        {
            return this.m_allowCancelBuildingConstruction;
        }

        public bool Village2TrainingOnlyUseRegularStorage()
        {
            return this.m_village2TrainingOnlyUseRegularStorage;
        }

        public bool EnableTroopDeletion()
        {
            return this.m_enableTroopDeletion;
        }

        public bool EnablePresets()
        {
            return this.m_enablePresets;
        }

        public bool EnableNameChange()
        {
            return this.m_enableNameChange;
        }

        public bool EnableQuickDonate()
        {
            return this.m_enableQuickDonate;
        }

        public bool EnableQuickDonateWar()
        {
            return this.m_enableQuickDonateWar;
        }

        public bool AllowClanCastleDeployOnObstacles()
        {
            return this.m_allowClanCastleDeployOnObstacles;
        }

        public bool SkeletonTriggerTesla()
        {
            return this.m_skeletonTriggerTesla;
        }

        public bool SkeletonOpenClanCastle()
        {
            return this.m_skeletonOpenClanCastle;
        }

        public bool UseTroopRequestSpeedUp()
        {
            return this.m_useTroopRequestSpeedUp;
        }

        public bool NoCooldownFromMoveEditModeActive()
        {
            return this.m_noCooldownFromMoveEditModeActive;
        }

        public bool UseVersusBattle()
        {
            return this.m_useVersusBattle;
        }

        public bool ScoringOnlyFromMatchedMode()
        {
            return this.m_scoringOnlyFromMatchedMode;
        }

        public bool EloOffsetDampeningEnabled()
        {
            return this.m_eloOffsetDampeningEnabled;
        }

        public bool EnableLeagues()
        {
            return this.m_enableLeagues;
        }

        public bool RevengeGiveLeagueBonus()
        {
            return this.m_revengeGiveLeagueBonus;
        }

        public bool RevengeGiveStarBonus()
        {
            return this.m_revengeGiveStarBonus;
        }

        public bool AllowStarsOverflowInStarBonus()
        {
            return this.m_allowStarsOverflowInStarBonus;
        }

        public bool LoadVillage2AsSnapshot()
        {
            return this.m_loadVillage2AsSnapshot;
        }

        public bool ReadyForWarAttackCheck()
        {
            return this.m_readyForWarAttackCheck;
        }

        public bool UseMoreAccurateLootCap()
        {
            return this.m_useMoreAccurateLootCap;
        }

        public bool EnableDefendingAllianceTroopJump()
        {
            return this.m_enableDefendingAllianceTroopJump;
        }

        public bool UseWallWeightsForJumpSpell()
        {
            return this.m_useWallWeightsForJumpSpell;
        }

        public bool JumpWhenHitJumpable()
        {
            return this.m_jumpWhenHitJumpable;
        }

        public bool SlideAlongObstacles()
        {
            return this.m_slideAlongObstacles;
        }

        public bool GuardPostNotFunctionalUnderUpgrade()
        {
            return this.m_guardPostNotFunctionalUnderUpgrade;
        }

        public bool RepathDuringFly()
        {
            return this.m_repathDuringFly;
        }

        public bool UseStickToClosestUnitHealer()
        {
            return this.m_useStickToClosestUnitHealer;
        }

        public bool HeroUsesAttackPosRandom()
        {
            return this.m_heroUsesAttackPosRandom;
        }

        public bool UseAttackPosRandomOn1stTarget()
        {
            return this.m_useAttackPosRandomOn1stTarget;
        }

        public bool TargetSelectionConsidersWallsOnPath()
        {
            return this.m_targetSelectionConsidersWallsOnPath;
        }

        public bool ValkyriePrefers4Buildings()
        {
            return this.m_valkyriePrefers4Buildings;
        }

        public bool TighterAttackPosition()
        {
            return this.m_tighterAttackPosition;
        }

        public bool AllianceTroopsPatrol()
        {
            return this.m_allianceTroopsPatrol;
        }

        public bool WallBreakerUseRooms()
        {
            return this.m_wallBreakerUseRooms;
        }

        public bool RememberOriginalTarget()
        {
            return this.m_rememberOriginalTarget;
        }

        public bool IgnoreAllianceAlertForNonValidTargets()
        {
            return this.m_ignoreAllianceAlertForNonValidTargets;
        }

        public bool RestartAttackTimerOnAreaDamageTurrets()
        {
            return this.m_restartAttackTimerOnAreaDamageTurrets;
        }

        public bool ClearAlertStateIfNoTargetFound()
        {
            return this.m_clearAlertStateIfNoTargetFound;
        }

        public bool MorePreciseTargetSelection()
        {
            return this.m_morePreciseTargetSelection;
        }

        public bool MovingUnitsUseSimpleSelect()
        {
            return this.m_movingUnitsUseSimpleSelect;
        }

        public bool UseSmarterHealer()
        {
            return this.m_useSmarterHealer;
        }

        public bool UsePoisonAvoidance()
        {
            return this.m_usePoisonAvoidance;
        }

        public bool RemoveUntriggeredTesla()
        {
            return this.m_removeUntriggeredTesla;
        }

        public LogicResourceData GetAllianceCreateResourceData()
        {
            return this.m_allianceCreateResourceData;
        }

        public LogicCharacterData GetVillage2StartUnitData()
        {
            return this.m_village2StartUnit;
        }

        public LogicResourceData GetAttackResource()
        {
            return LogicDataTables.GetGoldData();
        }

        public int GetVillage2FirstVictoryTrophies()
        {
            return this.m_village2FirstVictoryTrophies;
        }

        public int GetVillage2FirstVictoryGold()
        {
            return this.m_village2FirstVictoryGold;
        }

        public int GetVillage2FirstVictoryElixir()
        {
            return this.m_village2FirstVictoryElixir;
        }

        public int GetFriendlyBattleCost(int townHallLevel)
        {
            return LogicDataTables.GetTownHallLevel(townHallLevel).GetFriendlyCost();
        }

        public int GetTroopHousingBuildCostVillage2(LogicLevel level)
        {
            LogicBuildingData data = LogicDataTables.GetBuildingByName("Troop Housing2", null);

            if (data != null)
            {
                return this.m_village2TroopHousingBuildCost[LogicMath.Clamp(level.GetGameObjectManagerAt(1).GetGameObjectCountByData(data),
                                                                            0,
                                                                            this.m_village2TroopHousingBuildCost.Length - 1)];
            }

            Debugger.Error("Could not find Troop Housing2 data");

            return 0;
        }

        public int GetTroopHousingBuildTimeVillage2(LogicLevel level, int ignoreBuildingCnt)
        {
            LogicBuildingData data = LogicDataTables.GetBuildingByName("Troop Housing2", null);

            if (data != null)
            {
                return this.m_village2TroopHousingBuildTimeSecs[LogicMath.Clamp(level.GetGameObjectManagerAt(1).GetGameObjectCountByData(data) - ignoreBuildingCnt,
                                                                                0,
                                                                                this.m_village2TroopHousingBuildTimeSecs.Length - 1)];
            }

            Debugger.Error("Could not find Troop Housing2 data");

            return 0;
        }

        public int GetClockTowerBoostSecs(int upgLevel)
        {
            if (this.m_clockTowerBoostSecs.Length > upgLevel)
            {
                return this.m_clockTowerBoostSecs[upgLevel];
            }

            return this.m_clockTowerBoostSecs[this.m_clockTowerBoostSecs.Length - 1];
        }

        public int GetTutorialTrainingSpeedUpCost()
        {
            return this.m_troopTrainingSpeedUpCostTutorial;
        }

        public int GetHealStackPercent(int idx)
        {
            if (this.m_healStackPercent.Length != 0)
            {
                if (idx >= this.m_healStackPercent.Length)
                {
                    idx = this.m_healStackPercent.Length - 1;
                }

                return this.m_healStackPercent[idx];
            }

            return 100;
        }

        public int GetResourceDiamondCost(int count, LogicResourceData data)
        {
            if (LogicDataTables.GetDarkElixirData() != data)
            {
                int resourceDiamondCost100;
                int resourceDiamondCost1000;
                int resourceDiamondCost10000;
                int resourceDiamondCost100000;
                int resourceDiamondCost1000000;
                int resourceDiamondCost10000000;

                if (data.GetVillageType() == 1)
                {
                    resourceDiamondCost100 = this.m_village2ResourceDiamondCost100;
                    resourceDiamondCost1000 = this.m_village2ResourceDiamondCost1000;
                    resourceDiamondCost10000 = this.m_village2ResourceDiamondCost10000;
                    resourceDiamondCost100000 = this.m_village2resourceDiamondCost100000;
                    resourceDiamondCost1000000 = this.m_village2resourceDiamondCost1000000;
                    resourceDiamondCost10000000 = this.m_village2ResourceDiamondCost10000000;
                }
                else
                {
                    resourceDiamondCost100 = this.m_resourceDiamondCost100;
                    resourceDiamondCost1000 = this.m_resourceDiamondCost1000;
                    resourceDiamondCost10000 = this.m_resourceDiamondCost10000;
                    resourceDiamondCost100000 = this.m_resourceDiamondCost100000;
                    resourceDiamondCost1000000 = this.m_resourceDiamondCost1000000;
                    resourceDiamondCost10000000 = this.m_resourceDiamondCost10000000;
                }

                if (count >= 1)
                {
                    if (count >= 100)
                    {
                        if (count >= 1000)
                        {
                            if (count >= 10000)
                            {
                                if (count >= 100000)
                                {
                                    if (count >= 1000000)
                                    {
                                        return resourceDiamondCost1000000 + ((resourceDiamondCost10000000 - resourceDiamondCost1000000) * (count / 1000 - 1000) + 4500) / 9000;
                                    }

                                    return resourceDiamondCost100000 + ((resourceDiamondCost1000000 - resourceDiamondCost100000) * (count / 100 - 1000) + 4500) / 9000;
                                }

                                return resourceDiamondCost10000 + ((resourceDiamondCost100000 - resourceDiamondCost10000) * (count / 10 - 1000) + 4500) / 9000;
                            }

                            return resourceDiamondCost1000 + ((resourceDiamondCost10000 - resourceDiamondCost1000) * (count - 1000) + 4500) / 9000;
                        }

                        return resourceDiamondCost100 + ((resourceDiamondCost1000 - resourceDiamondCost100) * (count - 100) + 450) / 900;
                    }

                    return resourceDiamondCost100;
                }

                return 0;
            }

            return this.GetDarkElixirDiamondCost(count);
        }

        public int GetDarkElixirDiamondCost(int count)
        {
            if (count >= 1)
            {
                if (count >= 10)
                {
                    if (count >= 100)
                    {
                        if (count >= 1000)
                        {
                            if (count >= 10000)
                            {
                                return this.m_darkElixirDiamondCost10000 +
                                       ((this.m_darkElixirDiamondCost100000 - this.m_darkElixirDiamondCost10000) * (count - 10000) + 45000) / 90000;
                            }

                            return this.m_darkElixirDiamondCost1000 + ((this.m_darkElixirDiamondCost10000 - this.m_darkElixirDiamondCost1000) * (count - 1000) + 4500) / 9000;
                        }

                        return this.m_darkElixirDiamondCost100 + ((this.m_darkElixirDiamondCost1000 - this.m_darkElixirDiamondCost100) * (count - 100) + 450) / 900;
                    }

                    return this.m_darkElixirDiamondCost10 + ((this.m_darkElixirDiamondCost100 - this.m_darkElixirDiamondCost10) * (count - 10) + 45) / 90;
                }

                return this.m_darkElixirDiamondCost1 + ((this.m_darkElixirDiamondCost10 - this.m_darkElixirDiamondCost1) * (count - 1) + 4) / 9;
            }

            return 0;
        }

        public int GetSpeedUpCost(int time, int multiplier, int villageType)
        {
            if (time > 0)
            {
                int speedUpDiamondCostPerMin;
                int speedUpDiamondCostPerHour;
                int speedUpDiamondCostPerDay;
                int speedUpDiamondCostPerWeek;

                if (villageType == 1)
                {
                    speedUpDiamondCostPerMin = this.m_speedUpDiamondCostPerMinVillage2;
                    speedUpDiamondCostPerHour = this.m_speedUpDiamondCostPerHourVillage2;
                    speedUpDiamondCostPerDay = this.m_speedUpDiamondCostPerDayVillage2;
                    speedUpDiamondCostPerWeek = this.m_speedUpDiamondCostPerWeekVillage2;
                }
                else
                {
                    speedUpDiamondCostPerMin = this.m_speedUpDiamondCostPerMin;
                    speedUpDiamondCostPerHour = this.m_speedUpDiamondCostPerHour;
                    speedUpDiamondCostPerDay = this.m_speedUpDiamondCostPerDay;
                    speedUpDiamondCostPerWeek = this.m_speedUpDiamondCostPerWeek;
                }

                int multiplier1 = multiplier;
                int multiplier2 = 100;

                if (this.m_useNewSpeedUpCalculation)
                {
                    multiplier1 = 100;
                    multiplier2 = multiplier;
                }

                int cost = speedUpDiamondCostPerMin;

                if (time >= 60)
                {
                    if (time >= 3600)
                    {
                        if (time >= 86400)
                        {
                            int tmp1 = (speedUpDiamondCostPerWeek - speedUpDiamondCostPerDay) * (time - 86400);

                            cost = multiplier2 * speedUpDiamondCostPerDay / 100 + tmp1 / 100 * multiplier2 / 518400;

                            if (cost < 0 || tmp1 / 100 > 0x7FFFFFFF / multiplier2)
                            {
                                cost = multiplier2 * (speedUpDiamondCostPerDay + tmp1 / 518400) / 100;
                            }
                        }
                        else
                        {
                            cost = multiplier2 * speedUpDiamondCostPerHour / 100 +
                                   (speedUpDiamondCostPerDay - speedUpDiamondCostPerHour) * (time - 3600) / 100 * multiplier2 / 82800;
                        }
                    }
                    else
                    {
                        cost = multiplier2 * speedUpDiamondCostPerMin / 100 + (speedUpDiamondCostPerHour - speedUpDiamondCostPerMin) * (time - 60) * multiplier2 / 354000;
                    }
                }
                else if (this.m_useNewSpeedUpCalculation)
                {
                    cost = multiplier2 * speedUpDiamondCostPerMin * time / 6000;
                }

                return LogicMath.Max(cost * multiplier1 / 100, 1);
            }

            return 0;
        }

        public int GetLeagueBonusPercentage(int destructionPercentage)
        {
            if (this.m_leagueBonusPercentages.Length != 0 && this.m_leagueBonusAltPercentages.Length != 0)
            {
                for (int i = 0, j = 0, k = 0; i < this.m_leagueBonusPercentages.Length; i++)
                {
                    if (this.m_leagueBonusPercentages[i] >= destructionPercentage)
                    {
                        return k + (this.m_leagueBonusAltPercentages[i] - k) * (destructionPercentage - j) / (this.m_leagueBonusPercentages[i] - j);
                    }

                    j = this.m_leagueBonusPercentages[i];
                    k = this.m_leagueBonusAltPercentages[i];
                }
            }

            return 100;
        }

        public int GetAllianceScoreLimit(int idx)
        {
            return this.m_allianceScoreLimit[idx];
        }

        public int GetAllianceScoreLimitCount()
        {
            return this.m_allianceScoreLimit.Length;
        }

        public int GetDestructionToShield(int destructionPercentage)
        {
            int shield = 0;

            for (int i = 0; i < this.m_destructionToShield.Length; i++)
            {
                if (this.m_destructionToShield[i] <= destructionPercentage)
                {
                    shield = this.m_shieldHours[i];
                }
            }

            return shield;
        }

        public int GetAttackShieldReduceHours(int idx)
        {
            if (idx >= this.m_attackShieldReduceHours.Length)
            {
                idx = this.m_attackShieldReduceHours.Length - 1;
            }

            return this.m_attackShieldReduceHours[idx];
        }
    }
}