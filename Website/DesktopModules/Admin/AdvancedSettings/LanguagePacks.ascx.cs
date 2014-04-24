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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.XPath;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Upgrade.Internals;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;

using ICSharpCode.SharpZipLib.Zip;

namespace DotNetNuke.Modules.Admin.AdvancedSettings
{
    public partial class LanguagePacks : PortalModuleBase
    {
        private const string DefaultLanguageImage = "icon_languagePack.gif";
        private const string OwnerUpdateService = "DotNetNuke Update Service";

        private IDictionary<string, string> _packageTypes;

        protected IDictionary<string, string> PackageTypesList
        {
            get
            {
                if ((_packageTypes == null))
                {
                    _packageTypes = new Dictionary<string, string>();
                    string installPath;
                    string type;
                    type = "Language";
                    installPath = Globals.ApplicationMapPath + "\\Install\\Language";

                    _packageTypes[type] = installPath;
                }

                return _packageTypes;
            }
        }

        private void BindGrid(string installPath, DataGrid grid)
        {
            var packages = new List<PackageInfo>();
            var invalidPackages = new List<string>();

            foreach (string file in Directory.GetFiles(installPath))
            {
                if (file.ToLower().EndsWith(".zip") || file.ToLower().EndsWith(".resources"))
                {
                    Stream inputStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                    var unzip = new ZipInputStream(inputStream);

                    try
                    {
                        ZipEntry entry = unzip.GetNextEntry();

                        while (entry != null)
                        {
                            if (!entry.IsDirectory)
                            {
                                string fileName = entry.Name;
                                string extension = Path.GetExtension(fileName);
                                if (extension.ToLower() == ".dnn" || extension.ToLower() == ".dnn5")
                                {
                                    //Manifest
                                    var manifestReader = new StreamReader(unzip);
                                    string manifest = manifestReader.ReadToEnd();

                                    var package = new PackageInfo();
                                    package.Manifest = manifest;
                                    if (!string.IsNullOrEmpty(manifest))
                                    {
                                        var doc = new XPathDocument(new StringReader(manifest));
                                        XPathNavigator rootNav = doc.CreateNavigator().SelectSingleNode("dotnetnuke");
                                        string packageType = String.Empty;
                                        if (rootNav.Name == "dotnetnuke")
                                        {
                                            packageType = XmlUtils.GetAttributeValue(rootNav, "type");
                                        }
                                        else if (rootNav.Name.ToLower() == "languagepack")
                                        {
                                            packageType = "LanguagePack";
                                        }
                                        XPathNavigator nav = null;
                                        switch (packageType.ToLower())
                                        {
                                            case "package":
                                                nav = rootNav.SelectSingleNode("packages/package");
                                                break;

                                            case "languagepack":

                                                nav = Installer.ConvertLegacyNavigator(rootNav, new InstallerInfo()).SelectSingleNode("packages/package");
                                                break;
                                        }

                                        if (nav != null)
                                        {
                                            package.Name = XmlUtils.GetAttributeValue(nav, "name");
                                            package.PackageType = XmlUtils.GetAttributeValue(nav, "type");
                                            package.IsSystemPackage = XmlUtils.GetAttributeValueAsBoolean(nav, "isSystem", false);
                                            package.Version = new Version(XmlUtils.GetAttributeValue(nav, "version"));
                                            package.FriendlyName = XmlUtils.GetNodeValue(nav, "friendlyName");
                                            if (String.IsNullOrEmpty(package.FriendlyName))
                                            {
                                                package.FriendlyName = package.Name;
                                            }
                                            package.Description = XmlUtils.GetNodeValue(nav, "description");
                                            package.FileName = file.Replace(installPath + "\\", "");

                                            packages.Add(package);
                                        }
                                    }

                                    break;
                                }
                            }
                            entry = unzip.GetNextEntry();
                        }
                    }
                    catch (Exception)
                    {
                        invalidPackages.Add(file);
                    }
                    finally
                    {
                        unzip.Close();
                        unzip.Dispose();
                    }
                }
            }

            //now add language packs from update service
            try
            {
                StreamReader myResponseReader = UpdateService.GetLanguageList();
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(myResponseReader);
                XmlNodeList languages = xmlDoc.SelectNodes("available/language");

                if (languages != null)
                {
	                var installedPackages = PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "CoreLanguagePack");
	                var installedLanguages = installedPackages.Select(package => LanguagePackController.GetLanguagePackByPackage(package.PackageID)).ToList();
	                foreach (XmlNode language in languages)
                    {
                        string cultureCode = "";
                        string version = "";
                        foreach (XmlNode child in language.ChildNodes)
                        {
                            if (child.Name == "culturecode")
                            {
                                cultureCode = child.InnerText;
                            }

                            if (child.Name == "version")
                            {
                                version = child.InnerText;
                            }
                        }
	                    if (!string.IsNullOrEmpty(cultureCode) && !string.IsNullOrEmpty(version) && version.Length == 6)
	                    {
		                    var myCIintl = new CultureInfo(cultureCode, true);
		                    version = version.Insert(4, ".").Insert(2, ".");
		                    var package = new PackageInfo {Owner = OwnerUpdateService, Name = "LanguagePack-" + myCIintl.Name, FriendlyName = myCIintl.NativeName};
		                    package.Name = myCIintl.NativeName;
		                    package.Description = cultureCode;
		                    Version ver = null;
		                    Version.TryParse(version, out ver);
		                    package.Version = ver;

							if (
								installedLanguages.Any(
									l =>
									LocaleController.Instance.GetLocale(l.LanguageID).Code.ToLowerInvariant().Equals(cultureCode.ToLowerInvariant()) 
									&& installedPackages.First(p => p.PackageID == l.PackageID).Version >= ver))
							{
								continue;
							}

							if (packages.Any(p => p.Name == package.Name))
							{
								var existPackage = packages.First(p => p.Name == package.Name);
								if (package.Version > existPackage.Version)
								{
									packages.Remove(existPackage);
									packages.Add(package);
								}
							}
							else
							{
								packages.Add(package);
							}
	                    }
                    }
                }
            }
            catch (Exception)
            {
                //suppress for now - need to decide what to do when webservice is unreachable
                //throw;
                //same problem happens in InstallWizard.aspx.cs in BindLanguageList method
            }


