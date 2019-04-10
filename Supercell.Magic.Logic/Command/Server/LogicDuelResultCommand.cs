namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public class LogicDuelResultCommand : LogicServerCommand
    {
        private int m_scoreGain;
        private int m_resultType;

        private bool m_attacker;

        public override void Destruct()
        {
            base.Destruct();
        }

        public override void Decode(ByteStream stream)
        {
            this.m_scoreGain = stream.ReadInt();
            this.m_attacker = stream.ReadBoolean();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_scoreGain);
            encoder.WriteBoolean(this.m_attacker);

            base.Encode(encoder);
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                playerAvatar.SetDuelScore(playerAvatar.GetDuelScore() + this.m_scoreGain);

                switch (this.m_resultType)
                {
                    case 0:
                        playerAvatar.SetDuelLoseCount(playerAvatar.GetDuelLoseCount() + 1);
                        break;
                    case 1:
                        playerAvatar.SetDuelWinCount(playerAvatar.GetDuelWinCount() + 1);
                        break;
                    case 2:
                        playerAvatar.SetDuelDrawCount(playerAvatar.GetDuelDrawCount() + 1);
                        break;
                }

                level.GetAchievementManager().RefreshStatus();

                LogicAvatar homeOwnerAvatar = level.GetHomeOwnerAvatar();

                if (homeOwnerAvatar.GetChangeListener() != null)
                {
                    homeOwnerAvatar.GetChangeListener().DuelScoreChanged(homeOwnerAvatar.GetAllianceId(), this.m_scoreGain, this.m_resultType, true);
                }

                return 0;
            }

            return -1;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.DUEL_RESULT;
        }

        public void SetData(int scoreGain, int resultType, bool attacker)
        {
            this.m_scoreGain = scoreGain;
            this.m_resultType = resultType;
            this.m_attacker = attacker;
        }
    }
}