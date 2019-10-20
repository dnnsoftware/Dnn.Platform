#region Copyright
//
// DotNetNuke® - https://www.dnnsoftware.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

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

#endregion

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
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }

        #region Private Members

        private int _moduleId = -1;
        private ModuleInfo _module;

        private ModuleInfo Module
        {
            get { return _module ?? (_module = ModuleController.Instance.GetModule(_moduleId, TabId, false)); }
        }

        private string ReturnURL
        {
            get
            {
                return UrlUtils.ValidReturnUrl(Request.Params["ReturnURL"]) ?? _navigationManager.NavigateURL();
            }
        }

        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //get ModuleId
            if ((Request.QueryString["ModuleId"] != null))
            {
                _moduleId = Int32.Parse(Request.QueryString["ModuleId"]);
            }

            //Verify that the current user has access to edit this module
            if (!ModulePermissionController.HasModuleAccess(SecurityAccessLevel.ViewPermissions, String.Empty, Module))
            {
                Response.Redirect(Globals.AccessDeniedURL(), true);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdUpdate.Click += OnUpdateClick;

            try
            {
                cancelHyperLink.NavigateUrl = ReturnURL;

                if (Page.IsPostBack == false)
                {
                    dgPermissions.TabId = PortalSettings.ActiveTab.TabID;
                    dgPermissions.ModuleID = _moduleId;

                    if (Module != null)
                    {
                        cmdUpdate.Visible = ModulePermissionController.HasModulePermission(Module.ModulePermissions, "EDIT,MANAGE") || TabPermissionController.CanAddContentToPage();
                        permissionsRow.Visible = ModulePermissionController.CanAdminModule(Module) || TabPermissionController.CanAddContentToPage();
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
                if (Page.IsValid)
                {
                    Module.ModulePermissions.Clear();
                    Module.ModulePermissions.AddRange(dgPermissions.Permissions);

                    ModulePermissionController.SaveModulePermissions(Module);

                    //Navigate back to admin page
                    Response.Redirect(ReturnURL, true);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

    }
}
