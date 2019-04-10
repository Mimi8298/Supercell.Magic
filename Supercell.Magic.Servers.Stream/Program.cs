namespace Supercell.Magic.Servers.Stream
{
    using System;
    using Supercell.Magic.Servers.Stream.Network.Message;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Util;

    internal class Program
    {
        private static void Main(string[] args)
        {
            ServerCore.Init(11, args);
            ServerStream.Init();
            ServerCore.Start(new StreamMessageManager());

            Console.Title = string.Format("{0} - {1}", ServerUtil.GetServerName(ServerCore.Type), ServerCore.Id);
        }
    }
}