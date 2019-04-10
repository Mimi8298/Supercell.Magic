namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class LiveReplayHeaderMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24118;

        private int m_serverSubTick;
        private string m_streamHeaderJson;
        private byte[] m_compressedstreamHeaderJson;

        private LogicArrayList<LogicCommand> m_commands;

        public LiveReplayHeaderMessage() : this(0)
        {
            // LiveReplayHeaderMessage.
        }

        public LiveReplayHeaderMessage(short messageVersion) : base(messageVersion)
        {
            // LiveReplayHeaderMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_streamHeaderJson = this.m_stream.ReadString(900000);
            this.m_compressedstreamHeaderJson = this.m_stream.ReadBytes(this.m_stream.ReadBytesLength(), 900000);
            this.m_serverSubTick = this.m_stream.ReadInt();

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
                Debugger.Error(string.Format("LiveReplayHeaderMessage::decode() command count is too high! ({0})", count));
            }

            this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteString(this.m_streamHeaderJson);
            this.m_stream.WriteBytes(this.m_compressedstreamHeaderJson, this.m_compressedstreamHeaderJson.Length);
            this.m_stream.WriteInt(this.m_serverSubTick);

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

            this.m_stream.WriteInt(0);
        }

        public override short GetMessageType()
        {
            return LiveReplayHeaderMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_compressedstreamHeaderJson = null;
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

        public LogicArrayList<LogicCommand> GetCommands()
        {
            return this.m_commands;
        }

        public void SetCommands(LogicArrayList<LogicCommand> commands)
        {
            this.m_commands = commands;
        }

        public void SetCompressedStreamHeaderJson(byte[] value)
        {
            this.m_compressedstreamHeaderJson = value;
        }

        public byte[] RemoveCompressedstreamHeaderJson()
        {
            byte[] tmp = this.m_compressedstreamHeaderJson;
            this.m_compressedstreamHeaderJson = null;
            return tmp;
        }
    }
}