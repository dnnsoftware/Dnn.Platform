﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Analytics;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.EventQueue.Config;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Log;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Search;
    using DotNetNuke.Services.Social.Messaging.Internal;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Services.Upgrade.InternalController.Steps;
    using DotNetNuke.Services.Upgrade.Internals;
    using DotNetNuke.Services.Upgrade.Internals.Steps;
    using DotNetNuke.UI.Internals;

    using Assembly = System.Reflection.Assembly;
    using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;
    using Localization = DotNetNuke.Services.Localization.Localization;
    using ModuleInfo = DotNetNuke.Entities.Modules.ModuleInfo;
    using Util = DotNetNuke.Entities.Content.Common.Util;

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///  The Upgrade class provides Shared/Static methods to Upgrade/Install
    ///  a DotNetNuke Application.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class Upgrade
    {
        private const string FipsCompilanceAssembliesCheckedKey = "FipsCompilanceAssembliesChecked";
        private const string FipsCompilanceAssembliesFolder = "App_Data\\FipsCompilanceAssemblies";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Upgrade));
        private static readonly object _threadLocker = new object();
        private static DateTime _startTime;

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
                return currentTime.Subtract(_startTime);
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  AddAdminPages adds an Admin Page and an associated Module to all configured Portals.
        /// </summary>
        /// <param name = "tabName">The Name to give this new Tab.</param>
        /// <param name="description"></param>
        /// <param name = "tabIconFile">The Icon for this new Tab.</param>
        /// <param name="tabIconFileLarge"></param>
        /// <param name = "isVisible">A flag indicating whether the tab is visible.</param>
        /// <param name = "moduleDefId">The Module Deinition Id for the module to be aded to this tab.</param>
        /// <param name = "moduleTitle">The Module's title.</param>
        /// <param name = "moduleIconFile">The Module's icon.</param>
        /// <param name = "inheritPermissions">Modules Inherit the Pages View Permisions.</param>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  AddAdminPage adds an Admin Tab Page.
        /// </summary>
        /// <param name = "portal">The Portal.</param>
        /// <param name = "tabName">The Name to give this new Tab.</param>
        /// <param name="description"></param>
        /// <param name = "tabIconFile">The Icon for this new Tab.</param>
        /// <param name="tabIconFileLarge"></param>
        /// <param name = "isVisible">A flag indicating whether the tab is visible.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  AddHostPage adds a Host Tab Page.
        /// </summary>
        /// <param name = "tabName">The Name to give this new Tab.</param>
        /// <param name="description"></param>
        /// <param name = "tabIconFile">The Icon for this new Tab.</param>
        /// <param name="tabIconFileLarge"></param>
        /// <param name = "isVisible">A flag indicating whether the tab is visible.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  AddModuleControl adds a new Module Control to the system.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "moduleDefId">The Module Definition Id.</param>
        /// <param name = "controlKey">The key for this control in the Definition.</param>
        /// <param name = "controlTitle">The title of this control.</param>
        /// <param name = "controlSrc">Te source of ths control.</param>
        /// <param name = "iconFile">The icon file.</param>
        /// <param name = "controlType">The type of control.</param>
        /// <param name = "viewOrder">The vieworder for this module.</param>
        /// -----------------------------------------------------------------------------
        public static void AddModuleControl(int moduleDefId, string controlKey, string controlTitle, string controlSrc, string iconFile, SecurityAccessLevel controlType, int viewOrder)
        {
            // Call Overload with HelpUrl = Null.NullString
            AddModuleControl(moduleDefId, controlKey, controlTitle, controlSrc, iconFile, controlType, viewOrder, Null.NullString);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  AddModuleDefinition adds a new Core Module Definition to the system.
        /// </summary>
        /// <remarks>
        ///  This overload asumes the module is an Admin module and not a Premium Module.
        /// </remarks>
        /// <param name = "desktopModuleName">The Friendly Name of the Module to Add.</param>
        /// <param name = "description">Description of the Module.</param>
        /// <param name = "moduleDefinitionName">The Module Definition Name.</param>
        /// <returns>The Module Definition Id of the new Module.</returns>
        /// -----------------------------------------------------------------------------
        public static int AddModuleDefinition(string desktopModuleName, string description, string moduleDefinitionName)
        {
            // Call overload with Premium=False and Admin=True
            return AddModuleDefinition(desktopModuleName, description, moduleDefinitionName, false, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  AddModuleToPage adds a module to a Page.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "page">The Page to add the Module to.</param>
        /// <param name = "moduleDefId">The Module Deinition Id for the module to be aded to this tab.</param>
        /// <param name = "moduleTitle">The Module's title.</param>
        /// <param name = "moduleIconFile">The Module's icon.</param>
        /// <param name = "inheritPermissions">Inherit the Pages View Permisions.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static int AddModuleToPage(TabInfo page, int moduleDefId, string moduleTitle, string moduleIconFile, bool inheritPermissions)
        {
            return AddModuleToPage(page, moduleDefId, moduleTitle, moduleIconFile, inheritPermissions, true, Globals.glbDefaultPane);
        }

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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   AddPortal manages the Installation of a new DotNetNuke Portal.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Obsolete, AddPortal manages the Installation of a new DotNetNuke Portal.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        [Obsolete("Deprecated in DNN 9.3.0, will be removed in 11.0.0. Use the overloaded method with the 'superUser' parameter instead. Scheduled removal in v11.0.0.")]
        public static int AddPortal(XmlNode node, bool status, int indent)
        {
            return AddPortal(node, status, indent, null);
        }
        
        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   DeleteInstallerFiles - clean up install config and installwizard files
        ///   If installwizard is ran again this will be recreated via the dotnetnuke.install.config.resources file.
        /// </summary>
        /// <remarks>
        /// uses FileSystemUtils.DeleteFile as it checks for readonly attribute status
        /// and changes it if required, as well as verifying file exists.
        /// </remarks>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   DeleteFiles - clean up deprecated files and folders.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="providerPath">Path to provider.</param>
        /// <param name = "version">The Version being Upgraded.</param>
        /// <param name="writeFeedback">Display status in UI?.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  ExecuteScripts manages the Execution of Scripts from the Install/Scripts folder.
        ///  It is also triggered by InstallDNN and UpgradeDNN.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "strProviderPath">The path to the Data Provider.</param>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  ExecuteScript executes a special script.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "file">The script file to execute.</param>
        /// -----------------------------------------------------------------------------
        public static void ExecuteScript(string file)
        {
            // Execute if script is a provider script
            if (file.IndexOf("." + DefaultProvider) != -1)
            {
                ExecuteScript(file, true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  GetInstallTemplate retrieves the Installation Template as specifeid in web.config.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "xmlDoc">The Xml Document to load.</param>
        /// <returns>A string which contains the error message - if appropriate.</returns>
        /// -----------------------------------------------------------------------------
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

        /// <summary>
        ///  SetInstalltemplate saves the XmlDocument back to Installation Template specified in web.config.
        /// </summary>
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  GetInstallVersion retrieves the Base Instal Version as specifeid in the install
        ///  template.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "xmlDoc">The Install Template.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  GetLogFile gets the filename for the version's log file.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "providerPath">The path to the Data Provider.</param>
        /// <param name = "version">The Version.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static string GetLogFile(string providerPath, Version version)
        {
            return providerPath + GetStringVersion(version) + ".log.resources";
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  GetScriptFile gets the filename for the version.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "providerPath">The path to the Data Provider.</param>
        /// <param name = "version">The Version.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static string GetScriptFile(string providerPath, Version version)
        {
            return providerPath + GetStringVersion(version) + "." + DefaultProvider;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  GetStringVersion gets the Version String (xx.xx.xx) from the Version.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "version">The Version.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  GetSuperUser gets the superuser from the Install Template.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "xmlTemplate">The install Templae.</param>
        /// <param name = "writeFeedback">a flag to determine whether to output feedback.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  GetUpgradeScripts gets an ArrayList of the Scripts required to Upgrade to the
        ///  current Assembly Version.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "providerPath">The path to the Data Provider.</param>
        /// <param name = "databaseVersion">The current Database Version.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  InitialiseHostSettings gets the Host Settings from the Install Template.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "xmlTemplate">The install Templae.</param>
        /// <param name = "writeFeedback">a flag to determine whether to output feedback.</param>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  InstallDatabase runs all the "scripts" identifed in the Install Template to
        ///  install the base version.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="providerPath"></param>
        /// <param name = "xmlDoc">The Xml Document to load.</param>
        /// <param name = "writeFeedback">A flag that determines whether to output feedback to the Response Stream.</param>
        /// <param name="version"></param>
        /// <returns>A string which contains the error message - if appropriate.</returns>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  InstallDNN manages the Installation of a new DotNetNuke Application.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "strProviderPath">The path to the Data Provider.</param>
        /// -----------------------------------------------------------------------------
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
                IInstallationStep ensureLpAndTemplate = new UpdateLanguagePackStep();
                ensureLpAndTemplate.Execute();

                // install LP that contains templates if installing in a different language
                var installConfig = InstallController.Instance.GetInstallConfig();
                string culture = installConfig.InstallCulture;
                if (!culture.Equals("en-us", StringComparison.InvariantCultureIgnoreCase))
                {
                    string installFolder = HttpContext.Current.Server.MapPath("~/Install/language");
                    string lpAndTemplates = installFolder + "\\installlanguage.resources";

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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  InstallFiles intsalls any files listed in the Host Install Configuration file.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "xmlDoc">The Xml Document to load.</param>
        /// <param name = "writeFeedback">A flag that determines whether to output feedback to the Response Stream.</param>
        /// -----------------------------------------------------------------------------
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

        /// <summary>
        /// Gets a ist of installable extensions sorted to ensure dependencies are installed first.
        /// </summary>
        /// <returns></returns>
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
            foreach (PortalInfo portal in portals)
            {
                var tabID = TabController.GetTabByTabPath(portal.PortalID, tabPath, Null.NullString);
                if (tabID != Null.NullInteger)
                {
                    TabController.Instance.DeleteTab(tabID, portal.PortalID);
                }
            }
        }

        public static void RemoveHostPage(string pageName)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "RemoveHostPage:" + pageName);
            TabInfo skinsTab = TabController.Instance.GetTabByName(pageName, Null.NullInteger);
            if (skinsTab != null)
            {
                TabController.Instance.DeleteTab(skinsTab.TabID, Null.NullInteger);
            }
        }

        public static void StartTimer()
        {
            // Start Upgrade Timer
            _startTime = DateTime.Now;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  UpgradeApplication - This overload is used for general application upgrade operations.
        /// </summary>
        /// <remarks>
        ///  Since it is not version specific and is invoked whenever the application is
        ///  restarted, the operations must be re-executable.
        /// </remarks>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  UpgradeApplication - This overload is used for version specific application upgrade operations.
        /// </summary>
        /// <remarks>
        ///  This should be used for file system modifications or upgrade operations which
        ///  should only happen once. Database references are not recommended because future
        ///  versions of the application may result in code incompatibilties.
        /// </remarks>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
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
                if (version.Revision == -1)
                {
                    switch (version.ToString(3))
                    {
                        case "7.0.0":
                            UpgradeToVersion700();
                            break;
                        case "7.1.0":
                            UpgradeToVersion710();
                            break;
                        case "7.1.1":
                            UpgradeToVersion711();
                            break;
                        case "7.1.2":
                            UpgradeToVersion712();
                            break;
                        case "7.2.0":
                            UpgradeToVersion720();
                            break;
                        case "7.2.1":
                            UpgradeToVersion721();
                            break;
                        case "7.2.2":
                            UpgradeToVersion722();
                            break;
                        case "7.3.0":
                            UpgradeToVersion730();
                            break;
                        case "7.3.2":
                            UpgradeToVersion732();
                            break;
                        case "7.3.3":
                            UpgradeToVersion733();
                            break;
                        case "7.4.0":
                            UpgradeToVersion740();
                            break;
                        case "7.4.2":
                            UpgradeToVersion742();
                            break;
                        case "9.1.0":
                            UpgradeToVersion910();
                            break;
                        case "9.2.0":
                            UpgradeToVersion920();
                            break;
                        case "9.2.1":
                            UpgradeToVersion921();
                            break;
                        case "9.3.0":
                            UpgradeToVersion930();
                            break;
                        case "9.4.1":
                            UpgradeToVersion941();
                            break;
                        case "9.6.0":
                            UpgradeToVersion960();
                            break;
                    }
                }
                else
                {
                    // Incremental
                    switch (version.ToString(4))
                    {
                        case "8.0.0.6":
                            UpgradeToVersion8006();
                            break;
                        case "8.0.0.7":
                            UpgradeToVersion8007();
                            break;
                        case "8.0.0.13":
                            UpgradeToVersion80013();
                            break;
                        case "8.0.0.16":
                            UpgradeToVersion80016();
                            break;
                        case "8.0.0.26":
                            UpgradeToVersion80026();
                            break;
                        case "8.0.0.27":
                            UpgradeToVersion80027();
                            break;
                    }
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  UpgradeDNN manages the Upgrade of an exisiting DotNetNuke Application.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "providerPath">The path to the Data Provider.</param>
        /// <param name = "dataBaseVersion">The current Database Version.</param>
        /// -----------------------------------------------------------------------------
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

            DataProvider.Instance().SetCorePackageVersions();

            // perform general application upgrades
            HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Performing General Upgrades<br>");
            DnnInstallLogger.InstallLogInfo(Localization.GetString("GeneralUpgrades", Localization.GlobalResourceFile));
            UpgradeApplication();

            DataCache.ClearHostCache(true);
        }

        public static string UpgradeIndicator(Version version, bool isLocal, bool isSecureConnection)
        {
            return UpgradeIndicator(version, DotNetNukeContext.Current.Application.Type, DotNetNukeContext.Current.Application.Name, string.Empty, isLocal, isSecureConnection);
        }

        public static string UpgradeIndicator(Version version, string packageType, string packageName, string culture, bool isLocal, bool isSecureConnection)
        {
            string url = string.Empty;
            if (Host.CheckUpgrade && version != new Version(0, 0, 0))
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

                url += "&id=" + Host.GUID;
                if (packageType.Equals(DotNetNukeContext.Current.Application.Type, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(HostController.Instance.GetString("NewsletterSubscribeEmail")))
                    {
                        url += "&email=" + HttpUtility.UrlEncode(HostController.Instance.GetString("NewsletterSubscribeEmail"));
                    }

                    var portals = PortalController.Instance.GetPortals();
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  UpgradeVersion upgrades a single version.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "scriptFile">The upgrade script file.</param>
        /// <param name="writeFeedback">Write status to Response Stream?.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static string UpgradeVersion(string scriptFile, bool writeFeedback)
        {
            bool scriptExecuted;
            return UpgradeVersion(scriptFile, writeFeedback, out scriptExecuted);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  UpgradeVersion upgrades a single version.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="scriptFile">The upgrade script file.</param>
        /// <param name="writeFeedback">Write status to Response Stream?.</param>
        /// <param name="scriptExecuted">Identity whether the script file executed.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
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
                    LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString(),
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
                    lock (_threadLocker)
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

        public static string ActivateLicense()
        {
            var isLicensable = File.Exists(HttpContext.Current.Server.MapPath("~\\bin\\DotNetNuke.Professional.dll")) || File.Exists(HttpContext.Current.Server.MapPath("~\\bin\\DotNetNuke.Enterprise.dll"));
            var activationResult = string.Empty;

            if (isLicensable)
            {
                var sku = File.Exists(HttpContext.Current.Server.MapPath("~\\bin\\DotNetNuke.Enterprise.dll")) ? "DNNENT" : "DNNPRO";
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, Localization.GetString("ActivatingLicense", Localization.GlobalResourceFile));

                var installConfig = InstallController.Instance.GetInstallConfig();
                var licenseConfig = (installConfig != null) ? installConfig.License : null;

                if (licenseConfig != null)
                {
                    dynamic licenseActivation = Reflection.CreateObject(Reflection.CreateType("DotNetNuke.Professional.LicenseActivation.ViewLicx"));
                    licenseActivation.AutoActivation(licenseConfig.AccountEmail, licenseConfig.InvoiceNumber, licenseConfig.WebServer, licenseConfig.LicenseType, sku);
                    activationResult = licenseActivation.LicenseResult;

                    // Log Event to Event Log
                    EventLogController.Instance.AddLog(
                        "License Activation",
                        "License Activated during install for: " + licenseConfig.AccountEmail + " | invoice: " + licenseConfig.InvoiceNumber,
                        EventLogController.EventLogType.HOST_ALERT);
                }
            }

            return activationResult;
        }

        public static bool RemoveInvalidAntiForgeryCookie()
        {
            // DNN-9394: when upgrade from old version which use MVC version below than 5, it may saved antiforgery cookie
            // with a different cookie name which join the root path even equals to "/", then it will cause API request failed.
            // we need remove the cookie during upgrade process.
            var appPath = HttpRuntime.AppDomainAppVirtualPath;
            if (appPath == "/" && HttpContext.Current != null)
            {
                var cookieSuffix = Convert.ToBase64String(Encoding.UTF8.GetBytes(appPath)).Replace('+', '.').Replace('/', '-').Replace('=', '_');
                var cookieName = $"__RequestVerificationToken_{cookieSuffix}";
                var invalidCookie = HttpContext.Current.Request.Cookies[cookieName];
                if (invalidCookie != null)
                {
                    invalidCookie.Expires = DateTime.Now.AddYears(-1);
                    HttpContext.Current.Response.Cookies.Add(invalidCookie);

                    return true;
                }
            }

            return false;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ExecuteScript executes a SQl script file.
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///     <param name="scriptFile">The script to Execute.</param>
        /// <param name="writeFeedback">Need to output feedback message.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
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
                using (var streamWriter = File.CreateText(scriptFile.Replace("." + DefaultProvider, string.Empty) + ".log.resources"))
                {
                    streamWriter.WriteLine(exceptions);
                    streamWriter.Close();
                }
            }
            catch (Exception exc)
            {
                // does not have permission to create the log file
                Logger.Error(exc);
            }

            if (writeFeedback)
            {
                string resourcesFile = Path.GetFileName(scriptFile);
                if (!string.IsNullOrEmpty(resourcesFile))
                {
                    HtmlUtils.WriteScriptSuccessError(HttpContext.Current.Response, string.IsNullOrEmpty(exceptions), resourcesFile.Replace("." + DefaultProvider, ".log.resources"));
                }
            }

            return exceptions;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// InstallMemberRoleProvider - Installs the MemberRole Provider Db objects.
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///     <param name="providerPath">The Path to the Provider Directory.</param>
        /// <param name="writeFeedback">Whether need to output feedback message.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
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
            var adminUser = new UserInfo
            {
                FirstName = firstName,
                LastName = lastName,
                Username = userName,
                DisplayName = firstName + " " + lastName,
                Membership = { Password = password },
                Email = email,
                IsSuperUser = false,
            };
            adminUser.Membership.Approved = true;
            adminUser.Profile.FirstName = firstName;
            adminUser.Profile.LastName = lastName;
            adminUser.Membership.UpdatePassword = true;
            return adminUser;
        }

        internal static PortalController.PortalTemplateInfo FindBestTemplate(string templateFileName, string currentCulture)
        {
            if (string.IsNullOrEmpty(currentCulture))
            {
                currentCulture = Localization.SystemLocale;
            }

            var templates = PortalController.Instance.GetAvailablePortalTemplates();

            var defaultTemplates =
                templates.Where(x => Path.GetFileName(x.TemplateFilePath) == templateFileName).ToList();

            var match = defaultTemplates.FirstOrDefault(x => x.CultureCode.ToLowerInvariant() == currentCulture);
            if (match == null)
            {
                match = defaultTemplates.FirstOrDefault(x => x.CultureCode.ToLowerInvariant().StartsWith(currentCulture.Substring(0, 2)));
            }

            if (match == null)
            {
                match = defaultTemplates.FirstOrDefault(x => string.IsNullOrEmpty(x.CultureCode));
            }

            if (match == null)
            {
                throw new Exception("Unable to locate specified portal template: " + templateFileName);
            }

            return match;
        }

        internal static PortalController.PortalTemplateInfo FindBestTemplate(string templateFileName)
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
            if (CryptoConfig.AllowOnlyFipsAlgorithms && HostController.Instance.GetString(FipsCompilanceAssembliesCheckedKey) != currentVersion)
            {
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
        }

        protected static bool IsLanguageEnabled(int portalid, string code)
        {
            Locale enabledLanguage;
            return LocaleController.Instance.GetLocales(portalid).TryGetValue(code, out enabledLanguage);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddAdminPages adds an Admin Page and an associated Module to all configured Portals.
        /// </summary>
        ///     <param name="tabName">The Name to give this new Tab.</param>
        /// <param name="description">Description.</param>
        ///     <param name="tabIconFile">The Icon for this new Tab.</param>
        /// <param name="tabIconFileLarge">The large Icon for this new Tab.</param>
        ///     <param name="isVisible">A flag indicating whether the tab is visible.</param>
        ///     <param name="moduleDefId">The Module Deinition Id for the module to be aded to this tab.</param>
        ///     <param name="moduleTitle">The Module's title.</param>
        ///     <param name="moduleIconFile">The Module's icon.</param>
        /// -----------------------------------------------------------------------------
        private static void AddAdminPages(string tabName, string description, string tabIconFile, string tabIconFileLarge, bool isVisible, int moduleDefId, string moduleTitle, string moduleIconFile)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "AddAdminPages:" + tabName);

            // Call overload with InheritPermisions=True
            AddAdminPages(tabName, description, tabIconFile, tabIconFileLarge, isVisible, moduleDefId, moduleTitle, moduleIconFile, true);
        }

        private static void AddAdminRoleToPage(string tabPath)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "AddAdminRoleToPage:" + tabPath);

            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                int tabID = TabController.GetTabByTabPath(portal.PortalID, tabPath, Null.NullString);
                if (tabID != Null.NullInteger)
                {
                    TabInfo tab = TabController.Instance.GetTab(tabID, portal.PortalID, true);

                    if (tab.TabPermissions.Count == 0)
                    {
                        AddPagePermission(tab.TabPermissions, "View", Convert.ToInt32(portal.AdministratorRoleId));
                        AddPagePermission(tab.TabPermissions, "Edit", Convert.ToInt32(portal.AdministratorRoleId));
                        TabPermissionController.SaveTabPermissions(tab);
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddModuleControl adds a new Module Control to the system.
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///     <param name="moduleDefId">The Module Definition Id.</param>
        ///     <param name="controlKey">The key for this control in the Definition.</param>
        ///     <param name="controlTitle">The title of this control.</param>
        ///     <param name="controlSrc">Te source of ths control.</param>
        ///     <param name="iconFile">The icon file.</param>
        ///     <param name="controlType">The type of control.</param>
        ///     <param name="viewOrder">The vieworder for this module.</param>
        ///     <param name="helpURL">The Help Url.</param>
        /// -----------------------------------------------------------------------------
        private static void AddModuleControl(int moduleDefId, string controlKey, string controlTitle, string controlSrc, string iconFile, SecurityAccessLevel controlType, int viewOrder, string helpURL)
        {
            AddModuleControl(moduleDefId, controlKey, controlTitle, controlSrc, iconFile, controlType, viewOrder, helpURL, false);
        }

        private static void AddModuleControl(int moduleDefId, string controlKey, string controlTitle, string controlSrc, string iconFile, SecurityAccessLevel controlType, int viewOrder, string helpURL, bool supportsPartialRendering)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "AddModuleControl:" + moduleDefId);

            // check if module control exists
            var moduleControl = ModuleControlController.GetModuleControlByControlKey(controlKey, moduleDefId);
            if (moduleControl == null)
            {
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
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddModuleDefinition adds a new Core Module Definition to the system.
        /// </summary>
        /// <remarks>
        ///     This overload allows the caller to determine whether the module has a controller
        /// class.
        /// </remarks>
        ///     <param name="desktopModuleName">The Friendly Name of the Module to Add.</param>
        ///     <param name="description">Description of the Module.</param>
        ///     <param name="moduleDefinitionName">The Module Definition Name.</param>
        ///     <param name="premium">A flag representing whether the module is a Premium module.</param>
        ///     <param name="admin">A flag representing whether the module is an Admin module.</param>
        ///     <returns>The Module Definition Id of the new Module.</returns>
        /// -----------------------------------------------------------------------------
        private static int AddModuleDefinition(string desktopModuleName, string description, string moduleDefinitionName, bool premium, bool admin)
        {
            return AddModuleDefinition(desktopModuleName, description, moduleDefinitionName, string.Empty, false, premium, admin);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddModuleDefinition adds a new Core Module Definition to the system.
        /// </summary>
        /// <remarks>
        ///     This overload allows the caller to determine whether the module has a controller
        /// class.
        /// </remarks>
        ///     <param name="desktopModuleName">The Friendly Name of the Module to Add.</param>
        ///     <param name="description">Description of the Module.</param>
        ///     <param name="moduleDefinitionName">The Module Definition Name.</param>
        /// <param name="businessControllerClass">Business Control Class.</param>
        /// <param name="isPortable">Whether the module is enable for portals.</param>
        ///     <param name="premium">A flag representing whether the module is a Premium module.</param>
        ///     <param name="admin">A flag representing whether the module is an Admin module.</param>
        ///     <returns>The Module Definition Id of the new Module.</returns>
        /// -----------------------------------------------------------------------------
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
                if (desktopModuleName == "Extensions" || desktopModuleName == "Skin Designer")
                {
                    package.IsSystemPackage = true;
                }

                package.Version = new Version(1, 0, 0);

                PackageController.Instance.SaveExtensionPackage(package);

                string moduleName = desktopModuleName.Replace(" ", string.Empty);
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
            if (moduleDefinition == null)
            {
                moduleDefinition = new ModuleDefinitionInfo { ModuleDefID = Null.NullInteger, DesktopModuleID = desktopModule.DesktopModuleID, FriendlyName = moduleDefinitionName };

                moduleDefinition.ModuleDefID = ModuleDefinitionController.SaveModuleDefinition(moduleDefinition, false, false);
            }

            return moduleDefinition.ModuleDefID;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddModuleToPage adds a module to a Page.
        /// </summary>
        /// <remarks>
        /// This overload assumes ModulePermissions will be inherited.
        /// </remarks>
        ///     <param name="page">The Page to add the Module to.</param>
        ///     <param name="moduleDefId">The Module Deinition Id for the module to be aded to this tab.</param>
        ///     <param name="moduleTitle">The Module's title.</param>
        ///     <param name="moduleIconFile">The Module's icon.</param>
        /// -----------------------------------------------------------------------------
        private static int AddModuleToPage(TabInfo page, int moduleDefId, string moduleTitle, string moduleIconFile)
        {
            // Call overload with InheritPermisions=True
            return AddModuleToPage(page, moduleDefId, moduleTitle, moduleIconFile, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddPage adds a Tab Page.
        /// </summary>
        /// <remarks>
        /// Adds a Tab to a parentTab.
        /// </remarks>
        ///     <param name="parentTab">The Parent Tab.</param>
        ///     <param name="tabName">The Name to give this new Tab.</param>
        /// <param name="description">Description.</param>
        ///     <param name="tabIconFile">The Icon for this new Tab.</param>
        /// <param name="tabIconFileLarge">The Large Icon for this new Tab.</param>
        ///     <param name="isVisible">A flag indicating whether the tab is visible.</param>
        ///     <param name="permissions">Page Permissions Collection for this page.</param>
        /// <param name="isAdmin">Is an admin page.</param>
        /// -----------------------------------------------------------------------------
        private static TabInfo AddPage(TabInfo parentTab, string tabName, string description, string tabIconFile, string tabIconFileLarge, bool isVisible, TabPermissionCollection permissions, bool isAdmin)
        {
            int parentId = Null.NullInteger;
            int portalId = Null.NullInteger;

            if (parentTab != null)
            {
                parentId = parentTab.TabID;
                portalId = parentTab.PortalID;
            }

            return AddPage(portalId, parentId, tabName, description, tabIconFile, tabIconFileLarge, isVisible, permissions, isAdmin);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddPage adds a Tab Page.
        /// </summary>
        ///     <param name="portalId">The Id of the Portal.</param>
        ///     <param name="parentId">The Id of the Parent Tab.</param>
        ///     <param name="tabName">The Name to give this new Tab.</param>
        /// <param name="description">Description.</param>
        ///     <param name="tabIconFile">The Icon for this new Tab.</param>
        /// <param name="tabIconFileLarge">The large Icon for this new Tab.</param>
        ///     <param name="isVisible">A flag indicating whether the tab is visible.</param>
        ///     <param name="permissions">Page Permissions Collection for this page.</param>
        /// <param name="isAdmin">Is and admin page.</param>
        /// -----------------------------------------------------------------------------
        private static TabInfo AddPage(int portalId, int parentId, string tabName, string description, string tabIconFile, string tabIconFileLarge, bool isVisible, TabPermissionCollection permissions, bool isAdmin)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "AddPage:" + tabName);

            TabInfo tab = TabController.Instance.GetTabByName(tabName, portalId, parentId);

            if (tab == null || tab.ParentId != parentId)
            {
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

                if (permissions != null)
                {
                    foreach (TabPermissionInfo tabPermission in permissions)
                    {
                        tab.TabPermissions.Add(tabPermission, true);
                    }

                    TabPermissionController.SaveTabPermissions(tab);
                }
            }

            return tab;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddPagePermission adds a TabPermission to a TabPermission Collection.
        /// </summary>
        ///     <param name="permissions">Page Permissions Collection for this page.</param>
        ///     <param name="key">The Permission key.</param>
        ///     <param name="roleId">The role given the permission.</param>
        /// -----------------------------------------------------------------------------
        private static void AddPagePermission(TabPermissionCollection permissions, string key, int roleId)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "AddPagePermission:" + key);
            var permissionController = new PermissionController();
            var permission = (PermissionInfo)permissionController.GetPermissionByCodeAndKey("SYSTEM_TAB", key)[0];

            var tabPermission = new TabPermissionInfo { PermissionID = permission.PermissionID, RoleID = roleId, AllowAccess = true };

            permissions.Add(tabPermission);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddSearchResults adds a top level Hidden Search Results Page.
        /// </summary>
        ///     <param name="moduleDefId">The Module Deinition Id for the Search Results Module.</param>
        /// -----------------------------------------------------------------------------
        private static void AddSearchResults(int moduleDefId)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "AddSearchResults:" + moduleDefId);
            var portals = PortalController.Instance.GetPortals();
            int intPortal;

            // Add Page to Admin Menu of all configured Portals
            for (intPortal = 0; intPortal <= portals.Count - 1; intPortal++)
            {
                var tabPermissions = new TabPermissionCollection();

                var portal = (PortalInfo)portals[intPortal];

                AddPagePermission(tabPermissions, "View", Convert.ToInt32(Globals.glbRoleAllUsers));
                AddPagePermission(tabPermissions, "View", Convert.ToInt32(portal.AdministratorRoleId));
                AddPagePermission(tabPermissions, "Edit", Convert.ToInt32(portal.AdministratorRoleId));

                // Create New Page (or get existing one)
                var tab = AddPage(portal.PortalID, Null.NullInteger, "Search Results", string.Empty, string.Empty, string.Empty, false, tabPermissions, false);

                // Add Module To Page
                AddModuleToPage(tab, moduleDefId, "Search Results", string.Empty);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddSkinControl adds a new Module Control to the system.
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///     <param name="controlKey">The key for this control in the Definition.</param>
        /// <param name="packageName">Package Name.</param>
        ///     <param name="controlSrc">Te source of ths control.</param>
        /// -----------------------------------------------------------------------------
        private static void AddSkinControl(string controlKey, string packageName, string controlSrc)
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "AddSkinControl:" + controlKey);

            // check if skin control exists
            SkinControlInfo skinControl = SkinControlController.GetSkinControlByKey(controlKey);
            if (skinControl == null)
            {
                var package = new PackageInfo { Name = packageName, FriendlyName = string.Concat(controlKey, "SkinObject"), PackageType = "SkinObject", Version = new Version(1, 0, 0) };
                LegacyUtil.ParsePackageName(package);

                PackageController.Instance.SaveExtensionPackage(package);

                skinControl = new SkinControlInfo { PackageID = package.PackageID, ControlKey = controlKey, ControlSrc = controlSrc, SupportsPartialRendering = false };

                SkinControlController.SaveSkinControl(skinControl);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CoreModuleExists determines whether a Core Module exists on the system.
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///     <param name="desktopModuleName">The Friendly Name of the Module.</param>
        ///     <returns>True if the Module exists, otherwise False.</returns>
        /// -----------------------------------------------------------------------------
        private static bool CoreModuleExists(string desktopModuleName)
        {
            var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(desktopModuleName, Null.NullInteger);

            return desktopModule != null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// HostTabExists determines whether a tab of a given name exists under the Host tab.
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///     <param name="tabName">The Name of the Tab.</param>
        ///     <returns>True if the Tab exists, otherwise False.</returns>
        /// -----------------------------------------------------------------------------
        private static bool HostTabExists(string tabName)
        {
            bool tabExists = false;
            var hostTab = TabController.Instance.GetTabByName("Host", Null.NullInteger);

            var tab = TabController.Instance.GetTabByName(tabName, Null.NullInteger, hostTab.TabID);
            if (tab != null)
            {
                tabExists = true;
            }

            return tabExists;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// InstallMemberRoleProviderScript - Installs a specific MemberRole Provider script.
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///     <param name="providerPath">The Path to the Provider Directory.</param>
        ///     <param name="scriptFile">The Name of the Script File.</param>
        ///     <param name="writeFeedback">Whether or not to echo results.</param>
        private static string InstallMemberRoleProviderScript(string providerPath, string scriptFile, bool writeFeedback)
        {
            if (writeFeedback)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, "Executing Script: " + scriptFile + "<br>");
            }

            string exceptions = DataProvider.Instance().ExecuteScript(FileSystemUtils.ReadFile(providerPath + scriptFile + ".sql"));

            // log the results
            try
            {
                using (StreamWriter streamWriter = File.CreateText(providerPath + scriptFile + ".log.resources"))
                {
                    streamWriter.WriteLine(exceptions);
                    streamWriter.Close();
                }
            }
            catch (Exception exc)
            {
                // does not have permission to create the log file
                Logger.Error(exc);
            }

            return exceptions;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ParseFiles parses the Host Template's Files node.
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///     <param name="node">The Files node.</param>
        ///     <param name="portalId">The PortalId (-1 for Host Files).</param>
        /// -----------------------------------------------------------------------------
        private static void ParseFiles(XmlNode node, int portalId)
        {
            // Parse the File nodes
            if (node != null)
            {
                XmlNodeList nodes = node.SelectNodes("file");
                if (nodes != null)
                {
                    var folderManager = FolderManager.Instance;
                    var fileManager = FileManager.Instance;

                    foreach (XmlNode fileNode in nodes)
                    {
                        string fileName = XmlUtils.GetNodeValue(fileNode.CreateNavigator(), "filename");
                        string extension = XmlUtils.GetNodeValue(fileNode.CreateNavigator(), "extension");
                        long size = long.Parse(XmlUtils.GetNodeValue(fileNode.CreateNavigator(), "size"));
                        int width = XmlUtils.GetNodeValueInt(fileNode, "width");
                        int height = XmlUtils.GetNodeValueInt(fileNode, "height");
                        string contentType = XmlUtils.GetNodeValue(fileNode.CreateNavigator(), "contentType");
                        string folder = XmlUtils.GetNodeValue(fileNode.CreateNavigator(), "folder");

                        var folderInfo = folderManager.GetFolder(portalId, folder);
                        var file = new FileInfo(portalId, fileName, extension, (int)size, width, height, contentType, folder, folderInfo.FolderID, folderInfo.StorageLocation, true);

                        using (var fileContent = fileManager.GetFileContent(file))
                        {
                            var addedFile = fileManager.AddFile(folderInfo, file.FileName, fileContent, false);

                            file.FileId = addedFile.FileId;
                            file.EnablePublishPeriod = addedFile.EnablePublishPeriod;
                            file.EndDate = addedFile.EndDate;
                            file.StartDate = addedFile.StartDate;
                        }

                        fileManager.UpdateFile(file);
                    }
                }
            }
        }

        private static void UpgradeToVersion700()
        {
            // add the site Advanced Settings module to the admin tab
            if (CoreModuleExists("AdvancedSettings") == false)
            {
                var moduleDefId = AddModuleDefinition("AdvancedSettings", string.Empty, "Advanced Settings");
                AddModuleControl(moduleDefId, string.Empty, string.Empty, "DesktopModules/Admin/AdvancedSettings/AdvancedSettings.ascx", string.Empty, SecurityAccessLevel.Admin, 0);
                AddAdminPages(
                    "Advanced Settings",
                    string.Empty,
                    "~/Icons/Sigma/AdvancedSettings_16X16_Standard.png",
                    "~/Icons/Sigma/AdvancedSettings_32X32_Standard.png",
                    true,
                    moduleDefId,
                    "Advanced Settings",
                    "~/Icons/Sigma/AdvancedSettings_16X16_Standard.png");
            }

            ConvertCoreNotificationTypeActionsFor700();

            // Remove Feed Explorer module
            DesktopModuleController.DeleteDesktopModule("FeedExplorer");
            DesktopModuleController.DeleteDesktopModule("Solutions");

            // Register Newtonsoft assembly
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "Newtonsoft.Json.dll", "4.5.6");

            // subhost.aspx was updated
            UpdateChildPortalsDefaultPage();
        }

        private static void UpgradeToVersion710()
        {
            // create a placeholder entry - uses the most common 5 character password (seed list is 6 characters and above)
            const string listName = "BannedPasswords";
            var listController = new ListController();
            var entry = new ListEntryInfo();
            {
                entry.DefinitionID = Null.NullInteger;
                entry.PortalID = Null.NullInteger;
                entry.ListName = listName;
                entry.Value = "12345";
                entry.Text = "Placeholder";
                entry.SystemList = false;
            }

            // add list to each portal and update primary alias
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                entry.PortalID = portal.PortalID;
                entry.SystemList = false;
                entry.ListName = listName + "-" + portal.PortalID;
                listController.AddListEntry(entry);

                var defaultAlias = PortalController.GetPortalSetting("DefaultPortalAlias", portal.PortalID, string.Empty);
                if (!string.IsNullOrEmpty(defaultAlias))
                {
                    foreach (var alias in PortalAliasController.Instance.GetPortalAliasesByPortalId(portal.PortalID).Where(alias => alias.HTTPAlias == defaultAlias))
                    {
                        alias.IsPrimary = true;
                        PortalAliasController.Instance.UpdatePortalAlias(alias);
                    }
                }
            }

            // Add File Content Type
            var typeController = new ContentTypeController();
            var contentTypeFile = (from t in typeController.GetContentTypes() where t.ContentType == "File" select t).SingleOrDefault();

            if (contentTypeFile == null)
            {
                typeController.AddContentType(new ContentType { ContentType = "File" });
            }

            var fileContentType = (from t in typeController.GetContentTypes() where t.ContentType == "File" select t).SingleOrDefault();

            // only perform following for an existing installation upgrading
            if (Globals.Status == Globals.UpgradeStatus.Upgrade)
            {
                UpdateFoldersForParentId();
                ImportDocumentLibraryCategories();
                ImportDocumentLibraryCategoryAssoc(fileContentType);
            }

            // Add 404 Log
            var logTypeInfo = new LogTypeInfo
            {
                LogTypeKey = EventLogController.EventLogType.PAGE_NOT_FOUND_404.ToString(),
                LogTypeFriendlyName = "HTTP Error Code 404 Page Not Found",
                LogTypeDescription = string.Empty,
                LogTypeCSSClass = "OperationFailure",
                LogTypeOwner = "DotNetNuke.Logging.EventLogType",
            };
            LogController.Instance.AddLogType(logTypeInfo);

            // Add LogType
            var logTypeConf = new LogTypeConfigInfo
            {
                LoggingIsActive = true,
                LogTypeKey = EventLogController.EventLogType.PAGE_NOT_FOUND_404.ToString(),
                KeepMostRecent = "100",
                NotificationThreshold = 1,
                NotificationThresholdTime = 1,
                NotificationThresholdTimeType = LogTypeConfigInfo.NotificationThresholdTimeTypes.Seconds,
                MailFromAddress = Null.NullString,
                MailToAddress = Null.NullString,
                LogTypePortalID = "*",
            };
            LogController.Instance.AddLogTypeConfigInfo(logTypeConf);

            UninstallPackage("DotNetNuke.SearchInput", "Module");

            // enable password strength meter for new installs only
            HostController.Instance.Update("EnableStrengthMeter", Globals.Status == Globals.UpgradeStatus.Install ? "Y" : "N");

            // Add IP filter log type
            var logTypeFilterInfo = new LogTypeInfo
            {
                LogTypeKey = EventLogController.EventLogType.IP_LOGIN_BANNED.ToString(),
                LogTypeFriendlyName = "HTTP Error Code 403.6 forbidden ip address rejected",
                LogTypeDescription = string.Empty,
                LogTypeCSSClass = "OperationFailure",
                LogTypeOwner = "DotNetNuke.Logging.EventLogType",
            };
            LogController.Instance.AddLogType(logTypeFilterInfo);

            // Add LogType
            var logTypeFilterConf = new LogTypeConfigInfo
            {
                LoggingIsActive = true,
                LogTypeKey = EventLogController.EventLogType.IP_LOGIN_BANNED.ToString(),
                KeepMostRecent = "100",
                NotificationThreshold = 1,
                NotificationThresholdTime = 1,
                NotificationThresholdTimeType = LogTypeConfigInfo.NotificationThresholdTimeTypes.Seconds,
                MailFromAddress = Null.NullString,
                MailToAddress = Null.NullString,
                LogTypePortalID = "*",
            };
            LogController.Instance.AddLogTypeConfigInfo(logTypeFilterConf);

            int tabID = TabController.GetTabByTabPath(Null.NullInteger, "//Host//SearchAdmin", Null.NullString);
            if (tabID > Null.NullInteger)
            {
                TabController.Instance.DeleteTab(tabID, Null.NullInteger);
            }

            var modDef = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("Search Admin");

            if (modDef != null)
            {
                AddAdminPages("Search Admin", "Manage search settings associated with DotNetNuke's search capability.", "~/Icons/Sigma/Search_16x16_Standard.png", "~/Icons/Sigma/Search_32x32_Standard.png", true, modDef.ModuleDefID, "Search Admin", string.Empty);
            }

            CopyGettingStartedStyles();
        }

        private static void UpgradeToVersion711()
        {
            DesktopModuleController.DeleteDesktopModule("FileManager");

            // Add TabUrl Logtypes
            var logTypeInfo = new LogTypeInfo
            {
                LogTypeKey = EventLogController.EventLogType.TABURL_CREATED.ToString(),
                LogTypeFriendlyName = "TabURL created",
                LogTypeDescription = string.Empty,
                LogTypeCSSClass = "OperationSuccess",
                LogTypeOwner = "DotNetNuke.Logging.EventLogType",
            };
            LogController.Instance.AddLogType(logTypeInfo);

            logTypeInfo.LogTypeKey = EventLogController.EventLogType.TABURL_UPDATED.ToString();
            logTypeInfo.LogTypeFriendlyName = "TabURL updated";
            LogController.Instance.AddLogType(logTypeInfo);

            logTypeInfo.LogTypeKey = EventLogController.EventLogType.TABURL_DELETED.ToString();
            logTypeInfo.LogTypeFriendlyName = "TabURL deleted";
            LogController.Instance.AddLogType(logTypeInfo);
        }

        private static void UpgradeToVersion712()
        {
            // update console module in Admin/Host page to set OrderTabsByHierarchy setting to true.
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                var tabId = TabController.GetTabByTabPath(portal.PortalID, "//Admin", Null.NullString);
                if (tabId != Null.NullInteger)
                {
                    foreach (var module in ModuleController.Instance.GetTabModules(tabId).Where(m => m.Value.ModuleDefinition.FriendlyName == "Console"))
                    {
                        ModuleController.Instance.UpdateModuleSetting(module.Key, "OrderTabsByHierarchy", "True");
                    }
                }
            }

            var hostTabId = TabController.GetTabByTabPath(Null.NullInteger, "//Host", Null.NullString);
            if (hostTabId != Null.NullInteger)
            {
                foreach (var module in ModuleController.Instance.GetTabModules(hostTabId).Where(m => m.Value.ModuleDefinition.FriendlyName == "Console"))
                {
                    ModuleController.Instance.UpdateModuleSetting(module.Key, "OrderTabsByHierarchy", "True");
                }
            }
        }

        private static void UpgradeToVersion720()
        {
            var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName("51Degrees.mobi", Null.NullInteger);
            if (desktopModule != null)
            {
                DesktopModuleController.RemoveDesktopModuleFromPortals(desktopModule.DesktopModuleID);
            }

            desktopModule = DesktopModuleController.GetDesktopModuleByModuleName("DotNetNuke.RadEditorProvider", Null.NullInteger);
            if (desktopModule != null)
            {
                DesktopModuleController.RemoveDesktopModuleFromPortals(desktopModule.DesktopModuleID);
            }

            // ensure old codeplex module is uninstalled - need to check for both variants of package name
            UninstallPackage("DotNetNuke.Module Creator", "Module");
            UninstallPackage("DNNCorp.ModuleCreator", "Module");

            DesktopModuleController.AddModuleCategory("Developer");
            var moduleDefId = AddModuleDefinition("Module Creator", "Development of modules.", "Module Creator");
            AddModuleControl(moduleDefId, string.Empty, string.Empty, "DesktopModules/Admin/ModuleCreator/CreateModule.ascx", "~/DesktopModules/Admin/ModuleCreator/icon.png", SecurityAccessLevel.Host, 0);
            if (ModuleDefinitionController.GetModuleDefinitionByID(moduleDefId) != null)
            {
                var desktopModuleId = ModuleDefinitionController.GetModuleDefinitionByID(moduleDefId).DesktopModuleID;
                desktopModule = DesktopModuleController.GetDesktopModule(desktopModuleId, Null.NullInteger);
                desktopModule.Category = "Developer";
                DesktopModuleController.SaveDesktopModule(desktopModule, false, false);

                var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == desktopModule.PackageID);
                package.IconFile = "~/Icons/Sigma/ModuleCreator_32x32.png";
                PackageController.Instance.SaveExtensionPackage(package);
            }

            var typeController = new ContentTypeController();
            var fileContentType = (from t in typeController.GetContentTypes() where t.ContentType == "File" select t).SingleOrDefault();

            // only perform following for an existing installation upgrading
            if (Globals.Status == Globals.UpgradeStatus.Upgrade)
            {
                ImportDocumentLibraryCategories();
                ImportDocumentLibraryCategoryAssoc(fileContentType);
            }

            // fixes issue introduced by eventlog's being defined in upgrade.cs
            PortalController.EnsureRequiredEventLogTypesExist();

            // Remove Professional Features pages from CE
            int advancedFeaturesTabId = TabController.GetTabByTabPath(Null.NullInteger, "//Host//ProfessionalFeatures", Null.NullString);
            if (DotNetNukeContext.Current.Application.Name == "DNNCORP.CE")
            {
                foreach (var tab in TabController.GetTabsByParent(advancedFeaturesTabId, Null.NullInteger))
                {
                    TabController.Instance.DeleteTab(tab.TabID, Null.NullInteger);
                }

                TabController.Instance.DeleteTab(advancedFeaturesTabId, Null.NullInteger);
            }

            // Remove Whats New
            int whatsNewTabId = TabController.GetTabByTabPath(Null.NullInteger, "//Host//WhatsNew", Null.NullString);
            TabController.Instance.DeleteTab(whatsNewTabId, Null.NullInteger);

            // Remove WhatsNew module
            DesktopModuleController.DeleteDesktopModule("WhatsNew");

            // read plaintext password via old API and encrypt
            var current = HostController.Instance.GetString("SMTPPassword");
            if (!string.IsNullOrEmpty(current))
            {
                HostController.Instance.UpdateEncryptedString("SMTPPassword", current, Config.GetDecryptionkey());
            }
        }

        private static void UpgradeToVersion721()
        {
            try
            {
                // the username maybe html encode when register in 7.1.2, it will caught unicode charactors changed, need use InputFilter to correct the value.
                var portalSecurity = PortalSecurity.Instance;
                using (var reader = DataProvider.Instance().ExecuteSQL("SELECT UserID, Username FROM {databaseOwner}[{objectQualifier}Users] WHERE Username LIKE '%&%'"))
                {
                    while (reader.Read())
                    {
                        var userId = Convert.ToInt32(reader["UserID"]);
                        var userName = reader["Username"].ToString();

                        if (userName != HttpUtility.HtmlDecode(userName))
                        {
                            userName = portalSecurity.InputFilter(
                                HttpUtility.HtmlDecode(userName),
                                PortalSecurity.FilterFlag.NoScripting |
                                                                 PortalSecurity.FilterFlag.NoAngleBrackets |
                                                                 PortalSecurity.FilterFlag.NoMarkup);

                            UserController.ChangeUsername(userId, userName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            AddManageUsersModulePermissions();
        }

        private static void UpgradeToVersion722()
        {
            UninstallPackage("DotNetNuke.Messaging", "Module");

            // add event log type:POTENTIAL_PAYPAL_PAYMENT_FRAUD
            if (!DoesLogTypeExists(EventLogController.EventLogType.POTENTIAL_PAYPAL_PAYMENT_FRAUD.ToString()))
            {
                var logTypeInfo = new LogTypeInfo
                {
                    LogTypeKey =
                                              EventLogController.EventLogType.POTENTIAL_PAYPAL_PAYMENT_FRAUD.ToString(),
                    LogTypeFriendlyName = "Potential Paypal Payment Fraud",
                    LogTypeDescription = string.Empty,
                    LogTypeCSSClass = "OperationFailure",
                    LogTypeOwner = "DotNetNuke.Logging.EventLogType",
                };
                LogController.Instance.AddLogType(logTypeInfo);
            }

            // AdvancedSettings module needs to be made a system package
            var pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.AdvancedSettings");
            if (pkg != null)
            {
                pkg.IsSystemPackage = true;
                PackageController.Instance.SaveExtensionPackage(pkg);
            }

            // Site Wizard module needs to be made a system package
            pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.SiteWizard");
            if (pkg != null)
            {
                pkg.IsSystemPackage = true;
                PackageController.Instance.SaveExtensionPackage(pkg);
            }

            // Site Log module needs to be made a system package
            pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.SiteLog");
            if (pkg != null)
            {
                pkg.IsSystemPackage = true;
                PackageController.Instance.SaveExtensionPackage(pkg);
            }

            // Module Creator module needs to be made a system package
            pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Module Creator");
            if (pkg != null)
            {
                pkg.IsSystemPackage = true;
                PackageController.Instance.SaveExtensionPackage(pkg);
            }

            // Telerik.Web module needs to be made a system package
            pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Telerik.Web");
            if (pkg != null)
            {
                pkg.IsSystemPackage = true;
                PackageController.Instance.SaveExtensionPackage(pkg);
            }

            // jQuery needs to be made a system package
            pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "jQuery");
            if (pkg != null)
            {
                pkg.IsSystemPackage = true;
                PackageController.Instance.SaveExtensionPackage(pkg);
            }

            // jQuery-Migrate needs to be made a system package
            pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "jQuery-Migrate");
            if (pkg != null)
            {
                pkg.IsSystemPackage = true;
                PackageController.Instance.SaveExtensionPackage(pkg);
            }

            // Make ConfigurationManager Premium
            MakeModulePremium(@"ConfigurationManager");

            // Make ConfigurationManager Premium
            MakeModulePremium(@"Dashboard");

            // Make ProfessionalPreview Premium
            MakeModulePremium(@"ProfessionalPreview");
        }

        private static void UpgradeToVersion730()
        {
#pragma warning disable 612,618
            if (jQuery.UseHostedScript)
            {
                HostController.Instance.Update("CDNEnabled", "True", true);

                var jquery = JavaScriptLibraryController.Instance.GetLibrary(l => l.LibraryName == CommonJs.jQuery);
                jquery.CDNPath = jQuery.HostedUrl;
                JavaScriptLibraryController.Instance.SaveLibrary(jquery);

                var jqueryui = JavaScriptLibraryController.Instance.GetLibrary(l => l.LibraryName == CommonJs.jQueryUI);
                jqueryui.CDNPath = jQuery.HostedUIUrl;
                JavaScriptLibraryController.Instance.SaveLibrary(jqueryui);

                var jquerymigrate = JavaScriptLibraryController.Instance.GetLibrary(l => l.LibraryName == CommonJs.jQueryMigrate);
                jquerymigrate.CDNPath = jQuery.HostedMigrateUrl;
                JavaScriptLibraryController.Instance.SaveLibrary(jquerymigrate);
            }
#pragma warning restore 612,618
        }

        private static void UpgradeToVersion732()
        {
            // Register System referenced 3rd party assemblies.
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "Lucene.Net.dll", "3.0.3");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "Lucene.Net.Contrib.FastVectorHighlighter.dll", "3.0.3");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "Lucene.Net.Contrib.Analyzers.dll", "3.0.3");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "Microsoft.ApplicationBlocks.Data.dll", "2.0.0");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "Microsoft.Web.Helpers.dll", "2.0.20710");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "Microsoft.Web.Infrastructure.dll", "1.0.20105");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "Newtonsoft.Json.dll", "4.5.7");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "PetaPoco.dll", "5.0.1");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "SharpZipLib.dll", "0.81.0");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "System.Net.Http.dll", "1.0.0");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "System.Net.Http.Formatting.dll", "4.0.20710");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "System.Net.Http.WebRequest.dll", "1.0.0");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "System.Web.Helpers.dll", "2.0.20216");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "System.Web.Http.dll", "4.0.20710");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "System.Web.Http.WebHost.dll", "4.0.20710");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "System.Web.Razor.dll", "2.0.20216");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "System.Web.WebPages.Deployment.dll", "2.0.20710");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "System.Web.WebPages.dll", "2.0.20710");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "System.Web.WebPages.Razor.dll", "2.0.20126");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "WebFormsMvp.dll", "1.4.1");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "WebMatrix.Data.dll", "2.0.20126");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "WebMatrix.WebData.dll", "2.0.20126");

            // update help url
            HostController.Instance.Update("HelpURL", "https://dnndocs.com", false);
        }

        private static void UpgradeToVersion733()
        {
            var notificationType = new NotificationType { Name = "NewUnauthorizedUserRegistration", Description = "New Unauthorized User Registration Notification" };
            NotificationsController.Instance.CreateNotificationType(notificationType);

            var actions = new List<NotificationTypeAction>
                              {
                                  new NotificationTypeAction
                                      {
                                          NameResourceKey = "AuthorizeUser",
                                          DescriptionResourceKey = "AuthorizeUserDescription",
                                          APICall = "API/InternalServices/NewUserNotificationService/Authorize",
                                      },
                                  new NotificationTypeAction
                                      {
                                          NameResourceKey = "RejectUser",
                                          DescriptionResourceKey = "RejectUserDescription",
                                          APICall = "API/InternalServices/NewUserNotificationService/Reject"
                                      },
                              };

            NotificationsController.Instance.SetNotificationTypeActions(actions, notificationType.NotificationTypeId);
        }

        private static void UpgradeToVersion740()
        {
            string PageHeadTextForUpgrade = "<meta content=\"text/html; charset=UTF-8\" http-equiv=\"Content-Type\" />" + "\n" +
                                               "<meta name=\"REVISIT-AFTER\" content=\"1 DAYS\" />" + "\n" +
                                               "<meta name=\"RATING\" content=\"GENERAL\" />" + "\n" +

                                                "<meta name=\"RESOURCE-TYPE\" content=\"DOCUMENT\" />" + "\n" +
                                                "<meta content=\"text/javascript\" http-equiv=\"Content-Script-Type\" />" + "\n" +
                                                "<meta content=\"text/css\" http-equiv=\"Content-Style-Type\" />" + "\n";
            ArrayList portals = PortalController.Instance.GetPortals();
            foreach (PortalInfo portal in portals)
            {
                PortalController.UpdatePortalSetting(portal.PortalID, "PageHeadText", PageHeadTextForUpgrade);
            }

            RemoveContentListModuleFromSearchResultsPage();
            ReIndexUserSearch();
        }

        private static void UpgradeToVersion742()
        {
            var containerFolder = string.Format("{0}Containers\\DarkKnightMobile", Globals.HostMapPath);
            var skinFolder = string.Format("{0}Skins\\DarkKnightMobile", Globals.HostMapPath);
            if (!Directory.Exists(skinFolder))
            {
                UninstallPackage("DarkKnightMobile", "Skin"); // Skin
            }

            if (!Directory.Exists(containerFolder))
            {
                UninstallPackage("DarkKnightMobile", "Container"); // Container
            }
        }

        private static void ReIndexUserSearch()
        {
            var portals = PortalController.Instance.GetPortals();
            foreach (PortalInfo portal in portals)
            {
                PortalController.UpdatePortalSetting(portal.PortalID, UserIndexer.UserIndexResetFlag, "TRUE");
            }
        }

        private static void RemoveContentListModuleFromSearchResultsPage()
        {
            var portals = PortalController.Instance.GetPortals();
            foreach (PortalInfo portal in portals)
            {
                foreach (KeyValuePair<int, ModuleInfo> kvp in ModuleController.Instance.GetTabModules(portal.SearchTabId))
                {
                    var module = kvp.Value;
                    if (module.DesktopModule.FriendlyName == "ContentList")
                    {
                        // Delete the Module from the Modules list
                        ModuleController.Instance.DeleteTabModule(module.TabID, module.ModuleID, false);
                        break;
                    }
                }
            }
        }

        private static void AddManageUsersModulePermissions()
        {
            var permCtl = new PermissionController();
            var desktopInfo = DesktopModuleController.GetDesktopModuleByModuleName("Security", Null.NullInteger);

            // add new user dialog
            var md = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("User Account", desktopInfo.DesktopModuleID);
            try
            {
                var pi = new PermissionInfo
                {
                    ModuleDefID = md.ModuleDefID,
                    PermissionCode = "SECURITY_MODULE",
                    PermissionKey = "MANAGEUSER",
                    PermissionName = "Manage User",
                };

                permCtl.AddPermission(pi);
            }
            catch
            {
                // suppress
            }

            md = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("User Accounts", desktopInfo.DesktopModuleID);
            try
            {
                var pi = new PermissionInfo
                {
                    ModuleDefID = md.ModuleDefID,
                    PermissionCode = "SECURITY_MODULE",
                    PermissionKey = "MANAGEUSERS",
                    PermissionName = "Manage Users",
                };

                permCtl.AddPermission(pi);
            }
            catch
            {
                // suppress
            }

            md = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("Security Roles", desktopInfo.DesktopModuleID);
            try
            {
                var pi = new PermissionInfo
                {
                    ModuleDefID = md.ModuleDefID,
                    PermissionCode = "SECURITY_MODULE",
                    PermissionKey = "MANAGEROLES",
                    PermissionName = "Manage Roles",
                };

                permCtl.AddPermission(pi);
            }
            catch
            {
                // suppress
            }
        }

        private static ContentItem CreateFileContentItem()
        {
            var typeController = new ContentTypeController();
            var contentTypeFile = (from t in typeController.GetContentTypes() where t.ContentType == "File" select t).SingleOrDefault();

            if (contentTypeFile == null)
            {
                contentTypeFile = new ContentType { ContentType = "File" };
                contentTypeFile.ContentTypeId = typeController.AddContentType(contentTypeFile);
            }

            var objContent = new ContentItem
            {
                ContentTypeId = contentTypeFile.ContentTypeId,
                Indexed = false,
            };

            objContent.ContentItemId = DotNetNuke.Entities.Content.Common.Util.GetContentController().AddContentItem(objContent);

            return objContent;
        }

        private static void ImportDocumentLibraryCategoryAssoc(ContentType fileContentType)
        {
            DataProvider dataProvider = DataProvider.Instance();
            IDataReader dr;
            try
            {
                var termController = new TermController();
                var vocabulary = new VocabularyController().GetVocabularies().Single(v => v.Name == "Tags");
                var terms = termController.GetTermsByVocabulary(vocabulary.VocabularyId);
                using (dr = dataProvider.ExecuteReader("ImportDocumentLibraryCategoryAssoc"))
                {
                    while (dr.Read())
                    {
                        var file = FileManager.Instance.GetFile((int)dr["FileId"]);
                        ContentItem attachContentItem;
                        if (file.ContentItemID == Null.NullInteger)
                        {
                            attachContentItem = CreateFileContentItem();
                            file.ContentItemID = attachContentItem.ContentItemId;
                            FileManager.Instance.UpdateFile(file);
                        }
                        else
                        {
                            attachContentItem = Util.GetContentController().GetContentItem(file.ContentItemID);
                        }

                        var term = terms.SingleOrDefault(t => t.Name == dr["CategoryName"].ToString());
                        if (term == null)
                        {
                            term = new Term(dr["CategoryName"].ToString(), null, vocabulary.VocabularyId);
                            termController.AddTerm(term);
                        }

                        termController.AddTermToContent(term, attachContentItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static void ImportDocumentLibraryCategories()
        {
            VocabularyController vocabularyController = new VocabularyController();
            var defaultTags = (from v in vocabularyController.GetVocabularies() where v.IsSystem && v.Name == "Tags" select v).SingleOrDefault();

            DataProvider dataProvider = DataProvider.Instance();
            dataProvider.ExecuteNonQuery("ImportDocumentLibraryCategories", defaultTags.VocabularyId);
        }

        private static void UpdateFoldersForParentId()
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "UpgradeFolders");

            // Move old messages to new format. Do this in smaller batches so we can send feedback to browser and don't time out
            var foldersToConvert = DataProvider.Instance().GetLegacyFolderCount();
            var foldersRemaining = foldersToConvert;

            if (foldersRemaining > 0)
            {
                // Create an empty line
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, "<br/>", false);
            }

            while (foldersRemaining > 0)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, string.Format("Converting old Folders to new format. Total: {0} [Remaining: {1}]<br/>", foldersToConvert, foldersRemaining));
                try
                {
                    using (DataProvider.Instance().UpdateLegacyFolders())
                    {
                    }
                }
                catch (Exception ex)
                {
                    Exceptions.Exceptions.LogException(ex);
                }

                foldersRemaining = DataProvider.Instance().GetLegacyFolderCount();
            }

            if (foldersToConvert > 0)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, string.Format("Conversion of old Folders Completed. Total Converted: {0}<br/>", foldersToConvert));
            }
        }

        private static void UninstallPackage(string packageName, string packageType, bool deleteFiles = true, string version = "")
        {
            DnnInstallLogger.InstallLogInfo(string.Concat(Localization.GetString("LogStart", Localization.GlobalResourceFile), "Uninstallation of Package:", packageName, " Type:", packageType, " Version:", version));

            var searchInput = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p =>
                p.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase)
                && p.PackageType.Equals(packageType, StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrEmpty(version) || p.Version.ToString() == version));
            if (searchInput != null)
            {
                var searchInputInstaller = new Installer(searchInput, Globals.ApplicationMapPath);
                searchInputInstaller.UnInstall(deleteFiles);
            }
        }

        private static void ConvertCoreNotificationTypeActionsFor700()
        {
            var notificationTypeNames = new[] { "FriendRequest", "FollowerRequest", "FollowBackRequest", "TranslationSubmitted" };

            foreach (var name in notificationTypeNames)
            {
                var nt = NotificationsController.Instance.GetNotificationType(name);

                if (nt != null)
                {
                    var actions = NotificationsController.Instance.GetNotificationTypeActions(nt.NotificationTypeId).ToList();

                    if (actions.Any())
                    {
                        foreach (var action in actions)
                        {
                            action.APICall = action.APICall.Replace(".ashx", string.Empty);
                            NotificationsController.Instance.DeleteNotificationTypeAction(
                                action.NotificationTypeActionId);
                        }

                        NotificationsController.Instance.SetNotificationTypeActions(actions, nt.NotificationTypeId);
                    }
                }
                else
                {
                    AddMissingNotificationTypes(name);
                }
            }
        }

        private static void AddMissingNotificationTypes(string notificationTypeName)
        {
            switch (notificationTypeName)
            {
                case "FriendRequest":
                    var friendRequestType = new NotificationType { Name = notificationTypeName, Description = "Friend Request" };
                    var friendRequestTypeActions = new List<NotificationTypeAction>();
                    friendRequestTypeActions.Add(new NotificationTypeAction
                    {
                        NameResourceKey = "Accept",
                        DescriptionResourceKey = "AcceptFriend",
                        APICall = "API/InternalServices/RelationshipService/AcceptFriend",
                    });
                    NotificationsController.Instance.CreateNotificationType(friendRequestType);
                    NotificationsController.Instance.SetNotificationTypeActions(friendRequestTypeActions, friendRequestType.NotificationTypeId);
                    break;
                case "FollowerRequest":
                    var followerRequestType = new NotificationType { Name = notificationTypeName, Description = "Follower Request" };
                    NotificationsController.Instance.CreateNotificationType(followerRequestType);
                    break;
                case "FollowBackRequest":
                    var followBackRequestType = new NotificationType { Name = notificationTypeName, Description = "Follow Back Request" };
                    var followBackRequestTypeActions = new List<NotificationTypeAction>();
                    followBackRequestTypeActions.Add(new NotificationTypeAction
                    {
                        NameResourceKey = "FollowBack",
                        DescriptionResourceKey = "FollowBack",
                        ConfirmResourceKey = string.Empty,
                        APICall = "API/InternalServices/RelationshipService/FollowBack",
                    });
                    NotificationsController.Instance.CreateNotificationType(followBackRequestType);
                    NotificationsController.Instance.SetNotificationTypeActions(followBackRequestTypeActions, followBackRequestType.NotificationTypeId);
                    break;
                case "TranslationSubmitted":
                    var translationSubmittedType = new NotificationType { Name = notificationTypeName, Description = "Translation Submitted" };
                    NotificationsController.Instance.CreateNotificationType(translationSubmittedType);
                    break;
                default:
                    break;
            }
        }

        private static void UpdateChildPortalsDefaultPage()
        {
            DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "UpdateChildPortalsDefaultPage");

            // Update Child Portal subHost.aspx
            foreach (PortalAliasInfo aliasInfo in PortalAliasController.Instance.GetPortalAliases().Values)
            {
                // For the alias to be for a child it must be of the form ...../child
                int intChild = aliasInfo.HTTPAlias.IndexOf("/");
                if (intChild != -1 && intChild != (aliasInfo.HTTPAlias.Length - 1))
                {
                    var childPath = Globals.ApplicationMapPath + "\\" + aliasInfo.HTTPAlias.Substring(intChild + 1);
                    if (!string.IsNullOrEmpty(Globals.ApplicationPath))
                    {
                        childPath = childPath.Replace("\\", "/");
                        childPath = childPath.Replace(Globals.ApplicationPath, string.Empty);
                    }

                    childPath = childPath.Replace("/", "\\");

                    // check if File exists and make sure it's not the site's main default.aspx page
                    string childDefaultPage = childPath + "\\" + Globals.glbDefaultPage;
                    if (childPath != Globals.ApplicationMapPath && File.Exists(childDefaultPage))
                    {
                        var objDefault = new System.IO.FileInfo(childDefaultPage);
                        var objSubHost = new System.IO.FileInfo(Globals.HostMapPath + "subhost.aspx");

                        // check if upgrade is necessary
                        if (objDefault.Length != objSubHost.Length)
                        {
                            // check file is readonly
                            bool wasReadonly = false;
                            FileAttributes attributes = File.GetAttributes(childDefaultPage);
                            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                            {
                                wasReadonly = true;

                                // remove readonly attribute
                                File.SetAttributes(childDefaultPage, FileAttributes.Normal);
                            }

                            // Rename existing file
                            File.Copy(childDefaultPage, childPath + "\\old_" + Globals.glbDefaultPage, true);

                            // copy file
                            File.Copy(Globals.HostMapPath + "subhost.aspx", childDefaultPage, true);

                            // set back the readonly attribute
                            if (wasReadonly)
                            {
                                File.SetAttributes(childDefaultPage, FileAttributes.ReadOnly);
                            }
                        }
                    }
                }
            }
        }

        private static void CopyGettingStartedStyles()
        {
            // copy getting started css to portals folder.
            var hostGettingStartedFile = string.Format("{0}GettingStarted.css", Globals.HostMapPath);
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                if (File.Exists(hostGettingStartedFile))
                {
                    var portalFile = portal.HomeDirectoryMapPath + "GettingStarted.css";
                    if (!File.Exists(portalFile))
                    {
                        File.Copy(hostGettingStartedFile, portalFile);
                    }
                }

                // update the getting started page to have this custom style sheet.
                var gettingStartedTabId = PortalController.GetPortalSettingAsInteger("GettingStartedTabId", portal.PortalID, Null.NullInteger);
                if (gettingStartedTabId > Null.NullInteger)
                {
                    // check if tab exists
                    if (TabController.Instance.GetTab(gettingStartedTabId, portal.PortalID, true) != null)
                    {
                        TabController.Instance.UpdateTabSetting(gettingStartedTabId, "CustomStylesheet", "GettingStarted.css");
                    }
                }
            }
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

        private static bool DoesLogTypeExists(string logTypeKey)
        {
            LogTypeInfo logType;
            Dictionary<string, LogTypeInfo> logTypeDictionary = LogController.Instance.GetLogTypeInfoDictionary();
            logTypeDictionary.TryGetValue(logTypeKey, out logType);
            if (logType == null)
            {
                return false;
            }

            return true;
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

                    PackageController.ParsePackage(file, installPackagePath, packages, invalidPackages);
                    if (packages.ContainsKey(file))
                    {
                        // check whether have version conflict and remove old version.
                        var package = packages[file];

                        var installedPackage = PackageController.Instance.GetExtensionPackage(
                            Null.NullInteger,
                            p => p.Name.Equals(package.Name, StringComparison.OrdinalIgnoreCase)
                                    && p.PackageType.Equals(package.PackageType, StringComparison.OrdinalIgnoreCase));

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

                            if (oldPackages.Any())
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

                    if (extension != ".zip")
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

        private static void UpgradeToVersion8006()
        {
            RemoveAdminPages("//Admin//Languages");
            RemoveAdminPages("//Admin//Lists");
            RemoveAdminPages("//Admin//LogViewer");
            RemoveAdminPages("//Admin//Newsletters");
            RemoveAdminPages("//Admin//Pages");
            RemoveAdminPages("//Admin//RecycleBin");
            RemoveAdminPages("//Admin//SiteLog");
            RemoveAdminPages("//Admin//SiteWizard");
            RemoveAdminPages("//Admin//Vendors");
            RemoveHostPage("Lists");
            RemoveHostPage("Vendors");

            var package = PackageController.Instance.GetExtensionPackage(-1, p => p.Name == "DotNetNuke.Vendors");
            if (package != null)
            {
                PackageController.Instance.DeleteExtensionPackage(package);
            }

            package = PackageController.Instance.GetExtensionPackage(-1, p => p.Name == "DotNetNuke.SiteLog");
            if (package != null)
            {
                PackageController.Instance.DeleteExtensionPackage(package);
            }
        }

        private static void UpgradeToVersion8007()
        {
            RemoveHostPage("Dashboard");
            RemoveHostPage("SQL");
            RemoveHostPage("Configuration Manager");

            UninstallPackage("DotNetNuke.ProfessionalPreview", "Module");
            UninstallPackage("DotNetNuke.Dashboard", "Module");
            UninstallPackage("DotNetNuke.Configuration Manager", "Module");
        }

        private static void UpgradeToVersion80013()
        {
            UninstallPackage("DotNetNuke.Newsletters", "Module");
        }

        private static void UpgradeToVersion80016()
        {
            UninstallPackage("Solutions", "Module");

            RemoveAdminPages("//Admin//GoogleAnalytics");
            UninstallPackage("DotNetNuke.Google Analytics", "Module");

            RemoveAdminPages("//Admin//AdvancedSettings");
            UninstallPackage("DotNetNuke.AdvancedSettings", "Module");
            UninstallPackage("DotNetNuke.ContentList", "Module");

            RemoveAdminPages("//Admin//Skins");
            UninstallPackage("DotNetNuke.Skins", "Module");
            UninstallPackage("DotNetNuke.Skin Designer", "Module");
            UninstallPackage("DotNetNuke.Banners", "Module");

            RemoveGettingStartedPages();
        }

        private static void UpgradeToVersion80026()
        {
            FixTabsMissingLocalizedFields();
        }

        private static void UpgradeToVersion80027()
        {
            RemoveAdminPages("//Admin//DynamicContentTypeManager");
            UninstallPackage("Dnn.DynamicContentManager", "Module");
            UninstallPackage("Dnn.DynamicContentViewer", "Module");
        }

        private static void UpgradeToVersion910()
        {
            RemoveHostPage("Host Settings");
            RemoveHostPage("Site Management");
            RemoveHostPage("Schedule");
            RemoveHostPage("Superuser Accounts");
            RemoveHostPage("Extensions");
            RemoveHostPage("Device Detection Management");

            RemoveAdminPages("//Admin//Extensions");
            RemoveAdminPages("//Admin//SiteSettings");
            RemoveAdminPages("//Admin//SecurityRoles");
            RemoveAdminPages("//Admin//Taxonomy");
            RemoveAdminPages("//Admin//SiteRedirectionManagement");
            RemoveAdminPages("//Admin//DevicePreviewManagement");
            RemoveAdminPages("//Admin//SearchAdmin");

            // Normal Modules
            UninstallPackage("DotNetNuke.MobileManagement", "Module");
            UninstallPackage("DotNetNuke.Modules.PreviewProfileManagement", "Module");

            UninstallPackage("DotNetNuke.Dashboard.WebServer", "DashboardControl");
            UninstallPackage("DotNetNuke.Dashboard.Database", "DashboardControl");
            UninstallPackage("DotNetNuke.Dashboard.Host", "DashboardControl");
            UninstallPackage("DotNetNuke.Dashboard.Portals", "DashboardControl");
            UninstallPackage("DotNetNuke.Dashboard.Modules", "DashboardControl");
            UninstallPackage("DotNetNuke.Dashboard.Skins", "DashboardControl");

            // Admin Modules
            UninstallPackage("DotNetNuke.HostSettings", "Module");
            UninstallPackage("DotNetNuke.Languages", "Module");
            UninstallPackage("DotNetNuke.Lists", "Module");
            UninstallPackage("DotNetNuke.LogViewer", "Module");
            UninstallPackage("DotNetNuke.RecycleBin", "Module");
            UninstallPackage("DotNetNuke.Sitemap", "Module");
            UninstallPackage("DotNetNuke.SiteWizard", "Module");
            UninstallPackage("Dnn.Themes", "Module"); // aka. Skin Management
            UninstallPackage("DotNetNuke.Tabs", "Module");

            // at last remove "/Admin" / "/Host" pages
            UninstallPackage("DotNetNuke.Portals", "Module");
            UninstallPackage("DotNetNuke.Scheduler", "Module");
            UninstallPackage("DotNetNuke.SearchAdmin", "Module");
            UninstallPackage("DotNetNuke.SQL", "Module");
            UninstallPackage("DotNetNuke.Extensions", "Module");
            UninstallPackage("DotNetNuke.Configuration Manager", "Module");
            UninstallPackage("DotNetNuke.Dashboard", "Module");
            UninstallPackage("DotNetNuke.Google Analytics", "Module");
            UninstallPackage("DotNetNuke.Taxonomy", "Module");

            UninstallPackage("UrlManagement", "Library", false);
        }

        private static void UpgradeToVersion920()
        {
            DataProvider.Instance().UnRegisterAssembly(Null.NullInteger, "SharpZipLib.dll");
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "ICSharpCode.SharpZipLib.dll", "0.86.0");

            RemoveAdminPages("//Admin//SearchEngineSiteMap");
            RemoveAdminPages("//Admin//Solutions");
            RemoveAdminPages("//Admin//BulkEmail");

            RemoveHostPage("Marketplace");
            RemoveHostPage("Module Definitions");
            RemoveHostPage("Portals");

            if (!HostTabExists("Superuser Accounts"))
            {
                // add SuperUser Accounts module and tab
                var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName("Security", Null.NullInteger);
                if (desktopModule != null)
                {
                    var moduleDefId = ModuleDefinitionController
                        .GetModuleDefinitionByFriendlyName("User Accounts", desktopModule.DesktopModuleID).ModuleDefID;

                    // Create New Host Page (or get existing one)
                    var newPage = AddHostPage("Superuser Accounts", "Manage host user accounts.",
                        "~/Icons/Sigma/Users_16X16_Standard.png", "~/Icons/Sigma/Users_32X32_Standard.png", false);

                    // Add Module To Page
                    AddModuleToPage(newPage, moduleDefId, "SuperUser Accounts", "~/Icons/Sigma/Users_32X32_Standard.png");
                }
            }

            var portalController = PortalController.Instance;
            foreach (PortalInfo portal in portalController.GetPortals())
            {
                if (!string.IsNullOrEmpty(portal.ProcessorPassword))
                {
                    portalController.UpdatePortalInfo(portal);
                }
            }
        }

        private static void UpgradeToVersion921()
        {
            UninstallPackage("jQuery", "Javascript_Library", true, "1.9.1");
            UninstallPackage("jQuery-UI", "Javascript_Library", true, "1.11.3");
            UninstallPackage("jQuery-Migrate", "Javascript_Library", true, "1.2.1");
        }

        private static void UpgradeToVersion930()
        {
            var applicationName = System.Web.Security.Membership.ApplicationName;
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                Logger.Warn("Unable to run orphaned user check. Application name is missing or not defined.");
                return;
            }

            using (var reader = DataProvider.Instance().ExecuteReader("DeleteOrphanedAspNetUsers", applicationName))
            {
                while (reader.Read())
                {
                    var errorMsg = reader["ErrorMessage"];
                    if (errorMsg != null)
                    {
                        Logger.Error("Failed to remove orphaned aspnet users. Error: " +
                            errorMsg.ToString());
                    }
                }
            }
        }

        private static void UpgradeToVersion941()
        {
            // It's possible previous versions of DNN created invalid binding redirects with <dependentAssembly xmlns="">, which are ignored
            // This finds these and removes them, adding a correct binding redirect if one doesn't exist
            var webConfig = Config.Load();

            var ns = new XmlNamespaceManager(webConfig.NameTable);
            ns.AddNamespace("ab", "urn:schemas-microsoft-com:asm.v1");

            var invalidDependentAssemblies = webConfig.SelectNodes("/configuration/runtime/ab:assemblyBinding/dependentAssembly", ns);
            foreach (XmlNode dependentAssembly in invalidDependentAssemblies)
            {
                var assemblyBindingElement = dependentAssembly.ParentNode;
                var assemblyIdentity = dependentAssembly.ChildNodes.Cast<XmlNode>().SingleOrDefault(n => n.LocalName.Equals("assemblyIdentity", StringComparison.Ordinal));
                if (assemblyIdentity == null)
                {
                    assemblyBindingElement.RemoveChild(dependentAssembly);
                    continue;
                }

                var name = assemblyIdentity.Attributes["name"]?.Value;
                var publicKeyToken = assemblyIdentity.Attributes["publicKeyToken"]?.Value;
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(publicKeyToken))
                {
                    assemblyBindingElement.RemoveChild(dependentAssembly);
                    continue;
                }

                var dependentAssemblyXPath = $"/configuration/runtime/ab:assemblyBinding/ab:dependentAssembly[ab:assemblyIdentity/@name='{name}'][ab:assemblyIdentity/@publicKeyToken='{publicKeyToken}']";
                var validDependentAssembly = webConfig.SelectSingleNode(dependentAssemblyXPath, ns);
                if (validDependentAssembly != null)
                {
                    // a valid dependentAssembly exists for this assembly, just remove the invalid element
                    assemblyBindingElement.RemoveChild(dependentAssembly);
                    continue;
                }

                // otherwise, replace the invalid dependentAssembly with a valid version of it
                AssemblyName assemblyName;
                try
                {
                    assemblyName = AssemblyName.GetAssemblyName(Path.Combine(Globals.ApplicationMapPath, "bin", name + ".dll"));
                }
                catch
                {
                    assemblyBindingElement.RemoveChild(dependentAssembly);
                    continue;
                }

                var validAssemblyIdentity = webConfig.CreateElement("assemblyIdentity", "urn:schemas-microsoft-com:asm.v1");
                validAssemblyIdentity.AddAttribute("name", name);
                validAssemblyIdentity.AddAttribute("publicKeyToken", publicKeyToken);

                var validBindingRedirect = webConfig.CreateElement("bindingRedirect", "urn:schemas-microsoft-com:asm.v1");
                validBindingRedirect.AddAttribute("oldVersion", "0.0.0.0-32767.32767.32767.32767");
                validBindingRedirect.AddAttribute("newVersion", assemblyName.Version.ToString());

                validDependentAssembly = webConfig.CreateElement("dependentAssembly", "urn:schemas-microsoft-com:asm.v1");
                validDependentAssembly.AppendChild(validAssemblyIdentity);
                validDependentAssembly.AppendChild(validBindingRedirect);

                assemblyBindingElement.ReplaceChild(validDependentAssembly, dependentAssembly);
            }

            if (invalidDependentAssemblies.Count > 0)
            {
                Config.Save(webConfig);
            }
        }

        private static void UpgradeToVersion960()
        {
            // Set default end user upload extension whitelist - ensure we don't add extensions that were not in the master list before
            var toAdd = new List<string> { ".export" };
            HostController.Instance.Update("FileExtensions", Host.AllowedExtensionWhitelist.ToStorageString(toAdd));
            var exts = new FileExtensionWhitelist("jpg,jpeg,jpe,gif,bmp,png,svg,doc,docx,xls,xlsx,ppt,pptx,pdf,txt,zip,rar,ico,avi,mpg,mpeg,mp3,wmv,mov,wav,mp4,webm,ogv,export");
            exts.RestrictBy(Host.AllowedExtensionWhitelist);
            HostController.Instance.Update("DefaultEndUserExtensionWhitelist", exts.ToStorageString());
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

            if (resourcesDict.ContainsKey(cultureCode))
            {
                return resourcesDict[cultureCode];
            }

            try
            {
                var languageFilePath = Path.Combine(
                    Globals.HostMapPath,
                    string.Format("Default Website.template.{0}.resx", cultureCode));
                if (!File.Exists(languageFilePath))
                {
                    languageFilePath = Path.Combine(
                        Globals.HostMapPath,
                        string.Format("Default Website.template.{0}.resx", Localization.SystemLocale));
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