            if (invalidPackages.Count > 0)
            {
                string pkgErrorsMsg = invalidPackages.Aggregate(string.Empty, (current, pkg) => current + (pkg + "<br />"));
                Skin.AddModuleMessage(this, Localization.GetString("PackageErrors.Text", LocalResourceFile) + pkgErrorsMsg, ModuleMessage.ModuleMessageType.RedError);
            }

            grid.DataSource = packages;
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

            if (packageType == "LanguagePack")
            {
                packageType = "Language";
            }

            if (!PackageTypesList.ContainsKey(packageType))
            {
                return;
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

            return (!String.IsNullOrEmpty(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultLanguageImage;
        }

        protected string GetPackageType(object dataItem)
        {
            var kvp = (KeyValuePair<string, string>) dataItem;

            return Localization.GetString(kvp.Key + ".Type", LocalResourceFile);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

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
            if (!IsPostBack)
            {
                BindPackageTypes();
            }
        }

        private void extensionTypeRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            RepeaterItem item = e.Item;
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var kvp = (KeyValuePair<string, string>) e.Item.DataItem;

                var extensionsGrid = item.FindControl("extensionsGrid") as DataGrid;
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

                var installLink = (HyperLink) item.Controls[4].Controls[1];
                var deployLink = (LinkButton) item.Controls[4].Controls[3];
                var downloadLink = (LinkButton)item.Controls[4].Controls[5];
                if (package.Owner != OwnerUpdateService)
                {
                    if (HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains("popup"))
                    {
                        installLink.NavigateUrl = Util.InstallURL(ModuleContext.TabId, Globals.NavigateURL(PortalSettings.HomeTabId), package.PackageType, package.FileName);                        
                    }
                    else
                    {
                        installLink.NavigateUrl = Util.InstallURL(ModuleContext.TabId, "", package.PackageType, package.FileName);                        
                    }
                }
                else
                {
                    deployLink.Visible = true;
                    //store culture as a data attribute
                    deployLink.Attributes["data-id"] = package.Description;
                    installLink.Visible = false;
                    downloadLink.Attributes["data-id"] = package.Description;
                }

                downloadLink.Visible = ModuleContext.PortalSettings.UserInfo.IsSuperUser;
            }
        }

        public void DeployLanguage(Object sender, EventArgs e)
        {
            var thisButton = (LinkButton) sender;
            InstallController.Instance.IsAvailableLanguagePack(thisButton.Attributes["data-id"]);

            if (HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains("popup"))
            {
                thisButton.Attributes["popupUrl"] = Util.InstallURL(ModuleContext.TabId, Globals.NavigateURL(PortalSettings.HomeTabId), "CoreLanguagePack", "installlanguage.resources");                
            }
            else
            {
                thisButton.Attributes["popupUrl"] = Util.InstallURL(ModuleContext.TabId, Globals.NavigateURL(), "CoreLanguagePack", "installlanguage.resources");                
            }
        }

        protected void DownloadLanguage(object sender, EventArgs e)
        {
            var thisButton = (LinkButton) sender;
            if (thisButton.Attributes["data-id"] != null)
            {
                InstallController.Instance.IsAvailableLanguagePack(thisButton.Attributes["data-id"]);
            }

            thisButton.Attributes["popupurl"] = Globals.NavigateURL(TabId, string.Empty,
                    "action=download",
                    "ptype=LanguagePack",
                    "package=installlanguage.resources");
        }

        public bool ShowDescription { get; set; }
    }
}