using Cantarus.Modules.PolyDeploy.Components;
using Cantarus.Modules.PolyDeploy.DataAccess.Models;
using DotNetNuke.Web.Api;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Cantarus.Modules.PolyDeploy.WebAPI
{
    public class APIUsersController : DnnApiController
    {   
        [AllowAnonymous]
        [APIAuthentication]
        [HttpGet]
        public HttpResponseMessage Test()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "API User Controller");
        }
    }
}
