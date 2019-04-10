namespace Supercell.Magic.Servers.Proxy
{
    using System;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Servers.Proxy.Network.Message;

    internal class Program
    {
        private static void Main(string[] args)
        {
            ServerCore.Init(1, args);
            ServerProxy.Init();
            ServerCore.Start(new ProxyMessageManager());

            Console.Title = string.Format("{0} - {1}", ServerUtil.GetServerName(ServerCore.Type), ServerCore.Id);
        }
    }
}