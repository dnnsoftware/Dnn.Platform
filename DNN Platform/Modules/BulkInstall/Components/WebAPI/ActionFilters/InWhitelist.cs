// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.WebAPI.ActionFilters
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    using Dnn.Modules.BulkInstall.Components.DataAccess.Models;
    using Dnn.Modules.BulkInstall.Components.Exceptions;
    using Dnn.Modules.BulkInstall.Components.Logging;

    using DotNetNuke.DependencyInjection;

    /// <summary>Requires a request to be from an allowed IP address.</summary>
    internal sealed class InWhitelist : ActionFilterAttribute
    {
        [Dependency]
        private APIUserManager ApiUserManager { get; set; }

        [Dependency]
        private SettingManager SettingManager { get; set; }

        [Dependency]
        private IPSpecManager IPSpecManager { get; set; }

        [Dependency]
        private EventLogManager EventLogManager { get; set; }

        /// <inheritdoc/>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            // Get whitelist state.
            bool whitelistDisabled;

            try
            {
                // Attempt to retrieve disabled state.
                whitelistDisabled = !this.SettingManager.GetSetting(Settings.IpSafelistGroup, Settings.IpSafelistKey).ValueAsBoolean;
            }
            catch (SettingNotFoundException)
            {
                // Setting not set, default to off.
                whitelistDisabled = true;
            }

            // Get api user.
            string apiKey = actionContext.Request.GetApiKey();
            APIUser apiUser = this.ApiUserManager.GetByAPIKey(apiKey);

            // Is the whitelist disabled or does the api user have permission to
            // bypass it?
            if (whitelistDisabled || apiUser is { BypassIPWhitelist: true, })
            {
                // No need to perform whitelisting checks, return early.
                return;
            }

            bool authenticated = false;
            string message = "Access denied.";

            string forwardingAddress = null;
            string clientIpAddress = null;

            try
            {
                // There is a strong possibility that this is not the ip address of the machine
                // that sent the request. Being behind a load balancer with transparency switched
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
                    if (this.IPSpecManager.IsAllowed(clientIpAddress) || clientIpAddress.Equals("127.0.0.1", StringComparison.Ordinal))
                    {
                        authenticated = true;
                    }
                }
            }
            catch (Exception ex)
            {
                // Set appropriate message.
                message = "An error occurred while trying to authenticate this request.";

                this.EventLogManager.Log("AUTH_EXCEPTION", EventLogSeverity.Info, ex);
            }

            // If authentication failure occurs, return a response without carrying on executing actions.
            if (!authenticated)
            {
                string log = $"Whitelist check failed for IP address: {clientIpAddress}.";

                // Was it forwarded?
                if (forwardingAddress != null)
                {
                    log = $"Whitelist check failed for IP address: {clientIpAddress}, forwarded by: {forwardingAddress}.";
                }

                this.EventLogManager.Log("AUTH_BAD_IPADDRESS", EventLogSeverity.Warning, log);

                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, message);
            }
        }
    }
}
