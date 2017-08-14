using Cantarus.Modules.PolyDeploy.Components;
using DotNetNuke.Web.Api;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Cantarus.Modules.PolyDeploy.WebAPI
{
    [AllowAnonymous] // TODO: Secure this controller properly.
    public class SessionController : DnnApiController
    {
        // TODO: Will use DNN SF to secure.
        [HttpPost]
        public HttpResponseMessage Create()
        {
            var session = SessionManager.CreateSession();

            return Request.CreateResponse(HttpStatusCode.OK, session);
        }

        // TODO: Will use DNN SF to secure.
        [HttpGet]
        public HttpResponseMessage Get(string guid)
        {
            var session = SessionManager.GetSession(guid);

            return Request.CreateResponse(HttpStatusCode.OK, session);
        }
    }
}
