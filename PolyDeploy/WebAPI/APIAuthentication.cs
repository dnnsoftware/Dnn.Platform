using Cantarus.Modules.PolyDeploy.Components;
using Cantarus.Modules.PolyDeploy.DataAccess.Models;
using DotNetNuke.Services.Log.EventLog;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Cantarus.Modules.PolyDeploy.WebAPI
{
    public class APIAuthentication : ActionFilterAttribute
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
                        APIUser apiUser = APIUserController.GetByAPIKey(apiKey);

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

                // TODO: Add some logging of what happened here.
            }

            // If authentication failure occurs, return a response without carrying on executing actions.
            if (!authenticated)
            {
                EventLogController elc = new EventLogController();

                string log = string.Format("(APIKey: {0}) {1}", apiKey, message);

                elc.AddLog("PolyDeploy", log, EventLogController.EventLogType.HOST_ALERT);

                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, message);
            }
        }
    }
}
