namespace Supercell.Magic.Logic.Avatar
{
    using Supercell.Magic.Logic.Calendar;
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.League.Entry;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicClientAvatar : LogicAvatar
    {
        private LogicLong m_id;
        private LogicLong m_currentHomeId;
        private LogicLong m_allianceId;
        private LogicLong m_leagueInstanceId;
        private LogicLong m_challengeId;
        private LogicLong m_warInstanceId;

        private LogicLegendSeasonEntry m_legendSeasonEntry;
        private LogicLegendSeasonEntry m_legendSeasonEntryVillage2;

        private bool m_nameSetByUser;
        private bool m_allianceChatFilter;

        private LogicAvatarAllianceRole m_allianceRole;
        private int m_allianceExpLevel;
        private int m_legendaryScore;
        private int m_legendaryScoreVillage2;
        private int m_expLevel;
        private int m_expPoints;
        private int m_diamonds;
        private int m_freeDiamonds;
        private int m_cumulativePurchasedDiamonds;
        private int m_score;
        private int m_duelScore;
        private int m_warPreference;
        private int m_attackRating;
        private int m_attackKFactor;
        private int m_attackWinCount;
        private int m_attackLoseCount;
        private int m_defenseWinCount;
        private int m_defenseLoseCount;
        private int m_treasuryGoldCount;
        private int m_treasuryElixirCount;
        private int m_treasuryDarkElixirCount;
        private int m_nameChangeState;
        private int m_attackShieldReduceCounter;
        private int m_defenseVillageGuardCounter;
        private int m_duelWinCount;
        private int m_duelLoseCount;
        private int m_duelDrawCount;
        private int m_challengeState;

        private string m_facebookId;
        private string m_allianceName;
        private string m_name;

        public LogicClientAvatar()
        {
            this.m_legendSeasonEntry = new LogicLegendSeasonEntry();
            this.m_legendSeasonEntryVillage2 = new LogicLegendSeasonEntry();

            this.m_expLevel = 1;
            this.m_allianceBadgeId = -1;
            this.m_nameChangeState = -1;
            this.m_attackRating = 1200;
            this.m_attackKFactor = 60;
            this.m_warPreference = 1;

            this.InitBase();
        }

        public override void Destruct()
        {
            base.Destruct();

            if (this.m_legendSeasonEntry != null)
            {
                this.m_legendSeasonEntry.Destruct();
                this.m_legendSeasonEntry = null;
            }

            if (this.m_legendSeasonEntryVillage2 != null)
            {
                this.m_legendSeasonEntryVillage2.Destruct();
                this.m_legendSeasonEntryVillage2 = null;
            }

            this.m_id = null;
            this.m_currentHomeId = null;
            this.m_leagueInstanceId = null;
            this.m_allianceId = null;
            this.m_allianceName = null;
            this.m_facebookId = null;
            this.m_name = null;
        }

        public override void InitBase()
        {
            base.InitBase();

            this.m_name = string.Empty;

            this.m_id = new LogicLong();
            this.m_currentHomeId = new LogicLong();
            this.m_warInstanceId = new LogicLong();
        }

        public override void GetChecksum(ChecksumHelper checksumHelper)
        {
            checksumHelper.StartObject("LogicClientAvatar");
            base.GetChecksum(checksumHelper);
            checksumHelper.WriteValue("m_expPoints", this.m_expPoints);
            checksumHelper.WriteValue("m_expLevel", this.m_expLevel);
            checksumHelper.WriteValue("m_diamonds", this.m_diamonds);
            checksumHelper.WriteValue("m_freeDiamonds", this.m_freeDiamonds);
            checksumHelper.WriteValue("m_score", this.m_score);
            checksumHelper.WriteValue("m_duelScore", this.m_duelScore);

            if (this.IsInAlliance())
            {
                checksumHelper.WriteValue("isInAlliance", 13);
            }

            checksumHelper.EndObject();
        }

        public static LogicClientAvatar GetDefaultAvatar()
        {
            LogicClientAvatar defaultAvatar = new LogicClientAvatar();
            LogicGlobals globalsInstance = LogicDataTables.GetGlobals();

            defaultAvatar.m_diamonds = globalsInstance.GetStartingDiamonds();
            defaultAvatar.m_freeDiamonds = globalsInstance.GetStartingDiamonds();

            defaultAvatar.SetResourceCount(LogicDataTables.GetGoldData(), globalsInstance.GetStartingGold());
            defaultAvatar.SetResourceCount(LogicDataTables.GetGold2Data(), globalsInstance.GetStartingGold2());
            defaultAvatar.SetResourceCount(LogicDataTables.GetElixirData(), globalsInstance.GetStartingElixir());
            defaultAvatar.SetResourceCount(LogicDataTables.GetElixir2Data(), globalsInstance.GetStartingElixir2());

            return defaultAvatar;
        }

        public override bool IsClientAvatar()
        {
            return true;
        }

        public override LogicLong GetAllianceId()
        {
            return this.m_allianceId;
        }

        public void SetAllianceId(LogicLong value)
        {
            this.m_allianceId = value;
        }

        public override string GetAllianceName()
        {
            return this.m_allianceName;
        }

        public void SetAllianceName(string value)
        {
            this.m_allianceName = value;
        }

        public override LogicAvatarAllianceRole GetAllianceRole()
        {
            return this.m_allianceRole;
        }

        public void SetAllianceRole(LogicAvatarAllianceRole value)
        {
            this.m_allianceRole = value;
        }

        public override int GetAllianceBadgeId()
        {
            return this.m_allianceBadgeId;
        }

        public void SetAllianceBadgeId(int value)
        {
            this.m_allianceBadgeId = value;
        }

        public int GetAllianceLevel()
        {
            return this.m_allianceExpLevel;
        }

        public void SetAllianceLevel(int value)
        {
            this.m_allianceExpLevel = value;
        }

        public int GetWarPreference()
        {
            return this.m_warPreference;
        }

        public void SetWarPreference(int preference)
        {
            this.m_warPreference = preference;
        }

        public override bool IsInAlliance()
        {
            return this.m_allianceId != null;
        }

        public override int GetExpLevel()
        {
            return this.m_expLevel;
        }

        public void SetExpLevel(int expLevel)
        {
            this.m_expLevel = expLevel;
        }

        public int GetExpPoints()
        {
            return this.m_expPoints;
        }

        public void SetExpPoints(int expPoints)
        {
            this.m_expPoints = expPoints;
        }

        public LogicLong GetId()
        {
            return this.m_id;
        }

        public void SetId(LogicLong id)
        {
            this.m_id = id;
        }

        public LogicLong GetCurrentHomeId()
        {
            return this.m_currentHomeId;
        }

        public void SetCurrentHomeId(LogicLong id)
        {
            this.m_currentHomeId = id;
        }

        public bool GetNameSetByUser()
        {
            return this.m_nameSetByUser;
        }

        public void SetNameSetByUser(bool set)
        {
            this.m_nameSetByUser = set;
        }

        public int GetNameChangeState()
        {
            return this.m_nameChangeState;
        }

        public void SetNameChangeState(int state)
        {
            this.m_nameChangeState = state;
        }

        public override string GetName()
        {
            return this.m_name;
        }

        public void SetName(string name)
        {
            this.m_name = name;
        }

        public string GetFacebookId()
        {
            return this.m_facebookId;
        }

        public void SetFacebookId(string facebookId)
        {
            this.m_facebookId = facebookId;
        }

        public bool GetAllianceChatFilterEnabled()
        {
            return this.m_allianceChatFilter;
        }

        public void SetAllianceChatFilterEnabled(bool enabled)
        {
            this.m_allianceChatFilter = enabled;
        }

        public bool HasEnoughDiamonds(int count, bool callListener, LogicLevel level)
        {
            bool enough = this.m_diamonds >= count;

            if (!enough && callListener)
            {
                level.GetGameListener().NotEnoughDiamonds();
            }

            return enough;
        }

        public bool HasEnoughResources(LogicResourceData data, int count, bool callListener, LogicCommand command, bool unk)
        {
            bool enough = this.GetResourceCount(data) >= count;

            if (callListener && !enough)
            {
                if (this.m_level != null)
                {
                    this.m_level.GetGameListener().NotEnoughResources(data, count, command, unk);
                }
            }

            return enough;
        }

        public bool HasEnoughResources(LogicResourceData data1, int count1, LogicResourceData data2, int count2, bool callListener, LogicCommand command, bool unk)
        {
            int resourceCount1 = this.GetResourceCount(data1);
            int resourceCount2 = this.GetResourceCount(data2);

            bool enough = resourceCount1 >= count1 && resourceCount2 >= count2;

            if (callListener && !enough)
            {
                if (this.m_level != null)
                {
                    if (resourceCount1 >= count1 || resourceCount2 >= count2)
                    {
                        if (resourceCount1 >= count1)
                        {
                            if (resourceCount2 < count2)
                            {
                                this.m_level.GetGameListener().NotEnoughResources(data2, count2, command, unk);
                            }
                        }
                        else
                        {
                            this.m_level.GetGameListener().NotEnoughResources(data1, count1, command, unk);
                        }
                    }
                    else
                    {
                        this.m_level.GetGameListener().NotEnoughResources(data1, count1, data2, count2, command, unk);
                    }
                }
            }

            return enough;
        }

        public int GetDiamonds()
        {
            return this.m_diamonds;
        }

        public void SetDiamonds(int count)
        {
            this.m_diamonds = count;
        }

        public void UseDiamonds(int count)
        {
            this.m_diamonds -= count;

            if (this.m_freeDiamonds > this.m_diamonds)
            {
                this.m_freeDiamonds = this.m_diamonds;
            }
        }

        public int GetFreeDiamonds()
        {
            return this.m_freeDiamonds;
        }

        public void SetFreeDiamonds(int count)
        {
            this.m_freeDiamonds = count;
        }

        public void AddCumulativePurchasedDiamonds(int count)
        {
            this.m_cumulativePurchasedDiamonds += count;
        }

        public int GetCumulativePurchasedDiamonds()
        {
            return this.m_cumulativePurchasedDiamonds;
        }

        public int GetLeagueType()
        {
            return LogicMath.Clamp(this.m_leagueType, 0, LogicDataTables.GetTable(LogicDataType.LEAGUE).GetItemCount() - 1);
        }

        public void SetLeagueType(int value)
        {
            this.m_leagueType = value;
        }

        public int GetAttackWinCount()
        {
            return this.m_attackWinCount;
        }

        public void SetAttackWinCount(int value)
        {
            this.m_attackWinCount = value;
        }

        public int GetAttackLoseCount()
        {
            return this.m_attackLoseCount;
        }

        public void SetAttackLoseCount(int value)
        {
            this.m_attackLoseCount = value;
        }

        public int GetDefenseWinCount()
        {
            return this.m_defenseWinCount;
        }

        public void SetDefenseWinCount(int value)
        {
            this.m_defenseWinCount = value;
        }

        public int GetDefenseLoseCount()
        {
            return this.m_defenseLoseCount;
        }

        public void SetDefenseLoseCount(int value)
        {
            this.m_defenseLoseCount = value;
        }

        public int GetDuelWinCount()
        {
            return this.m_duelWinCount;
        }

        public void SetDuelWinCount(int value)
        {
            this.m_duelWinCount = value;
        }

        public int GetDuelLoseCount()
        {
            return this.m_duelLoseCount;
        }

        public void SetDuelLoseCount(int value)
        {
            this.m_duelLoseCount = value;
        }

        public int GetDuelDrawCount()
        {
            return this.m_duelDrawCount;
        }

        public void SetDuelDrawCount(int value)
        {
            this.m_duelDrawCount = value;
        }

        public LogicLong GetLeagueInstanceId()
        {
            return this.m_leagueInstanceId;
        }

        public void SetLeagueInstanceId(LogicLong id)
        {
            this.m_leagueInstanceId = id;
        }

        public int GetAttackShieldReduceCounter()
        {
            return this.m_attackShieldReduceCounter;
        }

        public void SetAttackShieldReduceCounter(int value)
        {
            this.m_attackShieldReduceCounter = value;
        }

        public int GetDefenseVillageGuardCounter()
        {
            return this.m_defenseVillageGuardCounter;
        }

        public void SetDefenseVillageGuardCounter(int value)
        {
            this.m_defenseVillageGuardCounter = value;
        }

        public int GetScore()
        {
            return this.m_score;
        }

        public int GetDuelScore()
        {
            return this.m_duelScore;
        }

        public void SetDuelScore(int score)
        {
            this.m_duelScore = score;
        }

        public void SetScore(int value)
        {
            this.m_score = value;
        }

        public int GetLegendaryScore()
        {
            return this.m_legendaryScore;
        }

        public void SetLegendaryScore(int value)
        {
            this.m_legendaryScore = value;
        }

        public int GetLegendaryScoreVillage2()
        {
            return this.m_legendaryScoreVillage2;
        }

        public void SetLegendaryScoreVillage2(int value)
        {
            this.m_legendaryScoreVillage2 = value;
        }

        public LogicLegendSeasonEntry GetLegendSeasonEntry()
        {
            return this.m_legendSeasonEntry;
        }

        public LogicLegendSeasonEntry GetLegendSeasonEntryVillage2()
        {
            return this.m_legendSeasonEntryVillage2;
        }

        public int GetChallengeState()
        {
            return this.m_challengeState;
        }

        public void SetChallengeState(int value)
        {
            this.m_challengeState = value;
        }

        public LogicLong GetChallengeId()
        {
            return this.m_challengeId;
        }

        public void SetChallengeId(LogicLong value)
        {
            this.m_challengeId = value;
        }

        public override int GetResourceCount(LogicResourceData data)
        {
            if (data.IsPremiumCurrency())
            {
                return this.m_diamonds;
            }

            return base.GetResourceCount(data);
        }

        public override LogicLeagueData GetLeagueTypeData()
        {
            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.LEAGUE);
            Debugger.DoAssert(this.m_leagueType > -1 && table.GetItemCount() > this.m_leagueType, "Player league ranking out of bounds");
            return (LogicLeagueData) table.GetItemAt(this.m_leagueType);
        }

