using Dnn.PersonaBar.Library;
using System.Net;
using System.Net.Http;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Prompt.Common
{
    public abstract class ControllerBase : PersonaBarApiController
    {

        protected HttpResponseMessage OkResponse(string msg, object data = null)
        {
            return Request.CreateResponse(HttpStatusCode.OK, new ResponseModel(false, msg, data?.ToString() ?? ""));
        }

        protected HttpResponseMessage UnauthorizedResponse(string msg = "", object data = null)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = Localization.GetString("Prompt_NotAuthorized", Components.Constants.LocalResourcesFile, true);
            }
            else
            {
                msg += " " + Localization.GetString("Prompt_SessionTimedOut", Components.Constants.LocalResourcesFile, true);
            }
            return Request.CreateResponse(HttpStatusCode.Unauthorized, new ResponseModel(true, msg, data?.ToString() ?? ""));
        }

        protected HttpResponseMessage BadRequestResponse(string msg = "", object data = null)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = Localization.GetString("Prompt_InvalidData", Components.Constants.LocalResourcesFile, true);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, new ResponseModel(true, msg, data?.ToString() ?? ""));
        }

        protected HttpResponseMessage ServerErrorResponse(string msg = "", object data = null)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = Localization.GetString("Prompt_ServerError", Components.Constants.LocalResourcesFile, true);
            }
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new ResponseModel(true, msg, data?.ToString() ?? ""));
        }

        protected HttpResponseMessage NotImplementedResponse(string msg = "", object data = null)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = Localization.GetString("Prompt_NotImplemented", Components.Constants.LocalResourcesFile, true);
            }
            return Request.CreateResponse(HttpStatusCode.NotImplemented, new ResponseModel(true, msg, data?.ToString() ?? ""));
        }

    }
}