using Cantarus.Modules.PolyDeploy.Components;
using DotNetNuke.Services.Log.EventLog;
using System;
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

            string forwardingAddress = null;
            string clientIpAddress = null;

            try
            {
                // There is a strong possibility that this is not the ip address of the machine
                // that sent the request. Being behind a load balancer with transparancy switched
                // off or being served through CloudFlare will both affect this value.
                clientIpAddress = HttpContext.Current.Request.UserHostAddress;

                // We need to get the X-Forwarded-For header from the request, if this is set we
                // should use it instead of the ip address from the request.
                string forwardedFor = HttpContext.Current.Request.Headers.Get("X-Forwarded-For");

                // Forwarded for set?
                if (forwardedFor != null)
                {
                    forwardingAddress = clientIpAddress;
                    clientIpAddress = forwardedFor;
                }

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
                EventLogController elc = new EventLogController();

                string log = string.Format("(IP: {0}) {1}", clientIpAddress, message);

                // Was it forwarded?
                if (forwardingAddress != null)
                {
                    log = string.Format("(IP: {0} | Forwarded by: {1}) {2}", clientIpAddress, forwardingAddress, message);
                }

                elc.AddLog("PolyDeploy", log, EventLogController.EventLogType.HOST_ALERT);

                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, message);
            }
        }
    }
}
