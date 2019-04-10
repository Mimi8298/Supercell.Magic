namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Logic.Command.Server;
    using Supercell.Magic.Titan.DataStream;

    public class GameAllowServerCommandMessage : ServerAccountMessage
    {
        public LogicServerCommand ServerCommand { get; set; }

        public override void Encode(ByteStream stream)
        {
            LogicCommandManager.EncodeCommand(stream, this.ServerCommand);
        }

        public override void Decode(ByteStream stream)
        {
            this.ServerCommand = (LogicServerCommand) LogicCommandManager.DecodeCommand(stream);
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.GAME_ALLOW_SERVER_COMMAND;
        }
    }
}