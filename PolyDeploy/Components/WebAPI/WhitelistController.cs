using Cantarus.Modules.PolyDeploy.Components;
using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using Cantarus.Modules.PolyDeploy.Components.WebAPI.ActionFilters;
using DotNetNuke.Web.Api;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Cantarus.Modules.PolyDeploy.Components.WebAPI
{
    [RequireHost]
    [ValidateAntiForgeryToken]
    [InWhitelist]
    public class WhitelistController : DnnApiController
    {
        [HttpGet]
        public HttpResponseMessage Create(string ip)
        {
            IPSpec ipSpec = IPSpecController.AddWhitelistIp(ip);

            return Request.CreateResponse(HttpStatusCode.OK, ipSpec);
        }
    }
}
