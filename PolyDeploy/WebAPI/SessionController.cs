using Cantarus.Modules.PolyDeploy.Components;
using DotNetNuke.Web.Api;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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

        // TODO: Will use DNN SF to secure.
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
    }
}
