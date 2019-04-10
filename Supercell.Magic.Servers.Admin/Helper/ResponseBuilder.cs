namespace Supercell.Magic.Servers.Admin.Helper
{
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;

    public static class ResponseBuilder
    {
        public static JObject BuildResponse(this Controller controller, HttpStatusCode successCode)
        {
            JObject jObject = new JObject();

            jObject.Add("success", (int) successCode);
            controller.Response.StatusCode = (int) successCode;

            return jObject;
        }

        public static JObject AddAttribute(this JObject obj, string attribute, JToken content)
        {
            obj.Add(attribute, content);
            return obj;
        }
    }
}