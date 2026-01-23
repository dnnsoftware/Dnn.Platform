// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.HttpModules
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;

    using Dnn.EditBar.UI.Controllers;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Cryptography;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.UI.Skins.EventListeners;
    using DotNetNuke.Web.Client.ResourceManager;

    using Microsoft.Extensions.DependencyInjection;

    using ICryptographyProvider = DotNetNuke.Abstractions.Security.ICryptographyProvider;

    public class EditBarModule : IHttpModule
    {
        private static readonly object LockAppStarted = new object();
        private static bool hasAppStarted;

        private readonly IHostSettings hostSettings;
        private readonly IClientResourceController clientResourceController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;
        private readonly IPortalController portalController;
        private readonly IUserController userController;
        private readonly IHostSettingsService hostSettingsService;

        /// <summary>Initializes a new instance of the <see cref="EditBarModule"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public EditBarModule()
            : this(null, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EditBarModule"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public EditBarModule(IHostSettings hostSettings)
            : this(hostSettings, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EditBarModule"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        public EditBarModule(IHostSettings hostSettings, IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger, IPortalController portalController, IUserController userController, IHostSettingsService hostSettingsService)
        {
            if (hostSettings is not null)
            {
                this.hostSettings = hostSettings;
                this.clientResourceController = clientResourceController;
                this.appStatus = appStatus;
                this.eventLogger = eventLogger;
                this.portalController = portalController;
                this.userController = userController;
                this.hostSettingsService = hostSettingsService;
            }
            else
            {
                var scope = HttpContextSource.Current?.GetScope();
                if (scope is not null)
                {
                    this.hostSettings = scope.ServiceProvider.GetRequiredService<IHostSettings>();
                    this.clientResourceController = scope.ServiceProvider.GetRequiredService<IClientResourceController>();
                    this.appStatus = scope.ServiceProvider.GetRequiredService<IApplicationStatusInfo>();
                    this.eventLogger = scope.ServiceProvider.GetRequiredService<IEventLogger>();
                    this.portalController = scope.ServiceProvider.GetRequiredService<IPortalController>();
                    this.userController = scope.ServiceProvider.GetRequiredService<IUserController>();
                    this.hostSettingsService = scope.ServiceProvider.GetRequiredService<IHostSettingsService>();
                }
                else
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    this.eventLogger = new EventLogController();
#pragma warning restore CS0618 // Type or member is obsolete
                    this.hostSettings = new HostSettings(new HostController(this.eventLogger, new Lazy<IPortalController>(() => PortalController.Instance)));
                    this.clientResourceController = new ClientResourceController(this.hostSettings);
                    this.appStatus = new ApplicationStatusInfo(new Application());
#pragma warning disable CS0618 // Type or member is obsolete
                    this.portalController = new PortalController(new BusinessControllerProvider(null), this.hostSettings, this.appStatus, this.eventLogger, CryptographyProvider.Instance() as ICryptographyProvider, new PermissionController(this.eventLogger));
#pragma warning restore CS0618 // Type or member is obsolete
                    this.userController = new UserController();
                    this.hostSettingsService = new HostController(this.eventLogger, new Lazy<IPortalController>(() => this.portalController));
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void Init(HttpApplication application)
        {
            if (hasAppStarted)
            {
                return;
            }

            lock (LockAppStarted)
            {
                if (hasAppStarted)
                {
                    return;
                }

                this.ApplicationStart();
                hasAppStarted = true;
            }
        }

        public void Dispose()
        {
        }

        private void ApplicationStart()
        {
            DotNetNukeContext.Current.SkinEventListeners.Add(new SkinEventListener(SkinEventType.OnSkinInit, this.OnSkinInit));
        }

        private void OnSkinInit(object sender, SkinEventArgs e)
        {
            if (this.hostSettings.DisableEditBar)
            {
                return;
            }

            var request = e.Skin.Page.Request;
            var isSpecialPageMode = request.QueryString["dnnprintmode"] == "true" || request.QueryString["popUp"] == "true";
            if (isSpecialPageMode || Globals.IsAdminControl())
            {
                return;
            }

            if (ContentEditorManager.GetCurrent(e.Skin.Page) == null && !Globals.IsAdminControl())
            {
                if (PortalSettings.Current.UserId > 0)
                {
                    e.Skin.Page.Form.Controls.Add(new ContentEditorManager(this.clientResourceController, this.appStatus, this.eventLogger, this.portalController, this.hostSettings, this.userController, this.hostSettingsService) { Skin = e.Skin, });
                }
            }
        }
    }
}
