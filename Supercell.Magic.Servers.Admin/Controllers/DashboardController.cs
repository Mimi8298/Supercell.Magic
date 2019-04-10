namespace Supercell.Magic.Servers.Admin.Controllers
{
    using System.Net;

    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;

    using Supercell.Magic.Servers.Admin.Attribute;
    using Supercell.Magic.Servers.Admin.Helper;
    using Supercell.Magic.Servers.Admin.Logic;

    [Route("api/[controller]")]
    [Produces("application/json")]
    [AuthCheck(UserRole.DEFAULT)]
    public class DashboardController : Controller
    {
        [HttpGet]
        public JObject Index()
        {
            return this.BuildResponse(HttpStatusCode.OK)
                       .AddAttribute("totalUsers", DashboardManager.TotalUsers)
                       .AddAttribute("dailyActiveUsers", DashboardManager.DailyActiveUsers)
                       .AddAttribute("newUsers", DashboardManager.NewUsers)
                       .AddAttribute("onlineUsers", ServerManager.OnlineUsers);
        }
    }
}