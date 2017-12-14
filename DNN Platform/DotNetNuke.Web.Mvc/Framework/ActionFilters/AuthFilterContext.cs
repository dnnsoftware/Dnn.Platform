
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Mvc;
using DotNetNuke.Entities.Host;

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    public class AuthFilterContext
    {
        public AuthFilterContext(AuthorizationContext filterContext, string authFailureMessage)
        {
            ActionContext = filterContext;
            AuthFailureMessage = authFailureMessage;
        }

        public AuthorizationContext ActionContext { get; private set; }
        public string AuthFailureMessage { get; set; }

        /// <summary>
        /// Processes requests that fail authorization. This default implementation creates a new response with the
        /// Unauthorized status code. Override this method to provide your own handling for unauthorized requests.
        /// </summary>
        public virtual void HandleUnauthorizedRequest()
        {
            ActionContext.Result = new HttpUnauthorizedResult(AuthFailureMessage);
            if (!Host.DebugMode)
            {
                ActionContext.RequestContext.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;
            }
        }
    }
}
