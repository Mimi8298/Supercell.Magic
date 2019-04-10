namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicFreeWorkerCommand : LogicCommand
    {
        private int m_villageType;
        private LogicCommand m_command;

        public LogicFreeWorkerCommand()
        {
            // LogicBuyResourcesCommand.
        }

        public LogicFreeWorkerCommand(LogicCommand resourceCommand, int villageType)
        {
            this.m_command = resourceCommand;
            this.m_villageType = villageType;
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_villageType = stream.ReadInt();

            if (stream.ReadBoolean())
            {
                this.m_command = LogicCommandManager.DecodeCommand(stream);
            }
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            base.Encode(encoder);

            encoder.WriteInt(this.m_villageType);

            if (this.m_command != null)
            {
                encoder.WriteBoolean(true);
                LogicCommandManager.EncodeCommand(encoder, this.m_command);
            }
            else
            {
                encoder.WriteBoolean(false);
            }
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.FREE_WORKER;
        }

        public override void Destruct()
        {
            base.Destruct();

            if (this.m_command != null)
            {
                this.m_command.Destruct();
                this.m_command = null;
            }
        }

        public override int Execute(LogicLevel level)
        {
            int villageType = this.m_villageType != -1 ? this.m_villageType : level.GetVillageType();
            int freeWorkers = level.GetWorkerManagerAt(villageType).GetFreeWorkers();

            if (freeWorkers == 0)
            {
                if (level.GetWorkerManagerAt(villageType).FinishTaskOfOneWorker())
                {
                    if (this.m_command != null)
                    {
                        int commandType = (int) this.m_command.GetCommandType();

                        if (commandType < 1000)
                        {
                            if (commandType >= 500 && commandType < 700)
                            {
                                this.m_command.Execute(level);
                            }
                        }
                    }

                    return 0;
                }
            }

            return -1;
        }
    }
}