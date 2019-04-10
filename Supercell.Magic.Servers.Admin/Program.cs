namespace Supercell.Magic.Servers.Admin
{
    using System;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Supercell.Magic.Servers.Admin.Logic;
    using Supercell.Magic.Servers.Admin.Network.Message;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Settings;

    public class Program
    {
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                          .UseUrls("http://0.0.0.0:5000")
                          .UseStartup<Startup>();
        }

        public static void Main(string[] args)
        {
            ServerCore.Init(0, args);
            ServerAdmin.Init();
            ServerCore.Start(new AdminMessageManager());

            if (EnvironmentSettings.Environment.Equals("prod", StringComparison.InvariantCultureIgnoreCase))
                ServerStatus.SetStatus(ServerStatusType.MAINTENANCE, 0, 0);

            Program.CreateWebHostBuilder(args).Build().Run();
        }
    }
}