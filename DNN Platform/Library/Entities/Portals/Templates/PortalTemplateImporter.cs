// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Templates
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals.Templates;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals.Internal;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;

    internal class PortalTemplateImporter
    {
        public const string HtmlTextTimeToAutoSave = "HtmlText_TimeToAutoSave";
        public const string HtmlTextAutoSaveEnabled = "HtmlText_AutoSaveEnabled";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalTemplateImporter));
        private readonly IPermissionDefinitionService permissionDefinitionService;
        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly ListController listController;
        private readonly IEventLogger eventLogger;
        private readonly IHostSettings hostSettings;
        private readonly IPortalController portalController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IPortalGroupController portalGroupController;
        private readonly IUserController userController;
        private readonly IFileContentTypeManager fileContentTypeManager;
        private readonly RoleProvider roleProvider;
        private readonly IRoleController roleController;

        internal PortalTemplateImporter(IPermissionDefinitionService permissionDefinitionService, IBusinessControllerProvider businessControllerProvider, ListController listController, IEventLogger eventLogger, IHostSettings hostSettings, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, IUserController userController, IFileContentTypeManager fileContentTypeManager, RoleProvider roleProvider, IRoleController roleController, IPortalTemplateInfo templateToLoad)
        {
            this.permissionDefinitionService = permissionDefinitionService;
            this.businessControllerProvider = businessControllerProvider;
            this.listController = listController;
            this.eventLogger = eventLogger;
            this.hostSettings = hostSettings;
            this.portalController = portalController;
            this.appStatus = appStatus;
            this.portalGroupController = portalGroupController;
            this.userController = userController;
            this.fileContentTypeManager = fileContentTypeManager;
            this.roleProvider = roleProvider;
            this.roleController = roleController;
            var buffer = new StringBuilder(File.ReadAllText(templateToLoad.TemplateFilePath));

            if (!string.IsNullOrEmpty(templateToLoad.LanguageFilePath))
            {
                ReplaceTemplateTokens(templateToLoad, buffer);
            }

            this.TemplatePath = Path.GetDirectoryName(templateToLoad.TemplateFilePath);
            this.Template = new XmlDocument { XmlResolver = null };
            using var templateReader = XmlReader.Create(new StringReader(buffer.ToString()), new XmlReaderSettings { XmlResolver = null, });
            this.Template.Load(templateReader);
        }

        internal PortalTemplateImporter(IPermissionDefinitionService permissionDefinitionService, IBusinessControllerProvider businessControllerProvider, ListController listController, IEventLogger eventLogger, IHostSettings hostSettings, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, IUserController userController, IFileContentTypeManager fileContentTypeManager, RoleProvider roleProvider, IRoleController roleController, string templatePath, string templateFile)
        {
            this.permissionDefinitionService = permissionDefinitionService;
            this.businessControllerProvider = businessControllerProvider;
            this.listController = listController;
            this.eventLogger = eventLogger;
            this.hostSettings = hostSettings;
            this.portalController = portalController;
            this.appStatus = appStatus;
            this.portalGroupController = portalGroupController;
            this.userController = userController;
            this.fileContentTypeManager = fileContentTypeManager;
            this.roleProvider = roleProvider;
            this.roleController = roleController;
            var buffer = new StringBuilder(File.ReadAllText(Path.Combine(templatePath, templateFile)));

            this.TemplatePath = templatePath;
            this.Template = new XmlDocument { XmlResolver = null };
            using var templateReader = XmlReader.Create(new StringReader(buffer.ToString()), new XmlReaderSettings { XmlResolver = null, });
            this.Template.Load(templateReader);
        }

        public string TemplatePath { get; set; }

        private XmlDocument Template { get; set; }

        internal void ParseTemplate(int portalId, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal)
        {
            this.ParseTemplateInternal(portalId, administratorId, mergeTabs, isNewPortal);
        }

        internal void ParseTemplateInternal(int portalId, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal)
        {
            this.ParseTemplateInternal(portalId, administratorId, mergeTabs, isNewPortal, out _);
        }

        internal void ParseTemplateInternal(int portalId, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal, out LocaleCollection localeCollection)
        {
            CachingProvider.DisableCacheExpiration();

            IFolderInfo objFolder;

            var node = this.Template.SelectSingleNode("//portal/settings");
            if (node != null && isNewPortal)
            {
                HtmlUtils.WriteKeepAlive();
                this.ParsePortalSettings(node, portalId);
            }

            node = this.Template.SelectSingleNode("//locales");
            if (node != null && isNewPortal)
            {
                HtmlUtils.WriteKeepAlive();
                localeCollection = this.ParseEnabledLocales(node, portalId);
            }
            else
            {
                var portalInfo = PortalController.Instance.GetPortal(portalId);
                var defaultLocale = LocaleController.Instance.GetLocale(portalInfo.DefaultLanguage);
                if (defaultLocale == null)
                {
                    defaultLocale = new Locale { Code = portalInfo.DefaultLanguage, Fallback = Localization.SystemLocale, Text = CultureInfo.GetCultureInfo(portalInfo.DefaultLanguage).NativeName };
                    Localization.SaveLanguage(this.eventLogger, defaultLocale, false);
                }

                localeCollection = new LocaleCollection { { defaultLocale.Code, defaultLocale } };
            }

            node = this.Template.SelectSingleNode("//portal/rolegroups");
            if (node != null)
            {
                this.ParseRoleGroups(node.CreateNavigator(), portalId, administratorId);
            }

            node = this.Template.SelectSingleNode("//portal/roles");
            if (node != null)
            {
                this.ParseRoles(node.CreateNavigator(), portalId, administratorId);
            }

            node = this.Template.SelectSingleNode("//portal/portalDesktopModules");
            if (node != null)
            {
                this.ParsePortalDesktopModules(node.CreateNavigator(), portalId);
            }

            node = this.Template.SelectSingleNode("//portal/folders");
            if (node != null)
            {
                this.ParseFolders(node, portalId);
            }

            node = this.Template.SelectSingleNode("//portal/extensionUrlProviders");
            if (node != null)
            {
                ParseExtensionUrlProviders(node.CreateNavigator(), portalId);
            }

            var defaultFolderMapping = FolderMappingController.Instance.GetDefaultFolderMapping(portalId);

            if (FolderManager.Instance.GetFolder(portalId, string.Empty) == null)
            {
                objFolder = FolderManager.Instance.AddFolder(defaultFolderMapping, string.Empty);
                objFolder.IsProtected = true;
                FolderManager.Instance.UpdateFolder(objFolder);

                this.AddFolderPermissions(portalId, objFolder.FolderID);
            }

            if (FolderManager.Instance.GetFolder(portalId, "Templates/") == null)
            {
                var folderMapping = FolderMappingsConfigController.Instance.GetFolderMapping(portalId, "Templates/") ?? defaultFolderMapping;
                objFolder = FolderManager.Instance.AddFolder(folderMapping, "Templates/");
                objFolder.IsProtected = true;
                FolderManager.Instance.UpdateFolder(objFolder);

                ////AddFolderPermissions(PortalId, objFolder.FolderID);
            }

            // force creation of users folder if not present on template
            if (FolderManager.Instance.GetFolder(portalId, "Users/") == null)
            {
                var folderMapping = FolderMappingsConfigController.Instance.GetFolderMapping(portalId, "Users/") ?? defaultFolderMapping;
                objFolder = FolderManager.Instance.AddFolder(folderMapping, "Users/");
                objFolder.IsProtected = true;
                FolderManager.Instance.UpdateFolder(objFolder);

                ////AddFolderPermissions(PortalId, objFolder.FolderID);
            }

            if (mergeTabs == PortalTemplateModuleAction.Replace)
            {
                foreach (KeyValuePair<int, TabInfo> tabPair in TabController.Instance.GetTabsByPortal(portalId))
                {
                    var objTab = tabPair.Value;
                    objTab.TabName = objTab.TabName + "_old";
                    objTab.TabPath = Globals.GenerateTabPath(objTab.ParentId, objTab.TabName);
                    objTab.IsDeleted = true;
                    TabController.Instance.UpdateTab(objTab);
                    foreach (KeyValuePair<int, ModuleInfo> modulePair in ModuleController.Instance.GetTabModules(objTab.TabID))
                    {
                        var objModule = modulePair.Value;
                        ModuleController.Instance.DeleteTabModule(objModule.TabID, objModule.ModuleID, false);
                    }
                }
            }

            node = this.Template.SelectSingleNode("//portal/tabs");
            if (node != null)
            {
                string version = this.Template.DocumentElement.GetAttribute("version");
                if (version != "5.0")
                {
                    XmlDocument xmlAdmin = new XmlDocument { XmlResolver = null };
                    try
                    {
                        string path = Path.Combine(this.TemplatePath, "admin.template");
                        if (!File.Exists(path))
                        {
                            // if the template is a merged copy of a localized template the
                            // admin.template may be one director up
                            path = Path.Combine(this.TemplatePath, "..\admin.template");
                        }

                        using (var templateReader = XmlReader.Create(path, new XmlReaderSettings { XmlResolver = null, }))
                        {
                            xmlAdmin.Load(templateReader);
                        }

                        XmlNode adminNode = xmlAdmin.SelectSingleNode("//portal/tabs");
                        foreach (XmlNode adminTabNode in adminNode.ChildNodes)
                        {
                            node.AppendChild(this.Template.ImportNode(adminTabNode, true));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }

                this.ParseTabs(node, portalId, false, mergeTabs, isNewPortal);
            }

            CachingProvider.EnableCacheExpiration();
        }

        internal string CreateProfileDefinitions(int portalId)
        {
            string strMessage = Null.NullString;
            try
            {
                // parse profile definitions if available
                var node = this.Template.SelectSingleNode("//portal/profiledefinitions");
                if (node != null)
                {
                    this.ParseProfileDefinitions(node, portalId);
                }
                else
                {
                    // template does not contain profile definitions ( i.e. was created prior to DNN 3.3.0 )
                    ProfileController.AddDefaultDefinitions(this.listController, this.eventLogger, this.hostSettings, this.portalController, this.appStatus, this.portalGroupController, portalId);
                }
            }
            catch (Exception ex)
            {
                strMessage = Localization.GetString("CreateProfileDefinitions.Error");
                Exceptions.LogException(ex);
            }

            return strMessage;
        }

        private static void ReplaceTemplateTokens(IPortalTemplateInfo templateToLoad, StringBuilder buffer)
        {
            XDocument languageDoc;
            using (var reader = PortalTemplateIO.Instance.OpenTextReader(templateToLoad.LanguageFilePath))
            {
                languageDoc = XDocument.Load(reader);
            }

            var localizedData = languageDoc.Descendants("data")
                .Where(x => x.Attribute("name") != null)
                .ToDictionary(x => x.Attribute("name").Value, x => x);

            // add default resource content that's missing in the translated resources
            if (templateToLoad.CultureCode != "en-US")
            {
                AddMissingDefaultResources(templateToLoad, localizedData);
            }

            foreach (var node in localizedData.Values)
            {
                var name = node.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                var valueElement = node.Element("value");
                if (valueElement != null)
                {
                    var value = valueElement.Value;
                    buffer.Replace($"[{name}]", value);
                }
            }
        }

        private static void AddMissingDefaultResources(IPortalTemplateInfo templateToLoad, Dictionary<string, XElement> localizedData)
        {
            var defaultLangFilePath = PortalTemplateIO.Instance.GetLanguageFilePath(templateToLoad.TemplateFilePath, "en-US");
            if (string.IsNullOrEmpty(defaultLangFilePath))
            {
                return;
            }

            XDocument defaultLanguageDoc;
            using (var reader = PortalTemplateIO.Instance.OpenTextReader(defaultLangFilePath))
            {
                defaultLanguageDoc = XDocument.Load(reader);
            }

            var defaultData = defaultLanguageDoc.Descendants("data");
            foreach (var defaultNode in defaultData)
            {
                var key = defaultNode.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                if (!localizedData.ContainsKey(key))
                {
                    localizedData.Add(key, defaultNode);
                }
            }
        }

        private static void ParseExtensionUrlProviders(XPathNavigator providersNavigator, int portalId)
        {
            var providers = ExtensionUrlProviderController.GetProviders(portalId);
            foreach (XPathNavigator providerNavigator in providersNavigator.Select("extensionUrlProvider"))
            {
                HtmlUtils.WriteKeepAlive();
                var providerName = XmlUtils.GetNodeValue(providerNavigator, "name");
                var provider = providers.SingleOrDefault(p => p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
                if (provider == null)
                {
                    continue;
                }

                var active = XmlUtils.GetNodeValueBoolean(providerNavigator, "active");
                if (active)
                {
                    ExtensionUrlProviderController.EnableProvider(provider.ExtensionUrlProviderId, portalId);
                }
                else
                {
                    ExtensionUrlProviderController.DisableProvider(provider.ExtensionUrlProviderId, portalId);
                }

                var settingsNavigator = providerNavigator.SelectSingleNode("settings");
                if (settingsNavigator != null)
                {
                    foreach (XPathNavigator settingNavigator in settingsNavigator.Select("setting"))
                    {
                        var name = XmlUtils.GetAttributeValue(settingNavigator, "name");
                        var value = XmlUtils.GetAttributeValue(settingNavigator, "value");
                        ExtensionUrlProviderController.SaveSetting(provider.ExtensionUrlProviderId, portalId, name, value);
                    }
                }
            }
        }

        private static FolderMappingInfo GetFolderMappingFromStorageLocation(int portalId, XmlNode folderNode)
        {
            var storageLocation = Convert.ToInt32(XmlUtils.GetNodeValue(folderNode, "storagelocation", "0"), CultureInfo.InvariantCulture);

            switch (storageLocation)
            {
                case (int)FolderController.StorageLocationTypes.SecureFileSystem:
                    return FolderMappingController.Instance.GetFolderMapping(portalId, "Secure");
                case (int)FolderController.StorageLocationTypes.DatabaseSecure:
                    return FolderMappingController.Instance.GetFolderMapping(portalId, "Database");
                default:
                    return FolderMappingController.Instance.GetDefaultFolderMapping(portalId);
            }
        }

        private void ParseProfileDefinitions(XmlNode nodeProfileDefinitions, int portalId)
        {
            Dictionary<string, ListEntryInfo> colDataTypes = this.listController.GetListEntryInfoDictionary("DataType");

            int orderCounter = -1;
            ProfilePropertyDefinition objProfileDefinition;
            bool preferredTimeZoneFound = false;
            foreach (XmlNode node in nodeProfileDefinitions.SelectNodes("//profiledefinition"))
            {
                orderCounter += 2;
                if (!colDataTypes.TryGetValue("DataType:" + XmlUtils.GetNodeValue(node.CreateNavigator(), "datatype"), out var typeInfo))
                {
                    typeInfo = colDataTypes["DataType:Unknown"];
                }

                objProfileDefinition = new ProfilePropertyDefinition(portalId)
                {
                    DataType = typeInfo.EntryID,
                    DefaultValue = string.Empty,
                    ModuleDefId = Null.NullInteger,
                    PropertyCategory = XmlUtils.GetNodeValue(node.CreateNavigator(), "propertycategory"),
                    PropertyName = XmlUtils.GetNodeValue(node.CreateNavigator(), "propertyname"),
                    Required = false,
                    Visible = true,
                    ViewOrder = orderCounter,
                    Length = XmlUtils.GetNodeValueInt(node, "length"),
                };

                switch (XmlUtils.GetNodeValueInt(node, "defaultvisibility", 2))
                {
                    case 0:
                        objProfileDefinition.DefaultVisibility = UserVisibilityMode.AllUsers;
                        break;
                    case 1:
                        objProfileDefinition.DefaultVisibility = UserVisibilityMode.MembersOnly;
                        break;
                    case 2:
                        objProfileDefinition.DefaultVisibility = UserVisibilityMode.AdminOnly;
                        break;
                }

                if (objProfileDefinition.PropertyName == "PreferredTimeZone")
                {
                    preferredTimeZoneFound = true;
                }

                ProfileController.AddPropertyDefinition(this.eventLogger, this.portalController, this.appStatus, this.portalGroupController, objProfileDefinition);
            }

            // 6.0 requires the old TimeZone property to be marked as Deleted
            ProfilePropertyDefinition timeZoneProperty = ProfileController.GetPropertyDefinitionByName(this.hostSettings, this.portalController, this.appStatus, this.portalGroupController, portalId, "TimeZone");
            if (timeZoneProperty != null)
            {
                ProfileController.DeletePropertyDefinition(this.eventLogger, this.portalController, this.appStatus, this.portalGroupController, timeZoneProperty);
            }

            // 6.0 introduced a new property called as PreferredTimeZone. If this property is not present in template
            // it should be added. Situation will mostly happen while using an older template file.
            if (!preferredTimeZoneFound)
            {
                orderCounter += 2;

                ListEntryInfo typeInfo = colDataTypes["DataType:TimeZoneInfo"];
                if (typeInfo == null)
                {
                    typeInfo = colDataTypes["DataType:Unknown"];
                }

                objProfileDefinition = new ProfilePropertyDefinition(portalId)
                {
                    DataType = typeInfo.EntryID,
                    DefaultValue = string.Empty,
                    ModuleDefId = Null.NullInteger,
                    PropertyCategory = "Preferences",
                    PropertyName = "PreferredTimeZone",
                    Required = false,
                    Visible = true,
                    ViewOrder = orderCounter,
                    Length = 0,
                    DefaultVisibility = UserVisibilityMode.AdminOnly,
                };
                ProfileController.AddPropertyDefinition(this.eventLogger, this.portalController, this.appStatus, this.portalGroupController, objProfileDefinition);
            }
        }

        private void ParsePortalDesktopModules(XPathNavigator nav, int portalId)
        {
            foreach (XPathNavigator desktopModuleNav in nav.Select("portalDesktopModule"))
            {
                HtmlUtils.WriteKeepAlive();
                var friendlyName = XmlUtils.GetNodeValue(desktopModuleNav, "friendlyname");
                if (!string.IsNullOrEmpty(friendlyName))
                {
                    var desktopModule = DesktopModuleController.GetDesktopModuleByFriendlyName(this.hostSettings, friendlyName);
                    if (desktopModule != null)
                    {
                        // Parse the permissions
                        DesktopModulePermissionCollection permissions = new DesktopModulePermissionCollection();
                        foreach (XPathNavigator permissionNav in
                                 desktopModuleNav.Select("portalDesktopModulePermissions/portalDesktopModulePermission"))
                        {
                            string code = XmlUtils.GetNodeValue(permissionNav, "permissioncode");
                            string key = XmlUtils.GetNodeValue(permissionNav, "permissionkey");
                            DesktopModulePermissionInfo desktopModulePermission = null;
                            var permission = this.permissionDefinitionService.GetDefinitionsByCodeAndKey(code, key).FirstOrDefault();
                            if (permission != null)
                            {
                                desktopModulePermission = new DesktopModulePermissionInfo(permission);
                            }

                            desktopModulePermission.AllowAccess = bool.Parse(XmlUtils.GetNodeValue(permissionNav, "allowaccess"));
                            string rolename = XmlUtils.GetNodeValue(permissionNav, "rolename");
                            if (!string.IsNullOrEmpty(rolename))
                            {
                                RoleInfo role = RoleController.Instance.GetRole(portalId, r => r.RoleName == rolename);
                                if (role != null)
                                {
                                    ((IPermissionInfo)desktopModulePermission).RoleId = role.RoleID;
                                }
                            }

                            permissions.Add(desktopModulePermission);
                        }

                        DesktopModuleController.AddDesktopModuleToPortal(portalId, desktopModule, permissions, false);
                    }
                }
            }
        }

        private void ParseFolderPermissions(XmlNodeList nodeFolderPermissions, int portalId, FolderInfo folder)
        {
            int permissionId = 0;

            // Clear the current folder permissions
            folder.FolderPermissions.Clear();
            foreach (XmlNode xmlFolderPermission in nodeFolderPermissions)
            {
                string permissionKey = XmlUtils.GetNodeValue(xmlFolderPermission.CreateNavigator(), "permissionkey");
                string permissionCode = XmlUtils.GetNodeValue(xmlFolderPermission.CreateNavigator(), "permissioncode");
                string roleName = XmlUtils.GetNodeValue(xmlFolderPermission.CreateNavigator(), "rolename");
                bool allowAccess = XmlUtils.GetNodeValueBoolean(xmlFolderPermission, "allowaccess");
                foreach (var permission in this.permissionDefinitionService.GetDefinitionsByCodeAndKey(permissionCode, permissionKey))
                {
                    permissionId = permission.PermissionId;
                }

                int roleId = int.MinValue;
                switch (roleName)
                {
                    case Globals.glbRoleAllUsersName:
                        roleId = Convert.ToInt32(Globals.glbRoleAllUsers, CultureInfo.InvariantCulture);
                        break;
                    case Globals.glbRoleUnauthUserName:
                        roleId = Convert.ToInt32(Globals.glbRoleUnauthUser, CultureInfo.InvariantCulture);
                        break;
                    default:
                        RoleInfo objRole = RoleController.Instance.GetRole(portalId, r => r.RoleName == roleName);
                        if (objRole != null)
                        {
                            roleId = objRole.RoleID;
                        }

                        break;
                }

                // if role was found add, otherwise ignore
                if (roleId != int.MinValue)
                {
                    IFolderPermissionInfo folderPermission = new FolderPermissionInfo { AllowAccess = allowAccess, };
                    folderPermission.FolderId = folder.FolderID;
                    folderPermission.PermissionId = permissionId;
                    folderPermission.RoleId = roleId;
                    folderPermission.UserId = Null.NullInteger;

                    bool canAdd = !folder.FolderPermissions
                        .Any((IFolderPermissionInfo fp) => fp.FolderId == folderPermission.FolderId &&
                                                           fp.PermissionId == folderPermission.PermissionId &&
                                                           fp.RoleId == folderPermission.RoleId &&
                                                           fp.UserId == folderPermission.UserId);
                    if (canAdd)
                    {
                        folder.FolderPermissions.Add((FolderPermissionInfo)folderPermission);
                    }
                }
            }

            FolderPermissionController.SaveFolderPermissions(folder);
        }

        private void ParseFiles(XmlNodeList nodeFiles, int portalId, FolderInfo folder)
        {
            var fileManager = FileManager.Instance;

            foreach (XmlNode node in nodeFiles)
            {
                var fileName = XmlUtils.GetNodeValue(node.CreateNavigator(), "filename");

                // First check if the file exists
                var file = fileManager.GetFile(folder, fileName);

                if (file != null)
                {
                    continue;
                }

                file = new Services.FileSystem.FileInfo
                {
                    PortalId = portalId,
                    FileName = fileName,
                    Extension = XmlUtils.GetNodeValue(node.CreateNavigator(), "extension"),
                    Size = XmlUtils.GetNodeValueInt(node, "size"),
                    Width = XmlUtils.GetNodeValueInt(node, "width"),
                    Height = XmlUtils.GetNodeValueInt(node, "height"),
                    ContentType = XmlUtils.GetNodeValue(node.CreateNavigator(), "contenttype"),
                    SHA1Hash = XmlUtils.GetNodeValue(node.CreateNavigator(), "sha1hash"),
                    FolderId = folder.FolderID,
                    Folder = folder.FolderPath,
                    Title = string.Empty,
                    StartDate = DateTime.Now,
                    EndDate = Null.NullDate,
                    EnablePublishPeriod = false,
                    ContentItemID = Null.NullInteger,
                };

                // Save new File
                try
                {
                    // Initially, install files are on local system, then we need the Standard folder provider to read the content regardless the target folderprovider
                    using (var fileContent = FolderProvider.Instance("StandardFolderProvider").GetFileStream(file))
                    {
                        var contentType = this.fileContentTypeManager.GetContentType(Path.GetExtension(fileName));
                        var userId = this.userController.GetCurrentUserInfo().UserID;
                        file.FileId = fileManager.AddFile(folder, fileName, fileContent, false, false, true, contentType, userId).FileId;
                    }

                    fileManager.UpdateFile(file);
                }
                catch (InvalidFileExtensionException ex)
                {
                    // when the file is not allowed, we should not break parse process, but just log the error.
                    Logger.Error(ex.Message);
                }
            }
        }

        private void CreateRoleGroup(RoleGroupInfo roleGroup)
        {
            // First check if the role exists
            var objRoleGroupInfo = RoleController.GetRoleGroupByName(this.roleProvider, roleGroup.PortalID, roleGroup.RoleGroupName);

            if (objRoleGroupInfo == null)
            {
                roleGroup.RoleGroupID = RoleController.AddRoleGroup(this.roleProvider, this.eventLogger, this.userController, this.portalController.GetCurrentSettings(), roleGroup);
            }
            else
            {
                roleGroup.RoleGroupID = objRoleGroupInfo.RoleGroupID;
            }
        }

        private int CreateRole(RoleInfo role)
        {
            int roleId;

            // First check if the role exists
            var objRoleInfo = this.roleController.GetRole(role.PortalID, r => r.RoleName == role.RoleName);
            if (objRoleInfo == null)
            {
                roleId = this.roleController.AddRole(role);
            }
            else
            {
                roleId = objRoleInfo.RoleID;
            }

            return roleId;
        }

        private int CreateRole(int portalId, string roleName, string description, float serviceFee, int billingPeriod, string billingFrequency, float trialFee, int trialPeriod, string trialFrequency, bool isPublic, bool isAuto)
        {
            return this.CreateRole(new RoleInfo
            {
                PortalID = portalId, RoleName = roleName, RoleGroupID = Null.NullInteger, Description = description,
                ServiceFee = Convert.ToSingle(serviceFee < 0 ? 0 : serviceFee),
                BillingPeriod = billingPeriod,
                BillingFrequency = billingFrequency,
                TrialFee = Convert.ToSingle(trialFee < 0 ? 0 : trialFee),
                TrialPeriod = trialPeriod,
                TrialFrequency = trialFrequency,
                IsPublic = isPublic,
                AutoAssignment = isAuto,
            });
        }

        private void CreateDefaultPortalRoles(int portalId, int administratorId, ref int administratorRoleId, ref int registeredRoleId, ref int subscriberRoleId, int unverifiedRoleId)
        {
            // create required roles if not already created
            if (administratorRoleId == -1)
            {
                administratorRoleId = this.CreateRole(portalId, "Administrators", "Administrators of this Website", 0, 0, "M", 0, 0, "N", false, false);
            }

            if (registeredRoleId == -1)
            {
                registeredRoleId = this.CreateRole(portalId, "Registered Users", "Registered Users", 0, 0, "M", 0, 0, "N", false, true);
            }

            if (subscriberRoleId == -1)
            {
                subscriberRoleId = this.CreateRole(portalId, "Subscribers", "A public role for site subscriptions", 0, 0, "M", 0, 0, "N", true, true);
            }

            if (unverifiedRoleId == -1)
            {
                this.CreateRole(portalId, "Unverified Users", "Unverified Users", 0, 0, "M", 0, 0, "N", false, false);
            }

            RoleController.Instance.AddUserRole(portalId, administratorId, administratorRoleId, RoleStatus.Approved, false, Null.NullDate, Null.NullDate);
            RoleController.Instance.AddUserRole(portalId, administratorId, registeredRoleId, RoleStatus.Approved, false, Null.NullDate, Null.NullDate);
            RoleController.Instance.AddUserRole(portalId, administratorId, subscriberRoleId, RoleStatus.Approved, false, Null.NullDate, Null.NullDate);
        }

        private LocaleCollection ParseEnabledLocales(XmlNode nodeEnabledLocales, int portalId)
        {
            var defaultLocale = LocaleController.Instance.GetDefaultLocale(portalId);
            var returnCollection = new LocaleCollection { { defaultLocale.Code, defaultLocale } };
            var clearCache = false;
            foreach (XmlNode node in nodeEnabledLocales.SelectNodes("//locale"))
            {
                var cultureCode = node.InnerText;
                var locale = LocaleController.Instance.GetLocale(cultureCode);
                if (locale == null)
                {
                    // if language does not exist in the installation, create it
                    locale = new Locale { Code = cultureCode, Fallback = Localization.SystemLocale, Text = CultureInfo.GetCultureInfo(cultureCode).NativeName };
                    Localization.SaveLanguage(this.eventLogger, locale, false);
                    clearCache = true;
                }

                if (locale.Code != defaultLocale.Code)
                {
                    returnCollection.Add(locale.Code, locale);
                }
            }

            if (clearCache)
            {
                DataCache.ClearHostCache(true);
            }

            return returnCollection;
        }

        private void ParseFolders(XmlNode nodeFolders, int portalId)
        {
            var folderManager = FolderManager.Instance;
            var folderMappingController = FolderMappingController.Instance;
            var xmlNodeList = nodeFolders.SelectNodes("//folder");
            if (xmlNodeList != null)
            {
                foreach (XmlNode node in xmlNodeList)
                {
                    HtmlUtils.WriteKeepAlive();
                    var folderPath = XmlUtils.GetNodeValue(node.CreateNavigator(), "folderpath");

                    // First check if the folder exists
                    var objInfo = folderManager.GetFolder(portalId, folderPath);

                    if (objInfo == null)
                    {
                        FolderMappingInfo folderMapping;
                        try
                        {
                            folderMapping = FolderMappingsConfigController.Instance.GetFolderMapping(portalId, folderPath)
                                            ?? GetFolderMappingFromStorageLocation(portalId, node);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            folderMapping = folderMappingController.GetDefaultFolderMapping(portalId);
                        }

                        var isProtected = XmlUtils.GetNodeValueBoolean(node, "isprotected");

                        try
                        {
                            // Save new folder
                            objInfo = folderManager.AddFolder(folderMapping, folderPath);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);

                            // Retry with default folderMapping
                            var defaultFolderMapping = folderMappingController.GetDefaultFolderMapping(portalId);
                            if (folderMapping.FolderMappingID != defaultFolderMapping.FolderMappingID)
                            {
                                objInfo = folderManager.AddFolder(defaultFolderMapping, folderPath);
                            }
                            else
                            {
                                throw;
                            }
                        }

                        objInfo.IsProtected = isProtected;

                        folderManager.UpdateFolder(objInfo);
                    }

                    var nodeFolderPermissions = node.SelectNodes("folderpermissions/permission");
                    this.ParseFolderPermissions(nodeFolderPermissions, portalId, (FolderInfo)objInfo);

                    var nodeFiles = node.SelectNodes("files/file");

                    this.ParseFiles(nodeFiles, portalId, (FolderInfo)objInfo);
                }
            }
        }

        private void ParsePortalSettings(XmlNode nodeSettings, int portalId)
        {
            string currentCulture = PortalController.GetActivePortalLanguage(this.hostSettings, this.appStatus, portalId);
            var objPortal = PortalController.Instance.GetPortal(portalId);
            objPortal.LogoFile = Globals.ImportFile(portalId, XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "logofile"));
            objPortal.FooterText = XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "footertext");
            if (nodeSettings.SelectSingleNode("expirydate") != null)
            {
                objPortal.ExpiryDate = XmlUtils.GetNodeValueDate(nodeSettings, "expirydate", Null.NullDate);
            }

            objPortal.UserRegistration = XmlUtils.GetNodeValueInt(nodeSettings, "userregistration");
            objPortal.BannerAdvertising = XmlUtils.GetNodeValueInt(nodeSettings, "banneradvertising");
            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "currency")))
            {
                objPortal.Currency = XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "currency");
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "hostfee")))
            {
                objPortal.HostFee = XmlUtils.GetNodeValueSingle(nodeSettings, "hostfee");
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "hostspace")))
            {
                objPortal.HostSpace = XmlUtils.GetNodeValueInt(nodeSettings, "hostspace");
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "pagequota")))
            {
                objPortal.PageQuota = XmlUtils.GetNodeValueInt(nodeSettings, "pagequota");
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "userquota")))
            {
                objPortal.UserQuota = XmlUtils.GetNodeValueInt(nodeSettings, "userquota");
            }

            objPortal.BackgroundFile = XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "backgroundfile");
            objPortal.PaymentProcessor = XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "paymentprocessor");

            objPortal.DefaultLanguage = XmlUtils.GetNodeValue(nodeSettings, "defaultlanguage", "en-US");
            PortalController.Instance.UpdatePortalInfo(objPortal);

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "skinsrc", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "DefaultPortalSkin", XmlUtils.GetNodeValue(nodeSettings, "skinsrc", string.Empty), true, currentCulture);
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "skinsrcadmin", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "DefaultAdminSkin", XmlUtils.GetNodeValue(nodeSettings, "skinsrcadmin", string.Empty), true, currentCulture);
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "containersrc", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "DefaultPortalContainer", XmlUtils.GetNodeValue(nodeSettings, "containersrc", string.Empty), true, currentCulture);
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "containersrcadmin", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "DefaultAdminContainer", XmlUtils.GetNodeValue(nodeSettings, "containersrcadmin", string.Empty), true, currentCulture);
            }

            // Enable Skin Widgets Setting
            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "enableskinwidgets", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "EnableSkinWidgets", XmlUtils.GetNodeValue(nodeSettings, "enableskinwidgets", string.Empty));
            }

            // Enable AutoSAve feature
            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "enableautosave", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, HtmlTextAutoSaveEnabled, XmlUtils.GetNodeValue(nodeSettings, "enableautosave", string.Empty));

                // Time to autosave, only if enableautosave exists
                if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "timetoautosave", string.Empty)))
                {
                    PortalController.UpdatePortalSetting(this.portalController, portalId, HtmlTextTimeToAutoSave, XmlUtils.GetNodeValue(nodeSettings, "timetoautosave", string.Empty));
                }
            }

            // Set Auto alias mapping
            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "portalaliasmapping", "CANONICALURL")))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "PortalAliasMapping", XmlUtils.GetNodeValue(nodeSettings, "portalaliasmapping", "CANONICALURL").ToUpperInvariant());
            }

            // Set Time Zone maping
            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "timezone", Localization.SystemTimeZone)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "TimeZone", XmlUtils.GetNodeValue(nodeSettings, "timezone", Localization.SystemTimeZone));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "contentlocalizationenabled")))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "ContentLocalizationEnabled", XmlUtils.GetNodeValue(nodeSettings, "contentlocalizationenabled"));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "inlineeditorenabled")))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "InlineEditorEnabled", XmlUtils.GetNodeValue(nodeSettings, "inlineeditorenabled"));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "enablepopups")))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "EnablePopUps", XmlUtils.GetNodeValue(nodeSettings, "enablepopups"));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "hidefoldersenabled")))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "HideFoldersEnabled", XmlUtils.GetNodeValue(nodeSettings, "hidefoldersenabled"));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "controlpanelmode")))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "ControlPanelMode", XmlUtils.GetNodeValue(nodeSettings, "controlpanelmode"));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "controlpanelsecurity")))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "ControlPanelSecurity", XmlUtils.GetNodeValue(nodeSettings, "controlpanelsecurity"));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "controlpanelvisibility")))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "ControlPanelVisibility", XmlUtils.GetNodeValue(nodeSettings, "controlpanelvisibility"));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "pageheadtext", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "PageHeadText", XmlUtils.GetNodeValue(nodeSettings, "pageheadtext", string.Empty));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "injectmodulehyperlink", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "InjectModuleHyperLink", XmlUtils.GetNodeValue(nodeSettings, "injectmodulehyperlink", string.Empty));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "addcompatiblehttpheader", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "AddCompatibleHttpHeader", XmlUtils.GetNodeValue(nodeSettings, "addcompatiblehttpheader", string.Empty));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "allowuseruiculture", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "AllowUserUICulture", XmlUtils.GetNodeValue(nodeSettings, "allowuseruiculture", string.Empty));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "enablebrowserlanguage", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "EnableBrowserLanguage", XmlUtils.GetNodeValue(nodeSettings, "enablebrowserlanguage", string.Empty));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "showcookieconsent", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "ShowCookieConsent", XmlUtils.GetNodeValue(nodeSettings, "showcookieconsent", "False"));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "cookiemorelink", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "CookieMoreLink", XmlUtils.GetNodeValue(nodeSettings, "cookiemorelink", string.Empty), true, currentCulture);
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "dataconsentactive", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "DataConsentActive", XmlUtils.GetNodeValue(nodeSettings, "dataconsentactive", "False"));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "dataconsenttermslastchange", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "DataConsentTermsLastChange", XmlUtils.GetNodeValue(nodeSettings, "dataconsenttermslastchange", string.Empty), true, currentCulture);
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "dataconsentconsentredirect", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "DataConsentConsentRedirect", XmlUtils.GetNodeValue(nodeSettings, "dataconsentconsentredirect", string.Empty), true, currentCulture);
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "dataconsentuserdeleteaction", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "DataConsentUserDeleteAction", XmlUtils.GetNodeValue(nodeSettings, "dataconsentuserdeleteaction", string.Empty), true, currentCulture);
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "dataconsentdelay", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "DataConsentDelay", XmlUtils.GetNodeValue(nodeSettings, "dataconsentdelay", string.Empty), true, currentCulture);
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "dataconsentdelaymeasurement", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "DataConsentDelayMeasurement", XmlUtils.GetNodeValue(nodeSettings, "dataconsentdelaymeasurement", string.Empty), true, currentCulture);
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "sitemapcachedays", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "SitemapCacheDays", XmlUtils.GetNodeValue(nodeSettings, "sitemapcachedays", string.Empty), true);
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "sitemapexcludepriority", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "SitemapExcludePriority", XmlUtils.GetNodeValue(nodeSettings, "sitemapexcludepriority", string.Empty), true);
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "sitemapincludehidden", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "SitemmpIncludeHidden", XmlUtils.GetNodeValue(nodeSettings, "sitemapincludehidden", string.Empty), true);
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "sitemaplevelmode", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "SitemapLevelMode", XmlUtils.GetNodeValue(nodeSettings, "sitemaplevelmode", string.Empty), true);
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "sitemapminpriority", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "SitemapMinPriority", XmlUtils.GetNodeValue(nodeSettings, "sitemapminpriority", string.Empty), true);
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "showquickmoduleaddmenu", string.Empty)))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, "ShowQuickModuleAddMenu", XmlUtils.GetNodeValue(nodeSettings, "showquickmoduleaddmenu", string.Empty));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "allowjsinmoduleheaders", string.Empty)))
            {
                PortalController.UpdatePortalSetting(portalId, "AllowJsInModuleHeaders", XmlUtils.GetNodeValue(nodeSettings, "allowjsinmoduleheaders", string.Empty));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "allowjsinmodulefooters", string.Empty)))
            {
                PortalController.UpdatePortalSetting(portalId, "AllowJsInModuleFooters", XmlUtils.GetNodeValue(nodeSettings, "allowjsinmodulefooters", string.Empty));
            }
        }

        private void ParseRoleGroups(XPathNavigator nav, int portalId, int administratorId)
        {
            var administratorRoleId = -1;
            var registeredRoleId = -1;
            var subscriberRoleId = -1;
            var unverifiedRoleId = -1;

            foreach (XPathNavigator roleGroupNav in nav.Select("rolegroup"))
            {
                HtmlUtils.WriteKeepAlive();
                var roleGroup = CBO.DeserializeObject<RoleGroupInfo>(new StringReader(roleGroupNav.OuterXml));
                if (roleGroup.RoleGroupName != "GlobalRoles")
                {
                    roleGroup.PortalID = portalId;
                    this.CreateRoleGroup(roleGroup);
                }

                foreach (var role in roleGroup.Roles.Values)
                {
                    role.PortalID = portalId;
                    role.RoleGroupID = roleGroup.RoleGroupID;
                    role.Status = RoleStatus.Approved;
                    switch (role.RoleType)
                    {
                        case RoleType.Administrator:
                            administratorRoleId = this.CreateRole(role);
                            break;
                        case RoleType.RegisteredUser:
                            registeredRoleId = this.CreateRole(role);
                            break;
                        case RoleType.Subscriber:
                            subscriberRoleId = this.CreateRole(role);
                            break;
                        case RoleType.None:
                            this.CreateRole(role);
                            break;
                        case RoleType.UnverifiedUser:
                            unverifiedRoleId = this.CreateRole(role);
                            break;
                    }
                }
            }

            this.CreateDefaultPortalRoles(portalId, administratorId, ref administratorRoleId, ref registeredRoleId, ref subscriberRoleId, unverifiedRoleId);

            // update portal setup
            var portal = PortalController.Instance.GetPortal(portalId);
            this.UpdatePortalSetup(
                portalId,
                administratorId,
                administratorRoleId,
                registeredRoleId,
                portal.SplashTabId,
                portal.HomeTabId,
                portal.LoginTabId,
                portal.RegisterTabId,
                portal.UserTabId,
                portal.SearchTabId,
                portal.Custom404TabId,
                portal.Custom500TabId,
                portal.TermsTabId,
                portal.PrivacyTabId,
                portal.AdminTabId,
                PortalController.GetActivePortalLanguage(this.hostSettings, this.appStatus, portalId));
        }

        private void ParseRoles(XPathNavigator nav, int portalID, int administratorId)
        {
            var administratorRoleId = -1;
            var registeredRoleId = -1;
            var subscriberRoleId = -1;
            var unverifiedRoleId = -1;

            foreach (XPathNavigator roleNav in nav.Select("role"))
            {
                HtmlUtils.WriteKeepAlive();
                var role = CBO.DeserializeObject<RoleInfo>(new StringReader(roleNav.OuterXml));
                role.PortalID = portalID;
                role.RoleGroupID = Null.NullInteger;
                switch (role.RoleType)
                {
                    case RoleType.Administrator:
                        administratorRoleId = this.CreateRole(role);
                        break;
                    case RoleType.RegisteredUser:
                        registeredRoleId = this.CreateRole(role);
                        break;
                    case RoleType.Subscriber:
                        subscriberRoleId = this.CreateRole(role);
                        break;
                    case RoleType.None:
                        this.CreateRole(role);
                        break;
                    case RoleType.UnverifiedUser:
                        unverifiedRoleId = this.CreateRole(role);
                        break;
                }
            }

            // create required roles if not already created
            this.CreateDefaultPortalRoles(portalID, administratorId, ref administratorRoleId, ref registeredRoleId, ref subscriberRoleId, unverifiedRoleId);

            // update portal setup
            var portal = PortalController.Instance.GetPortal(portalID);
            this.UpdatePortalSetup(
                portalID,
                administratorId,
                administratorRoleId,
                registeredRoleId,
                portal.SplashTabId,
                portal.HomeTabId,
                portal.LoginTabId,
                portal.RegisterTabId,
                portal.UserTabId,
                portal.SearchTabId,
                portal.Custom404TabId,
                portal.Custom500TabId,
                portal.TermsTabId,
                portal.PrivacyTabId,
                portal.AdminTabId,
                PortalController.GetActivePortalLanguage(this.hostSettings, this.appStatus, portalID));
        }

        private void ParseTab(XmlNode nodeTab, int portalId, bool isAdminTemplate, PortalTemplateModuleAction mergeTabs, ref Hashtable hModules, ref Hashtable hTabs, bool isNewPortal)
        {
            TabInfo tab = null;
            string strName = XmlUtils.GetNodeValue(nodeTab.CreateNavigator(), "name");
            var portal = PortalController.Instance.GetPortal(portalId);
            if (!string.IsNullOrEmpty(strName))
            {
                if (!isNewPortal)
                {
                    // running from wizard: try to find the tab by path
                    string parentTabName = string.Empty;
                    if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeTab.CreateNavigator(), "parent")))
                    {
                        parentTabName = XmlUtils.GetNodeValue(nodeTab.CreateNavigator(), "parent") + "/";
                    }

                    if (hTabs[parentTabName + strName] != null)
                    {
                        tab = TabController.Instance.GetTab(Convert.ToInt32(hTabs[parentTabName + strName], CultureInfo.InvariantCulture), portalId, false);
                    }
                }

                if (tab == null || isNewPortal)
                {
                    tab = TabController.DeserializeTab(this.businessControllerProvider, this.permissionDefinitionService, nodeTab, null, hTabs, portalId, isAdminTemplate, mergeTabs.ToOldEnum(), hModules);
                }

                // when processing the template we should try and identify the Admin tab
                var logType = "AdminTab";
                if (tab.TabName == "Admin")
                {
                    portal.AdminTabId = tab.TabID;
                }

                // when processing the template we can find: hometab, usertab, logintab
                switch (XmlUtils.GetNodeValue(nodeTab, "tabtype", string.Empty).ToLowerInvariant())
                {
                    case "splashtab":
                        portal.SplashTabId = tab.TabID;
                        logType = "SplashTab";
                        break;
                    case "hometab":
                        portal.HomeTabId = tab.TabID;
                        logType = "HomeTab";
                        break;
                    case "logintab":
                        portal.LoginTabId = tab.TabID;
                        logType = "LoginTab";
                        break;
                    case "usertab":
                        portal.UserTabId = tab.TabID;
                        logType = "UserTab";
                        break;
                    case "searchtab":
                        portal.SearchTabId = tab.TabID;
                        logType = "SearchTab";
                        break;
                    case "404tab":
                        portal.Custom404TabId = tab.TabID;
                        logType = "Custom404Tab";
                        break;
                    case "500tab":
                        portal.Custom500TabId = tab.TabID;
                        logType = "Custom500Tab";
                        break;
                    case "termstab":
                        portal.TermsTabId = tab.TabID;
                        logType = "TermsTabId";
                        break;
                    case "privacytab":
                        portal.PrivacyTabId = tab.TabID;
                        logType = "PrivacyTabId";
                        break;
                }

                this.UpdatePortalSetup(
                    portalId,
                    portal.AdministratorId,
                    portal.AdministratorRoleId,
                    portal.RegisteredRoleId,
                    portal.SplashTabId,
                    portal.HomeTabId,
                    portal.LoginTabId,
                    portal.RegisterTabId,
                    portal.UserTabId,
                    portal.SearchTabId,
                    portal.Custom404TabId,
                    portal.Custom500TabId,
                    portal.TermsTabId,
                    portal.PrivacyTabId,
                    portal.AdminTabId,
                    PortalController.GetActivePortalLanguage(this.hostSettings, this.appStatus, portalId));
                this.eventLogger.AddLog(
                    logType,
                    tab.TabID.ToString(CultureInfo.InvariantCulture),
                    PortalSettings.Current,
                    UserController.Instance.GetCurrentUserInfo().UserID,
                    EventLogType.PORTAL_SETTING_UPDATED);
            }
        }

        private void ParseTabs(XmlNode nodeTabs, int portalId, bool isAdminTemplate, PortalTemplateModuleAction mergeTabs, bool isNewPortal)
        {
            // used to control if modules are true modules or instances
            // will hold module ID from template / new module ID so new instances can reference right moduleid
            // only first one from the template will be created as a true module,
            // others will be moduleinstances (tabmodules)
            Hashtable hModules = new Hashtable();
            Hashtable hTabs = new Hashtable();

            // if running from wizard we need to pre-populate htabs with existing tabs so ParseTab
            // can find all existing ones
            if (!isNewPortal)
            {
                Hashtable hTabNames = new Hashtable();
                foreach (var tabPair in TabController.Instance.GetTabsByPortal(portalId))
                {
                    TabInfo objTab = tabPair.Value;
                    if (!objTab.IsDeleted)
                    {
                        var tabName = objTab.TabName;
                        if (!Null.IsNull(objTab.ParentId))
                        {
                            tabName =
                                $"{Convert.ToString(hTabNames[objTab.ParentId], CultureInfo.InvariantCulture)}/{objTab.TabName}";
                        }

                        hTabNames.Add(objTab.TabID, tabName);
                    }
                }

                // when parsing tabs we will need tabid given tabname
                foreach (int i in hTabNames.Keys)
                {
                    if (hTabs[hTabNames[i]] == null)
                    {
                        hTabs.Add(hTabNames[i], i);
                    }
                }

                hTabNames.Clear();
            }

            foreach (XmlNode nodeTab in nodeTabs.SelectNodes("//tab"))
            {
                HtmlUtils.WriteKeepAlive();
                this.ParseTab(nodeTab, portalId, isAdminTemplate, mergeTabs, ref hModules, ref hTabs, isNewPortal);
            }

            // Process tabs that are linked to tabs
            foreach (XmlNode nodeTab in nodeTabs.SelectNodes("//tab[url/@type = 'Tab']"))
            {
                HtmlUtils.WriteKeepAlive();
                int tabId = XmlUtils.GetNodeValueInt(nodeTab, "tabid", Null.NullInteger);
                string tabPath = XmlUtils.GetNodeValue(nodeTab, "url", Null.NullString);
                if (tabId > Null.NullInteger)
                {
                    TabInfo objTab = TabController.Instance.GetTab(tabId, portalId, false);
                    objTab.Url = TabController.GetTabByTabPath(this.hostSettings, portalId, tabPath, Null.NullString).ToString(CultureInfo.InvariantCulture);
                    TabController.Instance.UpdateTab(objTab);
                }
            }

            var folderManager = FolderManager.Instance;
            var fileManager = FileManager.Instance;

            // Process tabs that are linked to files
            foreach (XmlNode nodeTab in nodeTabs.SelectNodes("//tab[url/@type = 'File']"))
            {
                HtmlUtils.WriteKeepAlive();
                var tabId = XmlUtils.GetNodeValueInt(nodeTab, "tabid", Null.NullInteger);
                var filePath = XmlUtils.GetNodeValue(nodeTab, "url", Null.NullString);
                if (tabId > Null.NullInteger)
                {
                    var objTab = TabController.Instance.GetTab(tabId, portalId, false);

                    var fileName = Path.GetFileName(filePath);

                    var folderPath = filePath.Substring(0, filePath.LastIndexOf(fileName, StringComparison.OrdinalIgnoreCase));
                    var folder = folderManager.GetFolder(portalId, folderPath);

                    var file = fileManager.GetFile(folder, fileName);

                    objTab.Url = "FileID=" + file.FileId;
                    TabController.Instance.UpdateTab(objTab);
                }
            }
        }

        private void UpdatePortalSetup(int portalId, int administratorId, int administratorRoleId, int registeredRoleId, int splashTabId, int homeTabId, int loginTabId, int registerTabId, int userTabId, int searchTabId, int custom404TabId, int custom500TabId, int termsTabId, int privacyTabId, int adminTabId, string cultureCode)
        {
            DataProvider.Instance().UpdatePortalSetup(
                portalId,
                administratorId,
                administratorRoleId,
                registeredRoleId,
                splashTabId,
                homeTabId,
                loginTabId,
                registerTabId,
                userTabId,
                searchTabId,
                custom404TabId,
                custom500TabId,
                termsTabId,
                privacyTabId,
                adminTabId,
                cultureCode);
            this.eventLogger.AddLog("PortalId", portalId.ToString(CultureInfo.InvariantCulture), PortalSettings.Current, UserController.Instance.GetCurrentUserInfo().UserID, EventLogType.PORTALINFO_UPDATED);
            DataCache.ClearHostCache(true);
        }

        private void AddFolderPermissions(int portalId, int folderId)
        {
            var portal = PortalController.Instance.GetPortal(portalId);
            var folderManager = FolderManager.Instance;
            var folder = folderManager.GetFolder(folderId);
            foreach (var permission in this.permissionDefinitionService.GetDefinitionsByCodeAndKey("SYSTEM_FOLDER", string.Empty))
            {
                var folderPermission = new FolderPermissionInfo(permission)
                {
                    AllowAccess = true,
                };
                ((IPermissionInfo)folderPermission).RoleId = portal.AdministratorRoleId;
                ((IFolderPermissionInfo)folderPermission).FolderId = folder.FolderID;

                folder.FolderPermissions.Add(folderPermission);
                if (permission.PermissionKey == "READ")
                {
                    // add READ permissions to the All Users Role
                    if (folderManager is FolderManager fm)
                    {
                        fm.AddAllUserReadPermission(folder, permission);
                    }
                    else if (permission is PermissionInfo p)
                    {
                        folderManager.AddAllUserReadPermission(folder, p);
                    }
                    else
                    {
                        var permissionInfo = new PermissionInfo
                        {
                            PermissionCode = permission.PermissionCode,
                            PermissionKey = permission.PermissionKey,
                            PermissionName = permission.PermissionName,
                        };
                        ((IPermissionDefinitionInfo)permissionInfo).ModuleDefId = permission.ModuleDefId;
                        ((IPermissionDefinitionInfo)permissionInfo).PermissionId = permission.PermissionId;
                        folderManager.AddAllUserReadPermission(folder, permissionInfo);
                    }
                }
            }

            FolderPermissionController.SaveFolderPermissions((FolderInfo)folder);
        }
    }
}
