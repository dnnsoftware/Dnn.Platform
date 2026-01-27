// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Providers.Caching.SimpleWebFarmCachingProvider
{
    using System;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Cryptography;
    using DotNetNuke.Services.Log.EventLog;

    using ICryptographyProvider = DotNetNuke.Abstractions.Security.ICryptographyProvider;

    /// <summary>
    ///     This synchronization handler receives requests from other servers and passes them to the cache system for
    ///     processing.  Error handling is purposefully allowed to bubble up from here to ensure the caller is notified.
    /// </summary>
    public class SimpleWebFarmSynchronizationHandler : IHttpHandler
    {
        private readonly IHostSettings hostSettings;
        private readonly ICryptographyProvider cryptographyProvider;

        /// <summary>Initializes a new instance of the <see cref="SimpleWebFarmSynchronizationHandler"/> class.</summary>
        public SimpleWebFarmSynchronizationHandler()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SimpleWebFarmSynchronizationHandler"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        public SimpleWebFarmSynchronizationHandler(IHostSettings hostSettings, ICryptographyProvider cryptographyProvider)
        {
            this.hostSettings = hostSettings ??
                                new HostSettings(
                                    new HostController(
#pragma warning disable CS0618 // Type or member is obsolete
                                        new EventLogController(),
#pragma warning restore CS0618 // Type or member is obsolete
                                        new Lazy<IPortalController>(() => PortalController.Instance)));
#pragma warning disable CS0618 // Type or member is obsolete
            this.cryptographyProvider = cryptographyProvider ?? CryptographyProvider.Instance() as ICryptographyProvider;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>Gets a value indicating whether indicates that this handler can be reused for multiple requests.</summary>
        public bool IsReusable => true;

        /// <inheritdoc />
        public void ProcessRequest(HttpContext context)
        {
            // Validate the request for required inputs, return if no action possible
            if (string.IsNullOrWhiteSpace(context.Request.QueryString["command"]))
            {
                return; // No command we cannot process
            }

            if (string.IsNullOrWhiteSpace(context.Request.QueryString["detail"]))
            {
                return; // No action we cannot return
            }

            // Only continue if our provider is current
            if (CachingProvider.Instance() is not SimpleWebFarmCachingProvider provider)
            {
                return;
            }

            // Get the values, noting that if in debug we are not encrypted
            var command = this.hostSettings.DebugMode
                ? context.Request.QueryString["command"]
                : UrlUtils.DecryptParameter(this.cryptographyProvider, context.Request.QueryString["command"], this.hostSettings.Guid);

            var detail = this.hostSettings.DebugMode
                ? context.Request.QueryString["detail"]
                : UrlUtils.DecryptParameter(this.cryptographyProvider, context.Request.QueryString["detail"], this.hostSettings.Guid);

            // Pass the action on, if the current caching provider is ours
            provider.ProcessSynchronizationRequest(command, detail);
        }
    }
}
