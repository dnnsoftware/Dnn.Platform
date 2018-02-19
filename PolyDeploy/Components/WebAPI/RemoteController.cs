using Cantarus.Libraries.Encryption;
using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using Cantarus.Modules.PolyDeploy.Components.Logging;
using Cantarus.Modules.PolyDeploy.Components.WebAPI.ActionFilters;
using DotNetNuke.Web.Api;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Cantarus.Modules.PolyDeploy.Components.WebAPI
{
    [AllowAnonymous]
    [InWhitelist]
    [APIAuthentication]
    public class RemoteController : DnnApiController
    {
        [HttpGet]
        public HttpResponseMessage CreateSession()
        {
            Session session = SessionManager.CreateSession();

            return Request.CreateResponse(HttpStatusCode.OK, session);
        }

        [HttpGet]
        public HttpResponseMessage GetSession(string sessionGuid)
        {
            Session session = SessionManager.GetSession(sessionGuid);

            return Request.CreateResponse(HttpStatusCode.OK, session);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> AddPackages(string sessionGuid)
        {
            if (!SessionManager.SessionExists(sessionGuid))
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
                APIUser apiUser = APIUserManager.GetByAPIKey(apiKey);

                // Receive files.
                MultipartMemoryStreamProvider provider = await Request.Content.ReadAsMultipartAsync();

                foreach (HttpContent file in provider.Contents)
                {
                    string filename = file.Headers.ContentDisposition.FileName.Replace("\"", "");

                    using (MemoryStream ms = new MemoryStream(await file.ReadAsByteArrayAsync()))
                    {
                        using (Stream ds = Crypto.Decrypt(ms, apiUser.EncryptionKey))
                        {
                            SessionManager.AddPackage(sessionGuid, ds, filename);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogManager.Log("REMOTE_EXCEPTION", EventLogSeverity.Warning, null, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpGet]
        public HttpResponseMessage Install(string sessionGuid)
        {
            if (!SessionManager.SessionExists(sessionGuid))
            {
                // Session doesn't exist.
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid session.");
            }

            string apiKey = null;

            try
            {
                // Get the users ip address.
                string ipAddress = HttpContext.Current.Request.UserHostAddress;

                // Get the api key from the header.
                apiKey = Request.Headers.GetValues("x-api-key").FirstOrDefault();

                // Get the session.
                Session sessionObj = SessionManager.GetSession(sessionGuid);

                // Create a deploy operation.
                RemoteDeployment deployOperation = new RemoteDeployment(sessionObj, ipAddress, apiKey);

                // Deploy.
                deployOperation.Deploy();
            }
            catch (Exception ex)
            {
                EventLogManager.Log("REMOTE_EXCEPTION", EventLogSeverity.Warning, null, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Operation started.");
        }
    }
}
