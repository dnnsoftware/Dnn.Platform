using Cantarus.Libraries.Encryption;
using Cantarus.Modules.PolyDeploy.Components;
using Cantarus.Modules.PolyDeploy.DataAccess.Models;
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
        [HttpPost]
        public async Task<HttpResponseMessage> AddPackages(string session)
        {
            if (!SessionController.SessionExists(session))
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
                            SessionController.AddPackage(session, ds, filename);
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
        public HttpResponseMessage Install(string session)
        {
            if (!SessionController.SessionExists(session))
            {
                // Session doesn't exist.
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid session.");
            }

            Dictionary<string, List<InstallJob>> payload = null;

            EventLogController elc = new EventLogController();

            string apiKey = null;

            try
            {
                // Get the users ip address.
                string ipAddress = HttpContext.Current.Request.UserHostAddress;

                // Get the api key from the header.
                apiKey = Request.Headers.GetValues("x-api-key").FirstOrDefault();

                // Get the session path.
                string sessionPath = SessionController.PathForSession(session);

                // Create a deploy operation.
                CIDeploy deployOperation = new CIDeploy(sessionPath, ipAddress, apiKey);

                // Deploy.
                payload = deployOperation.Deploy();

                int installed = payload.ContainsKey("Installed") ? payload["Installed"].Count : -1;
                int failed = payload.ContainsKey("Failed") ? payload["Failed"].Count : -1;
                int total = -1;

                if (installed >= 0 && failed >= 0)
                {
                    total = installed + failed;
                }

                string log = string.Format("(APIKey: {0}) Installed {1}/{2} at {3}", apiKey, installed, total, DateTime.Now.ToString());

                elc.AddLog("PolyDeploy", log, EventLogController.EventLogType.HOST_ALERT);
            }
            catch (Exception ex)
            {
                string log = string.Format("(APIKey: {0}) Install failure: {1}", apiKey, ex.Message);

                elc.AddLog("PolyDeploy", log, EventLogController.EventLogType.HOST_ALERT);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, payload);
        }
    }
}
