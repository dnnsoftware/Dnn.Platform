using Cantarus.Modules.PolyDeploy.Components;
using Cantarus.Modules.PolyDeploy.DataAccess.Models;
using DotNetNuke.Web.Api;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Cantarus.Modules.PolyDeploy.WebAPI
{
    public class WhitelistController : DnnApiController
    {
        [RequireHost]
        [InWhitelist]
        [HttpGet]
        public HttpResponseMessage Create(string ip)
        {
            IPSpec ipSpec = IPSpecController.AddWhitelistIp(ip);

            return Request.CreateResponse(HttpStatusCode.OK, ipSpec);
        }
    }
}
