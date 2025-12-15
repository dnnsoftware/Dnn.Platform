// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Portals.Templates;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Portals.Templates;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Log;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Upgrade.InternalController.Steps;
    using DotNetNuke.Services.Upgrade.Internals;
    using DotNetNuke.Services.Upgrade.Internals.Steps;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using Microsoft.Extensions.DependencyInjection;

    using Assembly = System.Reflection.Assembly;
    using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;
    using Localization = DotNetNuke.Services.Localization.Localization;
    using ModuleInfo = DotNetNuke.Entities.Modules.ModuleInfo;

    /// <summary>The Upgrade class provides Shared/Static methods to Upgrade/Install a DotNetNuke Application.</summary>
    public partial class Upgrade
    {
        private const string FipsCompilanceAssembliesCheckedKey = "FipsCompilanceAssembliesChecked";
        private const string FipsCompilanceAssembliesFolder = "App_Data\\FipsCompilanceAssemblies";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Upgrade));
        private static readonly object ThreadLocker = new object();
        private static DateTime startTime;

        public static string DefaultProvider
        {
            get
            {
                return Config.GetDefaultProvider("data").Name;
            }
        }

        public static TimeSpan RunTime
        {
            get
            {
                DateTime currentTime = DateTime.Now;
                return currentTime.Subtract(startTime);
            }
        }

        private static Version ApplicationVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        public static int RemoveModule(string desktopModuleName, string tabName, int parentId, bool removeTab)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "RemoveModule:" + desktopModuleName);
            TabInfo tab = TabController.Instance.GetTabByName(tabName, Null.NullInteger, parentId);
            int moduleDefId = 0;
            int count = 0;

            // Get the Modules on the Tab
            if (tab != null)
            {
                foreach (KeyValuePair<int, ModuleInfo> kvp in ModuleController.Instance.GetTabModules(tab.TabID))
                {
                    var module = kvp.Value;
                    if (module.DesktopModule.FriendlyName == desktopModuleName)
                    {
                        // Delete the Module from the Modules list
                        ModuleController.Instance.DeleteTabModule(module.TabID, module.ModuleID, false);
                        moduleDefId = module.ModuleDefID;
                    }
                    else
                    {
                        count += 1;
                    }
                }

                // If Tab has no modules optionally remove tab
                if (count == 0 && removeTab)
                {
                    TabController.Instance.DeleteTab(tab.TabID, tab.PortalID);
                }
            }

            return moduleDefId;
        }

        public static void MakeModulePremium(string moduleName)
        {
            var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(moduleName, -1);
            if (desktopModule != null)
            {
                desktopModule.IsAdmin = true;
                desktopModule.IsPremium = true;
                DesktopModuleController.SaveDesktopModule(desktopModule, false, true);

                // Remove Portal/Module to PortalDesktopModules
                DesktopModuleController.RemoveDesktopModuleFromPortals(desktopModule.DesktopModuleID);
            }
        }

        /// <summary>AddAdminPages adds an Admin Page and an associated Module to all configured Portals.</summary>
        /// <param name="tabName">The Name to give this new tab.</param>
        /// <param name="description">The page description.</param>
        /// <param name="tabIconFile">The icon for this new tab.</param>
        /// <param name="tabIconFileLarge">The large icon for this new tab.</param>
        /// <param name="isVisible">A flag indicating whether the tab is visible.</param>
        /// <param name="moduleDefId">The Module Definition ID for the module to be added to this tab.</param>
        /// <param name="moduleTitle">The Module's title.</param>
        /// <param name="moduleIconFile">The Module's icon.</param>
        /// <param name="inheritPermissions">Modules Inherit the Pages View Permissions.</param>
        public static void AddAdminPages(string tabName, string description, string tabIconFile, string tabIconFileLarge, bool isVisible, int moduleDefId, string moduleTitle, string moduleIconFile, bool inheritPermissions)
        {
            ArrayList portals = PortalController.Instance.GetPortals();

            // Add Page to Admin Menu of all configured Portals
            for (var index = 0; index <= portals.Count - 1; index++)
            {
                var portal = (PortalInfo)portals[index];

                // Create New Admin Page (or get existing one)
                var newPage = AddAdminPage(portal, tabName, description, tabIconFile, tabIconFileLarge, isVisible);

                // Add Module To Page
                AddModuleToPage(newPage, moduleDefId, moduleTitle, moduleIconFile, inheritPermissions);
            }
        }

        /// <summary>AddAdminPage adds an Admin Tab Page.</summary>
        /// <param name="portal">The Portal.</param>
        /// <param name="tabName">The Name to give this new tab.</param>
        /// <param name="description">The page description.</param>
        /// <param name="tabIconFile">The icon for this new tab.</param>
        /// <param name="tabIconFileLarge">The large icon for this new tab.</param>
        /// <param name="isVisible">A flag indicating whether the tab is visible.</param>
        /// <returns>A <see cref="TabInfo"/> instance or <see langword="null"/>.</returns>
        public static TabInfo AddAdminPage(PortalInfo portal, string tabName, string description, string tabIconFile, string tabIconFileLarge, bool isVisible)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "AddAdminPage:" + tabName);
            TabInfo adminPage = TabController.Instance.GetTab(portal.AdminTabId, portal.PortalID, false);

            if (adminPage != null)
            {
                var tabPermissionCollection = new TabPermissionCollection();
                AddPagePermission(tabPermissionCollection, "View", Convert.ToInt32(portal.AdministratorRoleId));
                AddPagePermission(tabPermissionCollection, "Edit", Convert.ToInt32(portal.AdministratorRoleId));
                return AddPage(adminPage, tabName, description, tabIconFile, tabIconFileLarge, isVisible, tabPermissionCollection, true);
            }

            return null;
        }

        /// <summary>AddHostPage adds a Host Tab Page.</summary>
        /// <param name="tabName">The Name to give this new tab.</param>
        /// <param name="description">The page description.</param>
        /// <param name="tabIconFile">The icon for this new tab.</param>
        /// <param name="tabIconFileLarge">The large icon for this new tab.</param>
        /// <param name="isVisible">A flag indicating whether the tab is visible.</param>
        /// <returns>A <see cref="TabInfo"/> instance or <see langword="null"/>.</returns>
        public static TabInfo AddHostPage(string tabName, string description, string tabIconFile, string tabIconFileLarge, bool isVisible)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "AddHostPage:" + tabName);
            TabInfo hostPage = TabController.Instance.GetTabByName("Host", Null.NullInteger);

            if (hostPage != null)
            {
                return AddPage(hostPage, tabName, description, tabIconFile, tabIconFileLarge, isVisible, new TabPermissionCollection(), true);
            }

            return null;
        }

        /// <summary>AddModuleControl adds a new Module Control to the system.</summary>
        /// <param name="moduleDefId">The Module Definition Id.</param>
        /// <param name="controlKey">The key for this control in the Definition.</param>
        /// <param name="controlTitle">The title of this control.</param>
        /// <param name="controlSrc">The source of ths control.</param>
        /// <param name="iconFile">The icon file.</param>
        /// <param name="controlType">The type of control.</param>
        /// <param name="viewOrder">The vieworder for this module.</param>
        public static void AddModuleControl(int moduleDefId, string controlKey, string controlTitle, string controlSrc, string iconFile, SecurityAccessLevel controlType, int viewOrder)
        {
            // Call Overload with HelpUrl = Null.NullString
            AddModuleControl(moduleDefId, controlKey, controlTitle, controlSrc, iconFile, controlType, viewOrder, Null.NullString);
        }

        /// <summary>AddModuleDefinition adds a new Core Module Definition to the system.</summary>
        /// <remarks>This overload assumes the module is an Admin module and not a Premium Module.</remarks>
        /// <param name="desktopModuleName">The Friendly Name of the Module to Add.</param>
        /// <param name="description">Description of the Module.</param>
        /// <param name="moduleDefinitionName">The Module Definition Name.</param>
        /// <returns>The Module Definition Id of the new Module.</returns>
        public static int AddModuleDefinition(string desktopModuleName, string description, string moduleDefinitionName)
        {
            // Call overload with Premium=False and Admin=True
            return AddModuleDefinition(desktopModuleName, description, moduleDefinitionName, false, true);
        }

        /// <summary>AddModuleToPage adds a module to a Page.</summary>
        /// <param name="page">The Page to add the Module to.</param>
        /// <param name="moduleDefId">The Module Definition Id for the module to be added to this tab.</param>
        /// <param name="moduleTitle">The Module's title.</param>
        /// <param name="moduleIconFile">The Module's icon.</param>
        /// <param name="inheritPermissions">Inherit the Pages View Permissions.</param>
        /// <returns>The ID of the module, or <see cref="Null.NullInteger"/> to indicate failure.</returns>
        public static int AddModuleToPage(TabInfo page, int moduleDefId, string moduleTitle, string moduleIconFile, bool inheritPermissions)
        {
            return AddModuleToPage(page, moduleDefId, moduleTitle, moduleIconFile, inheritPermissions, true, Globals.glbDefaultPane);
        }

        /// <summary>AddModuleToPage adds a module to a Page.</summary>
        /// <param name="page">The Page to add the Module to.</param>
        /// <param name="moduleDefId">The Module Definition ID for the module to be added to this tab.</param>
        /// <param name="moduleTitle">The Module's title.</param>
        /// <param name="moduleIconFile">The Module's icon.</param>
        /// <param name="inheritPermissions">Inherit the Pages View Permissions.</param>
        /// <param name="displayTitle">Whether to display the title.</param>
        /// <param name="paneName">The pane name.</param>
        /// <returns>The ID of the module, or <see cref="Null.NullInteger"/> to indicate failure.</returns>
        public static int AddModuleToPage(TabInfo page, int moduleDefId, string moduleTitle, string moduleIconFile, bool inheritPermissions, bool displayTitle, string paneName)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "AddModuleToPage:" + moduleDefId);
            ModuleInfo moduleInfo;
            int moduleId = Null.NullInteger;

            if (page != null)
            {
                bool isDuplicate = false;
                foreach (var kvp in ModuleController.Instance.GetTabModules(page.TabID))
                {
                    moduleInfo = kvp.Value;
                    if (moduleInfo.ModuleDefID == moduleDefId)
                    {
                        isDuplicate = true;
                        moduleId = moduleInfo.ModuleID;
                    }
                }

                if (!isDuplicate)
                {
                    moduleInfo = new ModuleInfo
                    {
                        ModuleID = Null.NullInteger,
                        PortalID = page.PortalID,
                        TabID = page.TabID,
                        ModuleOrder = -1,
                        ModuleTitle = moduleTitle,
                        PaneName = paneName,
                        ModuleDefID = moduleDefId,
                        CacheTime = 0,
                        IconFile = moduleIconFile,
                        AllTabs = false,
                        Visibility = VisibilityState.None,
                        InheritViewPermissions = inheritPermissions,
                        DisplayTitle = displayTitle,
                    };

                    ModuleController.Instance.InitialModulePermission(moduleInfo, moduleInfo.TabID, inheritPermissions ? 0 : 1);

                    moduleInfo.TabModuleSettings["hideadminborder"] = "True";

                    try
                    {
                        moduleId = ModuleController.Instance.AddModule(moduleInfo);
                    }
                    catch (Exception exc)
                    {
                        Logger.Error(exc);
                        DnnInstallLogger.InstallLogError(exc);
                    }
                }
            }

            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogEnd", Localization.GlobalResourceFile) + "AddModuleToPage:" + moduleDefId);
            return moduleId;
        }

        /// <summary>AddModuleToPage adds a module to a Page.</summary>
        /// <param name="tabPath">The tab path of the page.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="moduleDefId">The Module Definition ID for the module to be added to this tab.</param>
        /// <param name="moduleTitle">The Module's title.</param>
        /// <param name="moduleIconFile">The Module's icon.</param>
        /// <param name="inheritPermissions">Inherit the Pages View Permissions.</param>
        /// <returns>The ID of the module, or <see cref="Null.NullInteger"/> to indicate failure.</returns>
        public static int AddModuleToPage(string tabPath, int portalId, int moduleDefId, string moduleTitle, string moduleIconFile, bool inheritPermissions)
        {
            int moduleId = Null.NullInteger;

            int tabID = TabController.GetTabByTabPath(portalId, tabPath, Null.NullString);
            if (tabID != Null.NullInteger)
            {
                TabInfo tab = TabController.Instance.GetTab(tabID, portalId, true);
                if (tab != null)
                {
                    moduleId = AddModuleToPage(tab, moduleDefId, moduleTitle, moduleIconFile, inheritPermissions);
                }
            }

            return moduleId;
        }

        public static void AddModuleToPages(string tabPath, int moduleDefId, string moduleTitle, string moduleIconFile, bool inheritPermissions)
        {
            var portals = PortalController.Instance.GetPortals();
            foreach (PortalInfo portal in portals)
            {
                int tabID = TabController.GetTabByTabPath(portal.PortalID, tabPath, Null.NullString);
                if (tabID != Null.NullInteger)
                {
                    var tab = TabController.Instance.GetTab(tabID, portal.PortalID, true);
                    if (tab != null)
                    {
                        AddModuleToPage(tab, moduleDefId, moduleTitle, moduleIconFile, inheritPermissions);
                    }
                }
            }
        }

        /// <summary>AddPortal manages the Installation of a new DotNetNuke Portal.</summary>
        /// <param name="node">The portal XML node.</param>
        /// <param name="status">Whether to write status messages to the HTTP response.</param>
        /// <param name="indent">The indentation level of the status messages.</param>
        /// <param name="superUser">The admin user for the portal or <see langword="null"/> to create a new user.</param>
        /// <returns>The ID of the new portal, or <c>-1</c> to indicate failure.</returns>
        public static int AddPortal(XmlNode node, bool status, int indent, UserInfo superUser = null)
        {
            int portalId = -1;
            try
            {
                string hostMapPath = Globals.HostMapPath;
                string childPath = string.Empty;
                string domain = string.Empty;

                if (HttpContext.Current != null)
                {
                    domain = Globals.GetDomainName(HttpContext.Current.Request, true).ToLowerInvariant().Replace("/install", string.Empty);
                }

                DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "AddPortal:" + domain);
                string portalName = XmlUtils.GetNodeValue(node.CreateNavigator(), "portalname");
                if (status)
                {
                    if (HttpContext.Current != null)
                    {
                        HtmlUtils.WriteFeedback(HttpContext.Current.Response, indent, "Creating Site: " + portalName + "<br>");
                    }
                }

                XmlNode adminNode = node.SelectSingleNode("administrator");
                if (adminNode != null)
                {
                    string firstName = XmlUtils.GetNodeValue(adminNode.CreateNavigator(), "firstname");
                    string lastName = XmlUtils.GetNodeValue(adminNode.CreateNavigator(), "lastname");
                    string username = XmlUtils.GetNodeValue(adminNode.CreateNavigator(), "username");
                    string password = XmlUtils.GetNodeValue(adminNode.CreateNavigator(), "password");
                    string email = XmlUtils.GetNodeValue(adminNode.CreateNavigator(), "email");
                    string description = XmlUtils.GetNodeValue(node.CreateNavigator(), "description");
                    string keyWords = XmlUtils.GetNodeValue(node.CreateNavigator(), "keywords");
                    string templateFileName = XmlUtils.GetNodeValue(node.CreateNavigator(), "templatefile");
                    string serverPath = Globals.ApplicationMapPath + "\\";
                    bool isChild = bool.Parse(XmlUtils.GetNodeValue(node.CreateNavigator(), "ischild"));
                    string homeDirectory = XmlUtils.GetNodeValue(node.CreateNavigator(), "homedirectory");

                    // Get the Portal Alias
                    XmlNodeList portalAliases = node.SelectNodes("portalaliases/portalalias");
                    string strPortalAlias = domain;
                    if (portalAliases != null)
                    {
                        if (portalAliases.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(portalAliases[0].InnerText))
                            {
                                strPortalAlias = portalAliases[0].InnerText;
                            }
                        }
                    }

                    // Create default email
                    if (string.IsNullOrEmpty(email))
                    {
                        email = "admin@" + domain.Replace("www.", string.Empty);

                        // Remove any domain subfolder information ( if it exists )
                        if (email.IndexOf("/") != -1)
                        {
                            email = email.Substring(0, email.IndexOf("/"));
                        }
                    }

                    if (isChild)
                    {
                        childPath = PortalController.GetPortalFolder(strPortalAlias);
                    }

                    var template = FindBestTemplate(templateFileName);
                    var userInfo = superUser ?? CreateUserInfo(firstName, lastName, username, password, email);

                    // Create Portal
                    portalId = PortalController.Instance.CreatePortal(
                        portalName,
                        userInfo,
                        description,
                        keyWords,
                        template,
                        homeDirectory,
                        strPortalAlias,
                        serverPath,
                        serverPath + childPath,
                        isChild);

                    if (portalId > -1)
                    {
                        // Add Extra Aliases
                        if (portalAliases != null)
                        {
                            foreach (XmlNode portalAlias in portalAliases)
                            {
                                if (!string.IsNullOrEmpty(portalAlias.InnerText))
                                {
                                    if (status)
                                    {
                                        if (HttpContext.Current != null)
                                        {
                                            HtmlUtils.WriteFeedback(HttpContext.Current.Response, indent, "Creating Site Alias: " + portalAlias.InnerText + "<br>");
                                        }
                                    }

                                    PortalController.Instance.AddPortalAlias(portalId, portalAlias.InnerText);
                                }
                            }
                        }

                        // Force Administrator to Update Password on first log in
                        PortalInfo portal = PortalController.Instance.GetPortal(portalId);
                        UserInfo adminUser = UserController.GetUserById(portalId, portal.AdministratorId);
                        adminUser.Membership.UpdatePassword = true;
                        UserController.UpdateUser(portalId, adminUser);
                    }

                    return portalId;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                if (HttpContext.Current != null)
                {
                    HtmlUtils.WriteFeedback(HttpContext.Current.Response, indent, "<font color='red'>Error!</font> " + ex.Message + ex.StackTrace + "<br>");
                    DnnInstallLogger.InstallLogError(ex);
                }

                // failure
                portalId = -1;
            }

            return portalId;
        }

        /// <summary>Obsolete, AddPortal manages the Installation of a new DotNetNuke Portal.</summary>
        /// <param name="node">The portal XML node.</param>
        /// <param name="status">Whether to write status messages to the HTTP response.</param>
        /// <param name="indent">The indentation level of the status messages.</param>
        /// <returns>The ID of the new portal, or <c>-1</c> to indicate failure.</returns>
        [DnnDeprecated(9, 3, 0, "Use the overloaded method with the 'superUser' parameter instead")]
        public static partial int AddPortal(XmlNode node, bool status, int indent)
        {
            return AddPortal(node, status, indent, null);
        }

        /// <summary>DeleteInstallerFiles - clean up install config and installwizard files. If installwizard is ran again this will be recreated via the dotnetnuke.install.config.resources file.</summary>
        /// <remarks>uses FileSystemUtils.DeleteFile as it checks for readonly attribute status and changes it if required, as well as verifying file exists.</remarks>
        public static void DeleteInstallerFiles()
        {
            var files = new List<string>
            {
                "DotNetNuke.install.config",
                "DotNetNuke.install.config.resources",
                "InstallWizard.aspx",
                "InstallWizard.aspx.cs",
                "InstallWizard.aspx.designer.cs",
                "UpgradeWizard.aspx",
                "UpgradeWizard.aspx.cs",
                "UpgradeWizard.aspx.designer.cs",
                "Install.aspx",
                "Install.aspx.cs",
                "Install.aspx.designer.cs",
            };

            foreach (var file in files)
            {
                try
                {
                    FileSystemUtils.DeleteFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install", file));
                }
                catch (Exception ex)
                {
                    Logger.Error("File deletion failed for [Install\\" + file + "]. PLEASE REMOVE THIS MANUALLY." + ex);
                }
            }
        }

        /// <summary>DeleteFiles - clean up deprecated files and folders.</summary>
        /// <param name="providerPath">Path to provider.</param>
        /// <param name="version">The Version being Upgraded.</param>
        /// <param name="writeFeedback">Display status in UI?.</param>
        /// <returns>Exceptions logged or <see cref="string.Empty"/>.</returns>
        public static string DeleteFiles(string providerPath, Version version, bool writeFeedback)
        {
            var stringVersion = GetStringVersionWithRevision(version);

            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "DeleteFiles:" + stringVersion);
            string exceptions = string.Empty;
            if (writeFeedback)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, "Cleaning Up Files: " + stringVersion);
            }

            string listFile = Globals.InstallMapPath + "Cleanup\\" + stringVersion + ".txt";
            try
            {
                if (File.Exists(listFile))
                {
                    exceptions = FileSystemUtils.DeleteFiles(File.ReadAllLines(listFile));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error cleanup file " + listFile, ex);

                exceptions += $"Error: {ex.Message + ex.StackTrace}{Environment.NewLine}";

                // log the results
                DnnInstallLogger.InstallLogError(exceptions);
                try
                {
                    using (StreamWriter streamWriter = File.CreateText(providerPath + stringVersion + "_Config.log"))
                    {
                        streamWriter.WriteLine(exceptions);
                        streamWriter.Close();
                    }
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }
            }

            if (writeFeedback)
            {
                HtmlUtils.WriteSuccessError(HttpContext.Current.Response, string.IsNullOrEmpty(exceptions));
            }

            return exceptions;
        }

        /// <summary>ExecuteScripts manages the Execution of Scripts from the Install/Scripts folder. It is also triggered by InstallDNN and UpgradeDNN.</summary>
        /// <param name="strProviderPath">The path to the Data Provider.</param>
        public static void ExecuteScripts(string strProviderPath)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "ExecuteScripts:" + strProviderPath);
            string scriptPath = Globals.ApplicationMapPath + "\\Install\\Scripts\\";
            if (Directory.Exists(scriptPath))
            {
                string[] files = Directory.GetFiles(scriptPath);
                foreach (string file in files)
                {
                    // Execute if script is a provider script
                    if (file.IndexOf("." + DefaultProvider) != -1)
                    {
                        ExecuteScript(file, true);

                        // delete the file
                        try
                        {
                            File.SetAttributes(file, FileAttributes.Normal);
                            File.Delete(file);
                        }
                        catch (Exception exc)
                        {
                            Logger.Error(exc);
                        }
                    }
                }
            }
        }

        /// <summary>ExecuteScript executes a special script.</summary>
        /// <param name="file">The script file to execute.</param>
        public static void ExecuteScript(string file)
        {
            // Execute if script is a provider script
            if (file.IndexOf("." + DefaultProvider) != -1)
            {
                ExecuteScript(file, true);
            }
        }

        /// <summary>GetInstallTemplate retrieves the Installation Template as specified in web.config.</summary>
        /// <param name="xmlDoc">The Xml Document to load.</param>
        /// <returns>A string which contains the error message - if appropriate.</returns>
        public static string GetInstallTemplate(XmlDocument xmlDoc)
        {
            string errorMessage = Null.NullString;
            string installTemplate = Config.GetSetting("InstallTemplate");
            try
            {
                xmlDoc.Load(Globals.ApplicationMapPath + "\\Install\\" + installTemplate);
            }
            catch
            {
                // error
                errorMessage = "Failed to load Install template.<br><br>";
            }

            return errorMessage;
        }

        /// <summary>SetInstallTemplate saves the XmlDocument back to Installation Template specified in web.config.</summary>
        /// <param name="xmlDoc">The Xml Document to save.</param>
        /// <returns>A string which contains the error massage - if appropriate.</returns>
        public static string SetInstallTemplate(XmlDocument xmlDoc)
        {
            string errorMessage = Null.NullString;
            string installTemplate = Config.GetSetting("InstallTemplate");
            string filePath = Globals.ApplicationMapPath + "\\Install\\" + installTemplate;
            try
            {
                // ensure the file is not read-only
                var attributes = File.GetAttributes(filePath);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    // file is readonly, then remove it
                    attributes = attributes & ~FileAttributes.ReadOnly;
                    File.SetAttributes(filePath, attributes);
                }

                xmlDoc.Save(filePath);
            }
            catch
            {
                // error
                errorMessage = "Failed to save Install template.<br><br>";
            }

            return errorMessage;
        }

        /// <summary>GetInstallVersion retrieves the Base Install Version as specified in the install template.</summary>
        /// <param name="xmlDoc">The Install Template.</param>
        /// <returns>The <see cref="Version"/>.</returns>
        public static Version GetInstallVersion(XmlDocument xmlDoc)
        {
            string version = Null.NullString;

            // get base version
            XmlNode node = xmlDoc.SelectSingleNode("//dotnetnuke");
            if (node != null)
            {
                version = XmlUtils.GetNodeValue(node.CreateNavigator(), "version");
            }

            return new Version(version);
        }

        /// <summary>GetLogFile gets the filename for the version's log file.</summary>
        /// <param name="providerPath">The path to the Data Provider.</param>
        /// <param name="version">The Version.</param>
        /// <returns>The path to the log file.</returns>
        public static string GetLogFile(string providerPath, Version version)
        {
            return providerPath + GetStringVersion(version) + ".log.resources";
        }

        /// <summary>GetScriptFile gets the filename for the version.</summary>
        /// <param name="providerPath">The path to the Data Provider.</param>
        /// <param name="version">The Version.</param>
        /// <returns>The path to the script file.</returns>
        public static string GetScriptFile(string providerPath, Version version)
        {
            return providerPath + GetStringVersion(version) + "." + DefaultProvider;
        }

        /// <summary>GetStringVersion gets the Version String (xx.xx.xx) from the Version.</summary>
        /// <param name="version">The Version.</param>
        /// <returns>The <paramref name="version"/> formatted as a <see cref="string"/>.</returns>
        public static string GetStringVersion(Version version)
        {
            var versionArray = new int[3];
            versionArray[0] = version.Major;
            versionArray[1] = version.Minor;
            versionArray[2] = version.Build;
            string stringVersion = Null.NullString;
            for (int i = 0; i <= 2; i++)
            {
                if (versionArray[i] == 0)
                {
                    stringVersion += "00";
                }
                else if (versionArray[i] >= 1 && versionArray[i] <= 9)
                {
                    stringVersion += "0" + versionArray[i];
                }
                else
                {
                    stringVersion += versionArray[i].ToString();
                }

                if (i < 2)
                {
                    stringVersion += ".";
                }
            }

            return stringVersion;
        }

        /// <summary>GetSuperUser gets the superuser from the Install Template.</summary>
        /// <param name="xmlTemplate">The install Template.</param>
        /// <param name="writeFeedback">a flag to determine whether to output feedback.</param>
        /// <returns>A <see cref="UserInfo"/> instance or <see langword="null"/>.</returns>
        public static UserInfo GetSuperUser(XmlDocument xmlTemplate, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "GetSuperUser");
            XmlNode node = xmlTemplate.SelectSingleNode("//dotnetnuke/superuser");
            UserInfo superUser = null;
            if (node != null)
            {
                if (writeFeedback)
                {
                    HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Configuring SuperUser:<br>");
                }

                // Parse the SuperUsers nodes
                string firstName = XmlUtils.GetNodeValue(node.CreateNavigator(), "firstname");
                string lastName = XmlUtils.GetNodeValue(node.CreateNavigator(), "lastname");
                string username = XmlUtils.GetNodeValue(node.CreateNavigator(), "username");
                string password = XmlUtils.GetNodeValue(node.CreateNavigator(), "password");
                string email = XmlUtils.GetNodeValue(node.CreateNavigator(), "email");
                string locale = XmlUtils.GetNodeValue(node.CreateNavigator(), "locale");
                string updatePassword = XmlUtils.GetNodeValue(node.CreateNavigator(), "updatepassword");

                superUser = new UserInfo
                {
                    PortalID = -1,
                    FirstName = firstName,
                    LastName = lastName,
                    Username = username,
                    DisplayName = firstName + " " + lastName,
                    Membership = { Password = password },
                    Email = email,
                    IsSuperUser = true,
                };
                superUser.Membership.Approved = true;

                superUser.Profile.FirstName = firstName;
                superUser.Profile.LastName = lastName;
                superUser.Profile.PreferredLocale = locale;
                superUser.Profile.PreferredTimeZone = TimeZoneInfo.Local;

                if (updatePassword.ToLowerInvariant() == "true")
                {
                    superUser.Membership.UpdatePassword = true;
                }
            }

            return superUser;
        }

        /// <summary>GetUpgradeScripts gets an ArrayList of the Scripts required to Upgrade to the current Assembly Version.</summary>
        /// <param name="providerPath">The path to the Data Provider.</param>
        /// <param name="databaseVersion">The current Database Version.</param>
        /// <returns>An <see cref="ArrayList"/> of <see cref="string"/> values, the file paths to the scripts.</returns>
        public static ArrayList GetUpgradeScripts(string providerPath, Version databaseVersion)
        {
            var scriptFiles = new ArrayList();
            string[] files = Directory.GetFiles(providerPath, "*." + DefaultProvider);
            Array.Sort(files); // The order of the returned file names is not guaranteed on certain NAS systems; use the Sort method if a specific sort order is required.

            Logger.TraceFormat("GetUpgradedScripts databaseVersion:{0} applicationVersion:{1}", databaseVersion, ApplicationVersion);

            foreach (string file in files)
            {
                // script file name must conform to ##.##.##.DefaultProviderName
                if (file != null)
                {
                    if (GetFileName(file).Length == 9 + DefaultProvider.Length)
                    {
                        var version = new Version(GetFileNameWithoutExtension(file));

                        // check if script file is relevant for upgrade
                        if (version > databaseVersion && version <= ApplicationVersion && GetFileName(file).Length == 9 + DefaultProvider.Length)
                        {
                            scriptFiles.Add(file);

                            // check if any incrementals exist
                            var incrementalfiles = AddAvailableIncrementalFiles(providerPath, version);
                            if (incrementalfiles != null)
                            {
                                scriptFiles.AddRange(incrementalfiles);
                            }

                            Logger.TraceFormat("GetUpgradedScripts including {0}", file);
                        }

                        if (version == databaseVersion && version <= ApplicationVersion && GetFileName(file).Length == 9 + DefaultProvider.Length)
                        {
                            var incrementalfiles = AddAvailableIncrementalFiles(providerPath, version);
                            if (incrementalfiles != null)
                            {
                                scriptFiles.AddRange(incrementalfiles);
                            }

                            Logger.TraceFormat("GetUpgradedScripts including {0}", file);
                        }

                        // else
                        // {
                        //    Logger.TraceFormat("GetUpgradedScripts excluding {0}", file);
                        // }
                    }
                }
            }

            return scriptFiles;
        }

        /// <summary>InitialiseHostSettings gets the Host Settings from the Install Template.</summary>
        /// <param name="xmlTemplate">The install Template.</param>
        /// <param name="writeFeedback">a flag to determine whether to output feedback.</param>
        public static void InitialiseHostSettings(XmlDocument xmlTemplate, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "InitialiseHostSettings");
            XmlNode node = xmlTemplate.SelectSingleNode("//dotnetnuke/settings");
            if (node != null)
            {
                if (writeFeedback)
                {
                    HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Loading Host Settings:<br>");
                }

                // Need to clear the cache to pick up new HostSettings from the SQLDataProvider script
                DataCache.RemoveCache(DataCache.HostSettingsCacheKey);

                // Parse the Settings nodes
                foreach (XmlNode settingNode in node.ChildNodes)
                {
                    string settingName = settingNode.Name;
                    string settingValue = settingNode.InnerText;
                    if (settingNode.Attributes != null)
                    {
                        XmlAttribute secureAttrib = settingNode.Attributes["Secure"];
                        bool settingIsSecure = false;
                        if (secureAttrib != null)
                        {
                            if (secureAttrib.Value.ToLowerInvariant() == "true")
                            {
                                settingIsSecure = true;
                            }
                        }

                        string domainName = Globals.GetDomainName(HttpContext.Current.Request);

                        switch (settingName)
                        {
                            case "HostURL":
                                if (string.IsNullOrEmpty(settingValue))
                                {
                                    settingValue = domainName;
                                }

                                break;
                            case "HostEmail":
                                if (string.IsNullOrEmpty(settingValue))
                                {
                                    settingValue = "support@" + domainName;

                                    // Remove any folders
                                    settingValue = settingValue.Substring(0, settingValue.IndexOf("/"));

                                    // Remove port number
                                    if (settingValue.IndexOf(":") != -1)
                                    {
                                        settingValue = settingValue.Substring(0, settingValue.IndexOf(":"));
                                    }
                                }

                                break;
                        }

                        HostController.Instance.Update(settingName, settingValue, settingIsSecure);
                    }
                }
            }
        }

        /// <summary>InstallDatabase runs all the "scripts" identified in the Install Template to install the base version.</summary>
        /// <param name="version">The version for which to run installation scripts.</param>
        /// <param name="providerPath">The data provider path.</param>
        /// <param name="xmlDoc">The Xml Document to load.</param>
        /// <param name="writeFeedback">A flag that determines whether to output feedback to the Response Stream.</param>
        /// <returns>A string which contains the error message - if appropriate.</returns>
        public static string InstallDatabase(Version version, string providerPath, XmlDocument xmlDoc, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "InstallDatabase:" + Globals.FormatVersion(version));
            string defaultProvider = Config.GetDefaultProvider("data").Name;
            string message = Null.NullString;

            // Output feedback line
            if (writeFeedback)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Installing Version: " + Globals.FormatVersion(version) + "<br>");
            }

            // Parse the script nodes
            XmlNode node = xmlDoc.SelectSingleNode("//dotnetnuke/scripts");
            if (node != null)
            {
                // Loop through the available scripts
                message = (from XmlNode scriptNode in node.SelectNodes("script") select scriptNode.InnerText + "." + defaultProvider).Aggregate(message, (current, script) => current + ExecuteScript(providerPath + script, writeFeedback));
            }

            // update the version
            Globals.UpdateDataBaseVersion(version);

            // Optionally Install the memberRoleProvider
            message += InstallMemberRoleProvider(providerPath, writeFeedback);

            return message;
        }

        /// <summary>InstallDNN manages the Installation of a new DotNetNuke Application.</summary>
        /// <param name="strProviderPath">The path to the Data Provider.</param>
        public static void InstallDNN(string strProviderPath)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "InstallDNN:" + strProviderPath);
            var xmlDoc = new XmlDocument { XmlResolver = null };

            // open the Install Template XML file
            string errorMessage = GetInstallTemplate(xmlDoc);

            if (string.IsNullOrEmpty(errorMessage))
            {
                // get base version
                Version baseVersion = GetInstallVersion(xmlDoc);

                // Install Base Version
                InstallDatabase(baseVersion, strProviderPath, xmlDoc, true);

                // Call Upgrade with the current DB Version to carry out any incremental upgrades
                UpgradeDNN(strProviderPath, baseVersion);

                // parse Host Settings if available
                InitialiseHostSettings(xmlDoc, true);

                // Create SuperUser only when it's not there (even soft deleted)
                var superUsers = UserController.GetUsers(true, true, Null.NullInteger);
                if (superUsers == null || superUsers.Count == 0)
                {
                    // parse SuperUser if Available
                    UserInfo superUser = GetSuperUser(xmlDoc, true);
                    UserController.CreateUser(ref superUser);
                    superUsers.Add(superUser);
                }

                // parse File List if available
                InstallFiles(xmlDoc, true);

                // Run any addition scripts in the Scripts folder
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Executing Additional Scripts:<br>");
                ExecuteScripts(strProviderPath);

                // Install optional resources if present
                var packages = GetInstallPackages();
                foreach (var package in packages)
                {
                    InstallPackage(package.Key, package.Value.PackageType, true);
                }

                // Set Status to None
                Globals.SetStatus(Globals.UpgradeStatus.None);

                // download LP (and templates) if not using en-us
                var ensureLpAndTemplate = new UpdateLanguagePackStep();
                ensureLpAndTemplate.Execute();

                // install LP that contains templates if installing in a different language
                var installConfig = InstallController.Instance.GetInstallConfig();
                string culture = installConfig.InstallCulture;
                if (!culture.Equals("en-us", StringComparison.InvariantCultureIgnoreCase))
                {
                    string installFolder = HttpContext.Current.Server.MapPath("~/Install/language");
                    string lpAndTemplates = $@"{installFolder}\installlanguage.resources";

                    if (File.Exists(lpAndTemplates))
                    {
                        InstallPackage(lpAndTemplates, "Language", false);
                    }
                }

                // parse portal(s) if available
                XmlNodeList nodes = xmlDoc.SelectNodes("//dotnetnuke/portals/portal");
                if (nodes != null)
                {
                    foreach (XmlNode node in nodes)
                    {
                        if (node != null)
                        {
                            // add item to identity install from install wizard.
                            if (HttpContext.Current != null)
                            {
                                HttpContext.Current.Items.Add("InstallFromWizard", true);
                            }

                            var portalHost = superUsers[0] as UserInfo;
                            int portalId = AddPortal(node, true, 2, portalHost);
                            if (portalId > -1)
                            {
                                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, "<font color='green'>Successfully Installed Site " + portalId + ":</font><br>");
                            }
                            else
                            {
                                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, "<font color='red'>Site failed to install:Error!</font><br>");
                            }
                        }
                    }
                }
            }
            else
            {
                // 500 Error - Redirect to ErrorPage
                if (HttpContext.Current != null)
                {
                    string url = "~/ErrorPage.aspx?status=500&error=" + errorMessage;
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Server.Transfer(url);
                }
            }
        }

        /// <summary>InstallFiles installs any files listed in the Host Install Configuration file.</summary>
        /// <param name="xmlDoc">The Xml Document to load.</param>
        /// <param name="writeFeedback">A flag that determines whether to output feedback to the Response Stream.</param>
        public static void InstallFiles(XmlDocument xmlDoc, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "InstallFiles");

            // Parse the file nodes
            XmlNode node = xmlDoc.SelectSingleNode("//dotnetnuke/files");
            if (node != null)
            {
                if (writeFeedback)
                {
                    HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Loading Host Files:<br>");
                }

                ParseFiles(node, Null.NullInteger);
            }

            // Synchronise Host Folder
            if (writeFeedback)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Synchronizing Host Files:<br>");
            }

            FolderManager.Instance.Synchronize(Null.NullInteger, string.Empty, true, true);
        }

        public static bool InstallPackage(string file, string packageType, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "InstallPackage:" + file);
            bool success = Null.NullBoolean;
            if (writeFeedback)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, "Installing Package File " + Path.GetFileNameWithoutExtension(file) + ": ");
            }

            bool deleteTempFolder = true;
            if (packageType == "Skin" || packageType == "Container")
            {
                deleteTempFolder = Null.NullBoolean;
            }

            var installer = new Installer(new FileStream(file, FileMode.Open, FileAccess.Read), Globals.ApplicationMapPath, true, deleteTempFolder);

            // Check if manifest is valid
            if (installer.IsValid)
            {
                installer.InstallerInfo.RepairInstall = true;
                success = installer.Install();
            }
            else
            {
                if (installer.InstallerInfo.ManifestFile == null)
                {
                    // Missing manifest
                    if (packageType == "Skin" || packageType == "Container")
                    {
                        // Legacy Skin/Container
                        string tempInstallFolder = installer.TempInstallFolder;
                        string manifestFile = Path.Combine(tempInstallFolder, Path.GetFileNameWithoutExtension(file) + ".dnn");
                        using (var manifestWriter = new StreamWriter(manifestFile))
                        {
                            manifestWriter.Write(LegacyUtil.CreateSkinManifest(file, packageType, tempInstallFolder));
                        }

                        installer = new Installer(tempInstallFolder, manifestFile, HttpContext.Current.Request.MapPath("."), true);

                        // Set the Repair flag to true for Batch Install
                        installer.InstallerInfo.RepairInstall = true;

                        success = installer.Install();
                    }
                    else if (Globals.Status != Globals.UpgradeStatus.None)
                    {
                        var message = string.Format(Localization.GetString("InstallPackageError", Localization.ExceptionsResourceFile), file, "Manifest file missing");
                        DnnInstallLogger.InstallLogError(message);
                    }
                }
                else
                {
                    // log the failure log when installer is invalid and not caught by mainfest file missing.
                    foreach (var log in installer.InstallerInfo.Log.Logs
                                                .Where(l => l.Type == LogType.Failure))
                    {
                        Logger.Error(log.Description);
                        DnnInstallLogger.InstallLogError(log.Description);
                    }

                    success = false;
                }
            }

            if (writeFeedback)
            {
                HtmlUtils.WriteSuccessError(HttpContext.Current.Response, success);
            }

            if (success)
            {
                // delete file
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }
            }

            return success;
        }

        /// <summary>Gets a list of installable extensions sorted to ensure dependencies are installed first.</summary>
        /// <returns>An <see cref="IDictionary{TKey,TValue}"/> where the key is the path to the package and the value is the <see cref="PackageInfo"/> instance.</returns>
        public static IDictionary<string, PackageInfo> GetInstallPackages()
        {
            var packageTypes = new string[] { "Library", "Module", "Skin", "Container", "JavaScriptLibrary", "Language", "Provider", "AuthSystem", "Package" };
            var invalidPackages = new List<string>();

            var packages = new Dictionary<string, PackageInfo>();

            ParsePackagesFromApplicationPath(packageTypes, packages, invalidPackages);

            // Add packages with no dependency requirements
            var sortedPackages = packages.Where(p => p.Value.Dependencies.Count == 0).ToDictionary(p => p.Key, p => p.Value);

            var prevDependentCount = -1;

            var dependentPackages = packages.Where(p => p.Value.Dependencies.Count > 0).ToDictionary(p => p.Key, p => p.Value);
            var dependentCount = dependentPackages.Count;
            while (dependentCount != prevDependentCount)
            {
                prevDependentCount = dependentCount;
                var addedPackages = new List<string>();
                foreach (var package in dependentPackages)
                {
                    if (package.Value.Dependencies.All(
                            d => sortedPackages.Any(p => p.Value.Name.Equals(d.PackageName, StringComparison.OrdinalIgnoreCase) && p.Value.Version >= d.Version)))
                    {
                        sortedPackages.Add(package.Key, package.Value);
                        addedPackages.Add(package.Key);
                    }
                }

                foreach (var packageKey in addedPackages)
                {
                    dependentPackages.Remove(packageKey);
                }

                dependentCount = dependentPackages.Count;
            }

            // Add any packages whose dependency cannot be resolved
            foreach (var package in dependentPackages)
            {
                sortedPackages.Add(package.Key, package.Value);
            }

            return sortedPackages;
        }

        public static void InstallPackages(string packageType, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "InstallPackages:" + packageType);
            if (writeFeedback)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Installing Optional " + packageType + "s:<br>");
            }

            string installPackagePath = Globals.ApplicationMapPath + "\\Install\\" + packageType;
            if (Directory.Exists(installPackagePath))
            {
                foreach (string file in Directory.GetFiles(installPackagePath))
                {
                    if (Path.GetExtension(file.ToLowerInvariant()) == ".zip" /*|| installLanguage */)
                    {
                        InstallPackage(file, packageType, writeFeedback);
                    }
                }
            }
        }

        public static bool IsNETFrameworkCurrent(string version)
        {
            bool isCurrent = Null.NullBoolean;
            switch (version)
            {
                case "3.5":
                    // Try and instantiate a 3.5 Class
                    if (Reflection.CreateType("System.Data.Linq.DataContext", true) != null)
                    {
                        isCurrent = true;
                    }

                    break;
                case "4.0":
                    // Look for requestValidationMode attribute
                    XmlDocument configFile = Config.Load();
                    XPathNavigator configNavigator = configFile.CreateNavigator().SelectSingleNode("//configuration/system.web/httpRuntime|//configuration/location/system.web/httpRuntime");
                    if (configNavigator != null && !string.IsNullOrEmpty(configNavigator.GetAttribute("requestValidationMode", string.Empty)))
                    {
                        isCurrent = true;
                    }

                    break;
                case "4.5":
                    // Try and instantiate a 4.5 Class
                    if (Reflection.CreateType("System.Reflection.ReflectionContext", true) != null)
                    {
                        isCurrent = true;
                    }

                    break;
            }

            return isCurrent;
        }

        public static void RemoveAdminPages(string tabPath)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "RemoveAdminPages:" + tabPath);

            var portals = PortalController.Instance.GetPortals();
            foreach (IPortalInfo portal in portals)
            {
                var tabID = TabController.GetTabByTabPath(portal.PortalId, tabPath, Null.NullString);
                if (tabID != Null.NullInteger)
                {
                    TabController.Instance.DeleteTab(tabID, portal.PortalId);
                }
            }
        }

        public static void RemoveHostPage(string pageName)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "RemoveHostPage:" + pageName);
            var hostTabId = TabController.GetTabByTabPath(Null.NullInteger, "//Host", Null.NullString);
            if (hostTabId == Null.NullInteger)
            {
                return;
            }

            var tabPath = Globals.GenerateTabPath(hostTabId, pageName);

            var hostModuleTabId = TabController.GetTabByTabPath(Null.NullInteger, tabPath, Null.NullString);
            if (hostModuleTabId != Null.NullInteger)
            {
                TabController.Instance.DeleteTab(hostModuleTabId, Null.NullInteger);
            }
        }

        public static void StartTimer()
        {
            // Start Upgrade Timer
            startTime = DateTime.Now;
        }

        /// <summary>UpgradeApplication - This overload is used for general application upgrade operations.</summary>
        /// <remarks>Since it is not version specific and is invoked whenever the application is restarted, the operations must be re-executable.</remarks>
        public static void UpgradeApplication()
        {
            try
            {
                // Remove UpdatePanel from Login Control - not neccessary in popup.
                var loginControl = ModuleControlController.GetModuleControlByControlKey("Login", -1);
                loginControl.SupportsPartialRendering = false;

                ModuleControlController.SaveModuleControl(loginControl, true);

                // Update the version of the client resources - so the cache is cleared
                DataCache.ClearHostCache(false);
                HostController.Instance.IncrementCrmVersion(true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                var log = new LogInfo
                {
                    LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString(),
                    BypassBuffering = true,
                };
                log.AddProperty("Upgraded DotNetNuke", "General");
                log.AddProperty("Warnings", "Error: " + ex.Message + Environment.NewLine);
                LogController.Instance.AddLog(log);
                try
                {
                    Exceptions.Exceptions.LogException(ex);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }
            }

            // Remove any .txt and .config files that may exist in the Install folder
            foreach (string file in Directory.GetFiles(Globals.InstallMapPath + "Cleanup\\", "??.??.??.txt")
                                        .Concat(Directory.GetFiles(Globals.InstallMapPath + "Cleanup\\", "??.??.??.??.txt")))
            {
                FileSystemUtils.DeleteFile(file);
            }

            foreach (string file in Directory.GetFiles(Globals.InstallMapPath + "Config\\", "??.??.??.config")
                                        .Concat(Directory.GetFiles(Globals.InstallMapPath + "Config\\", "??.??.??.??.config")))
            {
                FileSystemUtils.DeleteFile(file);
            }
        }

        /// <summary>UpgradeApplication - This overload is used for version specific application upgrade operations.</summary>
        /// <remarks>
        ///  This should be used for file system modifications or upgrade operations which
        ///  should only happen once. Database references are not recommended because future
        ///  versions of the application may result in code incompatibilities.
        /// </remarks>
        /// <param name="providerPath">The provider path to which log files will be written.</param>
        /// <param name="version">The version of the upgrade.</param>
        /// <param name="writeFeedback">Whether to write feedback messages to the HTTP response.</param>
        /// <returns>Exceptions logged or <see cref="string.Empty"/>.</returns>
        public static string UpgradeApplication(string providerPath, Version version, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + Localization.GetString("ApplicationUpgrades", Localization.GlobalResourceFile) + ": " + version.ToString(3));
            string exceptions = string.Empty;
            if (writeFeedback)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, Localization.GetString("ApplicationUpgrades", Localization.GlobalResourceFile) + " : " + GetStringVersionWithRevision(version));
            }

            try
            {
                switch (version.ToString(3))
                {
                    case "10.0.0":
                        UpgradeToVersion10_0_0();
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                exceptions += string.Format("Error: {0}{1}", ex.Message + ex.StackTrace, Environment.NewLine);

                // log the results
                if (string.IsNullOrEmpty(exceptions))
                {
                    DnnInstallLogger.InstallLogInfo(Localization.GetString("LogEnd", Localization.GlobalResourceFile) + Localization.GetString("ApplicationUpgrades", Localization.GlobalResourceFile) + ": " + version.ToString(3));
                }
                else
                {
                    DnnInstallLogger.InstallLogError(exceptions);
                }

                try
                {
                    using (StreamWriter streamWriter = File.CreateText(providerPath + Globals.FormatVersion(version) + "_Application.log.resources"))
                    {
                        streamWriter.WriteLine(exceptions);
                        streamWriter.Close();
                    }
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }
            }

            if (writeFeedback)
            {
                HtmlUtils.WriteSuccessError(HttpContext.Current.Response, string.IsNullOrEmpty(exceptions));
            }

            return exceptions;
        }

        public static string UpdateConfig(string providerPath, Version version, bool writeFeedback)
        {
            var stringVersion = GetStringVersionWithRevision(version);

            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "UpdateConfig:" + stringVersion);
            if (writeFeedback)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, $"Updating Config Files: {stringVersion}");
            }

            string strExceptions = UpdateConfig(providerPath, Globals.InstallMapPath + "Config\\" + stringVersion + ".config", version, "Core Upgrade");
            if (string.IsNullOrEmpty(strExceptions))
            {
                DnnInstallLogger.InstallLogInfo(Localization.GetString("LogEnd", Localization.GlobalResourceFile) + "UpdateConfig:" + stringVersion);
            }
            else
            {
                DnnInstallLogger.InstallLogError(strExceptions);
            }

            if (writeFeedback)
            {
                HtmlUtils.WriteSuccessError(HttpContext.Current.Response, string.IsNullOrEmpty(strExceptions));
            }

            return strExceptions;
        }

        public static string UpdateConfig(string configFile, Version version, string reason)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "UpdateConfig:" + version.ToString(3));
            string exceptions = string.Empty;
            if (File.Exists(configFile))
            {
                // Create XmlMerge instance from config file source
                StreamReader stream = File.OpenText(configFile);
                try
                {
                    var merge = new XmlMerge(stream, version.ToString(3), reason);

                    // Process merge
                    merge.UpdateConfigs();
                }
                catch (Exception ex)
                {
                    exceptions += string.Format("Error: {0}{1}", ex.Message + ex.StackTrace, Environment.NewLine);
                    Exceptions.Exceptions.LogException(ex);
                }
                finally
                {
                    // Close stream
                    stream.Close();
                }
            }

            if (string.IsNullOrEmpty(exceptions))
            {
                DnnInstallLogger.InstallLogInfo(Localization.GetString("LogEnd", Localization.GlobalResourceFile) + "UpdateConfig:" + version.ToString(3));
            }
            else
            {
                DnnInstallLogger.InstallLogError(exceptions);
            }

            return exceptions;
        }

        public static string UpdateConfig(string providerPath, string configFile, Version version, string reason)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "UpdateConfig:" + version.ToString(3));
            string exceptions = string.Empty;
            if (File.Exists(configFile))
            {
                // Create XmlMerge instance from config file source
                StreamReader stream = File.OpenText(configFile);
                try
                {
                    var merge = new XmlMerge(stream, version.ToString(3), reason);

                    // Process merge
                    merge.UpdateConfigs();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    exceptions += string.Format("Error: {0}{1}", ex.Message + ex.StackTrace, Environment.NewLine);

                    // log the results
                    try
                    {
                        using (StreamWriter streamWriter = File.CreateText(providerPath + Globals.FormatVersion(version) + "_Config.log"))
                        {
                            streamWriter.WriteLine(exceptions);
                            streamWriter.Close();
                        }
                    }
                    catch (Exception exc)
                    {
                        Logger.Error(exc);
                    }
                }
                finally
                {
                    // Close stream
                    stream.Close();
                }
            }

            if (string.IsNullOrEmpty(exceptions))
            {
                DnnInstallLogger.InstallLogInfo(Localization.GetString("LogEnd", Localization.GlobalResourceFile) + "UpdateConfig:" + version.ToString(3));
            }
            else
            {
                DnnInstallLogger.InstallLogError(exceptions);
            }

            return exceptions;
        }

        /// <summary>UpgradeDNN manages the Upgrade of an existing DotNetNuke Application.</summary>
        /// <param name="providerPath">The path to the Data Provider.</param>
        /// <param name="dataBaseVersion">The current Database Version.</param>
        public static void UpgradeDNN(string providerPath, Version dataBaseVersion)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "UpgradeDNN:" + Globals.FormatVersion(ApplicationVersion));
            HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Upgrading to Version: " + Globals.FormatVersion(ApplicationVersion) + "<br/>");

            // Process the Upgrade Script files
            var versions = new List<Version>();
            foreach (string scriptFile in GetUpgradeScripts(providerPath, dataBaseVersion))
            {
                var version = new Version(GetFileNameWithoutExtension(scriptFile));
                bool scriptExecuted;
                UpgradeVersion(scriptFile, true, out scriptExecuted);
                if (scriptExecuted)
                {
                    versions.Add(version);
                }
            }

            foreach (Version ver in versions)
            {
                // ' perform version specific application upgrades
                UpgradeApplication(providerPath, ver, true);
            }

            foreach (Version ver in versions)
            {
                // delete files which are no longer used
                DeleteFiles(providerPath, ver, true);
            }

            foreach (Version ver in versions)
            {
                // execute config file updates
                UpdateConfig(providerPath, ver, true);
            }

            // Removing ClientDependency Resources config from web.config
            ClientResourceManager.RemoveConfiguration();

            DataProvider.Instance().SetCorePackageVersions();

            // perform general application upgrades
            HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Performing General Upgrades<br>");
            DnnInstallLogger.InstallLogInfo(Localization.GetString("GeneralUpgrades", Localization.GlobalResourceFile));
            UpgradeApplication();

            DataCache.ClearHostCache(true);
        }

        /// <summary>Gets a URL for an image which indicates the latest known version of DNN.</summary>
        /// <param name="version">The package version.</param>
        /// <param name="isLocal">Whether the request is local.</param>
        /// <param name="isSecureConnection">Whether the request is over a secure connection.</param>
        /// <returns>An image URL or <see cref="string.Empty"/>.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial string UpgradeIndicator(Version version, bool isLocal, bool isSecureConnection)
            => UpgradeIndicator(null, null, null, version, isLocal, isSecureConnection);

        /// <summary>Gets a URL for an image which indicates the latest known version of DNN.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="version">The package version.</param>
        /// <param name="isLocal">Whether the request is local.</param>
        /// <param name="isSecureConnection">Whether the request is over a secure connection.</param>
        /// <returns>An image URL or <see cref="string.Empty"/>.</returns>
        public static string UpgradeIndicator(IHostSettings hostSettings, IHostSettingsService hostSettingsService, IPortalController portalController, Version version, bool isLocal, bool isSecureConnection)
        {
            return UpgradeIndicator(hostSettings, hostSettingsService, portalController, version, DotNetNukeContext.Current.Application.Type, DotNetNukeContext.Current.Application.Name, string.Empty, isLocal, isSecureConnection);
        }

        /// <summary>Gets a URL for an image which indicates the latest known version of the given package.</summary>
        /// <param name="version">The package version.</param>
        /// <param name="packageType">The package type.</param>
        /// <param name="packageName">The package name.</param>
        /// <param name="culture">The culture code.</param>
        /// <param name="isLocal">Whether the request is local.</param>
        /// <param name="isSecureConnection">Whether the request is over a secure connection.</param>
        /// <returns>An image URL or <see cref="string.Empty"/>.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial string UpgradeIndicator(Version version, string packageType, string packageName, string culture, bool isLocal, bool isSecureConnection)
            => UpgradeIndicator(null, null, null, version, packageType, packageName, culture, isLocal, isSecureConnection);

        /// <summary>Gets a URL for an image which indicates the latest known version of the given package.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="version">The package version.</param>
        /// <param name="packageType">The package type.</param>
        /// <param name="packageName">The package name.</param>
        /// <param name="culture">The culture code.</param>
        /// <param name="isLocal">Whether the request is local.</param>
        /// <param name="isSecureConnection">Whether the request is over a secure connection.</param>
        /// <returns>An image URL or <see cref="string.Empty"/>.</returns>
        public static string UpgradeIndicator(IHostSettings hostSettings, IHostSettingsService hostSettingsService, IPortalController portalController, Version version, string packageType, string packageName, string culture, bool isLocal, bool isSecureConnection)
        {
            hostSettings ??= Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            hostSettingsService ??= Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
            portalController ??= Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();

            string url = string.Empty;
            if (hostSettings.CheckUpgrade && version != new Version(0, 0, 0))
            {
                url = DotNetNukeContext.Current.Application.UpgradeUrl + "/update.aspx";

                // use network path reference so it works in ssl-offload scenarios
                url = url.Replace("http://", "//");
                url += "?core=" + Globals.FormatVersion(Assembly.GetExecutingAssembly().GetName().Version, "00", 3, string.Empty);
                url += "&version=" + Globals.FormatVersion(version, "00", 3, string.Empty);
                url += "&type=" + packageType;
                url += "&name=" + packageName;
                if (packageType.ToLowerInvariant() == "module")
                {
                    var moduleType = (from m in InstalledModulesController.GetInstalledModules() where m.ModuleName == packageName select m).SingleOrDefault();
                    if (moduleType != null)
                    {
                        url += "&no=" + moduleType.Instances;
                    }
                }

                url += "&id=" + hostSettings.Guid;
                if (packageType.Equals(DotNetNukeContext.Current.Application.Type, StringComparison.OrdinalIgnoreCase))
                {
                    var newsletterSubscribeEmail = hostSettingsService.GetString("NewsletterSubscribeEmail");
                    if (!string.IsNullOrEmpty(newsletterSubscribeEmail))
                    {
                        url += "&email=" + HttpUtility.UrlEncode(newsletterSubscribeEmail);
                    }

                    var portals = portalController.GetPortals();
                    url += "&no=" + portals.Count;
                    url += "&os=" + Globals.FormatVersion(Globals.OperatingSystemVersion, "00", 2, string.Empty);
                    url += "&net=" + Globals.FormatVersion(Globals.NETFrameworkVersion, "00", 2, string.Empty);
                    url += "&db=" + Globals.FormatVersion(Globals.DatabaseEngineVersion, "00", 2, string.Empty);
                    var source = Config.GetSetting("Source");
                    if (!string.IsNullOrEmpty(source))
                    {
                        url += "&src=" + source;
                    }
                }

                if (!string.IsNullOrEmpty(culture))
                {
                    url += "&culture=" + culture;
                }
            }

            return url;
        }

        public static string UpgradeRedirect()
        {
            return UpgradeRedirect(ApplicationVersion, DotNetNukeContext.Current.Application.Type, DotNetNukeContext.Current.Application.Name, string.Empty);
        }

        public static string UpgradeRedirect(Version version, string packageType, string packageName, string culture)
        {
            string url;
            if (!string.IsNullOrEmpty(Config.GetSetting("UpdateServiceRedirect")))
            {
                url = Config.GetSetting("UpdateServiceRedirect");
            }
            else
            {
                url = DotNetNukeContext.Current.Application.UpgradeUrl + "/redirect.aspx";
                url += "?core=" + Globals.FormatVersion(Assembly.GetExecutingAssembly().GetName().Version, "00", 3, string.Empty);
                url += "&version=" + Globals.FormatVersion(version, "00", 3, string.Empty);
                url += "&type=" + packageType;
                url += "&name=" + packageName;
                if (!string.IsNullOrEmpty(culture))
                {
                    url += "&culture=" + culture;
                }
            }

            return url;
        }

        /// <summary>UpgradeVersion upgrades a single version.</summary>
        /// <param name="scriptFile">The upgrade script file.</param>
        /// <param name="writeFeedback">Write status to Response Stream?.</param>
        /// <returns>Exceptions logged or <see cref="string.Empty"/>.</returns>
        public static string UpgradeVersion(string scriptFile, bool writeFeedback)
        {
            bool scriptExecuted;
            return UpgradeVersion(scriptFile, writeFeedback, out scriptExecuted);
        }

        /// <summary>UpgradeVersion upgrades a single version.</summary>
        /// <param name="scriptFile">The upgrade script file.</param>
        /// <param name="writeFeedback">Write status to Response Stream?.</param>
        /// <param name="scriptExecuted">Identity whether the script file executed.</param>
        /// <returns>Exceptions logged or <see cref="string.Empty"/>.</returns>
        public static string UpgradeVersion(string scriptFile, bool writeFeedback, out bool scriptExecuted)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "UpgradeVersion:" + scriptFile);
            var version = new Version(GetFileNameWithoutExtension(scriptFile));
            string exceptions = Null.NullString;
            scriptExecuted = false;

            // verify script has not already been run
            if (!Globals.FindDatabaseVersion(version.Major, version.Minor, version.Build))
            {
                // execute script file (and version upgrades) for version
                exceptions = ExecuteScript(scriptFile, writeFeedback);
                scriptExecuted = true;

                // update the version
                Globals.UpdateDataBaseVersion(version);

                var log = new LogInfo
                {
                    LogTypeKey = nameof(DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT),
                    BypassBuffering = true,
                };
                log.AddProperty("Upgraded DotNetNuke", "Version: " + Globals.FormatVersion(version));
                if (exceptions.Length > 0)
                {
                    log.AddProperty("Warnings", exceptions);
                }
                else
                {
                    log.AddProperty("No Warnings", string.Empty);
                }

                LogController.Instance.AddLog(log);
            }

            if (version.Revision > 0 &&
                version.Revision > Globals.GetLastAppliedIteration(version))
            {
                // execute script file (and version upgrades) for version
                exceptions = ExecuteScript(scriptFile, writeFeedback);
                scriptExecuted = true;

                // update the increment
                Globals.UpdateDataBaseVersionIncrement(version, version.Revision);

                var log = new LogInfo
                {
                    LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString(),
                    BypassBuffering = true,
                };
                log.AddProperty("Upgraded DotNetNuke", "Version: " + Globals.FormatVersion(version) + ", Iteration:" + version.Revision);
                if (exceptions.Length > 0)
                {
                    log.AddProperty("Warnings", exceptions);
                }
                else
                {
                    log.AddProperty("No Warnings", string.Empty);
                }

                LogController.Instance.AddLog(log);
            }

            if (string.IsNullOrEmpty(exceptions))
            {
                DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "UpgradeVersion:" + scriptFile);
            }
            else
            {
                DnnInstallLogger.InstallLogError(exceptions);
            }

            return exceptions;
        }

        public static bool UpdateNewtonsoftVersion()
        {
            try
            {
                // check whether current binding already specific to correct version.
                if (NewtonsoftNeedUpdate())
                {
                    lock (ThreadLocker)
                    {
                        if (NewtonsoftNeedUpdate())
                        {
                            var matchedFiles = Directory.GetFiles(Path.Combine(Globals.ApplicationMapPath, "Install\\Module"), "Newtonsoft.Json_*_Install.zip");
                            if (matchedFiles.Length > 0)
                            {
                                return InstallPackage(matchedFiles[0], "Library", false);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return false;
        }

        public static bool RemoveInvalidAntiForgeryCookie()
        {
            // DNN-9394: when upgrade from old version which use MVC version below than 5, it may have saved the antiforgery cookie
            // with a different cookie name which join the root path even equals to "/", then it will cause API request failed.
            // we need remove the cookie during upgrade process.
            var appPath = HttpRuntime.AppDomainAppVirtualPath;
            if (appPath != "/" || HttpContext.Current == null)
            {
                return false;
            }

            var cookieSuffix = Convert.ToBase64String(Encoding.UTF8.GetBytes(appPath)).Replace('+', '.').Replace('/', '-').Replace('=', '_');
            var cookieName = $"__RequestVerificationToken_{cookieSuffix}";
            var invalidCookie = HttpContext.Current.Request.Cookies[cookieName];
            if (invalidCookie == null)
            {
                return false;
            }

            invalidCookie.Expires = DateTime.Now.AddYears(-1);
            HttpContext.Current.Response.Cookies.Add(invalidCookie);

            return true;
        }

        /// <summary>ExecuteScript executes a SQL script file.</summary>
        /// <param name="scriptFile">The script to Execute.</param>
        /// <param name="writeFeedback">Need to output feedback message.</param>
        /// <returns>Exceptions logged or <see cref="string.Empty"/>.</returns>
        internal static string ExecuteScript(string scriptFile, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "ExecuteScript:" + scriptFile);
            if (writeFeedback)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, Localization.GetString("ExecutingScript", Localization.GlobalResourceFile) + ":" + Path.GetFileName(scriptFile));
            }

            // read script file for installation
            string script = FileSystemUtils.ReadFile(scriptFile);

            // execute SQL installation script
            string exceptions = DataProvider.Instance().ExecuteScript(script);

            // add installer logging
            if (string.IsNullOrEmpty(exceptions))
            {
                DnnInstallLogger.InstallLogInfo(Localization.GetString("LogEnd", Localization.GlobalResourceFile) + "ExecuteScript:" + scriptFile);
            }
            else
            {
                DnnInstallLogger.InstallLogError(exceptions);
            }

            // log the results
            try
            {
                var logName = scriptFile.Replace("." + DefaultProvider, string.Empty);
                using var streamWriter = File.CreateText($"{logName}.log.resources");
                streamWriter.WriteLine(exceptions);
                streamWriter.Close();
            }
            catch (Exception exc)
            {
                // does not have permission to create the log file
                Logger.Error(exc);
            }

            if (!writeFeedback)
            {
                return exceptions;
            }

            string resourcesFile = Path.GetFileName(scriptFile);
            if (!string.IsNullOrEmpty(resourcesFile))
            {
                HtmlUtils.WriteScriptSuccessError(HttpContext.Current.Response, string.IsNullOrEmpty(exceptions), resourcesFile.Replace($".{DefaultProvider}", ".log.resources"));
            }

            return exceptions;
        }

        /// <summary>InstallMemberRoleProvider - Installs the MemberRole Provider Db objects.</summary>
        /// <param name="providerPath">The Path to the Provider Directory.</param>
        /// <param name="writeFeedback">Whether need to output feedback message.</param>
        /// <returns>Exceptions logged or <see cref="string.Empty"/>.</returns>
        internal static string InstallMemberRoleProvider(string providerPath, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "InstallMemberRoleProvider");

            string exceptions = string.Empty;

            bool installMemberRole = true;
            if (Config.GetSetting("InstallMemberRole") != null)
            {
                installMemberRole = bool.Parse(Config.GetSetting("InstallMemberRole"));
            }

            if (installMemberRole)
            {
                if (writeFeedback)
                {
                    HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Installing MemberRole Provider:<br>");
                }

                // Install Common
                exceptions += InstallMemberRoleProviderScript(providerPath, "InstallCommon", writeFeedback);

                // Install Membership
                exceptions += InstallMemberRoleProviderScript(providerPath, "InstallMembership", writeFeedback);

                // Install Profile
                // exceptions += InstallMemberRoleProviderScript(providerPath, "InstallProfile", writeFeedback);
                // Install Roles
                // exceptions += InstallMemberRoleProviderScript(providerPath, "InstallRoles", writeFeedback);
            }

            if (string.IsNullOrEmpty(exceptions))
            {
                DnnInstallLogger.InstallLogInfo(Localization.GetString("LogEnd", Localization.GlobalResourceFile) + "InstallMemberRoleProvider");
            }
            else
            {
                DnnInstallLogger.InstallLogError(exceptions);
            }

            return exceptions;
        }

        internal static UserInfo CreateUserInfo(string firstName, string lastName, string userName, string password, string email)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "CreateUserInfo:" + userName);
            return new UserInfo
            {
                FirstName = firstName,
                LastName = lastName,
                Username = userName,
                DisplayName = firstName + " " + lastName,
                Membership = { Password = password, Approved = true, UpdatePassword = true },
                Email = email,
                IsSuperUser = false,
                Profile = { FirstName = firstName, LastName = lastName, },
            };
        }

        internal static IPortalTemplateInfo FindBestTemplate(string templateFileName, string currentCulture)
        {
            if (string.IsNullOrEmpty(currentCulture))
            {
                currentCulture = Localization.SystemLocale;
            }

            var templates = PortalTemplateController.Instance.GetPortalTemplates();

            var defaultTemplates =
                templates.Where(x => Path.GetFileName(x.TemplateFilePath) == templateFileName).ToList();

            return defaultTemplates.FirstOrDefault(x => x.CultureCode.ToLowerInvariant() == currentCulture) ??
                   defaultTemplates.FirstOrDefault(x => x.CultureCode.ToLowerInvariant().StartsWith(currentCulture.Substring(0, 2))) ??
                   defaultTemplates.FirstOrDefault(x => string.IsNullOrEmpty(x.CultureCode)) ??
                   throw new TemplateNotFoundException("Unable to locate specified portal template: " + templateFileName);
        }

        internal static IPortalTemplateInfo FindBestTemplate(string templateFileName)
        {
            // Load Template
            var installTemplate = new XmlDocument { XmlResolver = null };
            Upgrade.GetInstallTemplate(installTemplate);

            // Parse the root node
            XmlNode rootNode = installTemplate.SelectSingleNode("//dotnetnuke");
            string currentCulture = string.Empty;
            if (rootNode != null)
            {
                currentCulture = XmlUtils.GetNodeValue(rootNode.CreateNavigator(), "installCulture");
            }

            if (string.IsNullOrEmpty(currentCulture))
            {
                currentCulture = Localization.SystemLocale;
            }

            currentCulture = currentCulture.ToLowerInvariant();

            return FindBestTemplate(templateFileName, currentCulture);
        }

        internal static string GetFileNameWithoutExtension(string scriptFile)
        {
            return Path.GetFileNameWithoutExtension(scriptFile);
        }

        internal static void CheckFipsCompilanceAssemblies()
        {
            var currentVersion = Globals.FormatVersion(DotNetNukeContext.Current.Application.Version);
            if (!CryptoConfig.AllowOnlyFipsAlgorithms ||
                HostController.Instance.GetString(FipsCompilanceAssembliesCheckedKey) == currentVersion)
            {
                return;
            }

            var assemblyFolder = Path.Combine(Globals.ApplicationMapPath, FipsCompilanceAssembliesFolder);
            var assemblyFiles = Directory.GetFiles(assemblyFolder, "*.dll", SearchOption.TopDirectoryOnly);
            foreach (var assemblyFile in assemblyFiles)
            {
                FixFipsCompilanceAssembly(assemblyFile);
            }

            HostController.Instance.Update(FipsCompilanceAssembliesCheckedKey, currentVersion);

            if (HttpContext.Current != null)
            {
                Globals.Redirect(HttpContext.Current.Request.RawUrl, true);
            }
        }

        protected static bool IsLanguageEnabled(int portalid, string code)
        {
            Locale enabledLanguage;
            return LocaleController.Instance.GetLocales(portalid).TryGetValue(code, out enabledLanguage);
        }

        /// <summary>AddModuleControl adds a new Module Control to the system.</summary>
        /// <param name="moduleDefId">The Module Definition Id.</param>
        /// <param name="controlKey">The key for this control in the Definition.</param>
        /// <param name="controlTitle">The title of this control.</param>
        /// <param name="controlSrc">Te source of ths control.</param>
        /// <param name="iconFile">The icon file.</param>
        /// <param name="controlType">The type of control.</param>
        /// <param name="viewOrder">The vieworder for this module.</param>
        /// <param name="helpURL">The Help Url.</param>
        private static void AddModuleControl(int moduleDefId, string controlKey, string controlTitle, string controlSrc, string iconFile, SecurityAccessLevel controlType, int viewOrder, string helpURL)
        {
            AddModuleControl(moduleDefId, controlKey, controlTitle, controlSrc, iconFile, controlType, viewOrder, helpURL, false);
        }

        private static void AddModuleControl(int moduleDefId, string controlKey, string controlTitle, string controlSrc, string iconFile, SecurityAccessLevel controlType, int viewOrder, string helpURL, bool supportsPartialRendering)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "AddModuleControl:" + moduleDefId);

            // check if module control exists
            var moduleControl = ModuleControlController.GetModuleControlByControlKey(controlKey, moduleDefId);
            if (moduleControl != null)
            {
                return;
            }

            moduleControl = new ModuleControlInfo
            {
                ModuleControlID = Null.NullInteger,
                ModuleDefID = moduleDefId,
                ControlKey = controlKey,
                ControlTitle = controlTitle,
                ControlSrc = controlSrc,
                ControlType = controlType,
                ViewOrder = viewOrder,
                IconFile = iconFile,
                HelpURL = helpURL,
                SupportsPartialRendering = supportsPartialRendering,
            };

            ModuleControlController.AddModuleControl(moduleControl);
        }

        /// <summary>AddModuleDefinition adds a new Core Module Definition to the system.</summary>
        /// <remarks>This overload allows the caller to determine whether the module has a controller class.</remarks>
        /// <param name="desktopModuleName">The Friendly Name of the Module to Add.</param>
        /// <param name="description">Description of the Module.</param>
        /// <param name="moduleDefinitionName">The Module Definition Name.</param>
        /// <param name="premium">A flag representing whether the module is a Premium module.</param>
        /// <param name="admin">A flag representing whether the module is an Admin module.</param>
        /// <returns>The Module Definition Id of the new Module.</returns>
        private static int AddModuleDefinition(string desktopModuleName, string description, string moduleDefinitionName, bool premium, bool admin)
        {
            return AddModuleDefinition(desktopModuleName, description, moduleDefinitionName, string.Empty, false, premium, admin);
        }

        /// <summary>AddModuleDefinition adds a new Core Module Definition to the system.</summary>
        /// <remarks>This overload allows the caller to determine whether the module has a controller class.</remarks>
        /// <param name="desktopModuleName">The Friendly Name of the Module to Add.</param>
        /// <param name="description">Description of the Module.</param>
        /// <param name="moduleDefinitionName">The Module Definition Name.</param>
        /// <param name="businessControllerClass">Business Control Class.</param>
        /// <param name="isPortable">Whether the module is enable for portals.</param>
        /// <param name="premium">A flag representing whether the module is a Premium module.</param>
        /// <param name="admin">A flag representing whether the module is an Admin module.</param>
        /// <returns>The Module Definition Id of the new Module.</returns>
        private static int AddModuleDefinition(string desktopModuleName, string description, string moduleDefinitionName, string businessControllerClass, bool isPortable, bool premium, bool admin)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "AddModuleDefinition:" + desktopModuleName);

            // check if desktop module exists
            var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(desktopModuleName, Null.NullInteger);
            if (desktopModule == null)
            {
                var package = new PackageInfo
                {
                    Description = description,
                    FriendlyName = desktopModuleName,
                    Name = string.Concat("DotNetNuke.", desktopModuleName),
                    PackageType = "Module",
                    Owner = "DNN",
                    Organization = ".NET Foundation",
                    Url = "https://dnncommunity.org",
                    Email = "info@dnncommunity.org",
                };
                if (desktopModuleName is "Extensions" or "Skin Designer")
                {
                    package.IsSystemPackage = true;
                }

                package.Version = new Version(1, 0, 0);

                PackageController.Instance.SaveExtensionPackage(package);

                var moduleName = desktopModuleName.Replace(" ", string.Empty);
                desktopModule = new DesktopModuleInfo
                {
                    DesktopModuleID = Null.NullInteger,
                    PackageID = package.PackageID,
                    FriendlyName = desktopModuleName,
                    FolderName = "Admin/" + moduleName,
                    ModuleName = moduleName,
                    Description = description,
                    Version = "01.00.00",
                    BusinessControllerClass = businessControllerClass,
                    IsPortable = isPortable,
                    SupportedFeatures = 0,
                };
                if (isPortable)
                {
                    desktopModule.SupportedFeatures = 1;
                }

                desktopModule.IsPremium = premium;
                desktopModule.IsAdmin = admin;

                desktopModule.DesktopModuleID = DesktopModuleController.SaveDesktopModule(desktopModule, false, false);

                if (!premium)
                {
                    DesktopModuleController.AddDesktopModuleToPortals(desktopModule.DesktopModuleID);
                }
            }

            // check if module definition exists
            var moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(moduleDefinitionName, desktopModule.DesktopModuleID);
            if (moduleDefinition != null)
            {
                return moduleDefinition.ModuleDefID;
            }

            moduleDefinition = new ModuleDefinitionInfo { ModuleDefID = Null.NullInteger, DesktopModuleID = desktopModule.DesktopModuleID, FriendlyName = moduleDefinitionName };
            moduleDefinition.ModuleDefID = ModuleDefinitionController.SaveModuleDefinition(moduleDefinition, false, false);

            return moduleDefinition.ModuleDefID;
        }

        /// <summary>AddModuleToPage adds a module to a Page.</summary>
        /// <remarks>This overload assumes ModulePermissions will be inherited.</remarks>
        /// <param name="page">The Page to add the Module to.</param>
        /// <param name="moduleDefId">The Module Definition Id for the module to be added to this tab.</param>
        /// <param name="moduleTitle">The Module's title.</param>
        /// <param name="moduleIconFile">The Module's icon.</param>
        private static int AddModuleToPage(TabInfo page, int moduleDefId, string moduleTitle, string moduleIconFile)
        {
            // Call overload with InheritPermisions=True
            return AddModuleToPage(page, moduleDefId, moduleTitle, moduleIconFile, true);
        }

        /// <summary>AddPage adds a Tab Page.</summary>
        /// <remarks>Adds a Tab to a parentTab.</remarks>
        /// <param name="parentTab">The Parent Tab.</param>
        /// <param name="tabName">The Name to give this new Tab.</param>
        /// <param name="description">Description.</param>
        /// <param name="tabIconFile">The Icon for this new Tab.</param>
        /// <param name="tabIconFileLarge">The Large Icon for this new Tab.</param>
        /// <param name="isVisible">A flag indicating whether the tab is visible.</param>
        /// <param name="permissions">Page Permissions Collection for this page.</param>
        /// <param name="isAdmin">Is an admin page.</param>
        private static TabInfo AddPage(TabInfo parentTab, string tabName, string description, string tabIconFile, string tabIconFileLarge, bool isVisible, TabPermissionCollection permissions, bool isAdmin)
        {
            var parentId = Null.NullInteger;
            var portalId = Null.NullInteger;

            if (parentTab != null)
            {
                parentId = parentTab.TabID;
                portalId = parentTab.PortalID;
            }

            return AddPage(portalId, parentId, tabName, description, tabIconFile, tabIconFileLarge, isVisible, permissions, isAdmin);
        }

        /// <summary>AddPage adds a Tab Page.</summary>
        /// <param name="portalId">The Id of the Portal.</param>
        /// <param name="parentId">The Id of the Parent Tab.</param>
        /// <param name="tabName">The Name to give this new Tab.</param>
        /// <param name="description">Description.</param>
        /// <param name="tabIconFile">The Icon for this new Tab.</param>
        /// <param name="tabIconFileLarge">The large Icon for this new Tab.</param>
        /// <param name="isVisible">A flag indicating whether the tab is visible.</param>
        /// <param name="permissions">Page Permissions Collection for this page.</param>
        /// <param name="isAdmin">Is and admin page.</param>
        private static TabInfo AddPage(int portalId, int parentId, string tabName, string description, string tabIconFile, string tabIconFileLarge, bool isVisible, TabPermissionCollection permissions, bool isAdmin)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "AddPage:" + tabName);

            TabInfo tab = TabController.Instance.GetTabByName(tabName, portalId, parentId);

            if (tab is not null && tab.ParentId == parentId)
            {
                return tab;
            }

            tab = new TabInfo
            {
                TabID = Null.NullInteger,
                PortalID = portalId,
                TabName = tabName,
                Title = string.Empty,
                Description = description,
                KeyWords = string.Empty,
                IsVisible = isVisible,
                DisableLink = false,
                ParentId = parentId,
                IconFile = tabIconFile,
                IconFileLarge = tabIconFileLarge,
                IsDeleted = false,
            };
            tab.TabID = TabController.Instance.AddTab(tab, !isAdmin);

            if (permissions is null)
            {
                return tab;
            }

            foreach (TabPermissionInfo tabPermission in permissions)
            {
                tab.TabPermissions.Add(tabPermission, true);
            }

            TabPermissionController.SaveTabPermissions(tab);

            return tab;
        }

        /// <summary>AddPagePermission adds a TabPermission to a TabPermission Collection.</summary>
        /// <param name="permissions">Page Permissions Collection for this page.</param>
        /// <param name="key">The Permission key.</param>
        /// <param name="roleId">The role given the permission.</param>
        private static void AddPagePermission(TabPermissionCollection permissions, string key, int roleId)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "AddPagePermission:" + key);
            var permissionController = new PermissionController();
            var permission = (IPermissionInfo)permissionController.GetPermissionByCodeAndKey("SYSTEM_TAB", key)[0];

            var tabPermission = new TabPermissionInfo();
            ((IPermissionInfo)tabPermission).PermissionId = permission.PermissionId;
            ((IPermissionInfo)tabPermission).RoleId = roleId;
            tabPermission.AllowAccess = true;

            permissions.Add(tabPermission);
        }

        /// <summary>HostTabExists determines whether a tab of a given name exists under the Host tab.</summary>
        /// <param name="tabName">The Name of the Tab.</param>
        /// <returns>True if the Tab exists, otherwise False.</returns>
        private static bool HostTabExists(string tabName)
        {
            var tabExists = false;
            var hostTab = TabController.Instance.GetTabByName("Host", Null.NullInteger);

            var tab = TabController.Instance.GetTabByName(tabName, Null.NullInteger, hostTab.TabID);
            if (tab != null)
            {
                tabExists = true;
            }

            return tabExists;
        }

        /// <summary>InstallMemberRoleProviderScript - Installs a specific MemberRole Provider script.</summary>
        /// <param name="providerPath">The Path to the Provider Directory.</param>
        /// <param name="scriptFile">The Name of the Script File.</param>
        /// <param name="writeFeedback">Whether to echo results.</param>
        private static string InstallMemberRoleProviderScript(string providerPath, string scriptFile, bool writeFeedback)
        {
            if (writeFeedback)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, $"Executing Script: {scriptFile}<br>");
            }

            var exceptions = DataProvider.Instance().ExecuteScript(FileSystemUtils.ReadFile($"{providerPath}{scriptFile}.sql"));

            // log the results
            try
            {
                using var streamWriter = File.CreateText($"{providerPath}{scriptFile}.log.resources");
                streamWriter.WriteLine(exceptions);
                streamWriter.Close();
            }
            catch (Exception exc)
            {
                // does not have permission to create the log file
                Logger.Error(exc);
            }

            return exceptions;
        }

        /// <summary>ParseFiles parses the Host Template's Files node.</summary>
        /// <param name="node">The Files node.</param>
        /// <param name="portalId">The PortalId (-1 for Host Files).</param>
        private static void ParseFiles(XmlNode node, int portalId)
        {
            // Parse the File nodes
            var nodes = node?.SelectNodes("file");
            if (nodes is null)
            {
                return;
            }

            var folderManager = FolderManager.Instance;
            var fileManager = FileManager.Instance;

            foreach (XmlNode fileNode in nodes)
            {
                var fileName = XmlUtils.GetNodeValue(fileNode.CreateNavigator(), "filename");
                var extension = XmlUtils.GetNodeValue(fileNode.CreateNavigator(), "extension");
                var size = long.Parse(XmlUtils.GetNodeValue(fileNode.CreateNavigator(), "size"));
                var width = XmlUtils.GetNodeValueInt(fileNode, "width");
                var height = XmlUtils.GetNodeValueInt(fileNode, "height");
                var contentType = XmlUtils.GetNodeValue(fileNode.CreateNavigator(), "contentType");
                var folder = XmlUtils.GetNodeValue(fileNode.CreateNavigator(), "folder");

                var folderInfo = folderManager.GetFolder(portalId, folder);
                var file = new FileInfo(portalId, fileName, extension, (int)size, width, height, contentType, folder, folderInfo.FolderID, folderInfo.StorageLocation, true);

                using (var fileContent = fileManager.GetFileContent(file))
                {
                    const bool operationDoesNotRequirePermissionsCheck = true;
                    var addedFile = fileManager.AddFile(folderInfo, file.FileName, fileContent, false, !operationDoesNotRequirePermissionsCheck, FileContentTypeManager.Instance.GetContentType(Path.GetExtension(file.FileName)));

                    file.FileId = addedFile.FileId;
                    file.EnablePublishPeriod = addedFile.EnablePublishPeriod;
                    file.EndDate = addedFile.EndDate;
                    file.StartDate = addedFile.StartDate;
                }

                fileManager.UpdateFile(file);
            }
        }

        private static void UninstallPackage(string packageName, string packageType, bool deleteFiles = true, string version = "")
        {
            DnnInstallLogger.InstallLogInfo(string.Concat(Localization.GetString("LogStart", Localization.GlobalResourceFile), "Uninstallation of Package:", packageName, " Type:", packageType, " Version:", version));

            var searchInput = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p =>
                p.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase)
                && p.PackageType.Equals(packageType, StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrEmpty(version) || p.Version.ToString() == version));
            if (searchInput is null)
            {
                return;
            }

            var searchInputInstaller = new Installer(searchInput, Globals.ApplicationMapPath);
            searchInputInstaller.UnInstall(deleteFiles);
        }

        private static void RemoveGettingStartedPages()
        {
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                try
                {
                    var fileInfo = FileManager.Instance.GetFile(portal.PortalID, "GettingStarted.css");
                    if (fileInfo != null)
                    {
                        FileManager.Instance.DeleteFile(fileInfo);
                    }

                    var gettingStartedTabId = PortalController.GetPortalSettingAsInteger("GettingStartedTabId", portal.PortalID, Null.NullInteger);
                    if (gettingStartedTabId > Null.NullInteger)
                    {
                        // remove getting started page from portal
                        if (TabController.Instance.GetTab(gettingStartedTabId, portal.PortalID, true) != null)
                        {
                            TabController.Instance.DeleteTab(gettingStartedTabId, portal.PortalID);
                        }

                        PortalController.DeletePortalSetting(portal.PortalID, "GettingStartedTabId");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        private static string GetStringVersionWithRevision(Version version)
        {
            var stringVersion = GetStringVersion(version);
            if (version.Revision > 0)
            {
                stringVersion += "." + version.Revision.ToString("D2");
            }

            return stringVersion;
        }

        private static string[] AddAvailableIncrementalFiles(string providerPath, Version version)
        {
            if (version.Major < 8)
            {
                return null;
            }

            var files = Directory.GetFiles(providerPath, GetStringVersion(version) + ".*." + DefaultProvider);
            Array.Sort(files); // The order of the returned file names is not guaranteed; use the Sort method if a specific sort order is required.

            return files;
        }

        private static string GetFileName(string file)
        {
            return Path.GetFileName(file);
        }

        private static void ParsePackagesFromApplicationPath(IEnumerable<string> packageTypes, Dictionary<string, PackageInfo> packages, List<string> invalidPackages)
        {
            foreach (var packageType in packageTypes)
            {
                var installPackagePath = Globals.ApplicationMapPath + "\\Install\\" + packageType;
                if (!Directory.Exists(installPackagePath))
                {
                    continue;
                }

                var files = Directory.GetFiles(installPackagePath);
                if (files.Length <= 0)
                {
                    continue;
                }

                Array.Sort(files); // The order of the returned file names is not guaranteed on certain NAS systems; use the Sort method if a specific sort order is required.

                var optionalPackages = new List<string>();
                foreach (var file in files)
                {
                    var extension = Path.GetExtension(file.ToLowerInvariant());
                    if (extension != ".zip" && extension != ".resources")
                    {
                        continue;
                    }

                    var isInstalled = false;
                    PackageController.ParsePackage(file, installPackagePath, packages, invalidPackages);
                    if (packages.TryGetValue(file, out var package))
                    {
                        var installedPackage = PackageController.Instance.GetExtensionPackage(
                            Null.NullInteger,
                            p => p.Name.Equals(package.Name, StringComparison.OrdinalIgnoreCase)
                                    && p.PackageType.Equals(package.PackageType, StringComparison.OrdinalIgnoreCase));
                        isInstalled = installedPackage != null;

                        if (packages.Values.Count(p => p.FriendlyName.Equals(package.FriendlyName, StringComparison.OrdinalIgnoreCase)) > 1
                                || installedPackage != null)
                        {
                            var oldPackages = packages.Where(kvp => kvp.Value.FriendlyName.Equals(package.FriendlyName, StringComparison.OrdinalIgnoreCase)
                                                                        && kvp.Value.Version < package.Version).ToList();

                            // if there already have higher version installed, remove current one from list.
                            if (installedPackage != null && package.Version <= installedPackage.Version)
                            {
                                oldPackages.Add(new KeyValuePair<string, PackageInfo>(file, package));
                            }

                            if (oldPackages.Count != 0)
                            {
                                foreach (var oldPackage in oldPackages)
                                {
                                    try
                                    {
                                        packages.Remove(oldPackage.Key);
                                        FileWrapper.Instance.Delete(oldPackage.Key);
                                    }
                                    catch (Exception)
                                    {
                                        // do nothing here.
                                    }
                                }
                            }
                        }
                    }

                    if (extension != ".zip" && !isInstalled)
                    {
                        optionalPackages.Add(file);
                    }
                }

                // remove optional
                optionalPackages.ForEach(f =>
                                         {
                                             if (packages.ContainsKey(f))
                                             {
                                                 packages.Remove(f);
                                             }
                                         });
            }
        }

        private static void UpgradeToVersion10_0_0()
        {
            if (!HostTabExists("Superuser Accounts"))
            {
                // add SuperUser Accounts module and tab
                var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName("Security", Null.NullInteger);
                if (desktopModule != null)
                {
                    var moduleDefId = ModuleDefinitionController
                        .GetModuleDefinitionByFriendlyName("User Accounts", desktopModule.DesktopModuleID).ModuleDefID;

                    // Create New Host Page (or get existing one)
                    var newPage = AddHostPage(
                        "Superuser Accounts",
                        "Manage host user accounts.",
                        "~/Icons/Sigma/Users_16X16_Standard.png",
                        "~/Icons/Sigma/Users_32X32_Standard.png",
                        false);

                    // Add Module To Page
                    AddModuleToPage(newPage, moduleDefId, "SuperUser Accounts", "~/Icons/Sigma/Users_32X32_Standard.png");
                }
            }
        }

        private static void FixFipsCompilanceAssembly(string filePath)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);
                if (string.IsNullOrEmpty(fileName))
                {
                    return;
                }

                var assemblyPath = Path.Combine(Globals.ApplicationMapPath, "bin", fileName);
                if (File.Exists(assemblyPath))
                {
                    var backupFolder = Path.Combine(Globals.ApplicationMapPath, FipsCompilanceAssembliesFolder, "Backup");
                    if (!Directory.Exists(backupFolder))
                    {
                        Directory.CreateDirectory(backupFolder);
                    }

                    File.Copy(assemblyPath, Path.Combine(backupFolder, fileName + "." + DateTime.Now.Ticks), true);
                    File.Copy(filePath, assemblyPath, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static void FixTabsMissingLocalizedFields()
        {
            var portals = PortalController.Instance.GetPortals();
            IDictionary<string, XmlDocument> resourcesDict = new Dictionary<string, XmlDocument>();
            var localizeFieldRegex = new Regex("^\\[Tab[\\w\\.]+?\\.Text\\]$");
            foreach (PortalInfo portal in portals)
            {
                if (portal.AdminTabId > Null.NullInteger)
                {
                    var adminTabs = TabController.GetTabsByParent(portal.AdminTabId, portal.PortalID);
                    foreach (var tab in adminTabs)
                    {
                        var tabChanged = false;
                        if (!string.IsNullOrEmpty(tab.Title) && localizeFieldRegex.IsMatch(tab.Title))
                        {
                            tab.Title = FindLocalizedContent(tab.Title, portal.CultureCode, ref resourcesDict);
                            tabChanged = true;
                        }

                        if (!string.IsNullOrEmpty(tab.Description) && localizeFieldRegex.IsMatch(tab.Description))
                        {
                            tab.Description = FindLocalizedContent(tab.Description, portal.CultureCode, ref resourcesDict);
                            tabChanged = true;
                        }

                        if (tabChanged)
                        {
                            TabController.Instance.UpdateTab(tab);
                        }
                    }
                }
            }
        }

        private static string FindLocalizedContent(string field, string cultureCode, ref IDictionary<string, XmlDocument> resourcesDict)
        {
            var xmlDocument = FindLanguageXmlDocument(cultureCode, ref resourcesDict);
            if (xmlDocument != null)
            {
                var key = field.Substring(1, field.Length - 2);
                var localizedTitleNode = xmlDocument.SelectSingleNode("//data[@name=\"" + key + "\"]");
                if (localizedTitleNode != null)
                {
                    var valueNode = localizedTitleNode.SelectSingleNode("value");
                    if (valueNode != null)
                    {
                        var content = valueNode.InnerText;

                        if (!string.IsNullOrEmpty(content))
                        {
                            return content;
                        }
                    }
                }
            }

            return string.Empty;
        }

        private static XmlDocument FindLanguageXmlDocument(string cultureCode, ref IDictionary<string, XmlDocument> resourcesDict)
        {
            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = Localization.SystemLocale;
            }

            if (resourcesDict.TryGetValue(cultureCode, out var doc))
            {
                return doc;
            }

            try
            {
                var languageFilePath = Path.Combine(Globals.HostMapPath, $"Default Website.template.{cultureCode}.resx");
                if (!File.Exists(languageFilePath))
                {
                    languageFilePath = Path.Combine(Globals.HostMapPath, $"Default Website.template.{Localization.SystemLocale}.resx");
                }

                var xmlDocument = new XmlDocument { XmlResolver = null };
                xmlDocument.Load(languageFilePath);

                resourcesDict.Add(cultureCode, xmlDocument);

                return xmlDocument;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                return null;
            }
        }

        private static bool NewtonsoftNeedUpdate()
        {
            var currentConfig = Config.Load();
            var nsmgr = new XmlNamespaceManager(currentConfig.NameTable);
            nsmgr.AddNamespace("ab", "urn:schemas-microsoft-com:asm.v1");
            var bindingNode = currentConfig.SelectSingleNode(
                "/configuration/runtime/ab:assemblyBinding/ab:dependentAssembly[ab:assemblyIdentity/@name='Newtonsoft.Json']/ab:bindingRedirect", nsmgr);

            var newVersion = bindingNode?.Attributes?["newVersion"].Value;
            if (!string.IsNullOrEmpty(newVersion) && new Version(newVersion) >= new Version(10, 0, 0, 0))
            {
                return false;
            }

            return true;
        }
    }
}
