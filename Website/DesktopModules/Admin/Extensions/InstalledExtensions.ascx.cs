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
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Upgrade;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.WebControls;

#endregion

// ReSharper disable CheckNamespace
namespace DotNetNuke.Modules.Admin.Extensions
// ReSharper restore CheckNamespace
{
    public partial class InstalledExtensions : ModuleUserControlBase, IActionable
    {
        #region Private Members

        private const string DefaultExtensionImage = "icon_extensions_32px.png";
        private const string DefaultLanguageImage = "icon_languagePack.gif";
        private const string DefaultAuthenicationImage = "icon_authentication.png";
        private const string DefaultContainerImage = "icon_container.gif";
        private const string DefaultSkinImage = "icon_skin.gif";
        private const string DefaultProviderImage = "icon_provider.gif";
        private const string DefaultLibraryImage = "icon_library.png";
        private const string DefaultWidgetImage = "icon_widget.png";
        private const string DefaultDashboardImage = "icon_dashboard.png";

        private IDictionary<string, PackageType> _packageTypes;
        private IDictionary<int, PackageInfo> _packagesInUse;

        #endregion

        #region Protected Properties

        protected IDictionary<int, PackageInfo> PackagesInUse
        {
            get
            {
                if ((_packagesInUse == null))
                {
                    _packagesInUse = PackageController.GetModulePackagesInUse(PortalController.GetCurrentPortalSettings().PortalId, ModuleContext.PortalSettings.ActiveTab.IsSuperTab);
                }
                return _packagesInUse;
            }
        }

        protected IDictionary<string, PackageType> PackageTypesList
        {
            get
            {
                if ((_packageTypes == null))
                {
                    _packageTypes = new Dictionary<string, PackageType>();
                    foreach (PackageType packageType in PackageController.Instance.GetExtensionPackageTypes())
                    {
                        _packageTypes[packageType.PackageType] = packageType;
                    }
                }
                return _packageTypes;
            }
        }

        #endregion

        #region Private Methods

        private void AddModulesToList(List<PackageInfo> packages)
        {
            Dictionary<int, PortalDesktopModuleInfo> portalModules = DesktopModuleController.GetPortalDesktopModulesByPortalID(ModuleContext.PortalId);
            packages.AddRange(from modulePackage in PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "Module") 
                              let desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(modulePackage.PackageID) 
                                from portalModule in portalModules.Values 
                                where desktopModule != null && portalModule.DesktopModuleID == desktopModule.DesktopModuleID 
                                select modulePackage);
        }

        private void BindGrid(string packageType, DataGrid grid, Label noResultsLabel)
        {
            var packages = new List<PackageInfo>();

            grid.ItemDataBound += extensionsGrid_ItemDataBound;

            UpdateGridColumns(grid);

			if (String.IsNullOrEmpty(Upgrade.UpgradeIndicator(DotNetNukeContext.Current.Application.Version, "", "~", "", Request.IsLocal, Request.IsSecureConnection)))
			{
				lblUpdateRow.Visible = false;
				grid.Columns[8].HeaderText = "";
			}

			Localization.LocalizeDataGrid(ref grid, LocalResourceFile);

            switch (packageType)
            {
                case "Module":
                    if (!ModuleContext.PortalSettings.ActiveTab.IsSuperTab)
                    {
                        AddModulesToList(packages);
                    }
                    else
                    {
                        packages = PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "Module").ToList();
                    }
                    break;
                case "Skin":
                case "Container":
                    packages = PackageController.Instance.GetExtensionPackages(ModuleContext.PortalSettings.ActiveTab.IsSuperTab ? Null.NullInteger : ModuleContext.PortalId, p => p.PackageType == packageType).ToList();
                    break;
                default:
                    packages = PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == packageType).ToList();
                    break;
            }

            if (packages.Count > 0)
            {
                noResultsLabel.Visible = false;
                grid.Visible = true;
                grid.DataSource = packages;
                grid.DataBind();
            }
            else
            {
                grid.Visible = false;
                noResultsLabel.Visible = true;
            }
        }

        private void BindPackageTypes()
        {
            //force modules to be the first in the list
            extensionTypeRepeater.DataSource = PackageTypesList.OrderBy(p => p.Value.PackageType != "Module");
            extensionTypeRepeater.DataBind();
        }

        private void UpdateGridColumns(DataGrid grid)
        {
            foreach (DataGridColumn column in grid.Columns)
            {
                if (ReferenceEquals(column.GetType(), typeof(ImageCommandColumn)))
                {
                    var imageColumn = (ImageCommandColumn)column;
                    if (!String.IsNullOrEmpty(imageColumn.CommandName))
                    {
                        imageColumn.Text = Localization.GetString(imageColumn.CommandName, LocalResourceFile);
                    }
                    if (imageColumn.CommandName == "Delete")
                    {
                        var parameters = new string[2];
                        parameters[0] = "rtab=" + ModuleContext.TabId;
                        parameters[1] = "packageId=KEYFIELD";
                        var formatString = ModuleContext.NavigateUrl(ModuleContext.TabId, "UnInstall", false, parameters);                        
                        formatString = formatString.Replace("KEYFIELD", "{0}");
                        imageColumn.NavigateURLFormatString = formatString;
                        imageColumn.Visible = UserController.GetCurrentUserInfo().IsSuperUser;
                    }
                    if (imageColumn.CommandName == "Edit")
                    {
                        string formatString = ModuleContext.EditUrl("PackageID", "KEYFIELD", "Edit");
                        formatString = formatString.Replace("KEYFIELD", "{0}");
                        imageColumn.NavigateURLFormatString = formatString;
                    }
                }
            }
        }

        private string UpgradeIndicator(Version version, string packageType, string packageName, string culture)
        {
            string strURL = Upgrade.UpgradeIndicator(version, packageType, packageName, culture, Request.IsLocal, Request.IsSecureConnection);
            if (String.IsNullOrEmpty(strURL))
            {
                strURL = Globals.ApplicationPath + "/images/spacer.gif";
            }
            return strURL;
        }

        private string UpgradeRedirect(Version version, string packageType, string packageName, string culture)
        {
            return Upgrade.UpgradeRedirect(version, packageType, packageName, culture);
        }

        #endregion

        #region Protected Methods

        protected string FormatVersion(object version)
        {
            var package = version as PackageInfo;
            var retValue = Null.NullString;
            if (package != null)
            {
                retValue = package.Version.ToString(3);
            }
            return retValue;
        }

        protected string GetAboutTooltip(object dataItem)
        {
            var returnValue = string.Empty;
            try
            {
                if ((ModuleContext.PortalSettings.ActiveTab.IsSuperTab))
                {
                    var portalID = Convert.ToInt32(DataBinder.Eval(dataItem, "PortalID"));
                    if ((portalID != Null.NullInteger && portalID != int.MinValue))
                    {
                        var controller = new PortalController();
                        PortalInfo portal = controller.GetPortal(portalID);
                        returnValue = string.Format(Localization.GetString("InstalledOnPortal.Tooltip", LocalResourceFile), portal.PortalName);
                    }
                    else
                    {
                        returnValue = Localization.GetString("InstalledOnHost.Tooltip", LocalResourceFile);
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
            return returnValue;
        }

        protected string GetIsPackageInUseInfo(object dataItem)
        {
            var info = dataItem as PackageInfo;
            if (info != null)
            {
                var package = info;
                if ((package.PackageType.ToUpper() == "MODULE"))
                {
                    return PackagesInUse.ContainsKey(package.PackageID) ? "<a href=\"" + ModuleContext.EditUrl("PackageID", package.PackageID.ToString(CultureInfo.InvariantCulture), "UsageDetails") + "\">" + LocalizeString("Yes") + "</a>" : LocalizeString("No");
                }
            }
            return string.Empty;
        }

        protected string GetPackageTypeDescription(string typeKey)
        {
            var returnValue = typeKey;
            if ((PackageTypesList.ContainsKey(typeKey)))
            {
                returnValue = PackageTypesList[typeKey].Description;
            }
            return returnValue;
        }

        protected string GetPackageDescription(object dataItem)
        {
            var package = dataItem as PackageInfo;
            var retValue = Null.NullString;
            if (package != null)
            {
                retValue = package.Description;
            }
            return retValue;
        }

        protected string GetPackageIcon(object dataItem)
        {
            var package = dataItem as PackageInfo;
            if (package != null)
            {
                switch (package.PackageType)
                {
                    case "Module":
                        return (package.IconFile != string.Empty) ? package.IconFile : Globals.ImagePath + DefaultExtensionImage;
                    case "Container":
                        return (package.IconFile != string.Empty) ? package.IconFile : Globals.ImagePath + DefaultContainerImage;
                    case "Skin":
                    case "SkinObject":
                        return (package.IconFile != string.Empty) ? package.IconFile : Globals.ImagePath + DefaultSkinImage;
                    case "AuthenticationSystem":
                    case "Auth_System":
                        return (package.IconFile != string.Empty) ? package.IconFile : Globals.ImagePath + DefaultAuthenicationImage;
                    case "CoreLanguagePack":
                    case "ExtensionLanguagePack":
                        return (package.IconFile != string.Empty) ? package.IconFile : Globals.ImagePath + DefaultLanguageImage;
                    case "Provider":
                        return (package.IconFile != string.Empty) ? package.IconFile : Globals.ImagePath + DefaultProviderImage;
                    case "Widget":
                        return (package.IconFile != string.Empty) ? package.IconFile : Globals.ImagePath + DefaultWidgetImage;
                    case "DashboardControl":
                        return (package.IconFile != string.Empty) ? package.IconFile : Globals.ImagePath + DefaultDashboardImage;
                    case "Library":
                        return (package.IconFile != string.Empty) ? package.IconFile : Globals.ImagePath + DefaultLibraryImage;
                    default:
                        return (package.IconFile != string.Empty) ? package.IconFile : Globals.ImagePath + DefaultExtensionImage;
                }
            }
            return null;
        }

        protected string GetPackageType(object dataItem)
        {
            var kvp = (KeyValuePair<string, PackageType>) dataItem;

            var localizeName = Localization.GetString(kvp.Key + ".Type", LocalResourceFile);

			//catch empty resource key because some module will add itself package type.
			if(string.IsNullOrEmpty(localizeName))
			{
				localizeName = kvp.Key;
			}

        	return localizeName;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            #region Bind Handlers

            cboLocales.SelectedIndexChanged += cboLocales_SelectedIndexChanged;
            extensionTypeRepeater.ItemDataBound += extensionTypeRepeater_ItemDataBound;

            #endregion

            if (!Page.IsPostBack)
            {

                cboLocales.DataSource = LocaleController.Instance.GetLocales(Null.NullInteger).Values;
                cboLocales.DataBind();
                cboLocales.Items.Insert(0, new ListItem(Localization.GetString("Not_Specified", Localization.SharedResourceFile), ""));
            }

            if (ModuleContext.PortalSettings.ActiveTab.IsSuperTab)
            {
                languageSelectorRow.Visible = !Request.IsLocal;
            }
            else
            {
                languageSelectorRow.Visible = false;
            }
            BindPackageTypes();
        }

        protected string UpgradeService(Version version, string packageType, string packageName)
        {
            var strUpgradeService = "";
            strUpgradeService += "<a title=\"" + Localization.GetString("UpgradeMessage", LocalResourceFile) + "\" href=\"" + UpgradeRedirect(version, packageType, packageName, "") +
                                 "\" target=\"_new\"><img title=\"" + Localization.GetString("UpgradeMessage", LocalResourceFile) + "\" src=\"" +
                                 UpgradeIndicator(version, packageType, packageName, "") + "\" border=\"0\" /></a>";
            if (!string.IsNullOrEmpty(cboLocales.SelectedValue))
            {
                strUpgradeService += "<br />";
                strUpgradeService += "<a title=\"" + Localization.GetString("LanguageMessage", LocalResourceFile) + "\" href=\"" +
                                     UpgradeRedirect(version, packageType, packageName, cboLocales.SelectedItem.Value) + "\" target=\"_new\"><img title=\"" +
                                     Localization.GetString("LanguageMessage", LocalResourceFile) + "\" src=\"" + UpgradeIndicator(version, packageType, packageName, cboLocales.SelectedItem.Value) +
                                     "\" border=\"0\" /></a>";
            }
            return strUpgradeService;
        }

        #endregion

        #region Event Handlers

        protected void cboLocales_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindPackageTypes();
        }

        protected void extensionsGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            DataGridItem item = e.Item;
            if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.SelectedItem)
            {
                var editHyperlink = item.Controls[8].Controls[0] as HyperLink;
                if (editHyperlink != null)
                {
                    var package = (PackageInfo) item.DataItem;
                    if (ModuleContext.PortalSettings.ActiveTab.IsSuperTab)
                    {
                        editHyperlink.Visible = !package.IsSystemPackage && PackageController.CanDeletePackage(package, ModuleContext.PortalSettings);
                    }
                    else
                    {
                        editHyperlink.Visible = false;
                    }
                }
                editHyperlink = item.Controls[0].Controls[0] as HyperLink;
                if (editHyperlink != null)
                {
                    editHyperlink.Visible = ModuleContext.IsEditable;
                }
            }
        }

        private void extensionTypeRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            RepeaterItem item = e.Item;
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var kvp = (KeyValuePair<string, PackageType>)e.Item.DataItem;

                var extensionsGrid = item.FindControl("extensionsGrid") as DataGrid;

                var noResultsLabel = item.FindControl("noResultsLabel") as Label;

                BindGrid(kvp.Key, extensionsGrid, noResultsLabel);				
            }
        }

        #endregion

        #region IActionable Members

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var Actions = new ModuleActionCollection();
                if (ModuleContext.IsHostMenu)
                {
                    Actions.Add(ModuleContext.GetNextActionID(),
                                Localization.GetString("ExtensionInstall.Action", LocalResourceFile),
                                ModuleActionType.AddContent,
                                "",
                                "action_import.gif",
                                Util.InstallURL(ModuleContext.TabId, ""),
                                false,
                                SecurityAccessLevel.Host,
                                true,
                                false);
                    Actions.Add(ModuleContext.GetNextActionID(),
                                Localization.GetString("InstallExtensions.Action", LocalResourceFile),
                                ModuleActionType.AddContent,
                                "",
                                "action_import.gif",
                                ModuleContext.EditUrl("BatchInstall"),
                                false,
                                SecurityAccessLevel.Host,
                                true,
                                false);
                    Actions.Add(ModuleContext.GetNextActionID(),
                                Localization.GetString("CreateExtension.Action", LocalResourceFile),
                                ModuleActionType.AddContent,
                                "",
                                "add.gif",
                                ModuleContext.EditUrl("NewExtension"),
                                false,
                                SecurityAccessLevel.Host,
                                true,
                                false);
                    Actions.Add(ModuleContext.GetNextActionID(),
                                Localization.GetString("CreateModule.Action", LocalResourceFile),
                                ModuleActionType.AddContent,
                                "",
                                "add.gif",
                                ModuleContext.EditUrl("EditModuleDefinition"),
                                false,
                                SecurityAccessLevel.Host,
                                true,
                                false);
                    Actions.Add(ModuleContext.GetNextActionID(),
                                Localization.GetString("CreateLanguagePack.Action", LocalResourceFile),
                                ModuleActionType.AddContent,
                                "",
                                "add.gif",
                                ModuleContext.EditUrl("Type", "LanguagePack", "NewExtension"),
                                false,
                                SecurityAccessLevel.Host,
                                true,
                                false);
                    Actions.Add(ModuleContext.GetNextActionID(),
                                Localization.GetString("CreateSkin.Action", LocalResourceFile),
                                ModuleActionType.AddContent,
                                "",
                                "add.gif",
                                ModuleContext.EditUrl("Type", "Skin", "NewExtension"),
                                false,
                                SecurityAccessLevel.Host,
                                true,
                                false);
                    Actions.Add(ModuleContext.GetNextActionID(),
                                Localization.GetString("CreateContainer.Action", LocalResourceFile),
                                ModuleActionType.AddContent,
                                "",
                                "add.gif",
                                ModuleContext.EditUrl("Type", "Container", "NewExtension"),
                                false,
                                SecurityAccessLevel.Host,
                                true,
                                false);
                }
                return Actions;
            }
        }

        #endregion

    }
}