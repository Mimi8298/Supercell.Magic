namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Mode;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicBreakShieldCommand : LogicCommand
    {
        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.BREAK_SHIELD;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            if (level.GetVillageType() == 0)
            {
                LogicGameMode gameMode = level.GetGameMode();

                if (gameMode.GetShieldRemainingSeconds() <= 0)
                {
                    gameMode.SetShieldRemainingSeconds(0);
                    gameMode.SetGuardRemainingSeconds(0);

                    gameMode.GetLevel().GetHome().GetChangeListener().ShieldActivated(0, 0);
                }
                else
                {
                    int guardTime = gameMode.GetGuardRemainingSeconds();

                    gameMode.SetShieldRemainingSeconds(0);
                    gameMode.SetGuardRemainingSeconds(guardTime);
                    gameMode.SetPersonalBreakCooldownSeconds(LogicDataTables.GetGlobals().GetPersonalBreakLimitSeconds());

                    level.GetHome().GetChangeListener().ShieldActivated(0, guardTime);
                }

                return 0;
            }

            return -32;
        }
    }
}