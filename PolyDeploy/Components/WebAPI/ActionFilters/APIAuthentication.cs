using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using Cantarus.Modules.PolyDeploy.Components.Logging;
using DotNetNuke.Services.Log.EventLog;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Cantarus.Modules.PolyDeploy.Components.WebAPI.ActionFilters
{
    internal class APIAuthentication : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            bool authenticated = false;
            string message = "Access denied.";

            string apiKey = null;

            try
            {
                // Is there an api key header present?
                if (actionContext.Request.Headers.Contains("x-api-key"))
                {
                    // Get the api key from the header.
                    apiKey = actionContext.Request.Headers.GetValues("x-api-key").FirstOrDefault();

                    // Make sure it's not null and it's 32 characters or we're wasting our time.
                    if (apiKey != null && apiKey.Length == 32)
                    {
                        // Attempt to look up the api user.
                        APIUser apiUser = APIUserManager.GetByAPIKey(apiKey);

                        // Did we find one and double check the api key.
                        if (apiUser != null && apiUser.APIKey == apiKey)
                        {
                            // Genuine API user.
                            authenticated = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Set appropriate message.
                message = "An error occurred while trying to authenticate this request.";

                EventLogManager.Log("AUTH_EXCEPTION", EventLogSeverity.Info, null, ex);
            }

            // If authentication failure occurs, return a response without carrying on executing actions.
            if (!authenticated)
            {
                EventLogManager.Log("AUTH_BAD_APIKEY", EventLogSeverity.Warning, string.Format("Authentication failed for API key: {0}.", apiKey));

                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, message);
            }
        }
    }
}
