using Cantarus.Modules.PolyDeploy.Components;
using Cantarus.Modules.PolyDeploy.DataAccess.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Cantarus.Modules.PolyDeploy.WebAPI
{
    public class InWhitelist : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            bool authenticated = false;
            string message = "Access denied.";

            try
            {
                string clientIpAddress = HttpContext.Current.Request.UserHostAddress;

                // Got the ip address?
                if (!string.IsNullOrEmpty(clientIpAddress))
                {
                    // Is it whitelisted or localhost?
                    if (IPSpecController.IsWhitelisted(clientIpAddress) || clientIpAddress.Equals("127.0.0.1"))
                    {
                        authenticated = true;
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
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, message);
            }
        }
    }
}
