namespace Supercell.Magic.Servers.Battle
{
    using System;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Servers.Battle.Network.Message;

    internal class Program
    {
        private static void Main(string[] args)
        {
            ServerCore.Init(27, args);
            ServerBattle.Init();
            ServerCore.Start(new BattleMessageManager());

            Console.Title = string.Format("{0} - {1}", ServerUtil.GetServerName(ServerCore.Type), ServerCore.Id);
        }
    }
}