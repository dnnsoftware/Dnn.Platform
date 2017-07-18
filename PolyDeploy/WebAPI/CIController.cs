using Cantarus.Libraries.Encryption;
using Cantarus.Modules.PolyDeploy.Components;
using Cantarus.Modules.PolyDeploy.DataAccess.Models;
using DotNetNuke.Web.Api;
using System;
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
        [HttpPost]
        public async Task<HttpResponseMessage> Install()
        {
            Dictionary<string, List<InstallJob>> payload = null;

            try
            {
                // Does the request contain multipart/form-data?
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                // Get the users ip address.
                string ipAddress = HttpContext.Current.Request.UserHostAddress;

                // Get the api key from the header.
                string apiKey = Request.Headers.GetValues("x-api-key").FirstOrDefault();

                // Create a deploy operation.
                CIDeploy deployOperation = new CIDeploy(ipAddress, apiKey);

                // Receive files.
                MultipartMemoryStreamProvider provider = await Request.Content.ReadAsMultipartAsync();

                foreach (HttpContent file in provider.Contents)
                {
                    string filename = file.Headers.ContentDisposition.FileName.Replace("\"", "");

                    using (MemoryStream ms = new MemoryStream(await file.ReadAsByteArrayAsync()))
                    {
                        deployOperation.DecryptAndAddZip(ms, filename);
                    }
                }

                // Deploy.
                payload = deployOperation.Deploy();
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, payload);
        }
    }
}
