namespace Supercell.Magic.Servers.Scoring
{
    using System;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Servers.Scoring.Network.Message;

    internal class Program
    {
        private static void Main(string[] args)
        {
            ServerCore.Init(28, args);
            ServerScoring.Init();
            ServerCore.Start(new ScoringMessageManager());

            Console.Title = string.Format("{0} - {1}", ServerUtil.GetServerName(ServerCore.Type), ServerCore.Id);
        }
    }
}