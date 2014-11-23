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
using System.Web.UI.WebControls;
using System.Xml.XPath;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Upgrade;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Skins.Controls;

using ICSharpCode.SharpZipLib.Zip;
using System.Net;
using System.Xml;
using DotNetNuke.Entities.Host;

#endregion

namespace DotNetNuke.Modules.Admin.Extensions
{
    public partial class AvailableExtensions : ModuleUserControlBase
    {
        private const string DefaultExtensionImage = "icon_extensions_32px.png";
        private const string DefaultAuthenicationImage = "icon_authentication.png";
        private const string DefaultContainerImage = "icon_container.gif";
        private const string DefaultSkinImage = "icon_skin.gif";
        private const string DefaultProviderImage = "icon_provider.gif";

        private IDictionary<string, string> _packageTypes;

        protected IDictionary<string, string> PackageTypesList
        {
            get
            {
                if ((_packageTypes == null))
                {
                    _packageTypes = new Dictionary<string, string>();
                    foreach (PackageType packageType in PackageController.Instance.GetExtensionPackageTypes())
                    {
                        string installPath;
                        string type;
                        switch (packageType.PackageType)
                        {
                            case "Auth_System":
                                type = "AuthSystem";
                                installPath = Globals.ApplicationMapPath + "\\Install\\AuthSystem";
                                break;
                            case "JavaScript_Library":
                                type = "JavaScript_Library";
                                installPath = Globals.ApplicationMapPath + "\\Install\\JavaScriptLibrary";
                                break;
                            case "Module":
                            case "Skin":
                            case "Container":
                            case "Provider":
                                type = packageType.PackageType;
                                installPath = Globals.ApplicationMapPath + "\\Install\\" + packageType.PackageType;
                                break;
                            default:
                                type = String.Empty;
                                installPath = String.Empty;
                                break;
                        }
                        if (!String.IsNullOrEmpty(type) && Directory.Exists(installPath) && 
                            (Directory.GetFiles(installPath, "*.zip").Length > 0 || Directory.GetFiles(installPath, "*.resources").Length > 0))
                        {
                            _packageTypes[type] = installPath;
                        }
                    }
                }
                return _packageTypes;
            }
        }

        private void BindGrid(string installPath, DataGrid grid)
        {
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
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("PackageErrors.Text", LocalResourceFile) + pkgErrorsMsg, ModuleMessage.ModuleMessageType.RedError);
            }
            
            grid.DataSource = packages.Values;
            grid.DataBind();
        }

        private void BindPackageTypes()
        {
            extensionTypeRepeater.DataSource = PackageTypesList;
            extensionTypeRepeater.DataBind();
        }

        private void ProcessDownload()
        {
            // make sure only host users can download the packge.
            if (!ModuleContext.PortalSettings.UserInfo.IsSuperUser)
            {
                return;
            }

            var packageType = Request.QueryString["ptype"];
            var packageName = Request.QueryString["package"];
            if (string.IsNullOrEmpty(packageType) || string.IsNullOrEmpty(packageName))
            {
                return;
            }

            if (!PackageTypesList.ContainsKey(packageType))
            {
                //try to remove the underscore in package type.
                packageType = packageType.Replace("_", "");
                if (!PackageTypesList.ContainsKey(packageType))
                {
                    return;
                }
            }

            var packageFile = new FileInfo(Path.Combine(PackageTypesList[packageType], packageName));
            if (!packageFile.Exists)
            {
                return;
            }

            try
            {
                var fileName = packageName;
                if (fileName.EndsWith(".resources"))
                {
                    fileName = fileName.Replace(".resources", "") + ".zip";
                }
                Response.Clear();
                Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
                Response.AppendHeader("Content-Length", packageFile.Length.ToString());
                Response.ContentType = "application/zip, application/octet-stream";
                Response.WriteFile(packageFile.FullName);
            }
            catch (Exception ex)
            {
                //do nothing here, just ignore the error.
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

        protected string GetAboutTooltip(object dataItem)
        {
            string returnValue = string.Empty;
            try
            {
                if ((ModuleContext.PortalSettings.ActiveTab.IsSuperTab))
                {
                    int portalID = Convert.ToInt32(DataBinder.Eval(dataItem, "PortalID"));
                    if ((portalID != Null.NullInteger && portalID != int.MinValue))
                    {
                        var portal = PortalController.Instance.GetPortal(portalID);
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
                case "Container":
                    return (IconExists(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultContainerImage;
                case "Skin":
                    return (IconExists(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultSkinImage;
                case "AuthenticationSystem":
                case "Auth_System":
                    return (IconExists(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultAuthenicationImage;
                case "Provider":
                    return (IconExists(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultProviderImage;
                default:
                    return (IconExists(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultExtensionImage;
            }
        }

        private bool IconExists(string imagePath)
        {
            if (String.IsNullOrWhiteSpace(imagePath)) return false;

            string path;
            try
            {
                path = Server.MapPath(imagePath);
            }
            catch (HttpException)
            {
                return false;
            }
            return File.Exists(path);
        }

        protected string GetPackageType(object dataItem)
        {
            var kvp = (KeyValuePair<string, string>)dataItem;

            return Localization.GetString(kvp.Key + ".Type", LocalResourceFile);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            languagePacks.ModuleContext.Configuration = ModuleContext.Configuration;
            extensionTypeRepeater.ItemDataBound += extensionTypeRepeater_ItemDataBound;

            if (Request.QueryString["action"] != null
                && Request.QueryString["action"].ToLowerInvariant() == "download")
            {
                ProcessDownload();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            BindPackageTypes();
        }

        private void extensionTypeRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            RepeaterItem item = e.Item;
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var kvp = (KeyValuePair<string, string>)e.Item.DataItem;

                DataGrid extensionsGrid = item.FindControl("extensionsGrid") as DataGrid;
                extensionsGrid.ItemDataBound += extensionsGrid_ItemDataBound;

                Localization.LocalizeDataGrid(ref extensionsGrid, LocalResourceFile);
                BindGrid(kvp.Value, extensionsGrid);
            }
        }

        private void extensionsGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            DataGridItem item = e.Item;
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var package = (PackageInfo) e.Item.DataItem;

                var installLink = (HyperLink)item.Controls[4].Controls[1];
                var downloadLink = (HyperLink)item.Controls[4].Controls[3];

                installLink.NavigateUrl = Util.InstallURL(ModuleContext.TabId, "", package.PackageType, package.FileName);
                if (ModuleContext.PortalSettings.UserInfo.IsSuperUser)
                {
                    downloadLink.NavigateUrl = Globals.NavigateURL(ModuleContext.TabId, "", "action=download",
                        "ptype=" + package.PackageType, "package=" + package.FileName);
                }
                else
                {
                    downloadLink.Visible = false;
                }
            }
        }
    }
}