namespace Supercell.Magic.Servers.Admin.Logic
{
    using System;
    using System.Threading;
    using Couchbase.N1QL;
    using Newtonsoft.Json.Linq;
    using Supercell.Magic.Servers.Core.Util;

    public static class DashboardManager
    {
        public static int TotalUsers { get; set; }
        public static int DailyActiveUsers { get; set; }
        public static int NewUsers { get; set; }

        private static Thread m_thread;

        public static void Init()
        {
            DashboardManager.m_thread = new Thread(DashboardManager.Update);
            DashboardManager.m_thread.Start();
        }

        private static void Update()
        {
            while (true)
            {
                int todayTimestamp = TimeUtil.GetTimestamp(DateTime.Today);
                int tomorrowTimestamp = todayTimestamp + 86400;

                IQueryResult<JObject> totalUsersResult = ServerAdmin.AccountDatabase.ExecuteCommand<JObject>("SELECT COUNT(*) FROM `%BUCKET%` WHERE meta().id LIKE \"%KEY_PREFIX%%\"").Result;
                IQueryResult<JObject> dailyActiveUsersResult = ServerAdmin
                                                               .AccountDatabase
                                                               .ExecuteCommand<JObject>("SELECT COUNT(*) FROM `%BUCKET%` WHERE meta().id LIKE \"%KEY_PREFIX%%\" AND lastSessionTime BETWEEN " +
                                                                                        todayTimestamp + " AND " + tomorrowTimestamp).Result;
                IQueryResult<JObject> newUsersResult = ServerAdmin
                                                       .AccountDatabase.ExecuteCommand<JObject>("SELECT COUNT(*) FROM `%BUCKET%` WHERE meta().id LIKE \"%KEY_PREFIX%%\" AND createTime >= " +
                                                                                                todayTimestamp).Result;

                if (totalUsersResult.Success) DashboardManager.TotalUsers = (int) totalUsersResult.Rows[0]["$1"];
                if (dailyActiveUsersResult.Success) DashboardManager.DailyActiveUsers = (int) dailyActiveUsersResult.Rows[0]["$1"];
                if (newUsersResult.Success) DashboardManager.NewUsers = (int) newUsersResult.Rows[0]["$1"];

                Thread.Sleep(30000);
            }
        }
    }
}