namespace Supercell.Magic.Tools.Client
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Tools.Client.Network;

    internal class Program
    {
        private static Thread m_thread;
        private static List<ServerConnection> m_serverConnections;

        private static void Main(string[] args)
        {
            Debugger.SetListener(new DebuggerListener());
            ResourceManager.Init();

            Program.m_serverConnections = new List<ServerConnection>();
            Program.m_thread = new Thread(Program.Update);
            Program.m_thread.Start();
            Program.m_serverConnections.Add(new ServerConnection(Program.GetRandomAccount()));
        }

        private static LogicLong GetRandomAccount()
        {
            if (Directory.Exists("accounts"))
            {
                string[] files = Directory.GetFiles("accounts/", "*");

                if (files.Length > 0)
                {
                    string fileName = Path.GetFileName(files[Environment.TickCount % files.Length]);
                    string[] split = fileName.Split('-');

                    return new LogicLong(int.Parse(split[0]), int.Parse(split[1]));
                }
            }

            return null;
        }

        private static void Update()
        {
            while (true)
            {
                Parallel.ForEach(Program.m_serverConnections.ToArray(), serverConnection =>
                {
                    serverConnection.Update(0.016f);
                });

                Thread.Sleep(16);
            }
        }
    }
}