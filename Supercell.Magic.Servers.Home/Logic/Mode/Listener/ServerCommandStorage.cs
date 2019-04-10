namespace Supercell.Magic.Servers.Home.Logic.Mode.Listener
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
        private readonly LogicArrayList<LogicServerCommand> m_bufferedServerCommands;
        private readonly LogicArrayList<LogicServerCommand> m_executedServerCommands;

        public ServerCommandStorage(GameMode gameMode, LogicGameMode logicGameMode)
        {
            this.m_gameMode = gameMode;
            this.m_logicGameMode = logicGameMode;
            this.m_bufferedServerCommands = new LogicArrayList<LogicServerCommand>();
            this.m_executedServerCommands = new LogicArrayList<LogicServerCommand>();
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_bufferedServerCommands.Clear();
        }

        public override void CommandExecuted(LogicCommand command)
        {
            if (command.IsServerCommand())
            {
                this.m_bufferedServerCommands.Remove(this.m_bufferedServerCommands.IndexOf((LogicServerCommand) command));
                this.m_executedServerCommands.Add((LogicServerCommand) command);
            }
        }

        public void AddServerCommand(LogicServerCommand serverCommand)
        {
            this.m_bufferedServerCommands.Add(serverCommand);
        }

        public bool GetAwaitingExecutionOfCommandType(LogicCommandType type)
        {
            for (int i = 0; i < this.m_bufferedServerCommands.Size(); i++)
            {
                if (this.m_bufferedServerCommands[i].GetCommandType() == type)
                    return true;
            }

            return false;
        }

        public LogicArrayList<LogicServerCommand> RemoveExecutedServerCommands()
        {
            LogicArrayList<LogicServerCommand> arrayList = new LogicArrayList<LogicServerCommand>();
            arrayList.AddAll(this.m_executedServerCommands);
            this.m_executedServerCommands.Clear();
            return arrayList;
        }

        public void CheckExecutableServerCommands(int endSubTick, LogicArrayList<LogicCommand> commands)
        {
            for (int i = 0; i < commands.Size(); i++)
            {
                LogicCommand command = commands[i];
                
                if (command.IsServerCommand())
                {
                    if (this.m_logicGameMode.GetState() != 1)
                    {
                        commands.Remove(i--);
                        continue;
                    }
                    
                    LogicServerCommand serverCommand = (LogicServerCommand) command;
                    LogicServerCommand bufferedServerCommand = null;

                    for (int j = 0; j < this.m_bufferedServerCommands.Size(); j++)
                    {
                        LogicServerCommand tmp = this.m_bufferedServerCommands[j];

                        if (tmp.GetId() == serverCommand.GetId())
                        {
                            bufferedServerCommand = tmp;
                        }
                    }

                    if (bufferedServerCommand == null || bufferedServerCommand.GetCommandType() != serverCommand.GetCommandType() ||
                        bufferedServerCommand.GetExecuteSubTick() != -1 && bufferedServerCommand.GetExecuteSubTick() >= this.m_logicGameMode.GetLevel().GetLogicTime().GetTick())
                    {
                        commands.Remove(i--);
                        continue;
                    }

                    bufferedServerCommand.SetExecuteSubTick(serverCommand.GetExecuteSubTick());
                    commands[i] = bufferedServerCommand;
                }
            }
        }
    }
}