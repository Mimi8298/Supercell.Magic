namespace Supercell.Magic.Servers.Admin.Controllers
{
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json.Linq;

    using Supercell.Magic.Servers.Admin.Attribute;
    using Supercell.Magic.Servers.Admin.Helper;
    using Supercell.Magic.Servers.Admin.Logic;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Titan.Math;

    [Route("api/[controller]")]
    [Produces("application/json")]
    [AuthCheck(UserRole.MODERATOR)]
    public class ServerController : Controller
    {
        [HttpGet]
        public JObject Index()
        {
            return this.BuildResponse(HttpStatusCode.OK).AddAttribute("servers", ServerManager.Save());
        }

        [HttpGet("startMaintenance")]
        public JObject StartMaintenance()
        {
            if ((ServerStatus.Status == ServerStatusType.NORMAL || ServerStatus.Status == ServerStatusType.COOLDOWN_AFTER_MAINTENANCE) && this.Request.Query.TryGetValue("duration", out StringValues durationStr))
            {
                ServerStatus.SetStatus(ServerStatusType.SHUTDOWN_STARTED, TimeUtil.GetTimestamp() + 300, LogicMath.Max(int.Parse(durationStr) * 60, 0));
                return this.BuildResponse(HttpStatusCode.OK);
            }

            return this.BuildResponse(HttpStatusCode.Forbidden);
        }

        [HttpGet("stopMaintenance")]
        public JObject StopMaintenance()
        {
            if (ServerStatus.Status == ServerStatusType.MAINTENANCE)
            {
                ServerStatus.SetStatus(ServerStatusType.COOLDOWN_AFTER_MAINTENANCE, TimeUtil.GetTimestamp() + 300, 0);
                return this.BuildResponse(HttpStatusCode.OK);
            }

            return this.BuildResponse(HttpStatusCode.Forbidden);
        }

        [HttpGet("cancelCooldown")]
        public JObject CancelCooldown()
        {
            if (ServerStatus.Status == ServerStatusType.COOLDOWN_AFTER_MAINTENANCE)
            {
                ServerStatus.SetStatus(ServerStatusType.NORMAL, 0, 0);
                return this.BuildResponse(HttpStatusCode.OK);
            }

            return this.BuildResponse(HttpStatusCode.Forbidden);
        }

        [HttpGet("extendMaintenance")]
        public JObject ExtendMaintenance()
        {
            if (ServerStatus.Status == ServerStatusType.MAINTENANCE && this.Request.Query.TryGetValue("duration", out StringValues durationStr))
            {
                ServerStatus.SetStatus(ServerStatusType.MAINTENANCE, TimeUtil.GetTimestamp() + LogicMath.Max(int.Parse(durationStr) * 60, 0), 0);
                return this.BuildResponse(HttpStatusCode.OK);
            }

            return this.BuildResponse(HttpStatusCode.Forbidden);
        }
    }
}