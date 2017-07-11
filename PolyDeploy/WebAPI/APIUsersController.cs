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
        public HttpResponseMessage CreateUser()
        {
            APIUser newUser = APIUserController.Create("TestAPIUser");

            return Request.CreateResponse(HttpStatusCode.OK, newUser.APIKey);
        }
    }
}
