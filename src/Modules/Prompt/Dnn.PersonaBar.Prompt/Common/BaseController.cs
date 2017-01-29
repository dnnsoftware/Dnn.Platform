using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Prompt.Models;
using System.Net;
using System.Net.Http;
using System.Web;

namespace Dnn.PersonaBar.Prompt.Common
{
    public abstract class BaseController : PersonaBarApiController
    {
        protected HttpResponseMessage OKResponse(string msg, object data = null)
        {
            return Request.CreateResponse(HttpStatusCode.OK, new ResponseModel(false, msg, data == null ? "" : data.ToString()));
        }

        protected HttpResponseMessage UnauthorizedResponse(string msg = "", object data = null)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = "You are not authorized to access to this resource. Your session may have timed-out. If so login again.";
            }
            else
            {
                msg += " Your session may have timed-out. If so login again.";
            }
            return Request.CreateResponse(HttpStatusCode.Unauthorized, new ResponseModel(true, msg, data == null ? "" : data.ToString()));
        }

        protected HttpResponseMessage BadRequestResponse(string msg = "", object data = null)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = "You've submitted invalid data. Your request cannot be processed.";
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, new ResponseModel(true, msg, data == null ? "" : data.ToString()));
        }

        protected HttpResponseMessage ServerErrorResponse(string msg = "", object data = null)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = "The server has encoutered an issue and was unable to process your request. Please try again later.";
            }
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new ResponseModel(true, msg, data == null ? "" : data.ToString()));
        }

        protected HttpResponseMessage NotImplementedrResponse(string msg = "", object data = null)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = "This functionality has not yet been implemented.";
            }
            return Request.CreateResponse(HttpStatusCode.NotImplemented, new ResponseModel(true, msg, data == null ? "" : data.ToString()));
        }

        /// <summary>
        /// To use these methods, set: Backbone.emulateHTTP = true; in the App initialization.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        protected bool IsDeleteMethod()
        {
            HttpRequest req = HttpContext.Current.Request;
            string xheader = req.Headers["X-HTTP-Method-Override"];
            if (!string.IsNullOrEmpty(xheader) && xheader.ToUpper() == "DELETE")
            {
                return true;
            }
            return false;
        }
        protected bool IsUpdateMethod()
        {
            HttpRequest req = HttpContext.Current.Request;
            string xheader = req.Headers["X-HTTP-Method-Override"];
            if (!string.IsNullOrEmpty(xheader) && xheader.ToUpper() == "PUT")
            {
                return true;
            }
            return false;
        }

    }
}