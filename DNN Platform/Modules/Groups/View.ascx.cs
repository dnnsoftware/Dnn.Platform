#region Usings

using System;
using Microsoft.Extensions.DependencyInjection;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Modules.Groups.Components;
using DotNetNuke.Common;
using DotNetNuke.Framework;
using DotNetNuke.Abstractions;

#endregion

namespace DotNetNuke.Modules.Groups
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewSocialGroups class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class View : GroupsModuleBase
    {
        private readonly INavigationManager _navigationManager;
        public View()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
        }

        private void InitializeComponent()
        {
            Load += Page_Load;
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                JavaScript.RequestRegistration(CommonJs.DnnPlugins);
                if (GroupId < 0) {
                    if (TabId != GroupListTabId && !UserInfo.IsInRole(PortalSettings.AdministratorRoleName)) {
                       Response.Redirect(_navigationManager.NavigateURL(GroupListTabId));
                    }
                }
                GroupsModuleBase ctl = (GroupsModuleBase)LoadControl(ControlPath);
                ctl.ModuleConfiguration = this.ModuleConfiguration;
                plhContent.Controls.Clear();
                plhContent.Controls.Add(ctl);

            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion


    }
}
