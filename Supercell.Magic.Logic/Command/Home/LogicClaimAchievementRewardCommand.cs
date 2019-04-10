namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicClaimAchievementRewardCommand : LogicCommand
    {
        private LogicAchievementData m_achievementData;

        public LogicClaimAchievementRewardCommand()
        {
            // LogicClaimAchievementRewardCommand.
        }

        public LogicClaimAchievementRewardCommand(LogicAchievementData achievementData)
        {
            this.m_achievementData = achievementData;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_achievementData = (LogicAchievementData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.ACHIEVEMENT);
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            ByteStreamHelper.WriteDataReference(encoder, this.m_achievementData);
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.CLAIM_ACHIEVEMENT_REWARD;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_achievementData = null;
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null && this.m_achievementData != null)
            {
                if (playerAvatar.IsAchievementCompleted(this.m_achievementData) && !playerAvatar.IsAchievementRewardClaimed(this.m_achievementData))
                {
                    playerAvatar.XpGainHelper(this.m_achievementData.GetExpReward());

                    if (this.m_achievementData.GetDiamondReward() > 0)
                    {
                        int diamondReward = this.m_achievementData.GetDiamondReward();

                        playerAvatar.SetDiamonds(playerAvatar.GetDiamonds() + diamondReward);
                        playerAvatar.SetFreeDiamonds(playerAvatar.GetFreeDiamonds() + diamondReward);
                        playerAvatar.GetChangeListener().FreeDiamondsAdded(diamondReward, 4);
                    }

                    playerAvatar.SetAchievementRewardClaimed(this.m_achievementData, true);
                    playerAvatar.GetChangeListener().CommodityCountChanged(1, this.m_achievementData, 1);

                    return 0;
                }
            }

            return -1;
        }
    }
}