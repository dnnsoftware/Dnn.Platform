#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.Modules.Admin.Extensions
{

    /// <summary>
    /// The Extensions Module Control is used to manage the Extensions
    /// installed in this portal
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	01/04/2008	Created
    /// </history>
    public partial class Extensions : ModuleUserControlBase
    {
        #region Protected Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            cmdInstall.NavigateUrl = Util.InstallURL(ModuleContext.TabId, "");

            cmdInstall.Visible = ModuleContext.PortalSettings.UserInfo.IsSuperUser;
            createExtensionLink.Visible = ModuleContext.IsHostMenu;
            createModuleLink.Visible = ModuleContext.IsHostMenu;
 
            jQuery.RequestDnnPluginsRegistration();

            installedExtensionsControl.LocalResourceFile = LocalResourceFile;
            installedExtensionsControl.ModuleContext.Configuration = ModuleContext.Configuration;
            installedExtensionsTab.Visible = true;
            installedExtensionsControl.Visible = true;
 
            if (ModuleContext.PortalSettings.ActiveTab.IsSuperTab)
            {
                availableExtensionsControl.LocalResourceFile = LocalResourceFile;
                availableExtensionsControl.ModuleContext.Configuration = ModuleContext.Configuration;
                availableExtensionsTab.Visible = true;
                availableExtensionsControl.Visible = true;
                availableExtensionsTabExpand.Visible = true;
            }

            if (ModuleContext.PortalSettings.ActiveTab.IsSuperTab)
            {
                purchasedExtensionsControl.LocalResourceFile = LocalResourceFile;
                purchasedExtensionsControl.ModuleContext.Configuration = ModuleContext.Configuration;
                purchasedExtensionsTab.Visible = true;
                purchasedExtensionsControl.Visible = true;
            }

            if (ModuleContext.PortalSettings.ActiveTab.IsSuperTab)
            {
                moreExtensionsControl.LocalResourceFile = LocalResourceFile;
                moreExtensionsControl.ModuleContext.Configuration = ModuleContext.Configuration;
                moreExtensionsTab.Visible = true;
                moreExtensionsControl.Visible = true;
                availableExtensionsTabExpand.Visible = true;
            }

        }

        #endregion

    }
}