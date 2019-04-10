namespace Supercell.Magic.Servers.Battle.Session
{
    using System;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Session;

    using Supercell.Magic.Servers.Battle.Logic.Mode;
    using Supercell.Magic.Servers.Battle.Session.Message;

    public class BattleSession : ServerSession
    {
        public LogicMessageManager LogicMessageManager { get; }
        public GameMode GameMode { get; private set; }

        public BattleSession(StartServerSessionMessage message) : base(message)
        {
            this.LogicMessageManager = new LogicMessageManager(this);
        }

        public override void Destruct()
        {
            if (this.GameMode != null)
                this.GameMode.Destruct();
            base.Destruct();
        }

        public void SetGameMode(GameMode gameMode)
        {
            if (this.GameMode != null)
                this.GameMode.Destruct();
            this.GameMode = gameMode;
        }

        public void DestructGameMode()
        {
            if (this.GameMode != null)
            {
                this.GameMode.Destruct();
                this.GameMode = null;
            }
        }
    }
}