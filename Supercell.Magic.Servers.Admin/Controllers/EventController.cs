namespace Supercell.Magic.Servers.Admin.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using Supercell.Magic.Servers.Admin.Attribute;
    using Supercell.Magic.Servers.Admin.Helper;
    using Supercell.Magic.Servers.Admin.Logic;
    using Supercell.Magic.Titan.Math;

    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EventController : Controller
    {
        [HttpGet]
        [AuthCheck(UserRole.MODERATOR)]
        public JObject Get()
        {
            return this.BuildResponse(HttpStatusCode.OK).AddAttribute("events", LogManager.Save());
        }

        [HttpPost]
        public JObject Insert([FromBody] JObject obj)
        {
            LogEventEntry.EventType type = (LogEventEntry.EventType) (int) obj["type"];
            long accountId = (long) obj["accountId"];
            Dictionary<string, object> args = new Dictionary<string, object>();

            JObject argsObj = (JObject) obj["args"];

            if (argsObj != null)
            {
                switch (type)
                {
                    case LogEventEntry.EventType.OUT_OF_SYNC:
                        args.Add("subTick", (int)argsObj["subTick"]);
                        args.Add("clientChecksum", (int)argsObj["clientChecksum"]);
                        args.Add("serverChecksum", (int)argsObj["serverChecksum"]);
                        args.Add("debugJSON", (string)argsObj["debugJSON"]);
                        break;
                    default:
                        return this.BuildResponse(HttpStatusCode.InternalServerError);
                }

                LogManager.AddEventLog(type, new LogicLong((int) (accountId >> 32), (int) accountId), args);

                return this.BuildResponse(HttpStatusCode.OK);
            }

            return this.BuildResponse(HttpStatusCode.InternalServerError);
        }
    }
}