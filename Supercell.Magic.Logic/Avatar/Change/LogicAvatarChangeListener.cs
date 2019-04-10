namespace Supercell.Magic.Logic.Avatar.Change
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Offer;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class LogicAvatarChangeListener
    {
        public virtual void Destruct()
        {
            // Destruct.
        }

        public virtual void FreeDiamondsAdded(int count, int mode)
        {
        }

        public virtual void DiamondPurchaseMade(int type, int globalId, int level, int count, int villageType)
        {
        }

        public virtual void CommodityCountChanged(int commodityType, LogicData data, int count)
        {
        }

        public virtual void WarPreferenceChanged(int preference)
        {
        }

        public virtual void RevengeUsed(LogicLong id)
        {
        }

        public virtual void ExpPointsGained(int count)
        {
        }

        public virtual void ExpLevelGained(int count)
        {
        }

        public virtual void AllianceJoined(LogicLong allianceId, string allianceName, int allianceBadgeId, int allianceExpLevel, LogicAvatarAllianceRole allianceRole)
        {
        }

        public virtual void AllianceLevelChanged(int expLevel)
        {
        }

        public virtual void AllianceLeft()
        {
        }

        public virtual void AllianceUnitDonateOk(LogicCombatItemData data, int upgLevel, LogicLong streamId, bool quickDonate)
        {
        }

        public virtual void AllianceUnitDonateFailed(LogicCombatItemData data, int upgLevel, LogicLong streamId, bool quickDonate)
        {
        }

        public virtual void WarDonateOk(LogicCombatItemData data, int upgLevel, LogicLong streamId, bool quickDonate)
        {
        }

        public virtual void WarDonateFailed(LogicCombatItemData data, int upgLevel, LogicLong streamId, bool quickDonate)
        {
        }

        public virtual void StartWar(LogicArrayList<LogicLong> excludeMembers)
        {
        }

        public virtual void CancelWar()
        {
        }

        public virtual void StartArrangedWar(LogicArrayList<LogicLong> excludeMembers, LogicLong allianceId, int unk1, int unk2, int unk3)
        {
        }

        public virtual void AllianceUnitRemoved(LogicCombatItemData data, int upgLevel)
        {
        }

        public virtual void AllianceUnitAdded(LogicCombatItemData data, int upgLevel)
        {
        }

        public virtual void AllianceUnitCountChanged(LogicCombatItemData data, int upgLevel, int count)
        {
        }

        public virtual void RequestAllianceUnits(int upgLevel, int usedCapacity, int maxCapacity, int spellUsedCapacity, int maxSpellCapacity, string message)
        {
        }

        public virtual void SetAllianceCastleLevel(int count)
        {
        }

        public virtual void KickPlayer(LogicLong playerId, string message)
        {
        }

        public virtual void SetTownHallLevel(int count)
        {
        }

        public virtual void SetVillage2TownHallLevel(int count)
        {
        }

        public virtual void DeliverableAllianceGift(LogicDeliverableGift deliverableGift)
        {
        }

        public virtual void LegendSeasonScoreChanged(int state, int score, int scoreChange, bool bestSeason, int villageType)
        {
        }

        public virtual void ScoreChanged(LogicLong allianceId, int scoreGain, int minScoreGain, bool attacker, LogicLeagueData leagueData, LogicLeagueData prevLeagueData,
                                         int destructionPercentage)
        {
        }

        public virtual void DuelScoreChanged(LogicLong allianceId, int duelScoreGain, int resultType, bool attacker)
        {
        }

        public virtual void SendClanMail(string message)
        {
        }

        public virtual void ShareReplay(LogicLong replayId, string message)
        {
        }

        public virtual void ShareDuelReplay(LogicLong replayId, string message)
        {
        }

        public virtual void AllianceChatFilterChanged(bool enabled)
        {
        }

        public virtual void SendChallengeRequest(string message, int layoutId, bool warLayout, int villageType)
        {
        }

        public virtual void SendFriendlyBattleRequest(string message, LogicLong id, int layoutId, bool warLayout, int villageType)
        {
        }

        public virtual void StarBonusAdded(int goldCount, int elixirCount, int darkElixirCount)
        {
        }

        public virtual void WarTroopRequestMessageChanged(string message)
        {
        }

        public virtual void LeagueChanged(int leagueType, LogicLong leagueInstanceId)
        {
        }

        public virtual void AllianceXpGained(int count)
        {
        }

        public virtual void AttackStarted()
        {
        }

        public virtual void AttackShieldReduceCounterChanged(int count)
        {
        }

        public virtual void DefenseVillageGuardCounterChanged(int count)
        {
        }

        public virtual void BattleFeedback(int type, int stars)
        {
        }

        public virtual void REDPackageStateChanged(int value)
        {
        }

        public virtual int GetCurrentTimestamp()
        {
            return -1;
        }
    }
}