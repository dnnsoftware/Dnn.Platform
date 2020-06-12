
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information


using System;
using System.Web.UI;
using Microsoft.Extensions.DependencyInjection;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using Globals = DotNetNuke.Common.Globals;
using DotNetNuke.Abstractions;


// ReSharper disable CheckNamespace
namespace DotNetNuke.Modules.Admin.Modules
// ReSharper restore CheckNamespace
{

    /// <summary>
    /// The ModuleSettingsPage PortalModuleBase is used to edit the settings for a
    /// module.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class ModulePermissions : PortalModuleBase
    {
        private readonly INavigationManager _navigationManager;
        public ModulePermissions()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        private int _moduleId = -1;
        private ModuleInfo _module;

        private ModuleInfo Module
        {
            get { return this._module ?? (this._module = ModuleController.Instance.GetModule(this._moduleId, this.TabId, false)); }
        }

        private string ReturnURL
        {
            get
            {
                return UrlUtils.ValidReturnUrl(this.Request.Params["ReturnURL"]) ?? this._navigationManager.NavigateURL();
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // get ModuleId
            if (this.Request.QueryString["ModuleId"] != null)
            {
                this._moduleId = int.Parse(this.Request.QueryString["ModuleId"]);
            }

            // Verify that the current user has access to edit this module
            if (!ModulePermissionController.HasModuleAccess(SecurityAccessLevel.ViewPermissions, string.Empty, this.Module))
            {
                this.Response.Redirect(Globals.AccessDeniedURL(), true);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdUpdate.Click += this.OnUpdateClick;

            try
            {
                this.cancelHyperLink.NavigateUrl = this.ReturnURL;

                if (this.Page.IsPostBack == false)
                {
                    this.dgPermissions.TabId = this.PortalSettings.ActiveTab.TabID;
                    this.dgPermissions.ModuleID = this._moduleId;

                    if (this.Module != null)
                    {
                        this.cmdUpdate.Visible = ModulePermissionController.HasModulePermission(this.Module.ModulePermissions, "EDIT,MANAGE") || TabPermissionController.CanAddContentToPage();
                        this.permissionsRow.Visible = ModulePermissionController.CanAdminModule(this.Module) || TabPermissionController.CanAddContentToPage();
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnUpdateClick(object sender, EventArgs e)
        {
            try
            {
                if (this.Page.IsValid)
                {
                    this.Module.ModulePermissions.Clear();
                    this.Module.ModulePermissions.AddRange(this.dgPermissions.Permissions);

                    ModulePermissionController.SaveModulePermissions(this.Module);

                    // Navigate back to admin page
                    this.Response.Redirect(this.ReturnURL, true);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
