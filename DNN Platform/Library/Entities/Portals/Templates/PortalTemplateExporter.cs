using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace DotNetNuke.Entities.Portals.Templates
{
    internal class ExportTemplateRequest
    {
        public string FileName { get; set; }
        public string Description { get; set; }
        public int PortalId { get; set; }
        public IEnumerable<TabDto> Pages { get; set; }
        public IEnumerable<string> Locales { get; set; }
        public string LocalizationCulture { get; set; }
        public bool IsMultilanguage { get; set; }
        public bool IncludeContent { get; set; }
        public bool IncludeFiles { get; set; }
        public bool IncludeRoles { get; set; }
        public bool IncludeProfile { get; set; }
        public bool IncludeModules { get; set; }
    }
    internal class TabDto
    {
        public TabDto()
        {
            this.CheckedState = NodeCheckedState.UnChecked;
        }
        public string Name { get; set; }
        public string TabId { get; set; }
        public int ParentTabId { get; set; }
        public bool HasChildren { get; set; }
        public NodeCheckedState CheckedState { get; set; }
        public IList<TabDto> ChildTabs { get; set; }
    }
    internal enum NodeCheckedState
    {
        Checked = 0,
        UnChecked = 1,
        Partial = 2,
    }

    internal class PortalTemplateExporter
    {
        private string LocalResourcesFile => Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.Sites/App_LocalResources/Sites.resx");

        public string ExportPortalTemplate(ExportTemplateRequest request, out bool success)
        {
            var locales = request.Locales.ToList();
            var pages = request.Pages.ToList();
            var isValid = true;
            success = false;

            // Verify all ancestor pages are selected
            foreach (var page in pages)
            {
                if (page.ParentTabId != Null.NullInteger && pages.All(p => p.TabId != page.ParentTabId.ToString(CultureInfo.InvariantCulture)))
                {
                    isValid = false;
                }
            }

            if (!isValid)
            {
                return Localization.GetString("ErrorAncestorPages", this.LocalResourcesFile);
            }

            if (!pages.Any())
            {
                return Localization.GetString("ErrorPages", this.LocalResourcesFile);
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

            var filename = Globals.HostMapPath + request.FileName.Replace("/", @"\");
            if (!filename.EndsWith(".template", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".template";
            }

            using (var writer = XmlWriter.Create(filename, xmlSettings))
            {
                writer.WriteStartElement("portal");
                writer.WriteAttributeString("version", "5.0");

                // Add template description
                writer.WriteElementString("description", HttpUtility.HtmlEncode(request.Description));

                // Serialize portal settings
                var portal = PortalController.Instance.GetPortal(request.PortalId);

                this.SerializePortalSettings(writer, portal, request.IsMultilanguage);
                this.SerializeEnabledLocales(writer, portal, request.IsMultilanguage, locales);
                this.SerializeExtensionUrlProviders(writer, request.PortalId);

                if (request.IncludeProfile)
                {
                    // Serialize Profile Definitions
                    this.SerializeProfileDefinitions(writer, portal);
                }

                if (request.IncludeModules)
                {
                    // Serialize Portal Desktop Modules
                    DesktopModuleController.SerializePortalDesktopModules(writer, request.PortalId);
                }

                if (request.IncludeRoles)
                {
                    // Serialize Roles
                    RoleController.SerializeRoleGroups(writer, request.PortalId);
                }

                // Serialize tabs
                this.SerializeTabs(writer, portal, request.IsMultilanguage, pages, request.IncludeContent, locales, request.LocalizationCulture);

                if (request.IncludeFiles)
                {
                    // Create Zip File to hold files
                    var resourcesFile = new ZipArchive(File.Create(filename + ".resources"));

                    // Serialize folders (while adding files to zip file)
                    this.SerializeFolders(writer, portal, ref resourcesFile);

                    // Finish and Close Zip file
                    resourcesFile.Dispose();
                }

                writer.WriteEndElement();
                writer.Close();
            }

            EventManager.Instance.OnPortalTemplateCreated(new PortalTemplateEventArgs()
            {
                PortalId = request.PortalId,
                TemplatePath = filename,
            });

            success = true;
            return string.Format(Localization.GetString("ExportedMessage", this.LocalResourcesFile), filename);
        }

        private void SerializePortalSettings(XmlWriter writer, PortalInfo portal, bool isMultilanguage)
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

        private void SerializeEnabledLocales(XmlWriter writer, PortalInfo portal, bool isMultilanguage, IEnumerable<string> locales)
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

        private void SerializeExtensionUrlProviders(XmlWriter writer, int portalId)
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

        private void SerializeFolders(XmlWriter writer, PortalInfo objportal, ref ZipArchive zipFile)
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
                this.SerializeFolderPermissions(writer, objportal, folder.FolderPath);

                // Serialize files
                this.SerializeFiles(writer, objportal, folder.FolderPath, ref zipFile);

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void SerializeFiles(XmlWriter writer, IPortalInfo portal, string folderPath, ref ZipArchive zipFile)
        {
            var folderManager = FolderManager.Instance;
            var objFolder = folderManager.GetFolder(portal.PortalId, folderPath);

            writer.WriteStartElement("files");
            foreach (var fileInfo in folderManager.GetFiles(objFolder))
            {
                var objFile = (Services.FileSystem.FileInfo)fileInfo;

                // verify that the file exists on the file system
                var filePath = portal.HomeDirectoryMapPath + folderPath + this.GetActualFileName(objFile);
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
                        fileName: this.GetActualFileName(objFile),
                        folder: folderPath);
                }
            }

            writer.WriteEndElement();
        }

        private string GetActualFileName(IFileInfo objFile)
        {
            return (objFile.StorageLocation == (int)FolderController.StorageLocationTypes.SecureFileSystem)
                ? objFile.FileName + Globals.glbProtectedExtension
                : objFile.FileName;
        }

        private void SerializeFolderPermissions(XmlWriter writer, PortalInfo objportal, string folderPath)
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

        private void SerializeProfileDefinitions(XmlWriter writer, PortalInfo objportal)
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

        private void SerializeTabs(XmlWriter writer, PortalInfo portal, bool isMultilanguage, IEnumerable<TabDto> pages, bool includeContent, IEnumerable<string> locales, string localizationCulture = "")
        {
            // supporting object to build the tab hierarchy
            var tabs = new Hashtable();

            writer.WriteStartElement("tabs");
            var tabsToExport = this.GetTabsToExport(portal.PortalID, portal.DefaultLanguage, isMultilanguage, pages, null).ToList();

            if (isMultilanguage)
            {
                // Process Default Language first
                this.SerializeTabs(writer, portal, tabs,
                    this.GetExportableTabs(
                        TabController.Instance.GetTabsByPortal(portal.PortalID)
                            .WithCulture(portal.DefaultLanguage, true)), tabsToExport, includeContent);

                // Process other locales
                foreach (var cultureCode in locales)
                {
                    if (cultureCode != portal.DefaultLanguage)
                    {
                        this.SerializeTabs(writer, portal, tabs,
                            this.GetExportableTabs(
                                TabController.Instance.GetTabsByPortal(portal.PortalID).WithCulture(cultureCode, false)),
                            tabsToExport, includeContent);
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
                    this.SerializeTabs(writer, portal, tabs,
                     this.GetExportableTabs(TabController.Instance.GetTabsByPortal(portal.PortalID).WithCulture(localizationCulture, true)), tabsToExport,
                     includeContent);
                }
                else
                {
                    this.SerializeTabs(writer, portal, tabs,
                        this.GetExportableTabs(TabController.Instance.GetTabsByPortal(portal.PortalID)), tabsToExport,
                        includeContent);
                }
            }

            writer.WriteEndElement();
        }

        private void SerializeTabs(XmlWriter writer, PortalInfo portal, Hashtable tabs, TabCollection tabCollection, IEnumerable<TabDto> pages, bool chkContent)
        {
            pages = pages.ToList();
            foreach (var tab in tabCollection.Values.OrderBy(x => x.Level))
            {
                // if not deleted
                if (!tab.IsDeleted)
                {
                    XmlNode tabNode = null;
                    if (string.IsNullOrEmpty(tab.CultureCode) || tab.CultureCode == portal.DefaultLanguage)
                    {
                        // page in default culture and checked or page doesn't exist in tree(which should always export).
                        var tabId = tab.TabID.ToString(CultureInfo.InvariantCulture);
                        if (pages.Any(p => p.TabId == tabId && (p.CheckedState == NodeCheckedState.Checked || p.CheckedState == NodeCheckedState.Partial)) ||
                            pages.All(p => p.TabId != tabId))
                        {
                            tabNode = TabController.SerializeTab(new XmlDocument { XmlResolver = null }, tabs, tab, portal, chkContent);
                        }
                    }
                    else
                    {
                        // check if default culture page is selected or default page doesn't exist in tree(which should always export).
                        var defaultTab = tab.DefaultLanguageTab;
                        if (defaultTab == null
                            || pages.All(p => p.TabId != defaultTab.TabID.ToString(CultureInfo.InvariantCulture))
                            ||
                            pages.Count(
                                p =>
                                    p.TabId == defaultTab.TabID.ToString(CultureInfo.InvariantCulture) &&
                                    (p.CheckedState == NodeCheckedState.Checked || p.CheckedState == NodeCheckedState.Partial)) > 0)
                        {
                            tabNode = TabController.SerializeTab(new XmlDocument { XmlResolver = null }, tabs, tab, portal, chkContent);
                        }
                    }

                    if (tabNode != null)
                    {
                        tabNode.WriteTo(writer);
                    }
                }
            }
        }

        private IEnumerable<TabDto> GetTabsToExport(int portalId, string cultureCode, bool isMultiLanguage,
            IEnumerable<TabDto> userSelection, IList<TabDto> tabsCollection)
        {
            if (tabsCollection == null)
            {
                var tab = this.GetPortalTabs(portalId, cultureCode, isMultiLanguage);
                tabsCollection = tab.ChildTabs;
                tab.ChildTabs = null;
                tab.HasChildren = false;
                tabsCollection.Add(tab);
            }

            var selectedTabs = userSelection as List<TabDto> ?? userSelection.ToList();
            foreach (var tab in tabsCollection)
            {
                if (selectedTabs.Exists(x => x.TabId == tab.TabId))
                {
                    var existingTab = selectedTabs.First(x => x.TabId == tab.TabId);
                    tab.CheckedState = existingTab.CheckedState;
                    if (string.IsNullOrEmpty(Convert.ToString(existingTab.Name)))
                    {
                        selectedTabs.Remove(existingTab);
                        selectedTabs.Add(tab);
                    }
                }
                else
                {
                    selectedTabs.Add(tab);
                }

                if (tab.HasChildren)
                {
                    var checkedState = NodeCheckedState.UnChecked;
                    if (tab.CheckedState == NodeCheckedState.Checked)
                    {
                        checkedState = NodeCheckedState.Checked;
                    }

                    var descendants = this.GetTabsDescendants(portalId, Convert.ToInt32(tab.TabId), cultureCode).ToList();
                    descendants.ForEach(x => { x.CheckedState = checkedState; });

                    selectedTabs.AddRange(this.GetTabsToExport(portalId, cultureCode, isMultiLanguage, selectedTabs,
                        descendants).Where(x => !selectedTabs.Exists(y => y.TabId == x.TabId)));
                }
            }

            return selectedTabs;
        }

        private TabCollection GetExportableTabs(TabCollection tabs)
        {
            var exportableTabs = tabs.Where(kvp => !kvp.Value.IsSystem).Select(kvp => kvp.Value);
            return new TabCollection(exportableTabs);
        }

        private TabDto GetPortalTabs(int portalId, string cultureCode, bool isMultiLanguage)
        {
            var portalInfo = PortalController.Instance.GetPortal(portalId);

            var rootNode = new TabDto
            {
                Name = portalInfo.PortalName,
                TabId = Null.NullInteger.ToString(CultureInfo.InvariantCulture),
                ChildTabs = new List<TabDto>(),
                HasChildren = true,
            };
            var tabs = new List<TabInfo>();

            cultureCode = string.IsNullOrEmpty(cultureCode) ? portalInfo.CultureCode : cultureCode;
            tabs =
                TabController.GetPortalTabs(
                    isMultiLanguage
                        ? TabController.GetTabsBySortOrder(portalId, portalInfo.DefaultLanguage, true)
                        : TabController.GetTabsBySortOrder(portalId, cultureCode, true), Null.NullInteger, false,
                    "<" + Localization.GetString("None_Specified") + ">", true, false, true, false, false, true)
                    .Where(t => (!t.DisableLink || false) && !t.IsSystem)
                    .ToList();

            tabs = tabs.Where(tab => tab.Level == 0 && tab.TabID != portalInfo.AdminTabId).ToList();

            rootNode.HasChildren = tabs.Count > 0;
            foreach (var tab in tabs)
            {
                var node = new TabDto
                {
                    Name = tab.LocalizedTabName, // $"{tab.TabName} {GetNodeStatusIcon(tab)}",
                    TabId = tab.TabID.ToString(CultureInfo.InvariantCulture),
                    ParentTabId = tab.ParentId,
                    HasChildren = tab.HasChildren,
                    ChildTabs = new List<TabDto>(),
                };
                rootNode.ChildTabs.Add(node);
            }

            rootNode.ChildTabs = rootNode.ChildTabs.ToList();

            return rootNode;
        }

        private IEnumerable<TabDto> GetTabsDescendants(int portalId, int parentId, string cultureCode)
        {
            var descendants = new List<TabDto>();
            cultureCode = string.IsNullOrEmpty(cultureCode) ? PortalController.Instance.GetPortal(portalId).CultureCode : cultureCode;

            var tabs =
                this.GetExportableTabs(TabController.Instance.GetTabsByPortal(portalId)
                    .WithCulture(cultureCode, true))
                    .WithParentId(parentId).ToList();

            foreach (var tab in tabs.Where(x => x.ParentId == parentId && (!x.IsDeleted)))
            {
                var node = new TabDto
                {
                    Name = tab.TabName, // $"{tab.TabName} {GetNodeStatusIcon(tab)}",
                    TabId = tab.TabID.ToString(CultureInfo.InvariantCulture),
                    ParentTabId = tab.ParentId,
                    HasChildren = tab.HasChildren,
                };
                descendants.Add(node);
            }

            return descendants;
        }
    }
}
