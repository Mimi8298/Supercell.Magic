namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class LiveReplayDataMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24119;

        private int m_serverSubTick;
        private int m_viewerCount;
        private int m_enemyViewerCount;

        private LogicArrayList<LogicCommand> m_commands;

        public LiveReplayDataMessage() : this(0)
        {
            // LiveReplayDataMessage.
        }

        public LiveReplayDataMessage(short messageVersion) : base(messageVersion)
        {
            // LiveReplayDataMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_serverSubTick = this.m_stream.ReadVInt();
            this.m_viewerCount = this.m_stream.ReadVInt();
            this.m_enemyViewerCount = this.m_stream.ReadVInt();

            int count = this.m_stream.ReadInt();

            if (count <= 512)
            {
                if (count > 0)
                {
                    this.m_commands = new LogicArrayList<LogicCommand>(count);

                    for (int i = 0; i < count; i++)
                    {
                        LogicCommand command = LogicCommandManager.DecodeCommand(this.m_stream);

                        if (command != null)
                        {
                            this.m_commands.Add(command);
                        }
                    }
                }
            }
            else
            {
                Debugger.Error(string.Format("LiveReplayDataMessage::decode() command count is too high! ({0})", count));
            }
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteVInt(this.m_serverSubTick);
            this.m_stream.WriteVInt(this.m_viewerCount);
            this.m_stream.WriteVInt(this.m_enemyViewerCount);

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
                this.m_stream.WriteInt(0);
            }
        }

        public override short GetMessageType()
        {
            return LiveReplayDataMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_commands = null;
        }

        public int GetServerSubTick()
        {
            return this.m_serverSubTick;
        }

        public void SetServerSubTick(int value)
        {
            this.m_serverSubTick = value;
        }

        public void SetViewerCount(int value)
        {
            this.m_viewerCount = value;
        }

        public int GetViewerCount()
        {
            return this.m_viewerCount;
        }

        public void SetEnemyViewerCount(int value)
        {
            this.m_enemyViewerCount = value;
        }

        public int GetEnemyViewerCount()
        {
            return this.m_enemyViewerCount;
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