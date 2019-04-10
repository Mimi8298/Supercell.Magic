namespace Supercell.Magic.Servers.Battle.Logic.Mode.Listener
{
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Logic.Command.Listener;
    using Supercell.Magic.Logic.Command.Server;
    using Supercell.Magic.Logic.Mode;

    using Supercell.Magic.Titan.Util;

    public class ServerCommandStorage : LogicCommandManagerListener
    {
        private readonly GameMode m_gameMode;
        private readonly LogicGameMode m_logicGameMode;

        public ServerCommandStorage(GameMode gameMode, LogicGameMode logicGameMode)
        {
            this.m_gameMode = gameMode;
            this.m_logicGameMode = logicGameMode;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override void CommandExecuted(LogicCommand command)
        {
        }
        
        public void CheckExecutableServerCommands(int endSubTick, LogicArrayList<LogicCommand> commands)
        {
            for (int i = 0; i < commands.Size(); i++)
            {
                LogicCommand command = commands[i];
                
                if (command.IsServerCommand())
                    commands.Remove(i--);
            }
        }
    }
}