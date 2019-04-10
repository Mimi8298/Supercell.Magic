namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Logic.Command.Server;
    using Supercell.Magic.Logic.Home;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Util;

    public class GameHomeState : GameState
    {
        public int MaintenanceTime { get; set; }
        public int LayoutId { get; set; }
        public int MapId { get; set; }

        public LogicArrayList<LogicServerCommand> ServerCommands { get; set; }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

            stream.WriteVInt(this.MaintenanceTime);
            stream.WriteVInt(this.LayoutId);
            stream.WriteVInt(this.MapId);

            for (int i = 0; i < this.ServerCommands.Size(); i++)
            {
                LogicCommandManager.EncodeCommand(stream, this.ServerCommands[i]);
            }
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.MaintenanceTime = stream.ReadVInt();
            this.LayoutId = stream.ReadVInt();
            this.MapId = stream.ReadVInt();
            this.ServerCommands = new LogicArrayList<LogicServerCommand>();

            for (int i = stream.ReadVInt(); i > 0; i--)
            {
                this.ServerCommands.Add((LogicServerCommand) LogicCommandManager.DecodeCommand(stream));
            }
        }

        public override GameStateType GetGameStateType()
        {
            return GameStateType.HOME;
        }
    }
}