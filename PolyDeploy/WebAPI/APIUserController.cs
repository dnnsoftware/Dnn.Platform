using Cantarus.Modules.PolyDeploy.Components;
using Cantarus.Modules.PolyDeploy.DataAccess.Models;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace Cantarus.Modules.PolyDeploy.WebAPI
{
    [AllowAnonymous] // TODO: Secure this controller properly.
    public class APIUserController : DnnApiController
    {
        // TODO: Will use DNN SF to secure.
        [HttpGet]
        public HttpResponseMessage GetAll()
        {
            List<APIUser> apiUsers = APIUserManager.GetAll().ToList();

            foreach(APIUser apiUser in apiUsers)
            {
                apiUser.APIKey = string.Format("****************************{0}", apiUser.APIKey.Substring(apiUser.APIKey.Length - 4));
                apiUser.EncryptionKey = string.Format("****************************{0}", apiUser.EncryptionKey.Substring(apiUser.EncryptionKey.Length - 4));
            }

            return Request.CreateResponse(HttpStatusCode.Created, apiUsers);
        }

        // TODO: Will use DNN SF to secure.
        [HttpPost]
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

        // TODO: Will use DNN SF to secure.
        [HttpPut]
        public HttpResponseMessage Update()
        {
            JavaScriptSerializer jsonSer = new JavaScriptSerializer();

            string json = Request.Content.ReadAsStringAsync().Result;

            APIUser apiUser;

            try
            {
                apiUser = jsonSer.Deserialize<APIUser>(json);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Deserialization failure.");
            }

            try
            {
                apiUser = APIUserManager.Update(apiUser);
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to update API user.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, apiUser);
        }

        // TODO: Will use DNN SF to secure.
        [HttpDelete]
        public HttpResponseMessage Delete(int id)
        {
            JavaScriptSerializer jsonSer = new JavaScriptSerializer();

            string json = Request.Content.ReadAsStringAsync().Result;

            APIUser apiUser = APIUserManager.GetById(id);

            if (apiUser == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "API user not found.");
            }

            try
            {
                APIUserManager.Delete(apiUser);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to delete API user.");
            }

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}
