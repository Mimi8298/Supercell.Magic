namespace Supercell.Magic.Servers.Home.Logic.Mode.Listener
{
    using Supercell.Magic.Logic.Mode;
    using Supercell.Magic.Servers.Core.Network.Message.Session;

    public class GameListener : LogicGameListener
    {
        private readonly GameMode m_gameMode;

        public GameListener(GameMode gameMode)
        {
            this.m_gameMode = gameMode;
        }

        public override void MatchmakingCommandExecuted()
        {
            this.m_gameMode.GetSession().SendMessage(new GameMatchmakingMessage
            {
                MatchmakingType = GameMatchmakingMessage.GameMatchmakingType.DEFAULT
            }, 9);
            this.m_gameMode.SetShouldDestruct();
        }

        public override void MatchmakingVillage2CommandExecuted()
        {
            this.m_gameMode.GetSession().SendMessage(new GameMatchmakingMessage
            {
                MatchmakingType = GameMatchmakingMessage.GameMatchmakingType.DUEL
            }, 9);
            this.m_gameMode.SetShouldDestruct();
        }

        public override void NameChanged(string name)
        {
            this.m_gameMode.GetAvatarChangeListener().NameChanged(name, this.m_gameMode.GetPlayerAvatar().GetNameChangeState());
        }
    }
}