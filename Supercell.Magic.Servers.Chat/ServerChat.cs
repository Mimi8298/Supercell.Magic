namespace Supercell.Magic.Servers.Chat
{
    using Supercell.Magic.Servers.Chat.Logic;
    using Supercell.Magic.Servers.Chat.Session;
    using Supercell.Magic.Servers.Core.Util;

    public static class ServerChat
    {
        public static void Init()
        {
            ChatSessionManager.Init();
            ChatInstanceManager.Init();
            WordCensorUtil.Init();
        }
    }
}