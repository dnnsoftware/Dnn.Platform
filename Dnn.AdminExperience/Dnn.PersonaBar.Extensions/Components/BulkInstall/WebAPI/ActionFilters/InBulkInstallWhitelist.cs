// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.BulkInstall.WebAPI.ActionFilters
{
    using System;

    using Dnn.PersonaBar.Extensions.Components.BulkInstall;
    using Dnn.PersonaBar.Extensions.Components.BulkInstall.Exceptions;
    using Dnn.PersonaBar.Extensions.Components.BulkInstall.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.DependencyInjection;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Auth.ApiTokens;

    /// <summary>Requires a request to be from an allowed IP address.</summary>
    internal sealed class InBulkInstallWhitelist : AuthorizeAttributeBase
    {
        [Dependency]
        private APIUserManager ApiUserManager { get; set; }

        [Dependency]
        private SettingManager SettingManager { get; set; }

        [Dependency]
        private IPSpecManager IPSpecManager { get; set; }

        [Dependency]
        private EventLogManager EventLogManager { get; set; }

        [Dependency]
        private IApiTokenController ApiTokenController { get; set; }

        /// <inheritdoc />
        public override bool IsAuthorized(AuthFilterContext context)
        {
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

            if (whitelistDisabled)
            {
                return true;
            }

            // Get api user.
            var apiKey = this.ApiTokenController.GetCurrentThreadApiToken();
            var apiUser = apiKey is null ? null : this.ApiUserManager.GetByApiTokenId(apiKey.ApiTokenId);

            // Is the whitelist disabled or does the api user have permission to
            // bypass it?
            if (apiUser is { BypassIPWhitelist: true, })
            {
                // No need to perform whitelisting checks, return early.
                return true;
            }

            string forwardingAddress = null;
            string clientIpAddress = null;

            try
            {
                // There is a strong possibility that this is not the ip address of the machine
                // that sent the request. Being behind a load balancer with transparency switched
                // off or being served through CloudFlare will both affect this value.
                clientIpAddress = HttpContextSource.Current.Request.UserHostAddress;

                // We need to get the X-Forwarded-For header from the request, if this is set we
                // should use it instead of the ip address from the request.
                var forwardedFor = HttpContextSource.Current.Request.Headers.Get("X-Forwarded-For");

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
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                this.EventLogManager.Log("AUTH_EXCEPTION", EventLogSeverity.Info, ex);
            }

            var log = $"Whitelist check failed for IP address: {clientIpAddress}.";

            // Was it forwarded?
            if (forwardingAddress != null)
            {
                log = $"Whitelist check failed for IP address: {clientIpAddress}, forwarded by: {forwardingAddress}.";
            }

            this.EventLogManager.Log("AUTH_BAD_IPADDRESS", EventLogSeverity.Warning, log);

            return false;
        }
    }
}
