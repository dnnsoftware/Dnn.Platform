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

        [AllowAnonymous]
        [APIAuthentication]
        [HttpGet]
        public HttpResponseMessage Test()
        {

            string folderPath = Path.Combine("C:\\", "DNNDev", "INTO", "CompiledModules");

            try
            {
                InstallManager installManager = new InstallManager(folderPath);

                installManager.InstallPackages();

                return Request.CreateResponse(HttpStatusCode.OK, installManager);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            

            return Request.CreateResponse(HttpStatusCode.OK, "API User Controller");
        }
    }
}