        public override void XpGainHelper(int count)
        {
            if (count > 0)
            {
                int maxExpPoints = LogicDataTables.GetExperienceLevel(this.m_expLevel).GetMaxExpPoints();

                if (this.m_expLevel < LogicExperienceLevelData.GetLevelCap())
                {
                    int gainExpPoints = this.m_expPoints + count;

                    if (gainExpPoints >= maxExpPoints)
                    {
                        if (this.m_expLevel + 1 == LogicExperienceLevelData.GetLevelCap())
                        {
                            gainExpPoints = maxExpPoints;
                        }

                        gainExpPoints -= maxExpPoints;

                        this.m_expLevel += 1;
                        this.m_listener.ExpLevelGained(gainExpPoints);

                        if (this.m_level != null)
                        {
                            if (this.m_level.GetPlayerAvatar() == this)
                            {
                                this.m_level.GetGameListener().LevelUp(this.m_expLevel);
                            }

                            if (this.m_level.GetHomeOwnerAvatar() == this)
                            {
                                this.m_level.RefreshNewShopUnlocksExp();
                            }
                        }
                    }
                    else
                    {
                        this.m_listener.ExpPointsGained(gainExpPoints);
                    }

                    this.m_expPoints = gainExpPoints;
                }
            }
        }

        public void RemoveUnitsVillage2()
        {
            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.CHARACTER);

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicCharacterData characterData = (LogicCharacterData) table.GetItemAt(i);

