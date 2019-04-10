namespace Supercell.Magic.Servers.Core.Network.Message.Core
{
    public abstract class ServerCoreMessage : ServerMessage
    {
        public sealed override ServerMessageCategory GetMessageCategory()
        {
            return ServerMessageCategory.CORE;
        }
    }
}