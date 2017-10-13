using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using Cantarus.Modules.PolyDeploy.Components.WebAPI.ActionFilters;
using DotNetNuke.Web.Api;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Cantarus.Modules.PolyDeploy.Components.WebAPI
{
    [RequireHost]
    [ValidateAntiForgeryToken]
    [InWhitelist]
    public class SessionController : DnnApiController
    {
        [HttpPost]
        public HttpResponseMessage Create()
        {
            var session = SessionManager.CreateSession();

            return Request.CreateResponse(HttpStatusCode.OK, session);
        }

        [HttpGet]
        public HttpResponseMessage Get(string guid)
        {
            var session = SessionManager.GetSession(guid);

            return Request.CreateResponse(HttpStatusCode.OK, session);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> AddPackage(string guid)
        {
            if (!SessionManager.SessionExists(guid))
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

                // Receive files.
                MultipartMemoryStreamProvider provider = await Request.Content.ReadAsMultipartAsync();

                // TODO: Add filtering so that non .zip archives are not added.

                foreach (HttpContent file in provider.Contents)
                {
                    string filename = file.Headers.ContentDisposition.FileName.Replace("\"", "");

                    using (MemoryStream ms = new MemoryStream(await file.ReadAsByteArrayAsync()))
                    {
                        SessionManager.AddPackage(guid, ms, filename);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpGet]
        public HttpResponseMessage Summary(string guid)
        {
            if (!SessionManager.SessionExists(guid))
            {
                // Session doesn't exist.
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid session.");
            }

            try
            {
                // Get the users ip address.
                string ipAddress = HttpContext.Current.Request.UserHostAddress;

                // Get the session.
                Session sessionObj = SessionManager.GetSession(guid);

                // Create a deploy operation.
                Deployment deployOperation = new Deployment(sessionObj, ipAddress);

                var summary = deployOperation.Summary();

                return Request.CreateResponse(HttpStatusCode.OK, summary);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage Install(string guid)
        {
            if (!SessionManager.SessionExists(guid))
            {
                // Session doesn't exist.
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid session.");
            }

            try
            {
                // Get the users ip address.
                string ipAddress = HttpContext.Current.Request.UserHostAddress;

                // Get the session.
                Session sessionObj = SessionManager.GetSession(guid);

                // Create a deploy operation.
                Deployment deployOperation = new Deployment(sessionObj, ipAddress);

                deployOperation.Deploy();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}
