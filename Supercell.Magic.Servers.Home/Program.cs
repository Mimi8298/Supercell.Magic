namespace Supercell.Magic.Servers.Home
{
    using System;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Servers.Home.Network.Message;

    internal class Program
    {
        private static void Main(string[] args)
        {
            ServerCore.Init(10, args);
            ServerHome.Init();
            ServerCore.Start(new HomeMessageManager());

            Console.Title = string.Format("{0} - {1}", ServerUtil.GetServerName(ServerCore.Type), ServerCore.Id);
        }
    }
}