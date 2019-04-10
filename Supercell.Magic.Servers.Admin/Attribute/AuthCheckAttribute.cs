namespace Supercell.Magic.Servers.Admin.Attribute
{
    using System;
    using System.Text;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Primitives;
    using Supercell.Magic.Servers.Admin.Controllers;
    using Supercell.Magic.Servers.Admin.Logic;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class AuthCheckAttribute : Attribute, IAuthorizationFilter
    {
        public UserRole m_minRole;

        public AuthCheckAttribute(UserRole minRole)
        {
            this.m_minRole = minRole;
        }
        
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (this.m_minRole <= UserRole.NULL)
                return;

            if (!context.HttpContext.Request.Headers.TryGetValue(AuthController.TOKEN_ATTRIBUTE, out StringValues token) || !AuthManager.TryGetSession(token, out SessionEntry entry))
            {
                context.Result = new EmptyResult();
                context.HttpContext.Response.StatusCode = 401;
                context.HttpContext.Response.Body.Write(Encoding.UTF8.GetBytes("invalid token"));

                return;
            }

            if (entry.User.Role < this.m_minRole)
            {
                context.Result = new EmptyResult();
                context.HttpContext.Response.StatusCode = 401;
                context.HttpContext.Response.Body.Write(Encoding.UTF8.GetBytes("wrong privileges"));

                return;
            }
        }
    }
}