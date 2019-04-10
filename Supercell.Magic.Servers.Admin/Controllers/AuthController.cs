namespace Supercell.Magic.Servers.Admin.Controllers
{
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;

    using Newtonsoft.Json.Linq;

    using Supercell.Magic.Servers.Admin.Attribute;
    using Supercell.Magic.Servers.Admin.Helper;
    using Supercell.Magic.Servers.Admin.Logic;

    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : Controller
    {
        public const string TOKEN_ATTRIBUTE = "token";

        [HttpPost]
        [Route("login")]
        public JObject Login([FromBody] LoginRequest request)
        {
            if (this.Request.Headers.TryGetValue(AuthController.TOKEN_ATTRIBUTE, out StringValues prevToken) && AuthManager.IsOpenSession(prevToken))
                return this.BuildResponse(HttpStatusCode.Forbidden);
            if (!AuthManager.OpenSession(request.User, request.Password, out string token))
                return this.BuildResponse(HttpStatusCode.NotFound);
            return this.BuildResponse(HttpStatusCode.OK).AddAttribute("token", token);
        }

        [HttpGet]
        [Route("logout")]
        [AuthCheck(UserRole.DEFAULT)]
        public JObject Logout()
        {
            if (!AuthManager.CloseSession(this.Request.Headers[AuthController.TOKEN_ATTRIBUTE]))
                return this.BuildResponse(HttpStatusCode.NotFound);
            return this.BuildResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("info")]
        [AuthCheck(UserRole.DEFAULT)]
        public JObject GetUserInfo()
        {
            if (AuthManager.TryGetSession(this.Request.Headers[AuthController.TOKEN_ATTRIBUTE], out SessionEntry entry))
                return this.BuildResponse(HttpStatusCode.OK).AddAttribute("name", entry.User.User);
            return this.BuildResponse(HttpStatusCode.Forbidden);
        }

        public class LoginRequest
        {
            public string User { get; set; }
            public string Password { get; set; }
        }
    }
}