namespace Supercell.Magic.Servers.Home.Logic.Mode.Listener
{
    using Supercell.Magic.Logic.Home.Change;

    public class HomeChangeListener : LogicHomeChangeListener
    {
        private readonly GameMode m_gameMode;

        public HomeChangeListener(GameMode gameMode)
        {
            this.m_gameMode = gameMode;
        }
    }
}