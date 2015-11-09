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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml.XPath;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;

using ICSharpCode.SharpZipLib.Zip;

#endregion

namespace DotNetNuke.Modules.Admin.AdvancedSettings
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class AdvancedSettings : PortalModuleBase
    {
        #region Private Properties

        private const string DefaultExtensionImage = "icon_extensions_32px.png";
        private const string DefaultAuthenicationImage = "icon_authentication.png";
        private const string DefaultContainerImage = "icon_container.gif";
        private const string DefaultSkinImage = "icon_skin.gif";
        private const string DefaultProviderImage = "icon_provider.gif";

        #endregion

        #region Private Members

        private string ReturnUrl
        {
            get
            {
                return UrlUtils.ValidReturnUrl(Request.Params["ReturnUrl"]) ?? Globals.NavigateURL();
            }
        }

        #endregion

        #region Public Properties

        #endregion

        #region Private Methods

        #endregion

        #region Protected Methods

        protected bool IsHost
        {
            get
            {
                return UserInfo.IsSuperUser;
            }            
        }

        #endregion

        #region Event Handlers

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdSkinsUpdate.Click += OnUpdateSkinsClick;
            cmdSmtpUpdate.Click += OnUpdateSmtpClick;                      
            cmdSkinsCancel.Click += OnCancelClick;
            cmdSmtpCancel.Click += OnCancelClick;
            cmdLangCancel.Click += OnCancelClick;
            cmdAuthCancel.Click += OnCancelClick;
            cmdProvidersCancel.Click += OnCancelClick;
            cmdModulesCancel.Click += OnCancelClick;
            authSystemsGrid.ItemDataBound += OnAuthSystemsGridItemDataBound;
            providersGrid.ItemDataBound += OnProvidersGridItemDataBound;
            modulesGrid.ItemDataBound += OnModulesGridItemDataBound;

            languagePacks.ModuleContext.Configuration = ModuleContext.Configuration;

            //Hide Update button in SkinDesigner
            HideUpdateButtonInSkinDesigner();

            //Hide Cancel Button when not originating from Getting Started
            if(!HttpContext.Current.Request.Url.AbsoluteUri.Contains("gettingStarted"))
            {
                cmdSkinsCancel.Visible = false;
                cmdSmtpCancel.Visible = false;
                cmdLangCancel.Visible = false;
                cmdAuthCancel.Visible = false;
                cmdProvidersCancel.Visible = false;
                cmdModulesCancel.Visible = false;
            }

            try
            {
                if (!Page.IsPostBack)
                {
                    Localization.LocalizeDataGrid(ref authSystemsGrid, LocalResourceFile);
                    Localization.LocalizeDataGrid(ref providersGrid, LocalResourceFile);
                    Localization.LocalizeDataGrid(ref modulesGrid, LocalResourceFile);

                    BindGrid("AuthSystem", authSystemsGrid, divNoAuthSystems);
                    BindGrid("Provider", providersGrid, divNoProviders);
                    BindGrid("Module", modulesGrid, divNoModules);

                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void HideUpdateButtonInSkinDesigner()
        {
            this.Attributes.HideUpdateButton = true;
        }

        protected void OnUpdateSmtpClick(object sender, EventArgs e)
        {
            try
            {
                var returnUrl = ReturnUrl;

                if (IsHost)
                {
                    smtpServerSettings.Update(ref returnUrl);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            finally
            {
                DataCache.ClearHostCache(false);
            }
        }

        protected void OnCancelClick(object sender, EventArgs e)
        {
            Response.Redirect(UrlUtils.PopUpUrl("~/GettingStarted.aspx", this, PortalSettings, false, true));            
        }
        
        protected void OnUpdateSkinsClick(object sender, EventArgs e)
        {
            try
            {
                //Update skin changes
                Attributes.Update();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            finally
            {
                DataCache.ClearHostCache(false);
            }
        }
        #endregion

        #region Packages table

        private void OnAuthSystemsGridItemDataBound(object sender, DataGridItemEventArgs e)
        {
            DataGridItem item = e.Item;
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var package = (PackageInfo)e.Item.DataItem;
                var installLink = (HyperLink)item.Controls[4].Controls[1];
                if (HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains("popup"))
                {
                    installLink.NavigateUrl = Util.InstallURL(ModuleContext.TabId, Globals.NavigateURL(PortalSettings.HomeTabId), package.PackageType, package.FileName);                
                    
                }
                else
                {
                    installLink.NavigateUrl = Util.InstallURL(ModuleContext.TabId, Globals.NavigateURL(), package.PackageType, package.FileName);                    
                }
            }
        }

        private void OnModulesGridItemDataBound(object sender, DataGridItemEventArgs e)
        {
            DataGridItem item = e.Item;
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var package = (PackageInfo)e.Item.DataItem;
                var installLink = (HyperLink)item.Controls[4].Controls[1];
                if (HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains("popup"))
                {
                    installLink.NavigateUrl = Util.InstallURL(ModuleContext.TabId, Globals.NavigateURL(PortalSettings.HomeTabId), package.PackageType, package.FileName);                                    
                }
                else
                {
                    installLink.NavigateUrl = Util.InstallURL(ModuleContext.TabId, Globals.NavigateURL(), package.PackageType, package.FileName);                                    
                }
            }
        }

        private void OnProvidersGridItemDataBound(object sender, DataGridItemEventArgs e)
        {
            DataGridItem item = e.Item;
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var package = (PackageInfo)e.Item.DataItem;
                var installLink = (HyperLink)item.Controls[4].Controls[1];
                if (HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains("popup"))
                {
                    installLink.NavigateUrl = Util.InstallURL(ModuleContext.TabId, Globals.NavigateURL(PortalSettings.HomeTabId), package.PackageType, package.FileName);                                    
                }
                else
                {
                    installLink.NavigateUrl = Util.InstallURL(ModuleContext.TabId, Globals.NavigateURL(), package.PackageType, package.FileName);                                    
                }
            }
        }

        private void BindGrid(string type, DataGrid grid, HtmlGenericControl noItemsControl)
        {
            var installPath = Globals.ApplicationMapPath + "\\Install\\" + type;
            var packages = new Dictionary<string, PackageInfo>();
            var invalidPackages = new List<string>();

            foreach (string file in Directory.GetFiles(installPath))
            {
                if (file.ToLower().EndsWith(".zip") || file.ToLower().EndsWith(".resources"))
                {
                    PackageController.ParsePackage(file, installPath, packages, invalidPackages);
                }
            }

            if (invalidPackages.Count > 0)
            {
                var pkgErrorsMsg = invalidPackages.Aggregate(string.Empty, (current, pkg) => current + (pkg + "<br />"));
                Skin.AddModuleMessage(this, Localization.GetString("PackageErrors.Text", LocalResourceFile) + pkgErrorsMsg, ModuleMessage.ModuleMessageType.RedError);
            }

            if (packages.Count == 0)
            {
                noItemsControl.Visible = true;
                grid.Visible = false;    
            }
            else
            {
                noItemsControl.Visible = false;
                grid.DataSource = packages.Values;
                grid.DataBind();
            }            
        }

        protected string FormatVersion(object version)
        {
            var package = version as PackageInfo;
            string retValue = Null.NullString;
            if (package != null)
            {
                retValue = package.Version.ToString(3);
            }
            return retValue;
        }

        protected string GetPackageDescription(object dataItem)
        {
            var package = dataItem as PackageInfo;
            string retValue = Null.NullString;
            if (package != null)
            {
                retValue = package.Description;
            }
            return retValue;
        }

        protected string GetPackageIcon(object dataItem)
        {
            var package = dataItem as PackageInfo;
            switch (package.PackageType)
            {
                case "Module":
                    return (!String.IsNullOrEmpty(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultExtensionImage;
                case "Container":
                    return (!String.IsNullOrEmpty(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultContainerImage;
                case "Skin":
                    return (!String.IsNullOrEmpty(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultSkinImage;
                case "AuthenticationSystem":
                case "Auth_System":
                    return (!String.IsNullOrEmpty(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultAuthenicationImage;
                case "Provider":
                    return (!String.IsNullOrEmpty(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultProviderImage;
                default:
                    return (!String.IsNullOrEmpty(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultExtensionImage;
            }
        }
        
        protected string GetIconUrl(string icon)
        {
            return Globals.ImagePath + icon;
        }
      
        #endregion
    }
}