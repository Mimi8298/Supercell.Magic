namespace Supercell.Magic.Servers.Chat
{
    using System;
    using Supercell.Magic.Servers.Chat.Network.Message;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Util;

    internal class Program
    {
        private static void Main(string[] args)
        {
            ServerCore.Init(6, args);
            ServerChat.Init();
            ServerCore.Start(new ChatMessageManager());

            Console.Title = string.Format("{0} - {1}", ServerUtil.GetServerName(ServerCore.Type), ServerCore.Id);
        }
    }
}