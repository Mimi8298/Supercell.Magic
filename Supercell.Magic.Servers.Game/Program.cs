namespace Supercell.Magic.Servers.Game
{
    using System;
    using Supercell.Magic.Servers.Game.Network.Message;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Util;

    internal class Program
    {
        private static void Main(string[] args)
        {
            ServerCore.Init(9, args);
            ServerGame.Init();
            ServerCore.Start(new GameMessageManager());

            Console.Title = string.Format("{0} - {1}", ServerUtil.GetServerName(ServerCore.Type), ServerCore.Id);
        }
    }
}