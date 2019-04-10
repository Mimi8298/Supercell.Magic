namespace Supercell.Magic.Servers.Home.Session
{
    using System;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Session;

    using Supercell.Magic.Servers.Home.Logic.Mode;
    using Supercell.Magic.Servers.Home.Session.Message;

    public class HomeSession : ServerSession
    {
        public LogicMessageManager LogicMessageManager { get; }
        public GameMode GameMode { get; private set; }

        public HomeSession(StartServerSessionMessage message) : base(message)
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