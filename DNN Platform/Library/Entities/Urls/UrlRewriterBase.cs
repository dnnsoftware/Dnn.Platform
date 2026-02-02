// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Abstract class to allow derived classes of different implementations of Url Rewriter.</summary>
    public abstract class UrlRewriterBase
    {
        /// <summary>Initializes a new instance of the <see cref="UrlRewriterBase"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        protected UrlRewriterBase()
            : this(null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="UrlRewriterBase"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        protected UrlRewriterBase(IHostSettings hostSettings, IPortalAliasService portalAliasService, IHostSettingsService hostSettingsService, IPortalController portalController)
        {
            this.HostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.PortalAliasService = portalAliasService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>();
            this.HostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
            this.PortalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        }

        /// <summary>Gets the host settings.</summary>
        protected IHostSettings HostSettings { get; }

        /// <summary>Gets the portal alias service.</summary>
        protected IPortalAliasService PortalAliasService { get; }

        /// <summary>Gets the host settings service.</summary>
        protected IHostSettingsService HostSettingsService { get; }

        /// <summary>Gets the portal controller.</summary>
        protected IPortalController PortalController { get; }

        internal abstract void RewriteUrl(object sender, EventArgs e);

        protected static void AutoAddAlias(IHostSettings hostSettings, IPortalAliasService portalAliasService, HttpContextBase context)
        {
            var portalId = hostSettings.HostPortalId;

            // the domain name was not found so try using the host portal's first alias
            if (portalId > Null.NullInteger)
            {
                IPortalAliasInfo portalAliasInfo = new PortalAliasInfo();
                portalAliasInfo.HttpAlias = Globals.GetDomainName(context.Request, true);
                portalAliasInfo.PortalId = portalId;
                portalAliasService.AddPortalAlias(portalAliasInfo);

                context.Response.Redirect(context.Request.Url.ToString(), true);
            }
        }

        protected static void AutoAddAlias(HttpContext context)
        {
            var serviceProvider = context.GetScope().ServiceProvider;
            AutoAddAlias(serviceProvider.GetRequiredService<IHostSettings>(), serviceProvider.GetRequiredService<IPortalAliasService>(), new HttpContextWrapper(context));
        }

        protected static bool CanAutoAddPortalAlias(IHostSettingsService hostSettingsService, IPortalController portalController)
        {
            bool autoAddPortalAlias = hostSettingsService.GetBoolean("AutoAddPortalAlias");
            return autoAddPortalAlias && (portalController.GetPortals().Count == 1);
        }

        protected static bool CanAutoAddPortalAlias()
        {
            var serviceProvider = Globals.GetCurrentServiceProvider();
            return CanAutoAddPortalAlias(
                serviceProvider.GetRequiredService<IHostSettingsService>(),
                serviceProvider.GetRequiredService<IPortalController>());
        }

        protected void AutoAddAlias(HttpContextBase context) => AutoAddAlias(this.HostSettings, this.PortalAliasService, context);

        protected bool AutoAddPortalAliasEnabled() => CanAutoAddPortalAlias(this.HostSettingsService, this.PortalController);
    }
}
