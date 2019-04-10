namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public class LogicShuffleLeagueCommand : LogicServerCommand
    {
        private int m_args;

        public override void Destruct()
        {
            base.Destruct();
        }

        public override void Decode(ByteStream stream)
        {
            this.m_args = stream.ReadInt();
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_args);
            base.Encode(encoder);
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                playerAvatar.SetLeagueType(0);
                playerAvatar.SetLeagueInstanceId(null);
                playerAvatar.SetAttackWinCount(0);
                playerAvatar.SetAttackLoseCount(0);
                playerAvatar.SetDefenseWinCount(0);
                playerAvatar.SetDefenseLoseCount(0);
                level.SetLastLeagueShuffle(true);

                playerAvatar.GetChangeListener().LeagueChanged(0, null);

                return 0;
            }

            return -1;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.SHUFFLE_LEAGUE;
        }
    }
}