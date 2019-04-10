namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicAccountBoundCommand : LogicCommand
    {
        private int m_bound;

        public override void Decode(ByteStream stream)
        {
            this.m_bound = stream.ReadInt();
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_bound);
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.ACCOUNT_BOUND;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            level.GetPlayerAvatar().SetAccountBound();
            level.GetAchievementManager().RefreshStatus();

            return 0;
        }
    }
}