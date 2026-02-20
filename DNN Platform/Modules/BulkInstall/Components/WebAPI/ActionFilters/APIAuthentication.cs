// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.WebAPI.ActionFilters
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    using Dnn.Modules.BulkInstall.Components.DataAccess.Models;
    using Dnn.Modules.BulkInstall.Components.Logging;

    using DotNetNuke.DependencyInjection;
    using DotNetNuke.Web.Api;

    /// <summary>Requires a value API key for the request.</summary>
    internal sealed class APIAuthentication : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
        [Dependency]
        private APIUserManager ApiUserManager { get; set; }

        [Dependency]
        private EventLogManager EventLogManager { get; set; }

        /// <inheritdoc />
        public override bool IsAuthorized(AuthFilterContext context)
        {
            var authenticated = false;
            string apiKey = null;

            try
            {
                apiKey = context.ActionContext.Request.GetApiKey();

                // Make sure it's not null and it's 32 characters or we're wasting our time.
                if (apiKey is { Length: 32, })
                {
                    // Attempt to look up the api user.
                    APIUser apiUser = this.ApiUserManager.FindAndPrepare(apiKey);

                    // Did we find one and is it ready to use?
                    if (apiUser is { Prepared: true, })
                    {
                        // Genuine API user.
                        authenticated = true;
                    }
                }
            }
            catch (Exception ex)
            {
                this.EventLogManager.Log("AUTH_EXCEPTION", EventLogSeverity.Info, ex);
            }

            // If authentication failure occurs, return a response without carrying on executing actions.
            if (!authenticated)
            {
                this.EventLogManager.Log("AUTH_BAD_APIKEY", EventLogSeverity.Warning, $"Authentication failed for API key: {apiKey}.");
            }

            return authenticated;
        }
    }
}
