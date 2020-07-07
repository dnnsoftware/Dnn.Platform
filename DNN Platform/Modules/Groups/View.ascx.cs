// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Groups
{
    using System;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Modules.Groups.Components;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewSocialGroups class displays the content.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class View : GroupsModuleBase
    {
        private readonly INavigationManager _navigationManager;

        public View()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        protected override void OnInit(EventArgs e)
        {
            this.InitializeComponent();
            base.OnInit(e);
        }

        private void InitializeComponent()
        {
            this.Load += this.Page_Load;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                JavaScript.RequestRegistration(CommonJs.DnnPlugins);
                if (this.GroupId < 0)
                {
                    if (this.TabId != this.GroupListTabId && !this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
                    {
                        this.Response.Redirect(this._navigationManager.NavigateURL(this.GroupListTabId));
                    }
                }

                GroupsModuleBase ctl = (GroupsModuleBase)this.LoadControl(this.ControlPath);
                ctl.ModuleConfiguration = this.ModuleConfiguration;
                this.plhContent.Controls.Clear();
                this.plhContent.Controls.Add(ctl);
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
