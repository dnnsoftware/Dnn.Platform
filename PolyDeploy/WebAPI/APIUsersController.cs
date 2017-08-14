using Cantarus.Modules.PolyDeploy.Components;
using Cantarus.Modules.PolyDeploy.DataAccess.Models;
using DotNetNuke.Web.Api;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Cantarus.Modules.PolyDeploy.WebAPI
{
    public class APIUsersController : DnnApiController
    {
        [RequireHost]
        [HttpGet]
        public HttpResponseMessage Create(string name)
        {
            // Check we have a name.
            if (string.IsNullOrEmpty(name))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            // Create user.
            APIUser apiUser = APIUserManager.Create(name);

            return Request.CreateResponse(HttpStatusCode.Created, apiUser);
        }
    }
}
