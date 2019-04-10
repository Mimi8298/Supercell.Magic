namespace Supercell.Magic.Servers.Chat.Session
{
    using Supercell.Magic.Servers.Chat.Logic;
    using Supercell.Magic.Servers.Chat.Session.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Session;
    
    public class ChatSession : ServerSession
    {
        public LogicMessageManager LogicMessageManager { get; }
        public ChatInstance ChatInstance { get; private set; }

        public ChatSession(StartServerSessionMessage message) : base(message)
        {
            this.LogicMessageManager = new LogicMessageManager(this);
            this.ChatInstance = ChatInstanceManager.GetJoinableInstance(this.Country);
            this.ChatInstance.Add(this);
        }

        public override void Destruct()
        {
            this.ChatInstance.Remove(this);
            this.ChatInstance = null;
            base.Destruct();
        }
    }
}