namespace Supercell.Magic.Servers.Search
{
    using System;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Servers.Search.Network.Message;

    internal class Program
    {
        private static void Main(string[] args)
        {
            ServerCore.Init(29, args);
            ServerSearch.Init();
            ServerCore.Start(new SearchMessageManager());

            Console.Title = string.Format("{0} - {1}", ServerUtil.GetServerName(ServerCore.Type), ServerCore.Id);
        }
    }
}