                if (characterData.GetVillageType() == 1)
                {
                    this.SetUnitCountVillage2(characterData, 0);
                    this.m_listener.CommodityCountChanged(7, characterData, 0);
                }
            }
        }

        public void AddMissionResourceReward(LogicResourceData resourceData, int count)
        {
            if (resourceData != null)
            {
                if (count > 0)
                {
                    this.SetResourceCount(resourceData, this.GetResourceCount(resourceData) + count);
                    this.m_listener.CommodityCountChanged(0, resourceData, count);
                }
            }
        }

        public override bool AddDuelReward(int goldCount, int elixirCount, int bonusGoldCount, int bonusElixirCount, LogicLong matchId)
        {
            if (goldCount > 0 || elixirCount > 0)
            {
                this.m_level.RefreshResourceCaps();
                this.SetVariableByName("LootLimitWinCount", this.GetVariableByName("LootLimitWinCount") + 1);

                int goldCap = this.GetResourceCap(LogicDataTables.GetGold2Data());
                int elixirCap = this.GetResourceCap(LogicDataTables.GetElixir2Data());

                if (this.GetVariableByName("LootLimitCooldown") != 1)
                {
                    int lootLimitWinCount = this.GetVariableByName("LootLimitWinCount");

                    if (lootLimitWinCount >= this.m_level.GetGameMode().GetConfiguration().GetDuelBonusLimitWinsPerDay())
                    {
                        this.StartLootLimitCooldown();

                        if (bonusGoldCount > 0)
                        {
                            this.AddResource(0, LogicDataTables.GetGold2Data(), bonusGoldCount, goldCap);
                        }

                        if (bonusElixirCount > 0)
                        {
                            this.AddResource(0, LogicDataTables.GetElixir2Data(), bonusElixirCount, elixirCap);
                        }
                    }
                }

                if (goldCount != 0)
                {
                    this.AddResource(0, LogicDataTables.GetGold2Data(), goldCount, goldCap);
                }

                if (elixirCount != 0)
                {
                    this.AddResource(0, LogicDataTables.GetElixir2Data(), elixirCount, elixirCap);
                }
            }

            return true;
        }

        public override bool AddStarBonusReward(int goldCount, int elixirCount, int darkElixirCount)
        {
            int currentWarGoldCap = this.GetResourceCap(LogicDataTables.GetWarGoldData());
            int currentWarElixirCap = this.GetResourceCap(LogicDataTables.GetWarElixirData());
            int currentWarDarkElixirCap = this.GetResourceCap(LogicDataTables.GetWarDarkElixirData());

            this.m_level.RefreshResourceCaps();

            int updatedWarGoldCap = this.GetResourceCap(LogicDataTables.GetWarGoldData());
            int updatedWarElixirCap = this.GetResourceCap(LogicDataTables.GetWarElixirData());
            int updatedWarDarkElixirCap = this.GetResourceCap(LogicDataTables.GetWarDarkElixirData());

            if (goldCount != 0)
            {
                this.AddResource(0, LogicDataTables.GetWarGoldData(), goldCount, LogicMath.Max(currentWarGoldCap, updatedWarGoldCap));
            }

            if (elixirCount != 0)
            {
                this.AddResource(0, LogicDataTables.GetWarElixirData(), elixirCount, LogicMath.Max(currentWarElixirCap, updatedWarElixirCap));
            }

            if (darkElixirCount != 0 && this.IsDarkElixirUnlocked())
            {
                this.AddResource(0, LogicDataTables.GetWarDarkElixirData(), darkElixirCount, LogicMath.Max(currentWarDarkElixirCap, updatedWarDarkElixirCap));
            }
            else
            {
                darkElixirCount = 0;
            }

            this.m_level.GetGameListener().StarBonusAdded(goldCount, elixirCount, darkElixirCount);

            if (this.m_listener != null)
            {
                this.m_listener.StarBonusAdded(goldCount, elixirCount, darkElixirCount);
            }

            return true;
        }

        public override bool AddWarReward(int gold, int elixir, int darkElixir, int unk, LogicLong warInstanceId)
        {
            if (warInstanceId != null && !this.m_warInstanceId.Equals(warInstanceId))
            {
                this.m_treasuryGoldCount += gold;
                this.m_treasuryElixirCount += elixir;
                this.m_treasuryDarkElixirCount = darkElixir;

                this.m_warInstanceId = warInstanceId;

                return true;
            }

            return false;
        }

        public override void FastForwardLootLimit(int secs)
        {
            int remainingSecs = this.GetRemainingLootLimitTime();

            if (remainingSecs <= secs)
            {
                if (this.GetVariableByName("LootLimitCooldown") == 1)
                {
                    this.RestartLootLimitTimer(secs - remainingSecs, this.m_level.GetHomeOwnerAvatarChangeListener().GetCurrentTimestamp());
                    this.SetVariableByName("LootLimitCooldown", 0);
                    this.ResetLootLimitWinCount();
                }
                else
                {
                    this.SetVariableByName("LootLimitTimerEndSubTick", this.m_level.GetLogicTime().GetTick());
                }
            }
            else
            {
                int endSubtick = this.GetVariableByName("LootLimitTimerEndSubTick");
                int endTimestamp = this.GetVariableByName("LootLimitTimerEndTimestamp");
                int logicTime = this.m_level.GetLogicTime().GetTick();
                int currentTimestamp = this.m_level.GetGameMode().GetStartTime();
                int remainingTime = 60 * (endTimestamp - currentTimestamp);

                if (endTimestamp < 1 || currentTimestamp == -1)
                {
                    logicTime = endSubtick;
                    remainingTime = -60 * secs;
                }

                endSubtick = logicTime + remainingTime;

                if (LogicDataTables.GetGlobals().ClampAvatarTimersToMax())
                {
                    endSubtick = this.m_level.GetLogicTime().GetTick() + 60 * LogicMath.Clamp((endSubtick - this.m_level.GetLogicTime().GetTick()) / 60, 1,
                                                                                              60 * LogicCalendar.GetDuelLootLimitCooldownInMinutes(
                                                                                                  this.m_level.GetGameMode().GetCalendar(),
                                                                                                  this.m_level.GetGameMode().GetConfiguration()));
                }

                this.SetVariableByName("LootLimitTimerEndSubTick", endSubtick);
            }
        }

        public override void FastForwardStarBonusLimit(int secs)
        {
            int remainingSecs = this.GetRemainingStarBonusTime();

            if (remainingSecs <= secs)
            {
                if (this.GetVariableByName("StarBonusCooldown") == 1)
                {
                    this.RestartStartBonusLimitTimer(secs - remainingSecs, this.m_level.GetHomeOwnerAvatarChangeListener().GetCurrentTimestamp());
                    this.SetVariableByName("StarBonusCooldown", 0);
                }
                else
                {
                    this.SetVariableByName("StarBonusTimerEndSubTick", this.m_level.GetLogicTime().GetTick());
                }
            }
            else
            {
                int endSubtick = this.GetVariableByName("StarBonusTimerEndSubTick");
                int endTimestamp = this.GetVariableByName("StarBonusTimerEndTimestep");
                int logicTime = this.m_level.GetLogicTime().GetTick();
                int currentTimestamp = this.m_level.GetGameMode().GetStartTime();
                int remainingTime = 60 * (endTimestamp - currentTimestamp);

                if (endTimestamp < 1 || currentTimestamp == -1)
                {
                    logicTime = endSubtick;
                    remainingTime = -60 * secs;
                }

                endSubtick = logicTime + remainingTime;

                if (LogicDataTables.GetGlobals().ClampAvatarTimersToMax())
                {
                    endSubtick = this.m_level.GetLogicTime().GetTick() + 60 * LogicMath.Clamp((endSubtick - this.m_level.GetLogicTime().GetTick()) / 60, 1,
                                                                                              60 * LogicCalendar.GetDuelLootLimitCooldownInMinutes(
                                                                                                  this.m_level.GetGameMode().GetCalendar(),
                                                                                                  this.m_level.GetGameMode().GetConfiguration()));
                }

                this.SetVariableByName("StarBonusTimerEndSubTick", endSubtick);
            }
        }

        public void AddResource(int commodityType, LogicResourceData resourceData, int gainCount, int resourceCap)
        {
            int resourceCount = this.GetResourceCount(resourceData);
            int newCount = LogicMath.Max(resourceCount + gainCount, 0);

            if (gainCount <= 0)
            {
                this.SetResourceCount(resourceData, newCount);
                this.GetChangeListener().CommodityCountChanged(commodityType, resourceData, newCount);
            }
            else
            {
                newCount = LogicMath.Min(newCount, resourceCap);
                resourceCount = LogicMath.Min(resourceCount, resourceCap);

                if (newCount > resourceCount)
                {
                    this.SetResourceCount(resourceData, newCount);
                    this.GetChangeListener().CommodityCountChanged(commodityType, resourceData, newCount);
                }
            }
        }

        public void StartLootLimitCooldown()
        {
            int lootLimitFreeSpeedUp = this.GetVariableByName("LootLimitFreeSpeedUp");

            if (lootLimitFreeSpeedUp >= LogicDataTables.GetGlobals().GetDuelLootLimitFreeSpeedUps())
            {
                if (this.GetRemainingLootLimitTime() <= 0)
                {
                    this.RestartLootLimitTimer(0, this.m_level.GetHomeOwnerAvatarChangeListener().GetCurrentTimestamp());
                    this.ResetLootLimitWinCount();
                }
                else
                {
                    this.SetVariableByName("LootLimitCooldown", 1);
                }
            }
            else
            {
                this.RestartLootLimitTimer(0, this.m_level.GetHomeOwnerAvatarChangeListener().GetCurrentTimestamp());
                this.ResetLootLimitWinCount();
                this.SetVariableByName("LootLimitFreeSpeedUp", lootLimitFreeSpeedUp + 1);
            }
        }

        public int GetRemainingLootLimitTime()
        {
            int remainingSubtick = this.GetVariableByName("LootLimitTimerEndSubTick") - this.m_level.GetLogicTime().GetTick();

            if (remainingSubtick <= 0)
            {
                return 0;
            }

            return LogicMath.Max(1, (remainingSubtick + 59) / 60);
        }

        public int GetRemainingStarBonusTime()
        {
            int remainingSubtick = this.GetVariableByName("StarBonusTimerEndSubTick") - this.m_level.GetLogicTime().GetTick();

            if (remainingSubtick <= 0)
            {
                return 0;
            }

            return LogicMath.Max(1, (remainingSubtick + 59) / 60);
        }

        public void RestartLootLimitTimer(int passedSecs, int timestamp)
        {
            int secs = LogicCalendar.GetDuelLootLimitCooldownInMinutes(this.m_level.GetGameMode().GetCalendar(), this.m_level.GetGameMode().GetConfiguration()) * 60 - passedSecs;

            if (secs <= passedSecs)
            {
                this.SetVariableByName("LootLimitTimerEndSubTick", this.m_level.GetLogicTime().GetTick());
            }
            else
            {
                this.SetVariableByName("LootLimitTimerEndSubTick", this.m_level.GetLogicTime().GetTick() + secs * 60);

                if (timestamp != -1)
                {
                    this.SetVariableByName("LootLimitTimerEndTimestamp", timestamp + secs);
                }
            }
        }

        public void RestartStartBonusLimitTimer(int passedSecs, int timestamp)
        {
            int secs = LogicDataTables.GetGlobals().GetStarBonusCooldownMinutes() * 60 - passedSecs;

            if (secs <= passedSecs)
            {
                this.SetVariableByName("StarBonusTimerEndSubTick", this.m_level.GetLogicTime().GetTick());
            }
            else
            {
                this.SetVariableByName("StarBonusTimerEndSubTick", this.m_level.GetLogicTime().GetTick() + secs * 60);

                if (timestamp != -1)
                {
                    this.SetVariableByName("StarBonusTimerEndTimestep", timestamp + secs);
                }
            }
        }

        public override void UpdateLootLimitCooldown()
        {
            if (this.GetRemainingLootLimitTime() <= 0)
            {
                if (this.GetVariableByName("LootLimitCooldown") == 1)
                {
                    this.RestartLootLimitTimer(0, this.m_level.GetHomeOwnerAvatarChangeListener().GetCurrentTimestamp());
                    this.SetVariableByName("LootLimitCooldown", 0);
                    this.ResetLootLimitWinCount();
                }
            }
        }

        public override void UpdateStarBonusLimitCooldown()
        {
            if (this.GetRemainingStarBonusTime() <= 0)
            {
                if (this.GetVariableByName("StarBonusCooldown") == 1)
                {
                    this.RestartStartBonusLimitTimer(0, this.m_level.GetHomeOwnerAvatarChangeListener().GetCurrentTimestamp());
                    this.SetVariableByName("StarBonusCooldown", 0);
                }
            }
        }

        public void ResetLootLimitWinCount()
        {
            this.SetVariableByName("LootLimitWinCount", 0);
        }

        public int GetStarBonusCounter()
        {
            return this.GetVariableByName("StarBonusCounter");
        }

        public int GetLootLimitWinCount()
        {
            return this.GetVariableByName("LootLimitWinCount");
        }

        public void SetStarBonusCounter(int count)
        {
            this.SetVariableByName("StarBonusCounter", count);
        }

        public bool GetStarBonusCooldown()
        {
            return this.GetVariableByName("StarBonusCooldown") == 1;
        }

        public bool GetLootLimitCooldown()
        {
            return this.GetVariableByName("LootLimitCooldown") == 1;
        }

        public int GetTroopRequestCooldown()
        {
            if (this.IsInAlliance() && this.m_allianceExpLevel > 0)
            {
                LogicAllianceLevelData allianceLevelData = LogicDataTables.GetAllianceLevel(this.m_allianceExpLevel);

                if (allianceLevelData != null)
                {
                    return allianceLevelData.GetTroopRequestCooldown() * 60;
                }
            }

            return LogicDataTables.GetGlobals().GetAllianceTroopRequestCooldown();
        }

        public int GetTroopDonationRefund()
        {
            if (this.IsInAlliance() && this.m_allianceExpLevel > 0)
            {
                LogicAllianceLevelData allianceLevelData = LogicDataTables.GetAllianceLevel(this.m_allianceExpLevel);

                if (allianceLevelData != null)
                {
                    return allianceLevelData.GetTroopDonationRefund();
                }
            }

            return 0;
        }

        public void SetLastUsedArmy(LogicArrayList<LogicDataSlot> unitCount, LogicArrayList<LogicDataSlot> spellCount)
        {
            for (int i = 0; i < unitCount.Size(); i++)
            {
                LogicDataSlot slot = unitCount[i];
                LogicCombatItemData data = (LogicCombatItemData) slot.GetData();

                int count = slot.GetCount();

                if (this.GetUnitPresetCount(data, 0) != count)
                {
                    this.SetCommodityCount(2, data, count);
                    this.GetChangeListener().CommodityCountChanged(2, data, count);
                }
            }

            for (int i = 0; i < spellCount.Size(); i++)
            {
                LogicDataSlot slot = spellCount[i];
                LogicCombatItemData data = (LogicCombatItemData) slot.GetData();

                int count = slot.GetCount();

                if (this.GetUnitPresetCount(data, 0) != count)
                {
                    this.SetCommodityCount(2, data, count);
                    this.GetChangeListener().CommodityCountChanged(2, data, count);
                }
            }
        }

        public void StarBonusCollected()
        {
            int starBonusTimesCollected = this.GetVariableByName("StarBonusTimesCollected");
            this.SetVariableByName("StarBonusTimesCollected", starBonusTimesCollected + 1);

            if (starBonusTimesCollected == 0)
            {
                this.RestartStartBonusLimitTimer(0, this.m_level.GetHomeOwnerAvatar().GetChangeListener().GetCurrentTimestamp());
            }

            if (this.GetRemainingStarBonusTime() <= 0)
            {
                this.RestartStartBonusLimitTimer(0, this.m_level.GetHomeOwnerAvatar().GetChangeListener().GetCurrentTimestamp());
            }
            else
            {
                this.SetVariableByName("StarBonusCooldown", 1);
            }
        }

        public void Decode(ByteStream stream)
        {
            this.m_id = stream.ReadLong();
            this.m_currentHomeId = stream.ReadLong();

            if (stream.ReadBoolean())
            {
                this.m_allianceId = stream.ReadLong();
                this.m_allianceName = stream.ReadString(900000);
                this.m_allianceBadgeId = stream.ReadInt();
                this.m_allianceRole = (LogicAvatarAllianceRole) stream.ReadInt();
                this.m_allianceExpLevel = stream.ReadInt();
            }

            if (stream.ReadBoolean())
            {
                this.m_leagueInstanceId = stream.ReadLong();
            }

            this.m_legendaryScore = stream.ReadInt();
            this.m_legendaryScoreVillage2 = stream.ReadInt();
            this.m_legendSeasonEntry.Decode(stream);
            this.m_legendSeasonEntryVillage2.Decode(stream);

            this.m_duelWinCount = stream.ReadInt();
            this.m_duelLoseCount = stream.ReadInt();
            this.m_duelDrawCount = stream.ReadInt();
            this.m_leagueType = stream.ReadInt();
            this.m_allianceCastleLevel = stream.ReadInt();
            this.m_allianceCastleTotalCapacity = stream.ReadInt();
            this.m_allianceCastleUsedCapacity = stream.ReadInt();
            this.m_allianceCastleTotalSpellCapacity = stream.ReadInt();
            this.m_allianceCastleUsedSpellCapacity = stream.ReadInt();

            this.m_townHallLevel = stream.ReadInt();
            this.m_townHallLevelVillage2 = stream.ReadInt();

            this.m_name = stream.ReadString(900000);
            this.m_facebookId = stream.ReadString(900000);

            this.m_expLevel = stream.ReadInt();
            this.m_expPoints = stream.ReadInt();
            this.m_diamonds = stream.ReadInt();
            this.m_freeDiamonds = stream.ReadInt();
            this.m_attackRating = stream.ReadInt();
            this.m_attackKFactor = stream.ReadInt();
            this.m_score = stream.ReadInt();
            this.m_duelScore = stream.ReadInt();
            this.m_attackWinCount = stream.ReadInt();
            this.m_attackLoseCount = stream.ReadInt();
            this.m_defenseWinCount = stream.ReadInt();
            this.m_defenseLoseCount = stream.ReadInt();
            this.m_treasuryGoldCount = stream.ReadInt();
            this.m_treasuryElixirCount = stream.ReadInt();
            this.m_treasuryDarkElixirCount = stream.ReadInt();

            stream.ReadInt();

            if (stream.ReadBoolean())
            {
                stream.ReadLong();
            }

            this.m_nameSetByUser = stream.ReadBoolean();
            this.m_allianceChatFilter = stream.ReadBoolean();
            this.m_nameChangeState = stream.ReadInt();
            this.m_cumulativePurchasedDiamonds = stream.ReadInt();
            this.m_redPackageState = stream.ReadInt();
            this.m_warPreference = stream.ReadInt();
            this.m_attackShieldReduceCounter = stream.ReadInt();
            stream.ReadInt();

            if (stream.ReadBoolean())
            {
                this.m_challengeState = stream.ReadInt();
                this.m_challengeId = stream.ReadLong();
            }

            this.ClearDataSlotArray(this.m_resourceCap);
            this.ClearDataSlotArray(this.m_resourceCount);
            this.ClearDataSlotArray(this.m_unitCount);
            this.ClearDataSlotArray(this.m_spellCount);
            this.ClearDataSlotArray(this.m_unitUpgrade);
            this.ClearDataSlotArray(this.m_spellUpgrade);
            this.ClearDataSlotArray(this.m_heroUpgrade);
            this.ClearDataSlotArray(this.m_heroHealth);
            this.ClearDataSlotArray(this.m_heroState);
            this.ClearUnitSlotArray(this.m_allianceUnitCount);
            this.ClearDataSlotArray(this.m_achievementProgress);
            this.ClearDataSlotArray(this.m_npcStars);
            this.ClearDataSlotArray(this.m_lootedNpcGold);
            this.ClearDataSlotArray(this.m_lootedNpcElixir);
            this.ClearDataSlotArray(this.m_heroMode);
            this.ClearDataSlotArray(this.m_variables);
            this.ClearDataSlotArray(this.m_unitPreset1);
            this.ClearDataSlotArray(this.m_unitPreset2);
            this.ClearDataSlotArray(this.m_unitPreset3);
            this.ClearDataSlotArray(this.m_previousArmy);
            this.ClearDataSlotArray(this.m_eventUnitCounter);
            this.ClearDataSlotArray(this.m_unitCountVillage2);
            this.ClearDataSlotArray(this.m_unitCountNewVillage2);
            this.ClearDataSlotArray(this.m_freeActionCount);

            this.m_missionCompleted.Clear();
            this.m_achievementRewardClaimed.Clear();

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);
                slot.Decode(stream);
                this.m_resourceCap.Add(slot);
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_resourceCount.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - resource slot data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_unitCount.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - unit slot data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_spellCount.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - spell slot data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_unitUpgrade.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - unit upgrade slot data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_spellUpgrade.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - spell upgrade slot data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_heroUpgrade.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - hero upgrade slot data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_heroHealth.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - hero health slot data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_heroState.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - hero state slot data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicUnitSlot slot = new LogicUnitSlot(null, 0, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_allianceUnitCount.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - alliance unit data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicMissionData data = (LogicMissionData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.MISSION);

                if (data != null)
                {
                    this.m_missionCompleted.Add(data);
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicAchievementData data = (LogicAchievementData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.ACHIEVEMENT);

                if (data != null)
                {
                    this.m_achievementRewardClaimed.Add(data);
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_achievementProgress.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - achievement progress data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_npcStars.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - npc map progress data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_lootedNpcGold.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - npc looted gold data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_lootedNpcElixir.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - npc looted elixir data is NULL");
                }
            }

            this.m_allianceUnitVisitCapacity = stream.ReadInt();
            this.m_allianceUnitSpellVisitCapacity = stream.ReadInt();

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_heroMode.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - hero mode slot data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_variables.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - variables data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_unitPreset1.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - unitPreset1 data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_unitPreset2.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - unitPreset2 data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_unitPreset3.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - unitPreset3 data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_previousArmy.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - previousArmySize data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_eventUnitCounter.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - unitCounterForEvent data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_unitCountVillage2.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - unit village2 slot data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_unitCountNewVillage2.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - unit village2 new slot data is NULL");
                }
            }

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);

                slot.Decode(stream);

                if (slot.GetData() != null)
                {
                    this.m_freeActionCount.Add(slot);
                }
                else
                {
                    slot.Destruct();
                    slot = null;

                    Debugger.Error("LogicClientAvatar::decode - slot data is NULL");
                }
            }
        }

        public void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteLong(this.m_id);
            encoder.WriteLong(this.m_currentHomeId);

            if (this.m_allianceId != null)
            {
                encoder.WriteBoolean(true);
                encoder.WriteLong(this.m_allianceId);
                encoder.WriteString(this.m_allianceName);
                encoder.WriteInt(this.m_allianceBadgeId);
                encoder.WriteInt((int) this.m_allianceRole);
                encoder.WriteInt(this.m_allianceExpLevel);
            }
            else
            {
                encoder.WriteBoolean(false);
            }

            if (this.m_leagueInstanceId != null)
            {
                encoder.WriteBoolean(true);
                encoder.WriteLong(this.m_leagueInstanceId);
            }
            else
            {
                encoder.WriteBoolean(false);
            }

            encoder.WriteInt(this.m_legendaryScore);
            encoder.WriteInt(this.m_legendaryScoreVillage2);

            this.m_legendSeasonEntry.Encode(encoder);
            this.m_legendSeasonEntryVillage2.Encode(encoder);

            encoder.WriteInt(this.m_duelWinCount);
            encoder.WriteInt(this.m_duelLoseCount);
            encoder.WriteInt(this.m_duelDrawCount);

            encoder.WriteInt(this.m_leagueType);
            encoder.WriteInt(this.m_allianceCastleLevel);
            encoder.WriteInt(this.m_allianceCastleTotalCapacity);
            encoder.WriteInt(this.m_allianceCastleUsedCapacity);
            encoder.WriteInt(this.m_allianceCastleTotalSpellCapacity);
            encoder.WriteInt(this.m_allianceCastleUsedSpellCapacity);

            encoder.WriteInt(this.m_townHallLevel);
            encoder.WriteInt(this.m_townHallLevelVillage2);

            encoder.WriteString(this.m_name);
            encoder.WriteString(this.m_facebookId);

            encoder.WriteInt(this.m_expLevel);
            encoder.WriteInt(this.m_expPoints);
            encoder.WriteInt(this.m_diamonds);
            encoder.WriteInt(this.m_freeDiamonds);
            encoder.WriteInt(this.m_attackRating);
            encoder.WriteInt(this.m_attackKFactor);
            encoder.WriteInt(this.m_score);
            encoder.WriteInt(this.m_duelScore);
            encoder.WriteInt(this.m_attackWinCount);
            encoder.WriteInt(this.m_attackLoseCount);
            encoder.WriteInt(this.m_defenseWinCount);
            encoder.WriteInt(this.m_defenseLoseCount);
            encoder.WriteInt(this.m_treasuryGoldCount);
            encoder.WriteInt(this.m_treasuryElixirCount);
            encoder.WriteInt(this.m_treasuryDarkElixirCount);
            encoder.WriteInt(0);

            if (this.m_warInstanceId != null)
            {
                encoder.WriteBoolean(true);
                encoder.WriteLong(this.m_warInstanceId);
            }
            else
            {
                encoder.WriteBoolean(false);
            }

            encoder.WriteBoolean(this.m_nameSetByUser);
            encoder.WriteBoolean(this.m_allianceChatFilter);
            encoder.WriteInt(this.m_nameChangeState);
            encoder.WriteInt(this.m_cumulativePurchasedDiamonds);
            encoder.WriteInt(this.m_redPackageState);
            encoder.WriteInt(this.m_warPreference);
            encoder.WriteInt(this.m_attackShieldReduceCounter);
            encoder.WriteInt(0);

            if (this.m_challengeId != null)
            {
                encoder.WriteBoolean(true);

                encoder.WriteInt(this.m_challengeState);
                encoder.WriteLong(this.m_challengeId);
            }
            else
            {
                encoder.WriteBoolean(false);
            }

            encoder.WriteInt(this.m_resourceCap.Size());

            for (int i = 0; i < this.m_resourceCap.Size(); i++)
            {
                this.m_resourceCap[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_resourceCount.Size());

            for (int i = 0; i < this.m_resourceCount.Size(); i++)
            {
                this.m_resourceCount[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_unitCount.Size());

            for (int i = 0; i < this.m_unitCount.Size(); i++)
            {
                this.m_unitCount[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_spellCount.Size());

            for (int i = 0; i < this.m_spellCount.Size(); i++)
            {
                this.m_spellCount[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_unitUpgrade.Size());

            for (int i = 0; i < this.m_unitUpgrade.Size(); i++)
            {
                this.m_unitUpgrade[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_spellUpgrade.Size());

            for (int i = 0; i < this.m_spellUpgrade.Size(); i++)
            {
                this.m_spellUpgrade[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_heroUpgrade.Size());

            for (int i = 0; i < this.m_heroUpgrade.Size(); i++)
            {
                this.m_heroUpgrade[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_heroHealth.Size());

            for (int i = 0; i < this.m_heroHealth.Size(); i++)
            {
                this.m_heroHealth[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_heroState.Size());

            for (int i = 0; i < this.m_heroState.Size(); i++)
            {
                this.m_heroState[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_allianceUnitCount.Size());

            for (int i = 0; i < this.m_allianceUnitCount.Size(); i++)
            {
                this.m_allianceUnitCount[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_missionCompleted.Size());

            for (int i = 0; i < this.m_missionCompleted.Size(); i++)
            {
                ByteStreamHelper.WriteDataReference(encoder, this.m_missionCompleted[i]);
            }

            encoder.WriteInt(this.m_achievementRewardClaimed.Size());

            for (int i = 0; i < this.m_achievementRewardClaimed.Size(); i++)
            {
                ByteStreamHelper.WriteDataReference(encoder, this.m_achievementRewardClaimed[i]);
            }

            encoder.WriteInt(this.m_achievementProgress.Size());

            for (int i = 0; i < this.m_achievementProgress.Size(); i++)
            {
                this.m_achievementProgress[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_npcStars.Size());

            for (int i = 0; i < this.m_npcStars.Size(); i++)
            {
                this.m_npcStars[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_lootedNpcGold.Size());

            for (int i = 0; i < this.m_lootedNpcGold.Size(); i++)
            {
                this.m_lootedNpcGold[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_lootedNpcElixir.Size());

            for (int i = 0; i < this.m_lootedNpcElixir.Size(); i++)
            {
                this.m_lootedNpcElixir[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_allianceUnitVisitCapacity);
            encoder.WriteInt(this.m_allianceUnitSpellVisitCapacity);

            encoder.WriteInt(this.m_heroMode.Size());

            for (int i = 0; i < this.m_heroMode.Size(); i++)
            {
                this.m_heroMode[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_variables.Size());

            for (int i = 0; i < this.m_variables.Size(); i++)
            {
                this.m_variables[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_unitPreset1.Size());

            for (int i = 0; i < this.m_unitPreset1.Size(); i++)
            {
                this.m_unitPreset1[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_unitPreset2.Size());

            for (int i = 0; i < this.m_unitPreset2.Size(); i++)
            {
                this.m_unitPreset2[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_unitPreset3.Size());

            for (int i = 0; i < this.m_unitPreset3.Size(); i++)
            {
                this.m_unitPreset3[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_previousArmy.Size());

            for (int i = 0; i < this.m_previousArmy.Size(); i++)
            {
                this.m_previousArmy[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_eventUnitCounter.Size());

            for (int i = 0; i < this.m_eventUnitCounter.Size(); i++)
            {
                this.m_eventUnitCounter[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_unitCountVillage2.Size());

            for (int i = 0; i < this.m_unitCountVillage2.Size(); i++)
            {
                this.m_unitCountVillage2[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_unitCountNewVillage2.Size());

            for (int i = 0; i < this.m_unitCountNewVillage2.Size(); i++)
            {
                this.m_unitCountNewVillage2[i].Encode(encoder);
            }

            encoder.WriteInt(this.m_freeActionCount.Size());

            for (int i = 0; i < this.m_freeActionCount.Size(); i++)
            {
                this.m_freeActionCount[i].Encode(encoder);
            }
        }

        public void Load(LogicJSONObject jsonObject)
        {
            LogicJSONString nameObject = jsonObject.GetJSONString("name");

            if (nameObject != null)
            {
                this.m_name = nameObject.GetStringValue();
            }

            LogicJSONBoolean nameSetObject = jsonObject.GetJSONBoolean("name_set");

            if (nameSetObject != null)
            {
                this.m_nameSetByUser = nameSetObject.IsTrue();
            }

            LogicJSONNumber nameChangeStateObject = jsonObject.GetJSONNumber("name_change_state");

            if (nameChangeStateObject != null)
            {
                this.m_nameChangeState = nameChangeStateObject.GetIntValue();
            }

            LogicJSONNumber badgeIdObject = jsonObject.GetJSONNumber("badge_id");

            if (badgeIdObject != null)
            {
                this.m_allianceBadgeId = badgeIdObject.GetIntValue();
            }

            LogicJSONNumber allianceExpLevelObject = jsonObject.GetJSONNumber("alliance_exp_level");

            if (allianceExpLevelObject != null)
            {
                this.m_allianceExpLevel = allianceExpLevelObject.GetIntValue();
            }

            if (this.m_allianceBadgeId == -1)
            {
                this.m_allianceId = null;
            }
            else
            {
                LogicJSONNumber allianceIdLowObject = jsonObject.GetJSONNumber("alliance_id_low");
                LogicJSONNumber allianceIdHighObject = jsonObject.GetJSONNumber("alliance_id_high");

                int allIdHigh = -1;
                int allIdLow = -1;

                if (allianceIdHighObject != null && allianceIdLowObject != null)
                {
                    allIdHigh = allianceIdHighObject.GetIntValue();
                    allIdLow = allianceIdLowObject.GetIntValue();
                }

                this.m_allianceId = new LogicLong(allIdHigh, allIdLow);
                this.m_allianceName = LogicJSONHelper.GetString(jsonObject, "alliance_name");
                this.m_allianceRole = (LogicAvatarAllianceRole) LogicJSONHelper.GetInt(jsonObject, "alliance_role");
            }

            LogicJSONNumber leagueIdLowObject = jsonObject.GetJSONNumber("league_id_low");
            LogicJSONNumber leagueIdHighObject = jsonObject.GetJSONNumber("league_id_high");

            if (leagueIdHighObject != null && leagueIdLowObject != null)
            {
                this.m_leagueInstanceId = new LogicLong(leagueIdHighObject.GetIntValue(), leagueIdLowObject.GetIntValue());
            }

            this.m_allianceUnitVisitCapacity = LogicJSONHelper.GetInt(jsonObject, "alliance_unit_visit_capacity", 0);
            this.m_allianceUnitSpellVisitCapacity = LogicJSONHelper.GetInt(jsonObject, "alliance_unit_spell_visit_capacity", 0);
            this.m_expLevel = LogicJSONHelper.GetInt(jsonObject, "xp_level", 0);
            this.m_expPoints = LogicJSONHelper.GetInt(jsonObject, "xp_points", 0);
            this.m_diamonds = LogicJSONHelper.GetInt(jsonObject, "diamonds", 0);
            this.m_freeDiamonds = LogicJSONHelper.GetInt(jsonObject, "free_diamonds", 0);

            this.m_leagueType = LogicJSONHelper.GetInt(jsonObject, "league_type", 0);
            this.m_legendaryScore = LogicJSONHelper.GetInt(jsonObject, "legendary_score", 0);
            this.m_legendaryScoreVillage2 = LogicJSONHelper.GetInt(jsonObject, "legendary_score_v2", 0);

            LogicJSONObject legendLeagueEntry = jsonObject.GetJSONObject("legend_league_entry");

            if (legendLeagueEntry != null)
            {
                this.m_legendSeasonEntry.ReadFromJSON(legendLeagueEntry);
            }

            LogicJSONObject legendLeagueEntryV2 = jsonObject.GetJSONObject("legend_league_entry_v2");

            if (legendLeagueEntryV2 != null)
            {
                this.m_legendSeasonEntryVillage2.ReadFromJSON(legendLeagueEntryV2);
            }

            this.LoadDataSlotArray(jsonObject, "units", this.m_unitCount);
            this.LoadDataSlotArray(jsonObject, "spells", this.m_spellCount);
            this.LoadDataSlotArray(jsonObject, "unit_upgrades", this.m_unitUpgrade);
            this.LoadDataSlotArray(jsonObject, "spell_upgrades", this.m_spellUpgrade);
            this.LoadDataSlotArray(jsonObject, "resources", this.m_resourceCount);
            this.LoadDataSlotArray(jsonObject, "resource_caps", this.m_resourceCap);
            this.LoadUnitSlotArray(jsonObject, "alliance_units", this.m_allianceUnitCount);
            this.LoadDataSlotArray(jsonObject, "hero_states", this.m_heroState);
            this.LoadDataSlotArray(jsonObject, "hero_health", this.m_heroHealth);
            this.LoadDataSlotArray(jsonObject, "hero_upgrade", this.m_heroUpgrade);
            this.LoadDataSlotArray(jsonObject, "hero_modes", this.m_heroMode);
            this.LoadDataSlotArray(jsonObject, "variables", this.m_variables);
            this.LoadDataSlotArray(jsonObject, "units2", this.m_unitCountVillage2);
            this.LoadDataSlotArray(jsonObject, "units_new2", this.m_unitCountNewVillage2);
            this.LoadDataSlotArray(jsonObject, "unit_preset1", this.m_unitPreset1);
            this.LoadDataSlotArray(jsonObject, "unit_preset2", this.m_unitPreset2);
            this.LoadDataSlotArray(jsonObject, "unit_preset3", this.m_unitPreset3);
            this.LoadDataSlotArray(jsonObject, "previous_army", this.m_previousArmy);
            this.LoadDataSlotArray(jsonObject, "event_unit_counter", this.m_eventUnitCounter);
            this.LoadDataSlotArray(jsonObject, "looted_npc_gold", this.m_lootedNpcGold);
            this.LoadDataSlotArray(jsonObject, "looted_npc_elixir", this.m_lootedNpcElixir);
            this.LoadDataSlotArray(jsonObject, "npc_stars", this.m_npcStars);
            this.LoadDataSlotArray(jsonObject, "achievement_progress", this.m_achievementProgress);

            LogicJSONArray achievementRewardClaimedArray = jsonObject.GetJSONArray("achievement_rewards");

            if (achievementRewardClaimedArray != null)
            {
                if (achievementRewardClaimedArray.Size() != 0)
                {
                    this.m_achievementRewardClaimed.Clear();
                    this.m_achievementRewardClaimed.EnsureCapacity(achievementRewardClaimedArray.Size());

                    for (int i = 0; i < achievementRewardClaimedArray.Size(); i++)
                    {
                        LogicJSONNumber id = achievementRewardClaimedArray.GetJSONNumber(i);

                        if (id != null)
                        {
                            LogicData data = LogicDataTables.GetDataById(id.GetIntValue());

                            if (data != null)
                            {
                                this.m_achievementRewardClaimed.Add(data);
                            }
                        }
                    }
                }
            }

            LogicJSONArray missionCompletedArray = jsonObject.GetJSONArray("missions");

            if (missionCompletedArray != null)
            {
                if (missionCompletedArray.Size() != 0)
                {
                    this.m_missionCompleted.Clear();
                    this.m_missionCompleted.EnsureCapacity(missionCompletedArray.Size());

                    for (int i = 0; i < missionCompletedArray.Size(); i++)
                    {
                        LogicJSONNumber id = missionCompletedArray.GetJSONNumber(i);

                        if (id != null)
                        {
                            LogicData data = LogicDataTables.GetDataById(id.GetIntValue());

                            if (data != null)
                            {
                                this.m_missionCompleted.Add(data);
                            }
                        }
                    }
                }
            }

            this.m_allianceCastleLevel = LogicJSONHelper.GetInt(jsonObject, "castle_lvl", -1);
            this.m_allianceCastleTotalCapacity = LogicJSONHelper.GetInt(jsonObject, "castle_total", 0);
            this.m_allianceCastleUsedCapacity = LogicJSONHelper.GetInt(jsonObject, "castle_used", 0);
            this.m_allianceCastleTotalSpellCapacity = LogicJSONHelper.GetInt(jsonObject, "castle_total_sp", 0);
            this.m_allianceCastleUsedSpellCapacity = LogicJSONHelper.GetInt(jsonObject, "castle_used_sp", 0);
            this.m_townHallLevel = LogicJSONHelper.GetInt(jsonObject, "town_hall_lvl", 0);
            this.m_townHallLevelVillage2 = LogicJSONHelper.GetInt(jsonObject, "th_v2_lvl", 0);
            this.m_score = LogicJSONHelper.GetInt(jsonObject, "score", 0);
            this.m_duelScore = LogicJSONHelper.GetInt(jsonObject, "duel_score", 0);
            this.m_warPreference = LogicJSONHelper.GetInt(jsonObject, "war_preference", 0);
            this.m_attackRating = LogicJSONHelper.GetInt(jsonObject, "attack_rating", 0);
            this.m_attackKFactor = LogicJSONHelper.GetInt(jsonObject, "atack_kfactor", 0);
            this.m_attackWinCount = LogicJSONHelper.GetInt(jsonObject, "attack_win_cnt", 0);
            this.m_attackLoseCount = LogicJSONHelper.GetInt(jsonObject, "attack_lose_cnt", 0);
            this.m_defenseWinCount = LogicJSONHelper.GetInt(jsonObject, "defense_win_cnt", 0);
            this.m_defenseLoseCount = LogicJSONHelper.GetInt(jsonObject, "defense_lose_cnt", 0);
            this.m_treasuryGoldCount = LogicJSONHelper.GetInt(jsonObject, "treasury_gold_cnt", 0);
            this.m_treasuryElixirCount = LogicJSONHelper.GetInt(jsonObject, "treasury_elixir_cnt", 0);
            this.m_treasuryDarkElixirCount = LogicJSONHelper.GetInt(jsonObject, "treasury_dark_elixir_cnt", 0);
            this.m_redPackageState = LogicJSONHelper.GetInt(jsonObject, "red_package_state", 0);
        }

        public override void LoadForReplay(LogicJSONObject jsonObject, bool direct)
        {
            LogicJSONNumber avatarIdLowObject = jsonObject.GetJSONNumber("avatar_id_low");
            LogicJSONNumber avatarIdHighObject = jsonObject.GetJSONNumber("avatar_id_high");

            if (avatarIdHighObject != null)
            {
                if (avatarIdLowObject != null)
                {
                    this.m_id = new LogicLong(avatarIdHighObject.GetIntValue(), avatarIdLowObject.GetIntValue());
                }
            }

            LogicJSONString nameObject = jsonObject.GetJSONString("name");

            if (nameObject != null)
            {
                this.m_name = nameObject.GetStringValue();
            }

            LogicJSONNumber badgeIdObject = jsonObject.GetJSONNumber("badge_id");

            if (badgeIdObject != null)
            {
                this.m_allianceBadgeId = badgeIdObject.GetIntValue();
            }

            LogicJSONNumber allianceExpLevelObject = jsonObject.GetJSONNumber("alliance_exp_level");

            if (allianceExpLevelObject != null)
            {
                this.m_allianceExpLevel = allianceExpLevelObject.GetIntValue();
            }

            if (this.m_allianceBadgeId == -1)
            {
                this.m_allianceId = null;
            }
            else
            {
                LogicJSONNumber allianceIdLowObject = jsonObject.GetJSONNumber("alliance_id_low");
                LogicJSONNumber allianceIdHighObject = jsonObject.GetJSONNumber("alliance_id_high");

                int allIdHigh = -1;
                int allIdLow = -1;

                if (allianceIdHighObject != null)
                {
                    if (allianceIdLowObject != null)
                    {
                        allIdHigh = allianceIdHighObject.GetIntValue();
                        allIdLow = allianceIdLowObject.GetIntValue();
                    }
                }

                this.m_allianceId = new LogicLong(allIdHigh, allIdLow);
                this.m_allianceName = LogicJSONHelper.GetString(jsonObject, "alliance_name");
            }

            this.m_allianceUnitVisitCapacity = LogicJSONHelper.GetInt(jsonObject, "alliance_unit_visit_capacity", 0);
            this.m_allianceUnitSpellVisitCapacity = LogicJSONHelper.GetInt(jsonObject, "alliance_unit_spell_visit_capacity", 0);
            this.m_leagueType = LogicJSONHelper.GetInt(jsonObject, "league_type", 0);
            this.m_expLevel = LogicJSONHelper.GetInt(jsonObject, "xp_level", 1);

            if (!direct)
            {
                this.LoadDataSlotArray(jsonObject, "units", this.m_unitCount);
                this.LoadDataSlotArray(jsonObject, "spells", this.m_spellCount);
                this.LoadDataSlotArray(jsonObject, "unit_upgrades", this.m_unitUpgrade);
                this.LoadDataSlotArray(jsonObject, "spell_upgrades", this.m_spellUpgrade);
            }

            this.LoadDataSlotArray(jsonObject, "resources", this.m_resourceCount);
            this.LoadUnitSlotArray(jsonObject, "alliance_units", this.m_allianceUnitCount);
            this.LoadDataSlotArray(jsonObject, "hero_states", this.m_heroState);
            this.LoadDataSlotArray(jsonObject, "hero_health", this.m_heroHealth);
            this.LoadDataSlotArray(jsonObject, "hero_upgrade", this.m_heroUpgrade);
            this.LoadDataSlotArray(jsonObject, "hero_modes", this.m_heroMode);
            this.LoadDataSlotArray(jsonObject, "variables", this.m_variables);

            if (!direct)
            {
                this.LoadDataSlotArray(jsonObject, "units2", this.m_unitCountVillage2);
            }

            this.m_allianceCastleLevel = LogicJSONHelper.GetInt(jsonObject, "castle_lvl", -1);
            this.m_allianceCastleTotalCapacity = LogicJSONHelper.GetInt(jsonObject, "castle_total", 0);
            this.m_allianceCastleUsedCapacity = LogicJSONHelper.GetInt(jsonObject, "castle_used", 0);
            this.m_allianceCastleTotalSpellCapacity = LogicJSONHelper.GetInt(jsonObject, "castle_total_sp", 0);
            this.m_allianceCastleUsedSpellCapacity = LogicJSONHelper.GetInt(jsonObject, "castle_used_sp", 0);
            this.m_townHallLevel = LogicJSONHelper.GetInt(jsonObject, "town_hall_lvl", -1);
            this.m_townHallLevelVillage2 = LogicJSONHelper.GetInt(jsonObject, "th_v2_lvl", -1);
            this.m_score = LogicJSONHelper.GetInt(jsonObject, "score", 0);
            this.m_duelScore = LogicJSONHelper.GetInt(jsonObject, "duel_score", 0);
            this.m_redPackageState = LogicJSONHelper.GetInt(jsonObject, "red_package_state", 0);
        }

        private void LoadDataSlotArray(LogicJSONObject jsonObject, string key, LogicArrayList<LogicDataSlot> dataSlotArray)
        {
            this.ClearDataSlotArray(dataSlotArray);

            LogicJSONArray jsonArray = jsonObject.GetJSONArray(key);

            if (jsonArray != null)
            {
                int arraySize = jsonArray.Size();

                if (arraySize != 0)
                {
                    dataSlotArray.EnsureCapacity(arraySize);

                    for (int i = 0; i < arraySize; i++)
                    {
                        LogicJSONObject obj = jsonArray.GetJSONObject(i);

                        if (obj != null)
                        {
                            LogicDataSlot slot = new LogicDataSlot(null, 0);

                            slot.ReadFromJSON(obj);

                            if (slot.GetData() != null)
                            {
                                dataSlotArray.Add(slot);
                            }
                        }
                    }
                }
            }
        }

        private void LoadUnitSlotArray(LogicJSONObject jsonObject, string key, LogicArrayList<LogicUnitSlot> unitSlotArray)
        {
            this.ClearUnitSlotArray(unitSlotArray);

            LogicJSONArray jsonArray = jsonObject.GetJSONArray(key);

            if (jsonArray != null)
            {
                int arraySize = jsonArray.Size();

                if (arraySize != 0)
                {
                    unitSlotArray.EnsureCapacity(arraySize);

                    for (int i = 0; i < arraySize; i++)
                    {
                        LogicJSONObject obj = jsonArray.GetJSONObject(i);

                        if (obj != null)
                        {
                            LogicUnitSlot slot = new LogicUnitSlot(null, 0, 0);

                            slot.ReadFromJSON(obj);

                            if (slot.GetData() != null)
                            {
                                unitSlotArray.Add(slot);
                            }
                        }
                    }
                }
            }
        }

        public void Save(LogicJSONObject jsonObject)
        {
            jsonObject.Put("name", new LogicJSONString(this.m_name));
            jsonObject.Put("name_set", new LogicJSONBoolean(this.m_nameSetByUser));
            jsonObject.Put("name_change_state", new LogicJSONNumber(this.m_nameChangeState));
            jsonObject.Put("alliance_name", new LogicJSONString(this.m_allianceName ?? string.Empty));
            jsonObject.Put("xp_level", new LogicJSONNumber(this.m_expLevel));
            jsonObject.Put("xp_points", new LogicJSONNumber(this.m_expPoints));
            jsonObject.Put("diamonds", new LogicJSONNumber(this.m_diamonds));
            jsonObject.Put("free_diamonds", new LogicJSONNumber(this.m_freeDiamonds));

            if (this.m_allianceId != null)
            {
                jsonObject.Put("alliance_id_high", new LogicJSONNumber(this.m_allianceId.GetHigherInt()));
                jsonObject.Put("alliance_id_low", new LogicJSONNumber(this.m_allianceId.GetLowerInt()));
                jsonObject.Put("badge_id", new LogicJSONNumber(this.m_allianceBadgeId));
                jsonObject.Put("alliance_role", new LogicJSONNumber((int) this.m_allianceRole));
                jsonObject.Put("alliance_exp_level", new LogicJSONNumber(this.m_allianceExpLevel));
                jsonObject.Put("alliance_unit_visit_capacity", new LogicJSONNumber(this.m_allianceUnitVisitCapacity));
                jsonObject.Put("alliance_unit_spell_visit_capacity", new LogicJSONNumber(this.m_allianceUnitSpellVisitCapacity));
            }

            if (this.m_leagueInstanceId != null)
            {
                jsonObject.Put("league_id_high", new LogicJSONNumber(this.m_leagueInstanceId.GetHigherInt()));
                jsonObject.Put("league_id_low", new LogicJSONNumber(this.m_leagueInstanceId.GetLowerInt()));
            }

            jsonObject.Put("league_type", new LogicJSONNumber(this.m_leagueType));
            jsonObject.Put("legendary_score", new LogicJSONNumber(this.m_legendaryScore));
            jsonObject.Put("legendary_score_v2", new LogicJSONNumber(this.m_legendaryScoreVillage2));

            LogicJSONObject legendLeagueTournamentEntryObject = new LogicJSONObject();
            this.m_legendSeasonEntry.WriteToJSON(legendLeagueTournamentEntryObject);
            jsonObject.Put("legend_league_entry", legendLeagueTournamentEntryObject);

            LogicJSONObject legendLeagueTournamentEntryVillage2Object = new LogicJSONObject();
            this.m_legendSeasonEntryVillage2.WriteToJSON(legendLeagueTournamentEntryVillage2Object);
            jsonObject.Put("legend_league_entry_v2", legendLeagueTournamentEntryVillage2Object);

            this.SaveDataSlotArray(jsonObject, "units", this.m_unitCount);
            this.SaveDataSlotArray(jsonObject, "spells", this.m_spellCount);
            this.SaveDataSlotArray(jsonObject, "unit_upgrades", this.m_unitUpgrade);
            this.SaveDataSlotArray(jsonObject, "spell_upgrades", this.m_spellUpgrade);
            this.SaveDataSlotArray(jsonObject, "resources", this.m_resourceCount);
            this.SaveDataSlotArray(jsonObject, "resource_caps", this.m_resourceCap);
            this.SaveUnitSlotArray(jsonObject, "alliance_units", this.m_allianceUnitCount);
            this.SaveDataSlotArray(jsonObject, "hero_states", this.m_heroState);
            this.SaveDataSlotArray(jsonObject, "hero_health", this.m_heroHealth);
            this.SaveDataSlotArray(jsonObject, "hero_upgrade", this.m_heroUpgrade);
            this.SaveDataSlotArray(jsonObject, "hero_modes", this.m_heroMode);
            this.SaveDataSlotArray(jsonObject, "variables", this.m_variables);
            this.SaveDataSlotArray(jsonObject, "units2", this.m_unitCountVillage2);
            this.SaveDataSlotArray(jsonObject, "units_new2", this.m_unitCountNewVillage2);
            this.SaveDataSlotArray(jsonObject, "unit_preset1", this.m_unitPreset1);
            this.SaveDataSlotArray(jsonObject, "unit_preset2", this.m_unitPreset2);
            this.SaveDataSlotArray(jsonObject, "unit_preset3", this.m_unitPreset3);
            this.SaveDataSlotArray(jsonObject, "previous_army", this.m_previousArmy);
            this.SaveDataSlotArray(jsonObject, "event_unit_counter", this.m_eventUnitCounter);
            this.SaveDataSlotArray(jsonObject, "looted_npc_gold", this.m_lootedNpcGold);
            this.SaveDataSlotArray(jsonObject, "looted_npc_elixir", this.m_lootedNpcElixir);
            this.SaveDataSlotArray(jsonObject, "npc_stars", this.m_npcStars);
            this.SaveDataSlotArray(jsonObject, "achievement_progress", this.m_achievementProgress);

            LogicJSONArray achievementRewardClaimedArray = new LogicJSONArray();

            for (int i = 0; i < this.m_achievementRewardClaimed.Size(); i++)
            {
                achievementRewardClaimedArray.Add(new LogicJSONNumber(this.m_achievementRewardClaimed[i].GetGlobalID()));
            }

            jsonObject.Put("achievement_rewards", achievementRewardClaimedArray);

            LogicJSONArray missionCompletedArray = new LogicJSONArray();

            for (int i = 0; i < this.m_missionCompleted.Size(); i++)
            {
                missionCompletedArray.Add(new LogicJSONNumber(this.m_missionCompleted[i].GetGlobalID()));
            }

            jsonObject.Put("missions", missionCompletedArray);

            jsonObject.Put("castle_lvl", new LogicJSONNumber(this.m_allianceCastleLevel));
            jsonObject.Put("castle_total", new LogicJSONNumber(this.m_allianceCastleTotalCapacity));
            jsonObject.Put("castle_used", new LogicJSONNumber(this.m_allianceCastleUsedCapacity));
            jsonObject.Put("castle_total_sp", new LogicJSONNumber(this.m_allianceCastleTotalSpellCapacity));
            jsonObject.Put("castle_used_sp", new LogicJSONNumber(this.m_allianceCastleUsedSpellCapacity));
            jsonObject.Put("town_hall_lvl", new LogicJSONNumber(this.m_townHallLevel));
            jsonObject.Put("th_v2_lvl", new LogicJSONNumber(this.m_townHallLevelVillage2));
            jsonObject.Put("score", new LogicJSONNumber(this.m_score));
            jsonObject.Put("duel_score", new LogicJSONNumber(this.m_duelScore));
            jsonObject.Put("war_preference", new LogicJSONNumber(this.m_warPreference));
            jsonObject.Put("attack_rating", new LogicJSONNumber(this.m_attackRating));
            jsonObject.Put("atack_kfactor", new LogicJSONNumber(this.m_attackKFactor));
            jsonObject.Put("attack_win_cnt", new LogicJSONNumber(this.m_attackWinCount));
            jsonObject.Put("attack_lose_cnt", new LogicJSONNumber(this.m_attackLoseCount));
            jsonObject.Put("defense_win_cnt", new LogicJSONNumber(this.m_defenseWinCount));
            jsonObject.Put("defense_lose_cnt", new LogicJSONNumber(this.m_defenseLoseCount));
            jsonObject.Put("treasury_gold_cnt", new LogicJSONNumber(this.m_treasuryGoldCount));
            jsonObject.Put("treasury_elixir_cnt", new LogicJSONNumber(this.m_treasuryElixirCount));
            jsonObject.Put("treasury_dark_elixir_cnt", new LogicJSONNumber(this.m_treasuryDarkElixirCount));

            if (this.m_redPackageState != 0)
            {
                jsonObject.Put("red_package_state", new LogicJSONNumber(this.m_redPackageState));
            }
        }

        public override void SaveToReplay(LogicJSONObject jsonObject)
        {
            jsonObject.Put("avatar_id_high", new LogicJSONNumber(this.m_id.GetHigherInt()));
            jsonObject.Put("avatar_id_low", new LogicJSONNumber(this.m_id.GetLowerInt()));
            jsonObject.Put("name", new LogicJSONString(this.m_name));
            jsonObject.Put("alliance_name", new LogicJSONString(this.m_allianceName ?? string.Empty));
            jsonObject.Put("xp_level", new LogicJSONNumber(this.m_expLevel));

            if (this.m_allianceId != null)
            {
                jsonObject.Put("alliance_id_high", new LogicJSONNumber(this.m_allianceId.GetHigherInt()));
                jsonObject.Put("alliance_id_low", new LogicJSONNumber(this.m_allianceId.GetLowerInt()));
                jsonObject.Put("badge_id", new LogicJSONNumber(this.m_allianceBadgeId));
                jsonObject.Put("alliance_exp_level", new LogicJSONNumber(this.m_allianceExpLevel));
                jsonObject.Put("alliance_unit_visit_capacity", new LogicJSONNumber(this.m_allianceUnitVisitCapacity));
                jsonObject.Put("alliance_unit_spell_visit_capacity", new LogicJSONNumber(this.m_allianceUnitSpellVisitCapacity));
            }

            jsonObject.Put("league_type", new LogicJSONNumber(this.m_leagueType));

            this.SaveDataSlotArray(jsonObject, "units", this.m_unitCount);
            this.SaveDataSlotArray(jsonObject, "spells", this.m_spellCount);
            this.SaveDataSlotArray(jsonObject, "unit_upgrades", this.m_unitUpgrade);
            this.SaveDataSlotArray(jsonObject, "spell_upgrades", this.m_spellUpgrade);
            this.SaveDataSlotArray(jsonObject, "resources", this.m_resourceCount);
            this.SaveUnitSlotArray(jsonObject, "alliance_units", this.m_allianceUnitCount);
            this.SaveDataSlotArray(jsonObject, "hero_states", this.m_heroState);
            this.SaveDataSlotArray(jsonObject, "hero_health", this.m_heroHealth);
            this.SaveDataSlotArray(jsonObject, "hero_upgrade", this.m_heroUpgrade);
            this.SaveDataSlotArray(jsonObject, "hero_modes", this.m_heroMode);
            this.SaveDataSlotArray(jsonObject, "variables", this.m_variables);
            this.SaveDataSlotArray(jsonObject, "units2", this.m_unitCountVillage2);

            jsonObject.Put("castle_lvl", new LogicJSONNumber(this.m_allianceCastleLevel));
            jsonObject.Put("castle_total", new LogicJSONNumber(this.m_allianceCastleTotalCapacity));
            jsonObject.Put("castle_used", new LogicJSONNumber(this.m_allianceCastleUsedCapacity));
            jsonObject.Put("castle_total_sp", new LogicJSONNumber(this.m_allianceCastleTotalSpellCapacity));
            jsonObject.Put("castle_used_sp", new LogicJSONNumber(this.m_allianceCastleUsedSpellCapacity));
            jsonObject.Put("town_hall_lvl", new LogicJSONNumber(this.m_townHallLevel));
            jsonObject.Put("th_v2_lvl", new LogicJSONNumber(this.m_townHallLevelVillage2));
            jsonObject.Put("score", new LogicJSONNumber(this.m_score));
            jsonObject.Put("duel_score", new LogicJSONNumber(this.m_duelScore));

            if (this.m_redPackageState != 0)
            {
                jsonObject.Put("red_package_state", new LogicJSONNumber(this.m_redPackageState));
            }
        }

        public override void SaveToDirect(LogicJSONObject jsonObject)
        {
            jsonObject.Put("avatar_id_high", new LogicJSONNumber(this.m_id.GetHigherInt()));
            jsonObject.Put("avatar_id_low", new LogicJSONNumber(this.m_id.GetLowerInt()));
            jsonObject.Put("name", new LogicJSONString(this.m_name));
            jsonObject.Put("alliance_name", new LogicJSONString(this.m_allianceName ?? string.Empty));
            jsonObject.Put("xp_level", new LogicJSONNumber(this.m_expLevel));

            if (this.m_allianceId != null)
            {
                jsonObject.Put("alliance_id_high", new LogicJSONNumber(this.m_allianceId.GetHigherInt()));
                jsonObject.Put("alliance_id_low", new LogicJSONNumber(this.m_allianceId.GetLowerInt()));
                jsonObject.Put("badge_id", new LogicJSONNumber(this.m_allianceBadgeId));
                jsonObject.Put("alliance_exp_level", new LogicJSONNumber(this.m_allianceExpLevel));
                jsonObject.Put("alliance_unit_visit_capacity", new LogicJSONNumber(this.m_allianceUnitVisitCapacity));
                jsonObject.Put("alliance_unit_spell_visit_capacity", new LogicJSONNumber(this.m_allianceUnitSpellVisitCapacity));
            }

            jsonObject.Put("league_type", new LogicJSONNumber(this.m_leagueType));

            this.SaveDataSlotArray(jsonObject, "resources", this.m_resourceCount);
            this.SaveUnitSlotArray(jsonObject, "alliance_units", this.m_allianceUnitCount);
            this.SaveDataSlotArray(jsonObject, "hero_states", this.m_heroState);
            this.SaveDataSlotArray(jsonObject, "hero_health", this.m_heroHealth);
            this.SaveDataSlotArray(jsonObject, "hero_upgrade", this.m_heroUpgrade);
            this.SaveDataSlotArray(jsonObject, "hero_modes", this.m_heroMode);
            this.SaveDataSlotArray(jsonObject, "variables", this.m_variables);

            jsonObject.Put("castle_lvl", new LogicJSONNumber(this.m_allianceCastleLevel));
            jsonObject.Put("castle_total", new LogicJSONNumber(this.m_allianceCastleTotalCapacity));
            jsonObject.Put("castle_used", new LogicJSONNumber(this.m_allianceCastleUsedCapacity));
            jsonObject.Put("castle_total_sp", new LogicJSONNumber(this.m_allianceCastleTotalSpellCapacity));
            jsonObject.Put("castle_used_sp", new LogicJSONNumber(this.m_allianceCastleUsedSpellCapacity));
            jsonObject.Put("town_hall_lvl", new LogicJSONNumber(this.m_townHallLevel));
            jsonObject.Put("th_v2_lvl", new LogicJSONNumber(this.m_townHallLevelVillage2));
            jsonObject.Put("score", new LogicJSONNumber(this.m_score));
            jsonObject.Put("duel_score", new LogicJSONNumber(this.m_duelScore));

            if (this.m_redPackageState != 0)
            {
                jsonObject.Put("red_package_state", new LogicJSONNumber(this.m_redPackageState));
            }
        }

        private void SaveDataSlotArray(LogicJSONObject jsonObject, string key, LogicArrayList<LogicDataSlot> dataSlotArray)
        {
            LogicJSONArray jsonArray = new LogicJSONArray(dataSlotArray.Size());

            for (int i = 0; i < dataSlotArray.Size(); i++)
            {
                LogicJSONObject obj = new LogicJSONObject();
                dataSlotArray[i].WriteToJSON(obj);
                jsonArray.Add(obj);
            }

            jsonObject.Put(key, jsonArray);
        }

        private void SaveUnitSlotArray(LogicJSONObject jsonObject, string key, LogicArrayList<LogicUnitSlot> unitSlotArray)
        {
            LogicJSONArray jsonArray = new LogicJSONArray(unitSlotArray.Size());

            for (int i = 0; i < unitSlotArray.Size(); i++)
            {
                LogicJSONObject obj = new LogicJSONObject();
                unitSlotArray[i].WriteToJSON(obj);
                jsonArray.Add(obj);
            }

            jsonObject.Put(key, jsonArray);
        }
    }
}