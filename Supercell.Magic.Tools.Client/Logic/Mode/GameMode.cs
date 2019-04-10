namespace Supercell.Magic.Tools.Client.Logic.Mode
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Home;
    using Supercell.Magic.Logic.Mode;

    public class GameMode
    {
        private readonly LogicGameMode m_logicGameMode;

        public GameMode()
        {
            this.m_logicGameMode = new LogicGameMode();
        }

        public void LoadHomeState(LogicClientHome logicClientHome, LogicClientAvatar logicClientAvatar, int currentTimestamp, int passedSeconds, int secondsSinceLastMaintenance,
                                  int reengagementSeconds)
        {
            this.m_logicGameMode.LoadHomeState(logicClientHome, logicClientAvatar, passedSeconds, 0, currentTimestamp, secondsSinceLastMaintenance, reengagementSeconds);
        }
    }
}