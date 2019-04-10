namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicSeenBuilderMenuCommand : LogicCommand
    {
        private int m_villageType;

        public LogicSeenBuilderMenuCommand()
        {
            // LogicSeenBuilderMenuCommand.
        }

        public LogicSeenBuilderMenuCommand(int villageType)
        {
            this.m_villageType = villageType;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_villageType = stream.ReadInt();
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_villageType);
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.SEEN_BUILDER_MENU;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            if (this.m_villageType == 0)
            {
                level.GetPlayerAvatar().SetVariableByName("SeenBuilderMenu", 1);
                return 0;
            }

            return -1;
        }
    }
}