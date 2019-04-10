namespace Supercell.Magic.Servers.Battle.Session.Message
{
    using Supercell.Magic.Logic.Message.Battle;
    using Supercell.Magic.Servers.Battle.Logic.Mode;
    using Supercell.Magic.Servers.Battle.Session;
    using Supercell.Magic.Titan.Message;

    public class LogicMessageManager
    {
        private readonly BattleSession m_session;

        public LogicMessageManager(BattleSession session)
        {
            this.m_session = session;
        }

        public void ReceiveMessage(PiranhaMessage message)
        {
            switch (message.GetMessageType())
            {
                case BattleEndClientTurnMessage.MESSAGE_TYPE:
                    this.OnBattleEndClientTurnMessageReceived((BattleEndClientTurnMessage) message);
                    break;
            }
        }

        private void OnBattleEndClientTurnMessageReceived(BattleEndClientTurnMessage message)
        {
            GameMode gameMode = this.m_session.GameMode;

            if (gameMode != null)
                gameMode.OnClientTurnReceived(message.GetSubTick(), message.GetChecksum(), message.GetCommands());
        }
    }
}