namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Titan.Message;

    public class AvailableServerCommandMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24111;

        private LogicCommand m_serverCommand;

        public AvailableServerCommandMessage() : this(0)
        {
            // AvailableServerCommandMessage.
        }

        public AvailableServerCommandMessage(short messageVersion) : base(messageVersion)
        {
            // AvailableServerCommandMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_serverCommand = LogicCommandManager.DecodeCommand(this.m_stream);
        }

        public override void Encode()
        {
            base.Encode();
            LogicCommandManager.EncodeCommand(this.m_stream, this.m_serverCommand);
        }

        public override short GetMessageType()
        {
            return AvailableServerCommandMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 10;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_serverCommand = null;
        }

        public LogicCommand RemoveServerCommand()
        {
            LogicCommand tmp = this.m_serverCommand;
            this.m_serverCommand = null;
            return tmp;
        }

        public void SetServerCommand(LogicCommand command)
        {
            this.m_serverCommand = command;
        }
    }
}