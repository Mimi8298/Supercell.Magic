namespace Supercell.Magic.Servers.Admin.Controllers
{
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using Supercell.Magic.Servers.Admin.Helper;
    using Supercell.Magic.Servers.Admin.Logic;

    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PublicController : Controller
    {
        [HttpGet]
        public JObject Index()
        {
            return this.BuildResponse(HttpStatusCode.OK)
                       .AddAttribute("totalUsers", DashboardManager.TotalUsers)
                       .AddAttribute("onlineUsers", ServerManager.OnlineUsers)
                       .AddAttribute("averagePings", ServerManager.AveragePings);
        }
    }
}