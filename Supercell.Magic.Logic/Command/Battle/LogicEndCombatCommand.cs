namespace Supercell.Magic.Logic.Command.Battle
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicEndCombatCommand : LogicCommand
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
            return LogicCommandType.END_COMBAT;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            if (level.GetBattleLog().GetBattleStarted())
            {
                level.EndBattle();
                level.GetGameListener().BattleEndedByPlayer();

                return 0;
            }

            return -1;
        }
    }
}