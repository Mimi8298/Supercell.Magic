namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class EndClientTurnMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14102;

        private int m_subTick;
        private int m_checksum;

        private LogicArrayList<LogicCommand> m_commands;
        
        public EndClientTurnMessage() : this(0)
        {
            // EndClientTurnMessage.
        }

        public EndClientTurnMessage(short messageVersion) : base(messageVersion)
        {
            // EndClientTurnMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_subTick = this.m_stream.ReadInt();
            this.m_checksum = this.m_stream.ReadInt();

            int arraySize = this.m_stream.ReadInt();

            if (arraySize <= 1024)
            {
                if (arraySize > 0)
                {
                    this.m_commands = new LogicArrayList<LogicCommand>(arraySize);

                    do
                    {
                        LogicCommand command = LogicCommandManager.DecodeCommand(this.m_stream);

                        if (command == null)
                        {
                            break;
                        }

                        this.m_commands.Add(command);
                    } while (--arraySize != 0);
                }
            }
            else
            {
                Debugger.Error(string.Format("EndClientTurn::decode() command count is too high! ({0})", arraySize));
            }
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(this.m_subTick);
            this.m_stream.WriteInt(this.m_checksum);

            if (this.m_commands != null)
            {
                this.m_stream.WriteInt(this.m_commands.Size());

                for (int i = 0; i < this.m_commands.Size(); i++)
                {
                    LogicCommandManager.EncodeCommand(this.m_stream, this.m_commands[i]);
                }
            }
            else
            {
                this.m_stream.WriteInt(-1);
            }
        }

        public override short GetMessageType()
        {
            return EndClientTurnMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 10;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_commands = null;
        }

        public int GetSubTick()
        {
            return this.m_subTick;
        }

        public void SetSubTick(int value)
        {
            this.m_subTick = value;
        }

        public int GetChecksum()
        {
            return this.m_checksum;
        }

        public void SetChecksum(int value)
        {
            this.m_checksum = value;
        }

        public LogicArrayList<LogicCommand> GetCommands()
        {
            return this.m_commands;
        }

        public void SetCommands(LogicArrayList<LogicCommand> commands)
        {
            this.m_commands = commands;
        }
    }
}