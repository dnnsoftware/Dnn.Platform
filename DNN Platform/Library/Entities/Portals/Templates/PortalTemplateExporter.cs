// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Templates
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;

    internal class PortalTemplateExporter
    {
        private static string LocalResourcesFile => Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.Sites/App_LocalResources/Sites.resx");

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal (bool Success, string Message) ExportPortalTemplate(IBusinessControllerProvider businessControllerProvider, int portalId, string fileName, string description, bool isMultiLanguage, IEnumerable<string> locales, string localizationCulture, IEnumerable<int> exportTabIds, bool includeContent, bool includeFiles, bool includeModules, bool includeProfile, bool includeRoles)
        {
            if (!exportTabIds.Any())
            {
                return (false, Localization.GetString("ErrorPages", LocalResourcesFile));
            }

            var xmlSettings = new XmlWriterSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                OmitXmlDeclaration = true,
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8,
                WriteEndDocumentOnClose = true,
            };

            var filename = Globals.HostMapPath + fileName.Replace("/", @"\");
            if (!filename.EndsWith(".template", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".template";
            }

            using (var writer = XmlWriter.Create(filename, xmlSettings))
            {
                writer.WriteStartElement("portal");
                writer.WriteAttributeString("version", "5.0");

                // Add template description
                writer.WriteElementString("description", HttpUtility.HtmlEncode(description));

                // Serialize portal settings
                var portal = PortalController.Instance.GetPortal(portalId);

                SerializePortalSettings(writer, portal, isMultiLanguage);
                SerializeEnabledLocales(writer, portal, isMultiLanguage, locales);
                SerializeExtensionUrlProviders(writer, portalId);

                if (includeProfile)
                {
                    // Serialize Profile Definitions
                    SerializeProfileDefinitions(writer, portal);
                }

                if (includeModules)
                {
                    // Serialize Portal Desktop Modules
                    DesktopModuleController.SerializePortalDesktopModules(writer, portalId);
                }

                if (includeRoles)
                {
                    // Serialize Roles
                    RoleController.SerializeRoleGroups(writer, portalId);
                }

                // Serialize tabs
                SerializeTabs(businessControllerProvider, writer, portal, isMultiLanguage, exportTabIds, includeContent, locales, localizationCulture);

                if (includeFiles)
                {
                    // Create Zip File to hold files
                    var resourcesFile = new ZipArchive(File.Create(filename + ".resources"), ZipArchiveMode.Create, true);

                    // Serialize folders (while adding files to zip file)
                    SerializeFolders(writer, portal, ref resourcesFile);

                    // Finish and Close Zip file
                    resourcesFile.Dispose();
                }

                writer.WriteEndElement();
                writer.Close();
            }

            EventManager.Instance.OnPortalTemplateCreated(new PortalTemplateEventArgs()
            {
                PortalId = portalId,
                TemplatePath = filename,
            });

            return (true, string.Format(Localization.GetString("ExportedMessage", LocalResourcesFile), filename));
        }

        private static void SerializePortalSettings(XmlWriter writer, PortalInfo portal, bool isMultilanguage)
        {
            writer.WriteStartElement("settings");

            writer.WriteElementString("logofile", portal.LogoFile);
            writer.WriteElementString("footertext", portal.FooterText);
            writer.WriteElementString("userregistration", portal.UserRegistration.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("banneradvertising", portal.BannerAdvertising.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("defaultlanguage", portal.DefaultLanguage);

            var settingsDictionary = PortalController.Instance.GetPortalSettings(portal.PortalID);

            string setting;
            settingsDictionary.TryGetValue("DefaultPortalSkin", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("skinsrc", setting);
            }

            settingsDictionary.TryGetValue("DefaultAdminSkin", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("skinsrcadmin", setting);
            }

            settingsDictionary.TryGetValue("DefaultPortalContainer", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("containersrc", setting);
            }

            settingsDictionary.TryGetValue("DefaultAdminContainer", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("containersrcadmin", setting);
            }

            settingsDictionary.TryGetValue("EnableSkinWidgets", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("enableskinwidgets", setting);
            }

            settingsDictionary.TryGetValue("portalaliasmapping", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("portalaliasmapping", setting);
            }

            writer.WriteElementString("contentlocalizationenabled", isMultilanguage.ToString());

            settingsDictionary.TryGetValue("TimeZone", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("timezone", setting);
            }

            settingsDictionary.TryGetValue("EnablePopUps", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("enablepopups", setting);
            }

            settingsDictionary.TryGetValue("InlineEditorEnabled", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("inlineeditorenabled", setting);
            }

            settingsDictionary.TryGetValue("HideFoldersEnabled", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("hidefoldersenabled", setting);
            }

            settingsDictionary.TryGetValue("ControlPanelMode", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("controlpanelmode", setting);
            }

            settingsDictionary.TryGetValue("ControlPanelSecurity", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("controlpanelsecurity", setting);
            }

            settingsDictionary.TryGetValue("ControlPanelVisibility", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("controlpanelvisibility", setting);
            }

            writer.WriteElementString("hostspace", portal.HostSpace.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("userquota", portal.UserQuota.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("pagequota", portal.PageQuota.ToString(CultureInfo.InvariantCulture));

            settingsDictionary.TryGetValue("PageHeadText", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("pageheadtext", setting);
            }

            settingsDictionary.TryGetValue("InjectModuleHyperLink", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("injectmodulehyperlink", setting);
            }

            settingsDictionary.TryGetValue("AddCompatibleHttpHeader", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("addcompatiblehttpheader", setting);
            }

            settingsDictionary.TryGetValue("ShowCookieConsent", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("showcookieconsent", setting);
            }

            settingsDictionary.TryGetValue("CookieMoreLink", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("cookiemorelink", setting);
            }

            settingsDictionary.TryGetValue("DataConsentActive", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("dataconsentactive", setting);
            }

            settingsDictionary.TryGetValue("DataConsentTermsLastChange", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("dataconsenttermslastchange", setting);
            }

            settingsDictionary.TryGetValue("DataConsentConsentRedirect", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("dataconsentconsentredirect", setting);
            }

            settingsDictionary.TryGetValue("DataConsentUserDeleteAction", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("dataconsentuserdeleteaction", setting);
            }

            settingsDictionary.TryGetValue("DataConsentDelay", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("dataconsentdelay", setting);
            }

            settingsDictionary.TryGetValue("DataConsentDelayMeasurement", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("dataconsentdelaymeasurement", setting);
            }

            settingsDictionary.TryGetValue("ShowQuickModuleAddMenu", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("showquickmoduleaddmenu", setting);
            }

            // End Portal Settings
            writer.WriteEndElement();
        }

        private static void SerializeEnabledLocales(XmlWriter writer, PortalInfo portal, bool isMultilanguage, IEnumerable<string> locales)
        {
            var enabledLocales = LocaleController.Instance.GetLocales(portal.PortalID);
            if (enabledLocales.Count > 1)
            {
                writer.WriteStartElement("locales");
                if (isMultilanguage)
                {
                    foreach (var cultureCode in locales)
                    {
                        writer.WriteElementString("locale", cultureCode);
                    }
                }
                else
                {
                    foreach (var enabledLocale in enabledLocales)
                    {
                        writer.WriteElementString("locale", enabledLocale.Value.Code);
                    }
                }

                writer.WriteEndElement();
            }
        }

        private static void SerializeExtensionUrlProviders(XmlWriter writer, int portalId)
        {
            var providers = ExtensionUrlProviderController.GetModuleProviders(portalId);
            if (!providers.Any())
            {
                return;
            }

            writer.WriteStartElement("extensionUrlProviders");

            foreach (var provider in providers)
            {
                writer.WriteStartElement("extensionUrlProvider");
                writer.WriteElementString("name", provider.ProviderConfig.ProviderName);
                writer.WriteElementString("active", provider.ProviderConfig.IsActive.ToString());
                var settings = provider.ProviderConfig.Settings;
                if (settings.Any())
                {
                    writer.WriteStartElement("settings");
                    foreach (var setting in settings)
                    {
                        writer.WriteStartElement("setting");
                        writer.WriteAttributeString("name", setting.Key);
                        writer.WriteAttributeString("value", setting.Value);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }

                ////if (provider.ProviderConfig.TabIds.Any())
                ////{
                ////    writer.WriteStartElement("tabIds");
                ////    foreach (var tabId in provider.ProviderConfig.TabIds)
                ////    {
                ////        writer.WriteElementString("tabId", tabId.ToString(CultureInfo.InvariantCulture));
                ////    }
                ////    writer.WriteEndElement();
                ////}

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private static void SerializeFolders(XmlWriter writer, PortalInfo objportal, ref ZipArchive zipFile)
        {
            // Sync db and filesystem before exporting so all required files are found
            var folderManager = FolderManager.Instance;
            folderManager.Synchronize(objportal.PortalID);
            writer.WriteStartElement("folders");

            foreach (var folder in folderManager.GetFolders(objportal.PortalID))
            {
                writer.WriteStartElement("folder");

                writer.WriteElementString("folderpath", folder.FolderPath);
                writer.WriteElementString("storagelocation", folder.StorageLocation.ToString());

                // Serialize Folder Permissions
                SerializeFolderPermissions(writer, objportal, folder.FolderPath);

                // Serialize files
                SerializeFiles(writer, objportal, folder.FolderPath, ref zipFile);

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private static void SerializeFiles(XmlWriter writer, IPortalInfo portal, string folderPath, ref ZipArchive zipFile)
        {
            var folderManager = FolderManager.Instance;
            var objFolder = folderManager.GetFolder(portal.PortalId, folderPath);

            writer.WriteStartElement("files");
            foreach (var fileInfo in folderManager.GetFiles(objFolder))
            {
                var objFile = (Services.FileSystem.FileInfo)fileInfo;

                // verify that the file exists on the file system
                var filePath = portal.HomeDirectoryMapPath + folderPath + GetActualFileName(objFile);
                if (File.Exists(filePath))
                {
                    writer.WriteStartElement("file");

                    writer.WriteElementString("contenttype", objFile.ContentType);
                    writer.WriteElementString("extension", objFile.Extension);
                    writer.WriteElementString("filename", objFile.FileName);
                    writer.WriteElementString("height", objFile.Height.ToString(CultureInfo.InvariantCulture));
                    writer.WriteElementString("size", objFile.Size.ToString(CultureInfo.InvariantCulture));
                    writer.WriteElementString("width", objFile.Width.ToString(CultureInfo.InvariantCulture));

                    writer.WriteEndElement();

                    FileSystemUtils.AddToZip(
                        zipFile: ref zipFile,
                        filePath: filePath,
                        fileName: GetActualFileName(objFile),
                        folder: folderPath);
                }
            }

            writer.WriteEndElement();
        }

        private static string GetActualFileName(Services.FileSystem.FileInfo objFile)
        {
            return (objFile.StorageLocation == (int)FolderController.StorageLocationTypes.SecureFileSystem)
                ? objFile.FileName + Globals.glbProtectedExtension
                : objFile.FileName;
        }

        private static void SerializeFolderPermissions(XmlWriter writer, PortalInfo objportal, string folderPath)
        {
            var permissions = FolderPermissionController.GetFolderPermissionsCollectionByFolder(objportal.PortalID, folderPath);

            writer.WriteStartElement("folderpermissions");

            foreach (FolderPermissionInfo permission in permissions)
            {
                writer.WriteStartElement("permission");

                writer.WriteElementString("permissioncode", permission.PermissionCode);
                writer.WriteElementString("permissionkey", permission.PermissionKey);
                writer.WriteElementString("rolename", permission.RoleName);
                writer.WriteElementString("allowaccess", permission.AllowAccess.ToString().ToLowerInvariant());

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private static void SerializeProfileDefinitions(XmlWriter writer, PortalInfo objportal)
        {
            var objListController = new ListController();

            writer.WriteStartElement("profiledefinitions");
            foreach (ProfilePropertyDefinition objProfileProperty in
                ProfileController.GetPropertyDefinitionsByPortal(objportal.PortalID, false, false))
            {
                writer.WriteStartElement("profiledefinition");

                writer.WriteElementString("propertycategory", objProfileProperty.PropertyCategory);
                writer.WriteElementString("propertyname", objProfileProperty.PropertyName);

                var objList = objListController.GetListEntryInfo("DataType", objProfileProperty.DataType);
                writer.WriteElementString("datatype", objList == null ? "Unknown" : objList.Value);
                writer.WriteElementString("length", objProfileProperty.Length.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("defaultvisibility", Convert.ToInt32(objProfileProperty.DefaultVisibility).ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private static void SerializeTabs(IBusinessControllerProvider businessControllerProvider, XmlWriter writer, PortalInfo portal, bool isMultilanguage, IEnumerable<int> tabsToExport, bool includeContent, IEnumerable<string> locales, string localizationCulture = "")
        {
            // supporting object to build the tab hierarchy
            var tabs = new Hashtable();

            writer.WriteStartElement("tabs");

            if (isMultilanguage)
            {
                // Process Default Language first
                SerializeTabs(
                    businessControllerProvider,
                    writer,
                    portal,
                    tabs,
                    GetExportableTabs(TabController.Instance.GetTabsByPortal(portal.PortalID).WithCulture(portal.DefaultLanguage, true)),
                    tabsToExport,
                    includeContent);

                // Process other locales
                foreach (var cultureCode in locales)
                {
                    if (cultureCode != portal.DefaultLanguage)
                    {
                        SerializeTabs(
                            businessControllerProvider,
                            writer,
                            portal,
                            tabs,
                            GetExportableTabs(TabController.Instance.GetTabsByPortal(portal.PortalID).WithCulture(cultureCode, false)),
                            tabsToExport,
                            includeContent);
                    }
                }
            }
            else
            {
                string contentLocalizable;
                if (PortalController.Instance.GetPortalSettings(portal.PortalID)
                    .TryGetValue("ContentLocalizationEnabled", out contentLocalizable) &&
                    Convert.ToBoolean(contentLocalizable))
                {
                    SerializeTabs(
                        businessControllerProvider,
                        writer,
                        portal,
                        tabs,
                        GetExportableTabs(TabController.Instance.GetTabsByPortal(portal.PortalID).WithCulture(localizationCulture, true)),
                        tabsToExport,
                        includeContent);
                }
                else
                {
                    SerializeTabs(
                        businessControllerProvider,
                        writer,
                        portal,
                        tabs,
                        GetExportableTabs(TabController.Instance.GetTabsByPortal(portal.PortalID)),
                        tabsToExport,
                        includeContent);
                }
            }

            writer.WriteEndElement();
        }

        private static void SerializeTabs(IBusinessControllerProvider businessControllerProvider, XmlWriter writer, PortalInfo portal, Hashtable tabs, TabCollection tabCollection, IEnumerable<int> tabsToExport, bool chkContent)
        {
            tabsToExport = tabsToExport.ToList();
            foreach (var tab in tabCollection.Values.OrderBy(x => x.Level))
            {
                // if not deleted
                if (!tab.IsDeleted)
                {
                    XmlNode tabNode = null;
                    if (string.IsNullOrEmpty(tab.CultureCode) || tab.CultureCode == portal.DefaultLanguage)
                    {
                        // page in default culture and checked or page doesn't exist in tree(which should always export).
                        var tabId = tab.TabID;
                        if (tabsToExport.Any(p => p == tabId) ||
                            tabsToExport.All(p => p != tabId))
                        {
                            tabNode = TabController.SerializeTab(businessControllerProvider, new XmlDocument { XmlResolver = null }, tabs, tab, portal, chkContent);
                        }
                    }
                    else
                    {
                        // check if default culture page is selected or default page doesn't exist in tree(which should always export).
                        var defaultTab = tab.DefaultLanguageTab;
                        if (defaultTab == null
                            || tabsToExport.All(p => p != defaultTab.TabID)
                            || tabsToExport.Any(p => p == defaultTab.TabID))
                        {
                            tabNode = TabController.SerializeTab(businessControllerProvider, new XmlDocument { XmlResolver = null }, tabs, tab, portal, chkContent);
                        }
                    }

                    tabNode?.WriteTo(writer);
                }
            }
        }

        private static TabCollection GetExportableTabs(TabCollection tabs)
        {
            var exportableTabs = tabs.Where(kvp => !kvp.Value.IsSystem).Select(kvp => kvp.Value);
            return new TabCollection(exportableTabs);
        }
    }
}
