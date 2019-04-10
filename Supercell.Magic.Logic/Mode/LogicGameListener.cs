namespace Supercell.Magic.Logic.Mode
{
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class LogicGameListener
    {
        public void Destruct()
        {
            // Destruct.
        }

        public virtual void ReplayFailed()
        {
        }

        public virtual void NotEnoughWorkers(LogicCommand command, int villageType)
        {
        }

        public virtual void NotEnoughResources(LogicResourceData data, int count, LogicCommand command, bool unk)
        {
        }

        public virtual void NotEnoughResources(LogicResourceData data1, int count1, LogicResourceData data2, int count2, LogicCommand command, bool unk)
        {
        }

        public virtual void NotEnoughDiamonds()
        {
        }

        public virtual void BuildingCapReached(LogicBuildingData data)
        {
        }

        public virtual void BuildingGearUpCapReached(LogicBuildingData data)
        {
        }

        public virtual void TrapCapReached(LogicTrapData data)
        {
        }

        public virtual void DecoCapReached(LogicDecoData data)
        {
        }

        public virtual void AllianceCreated()
        {
        }

        public virtual void AllianceJoined()
        {
        }

        public virtual void AllianceLeft()
        {
        }

        public virtual void AllianceSettingsChanged()
        {
        }

        public virtual void LevelUp(int expLevel)
        {
        }

        public virtual void ShowTroopPlacementTutorial(int data)
        {
        }

        public virtual void TownHallLevelTooLow(int lvl)
        {
        }

        public virtual void AchievementCompleted(LogicAchievementData data)
        {
        }

        public virtual void AchievementProgress(LogicAchievementData data)
        {
        }

        public virtual void DiamondsBought()
        {
        }

        public virtual void UnitUpgradeCompleted(LogicCombatItemData data, int upgLevel, bool tick)
        {
        }

        public virtual void MatchmakingCommandExecuted()
        {
        }

        public virtual void MatchmakingVillage2CommandExecuted()
        {
        }

        public virtual void AttackerPlaced(LogicData data)
        {
        }

        public virtual void BattleEndedByPlayer()
        {
        }

        public virtual void LegendSeasonScoreChanged(int state, int score, int scoreChange, bool bestSeason, int villageType)
        {
        }

        public virtual void ChallengeStateChanged(LogicLong challengeId, LogicLong argsId, int challengeState)
        {
        }

        public virtual void CancelFriendlyVersusBattle()
        {
        }

        public virtual void NameChanged(string name)
        {
        }

        public virtual void UnitReceivedFromAlliance(string name, LogicCombatItemData data, int upgLevel)
        {
        }

        public virtual LogicJSONObject ParseCompressedHomeJSON(byte[] compressed, int length)
        {
            return null;
        }

        public virtual void StarBonusAdded(int goldCount, int elixirCount, int darkElixirCount)
        {
        }

        public virtual void DuelReplayShared(LogicLong replayId)
        {
        }

        public virtual void ShieldActivated(int hours)
        {
        }
    }
}