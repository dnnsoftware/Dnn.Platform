// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Groups
{
    using System;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Exceptions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The ViewSocialGroups class displays the content.</summary>
    public partial class View : GroupsModuleBase
    {
        private readonly INavigationManager navigationManager;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="View"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
        public View()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="View"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        public View(INavigationManager navigationManager, IApplicationStatusInfo appStatus, IEventLogger eventLogger)
        {
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
            this.appStatus = appStatus ?? this.DependencyProvider.GetRequiredService<IApplicationStatusInfo>();
            this.eventLogger = eventLogger ?? this.DependencyProvider.GetRequiredService<IEventLogger>();
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
            this.InitializeComponent();
            base.OnInit(e);
        }

        private void InitializeComponent()
        {
            this.Load += this.Page_Load;
        }

        /// <summary>Page_Load runs when the control is loaded.</summary>
        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, CommonJs.DnnPlugins);
                if (this.GroupId < 0)
                {
                    if (this.TabId != this.GroupListTabId && !this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
                    {
                        this.Response.Redirect(this.navigationManager.NavigateURL(this.GroupListTabId));
                    }
                }

                GroupsModuleBase ctl = (GroupsModuleBase)this.LoadControl(this.ControlPath);
                ctl.ModuleConfiguration = this.ModuleConfiguration;
                this.plhContent.Controls.Clear();
                this.plhContent.Controls.Add(ctl);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
