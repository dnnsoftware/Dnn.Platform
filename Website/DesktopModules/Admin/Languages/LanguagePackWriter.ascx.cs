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
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.XPath;

using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Installer.Writers;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Modules.Admin.Languages
{
    public partial class LanguagePackWriter : PortalModuleBase
    {
        private Dictionary<string, InstallFile> _Files;
        private bool _IsPackCreated = true;
        private string _Manifest = Null.NullString;
        private Services.Installer.Writers.LanguagePackWriter packageWriter;

        protected string BasePath
        {
            get
            {
                return Server.MapPath("~/Install/Language");
            }
        }

        private void CreateAuthSystemPackage(PackageInfo authPackage, bool createZip)
        {
            var Package = new PackageInfo();
            Package.Name = authPackage.Name;
            Package.FriendlyName = authPackage.Name;
            Package.Version = authPackage.Version;
            Package.License = Util.PACKAGE_NoLicense;

            string fileName = Path.Combine(BasePath, "ResourcePack." + Package.Name);

            AuthenticationInfo authSystem = AuthenticationController.GetAuthenticationServiceByPackageID(authPackage.PackageID);
            string authPath = authSystem.LoginControlSrc.Substring(0, authSystem.LoginControlSrc.LastIndexOf("/"));
            CreatePackage(Package, authPackage.PackageID, authPath.Replace("/", "\\"), fileName, createZip);
        }

        private void CreateCorePackage(bool createZip)
        {
            var Package = new PackageInfo();
            Package.Name = Globals.CleanFileName(txtFileName.Text);
            Package.Version = DotNetNukeContext.Current.Application.Version;
            Package.License = Util.PACKAGE_NoLicense;

            string fileName = Path.Combine(BasePath, "ResourcePack." + Package.Name);

            CreatePackage(Package, -2, "", fileName, createZip);
        }

        private void CreateFullPackage()
        {
            Locale language = LocaleController.Instance.GetLocale(cboLanguage.SelectedValue);
            var Package = new PackageInfo();
            Package.Name = Globals.CleanFileName(txtFileName.Text);
            Package.Version = DotNetNukeContext.Current.Application.Version;
            Package.License = Util.PACKAGE_NoLicense;
            Package.PackageType = "CoreLanguagePack";

            _Files = new Dictionary<string, InstallFile>();
            CreateCorePackage(false);
            foreach (DesktopModuleInfo desktopModule in DesktopModuleController.GetDesktopModules(Null.NullInteger).Values)
            {
                if (!desktopModule.FolderName.StartsWith("Admin/"))
                {
                    CreateModulePackage(desktopModule, false);
                }
            }
            foreach (PackageInfo provider in PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType =="Provider"))
            {
                CreateProviderPackage(provider, false);
            }
            foreach (PackageInfo authSystem in PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType =="Auth_System"))
            {
                CreateAuthSystemPackage(authSystem, false);
            }
            string fileName = Path.Combine(BasePath, "ResourcePack." + Package.Name);
            fileName = fileName + "." + Package.Version.ToString(3) + "." + language.Code + ".zip";

            packageWriter = PackageWriterFactory.GetWriter(Package) as Services.Installer.Writers.LanguagePackWriter;
            packageWriter.Language = language;
            packageWriter.BasePath = "";
            foreach (KeyValuePair<string, InstallFile> kvp in _Files)
            {
                packageWriter.Files.Add(kvp.Key, kvp.Value);
            }
            packageWriter.CreatePackage(fileName, Package.Name + " " + language.Text + ".dnn", _Manifest, true);
        }

        private void CreateModulePackage(DesktopModuleInfo desktopModule, bool createZip)
        {
            PackageInfo modulePackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == desktopModule.PackageID);

            var Package = new PackageInfo();
            Package.Name = modulePackage.Name;
            Package.FriendlyName = modulePackage.Name;
            Package.Version = modulePackage.Version;
            Package.License = Util.PACKAGE_NoLicense;

            string fileName = Path.Combine(BasePath, "ResourcePack." + Package.Name);

            CreatePackage(Package, modulePackage.PackageID, Path.Combine("DesktopModules\\", desktopModule.FolderName), fileName, createZip);
        }

        private void CreatePackage(PackageInfo package, int dependentPackageID, string basePath, string fileName, bool createZip)
        {
            string manifest;

            Locale language = LocaleController.Instance.GetLocale(cboLanguage.SelectedValue);
            var languagePack = new LanguagePackInfo();
            languagePack.LanguageID = language.LanguageId;
            languagePack.DependentPackageID = dependentPackageID;

            if (dependentPackageID == -2)
            {
                package.PackageType = "CoreLanguagePack";
            }
            else
            {
                package.PackageType = "ExtensionLanguagePack";
            }
            package.Name += " " + language.Text;
            package.FriendlyName += " " + language.Text;

            packageWriter = PackageWriterFactory.GetWriter(package) as Services.Installer.Writers.LanguagePackWriter;
            packageWriter.Language = language;
            packageWriter.LanguagePack = languagePack;
            packageWriter.BasePath = basePath;
            packageWriter.GetFiles(false);

            if (packageWriter.Files.Count > 0)
            {
                _IsPackCreated = true;
                if (createZip)
                {
                    manifest = packageWriter.WriteManifest(true);
                    fileName = fileName + "." + package.Version.ToString(3) + "." + language.Code + ".zip";
                    packageWriter.CreatePackage(fileName, package.Name + ".dnn", manifest, true);
                }
                else
                {
                    packageWriter.BasePath = "";
                    _Manifest += packageWriter.WriteManifest(true);
                    foreach (KeyValuePair<string, InstallFile> kvp in packageWriter.Files)
                    {
                        _Files[kvp.Key] = kvp.Value;
                    }
                }
            }
            else
            {
                _IsPackCreated = false;
            }
        }

        private void CreateProviderPackage(PackageInfo providerPackage, bool createZip)
        {
            var Package = new PackageInfo();
            Package.Name = providerPackage.Name;
            Package.FriendlyName = providerPackage.Name;
            Package.Version = providerPackage.Version;
            Package.License = Util.PACKAGE_NoLicense;

            string fileName = Path.Combine(BasePath, "ResourcePack." + Package.Name);

            //Get the provider "path"
            XmlDocument configDoc = Config.Load();
            string providerName = Package.Name;
            if (providerName.IndexOf(".") > Null.NullInteger)
            {
                providerName = providerName.Substring(providerName.IndexOf(".") + 1);
            }
            switch (providerName)
            {
                case "SchedulingProvider":
                    providerName = "DNNScheduler";
                    break;
                case "SearchIndexProvider":
                    providerName = "ModuleIndexProvider";
                    break;
                case "SearchProvider":
                    providerName = "SearchDataStoreProvider";
                    break;
            }
            XPathNavigator providerNavigator = configDoc.CreateNavigator().SelectSingleNode("/configuration/dotnetnuke/*/providers/add[@name='" + providerName + "']");
            if(providerNavigator == null)
            {
                providerNavigator = configDoc.CreateNavigator().SelectSingleNode("/configuration/dotnetnuke/*/providers/add[@name='" + Package.Name + "']");
            }

            if (providerNavigator != null)
            {
                string providerPath = providerNavigator.GetAttribute("providerPath", "");
                CreatePackage(Package, providerPackage.PackageID, providerPath.Substring(2, providerPath.Length - 2).Replace("/", "\\"), fileName, createZip);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            rbPackType.SelectedIndexChanged += rbPackType_SelectedIndexChanged;
            cmdCancel.Click += cmdCancel_Click;
            cmdCreate.Click += cmdCreate_Click;

            if (!Page.IsPostBack)
            {
                foreach (Locale language in LocaleController.Instance.GetLocales(Null.NullInteger).Values)
                {
                    cboLanguage.AddItem(language.Text, language.Code);
                }
                rowitems.Visible = false;
            }
        }

        private void rbPackType_SelectedIndexChanged(Object sender, EventArgs e)
        {
            pnlLogs.Visible = false;
            switch (rbPackType.SelectedValue)
            {
                case "Core":
                    rowitems.Visible = false;
                    txtFileName.Text = "Core";
                    lblFilenameFix.Text = Server.HtmlEncode(".<version>.<locale>.zip");
                    rowFileName.Visible = true;
                    break;
                case "Module":
                    rowitems.Visible = true;
                    lstItems.Items.Clear();
                    lstItems.ClearSelection();
                    foreach (DesktopModuleInfo objDM in DesktopModuleController.GetDesktopModules(Null.NullInteger).Values)
                    {
                        if (!objDM.FolderName.StartsWith("Admin/"))
                        {
                            if (Null.IsNull(objDM.Version))
                            {
                                lstItems.Items.Add(new ListItem(objDM.FriendlyName, objDM.DesktopModuleID.ToString()));
                            }
                            else
                            {
                                lstItems.Items.Add(new ListItem(objDM.FriendlyName + " [" + objDM.Version + "]", objDM.DesktopModuleID.ToString()));
                            }
                        }
                    }

                    lblItems.Text = Localization.GetString("SelectModules", LocalResourceFile);
                    rowFileName.Visible = false;
                    break;
                case "Provider":
                    rowitems.Visible = true;
                    lstItems.Items.Clear();
                    lstItems.ClearSelection();
                    foreach (PackageInfo objPackage in PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "Provider"))
                    {
                        if (Null.IsNull(objPackage.Version))
                        {
                            lstItems.Items.Add(new ListItem(objPackage.FriendlyName, objPackage.PackageID.ToString()));
                        }
                        else
                        {
                            lstItems.Items.Add(new ListItem(objPackage.FriendlyName + " [" + Globals.FormatVersion(objPackage.Version) + "]", objPackage.PackageID.ToString()));
                        }
                    }

                    rowFileName.Visible = false;
                    break;
                case "AuthSystem":
                    rowitems.Visible = true;
                    lstItems.Items.Clear();
                    lstItems.ClearSelection();
                    foreach (PackageInfo objPackage in PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType =="Auth_System"))
                    {
                        if (Null.IsNull(objPackage.Version))
                        {
                            lstItems.Items.Add(new ListItem(objPackage.FriendlyName, objPackage.PackageID.ToString()));
                        }
                        else
                        {
                            lstItems.Items.Add(new ListItem(objPackage.FriendlyName + " [" + Globals.FormatVersion(objPackage.Version) + "]", objPackage.PackageID.ToString()));
                        }
                    }

                    rowFileName.Visible = false;
                    break;
                case "Full":
                    rowitems.Visible = false;
                    txtFileName.Text = "Full";
                    lblFilenameFix.Text = Server.HtmlEncode(".<version>.<locale>.zip");
                    rowFileName.Visible = true;
                    break;
            }
        }

        private void cmdCreate_Click(Object sender, EventArgs e)
        {
            _IsPackCreated = true;
            try
            {
                switch (rbPackType.SelectedValue)
                {
                    case "Core":
                        CreateCorePackage(true);
                        break;
                    case "Module":
                        foreach (ListItem moduleItem in lstItems.Items)
                        {
                            if (moduleItem.Selected)
                            {
								//Get the Module
                                DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModule(int.Parse(moduleItem.Value), Null.NullInteger);
                                CreateModulePackage(desktopModule, true);
                            }
                        }

                        break;
                    case "Provider":
                        foreach (ListItem providerItem in lstItems.Items)
                        {
                            if (providerItem.Selected)
                            {
								//Get the Provider
                                PackageInfo provider = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == int.Parse(providerItem.Value));
                                CreateProviderPackage(provider, true);
                            }
                        }

                        break;
                    case "AuthSystem":
                        foreach (ListItem authItem in lstItems.Items)
                        {
                            if (authItem.Selected)
                            {
								//Get the AuthSystem
                                PackageInfo authSystem = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID ==int.Parse(authItem.Value));
                                CreateAuthSystemPackage(authSystem, true);
                            }
                        }

                        break;
                    case "Full":
                        CreateFullPackage();
                        break;
                }
                if (_IsPackCreated)
                {
                    UI.Skins.Skin.AddModuleMessage(this, String.Format(Localization.GetString("Success", LocalResourceFile), PortalSettings.PortalAlias.HTTPAlias), ModuleMessage.ModuleMessageType.GreenSuccess);
                }
                else
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("Failure", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        private void cmdCancel_Click(Object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(Globals.NavigateURL());
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}