using Cantarus.Libraries.Encryption;
using Cantarus.Modules.PolyDeploy.Components;
using Cantarus.Modules.PolyDeploy.DataAccess.Models;
using Cantarus.Modules.PolyDeploy.WebAPI.ActionFilters;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Web.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Cantarus.Modules.PolyDeploy.WebAPI
{
    public class CIController : DnnApiController
    {
        [AllowAnonymous]
        [InWhitelist]
        [APIAuthentication]
        [HttpGet]
        public HttpResponseMessage CreateSession()
        {
            Session session = SessionController.CreateSession();

            return Request.CreateResponse(HttpStatusCode.OK, session);
        }

        [AllowAnonymous]
        [InWhitelist]
        [APIAuthentication]
        [HttpGet]
        public HttpResponseMessage GetSession(string sessionGuid)
        {
            Session session = SessionController.GetSession(sessionGuid);

            return Request.CreateResponse(HttpStatusCode.OK, session);
        }

        [AllowAnonymous]
        [InWhitelist]
        [APIAuthentication]
        [HttpPost]
        public async Task<HttpResponseMessage> AddPackages(string sessionGuid)
        {
            if (!SessionController.SessionExists(sessionGuid))
            {
                // Session doesn't exist.
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid session.");
            }

            try
            {
                // Does the request contain multipart/form-data?
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                // Get the api key from the header.
                string apiKey = Request.Headers.GetValues("x-api-key").FirstOrDefault();

                // Get the api user.
                APIUser apiUser = APIUserController.GetByAPIKey(apiKey);

                // Receive files.
                MultipartMemoryStreamProvider provider = await Request.Content.ReadAsMultipartAsync();

                foreach (HttpContent file in provider.Contents)
                {
                    string filename = file.Headers.ContentDisposition.FileName.Replace("\"", "");

                    using (MemoryStream ms = new MemoryStream(await file.ReadAsByteArrayAsync()))
                    {
                        using (Stream ds = Crypto.Decrypt(ms, apiUser.EncryptionKey))
                        {
                            SessionController.AddPackage(sessionGuid, ds, filename);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [AllowAnonymous]
        [InWhitelist]
        [APIAuthentication]
        [HttpGet]
        public HttpResponseMessage Install(string sessionGuid)
        {
            if (!SessionController.SessionExists(sessionGuid))
            {
                // Session doesn't exist.
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid session.");
            }

            EventLogController elc = new EventLogController();

            string apiKey = null;

            try
            {
                // Get the users ip address.
                string ipAddress = HttpContext.Current.Request.UserHostAddress;

                // Get the api key from the header.
                apiKey = Request.Headers.GetValues("x-api-key").FirstOrDefault();

                // Get the session.
                Session sessionObj = SessionController.GetSession(sessionGuid);

                // Create a deploy operation.
                RemoteDeployment deployOperation = new RemoteDeployment(sessionObj, ipAddress, apiKey);

                // Deploy.
                deployOperation.Deploy();
            }
            catch (Exception ex)
            {
                string log = string.Format("(APIKey: {0}) Install failure: {1}", apiKey, ex.Message);

                elc.AddLog("PolyDeploy", log, EventLogController.EventLogType.HOST_ALERT);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Operation started.");
        }
    }
}
