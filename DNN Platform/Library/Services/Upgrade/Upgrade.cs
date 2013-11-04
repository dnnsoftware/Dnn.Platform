
#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using System.Xml.XPath;

using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Content.Workflow;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Portals.Internal;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Social;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Modules.Dashboard.Components.Modules;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Analytics;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.EventQueue.Config;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Dependencies;
using DotNetNuke.Services.Installer.Log;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Localization.Internal;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Search;
using DotNetNuke.Services.Social.Messaging.Internal;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Services.Upgrade.InternalController.Steps;
using DotNetNuke.Services.Upgrade.Internals;
using DotNetNuke.Services.Upgrade.Internals.Steps;
using DotNetNuke.UI.Internals;

using ICSharpCode.SharpZipLib.Zip;

using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;
using ModuleInfo = DotNetNuke.Entities.Modules.ModuleInfo;
using Util = DotNetNuke.Entities.Content.Common.Util;

#endregion

namespace DotNetNuke.Services.Upgrade
{
    ///-----------------------------------------------------------------------------
    ///<summary>
    ///  The Upgrade class provides Shared/Static methods to Upgrade/Install
    ///  a DotNetNuke Application
    ///</summary>
    ///<remarks>
    ///</remarks>
    ///<history>
    ///  [cnurse]	11/6/2004	documented
    ///</history>
    ///-----------------------------------------------------------------------------
    public class Upgrade
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Upgrade));

        #region Private Shared Field

        private static DateTime _startTime;

        #endregion

        #region Public Properties

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

        #endregion

        #region Private Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddAdminPages adds an Admin Page and an associated Module to all configured Portals
        /// </summary>
        ///	<param name="tabName">The Name to give this new Tab</param>
        /// <param name="description">Description.</param>
        ///	<param name="tabIconFile">The Icon for this new Tab</param>
        /// <param name="tabIconFileLarge">The large Icon for this new Tab</param>
        ///	<param name="isVisible">A flag indicating whether the tab is visible</param>
        ///	<param name="moduleDefId">The Module Deinition Id for the module to be aded to this tab</param>
        ///	<param name="moduleTitle">The Module's title</param>
        ///	<param name="moduleIconFile">The Module's icon</param>
        /// <history>
        /// 	[cnurse]	11/16/2004	created 
        /// </history>
        /// -----------------------------------------------------------------------------
        private static void AddAdminPages(string tabName, string description, string tabIconFile, string tabIconFileLarge, bool isVisible, int moduleDefId, string moduleTitle, string moduleIconFile)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddAdminPages:" + tabName);
            //Call overload with InheritPermisions=True
            AddAdminPages(tabName, description, tabIconFile, tabIconFileLarge, isVisible, moduleDefId, moduleTitle, moduleIconFile, true);

        }

        private static void AddAdminRoleToPage(string tabPath)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddAdminRoleToPage:" + tabPath);
            var portalController = new PortalController();
            var tabController = new TabController();
            TabInfo tab;

            foreach (PortalInfo portal in portalController.GetPortals())
            {
                int tabID = TabController.GetTabByTabPath(portal.PortalID, tabPath, Null.NullString);
                if ((tabID != Null.NullInteger))
                {
                    tab = tabController.GetTab(tabID, portal.PortalID, true);

                    if ((tab.TabPermissions.Count == 0))
                    {
                        AddPagePermission(tab.TabPermissions, "View", Convert.ToInt32(portal.AdministratorRoleId));
                        AddPagePermission(tab.TabPermissions, "Edit", Convert.ToInt32(portal.AdministratorRoleId));
                        TabPermissionController.SaveTabPermissions(tab);
                    }
                }
            }
        }

        private static void AddConsoleModuleSettings(int moduleID)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddConsoleModuleSettings:" + moduleID);
            var moduleController = new ModuleController();

            moduleController.UpdateModuleSetting(moduleID, "DefaultSize", "IconFileLarge");
            moduleController.UpdateModuleSetting(moduleID, "AllowSizeChange", "False");
            moduleController.UpdateModuleSetting(moduleID, "DefaultView", "Hide");
            moduleController.UpdateModuleSetting(moduleID, "AllowViewChange", "False");
            moduleController.UpdateModuleSetting(moduleID, "ShowTooltip", "True");
        }

        private static void AddEventQueueApplicationStartFirstRequest()
        {
            //Add new EventQueue Event
            var config = EventQueueConfiguration.GetConfig();
            if (config != null)
            {
                if (!config.PublishedEvents.ContainsKey("Application_Start_FirstRequest"))
                {
                    foreach (SubscriberInfo subscriber in config.EventQueueSubscribers.Values)
                    {
                        EventQueueConfiguration.RegisterEventSubscription(config, "Application_Start_FirstRequest", subscriber);
                    }

                    EventQueueConfiguration.SaveConfig(config, string.Format("{0}EventQueue\\EventQueue.config", Globals.HostMapPath));
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddModuleControl adds a new Module Control to the system
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="moduleDefId">The Module Definition Id</param>
        ///	<param name="controlKey">The key for this control in the Definition</param>
        ///	<param name="controlTitle">The title of this control</param>
        ///	<param name="controlSrc">Te source of ths control</param>
        ///	<param name="iconFile">The icon file</param>
        ///	<param name="controlType">The type of control</param>
        ///	<param name="viewOrder">The vieworder for this module</param>
        ///	<param name="helpURL">The Help Url</param>
        /// <history>
        /// 	[cnurse]	11/08/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        private static void AddModuleControl(int moduleDefId, string controlKey, string controlTitle, string controlSrc, string iconFile, SecurityAccessLevel controlType, int viewOrder, string helpURL)
        {
            AddModuleControl(moduleDefId, controlKey, controlTitle, controlSrc, iconFile, controlType, viewOrder, helpURL, false);
        }

        private static void AddModuleControl(int moduleDefId, string controlKey, string controlTitle, string controlSrc, string iconFile, SecurityAccessLevel controlType, int viewOrder, string helpURL, bool supportsPartialRendering)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddModuleControl:" + moduleDefId);
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
                                           SupportsPartialRendering = supportsPartialRendering
                                       };

                ModuleControlController.AddModuleControl(moduleControl);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddModuleDefinition adds a new Core Module Definition to the system
        /// </summary>
        /// <remarks>
        ///	This overload allows the caller to determine whether the module has a controller
        /// class
        /// </remarks>
        ///	<param name="desktopModuleName">The Friendly Name of the Module to Add</param>
        ///	<param name="description">Description of the Module</param>
        ///	<param name="moduleDefinitionName">The Module Definition Name</param>
        ///	<param name="premium">A flag representing whether the module is a Premium module</param>
        ///	<param name="admin">A flag representing whether the module is an Admin module</param>
        ///	<returns>The Module Definition Id of the new Module</returns>
        /// <history>
        /// 	[cnurse]	10/14/2004	documented
        ///     [cnurse]    11/11/2004  removed addition of Module Control (now in AddMOduleControl)
        /// </history>
        /// -----------------------------------------------------------------------------
        private static int AddModuleDefinition(string desktopModuleName, string description, string moduleDefinitionName, bool premium, bool admin)
        {
            return AddModuleDefinition(desktopModuleName, description, moduleDefinitionName, "", false, premium, admin);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddModuleDefinition adds a new Core Module Definition to the system
        /// </summary>
        /// <remarks>
        ///	This overload allows the caller to determine whether the module has a controller
        /// class
        /// </remarks>
        ///	<param name="desktopModuleName">The Friendly Name of the Module to Add</param>
        ///	<param name="description">Description of the Module</param>
        ///	<param name="moduleDefinitionName">The Module Definition Name</param>
        /// <param name="businessControllerClass">Business Control Class.</param>
        /// <param name="isPortable">Whether the module is enable for portals.</param>
        ///	<param name="premium">A flag representing whether the module is a Premium module</param>
        ///	<param name="admin">A flag representing whether the module is an Admin module</param>
        ///	<returns>The Module Definition Id of the new Module</returns>
        /// <history>
        /// 	[cnurse]	10/14/2004	documented
        ///     [cnurse]    11/11/2004  removed addition of Module Control (now in AddMOduleControl)
        /// </history>
        /// -----------------------------------------------------------------------------
        private static int AddModuleDefinition(string desktopModuleName, string description, string moduleDefinitionName, string businessControllerClass, bool isPortable, bool premium, bool admin)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddModuleDefinition:" + desktopModuleName);
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
                                      Owner = "DotNetNuke",
                                      Organization = "DotNetNuke Corporation",
                                      Url = "www.dotnetnuke.com",
                                      Email = "support@dotnetnuke.com"
                                  };
                if (desktopModuleName == "Extensions" || desktopModuleName == "Skin Designer" || desktopModuleName == "Dashboard")
                {
                    package.IsSystemPackage = true;
                }
                package.Version = new Version(1, 0, 0);

                PackageController.Instance.SaveExtensionPackage(package);

                string moduleName = desktopModuleName.Replace(" ", "");
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
                                        SupportedFeatures = 0
                                    };
                if ((isPortable))
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
        /// AddModuleToPage adds a module to a Page
        /// </summary>
        /// <remarks>
        /// This overload assumes ModulePermissions will be inherited
        /// </remarks>
        ///	<param name="page">The Page to add the Module to</param>
        ///	<param name="moduleDefId">The Module Deinition Id for the module to be aded to this tab</param>
        ///	<param name="moduleTitle">The Module's title</param>
        ///	<param name="moduleIconFile">The Module's icon</param>
        /// <history>
        /// 	[cnurse]	11/11/2004	created 
        /// </history>
        /// -----------------------------------------------------------------------------
        private static int AddModuleToPage(TabInfo page, int moduleDefId, string moduleTitle, string moduleIconFile)
        {
            //Call overload with InheritPermisions=True
            return AddModuleToPage(page, moduleDefId, moduleTitle, moduleIconFile, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddPage adds a Tab Page
        /// </summary>
        /// <remarks>
        /// Adds a Tab to a parentTab
        /// </remarks>
        ///	<param name="parentTab">The Parent Tab</param>
        ///	<param name="tabName">The Name to give this new Tab</param>
        /// <param name="description">Description.</param>
        ///	<param name="tabIconFile">The Icon for this new Tab</param>
        /// <param name="tabIconFileLarge">The Large Icon for this new Tab</param>
        ///	<param name="isVisible">A flag indicating whether the tab is visible</param>
        ///	<param name="permissions">Page Permissions Collection for this page</param>
        /// <param name="isAdmin">Is an admin page</param>
        /// <history>
        /// 	[cnurse]	11/11/2004	created 
        /// </history>
        /// -----------------------------------------------------------------------------
        private static TabInfo AddPage(TabInfo parentTab, string tabName, string description, string tabIconFile, string tabIconFileLarge, bool isVisible, TabPermissionCollection permissions, bool isAdmin)
        {
            int parentId = Null.NullInteger;
            int portalId = Null.NullInteger;

            if ((parentTab != null))
            {
                parentId = parentTab.TabID;
                portalId = parentTab.PortalID;
            }


            return AddPage(portalId, parentId, tabName, description, tabIconFile, tabIconFileLarge, isVisible, permissions, isAdmin);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddPage adds a Tab Page
        /// </summary>
        ///	<param name="portalId">The Id of the Portal</param>
        ///	<param name="parentId">The Id of the Parent Tab</param>
        ///	<param name="tabName">The Name to give this new Tab</param>
        /// <param name="description">Description.</param>
        ///	<param name="tabIconFile">The Icon for this new Tab</param>
        /// <param name="tabIconFileLarge">The large Icon for this new Tab</param>
        ///	<param name="isVisible">A flag indicating whether the tab is visible</param>
        ///	<param name="permissions">Page Permissions Collection for this page</param>
        /// <param name="isAdmin">Is and admin page</param>
        /// <history>
        /// 	[cnurse]	11/11/2004	created 
        /// </history>
        /// -----------------------------------------------------------------------------
        private static TabInfo AddPage(int portalId, int parentId, string tabName, string description, string tabIconFile, string tabIconFileLarge, bool isVisible, TabPermissionCollection permissions, bool isAdmin)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddPage:" + tabName);
            var tabController = new TabController();

            TabInfo tab = tabController.GetTabByName(tabName, portalId, parentId);

            if (tab == null || tab.ParentId != parentId)
            {
                tab = new TabInfo
                          {
                              TabID = Null.NullInteger,
                              PortalID = portalId,
                              TabName = tabName,
                              Title = "",
                              Description = description,
                              KeyWords = "",
                              IsVisible = isVisible,
                              DisableLink = false,
                              ParentId = parentId,
                              IconFile = tabIconFile,
                              IconFileLarge = tabIconFileLarge,
                              IsDeleted = false
                          };
                tab.TabID = tabController.AddTab(tab, !isAdmin);

                if (((permissions != null)))
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
        /// AddPagePermission adds a TabPermission to a TabPermission Collection
        /// </summary>
        ///	<param name="permissions">Page Permissions Collection for this page</param>
        ///	<param name="key">The Permission key</param>
        ///	<param name="roleId">The role given the permission</param>
        /// <history>
        /// 	[cnurse]	11/11/2004	created 
        /// </history>
        /// -----------------------------------------------------------------------------
        private static void AddPagePermission(TabPermissionCollection permissions, string key, int roleId)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddPagePermission:" + key);
            var permissionController = new PermissionController();
            var permission = (PermissionInfo)permissionController.GetPermissionByCodeAndKey("SYSTEM_TAB", key)[0];

            var tabPermission = new TabPermissionInfo { PermissionID = permission.PermissionID, RoleID = roleId, AllowAccess = true };

            permissions.Add(tabPermission);
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddSearchResults adds a top level Hidden Search Results Page
        /// </summary>
        ///	<param name="moduleDefId">The Module Deinition Id for the Search Results Module</param>
        /// <history>
        /// 	[cnurse]	11/11/2004	created 
        /// </history>
        /// -----------------------------------------------------------------------------
        private static void AddSearchResults(int moduleDefId)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddSearchResults:" + moduleDefId);
            var portalController = new PortalController();
            PortalInfo portal;
            var portals = portalController.GetPortals();
            int intPortal;

            //Add Page to Admin Menu of all configured Portals
            for (intPortal = 0; intPortal <= portals.Count - 1; intPortal++)
            {
                var tabPermissions = new TabPermissionCollection();

                portal = (PortalInfo)portals[intPortal];

                AddPagePermission(tabPermissions, "View", Convert.ToInt32(Globals.glbRoleAllUsers));
                AddPagePermission(tabPermissions, "View", Convert.ToInt32(portal.AdministratorRoleId));
                AddPagePermission(tabPermissions, "Edit", Convert.ToInt32(portal.AdministratorRoleId));

                //Create New Page (or get existing one)
                var tab = AddPage(portal.PortalID, Null.NullInteger, "Search Results", "", "", "", false, tabPermissions, false);

                //Add Module To Page
                AddModuleToPage(tab, moduleDefId, "Search Results", "");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddSkinControl adds a new Module Control to the system
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="controlKey">The key for this control in the Definition</param>
        /// <param name="packageName">Package Name.</param>
        ///	<param name="controlSrc">Te source of ths control</param>
        /// <history>
        /// 	[cnurse]	05/26/2008	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private static void AddSkinControl(string controlKey, string packageName, string controlSrc)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddSkinControl:" + controlKey);
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

        private static void AddDefaultModuleIcons()
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddDefaultModuleIcons");
            var pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Google Analytics");
            if (pkg != null)
            {
                pkg.FolderName = "DesktopModules/Admin/Analytics";
                pkg.IconFile = "~/DesktopModules/Admin/Analytics/analytics.gif";
                PackageController.Instance.SaveExtensionPackage(pkg);
            }

            pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Configuration Manager");
            if (pkg != null)
            {
                pkg.FolderName = "DesktopModules/Admin/XmlMerge";
                pkg.IconFile = "~/DesktopModules/Admin/XmlMerge/xmlMerge.png";
                PackageController.Instance.SaveExtensionPackage(pkg);
            }

            pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Console");
            if (pkg != null)
            {
                pkg.FolderName = "DesktopModules/Admin/Console";
                pkg.IconFile = "~/DesktopModules/Admin/Console/console.gif";
                PackageController.Instance.SaveExtensionPackage(pkg);
            }

            pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.ContentList");
            if (pkg != null)
            {
                pkg.FolderName = "DesktopModules/Admin/ContentList";
                pkg.IconFile = "~/DesktopModules/Admin/ContentList/contentList.gif";
                PackageController.Instance.SaveExtensionPackage(pkg);
            }

            pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Dashboard");
            if (pkg != null)
            {
                pkg.FolderName = "DesktopModules/Admin/Dashboard";
                pkg.IconFile = "~/DesktopModules/Admin/Dashboard/dashboard.gif";
                PackageController.Instance.SaveExtensionPackage(pkg);
            }

            pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Languages");
            if (pkg != null)
            {
                pkg.FolderName = "DesktopModules/Admin/Languages";
                pkg.IconFile = "~/DesktopModules/Admin/Languages/languages.gif";
                PackageController.Instance.SaveExtensionPackage(pkg);
            }

            pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Marketplace");
            if (pkg != null)
            {
                pkg.FolderName = "DesktopModules/Admin/Marketplace";
                pkg.IconFile = "~/DesktopModules/Admin/Marketplace/marketplace.gif";
                PackageController.Instance.SaveExtensionPackage(pkg);
            }

            pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Sitemap");
            if (pkg != null)
            {
                pkg.FolderName = "DesktopModules/Admin/Sitemap";
                pkg.IconFile = "~/DesktopModules/Admin/Sitemap/sitemap.gif";
                PackageController.Instance.SaveExtensionPackage(pkg);
            }

            pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Skin Designer");
            if (pkg != null)
            {
                pkg.FolderName = "DesktopModules/Admin/SkinDesigner";
                pkg.IconFile = "~/DesktopModules/Admin/SkinDesigner/skinDesigner.gif";
                PackageController.Instance.SaveExtensionPackage(pkg);
            }

            pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Skins");
            if (pkg != null)
            {
                pkg.FolderName = "DesktopModules/Admin/Skins";
                pkg.IconFile = "~/DesktopModules/Admin/Skins/skins.gif";
                PackageController.Instance.SaveExtensionPackage(pkg);
            }

            pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.ViewProfile");
            if (pkg != null)
            {
                pkg.FolderName = "DesktopModules/Admin/ViewProfile";
                pkg.IconFile = "~/DesktopModules/Admin/ViewProfile/viewProfile.gif";
                PackageController.Instance.SaveExtensionPackage(pkg);
            }

            pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.ProfessionalPreview");
            if (pkg != null)
            {
                pkg.FolderName = "DesktopModules/Admin/ProfessionalPreview";
                pkg.IconFile = "~/DesktopModules/Admin/ProfessionalPreview/professionalPreview.gif";
                PackageController.Instance.SaveExtensionPackage(pkg);
            }
        }

        private static void AddModuleCategories()
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddModuleCategories");
            DesktopModuleController.AddModuleCategory("< None >");
            DesktopModuleController.AddModuleCategory("Admin");
            DesktopModuleController.AddModuleCategory("Common");


            foreach (var desktopModuleInfo in DesktopModuleController.GetDesktopModules(Null.NullInteger))
            {
                bool update = false;
                switch (desktopModuleInfo.Value.ModuleName)
                {
                    case "Portals":
                    case "SQL":
                    case "HostSettings":
                    case "Scheduler":
                    case "SearchAdmin":
                    case "Lists":
                    case "Extensions":
                    case "WhatsNew":
                    case "Dashboard":
                    case "Marketplace":
                    case "ConfigurationManager":
                    case "Security":
                    case "Tabs":
                    case "Vendors":
                    case "Banners":
                    case "FileManager":
                    case "SiteLog":
                    case "Newsletters":
                    case "RecycleBin":
                    case "LogViewer":
                    case "SiteWizard":
                    case "Languages":
                    case "Skins":
                    case "SkinDesigner":
                    case "GoogleAnalytics":
                    case "Sitemap":
                    case "DotNetNuke.Taxonomy":
                        desktopModuleInfo.Value.Category = "Admin";
                        update = true;
                        break;
                    default:
                        break;
                }
                if (update)
                {
                    if (desktopModuleInfo.Value.PackageID == Null.NullInteger)
                    {
                        LegacyUtil.ProcessLegacyModule(desktopModuleInfo.Value);
                    }
                    DesktopModuleController.SaveDesktopModule(desktopModuleInfo.Value, false, false);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CoreModuleExists determines whether a Core Module exists on the system
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="desktopModuleName">The Friendly Name of the Module</param>
        ///	<returns>True if the Module exists, otherwise False</returns>
        /// <history>
        /// 	[cnurse]	10/14/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        private static bool CoreModuleExists(string desktopModuleName)
        {
            var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(desktopModuleName, Null.NullInteger);

            return ((desktopModule != null));
        }

        private static void EnableModalPopUps()
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "EnableModalPopUps");
            foreach (var desktopModuleInfo in DesktopModuleController.GetDesktopModules(Null.NullInteger))
            {
                switch (desktopModuleInfo.Value.ModuleName)
                {
                    case "Portals":
                    case "SQL":
                    case "HostSettings":
                    case "Scheduler":
                    case "SearchAdmin":
                    case "Lists":
                    case "Extensions":
                    case "WhatsNew":
                    case "Dashboard":
                    case "Marketplace":
                    case "ConfigurationManager":
                    case "Security":
                    case "Tabs":
                    case "Vendors":
                    case "Banners":
                    case "FileManager":
                    case "SiteLog":
                    case "Newsletters":
                    case "RecycleBin":
                    case "LogViewer":
                    case "SiteWizard":
                    case "Languages":
                    case "Skins":
                    case "SkinDesigner":
                    case "GoogleAnalytics":
                    case "Sitemap":
                    case "DotNetNuke.Taxonomy":
                        foreach (ModuleDefinitionInfo definition in desktopModuleInfo.Value.ModuleDefinitions.Values)
                        {
                            foreach (ModuleControlInfo control in definition.ModuleControls.Values)
                            {
                                if (!String.IsNullOrEmpty(control.ControlKey))
                                {
                                    control.SupportsPopUps = true;
                                    ModuleControlController.SaveModuleControl(control, false);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            foreach (ModuleControlInfo control in ModuleControlController.GetModuleControlsByModuleDefinitionID(Null.NullInteger).Values)
            {
                control.SupportsPopUps = true;
                ModuleControlController.SaveModuleControl(control, false);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ExecuteScript executes a SQl script file
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="scriptFile">The script to Execute</param>
        /// <param name="writeFeedback">Need to output feedback message.</param>
        /// <history>
        /// 	[cnurse]	11/09/2004	created
        /// </history>
        /// -----------------------------------------------------------------------------
        internal static string ExecuteScript(string scriptFile, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "ExecuteScript:" + scriptFile);
            if (writeFeedback)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, Localization.Localization.GetString("ExecutingScript", Localization.Localization.GlobalResourceFile) + ":" + Path.GetFileName(scriptFile));
            }

            // read script file for installation
            string script = FileSystemUtils.ReadFile(scriptFile);

            // execute SQL installation script
            string exceptions = DataProvider.Instance().ExecuteScript(script);

            //add installer logging
            if (string.IsNullOrEmpty(exceptions))
            {
                DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogEnd", Localization.Localization.GlobalResourceFile) + "ExecuteScript:" + scriptFile);
            }
            else
            {
                DnnInstallLogger.InstallLogError(exceptions);
            }

            // log the results
            try
            {
                using (var streamWriter = File.CreateText(scriptFile.Replace("." + DefaultProvider, "") + ".log.resources"))
                {
                    streamWriter.WriteLine(exceptions);
                    streamWriter.Close();
                }
            }
            catch (Exception exc)
            {
                //does not have permission to create the log file
                Logger.Error(exc);
            }

            if (writeFeedback)
            {
                string resourcesFile = Path.GetFileName(scriptFile);
                if (!String.IsNullOrEmpty(resourcesFile))
                {
                    HtmlUtils.WriteScriptSuccessError(HttpContext.Current.Response, (string.IsNullOrEmpty(exceptions)), resourcesFile.Replace("." + DefaultProvider, ".log.resources"));
                }
            }

            return exceptions;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleDefinition gets the Module Definition Id of a module
        /// </summary>
        ///	<param name="desktopModuleName">The Friendly Name of the Module to Add</param>
        ///	<param name="moduleDefinitionName">The Module Definition Name</param>
        ///	<returns>The Module Definition Id of the Module (-1 if no module definition)</returns>
        /// <history>
        /// 	[cnurse]	11/16/2004	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private static int GetModuleDefinition(string desktopModuleName, string moduleDefinitionName)
        {
            // get desktop module
            var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(desktopModuleName, Null.NullInteger);
            if (desktopModule == null)
            {
                return -1;
            }

            // get module definition
            ModuleDefinitionInfo objModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(moduleDefinitionName, desktopModule.DesktopModuleID);
            if (objModuleDefinition == null)
            {
                return -1;
            }


            return objModuleDefinition.ModuleDefID;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// HostTabExists determines whether a tab of a given name exists under the Host tab
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="tabName">The Name of the Tab</param>
        ///	<returns>True if the Tab exists, otherwise False</returns>
        /// <history>
        /// 	[cnurse]	11/08/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        private static bool HostTabExists(string tabName)
        {
            bool tabExists = false;
            var tabController = new TabController();
            var hostTab = tabController.GetTabByName("Host", Null.NullInteger);

            var tab = tabController.GetTabByName(tabName, Null.NullInteger, hostTab.TabID);
            if ((tab != null))
            {
                tabExists = true;
            }


            return tabExists;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// InstallMemberRoleProvider - Installs the MemberRole Provider Db objects
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="providerPath">The Path to the Provider Directory</param>
        /// <param name="writeFeedback">Whether need to output feedback message.</param>
        /// <history>
        /// 	[cnurse]	02/02/2005	created
        /// </history>
        /// -----------------------------------------------------------------------------
        internal static string InstallMemberRoleProvider(string providerPath, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "InstallMemberRoleProvider");

            string exceptions = "";

            bool installMemberRole = true;
            if ((Config.GetSetting("InstallMemberRole") != null))
            {
                installMemberRole = bool.Parse(Config.GetSetting("InstallMemberRole"));
            }

            if (installMemberRole)
            {
                if (writeFeedback)
                {
                    HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Installing MemberRole Provider:<br>");
                }

                //Install Common
                exceptions += InstallMemberRoleProviderScript(providerPath, "InstallCommon", writeFeedback);
                //Install Membership
                exceptions += InstallMemberRoleProviderScript(providerPath, "InstallMembership", writeFeedback);
                //Install Profile
                //exceptions += InstallMemberRoleProviderScript(providerPath, "InstallProfile", writeFeedback);
                //Install Roles
                //exceptions += InstallMemberRoleProviderScript(providerPath, "InstallRoles", writeFeedback);
            }

            if (String.IsNullOrEmpty(exceptions))
            {
                DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogEnd", Localization.Localization.GlobalResourceFile) + "InstallMemberRoleProvider");
            }
            else
            {
                DnnInstallLogger.InstallLogError(exceptions);
            }

            return exceptions;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// InstallMemberRoleProviderScript - Installs a specific MemberRole Provider script
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="providerPath">The Path to the Provider Directory</param>
        ///	<param name="scriptFile">The Name of the Script File</param>
        ///	<param name="writeFeedback">Whether or not to echo results</param>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
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
                //does not have permission to create the log file
                Logger.Error(exc);
            }

            return exceptions;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ParseFiles parses the Host Template's Files node
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="node">The Files node</param>
        ///	<param name="portalId">The PortalId (-1 for Host Files)</param>
        /// <history>
        /// 	[cnurse]	11/08/2004	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private static void ParseFiles(XmlNode node, int portalId)
        {
            //Parse the File nodes
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RemoveCoreModule removes a Core Module from the system
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="desktopModuleName">The Friendly Name of the Module to Remove</param>
        ///	<param name="parentTabName">The Name of the parent Tab/Page for this module</param>
        ///	<param name="tabName">The Name to tab that contains the Module</param>
        ///	<param name="removeTab">A flag to determine whether to remove the Tab if it has no
        ///	other modules</param>
        /// <history>
        /// 	[cnurse]	10/14/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        private static void RemoveCoreModule(string desktopModuleName, string parentTabName, string tabName, bool removeTab)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "RemoveCoreModule:" + desktopModuleName);

            int moduleDefId = Null.NullInteger;
            int desktopModuleId = 0;

            //Find and remove the Module from the Tab
            switch (parentTabName)
            {
                case "Host":
                    var tabController = new TabController();
                    var tab = tabController.GetTabByName("Host", Null.NullInteger, Null.NullInteger);

                    if (tab != null)
                    {
                        moduleDefId = RemoveModule(desktopModuleName, tabName, tab.TabID, removeTab);
                    }
                    break;
                case "Admin":
                    var portalController = new PortalController();
                    PortalInfo portal;

                    var portals = portalController.GetPortals();

                    //Iterate through the Portals to remove the Module from the Tab
                    for (int intPortal = 0; intPortal <= portals.Count - 1; intPortal++)
                    {
                        portal = (PortalInfo)portals[intPortal];
                        moduleDefId = RemoveModule(desktopModuleName, tabName, portal.AdminTabId, removeTab);
                    }
                    break;
            }

            DesktopModuleInfo desktopModule = null;
            if (moduleDefId == Null.NullInteger)
            {
                desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(desktopModuleName, Null.NullInteger);
                desktopModuleId = desktopModule.DesktopModuleID;
            }
            else
            {
                //Get the Module Definition
                ModuleDefinitionInfo moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(moduleDefId);
                if (moduleDefinition != null)
                {
                    desktopModuleId = moduleDefinition.DesktopModuleID;
                    desktopModule = DesktopModuleController.GetDesktopModule(desktopModuleId, Null.NullInteger);
                }
            }

            if (desktopModule != null)
            {
                //Delete the Desktop Module
                var desktopModuleController = new DesktopModuleController();
                desktopModuleController.DeleteDesktopModule(desktopModuleId);

                //Delete the Package
                PackageController.Instance.DeleteExtensionPackage(PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == desktopModule.PackageID));
            }
        }

        private static int RemoveModule(string desktopModuleName, string tabName, int parentId, bool removeTab)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "RemoveModule:" + desktopModuleName);
            var tabController = new TabController();
            var moduleController = new ModuleController();
            TabInfo tab = tabController.GetTabByName(tabName, Null.NullInteger, parentId);
            int moduleDefId = 0;
            int count = 0;

            //Get the Modules on the Tab
            if (tab != null)
            {
                foreach (KeyValuePair<int, ModuleInfo> kvp in moduleController.GetTabModules(tab.TabID))
                {
                    var module = kvp.Value;
                    if (module.DesktopModule.FriendlyName == desktopModuleName)
                    {
                        //Delete the Module from the Modules list
                        moduleController.DeleteTabModule(module.TabID, module.ModuleID, false);
                        moduleDefId = module.ModuleDefID;
                    }
                    else
                    {
                        count += 1;
                    }
                }

                //If Tab has no modules optionally remove tab
                if (count == 0 && removeTab)
                {
                    tabController.DeleteTab(tab.TabID, tab.PortalID);
                }
            }

            return moduleDefId;
        }

        private static void RemoveModuleControl(int moduleDefId, string controlKey)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "RemoveModuleControl:" + moduleDefId);
            // get Module Control
            var moduleControl = ModuleControlController.GetModuleControlByControlKey(controlKey, moduleDefId);
            if (moduleControl != null)
            {
                ModuleControlController.DeleteModuleControl(moduleControl.ModuleControlID);
            }
        }

        private static void RemoveModuleFromPortals(string friendlyName)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "RemoveModuleFromPortals:" + friendlyName);
            DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModuleByFriendlyName(friendlyName);
            if (desktopModule != null)
            {
                //Module was incorrectly assigned as "IsPremium=False"
                if (desktopModule.PackageID > Null.NullInteger)
                {
                    desktopModule.IsPremium = true;
                    DesktopModuleController.SaveDesktopModule(desktopModule, false, true);
                }

                //Remove the module from Portals
                DesktopModuleController.RemoveDesktopModuleFromPortals(desktopModule.DesktopModuleID);
            }
        }

        private static bool TabPermissionExists(TabPermissionInfo tabPermission, int portalID)
        {
            return TabPermissionController.GetTabPermissions(tabPermission.TabID, portalID).Cast<TabPermissionInfo>().Any(permission => permission.TabID == tabPermission.TabID && permission.RoleID == tabPermission.RoleID && permission.PermissionID == tabPermission.PermissionID);
        }

        private static void FavIconsToPortalSettings()
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "FavIconsToPortalSettings");
            const string fileName = "favicon.ico";
            var portals = new PortalController().GetPortals().Cast<PortalInfo>();

            foreach (var portalInfo in portals)
            {
                string localPath = Path.Combine(portalInfo.HomeDirectoryMapPath, fileName);

                if (File.Exists(localPath))
                {
                    try
                    {
                        int fileId;
                        var folder = FolderManager.Instance.GetFolder(portalInfo.PortalID, "");
                        if (!FileManager.Instance.FileExists(folder, fileName))
                        {
                            using (var stream = File.OpenRead(localPath))
                            {
                                FileManager.Instance.AddFile(folder, fileName, stream, /*overwrite*/ false);
                            }
                        }
                        fileId = FileManager.Instance.GetFile(folder, fileName).FileId;

                        new FavIcon(portalInfo.PortalID).Update(fileId);
                    }
                    catch (Exception e)
                    {
                        string message = string.Format("Unable to setup Favicon for Portal: {0}", portalInfo.PortalName);
                        var controller = new EventLogController();
                        var info = new LogInfo();
                        info.LogTypeKey = EventLogController.EventLogType.ADMIN_ALERT.ToString();
                        info.AddProperty("Issue", message);
                        info.AddProperty("ExceptionMessage", e.Message);
                        info.AddProperty("StackTrace", e.StackTrace);
                        controller.AddLog(info);

                        Logger.Warn(message, e);
                    }
                }
            }
        }

        private static void AddIconToAllowedFiles()
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddIconToAllowedFiles");
            var toAdd = new List<string> { ".ico" };
            HostController.Instance.Update("FileExtensions", Host.AllowedExtensionWhitelist.ToStorageString(toAdd));
        }

        private static void UpgradeToVersion323()
        {
            //add new SecurityException
            var logController = new LogController();
            string configFile = Globals.HostMapPath + "Logs\\LogConfig\\SecurityExceptionTemplate.xml.resources";
            logController.AddLogType(configFile, Null.NullString);
        }

        private static void UpgradeToVersion440()
        {
            // remove module cache files with *.htm extension ( they are now securely named *.resources )
            var portalController = new PortalController();
            var portals = portalController.GetPortals();
            foreach (PortalInfo objPortal in portals)
            {
                if (Directory.Exists(Globals.ApplicationMapPath + "\\Portals\\" + objPortal.PortalID + "\\Cache\\"))
                {
                    string[] files = Directory.GetFiles(Globals.ApplicationMapPath + "\\Portals\\" + objPortal.PortalID + "\\Cache\\", "*.htm");
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }
                }
            }
        }

        private static void UpgradeToVersion470()
        {
            string hostTemplateFile = Globals.HostMapPath + "Templates\\Default.page.template";
            if (File.Exists(hostTemplateFile))
            {
                var portalController = new PortalController();
                ArrayList portals = portalController.GetPortals();
                foreach (PortalInfo portal in portals)
                {
                    string portalTemplateFolder = portal.HomeDirectoryMapPath + "Templates\\";

                    if (!Directory.Exists(portalTemplateFolder))
                    {
                        //Create Portal Templates folder
                        Directory.CreateDirectory(portalTemplateFolder);
                    }
                    string portalTemplateFile = portalTemplateFolder + "Default.page.template";
                    if (!File.Exists(portalTemplateFile))
                    {
                        File.Copy(hostTemplateFile, portalTemplateFile);

                        //Synchronize the Templates folder to ensure the templates are accessible
                        FolderManager.Instance.Synchronize(portal.PortalID, "Templates/", false, true);
                    }
                }
            }
        }

        private static void UpgradeToVersion482()
        {
            //checks for the very rare case where the default validationkey prior to 4.08.02
            //is still being used and updates it
            Config.UpdateValidationKey();
        }

        private static void UpgradeToVersion500()
        {
            var portalController = new PortalController();
            ArrayList portals = portalController.GetPortals();
            var tabController = new TabController();

            //Add Edit Permissions for Admin Tabs to legacy portals
            var permissionController = new PermissionController();
            ArrayList permissions = permissionController.GetPermissionByCodeAndKey("SYSTEM_TAB", "EDIT");
            int permissionId = -1;
            if (permissions.Count == 1)
            {
                var permission = permissions[0] as PermissionInfo;
                if (permission != null)
                {
                    permissionId = permission.PermissionID;
                }

                foreach (PortalInfo portal in portals)
                {
                    var adminTab = tabController.GetTab(portal.AdminTabId, portal.PortalID, true);
                    if (adminTab != null)
                    {
                        var tabPermission = new TabPermissionInfo { TabID = adminTab.TabID, PermissionID = permissionId, AllowAccess = true, RoleID = portal.AdministratorRoleId };
                        if (!TabPermissionExists(tabPermission, portal.PortalID))
                        {
                            adminTab.TabPermissions.Add(tabPermission);
                        }

                        //Save Tab Permissions to Data Base
                        TabPermissionController.SaveTabPermissions(adminTab);

                        foreach (var childTab in TabController.GetTabsByParent(portal.AdminTabId, portal.PortalID))
                        {
                            tabPermission = new TabPermissionInfo { TabID = childTab.TabID, PermissionID = permissionId, AllowAccess = true, RoleID = portal.AdministratorRoleId };
                            if (!TabPermissionExists(tabPermission, portal.PortalID))
                            {
                                childTab.TabPermissions.Add(tabPermission);
                            }
                            //Save Tab Permissions to Data Base
                            TabPermissionController.SaveTabPermissions(childTab);
                        }
                    }
                }
            }

            //Update Host/Admin modules Visibility setting
            bool superTabProcessed = Null.NullBoolean;
            var moduleController = new ModuleController();
            foreach (PortalInfo portal in portals)
            {
                if (!superTabProcessed)
                {
                    //Process Host Tabs
                    foreach (TabInfo childTab in TabController.GetTabsByParent(portal.SuperTabId, Null.NullInteger))
                    {
                        foreach (ModuleInfo tabModule in moduleController.GetTabModules(childTab.TabID).Values)
                        {
                            tabModule.Visibility = VisibilityState.None;
                            moduleController.UpdateModule(tabModule);
                        }
                    }
                }

                //Process Portal Tabs
                foreach (TabInfo childTab in TabController.GetTabsByParent(portal.AdminTabId, portal.PortalID))
                {
                    foreach (ModuleInfo tabModule in moduleController.GetTabModules(childTab.TabID).Values)
                    {
                        tabModule.Visibility = VisibilityState.None;
                        moduleController.UpdateModule(tabModule);
                    }
                }
            }

            //Upgrade PortalDesktopModules to support new "model"
            permissions = permissionController.GetPermissionByCodeAndKey("SYSTEM_DESKTOPMODULE", "DEPLOY");
            if (permissions.Count == 1)
            {
                var permission = permissions[0] as PermissionInfo;
                if (permission != null)
                {
                    permissionId = permission.PermissionID;
                }
                foreach (PortalInfo portal in portals)
                {
                    foreach (DesktopModuleInfo desktopModule in DesktopModuleController.GetDesktopModules(Null.NullInteger).Values)
                    {
                        if (!desktopModule.IsPremium)
                        {
                            //Parse the permissions
                            var deployPermissions = new DesktopModulePermissionCollection();
                            DesktopModulePermissionInfo deployPermission;

                            // if Not IsAdmin add Registered Users
                            if (!desktopModule.IsAdmin)
                            {
                                deployPermission = new DesktopModulePermissionInfo { PermissionID = permissionId, AllowAccess = true, RoleID = portal.RegisteredRoleId };
                                deployPermissions.Add(deployPermission);
                            }

                            // if Not a Host Module add Administrators
                            const string hostModules = "Portals, SQL, HostSettings, Scheduler, SearchAdmin, Lists, SkinDesigner, Extensions";
                            if (!hostModules.Contains(desktopModule.ModuleName))
                            {
                                deployPermission = new DesktopModulePermissionInfo { PermissionID = permissionId, AllowAccess = true, RoleID = portal.AdministratorRoleId };
                                deployPermissions.Add(deployPermission);
                            }

                            //Add Portal/Module to PortalDesktopModules
                            DesktopModuleController.AddDesktopModuleToPortal(portal.PortalID, desktopModule, deployPermissions, false);
                        }
                    }

                    DataCache.ClearPortalCache(portal.PortalID, true);
                }
            }

            LegacyUtil.ProcessLegacyModules();
            LegacyUtil.ProcessLegacyLanguages();
            LegacyUtil.ProcessLegacySkins();
            LegacyUtil.ProcessLegacySkinControls();
        }

        private static void UpgradeToVersion501()
        {
            //add new Cache Error Event Type
            var logController = new LogController();
            string configFile = string.Format("{0}Logs\\LogConfig\\CacheErrorTemplate.xml.resources", Globals.HostMapPath);
            logController.AddLogType(configFile, Null.NullString);
        }

        private static void UpgradeToVersion510()
        {
            var portalController = new PortalController();
            var tabController = new TabController();
            var moduleController = new ModuleController();
            int moduleDefId;

            //add Dashboard module and tab
            if (HostTabExists("Dashboard") == false)
            {
                moduleDefId = AddModuleDefinition("Dashboard", "Provides a snapshot of your DotNetNuke Application.", "Dashboard", true, true);
                AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/Dashboard/Dashboard.ascx", "icon_dashboard_32px.gif", SecurityAccessLevel.Host, 0);
                AddModuleControl(moduleDefId, "Export", "", "DesktopModules/Admin/Dashboard/Export.ascx", "", SecurityAccessLevel.Host, 0);
                AddModuleControl(moduleDefId, "DashboardControls", "", "DesktopModules/Admin/Dashboard/DashboardControls.ascx", "", SecurityAccessLevel.Host, 0);

                //Create New Host Page (or get existing one)
                TabInfo dashboardPage = AddHostPage("Dashboard", "Summary view of application and site settings.", "~/images/icon_dashboard_16px.gif", "~/images/icon_dashboard_32px.gif", true);

                //Add Module To Page
                AddModuleToPage(dashboardPage, moduleDefId, "Dashboard", "~/images/icon_dashboard_32px.gif");
            }
            else
            {
                //Module was incorrectly assigned as "IsPremium=False"
                RemoveModuleFromPortals("Dashboard");
                //fix path for dashboarcontrols
                moduleDefId = GetModuleDefinition("Dashboard", "Dashboard");
                RemoveModuleControl(moduleDefId, "DashboardControls");
                AddModuleControl(moduleDefId, "DashboardControls", "", "DesktopModules/Admin/Dashboard/DashboardControls.ascx", "", SecurityAccessLevel.Host, 0);
            }

            //Add the Extensions Module
            if (CoreModuleExists("Extensions") == false)
            {
                moduleDefId = AddModuleDefinition("Extensions", "", "Extensions");
                AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/Extensions/Extensions.ascx", "~/images/icon_extensions_32px.png", SecurityAccessLevel.View, 0);
                AddModuleControl(moduleDefId, "Edit", "Edit Feature", "DesktopModules/Admin/Extensions/EditExtension.ascx", "~/images/icon_extensions_32px.png", SecurityAccessLevel.Edit, 0);
                AddModuleControl(moduleDefId, "PackageWriter", "Package Writer", "DesktopModules/Admin/Extensions/PackageWriter.ascx", "~/images/icon_extensions_32px.png", SecurityAccessLevel.Host, 0);
                AddModuleControl(moduleDefId, "EditControl", "Edit Control", "DesktopModules/Admin/Extensions/Editors/EditModuleControl.ascx", "~/images/icon_extensions_32px.png", SecurityAccessLevel.Host, 0);
                AddModuleControl(moduleDefId, "ImportModuleDefinition", "Import Module Definition", "DesktopModules/Admin/Extensions/Editors/ImportModuleDefinition.ascx", "~/images/icon_extensions_32px.png", SecurityAccessLevel.Host, 0);
                AddModuleControl(moduleDefId, "BatchInstall", "Batch Install", "DesktopModules/Admin/Extensions/BatchInstall.ascx", "~/images/icon_extensions_32px.png", SecurityAccessLevel.Host, 0);
                AddModuleControl(moduleDefId, "NewExtension", "New Extension Wizard", "DesktopModules/Admin/Extensions/ExtensionWizard.ascx", "~/images/icon_extensions_32px.png", SecurityAccessLevel.Host, 0);
                AddModuleControl(moduleDefId, "UsageDetails", "Usage Information", "DesktopModules/Admin/Extensions/UsageDetails.ascx", "~/images/icon_extensions_32px.png", SecurityAccessLevel.Host, 0, "", true);
            }
            else
            {
                moduleDefId = GetModuleDefinition("Extensions", "Extensions");
                RemoveModuleControl(moduleDefId, "EditLanguage");
                RemoveModuleControl(moduleDefId, "TimeZone");
                RemoveModuleControl(moduleDefId, "Verify");
                RemoveModuleControl(moduleDefId, "LanguageSettings");
                RemoveModuleControl(moduleDefId, "EditResourceKey");
                RemoveModuleControl(moduleDefId, "EditSkins");
                AddModuleControl(moduleDefId, "UsageDetails", "Usage Information", "DesktopModules/Admin/Extensions/UsageDetails.ascx", "~/images/icon_extensions_32px.png", SecurityAccessLevel.Host, 0, "", true);

                //Module was incorrectly assigned as "IsPremium=False"
                RemoveModuleFromPortals("Extensions");
            }

            //Remove Module Definitions Module from Host Page (if present)
            RemoveCoreModule("Module Definitions", "Host", "Module Definitions", false);

            //Remove old Module Definition Validator module
            DesktopModuleController.DeleteDesktopModule("Module Definition Validator");

            //Get Module Definitions
            TabInfo definitionsPage = tabController.GetTabByName("Module Definitions", Null.NullInteger);

            //Add Module To Page if not present
            int moduleId = AddModuleToPage(definitionsPage, moduleDefId, "Module Definitions", "~/images/icon_moduledefinitions_32px.gif");
            moduleController.UpdateModuleSetting(moduleId, "Extensions_Mode", "Module");

            //Add Extensions Host Page
            TabInfo extensionsPage = AddHostPage("Extensions", "Install, add, modify and delete extensions, such as modules, skins and language packs.", "~/images/icon_extensions_16px.gif", "~/images/icon_extensions_32px.png", true);

            moduleId = AddModuleToPage(extensionsPage, moduleDefId, "Extensions", "~/images/icon_extensions_32px.png");
            moduleController.UpdateModuleSetting(moduleId, "Extensions_Mode", "All");

            //Add Extensions Module to Admin Page for all Portals
            AddAdminPages("Extensions", "Install, add, modify and delete extensions, such as modules, skins and language packs.", "~/images/icon_extensions_16px.gif", "~/images/icon_extensions_32px.png", true, moduleDefId, "Extensions", "~/images/icon_extensions_32px.png");

            //Remove Host Languages Page
            RemoveHostPage("Languages");

            //Remove Admin > Authentication Pages
            RemoveAdminPages("//Admin//Authentication");

            //Remove old Languages module
            DesktopModuleController.DeleteDesktopModule("Languages");

            //Add new Languages module
            moduleDefId = AddModuleDefinition("Languages", "", "Languages", false, false);
            AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/Languages/languageeditor.ascx", "~/images/icon_language_32px.gif", SecurityAccessLevel.View, 0);
            AddModuleControl(moduleDefId, "Edit", "Edit Language", "DesktopModules/Admin/Languages/EditLanguage.ascx", "~/images/icon_language_32px.gif", SecurityAccessLevel.Edit, 0);
            AddModuleControl(moduleDefId, "EditResourceKey", "Full Language Editor", "DesktopModules/Admin/Languages/languageeditorext.ascx", "~/images/icon_language_32px.gif", SecurityAccessLevel.Edit, 0);
            AddModuleControl(moduleDefId, "LanguageSettings", "Language Settings", "DesktopModules/Admin/Languages/LanguageSettings.ascx", "", SecurityAccessLevel.Edit, 0);
            AddModuleControl(moduleDefId, "TimeZone", "TimeZone Editor", "DesktopModules/Admin/Languages/timezoneeditor.ascx", "~/images/icon_language_32px.gif", SecurityAccessLevel.Host, 0);
            AddModuleControl(moduleDefId, "Verify", "Resource File Verifier", "DesktopModules/Admin/Languages/resourceverifier.ascx", "", SecurityAccessLevel.Host, 0);
            AddModuleControl(moduleDefId, "PackageWriter", "Language Pack Writer", "DesktopModules/Admin/Languages/LanguagePackWriter.ascx", "", SecurityAccessLevel.Host, 0);

            //Add Module to Admin Page for all Portals
            AddAdminPages("Languages", "Manage Language Resources.", "~/images/icon_language_16px.gif", "~/images/icon_language_32px.gif", true, moduleDefId, "Language Editor", "~/images/icon_language_32px.gif");

            //Remove Host Skins Page
            RemoveHostPage("Skins");

            //Remove old Skins module
            DesktopModuleController.DeleteDesktopModule("Skins");

            //Add new Skins module
            moduleDefId = AddModuleDefinition("Skins", "", "Skins", false, false);
            AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/Skins/editskins.ascx", "~/images/icon_skins_32px.gif", SecurityAccessLevel.View, 0);

            //Add Module to Admin Page for all Portals
            AddAdminPages("Skins", "Manage Skin Resources.", "~/images/icon_skins_16px.gif", "~/images/icon_skins_32px.gif", true, moduleDefId, "Skin Editor", "~/images/icon_skins_32px.gif");

            //Remove old Skin Designer module
            DesktopModuleController.DeleteDesktopModule("Skin Designer");
            DesktopModuleController.DeleteDesktopModule("SkinDesigner");

            //Add new Skin Designer module
            moduleDefId = AddModuleDefinition("Skin Designer", "Allows you to modify skin attributes.", "Skin Designer", true, true);
            AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/SkinDesigner/Attributes.ascx", "~/images/icon_skins_32px.gif", SecurityAccessLevel.Host, 0);

            //Add new Skin Designer to every Admin Skins Tab
            AddModuleToPages("//Admin//Skins", moduleDefId, "Skin Designer", "~/images/icon_skins_32px.gif", true);

            //Remove Admin Whats New Page
            RemoveAdminPages("//Admin//WhatsNew");

            //WhatsNew needs to be set to IsPremium and removed from all portals
            RemoveModuleFromPortals("WhatsNew");

            //Create New WhatsNew Host Page (or get existing one)
            TabInfo newPage = AddHostPage("What's New", "Provides a summary of the major features for each release.", "~/images/icon_whatsnew_16px.gif", "~/images/icon_whatsnew_32px.gif", true);

            //Add WhatsNew Module To Page
            moduleDefId = GetModuleDefinition("WhatsNew", "WhatsNew");
            AddModuleToPage(newPage, moduleDefId, "What's New", "~/images/icon_whatsnew_32px.gif");

            //add console module
            moduleDefId = AddModuleDefinition("Console", "Display children pages as icon links for navigation.", "Console", "DotNetNuke.Modules.Console.Components.ConsoleController", true, false, false);
            AddModuleControl(moduleDefId, "", "Console", "DesktopModules/Admin/Console/ViewConsole.ascx", "", SecurityAccessLevel.Anonymous, 0);
            AddModuleControl(moduleDefId, "Settings", "Console Settings", "DesktopModules/Admin/Console/Settings.ascx", "", SecurityAccessLevel.Admin, 0);

            //add console module to host page
            moduleId = AddModuleToPage("//Host", Null.NullInteger, moduleDefId, "Basic Features", "", true);
            int tabId = TabController.GetTabByTabPath(Null.NullInteger, "//Host", Null.NullString);
            TabInfo tab;

            //add console settings for host page
            if ((tabId != Null.NullInteger))
            {
                tab = tabController.GetTab(tabId, Null.NullInteger, true);
                if (((tab != null)))
                {
                    AddConsoleModuleSettings(moduleId);
                }
            }

            //add module to all admin pages
            foreach (PortalInfo portal in portalController.GetPortals())
            {
                tabId = TabController.GetTabByTabPath(portal.PortalID, "//Admin", Null.NullString);
                if ((tabId != Null.NullInteger))
                {
                    tab = tabController.GetTab(tabId, portal.PortalID, true);
                    if (((tab != null)))
                    {
                        moduleId = AddModuleToPage(tab, moduleDefId, "Basic Features", "", true);
                        AddConsoleModuleSettings(moduleId);
                    }
                }
            }

            //Add Google Analytics module
            moduleDefId = AddModuleDefinition("Google Analytics", "Configure Site Google Analytics settings.", "GoogleAnalytics", false, false);
            AddModuleControl(moduleDefId, "", "Google Analytics", "DesktopModules/Admin/Analytics/GoogleAnalyticsSettings.ascx", "", SecurityAccessLevel.Admin, 0);
            AddAdminPages("Google Analytics", "Configure Site Google Analytics settings.", "~/images/icon_analytics_16px.gif", "~/images/icon_analytics_32px.gif", true, moduleDefId, "Google Analytics", "~/images/icon_analytics_32px.gif");
        }

        private static void UpgradeToVersion511()
        {
            //New Admin pages may not have administrator permission
            //Add Admin role if it does not exist for google analytics or extensions
            AddAdminRoleToPage("//Admin//Extensions");
            AddAdminRoleToPage("//Admin//GoogleAnalytics");
        }

        private static void UpgradeToVersion513()
        {
            //Ensure that default language is present (not neccessarily enabled)
            var defaultLanguage = LocaleController.Instance.GetLocale("en-US") ?? new Locale();
            defaultLanguage.Code = "en-US";
            defaultLanguage.Text = "English (United States)";
            Localization.Localization.SaveLanguage(defaultLanguage);

            //Ensure that there is a Default Authorization System
            var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DefaultAuthentication");
            if (package == null)
            {
                package = new PackageInfo
                              {
                                  Name = "DefaultAuthentication",
                                  FriendlyName = "Default Authentication",
                                  Description = "The Default UserName/Password Authentication System for DotNetNuke.",
                                  PackageType = "Auth_System",
                                  Version = new Version(1, 0, 0),
                                  Owner = "DotNetNuke",
                                  License = Localization.Localization.GetString("License", Localization.Localization.GlobalResourceFile),
                                  Organization = "DotNetNuke Corporation",
                                  Url = "www.dotnetnuke.com",
                                  Email = "support@dotnetnuke.com",
                                  ReleaseNotes = "There are no release notes for this version.",
                                  IsSystemPackage = true
                              };
                PackageController.Instance.SaveExtensionPackage(package);

                //Add Authentication System
                var authSystem = AuthenticationController.GetAuthenticationServiceByType("DNN") ?? new AuthenticationInfo();
                authSystem.PackageID = package.PackageID;
                authSystem.AuthenticationType = "DNN";
                authSystem.SettingsControlSrc = "DesktopModules/AuthenticationServices/DNN/Settings.ascx";
                authSystem.LoginControlSrc = "DesktopModules/AuthenticationServices/DNN/Login.ascx";
                authSystem.IsEnabled = true;

                if (authSystem.AuthenticationID == Null.NullInteger)
                {
                    AuthenticationController.AddAuthentication(authSystem);
                }
                else
                {
                    AuthenticationController.UpdateAuthentication(authSystem);
                }
            }
        }

        private static void UpgradeToVersion520()
        {
            //Add new ViewSource control
            AddModuleControl(Null.NullInteger, "ViewSource", "View Module Source", "Admin/Modules/ViewSource.ascx", "~/images/icon_source_32px.gif", SecurityAccessLevel.Host, 0, "", true);

            //Add Marketplace module definition
            int moduleDefId = AddModuleDefinition("Marketplace", "Search for DotNetNuke modules, extension and skins.", "Marketplace");
            AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/Marketplace/Marketplace.ascx", "~/images/icon_marketplace_32px.gif", SecurityAccessLevel.Host, 0);

            //Add marketplace Module To Page
            TabInfo newPage = AddHostPage("Marketplace", "Search for DotNetNuke modules, extension and skins.", "~/images/icon_marketplace_16px.gif", "~/images/icon_marketplace_32px.gif", true);
            moduleDefId = GetModuleDefinition("Marketplace", "Marketplace");
            AddModuleToPage(newPage, moduleDefId, "Marketplace", "~/images/icon_marketplace_32px.gif");
        }

        private static void UpgradeToVersion521()
        {
            // UpgradeDefaultLanguages is a temporary procedure containing code that
            // needed to execute after the 5.1.3 application upgrade code above
            DataProvider.Instance().ExecuteNonQuery("UpgradeDefaultLanguages");

            // This procedure is not intended to be part of the database schema
            // and is therefore dropped once it has been executed.
            DataProvider.Instance().ExecuteSQL("DROP PROCEDURE {databaseOwner}{objectQualifier}UpgradeDefaultLanguages");
        }

        private static void UpgradeToVersion530()
        {
            //update languages module
            int moduleDefId = GetModuleDefinition("Languages", "Languages");
            RemoveModuleControl(moduleDefId, "");
            AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/Languages/languageEnabler.ascx", "~/images/icon_language_32px.gif", SecurityAccessLevel.View, 0, "", true);
            AddModuleControl(moduleDefId, "Editor", "", "DesktopModules/Admin/Languages/languageeditor.ascx", "~/images/icon_language_32px.gif", SecurityAccessLevel.View, 0);

            //Add new View Profile module
            moduleDefId = AddModuleDefinition("ViewProfile", "", "ViewProfile", false, false);
            AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/ViewProfile/ViewProfile.ascx", "~/images/icon_profile_32px.gif", SecurityAccessLevel.View, 0);
            AddModuleControl(moduleDefId, "Settings", "Settings", "DesktopModules/Admin/ViewProfile/Settings.ascx", "~/images/icon_profile_32px.gif", SecurityAccessLevel.Edit, 0);

            //Add new Sitemap settings module
            moduleDefId = AddModuleDefinition("Sitemap", "", "Sitemap", false, false);
            AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/Sitemap/SitemapSettings.ascx", "~/images/icon_analytics_32px.gif", SecurityAccessLevel.View, 0);
            AddAdminPages("Search Engine Sitemap", "Configure the sitemap for submission to common search engines.", "~/images/icon_analytics_16px.gif", "~/images/icon_analytics_32px.gif", true, moduleDefId, "Search Engine Sitemap", "~/images/icon_analytics_32px.gif");


            //Add new Photo Profile field to Host
            var listController = new ListController();
            Dictionary<string, ListEntryInfo> dataTypes = listController.GetListEntryInfoDictionary("DataType");

            var properties = ProfileController.GetPropertyDefinitionsByPortal(Null.NullInteger);
            ProfileController.AddDefaultDefinition(Null.NullInteger, "Preferences", "Photo", "Image", 0, properties.Count * 2 + 2, UserVisibilityMode.AllUsers, dataTypes);

            string installTemplateFile = string.Format("{0}Template\\UserProfile.page.template", Globals.InstallMapPath);
            string hostTemplateFile = string.Format("{0}Templates\\UserProfile.page.template", Globals.HostMapPath);
            if (File.Exists(installTemplateFile))
            {
                if (!File.Exists(hostTemplateFile))
                {
                    File.Copy(installTemplateFile, hostTemplateFile);
                }
            }
            if (File.Exists(hostTemplateFile))
            {
                var tabController = new TabController();
                var portalController = new PortalController();
                ArrayList portals = portalController.GetPortals();
                foreach (PortalInfo portal in portals)
                {
                    properties = ProfileController.GetPropertyDefinitionsByPortal(portal.PortalID);

                    //Add new Photo Profile field to Portal
                    ProfileController.AddDefaultDefinition(portal.PortalID, "Preferences", "Photo", "Image", 0, properties.Count * 2 + 2, UserVisibilityMode.AllUsers, dataTypes);

                    //Rename old Default Page template
                    string defaultPageTemplatePath = string.Format("{0}Templates\\Default.page.template", portal.HomeDirectoryMapPath);
                    if (File.Exists(defaultPageTemplatePath))
                    {
                        File.Move(defaultPageTemplatePath, String.Format("{0}Templates\\Default_old.page.template", portal.HomeDirectoryMapPath));
                    }

                    //Update Default profile template in every portal
                    portalController.CopyPageTemplate("Default.page.template", portal.HomeDirectoryMapPath);

                    //Add User profile template to every portal
                    portalController.CopyPageTemplate("UserProfile.page.template", portal.HomeDirectoryMapPath);

                    //Synchronize the Templates folder to ensure the templates are accessible
                    FolderManager.Instance.Synchronize(portal.PortalID, "Templates/", false, true);

                    var xmlDoc = new XmlDocument();
                    try
                    {
                        // open the XML file
                        xmlDoc.Load(hostTemplateFile);
                    }
                    catch (Exception ex)
                    {
                        Exceptions.Exceptions.LogException(ex);
                    }

                    XmlNode userTabNode = xmlDoc.SelectSingleNode("//portal/tabs/tab");
                    if (userTabNode != null)
                    {
                        string tabName = XmlUtils.GetNodeValue(userTabNode.CreateNavigator(), "name");

                        var userTab = tabController.GetTabByName(tabName, portal.PortalID) ?? TabController.DeserializeTab(userTabNode, null, portal.PortalID, PortalTemplateModuleAction.Merge);

                        //Update SiteSettings to point to the new page
                        if (portal.UserTabId > Null.NullInteger)
                        {
                            portal.RegisterTabId = portal.UserTabId;
                        }
                        else
                        {
                            portal.UserTabId = userTab.TabID;
                        }
                    }
                    portalController.UpdatePortalInfo(portal);

                    //Add Users folder to every portal
                    string usersFolder = string.Format("{0}Users\\", portal.HomeDirectoryMapPath);

                    if (!Directory.Exists(usersFolder))
                    {
                        //Create Users folder
                        Directory.CreateDirectory(usersFolder);

                        //Synchronize the Users folder to ensure the user folder is accessible
                        FolderManager.Instance.Synchronize(portal.PortalID, "Users/", false, true);
                    }
                }
            }
            AddEventQueueApplicationStartFirstRequest();

            //Change Key for Module Defintions;
            moduleDefId = GetModuleDefinition("Extensions", "Extensions");
            RemoveModuleControl(moduleDefId, "ImportModuleDefinition");
            AddModuleControl(moduleDefId, "EditModuleDefinition", "Edit Module Definition", "DesktopModules/Admin/Extensions/Editors/EditModuleDefinition.ascx", "~/images/icon_extensions_32px.png", SecurityAccessLevel.Host, 0);

            //Module was incorrectly assigned as "IsPremium=False"
            RemoveModuleFromPortals("Users And Roles");
        }

        private static void UpgradeToVersion540()
        {
            var configDoc = Config.Load();
            var configNavigator = configDoc.CreateNavigator().SelectSingleNode("/configuration/system.web.extensions");
            if (configNavigator == null)
            {
                //attempt to remove "System.Web.Extensions" configuration section
                string upgradeFile = string.Format("{0}\\Config\\SystemWebExtensions.config", Globals.InstallMapPath);
                string message = UpdateConfig(upgradeFile, DotNetNukeContext.Current.Application.Version, "Remove System.Web.Extensions");
                var eventLogController = new EventLogController();
                eventLogController.AddLog("UpgradeConfig",
                                          string.IsNullOrEmpty(message)
                                              ? "Remove System Web Extensions"
                                              : string.Format("Remove System Web Extensions failed. Error reported during attempt to update:{0}", message),
                                          PortalController.GetCurrentPortalSettings(),
                                          UserController.GetCurrentUserInfo().UserID,
                                          EventLogController.EventLogType.HOST_ALERT);
            }

            //Add Styles Skin Object
            AddSkinControl("TAGS", "DotNetNuke.TagsSkinObject", "Admin/Skins/Tags.ascx");

            //Add Content List module definition
            int moduleDefId = AddModuleDefinition("ContentList", "This module displays a list of content by tag.", "Content List");
            AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/ContentList/ContentList.ascx", "", SecurityAccessLevel.View, 0);

            //Update registration page
            var portalController = new PortalController();
            ArrayList portals = portalController.GetPortals();
            foreach (PortalInfo portal in portals)
            {
                //objPortal.RegisterTabId = objPortal.UserTabId;
                portalController.UpdatePortalInfo(portal);

                //Add ContentList to Search Results Page
                var tabController = new TabController();
                int tabId = TabController.GetTabByTabPath(portal.PortalID, "//SearchResults", Null.NullString);
                TabInfo searchPage = tabController.GetTab(tabId, portal.PortalID, false);
                AddModuleToPage(searchPage, moduleDefId, "Results", "");
            }
        }

        private static void UpgradeToVersion543()
        {
            // get log file path
            string logFilePath = DataProvider.Instance().GetProviderPath();
            if (Directory.Exists(logFilePath))
            {
                //get log files
                foreach (string fileName in Directory.GetFiles(logFilePath, "*.log"))
                {
                    if (File.Exists(fileName + ".resources"))
                    {
                        File.Delete(fileName + ".resources");
                    }
                    //copy requires use of move
                    File.Move(fileName, fileName + ".resources");
                }
            }
        }

        private static void UpgradeToVersion550()
        {
            //update languages module
            int moduleDefId = GetModuleDefinition("Languages", "Languages");
            AddModuleControl(moduleDefId, "TranslationStatus", "", "DesktopModules/Admin/Languages/TranslationStatus.ascx", "~/images/icon_language_32px.gif", SecurityAccessLevel.Edit, 0);

            //due to an error in 5.3.0 we need to recheck and readd Application_Start_FirstRequest
            AddEventQueueApplicationStartFirstRequest();

            // check if UserProfile page template exists in Host folder and if not, copy it from Install folder
            string installTemplateFile = string.Format("{0}Templates\\UserProfile.page.template", Globals.InstallMapPath);
            if (File.Exists(installTemplateFile))
            {
                string hostTemplateFile = string.Format("{0}Templates\\UserProfile.page.template", Globals.HostMapPath);
                if (!File.Exists(hostTemplateFile))
                {
                    File.Copy(installTemplateFile, hostTemplateFile);
                }
            }

            //Fix the permission for User Folders
            var portalController = new PortalController();
            foreach (PortalInfo portal in portalController.GetPortals())
            {
                foreach (FolderInfo folder in FolderManager.Instance.GetFolders(portal.PortalID))
                {
                    if (folder.FolderPath.StartsWith("Users/"))
                    {
                        foreach (PermissionInfo permission in PermissionController.GetPermissionsByFolder())
                        {
                            if (permission.PermissionKey.ToUpper() == "READ")
                            {
                                //Add All Users Read Access to the folder
                                int roleId = Int32.Parse(Globals.glbRoleAllUsers);
                                if (!folder.FolderPermissions.Contains(permission.PermissionKey, folder.FolderID, roleId, Null.NullInteger))
                                {
                                    var folderPermission = new FolderPermissionInfo(permission) { FolderID = folder.FolderID, UserID = Null.NullInteger, RoleID = roleId, AllowAccess = true };

                                    folder.FolderPermissions.Add(folderPermission);
                                }
                            }
                        }

                        FolderPermissionController.SaveFolderPermissions(folder);
                    }
                }
                //Remove user page template from portal if it exists (from 5.3)
                if (File.Exists(string.Format("{0}Templates\\UserProfile.page.template", portal.HomeDirectoryMapPath)))
                {
                    File.Delete(string.Format("{0}Templates\\UserProfile.page.template", portal.HomeDirectoryMapPath));
                }
            }

            //DNN-12894 -   Country Code for "United Kingdom" is incorrect
            var listController = new ListController();
            var listItem = listController.GetListEntryInfo("Country", "UK");
            if (listItem != null)
            {
                listItem.Value = "GB";
                listController.UpdateListEntry(listItem);
            }


            foreach (PortalInfo portal in new PortalController().GetPortals())
            {
                //fix issue where portal default language may be disabled
                string defaultLanguage = portal.DefaultLanguage;
                if (!IsLanguageEnabled(portal.PortalID, defaultLanguage))
                {
                    Locale language = LocaleController.Instance.GetLocale(defaultLanguage);
                    Localization.Localization.AddLanguageToPortal(portal.PortalID, language.LanguageId, true);
                }
                //preemptively create any missing localization records rather than relying on dynamic creation
                foreach (Locale locale in LocaleController.Instance.GetLocales(portal.PortalID).Values)
                {
                    DataProvider.Instance().EnsureLocalizationExists(portal.PortalID, locale.Code);
                }
            }
        }

        private static void UpgradeToVersion560()
        {
            //Add .htmtemplate file extension
            var toAdd = new List<string> { ".htmtemplate" };
            HostController.Instance.Update("FileExtensions", Host.AllowedExtensionWhitelist.ToStorageString(toAdd));

            //Add new Xml Merge module
            int moduleDefId = AddModuleDefinition("Configuration Manager", "", "Configuration Manager", false, false);
            AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/XmlMerge/XmlMerge.ascx", "~/images/icon_configuration_32px.png", SecurityAccessLevel.Host, 0);

            //Add Module To Page
            TabInfo hostPage = AddHostPage("Configuration Manager", "Modify configuration settings for your site", "~/images/icon_configuration_16px.png", "~/images/icon_configuration_32px.png", true);
            AddModuleToPage(hostPage, moduleDefId, "Configuration Manager", "~/images/icon_configuration_32px.png");

            //Update Google Analytics Script in SiteAnalysis.config
            var googleAnalyticsController = new GoogleAnalyticsController();
            googleAnalyticsController.UpgradeModule("05.06.00");

            //Updated LanguageSettings.ascx control to be a Settings control
            ModuleDefinitionInfo languageModule = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("Languages");
            ModuleControlInfo moduleControl = ModuleControlController.GetModuleControlsByModuleDefinitionID(languageModule.ModuleDefID)["LanguageSettings"];
            moduleControl.ControlKey = "Settings";
            ModuleControlController.UpdateModuleControl(moduleControl);
        }

        private static void UpgradeToVersion562()
        {
            //Add new Photo Profile field to Host
            var listController = new ListController();
            Dictionary<string, ListEntryInfo> dataTypes = listController.GetListEntryInfoDictionary("DataType");

            var properties = ProfileController.GetPropertyDefinitionsByPortal(Null.NullInteger);
            ProfileController.AddDefaultDefinition(Null.NullInteger, "Preferences", "Photo", "Image", 0, properties.Count * 2 + 2, UserVisibilityMode.AllUsers, dataTypes);

            HostController.Instance.Update("AutoAddPortalAlias", Globals.Status == Globals.UpgradeStatus.Install ? "Y" : "N");

            // remove the system message module from the admin tab
            // System Messages are now managed through Localization
            if (CoreModuleExists("System Messages"))
            {
                RemoveCoreModule("System Messages", "Admin", "Site Settings", false);
            }

            // remove portal alias module
            if (CoreModuleExists("PortalAliases"))
            {
                RemoveCoreModule("PortalAliases", "Admin", "Site Settings", false);
            }

            // add the log viewer module to the admin tab
            int moduleDefId;
            if (CoreModuleExists("LogViewer") == false)
            {
                moduleDefId = AddModuleDefinition("LogViewer", "Allows you to view log entries for site events.", "Log Viewer");
                AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/LogViewer/LogViewer.ascx", "", SecurityAccessLevel.Admin, 0);
                AddModuleControl(moduleDefId, "Edit", "Edit Log Settings", "DesktopModules/Admin/LogViewer/EditLogTypes.ascx", "", SecurityAccessLevel.Host, 0);

                //Add the Module/Page to all configured portals
                AddAdminPages("Log Viewer", "View a historical log of database events such as event schedules, exceptions, account logins, module and page changes, user account activities, security role activities, etc.", "icon_viewstats_16px.gif", "icon_viewstats_32px.gif", true, moduleDefId, "Log Viewer", "icon_viewstats_16px.gif");
            }

            // add the schedule module to the host tab
            TabInfo newPage;
            if (CoreModuleExists("Scheduler") == false)
            {
                moduleDefId = AddModuleDefinition("Scheduler", "Allows you to schedule tasks to be run at specified intervals.", "Scheduler");
                AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/Scheduler/ViewSchedule.ascx", "", SecurityAccessLevel.Admin, 0);
                AddModuleControl(moduleDefId, "Edit", "Edit Schedule", "DesktopModules/Admin/Scheduler/EditSchedule.ascx", "", SecurityAccessLevel.Host, 0);
                AddModuleControl(moduleDefId, "History", "Schedule History", "DesktopModules/Admin/Scheduler/ViewScheduleHistory.ascx", "", SecurityAccessLevel.Host, 0);
                AddModuleControl(moduleDefId, "Status", "Schedule Status", "DesktopModules/Admin/Scheduler/ViewScheduleStatus.ascx", "", SecurityAccessLevel.Host, 0);

                //Create New Host Page (or get existing one)
                newPage = AddHostPage("Schedule", "Add, modify and delete scheduled tasks to be run at specified intervals.", "icon_scheduler_16px.gif", "icon_scheduler_32px.gif", true);

                //Add Module To Page
                AddModuleToPage(newPage, moduleDefId, "Schedule", "icon_scheduler_16px.gif");
            }

            // add the Search Admin module to the host tab
            if (CoreModuleExists("SearchAdmin") == false)
            {
                moduleDefId = AddModuleDefinition("SearchAdmin", "The Search Admininstrator provides the ability to manage search settings.", "Search Admin");
                AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/SearchAdmin/SearchAdmin.ascx", "", SecurityAccessLevel.Host, 0);

                //Create New Host Page (or get existing one)
                newPage = AddHostPage("Search Admin", "Manage search settings associated with DotNetNuke's search capability.", "icon_search_16px.gif", "icon_search_32px.gif", true);

                //Add Module To Page
                AddModuleToPage(newPage, moduleDefId, "Search Admin", "icon_search_16px.gif");
            }

            // add the Search Input module
            if (CoreModuleExists("SearchInput") == false)
            {
                moduleDefId = AddModuleDefinition("SearchInput", "The Search Input module provides the ability to submit a search to a given search results module.", "Search Input", false, false);
                AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/SearchInput/SearchInput.ascx", "", SecurityAccessLevel.Anonymous, 0);
                AddModuleControl(moduleDefId, "Settings", "Search Input Settings", "DesktopModules/Admin/SearchInput/Settings.ascx", "", SecurityAccessLevel.Edit, 0);
            }

            // add the Search Results module
            if (CoreModuleExists("SearchResults") == false)
            {
                moduleDefId = AddModuleDefinition("SearchResults", "The Search Reasults module provides the ability to display search results.", "Search Results", false, false);
                AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/SearchResults/SearchResults.ascx", "", SecurityAccessLevel.Anonymous, 0);
                AddModuleControl(moduleDefId, "Settings", "Search Results Settings", "DesktopModules/Admin/SearchResults/Settings.ascx", "", SecurityAccessLevel.Edit, 0);

                //Add the Search Module/Page to all configured portals
                AddSearchResults(moduleDefId);
            }

            // add the site wizard module to the admin tab
            if (CoreModuleExists("SiteWizard") == false)
            {
                moduleDefId = AddModuleDefinition("SiteWizard", "The Administrator can use this user-friendly wizard to set up the common Extensions of the Portal/Site.", "Site Wizard");
                AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/SiteWizard/Sitewizard.ascx", "", SecurityAccessLevel.Admin, 0);
                AddAdminPages("Site Wizard", "Configure portal settings, page design and apply a site template using a step-by-step wizard.", "icon_wizard_16px.gif", "icon_wizard_32px.gif", true, moduleDefId, "Site Wizard", "icon_wizard_16px.gif");
            }

            //add Lists module and tab
            if (HostTabExists("Lists") == false)
            {
                moduleDefId = AddModuleDefinition("Lists", "Allows you to edit common lists.", "Lists");
                AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/Lists/ListEditor.ascx", "", SecurityAccessLevel.Host, 0);

                //Create New Host Page (or get existing one)
                newPage = AddHostPage("Lists", "Manage common lists.", "icon_lists_16px.gif", "icon_lists_32px.gif", true);

                //Add Module To Page
                AddModuleToPage(newPage, moduleDefId, "Lists", "icon_lists_16px.gif");
            }

            if (HostTabExists("Superuser Accounts") == false)
            {
                //add SuperUser Accounts module and tab
                DesktopModuleInfo objDesktopModuleInfo = DesktopModuleController.GetDesktopModuleByModuleName("Security", Null.NullInteger);
                moduleDefId = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("User Accounts", objDesktopModuleInfo.DesktopModuleID).ModuleDefID;

                //Create New Host Page (or get existing one)
                newPage = AddHostPage("Superuser Accounts", "Manage host user accounts.", "icon_users_16px.gif", "icon_users_32px.gif", true);

                //Add Module To Page
                AddModuleToPage(newPage, moduleDefId, "SuperUser Accounts", "icon_users_32px.gif");
            }

            //Add Edit Role Groups
            moduleDefId = GetModuleDefinition("Security", "Security Roles");
            AddModuleControl(moduleDefId, "EditGroup", "Edit Role Groups", "DesktopModules/Admin/Security/EditGroups.ascx", "icon_securityroles_32px.gif", SecurityAccessLevel.Edit, Null.NullInteger);
            AddModuleControl(moduleDefId, "UserSettings", "Manage User Settings", "DesktopModules/Admin/Security/UserSettings.ascx", "~/images/settings.gif", SecurityAccessLevel.Edit, Null.NullInteger);

            //Add User Accounts Controls
            moduleDefId = GetModuleDefinition("Security", "User Accounts");
            AddModuleControl(moduleDefId, "ManageProfile", "Manage Profile Definition", "DesktopModules/Admin/Security/ProfileDefinitions.ascx", "icon_users_32px.gif", SecurityAccessLevel.Edit, Null.NullInteger);
            AddModuleControl(moduleDefId, "EditProfileProperty", "Edit Profile Property Definition", "DesktopModules/Admin/Security/EditProfileDefinition.ascx", "icon_users_32px.gif", SecurityAccessLevel.Edit, Null.NullInteger);
            AddModuleControl(moduleDefId, "UserSettings", "Manage User Settings", "DesktopModules/Admin/Security/UserSettings.ascx", "~/images/settings.gif", SecurityAccessLevel.Edit, Null.NullInteger);
            AddModuleControl(Null.NullInteger, "Profile", "Profile", "DesktopModules/Admin/Security/ManageUsers.ascx", "icon_users_32px.gif", SecurityAccessLevel.Anonymous, Null.NullInteger);
            AddModuleControl(Null.NullInteger, "SendPassword", "Send Password", "DesktopModules/Admin/Security/SendPassword.ascx", "", SecurityAccessLevel.Anonymous, Null.NullInteger);
            AddModuleControl(Null.NullInteger, "ViewProfile", "View Profile", "DesktopModules/Admin/Security/ViewProfile.ascx", "icon_users_32px.gif", SecurityAccessLevel.Anonymous, Null.NullInteger);

            //Update Child Portal subHost.aspx
            UpdateChildPortalsDefaultPage();

            // add the solutions explorer module to the admin tab
            if (CoreModuleExists("Solutions") == false)
            {
                moduleDefId = AddModuleDefinition("Solutions", "Browse additional solutions for your application.", "Solutions", false, false);
                AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/Solutions/Solutions.ascx", "", SecurityAccessLevel.Admin, 0);
                AddAdminPages("Solutions", "DotNetNuke Solutions Explorer page provides easy access to locate free and commercial DotNetNuke modules, skin and more.", "icon_solutions_16px.gif", "icon_solutions_32px.gif", true, moduleDefId, "Solutions Explorer", "icon_solutions_32px.gif");
            }


            //Add Search Skin Object
            AddSkinControl("SEARCH", "DotNetNuke.SearchSkinObject", "Admin/Skins/Search.ascx");

            //Add TreeView Skin Object
            AddSkinControl("TREEVIEW", "DotNetNuke.TreeViewSkinObject", "Admin/Skins/TreeViewMenu.ascx");

            //Add Text Skin Object
            AddSkinControl("TEXT", "DotNetNuke.TextSkinObject", "Admin/Skins/Text.ascx");

            //Add Styles Skin Object

            AddSkinControl("STYLES", "DotNetNuke.StylesSkinObject", "Admin/Skins/Styles.ascx");
        }

        private static void UpgradeToVersion600()
        {
            var tabController = new TabController();

            var hostPages = tabController.GetTabsByPortal(Null.NullInteger);

            //This ensures that all host pages have a tab path.
            //so they can be found later. (DNNPRO-17129)
            foreach (var hostPage in hostPages.Values)
            {
                hostPage.TabPath = Globals.GenerateTabPath(hostPage.ParentId, hostPage.TabName);
                tabController.UpdateTab(hostPage);
            }

            var settings = PortalController.GetCurrentPortalSettings();

            var moduleController = new ModuleController();

            if (settings != null)
            {
                var hostTab = tabController.GetTab(settings.SuperTabId, Null.NullInteger, false);
                hostTab.IsVisible = false;
                tabController.UpdateTab(hostTab);
                foreach (var module in moduleController.GetTabModules(settings.SuperTabId).Values)
                {
                    moduleController.UpdateTabModuleSetting(module.TabModuleID, "hideadminborder", "true");
                }
            }

            //remove timezone editor
            int moduleDefId = GetModuleDefinition("Languages", "Languages");
            RemoveModuleControl(moduleDefId, "TimeZone");

            //6.0 requires the old TimeZone property to be marked as Deleted - Delete for Host
            ProfilePropertyDefinition ppdHostTimeZone = ProfileController.GetPropertyDefinitionByName(Null.NullInteger, "TimeZone");
            if (ppdHostTimeZone != null)
            {
                ProfileController.DeletePropertyDefinition(ppdHostTimeZone);
            }

            var portalController = new PortalController();
            foreach (PortalInfo portal in portalController.GetPortals())
            {
                //update timezoneinfo
#pragma warning disable 612,618
                TimeZoneInfo timeZoneInfo = Localization.Localization.ConvertLegacyTimeZoneOffsetToTimeZoneInfo(portal.TimeZoneOffset);
#pragma warning restore 612,618
                PortalController.UpdatePortalSetting(portal.PortalID, "TimeZone", timeZoneInfo.Id, false);

                //6.0 requires the old TimeZone property to be marked as Deleted - Delete for Portals
                ProfilePropertyDefinition ppdTimeZone = ProfileController.GetPropertyDefinitionByName(portal.PortalID, "TimeZone");
                if (ppdTimeZone != null)
                {
                    ProfileController.DeletePropertyDefinition(ppdTimeZone);
                }

                var adminTab = tabController.GetTab(portal.AdminTabId, portal.PortalID, false);

                adminTab.IsVisible = false;
                tabController.UpdateTab(adminTab);

                foreach (var module in moduleController.GetTabModules(portal.AdminTabId).Values)
                {
                    moduleController.UpdateTabModuleSetting(module.TabModuleID, "hideadminborder", "true");
                }
            }

            //Ensure that Display Beta Notice setting is present
            var displayBetaNotice = Host.DisplayBetaNotice;
            HostController.Instance.Update("DisplayBetaNotice", displayBetaNotice ? "Y" : "N");

            moduleDefId = GetModuleDefinition("Languages", "Languages");
            AddModuleControl(moduleDefId, "EnableContent", "Enable Localized Content", "DesktopModules/Admin/Languages/EnableLocalizedContent.ascx", "", SecurityAccessLevel.Host, 0, null, false);

            AddDefaultModuleIcons();

            AddIconToAllowedFiles();

            FavIconsToPortalSettings();

            var tab = tabController.GetTabByName("Host", Null.NullInteger, Null.NullInteger);

            if (tab != null)
            {
                RemoveModule("Extensions", "Module Definitions", tab.TabID, true);
                RemoveModule("Marketplace", "Marketplace", tab.TabID, true);
            }
        }

        private static void UpgradeToVersion601()
        {
            //List module needs to be available to Portals also
            var pkg = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Lists");
            if (pkg != null)
            {
                //List package is no longer a system package
                pkg.IsSystemPackage = false;
                PackageController.Instance.SaveExtensionPackage(pkg);

                //List desktop module is no longer premium or admin module
                var desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(pkg.PackageID);
                desktopModule.IsAdmin = false;
                desktopModule.IsPremium = false;
                DesktopModuleController.SaveDesktopModule(desktopModule, false, true);

                var permissionController = new PermissionController();
                ArrayList permissions = permissionController.GetPermissionByCodeAndKey("SYSTEM_DESKTOPMODULE", "DEPLOY");
                if (permissions.Count == 1)
                {
                    var permission = permissions[0] as PermissionInfo;
                    if (permission != null)
                    {
                        var portalController = new PortalController();
                        foreach (PortalInfo portal in portalController.GetPortals())
                        {
                            //ensure desktop module is not present in the portal
                            var pdmi = DesktopModuleController.GetPortalDesktopModule(portal.PortalID, desktopModule.DesktopModuleID);
                            if (pdmi == null)
                            {
                                //Parse the permissions
                                var deployPermissions = new DesktopModulePermissionCollection();
                                var deployPermission = new DesktopModulePermissionInfo { PermissionID = permission.PermissionID, AllowAccess = true, RoleID = portal.AdministratorRoleId };
                                deployPermissions.Add(deployPermission);

                                //Add Portal/Module to PortalDesktopModules
                                DesktopModuleController.AddDesktopModuleToPortal(portal.PortalID, desktopModule, deployPermissions, true);
                            }
                        }
                    }
                }
            }
        }

        private static void UpgradeToVersion602()
        {
            //Add avi,mpg,mpeg,mp3,wmv,mov,wav extensions
            var exts = new List<string> { ".avi", ".mpg", ".mpeg", ".mp3", ".wmv", ".mov", ".wav" };
            HostController.Instance.Update("FileExtensions", Host.AllowedExtensionWhitelist.ToStorageString(exts));

            //Fix the icons for SiteMap page
            var portalController = new PortalController();
            foreach (PortalInfo portal in portalController.GetPortals())
            {
                var tabController = new TabController();
                var siteMap = tabController.GetTabByName("Search Engine SiteMap", portal.PortalID);

                if (siteMap != null)
                {
                    siteMap.IconFile = "~/Icons/Sigma/Sitemap_16X16_Standard.png";
                    siteMap.IconFileLarge = "~/Icons/Sigma/Sitemap_32X32_Standard.png";
                    tabController.UpdateTab(siteMap);
                }
            }
        }

        private static void UpgradeToVersion610()
        {
            AddModuleCategories();

            //update languages module
            int moduleDefId = GetModuleDefinition("Languages", "Languages");
            AddModuleControl(moduleDefId, "LocalizePages", "Localize Pages", "DesktopModules/Admin/Languages/LocalizePages.ascx", "~/images/icon_language_32px.gif", SecurityAccessLevel.Edit, 0, Null.NullString, true);

            //add store control
            moduleDefId = AddModuleDefinition("Extensions", "", "Extensions");
            AddModuleControl(moduleDefId, "Store", "Store Details", "DesktopModules/Admin/Extensions/Store.ascx", "~/images/icon_extensions_32px.png", SecurityAccessLevel.Host, 0);

            EnableModalPopUps();

            var tabController = new TabController();
            var tab = tabController.GetTabByName("Portals", Null.NullInteger);
            tab.TabName = "Site Management";
            tabController.UpdateTab(tab);

            var moduleController = new ModuleController();
            foreach (var module in moduleController.GetTabModules(tab.TabID).Values)
            {
                if (module.ModuleTitle == "Portals")
                {
                    module.ModuleTitle = "Site Management";
                    moduleController.UpdateModule(module);
                }
            }

            //Add List module to Admin page of every portal                      
            ModuleDefinitionInfo mDef = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("Lists");
            if (mDef != null)
            {
                AddAdminPages("Lists",
                                "Manage common lists",
                                "~/Icons/Sigma/Lists_16X16_Standard.png",
                                "~/Icons/Sigma/Lists_32X32_Standard.png",
                                true,
                                mDef.ModuleDefID,
                                "Lists",
                                "~/Icons/Sigma/Lists_16X16_Standard.png",
                                true);
            }

            //update DotNetNuke.Portals' friend name to 'Sites'.
            var portalPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Portals");
            if (portalPackage != null)
            {
                portalPackage.FriendlyName = "Sites";
                PackageController.Instance.SaveExtensionPackage(portalPackage);
            }

            //add mobile preview control
            AddModuleControl(Null.NullInteger, "MobilePreview", "Mobile Preview", "DesktopModules/Admin/MobilePreview/Preview.ascx", string.Empty, SecurityAccessLevel.Admin, Null.NullInteger);
        }

        private static void UpgradeToVersion612()
        {
            //update DotNetNuke.Portals' friend name to 'Sites'.
            var portalPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Portals");
            if (portalPackage != null)
            {
                portalPackage.FriendlyName = "Site Management";
                portalPackage.Description =
                    "The Super User can manage the various parent and child sites within the install instance. This module allows you to add a new site, modify an existing site, and delete a site.";
                PackageController.Instance.SaveExtensionPackage(portalPackage);
            }

            //update 'Portal' to 'Sites' in package description.
            portalPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Tabs");
            if (portalPackage != null)
            {
                portalPackage.Description =
                    "Administrators can manage the Pages within the site. This module allows you to create a new page, modify an existing page, delete pages, change the page order, and change the hierarchical page level.";
                PackageController.Instance.SaveExtensionPackage(portalPackage);
            }

            portalPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Vendors");
            if (portalPackage != null)
            {
                portalPackage.Description =
                    "Administrators can manage the Vendors and Banners associated to the site. This module allows you to add a new vendor, modify an existing vendor, and delete a vendor.";
                PackageController.Instance.SaveExtensionPackage(portalPackage);
            }

            portalPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.SiteLog");
            if (portalPackage != null)
            {
                portalPackage.Description =
                    "Administrators can view the details of visitors using their site. There are a variety of reports available to display information regarding site usage, membership, and volumes.";
                PackageController.Instance.SaveExtensionPackage(portalPackage);
            }

            portalPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.SiteWizard");
            if (portalPackage != null)
            {
                portalPackage.Description =
                    "The Administrator can use this user-friendly wizard to set up the common features of the site.";
                PackageController.Instance.SaveExtensionPackage(portalPackage);
            }

            portalPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Security");
            if (portalPackage != null)
            {
                portalPackage.Description =
                    "Administrators can manage the security roles defined for their site. The module allows you to add new security roles, modify existing security roles, delete security roles, and manage the users assigned to security roles.";
                PackageController.Instance.SaveExtensionPackage(portalPackage);
            }

            portalPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.LogViewer");
            if (portalPackage != null)
            {
                portalPackage.Description =
                    "Allows you to view log entries for site events.";
                PackageController.Instance.SaveExtensionPackage(portalPackage);
            }

            portalPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Google Analytics");
            if (portalPackage != null)
            {
                portalPackage.Description =
                    "Configure Site Google Analytics settings.";
                PackageController.Instance.SaveExtensionPackage(portalPackage);
            }
        }

        private static void UpgradeToVersion613()
        {
            //Rename admin pages page's title to 'Page Management'.
            var portalController = new PortalController();
            foreach (PortalInfo portal in portalController.GetPortals())
            {
                var tabController = new TabController();
                var pagesTabId = TabController.GetTabByTabPath(portal.PortalID, "//Admin//Pages", Null.NullString);

                if (pagesTabId != Null.NullInteger)
                {
                    var pagesTab = tabController.GetTab(pagesTabId, portal.PortalID, false);
                    if (pagesTab != null && pagesTab.Title == "Pages")
                    {
                        pagesTab.Title = "Page Management";
                        tabController.UpdateTab(pagesTab);
                    }
                }
            }
        }

        private static void UpgradeToVersion620()
        {
            //add host (system) profanityfilter list
            const string listName = "ProfanityFilter";
            var listController = new ListController();
            var entry = new ListEntryInfo();
            {
                entry.DefinitionID = Null.NullInteger;
                entry.PortalID = Null.NullInteger;
                entry.ListName = listName;
                entry.Value = "ReplaceWithNothing";
                entry.Text = "FindThisText";
                entry.SystemList = true;
            }
            listController.AddListEntry(entry);

            //add same list to each portal
            var portalController = new PortalController();
            foreach (PortalInfo portal in portalController.GetPortals())
            {
                entry.PortalID = portal.PortalID;
                entry.SystemList = false;
                entry.ListName = listName + "-" + portal.PortalID;
                listController.AddListEntry(entry);

                //also create default social relationship entries for the portal
                RelationshipController.Instance.CreateDefaultRelationshipsForPortal(portal.PortalID);
            }

            //Convert old Messages to new schema
            ConvertOldMessages();

            //Replace old Messaging module on User Profile with new 
            ReplaceMessagingModule();

            //Move Photo Property to the end of the propert list.
            MovePhotoProperty();

            //Update Child Portal's Default Page
            UpdateChildPortalsDefaultPage();

            //Add core notification types
            AddCoreNotificationTypesFor620();

            //Console module should not be IPortable
            var consoleModule = DesktopModuleController.GetDesktopModuleByModuleName("Console", Null.NullInteger);
            consoleModule.SupportedFeatures = 0;
            consoleModule.BusinessControllerClass = "";
            DesktopModuleController.SaveDesktopModule(consoleModule, false, false);
        }

        private static void UpgradeToVersion621()
        {
            //update administrators' role description.
            var portalController = new PortalController();
            var moduleController = new ModuleController();
            foreach (PortalInfo portal in portalController.GetPortals())
            {
                //update about me's template
                var myProfileTabId = TabController.GetTabByTabPath(portal.PortalID, "//ActivityFeed//MyProfile", string.Empty);
                if (myProfileTabId != Null.NullInteger)
                {
                    var tabModules = moduleController.GetTabModules(myProfileTabId);
                    foreach (var module in tabModules.Values)
                    {
                        var settings = moduleController.GetTabModuleSettings(module.TabModuleID);
                        if (settings.ContainsKey("ProfileTemplate") && settings["ProfileTemplate"].ToString().Contains("<div class=\"pBio\">"))
                        {
                            var template = @"<div class=""pBio"">
                                    <h3 data-bind=""text: AboutMeText""></h3>
                                    <span data-bind=""text: EmptyAboutMeText, visible: Biography().length==0""></span>
                                    <p data-bind=""html: Biography""></p>
                                    </div>
                                    <div class=""pAddress"">
                                    <h3 data-bind=""text: LocationText""></h3>
                                    <span data-bind=""text: EmptyLocationText, visible: Street().length=0 && Location().length==0 && Country().length==0 && PostalCode().length==0""></span>
                                    <p><span data-bind=""text: Street()""></span><span data-bind=""visible: Street().length > 0""><br/></span>
                                    <span data-bind=""text: Location()""></span><span data-bind=""visible: Location().length > 0""><br/></span>
                                    <span data-bind=""text: Country()""></span><span data-bind=""visible: Country().length > 0""><br/></span>
                                    <span data-bind=""text: PostalCode()""></span>
                                    </p>
                                    </div>
                                    <div class=""pContact"">
                                    <h3 data-bind=""text: GetInTouchText""></h3>
                                    <span data-bind=""text: EmptyGetInTouchText, visible: Telephone().length==0 && Email().length==0 && Website().length==0 && IM().length==0""></span>
                                    <ul>
                                    <li data-bind=""visible: Telephone().length > 0""><strong><span data-bind=""text: TelephoneText"">:</span></strong> <span data-bind=""text: Telephone()""></span></li>
                                    <li data-bind=""visible: Email().length > 0""><strong><span data-bind=""text: EmailText"">:</span></strong> <span data-bind=""text: Email()""></span></li>
                                    <li data-bind=""visible: Website().length > 0""><strong><span data-bind=""text: WebsiteText"">:</span></strong> <span data-bind=""text: Website()""></span></li>
                                    <li data-bind=""visible: IM().length > 0""><strong><span data-bind=""text: IMText"">:</span></strong> <span data-bind=""text: IM()""></span></li>
                                    </ul>
                                    </div>
                                    <div class=""dnnClear""></div>";
                            moduleController.UpdateTabModuleSetting(module.TabModuleID, "ProfileTemplate", template);
                        }
                    }
                }
            }
        }

        private static void UpgradeToVersion623()
        {
            if (Host.jQueryUrl == "http://ajax.googleapis.com/ajax/libs/jquery/1/jquery.min.js")
            {
                HostController.Instance.Update("jQueryUrl", jQuery.DefaultHostedUrl);
            }

            if (Host.jQueryUIUrl == "http://ajax.googleapis.com/ajax/libs/jqueryui/1/jquery-ui.min.js")
            {
                HostController.Instance.Update("jQueryUIUrl", jQuery.DefaultUIHostedUrl);
            }
        }

        private static void UpgradeToVersion624()
        {
            UninstallPackage("DotNetNuke.MarketPlace");
        }

        private static void UpgradeToVersion700()
        {
            // add the site Advanced Settings module to the admin tab
            if (CoreModuleExists("AdvancedSettings") == false)
            {
                var moduleDefId = AddModuleDefinition("AdvancedSettings", "", "Advanced Settings");
                AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/AdvancedSettings/AdvancedSettings.ascx", "", SecurityAccessLevel.Admin, 0);
                AddAdminPages("Advanced Settings",
                            "",
                            "~/Icons/Sigma/AdvancedSettings_16X16_Standard.png",
                            "~/Icons/Sigma/AdvancedSettings_32X32_Standard.png",
                            true,
                            moduleDefId,
                            "Advanced Settings",
                            "~/Icons/Sigma/AdvancedSettings_16X16_Standard.png");
            }

            ConvertCoreNotificationTypeActionsFor700();

            //Remove Feed Explorer module
            DesktopModuleController.DeleteDesktopModule("FeedExplorer");
            DesktopModuleController.DeleteDesktopModule("Solutions");

            //Register Newtonsoft assembly
            DataProvider.Instance().RegisterAssembly(Null.NullInteger, "Newtonsoft.Json.dll", "4.5.6");

            //subhost.aspx was updated
            UpdateChildPortalsDefaultPage();
        }

        private static void UpgradeToVersion710()
        {
            //create a placeholder entry - uses the most common 5 character password (seed list is 6 characters and above)
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

            //add list to each portal and update primary alias
            var portalController = new PortalController();
            foreach (PortalInfo portal in portalController.GetPortals())
            {
                entry.PortalID = portal.PortalID;
                entry.SystemList = false;
                entry.ListName = listName + "-" + portal.PortalID;
                listController.AddListEntry(entry);

                var defaultAlias = PortalController.GetPortalSetting("DefaultPortalAlias", portal.PortalID, String.Empty);
                if (!String.IsNullOrEmpty(defaultAlias))
                {
                    foreach (var alias in TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(portal.PortalID).Where(alias => alias.HTTPAlias == defaultAlias))
                    {
                        alias.IsPrimary = true;
                        TestablePortalAliasController.Instance.UpdatePortalAlias(alias);
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


            //only perform following for an existing installation upgrading
            if (Globals.Status == Globals.UpgradeStatus.Upgrade)
            {
                UpdateFoldersForParentId();
                ImportDocumentLibraryCategories();
                ImportDocumentLibraryCategoryAssoc(fileContentType);
            }

            //Add 404 Log
            var logController = new LogController();
            var logTypeInfo = new LogTypeInfo
            {
                LogTypeKey = EventLogController.EventLogType.PAGE_NOT_FOUND_404.ToString(),
                LogTypeFriendlyName = "HTTP Error Code 404 Page Not Found",
                LogTypeDescription = "",
                LogTypeCSSClass = "OperationFailure",
                LogTypeOwner = "DotNetNuke.Logging.EventLogType"
            };
            logController.AddLogType(logTypeInfo);

            //Add LogType
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
                LogTypePortalID = "*"
            };
            logController.AddLogTypeConfigInfo(logTypeConf);

            UninstallPackage("DotNetNuke.SearchInput");

            //enable password strength meter for new installs only
            HostController.Instance.Update("EnableStrengthMeter", Globals.Status == Globals.UpgradeStatus.Install ? "Y" : "N");

            //Add IP filter log type
            var logTypeFilterInfo = new LogTypeInfo
            {
                LogTypeKey = EventLogController.EventLogType.IP_LOGIN_BANNED.ToString(),
                LogTypeFriendlyName = "HTTP Error Code 403.6 forbidden ip address rejected",
                LogTypeDescription = "",
                LogTypeCSSClass = "OperationFailure",
                LogTypeOwner = "DotNetNuke.Logging.EventLogType"
            };
            logController.AddLogType(logTypeFilterInfo);

            //Add LogType
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
                LogTypePortalID = "*"
            };
            logController.AddLogTypeConfigInfo(logTypeFilterConf);

            var tabController = new TabController();

            int tabID = TabController.GetTabByTabPath(Null.NullInteger, "//Host//SearchAdmin", Null.NullString);
            if (tabID > Null.NullInteger)
                tabController.DeleteTab(tabID, Null.NullInteger);

            var modDef = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("Search Admin");

            if (modDef != null)
                AddAdminPages("Search Admin", "Manage search settings associated with DotNetNuke's search capability.", "~/Icons/Sigma/Search_16x16_Standard.png", "~/Icons/Sigma/Search_32x32_Standard.png", true, modDef.ModuleDefID, "Search Admin", "");

            CopyGettingStartedStyles();
        }

        private static void UpgradeToVersion711()
        {
            DesktopModuleController.DeleteDesktopModule("FileManager");

            //Add TabUrl Logtypes
            var logController = new LogController();
            var logTypeInfo = new LogTypeInfo
            {
                LogTypeKey = EventLogController.EventLogType.TABURL_CREATED.ToString(),
                LogTypeFriendlyName = "TabURL created",
                LogTypeDescription = "",
                LogTypeCSSClass = "OperationSuccess",
                LogTypeOwner = "DotNetNuke.Logging.EventLogType"
            };
            logController.AddLogType(logTypeInfo);

            logTypeInfo.LogTypeKey = EventLogController.EventLogType.TABURL_UPDATED.ToString();
            logTypeInfo.LogTypeFriendlyName = "TabURL updated";
            logController.AddLogType(logTypeInfo);

            logTypeInfo.LogTypeKey = EventLogController.EventLogType.TABURL_DELETED.ToString();
            logTypeInfo.LogTypeFriendlyName = "TabURL deleted";
            logController.AddLogType(logTypeInfo);

        }

        private static void UpgradeToVersion712()
        {
            //update console module in Admin/Host page to set OrderTabsByHierarchy setting to true.
            var portalController = new PortalController();
            var moduleController = new ModuleController();
            foreach (PortalInfo portal in portalController.GetPortals())
            {
                var tabId = TabController.GetTabByTabPath(portal.PortalID, "//Admin", Null.NullString);
                if (tabId != Null.NullInteger)
                {
                    foreach (var module in new ModuleController().GetTabModules(tabId).Where(m => m.Value.ModuleDefinition.FriendlyName == "Console"))
                    {
                        moduleController.UpdateModuleSetting(module.Key, "OrderTabsByHierarchy", "True");
                    }
                }
            }

            var hostTabId = TabController.GetTabByTabPath(Null.NullInteger, "//Host", Null.NullString);
            if (hostTabId != Null.NullInteger)
            {
                foreach (var module in new ModuleController().GetTabModules(hostTabId).Where(m => m.Value.ModuleDefinition.FriendlyName == "Console"))
                {
                    moduleController.UpdateModuleSetting(module.Key, "OrderTabsByHierarchy", "True");
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

            DesktopModuleController.AddModuleCategory("Developer");
            var moduleDefId = AddModuleDefinition("Module Creator", "Development of modules.", "Module Creator");
            AddModuleControl(moduleDefId, "", "", "DesktopModules/Admin/ModuleCreator/CreateModule.ascx", "~/DesktopModules/Admin/ModuleCreator/icon.png", SecurityAccessLevel.Host, 0);
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

            //only perform following for an existing installation upgrading
            if (Globals.Status == Globals.UpgradeStatus.Upgrade)
            {
                ImportDocumentLibraryCategories();
                ImportDocumentLibraryCategoryAssoc(fileContentType);

                AddDefaultContentWorkflows();
            }
            
            //fixes issue introduced by eventlog's being defined in upgrade.cs
            PortalController.EnsureRequiredEventLogTypesExist();

            //Remove Professional Features pages from CE
            int advancedFeaturesTabId = TabController.GetTabByTabPath(Null.NullInteger, "//Host//ProfessionalFeatures", Null.NullString);
            var tabController = new TabController();
            if (DotNetNukeContext.Current.Application.Name == "DNNCORP.CE")
            {
                foreach (var tab in TabController.GetTabsByParent(advancedFeaturesTabId, Null.NullInteger))
                {
                    tabController.DeleteTab(tab.TabID, Null.NullInteger);
                }
                tabController.DeleteTab(advancedFeaturesTabId, Null.NullInteger);
            }

            //Remove Whats New
            int whatsNewTabId = TabController.GetTabByTabPath(Null.NullInteger, "//Host//WhatsNew", Null.NullString);
            tabController.DeleteTab(whatsNewTabId, Null.NullInteger);

            //Remove WhatsNew module
            DesktopModuleController.DeleteDesktopModule("WhatsNew");

            //read plaintext password via old API and encrypt
            var current = HostController.Instance.GetString("SMTPPassword");
            if (!string.IsNullOrEmpty(current))
            {
                HostController.Instance.UpdateEncryptedString("SMTPPassword", current, Config.GetDecryptionkey());
            }
        }


        private static void AddDefaultContentWorkflows()
        {
            foreach (PortalInfo portal in TestablePortalController.Instance.GetPortals())
            {
                ContentWorkflowController.Instance.CreateDefaultWorkflows(portal.PortalID);
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
                dr = dataProvider.ExecuteReader("ImportDocumentLibraryCategoryAssoc");
                var termController = new TermController();
                var vocabulary = new VocabularyController().GetVocabularies().Single(v => v.Name == "Tags");
                var terms = termController.GetTermsByVocabulary(vocabulary.VocabularyId);

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
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "UpgradeFolders");
            //Move old messages to new format. Do this in smaller batches so we can send feedback to browser and don't time out
            var foldersToConvert = DataProvider.Instance().GetLegacyFolderCount();
            var foldersRemaining = foldersToConvert;

            if (foldersRemaining > 0)
            {
                //Create an empty line
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, "<br/>", false);
            }

            while (foldersRemaining > 0)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, string.Format("Converting old Folders to new format. Total: {0} [Remaining: {1}]<br/>", foldersToConvert, foldersRemaining));
                try
                {
                    DataProvider.Instance().UpdateLegacyFolders();
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


        private static void UninstallPackage(string packageName)
        {
            var searchInput = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == packageName);
            if (searchInput != null)
            {
                var searchInputInstaller = new Installer.Installer(searchInput, Globals.ApplicationMapPath);
                searchInputInstaller.UnInstall(true);
            }
        }

        private static void ConvertCoreNotificationTypeActionsFor700()
        {
            var notificationTypeNames = new[] { "FriendRequest", "FollowerRequest", "FollowBackRequest", "TranslationSubmitted" };

            foreach (var name in notificationTypeNames)
            {
                var nt = NotificationsController.Instance.GetNotificationType(name);

                var actions = NotificationsController.Instance.GetNotificationTypeActions(nt.NotificationTypeId).ToList();

                if (actions.Any())
                {

                    foreach (var action in actions)
                    {
                        action.APICall = action.APICall.Replace(".ashx", "");
                        NotificationsController.Instance.DeleteNotificationTypeAction(action.NotificationTypeActionId);
                    }

                    NotificationsController.Instance.SetNotificationTypeActions(actions, nt.NotificationTypeId);
                }
            }
        }

        private static void AddCoreNotificationTypesFor620()
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddCoreNotificationTypesFor620");
            var actions = new List<NotificationTypeAction>();

            //Friend request
            var type = new NotificationType { Name = "FriendRequest", Description = "Friend Request" };
            actions.Add(new NotificationTypeAction
            {
                NameResourceKey = "Accept",
                DescriptionResourceKey = "AcceptFriend",
                APICall = "DesktopModules/InternalServices/API/RelationshipService/AcceptFriend"
            });
            NotificationsController.Instance.CreateNotificationType(type);
            NotificationsController.Instance.SetNotificationTypeActions(actions, type.NotificationTypeId);

            //Follower
            type = new NotificationType { Name = "FollowerRequest", Description = "Follower Request" };
            NotificationsController.Instance.CreateNotificationType(type);

            //Follow Back
            type = new NotificationType { Name = "FollowBackRequest", Description = "Follow Back Request" };
            actions.Clear();
            actions.Add(new NotificationTypeAction
            {
                NameResourceKey = "FollowBack",
                DescriptionResourceKey = "FollowBack",
                ConfirmResourceKey = "",
                APICall = "DesktopModules/InternalServices/API/RelationshipService/FollowBack"
            });
            NotificationsController.Instance.CreateNotificationType(type);
            NotificationsController.Instance.SetNotificationTypeActions(actions, type.NotificationTypeId);

            //Translation submitted
            type = new NotificationType { Name = "TranslationSubmitted", Description = "Translation Submitted" };
            NotificationsController.Instance.CreateNotificationType(type);
        }

        private static void ConvertOldMessages()
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "ConvertOldMessages");
            //Move old messages to new format. Do this in smaller batches so we can send feedback to browser and don't time out
            var messagesToConvert = InternalMessagingController.Instance.CountLegacyMessages();
            var messagesRemaining = messagesToConvert;
            const int batchSize = 500;

            if (messagesRemaining > 0)
            {
                //Create an empty line
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, "<br/>", false);
            }

            while (messagesRemaining > 0)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, string.Format("Converting old Messages to new format. Total: {0} [Remaining: {1}]<br/>", messagesToConvert, messagesRemaining));
                try
                {
                    InternalMessagingController.Instance.ConvertLegacyMessages(0, batchSize);
                }
                catch (Exception ex)
                {
                    Exceptions.Exceptions.LogException(ex);
                }

                messagesRemaining -= batchSize;
            }

            if (messagesToConvert > 0)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, string.Format("Conversion of old Messages Completed. Total Converted: {0}<br/>", messagesToConvert));
            }
        }

        private static void ReplaceMessagingModule()
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "ReplaceMessagingModule");
            var portalController = new PortalController();
            var moduleController = new ModuleController();
            var tabController = new TabController();

            var moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("Message Center");
            if (moduleDefinition == null) return;

            var portals = portalController.GetPortals();
            foreach (PortalInfo portal in portals)
            {
                if (portal.UserTabId > Null.NullInteger)
                {
                    //Find TabInfo
                    TabInfo tab = tabController.GetTab(portal.UserTabId, portal.PortalID, true);
                    if (tab != null)
                    {
                        //Add new module to the page
                        AddModuleToPage(tab, moduleDefinition.ModuleDefID, "Message Center", "", true);
                    }

                    foreach (KeyValuePair<int, ModuleInfo> kvp in moduleController.GetTabModules(portal.UserTabId))
                    {
                        var module = kvp.Value;
                        if (module.DesktopModule.FriendlyName == "Messaging")
                        {
                            //Delete the Module from the Modules list
                            moduleController.DeleteTabModule(module.TabID, module.ModuleID, false);
                            break;
                        }
                    }
                }
            }
        }

        private static void MovePhotoProperty()
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "MovePhotoProperty");
            var portalController = new PortalController();
            foreach (PortalInfo portal in portalController.GetPortals())
            {
                var properties = ProfileController.GetPropertyDefinitionsByPortal(portal.PortalID).Cast<ProfilePropertyDefinition>();
                var propPhoto = properties.FirstOrDefault(p => p.PropertyName == "Photo");
                if (propPhoto != null)
                {
                    var maxOrder = properties.Max(p => p.ViewOrder);
                    if (propPhoto.ViewOrder != maxOrder)
                    {
                        properties.Where(p => p.ViewOrder > propPhoto.ViewOrder).ToList().ForEach(p =>
                        {
                            p.ViewOrder -= 2;
                            ProfileController.UpdatePropertyDefinition(p);
                        });
                        propPhoto.ViewOrder = maxOrder;
                        ProfileController.UpdatePropertyDefinition(propPhoto);
                    }
                }
            }
        }

        private static void UpdateChildPortalsDefaultPage()
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "UpdateChildPortalsDefaultPage");
            //Update Child Portal subHost.aspx
            foreach (PortalAliasInfo aliasInfo in TestablePortalAliasController.Instance.GetPortalAliases().Values)
            {
                //For the alias to be for a child it must be of the form ...../child
                int intChild = aliasInfo.HTTPAlias.IndexOf("/");
                if (intChild != -1 && intChild != (aliasInfo.HTTPAlias.Length - 1))
                {
                    var childPath = Globals.ApplicationMapPath + "\\" + aliasInfo.HTTPAlias.Substring(intChild + 1);
                    if (!string.IsNullOrEmpty(Globals.ApplicationPath))
                    {
                        childPath = childPath.Replace("\\", "/");
                        childPath = childPath.Replace(Globals.ApplicationPath, "");
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
                            //check file is readonly
                            bool wasReadonly = false;
                            FileAttributes attributes = File.GetAttributes(childDefaultPage);
                            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                            {
                                wasReadonly = true;
                                //remove readonly attribute
                                File.SetAttributes(childDefaultPage, FileAttributes.Normal);
                            }

                            //Rename existing file                                
                            File.Copy(childDefaultPage, childPath + "\\old_" + Globals.glbDefaultPage, true);

                            //copy file
                            File.Copy(Globals.HostMapPath + "subhost.aspx", childDefaultPage, true);

                            //set back the readonly attribute
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
            //copy getting started css to portals folder.
            var hostGettingStartedFile = string.Format("{0}GettingStarted.css", Globals.HostMapPath);
            var tabController = new TabController();
            foreach (PortalInfo portal in new PortalController().GetPortals())
            {

                if (File.Exists(hostGettingStartedFile))
                {
                    var portalFile = portal.HomeDirectoryMapPath + "GettingStarted.css";
                    if (!File.Exists(portalFile))
                    {
                        File.Copy(hostGettingStartedFile, portalFile);
                    }
                }

                //update the getting started page to have this custom style sheet.
                var gettingStartedTabId = PortalController.GetPortalSettingAsInteger("GettingStartedTabId", portal.PortalID, Null.NullInteger);
                if (gettingStartedTabId > Null.NullInteger)
                {
                    // check if tab exists
                    if (tabController.GetTab(gettingStartedTabId, portal.PortalID, true) != null)
                    {
                        tabController.UpdateTabSetting(gettingStartedTabId, "CustomStylesheet", "GettingStarted.css");
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  AddAdminPages adds an Admin Page and an associated Module to all configured Portals
        ///</summary>
        ///<param name = "tabName">The Name to give this new Tab</param>
        ///<param name="description"></param>
        ///<param name = "tabIconFile">The Icon for this new Tab</param>
        ///<param name="tabIconFileLarge"></param>
        ///<param name = "isVisible">A flag indicating whether the tab is visible</param>
        ///<param name = "moduleDefId">The Module Deinition Id for the module to be aded to this tab</param>
        ///<param name = "moduleTitle">The Module's title</param>
        ///<param name = "moduleIconFile">The Module's icon</param>
        ///<param name = "inheritPermissions">Modules Inherit the Pages View Permisions</param>
        ///<history>
        ///  [cnurse]	11/11/2004	created
        ///</history>
        ///-----------------------------------------------------------------------------
        public static void AddAdminPages(string tabName, string description, string tabIconFile, string tabIconFileLarge, bool isVisible, int moduleDefId, string moduleTitle, string moduleIconFile, bool inheritPermissions)
        {
            var portalController = new PortalController();
            ArrayList portals = portalController.GetPortals();

            //Add Page to Admin Menu of all configured Portals
            for (int intPortal = 0; intPortal <= portals.Count - 1; intPortal++)
            {
                var portal = (PortalInfo)portals[intPortal];

                //Create New Admin Page (or get existing one)
                TabInfo newPage = AddAdminPage(portal, tabName, description, tabIconFile, tabIconFileLarge, isVisible);

                //Add Module To Page
                AddModuleToPage(newPage, moduleDefId, moduleTitle, moduleIconFile, inheritPermissions);
            }
        }

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  AddAdminPage adds an Admin Tab Page
        ///</summary>
        ///<param name = "portal">The Portal</param>
        ///<param name = "tabName">The Name to give this new Tab</param>
        ///<param name="description"></param>
        ///<param name = "tabIconFile">The Icon for this new Tab</param>
        ///<param name="tabIconFileLarge"></param>
        ///<param name = "isVisible">A flag indicating whether the tab is visible</param>
        ///<history>
        ///  [cnurse]	11/11/2004	created
        ///</history>
        ///-----------------------------------------------------------------------------
        public static TabInfo AddAdminPage(PortalInfo portal, string tabName, string description, string tabIconFile, string tabIconFileLarge, bool isVisible)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddAdminPage:" + tabName);
            var tabController = new TabController();
            TabInfo adminPage = tabController.GetTab(portal.AdminTabId, portal.PortalID, false);

            if ((adminPage != null))
            {
                var tabPermissionCollection = new TabPermissionCollection();
                AddPagePermission(tabPermissionCollection, "View", Convert.ToInt32(portal.AdministratorRoleId));
                AddPagePermission(tabPermissionCollection, "Edit", Convert.ToInt32(portal.AdministratorRoleId));
                return AddPage(adminPage, tabName, description, tabIconFile, tabIconFileLarge, isVisible, tabPermissionCollection, true);
            }
            return null;
        }

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  AddHostPage adds a Host Tab Page
        ///</summary>
        ///<param name = "tabName">The Name to give this new Tab</param>
        ///<param name="description"></param>
        ///<param name = "tabIconFile">The Icon for this new Tab</param>
        ///<param name="tabIconFileLarge"></param>
        ///<param name = "isVisible">A flag indicating whether the tab is visible</param>
        ///<history>
        ///  [cnurse]	11/11/2004	created
        ///</history>
        ///-----------------------------------------------------------------------------
        public static TabInfo AddHostPage(string tabName, string description, string tabIconFile, string tabIconFileLarge, bool isVisible)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddHostPage:" + tabName);
            var tabController = new TabController();
            TabInfo hostPage = tabController.GetTabByName("Host", Null.NullInteger);

            if ((hostPage != null))
            {
                return AddPage(hostPage, tabName, description, tabIconFile, tabIconFileLarge, isVisible, new TabPermissionCollection(), true);
            }
            return null;
        }

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  AddModuleControl adds a new Module Control to the system
        ///</summary>
        ///<remarks>
        ///</remarks>
        ///<param name = "moduleDefId">The Module Definition Id</param>
        ///<param name = "controlKey">The key for this control in the Definition</param>
        ///<param name = "controlTitle">The title of this control</param>
        ///<param name = "controlSrc">Te source of ths control</param>
        ///<param name = "iconFile">The icon file</param>
        ///<param name = "controlType">The type of control</param>
        ///<param name = "viewOrder">The vieworder for this module</param>
        ///<history>
        ///  [cnurse]	11/08/2004	documented
        ///</history>
        ///-----------------------------------------------------------------------------
        public static void AddModuleControl(int moduleDefId, string controlKey, string controlTitle, string controlSrc, string iconFile, SecurityAccessLevel controlType, int viewOrder)
        {
            //Call Overload with HelpUrl = Null.NullString
            AddModuleControl(moduleDefId, controlKey, controlTitle, controlSrc, iconFile, controlType, viewOrder, Null.NullString);
        }

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  AddModuleDefinition adds a new Core Module Definition to the system
        ///</summary>
        ///<remarks>
        ///  This overload asumes the module is an Admin module and not a Premium Module
        ///</remarks>
        ///<param name = "desktopModuleName">The Friendly Name of the Module to Add</param>
        ///<param name = "description">Description of the Module</param>
        ///<param name = "moduleDefinitionName">The Module Definition Name</param>
        ///<returns>The Module Definition Id of the new Module</returns>
        ///<history>
        ///  [cnurse]	10/14/2004	documented
        ///</history>
        ///-----------------------------------------------------------------------------
        public static int AddModuleDefinition(string desktopModuleName, string description, string moduleDefinitionName)
        {
            //Call overload with Premium=False and Admin=True
            return AddModuleDefinition(desktopModuleName, description, moduleDefinitionName, false, true);
        }

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  AddModuleToPage adds a module to a Page
        ///</summary>
        ///<remarks>
        ///</remarks>
        ///<param name = "page">The Page to add the Module to</param>
        ///<param name = "moduleDefId">The Module Deinition Id for the module to be aded to this tab</param>
        ///<param name = "moduleTitle">The Module's title</param>
        ///<param name = "moduleIconFile">The Module's icon</param>
        ///<param name = "inheritPermissions">Inherit the Pages View Permisions</param>
        ///<history>
        ///  [cnurse]	11/16/2004	created
        ///</history>
        ///-----------------------------------------------------------------------------
		public static int AddModuleToPage(TabInfo page, int moduleDefId, string moduleTitle, string moduleIconFile, bool inheritPermissions)
		{
			return AddModuleToPage(page, moduleDefId, moduleTitle, moduleIconFile, inheritPermissions, true, Globals.glbDefaultPane);
		}

		public static int AddModuleToPage(TabInfo page, int moduleDefId, string moduleTitle, string moduleIconFile, bool inheritPermissions, bool displayTitle, string paneName)
		{
			DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddModuleToPage:" + moduleDefId);
			var moduleController = new ModuleController();
			ModuleInfo moduleInfo;
			int moduleId = Null.NullInteger;

			if ((page != null))
			{
				bool isDuplicate = false;
				foreach (var kvp in moduleController.GetTabModules(page.TabID))
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
						DisplayTitle = displayTitle
					};

					try
					{
						moduleId = moduleController.AddModule(moduleInfo);
					}
					catch (Exception exc)
					{
						Logger.Error(exc);
						DnnInstallLogger.InstallLogError(exc);
					}
				}
			}
			DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogEnd", Localization.Localization.GlobalResourceFile) + "AddModuleToPage:" + moduleDefId);
			return moduleId;
		}


        public static int AddModuleToPage(string tabPath, int portalId, int moduleDefId, string moduleTitle, string moduleIconFile, bool inheritPermissions)
        {
            var tabController = new TabController();
            int moduleId = Null.NullInteger;

            int tabID = TabController.GetTabByTabPath(portalId, tabPath, Null.NullString);
            if ((tabID != Null.NullInteger))
            {
                TabInfo tab = tabController.GetTab(tabID, portalId, true);
                if ((tab != null))
                {
                    moduleId = AddModuleToPage(tab, moduleDefId, moduleTitle, moduleIconFile, inheritPermissions);
                }
            }
            return moduleId;
        }

        public static void AddModuleToPages(string tabPath, int moduleDefId, string moduleTitle, string moduleIconFile, bool inheritPermissions)
        {
            var objPortalController = new PortalController();
            var objTabController = new TabController();

            ArrayList portals = objPortalController.GetPortals();
            foreach (PortalInfo portal in portals)
            {
                int tabID = TabController.GetTabByTabPath(portal.PortalID, tabPath, Null.NullString);
                if ((tabID != Null.NullInteger))
                {
                    TabInfo tab = objTabController.GetTab(tabID, portal.PortalID, true);
                    if ((tab != null))
                    {
                        AddModuleToPage(tab, moduleDefId, moduleTitle, moduleIconFile, inheritPermissions);
                    }
                }
            }
        }
        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   AddPortal manages the Installation of a new DotNetNuke Portal
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [cnurse]	11/06/2004	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static int AddPortal(XmlNode node, bool status, int indent)
        {

            int portalId = -1;
            try
            {
                string hostMapPath = Globals.HostMapPath;
                string childPath = "";
                string domain = "";

                if ((HttpContext.Current != null))
                {
                    domain = Globals.GetDomainName(HttpContext.Current.Request, true).ToLowerInvariant().Replace("/install", "");
                }
                DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "AddPortal:" + domain);
                string portalName = XmlUtils.GetNodeValue(node.CreateNavigator(), "portalname");
                if (status)
                {
                    if (HttpContext.Current != null)
                    {
                        HtmlUtils.WriteFeedback(HttpContext.Current.Response, indent, "Creating Site: " + portalName + "<br>");
                    }
                }

                var portalController = new PortalController();
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

                    //Get the Portal Alias
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

                    //Create default email
                    if (string.IsNullOrEmpty(email))
                    {
                        email = "admin@" + domain.Replace("www.", "");
                        //Remove any domain subfolder information ( if it exists )
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
                    var userInfo = CreateUserInfo(firstName, lastName, username, password, email);

                    //Create Portal
                    portalId = portalController.CreatePortal(portalName,
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
                        //Add Extra Aliases
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
                                    portalController.AddPortalAlias(portalId, portalAlias.InnerText);
                                }
                            }
                        }

                        //Force Administrator to Update Password on first log in
                        PortalInfo portal = portalController.GetPortal(portalId);
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
                    HtmlUtils.WriteFeedback(HttpContext.Current.Response, indent, "<font color='red'>Error: " + ex.Message + ex.StackTrace + "</font><br>");
                    DnnInstallLogger.InstallLogError(ex);
                }
                // failure
                portalId = -1;
            }
            return portalId;
        }

        internal static UserInfo CreateUserInfo(string firstName, string lastName, string userName, string password, string email)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "CreateUserInfo:" + userName);
            var adminUser = new UserInfo
                                {
                                    FirstName = firstName,
                                    LastName = lastName,
                                    Username = userName,
                                    DisplayName = firstName + " " + lastName,
                                    Membership = { Password = password },
                                    Email = email,
                                    IsSuperUser = false
                                };
            adminUser.Membership.Approved = true;
            adminUser.Profile.FirstName = firstName;
            adminUser.Profile.LastName = lastName;
            adminUser.Membership.UpdatePassword = true;
            return adminUser;
        }

        internal static PortalController.PortalTemplateInfo FindBestTemplate(string templateFileName)
        {
            var templates = TestablePortalController.Instance.GetAvailablePortalTemplates();

            //Load Template
            var installTemplate = new XmlDocument();
            Upgrade.GetInstallTemplate(installTemplate);
            //Parse the root node
            XmlNode rootNode = installTemplate.SelectSingleNode("//dotnetnuke");
            String currentCulture = "";
            if (rootNode != null)
            {
                currentCulture = XmlUtils.GetNodeValue(rootNode.CreateNavigator(), "installCulture");
            }

            if (String.IsNullOrEmpty(currentCulture))
            {
                currentCulture = Localization.Localization.SystemLocale;
            }
            currentCulture = currentCulture.ToLower();
            var defaultTemplates =
                templates.Where(x => Path.GetFileName(x.TemplateFilePath) == templateFileName).ToList();

            var match = defaultTemplates.FirstOrDefault(x => x.CultureCode.ToLower() == currentCulture);
            if (match == null)
            {
                match = defaultTemplates.FirstOrDefault(x => x.CultureCode.ToLower().StartsWith(currentCulture.Substring(0, 2)));
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

        public static string BuildUserTable(IDataReader dr, string header, string message)
        {
            string warnings = Null.NullString;
            var stringBuilder = new StringBuilder();
            bool hasRows = false;

            stringBuilder.Append("<h3>" + header + "</h3>");
            stringBuilder.Append("<p>" + message + "</p>");
            stringBuilder.Append("<table cellspacing='4' cellpadding='4' border='0'>");
            stringBuilder.Append("<tr>");
            stringBuilder.Append("<td class='NormalBold'>ID</td>");
            stringBuilder.Append("<td class='NormalBold'>UserName</td>");
            stringBuilder.Append("<td class='NormalBold'>First Name</td>");
            stringBuilder.Append("<td class='NormalBold'>Last Name</td>");
            stringBuilder.Append("<td class='NormalBold'>Email</td>");
            stringBuilder.Append("</tr>");
            while (dr.Read())
            {
                hasRows = true;
                stringBuilder.Append("<tr>");
                stringBuilder.Append("<td class='Norma'>" + dr.GetInt32(0) + "</td>");
                stringBuilder.Append("<td class='Norma'>" + dr.GetString(1) + "</td>");
                stringBuilder.Append("<td class='Norma'>" + dr.GetString(2) + "</td>");
                stringBuilder.Append("<td class='Norma'>" + dr.GetString(3) + "</td>");
                stringBuilder.Append("<td class='Norma'>" + dr.GetString(4) + "</td>");
                stringBuilder.Append("</tr>");
            }

            stringBuilder.Append("</table>");

            if (hasRows)
            {
                warnings = stringBuilder.ToString();
            }


            return warnings;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   CheckUpgrade checks whether there are any possible upgrade issues
        /// </summary>
        /// <history>
        ///   [cnurse]	04/11/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string CheckUpgrade()
        {
            DataProvider dataProvider = DataProvider.Instance();
            IDataReader dr;
            string warnings = Null.NullString;

            try
            {
                dr = dataProvider.ExecuteReader("CheckUpgrade");

                warnings = BuildUserTable(dr, "Duplicate SuperUsers", "We have detected that the following SuperUsers have duplicate entries as Portal Users. Although, no longer supported, these users may have been created in early Betas of DNN v3.0. You need to be aware that after the upgrade, these users will only be able to log in using the Super User Account's password.");

                if (dr.NextResult())
                {
                    warnings += BuildUserTable(dr, "Duplicate Portal Users", "We have detected that the following Users have duplicate entries (they exist in more than one portal). You need to be aware that after the upgrade, the password for some of these users may have been automatically changed (as the system now only uses one password per user, rather than one password per user per portal). It is important to remember that your Users can always retrieve their password using the Password Reminder feature, which will be sent to the Email addess shown in the table.");
                }
            }
            catch (SqlException ex)
            {
                Logger.Error(ex);
                warnings += ex.Message;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                warnings += ex.Message;
            }

            try
            {
                dr = dataProvider.ExecuteReader("GetUserCount");
                dr.Read();
                int userCount = dr.GetInt32(0);
                // ReSharper disable PossibleLossOfFraction
                double time = userCount / 10834;
                // ReSharper restore PossibleLossOfFraction
                if (userCount > 1000)
                {
                    warnings += "<br/><h3>More than 1000 Users</h3><p>This DotNetNuke Database has " + userCount +
                                   " users. As the users and their profiles are transferred to a new format, it is estimated that the script will take ~" + time.ToString("F2") +
                                   " minutes to execute.</p>";
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                warnings += Environment.NewLine + Environment.NewLine + ex.Message;
            }


            return warnings;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   DeleteInstallerFiles - clean up install config
        ///   If installwizard is ran again this will be recreated via the dotnetnuke.install.config.resources file
        /// </summary>
        /// <remarks>
        /// uses FileSystemUtils.DeleteFile as it checks for readonly attribute status
        /// and changes it if required, as well as verifying file exists.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static void DeleteInstallerFiles()
        {
            FileSystemUtils.DeleteFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install", "DotNetNuke.install.config"));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   DeleteFiles - clean up deprecated files and folders
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="providerPath">Path to provider</param>
        /// <param name = "version">The Version being Upgraded</param>
        /// <param name="writeFeedback">Display status in UI?</param>
        /// <history>
        ///   [swalker]	11/09/2004	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string DeleteFiles(string providerPath, Version version, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "DeleteFiles:" + Globals.FormatVersion(version));
            string exceptions = "";
            if (writeFeedback)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, "Cleaning Up Files: " + Globals.FormatVersion(version));
            }

            try
            {
                string listFile = Globals.InstallMapPath + "Cleanup\\" + GetStringVersion(version) + ".txt";

                if (File.Exists(listFile))
                {
                    exceptions = FileSystemUtils.DeleteFiles(FileSystemUtils.ReadFile(listFile).Split('\r', '\n'));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                exceptions += string.Format("Error: {0}{1}", ex.Message + ex.StackTrace, Environment.NewLine);
                // log the results
                DnnInstallLogger.InstallLogError(exceptions);
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

            if (writeFeedback)
            {
                HtmlUtils.WriteSuccessError(HttpContext.Current.Response, (string.IsNullOrEmpty(exceptions)));
            }

            return exceptions;
        }

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  ExecuteScripts manages the Execution of Scripts from the Install/Scripts folder.
        ///  It is also triggered by InstallDNN and UpgradeDNN
        ///</summary>
        ///<remarks>
        ///</remarks>
        ///<param name = "strProviderPath">The path to the Data Provider</param>
        ///<history>
        ///  [cnurse]	05/04/2005	created
        ///</history>
        ///-----------------------------------------------------------------------------
        public static void ExecuteScripts(string strProviderPath)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "ExecuteScripts:" + strProviderPath);
            string scriptPath = Globals.ApplicationMapPath + "\\Install\\Scripts\\";
            if (Directory.Exists(scriptPath))
            {
                string[] files = Directory.GetFiles(scriptPath);
                foreach (string file in files)
                {
                    //Execute if script is a provider script
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

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  ExecuteScript executes a special script
        ///</summary>
        ///<remarks>
        ///</remarks>
        ///<param name = "file">The script file to execute</param>
        ///<history>
        ///  [cnurse]	04/11/2006	created
        ///</history>
        ///-----------------------------------------------------------------------------
        public static void ExecuteScript(string file)
        {
            //Execute if script is a provider script
            if (file.IndexOf("." + DefaultProvider) != -1)
            {
                ExecuteScript(file, true);
            }
        }

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  GetInstallTemplate retrieves the Installation Template as specifeid in web.config
        ///</summary>
        ///<remarks>
        ///</remarks>
        ///<param name = "xmlDoc">The Xml Document to load</param>
        ///<returns>A string which contains the error message - if appropriate</returns>
        ///<history>
        ///  [cnurse]	02/13/2007	created
        ///</history>
        ///-----------------------------------------------------------------------------
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
        ///  SetInstalltemplate saves the XmlDocument back to Installation Template specified in web.config
        /// </summary>
        /// <param name="xmlDoc">The Xml Document to save</param>
        /// <returns>A string which contains the error massage - if appropriate</returns>
        public static string SetInstallTemplate(XmlDocument xmlDoc)
        {
            string errorMessage = Null.NullString;
            string installTemplate = Config.GetSetting("InstallTemplate");
            string filePath = Globals.ApplicationMapPath + "\\Install\\" + installTemplate;
            try
            {
                //ensure the file is not read-only
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

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  GetInstallVersion retrieves the Base Instal Version as specifeid in the install
        ///  template
        ///</summary>
        ///<remarks>
        ///</remarks>
        ///<param name = "xmlDoc">The Install Template</param>
        ///<history>
        ///  [cnurse]	02/13/2007	created
        ///</history>
        ///-----------------------------------------------------------------------------
        public static Version GetInstallVersion(XmlDocument xmlDoc)
        {
            string version = Null.NullString;

            //get base version
            XmlNode node = xmlDoc.SelectSingleNode("//dotnetnuke");
            if ((node != null))
            {
                version = XmlUtils.GetNodeValue(node.CreateNavigator(), "version");
            }

            return new Version(version);
        }

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  GetLogFile gets the filename for the version's log file
        ///</summary>
        ///<remarks>
        ///</remarks>
        ///<param name = "providerPath">The path to the Data Provider</param>
        ///<param name = "version">The Version</param>
        ///<history>
        ///  [cnurse]	02/16/2007	created
        ///</history>
        ///-----------------------------------------------------------------------------
        public static string GetLogFile(string providerPath, Version version)
        {
            return providerPath + GetStringVersion(version) + ".log.resources";
        }

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  GetScriptFile gets the filename for the version
        ///</summary>
        ///<remarks>
        ///</remarks>
        ///<param name = "providerPath">The path to the Data Provider</param>
        ///<param name = "version">The Version</param>
        ///<history>
        ///  [cnurse]	02/16/2007	created
        ///</history>
        ///-----------------------------------------------------------------------------
        public static string GetScriptFile(string providerPath, Version version)
        {
            return providerPath + GetStringVersion(version) + "." + DefaultProvider;
        }

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  GetStringVersion gets the Version String (xx.xx.xx) from the Version
        ///</summary>
        ///<remarks>
        ///</remarks>
        ///<param name = "version">The Version</param>
        ///<history>
        ///  [cnurse]	02/15/2007	created
        ///</history>
        ///-----------------------------------------------------------------------------
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

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  GetSuperUser gets the superuser from the Install Template
        ///</summary>
        ///<remarks>
        ///</remarks>
        ///<param name = "xmlTemplate">The install Templae</param>
        ///<param name = "writeFeedback">a flag to determine whether to output feedback</param>
        ///<history>
        ///  [cnurse]	02/16/2007	created
        ///</history>
        ///-----------------------------------------------------------------------------
        public static UserInfo GetSuperUser(XmlDocument xmlTemplate, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "GetSuperUser");
            XmlNode node = xmlTemplate.SelectSingleNode("//dotnetnuke/superuser");
            UserInfo superUser = null;
            if ((node != null))
            {
                if (writeFeedback)
                {
                    HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Configuring SuperUser:<br>");
                }

                //Parse the SuperUsers nodes
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
                                    IsSuperUser = true
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

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  GetUpgradeScripts gets an ArrayList of the Scripts required to Upgrade to the
        ///  current Assembly Version
        ///</summary>
        ///<remarks>
        ///</remarks>
        ///<param name = "providerPath">The path to the Data Provider</param>
        ///<param name = "databaseVersion">The current Database Version</param>
        ///<history>
        ///  [cnurse]	02/14/2007	created
        ///</history>
        ///-----------------------------------------------------------------------------
        public static ArrayList GetUpgradeScripts(string providerPath, Version databaseVersion)
        {
            var scriptFiles = new ArrayList();
            string[] files = Directory.GetFiles(providerPath, "*." + DefaultProvider);

            Logger.TraceFormat("GetUpgradedScripts databaseVersion:{0} applicationVersion:{1}", databaseVersion, DotNetNukeContext.Current.Application.Version);

            foreach (string file in files)
            {
                // script file name must conform to ##.##.##.DefaultProviderName
                if (file != null)
                {
                    if (GetFileName(file).Length == 9 + DefaultProvider.Length)
                    {
                        var version = new Version(GetFileNameWithoutExtension(file));
                        // check if script file is relevant for upgrade
                        if (version > databaseVersion && version <= DotNetNukeContext.Current.Application.Version)
                        {
                            scriptFiles.Add(file);
                            Logger.TraceFormat("GetUpgradedScripts including {0}", file);
                        }
                        else
                        {
                            Logger.TraceFormat("GetUpgradedScripts excluding {0}", file);
                        }
                    }
                }
            }
            scriptFiles.Sort();

            return scriptFiles;
        }

        private static string GetFileName(string file)
        {
            return Path.GetFileName(file);
        }

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  InitialiseHostSettings gets the Host Settings from the Install Template
        ///</summary>
        ///<remarks>
        ///</remarks>
        ///<param name = "xmlTemplate">The install Templae</param>
        ///<param name = "writeFeedback">a flag to determine whether to output feedback</param>
        ///<history>
        ///  [cnurse]	02/16/2007	created
        ///</history>
        ///-----------------------------------------------------------------------------
        public static void InitialiseHostSettings(XmlDocument xmlTemplate, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "InitialiseHostSettings");
            XmlNode node = xmlTemplate.SelectSingleNode("//dotnetnuke/settings");
            if ((node != null))
            {
                if (writeFeedback)
                {
                    HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Loading Host Settings:<br>");
                }

                //Need to clear the cache to pick up new HostSettings from the SQLDataProvider script
                DataCache.RemoveCache(DataCache.HostSettingsCacheKey);

                //Parse the Settings nodes
                foreach (XmlNode settingNode in node.ChildNodes)
                {
                    string settingName = settingNode.Name;
                    string settingValue = settingNode.InnerText;
                    if (settingNode.Attributes != null)
                    {
                        XmlAttribute secureAttrib = settingNode.Attributes["Secure"];
                        bool settingIsSecure = false;
                        if ((secureAttrib != null))
                        {
                            if (secureAttrib.Value.ToLower() == "true")
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

                                    //Remove any folders
                                    settingValue = settingValue.Substring(0, settingValue.IndexOf("/"));

                                    //Remove port number
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

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  InstallDatabase runs all the "scripts" identifed in the Install Template to
        ///  install the base version
        ///</summary>
        ///<remarks>
        ///</remarks>
        ///<param name="providerPath"></param>
        ///<param name = "xmlDoc">The Xml Document to load</param>
        ///<param name = "writeFeedback">A flag that determines whether to output feedback to the Response Stream</param>
        ///<param name="version"></param>
        ///<returns>A string which contains the error message - if appropriate</returns>
        ///<history>
        ///  [cnurse]	02/13/2007	created
        ///</history>
        ///-----------------------------------------------------------------------------
        public static string InstallDatabase(Version version, string providerPath, XmlDocument xmlDoc, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "InstallDatabase:" + Globals.FormatVersion(version));
            string defaultProvider = Config.GetDefaultProvider("data").Name;
            string message = Null.NullString;

            //Output feedback line
            if (writeFeedback)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Installing Version: " + Globals.FormatVersion(version) + "<br>");
            }

            //Parse the script nodes
            XmlNode node = xmlDoc.SelectSingleNode("//dotnetnuke/scripts");
            if ((node != null))
            {
                // Loop through the available scripts
                message = (from XmlNode scriptNode in node.SelectNodes("script") select scriptNode.InnerText + "." + defaultProvider).Aggregate(message, (current, script) => current + ExecuteScript(providerPath + script, writeFeedback));
            }

            // update the version
            Globals.UpdateDataBaseVersion(version);

            //Optionally Install the memberRoleProvider
            message += InstallMemberRoleProvider(providerPath, writeFeedback);

            return message;
        }

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  InstallDNN manages the Installation of a new DotNetNuke Application
        ///</summary>
        ///<remarks>
        ///</remarks>
        ///<param name = "strProviderPath">The path to the Data Provider</param>
        ///<history>
        ///  [cnurse]	11/06/2004	created
        ///</history>
        ///-----------------------------------------------------------------------------
        public static void InstallDNN(string strProviderPath)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "InstallDNN:" + strProviderPath);
            var xmlDoc = new XmlDocument();

            // open the Install Template XML file
            string errorMessage = GetInstallTemplate(xmlDoc);

            if (string.IsNullOrEmpty(errorMessage))
            {
                //get base version
                Version baseVersion = GetInstallVersion(xmlDoc);

                //Install Base Version
                InstallDatabase(baseVersion, strProviderPath, xmlDoc, true);

                //Call Upgrade with the current DB Version to carry out any incremental upgrades
                UpgradeDNN(strProviderPath, baseVersion);

                // parse Host Settings if available
                InitialiseHostSettings(xmlDoc, true);

                // parse SuperUser if Available
                UserInfo superUser = GetSuperUser(xmlDoc, true);
                UserController.CreateUser(ref superUser);

                // parse File List if available
                InstallFiles(xmlDoc, true);

                //Run any addition scripts in the Scripts folder
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Executing Additional Scripts:<br>");
                ExecuteScripts(strProviderPath);

                //Install optional resources if present
                var packages = GetInstallPackages();
                foreach (var package in packages)
                {
                    InstallPackage(package.Key, package.Value.PackageType, true);
                }

                //Set Status to None
                Globals.SetStatus(Globals.UpgradeStatus.None);

                //download LP (and templates) if not using en-us
                IInstallationStep ensureLpAndTemplate = new UpdateLanguagePackStep();
                ensureLpAndTemplate.Execute();

                //install LP that contains templates if installing in a different language   
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
                        if ((node != null))
                        {
                            int portalId = AddPortal(node, true, 2);
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
                //500 Error - Redirect to ErrorPage
                if ((HttpContext.Current != null))
                {
                    string url = "~/ErrorPage.aspx?status=500&error=" + errorMessage;
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Server.Transfer(url);
                }
            }
        }

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  InstallFiles intsalls any files listed in the Host Install Configuration file
        ///</summary>
        ///<remarks>
        ///</remarks>
        ///<param name = "xmlDoc">The Xml Document to load</param>
        ///<param name = "writeFeedback">A flag that determines whether to output feedback to the Response Stream</param>
        ///<history>
        ///  [cnurse]	02/19/2007	created
        ///</history>
        ///-----------------------------------------------------------------------------
        public static void InstallFiles(XmlDocument xmlDoc, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "InstallFiles");
            //Parse the file nodes
            XmlNode node = xmlDoc.SelectSingleNode("//dotnetnuke/files");
            if ((node != null))
            {
                if (writeFeedback)
                {
                    HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Loading Host Files:<br>");
                }
                ParseFiles(node, Null.NullInteger);
            }

            //Synchronise Host Folder
            if (writeFeedback)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Synchronizing Host Files:<br>");
            }

            FolderManager.Instance.Synchronize(Null.NullInteger, "", true, true);
        }

        public static bool InstallPackage(string file, string packageType, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "InstallPackage:" + file);
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

            var installer = new Installer.Installer(new FileStream(file, FileMode.Open, FileAccess.Read), Globals.ApplicationMapPath, true, deleteTempFolder);

            //Check if manifest is valid
            if (installer.IsValid)
            {
                installer.InstallerInfo.RepairInstall = true;
                success = installer.Install();
            }
            else
            {
                if (installer.InstallerInfo.ManifestFile == null)
                {
                    //Missing manifest
                    if (packageType == "Skin" || packageType == "Container")
                    {
                        //Legacy Skin/Container
                        string tempInstallFolder = installer.TempInstallFolder;
                        string manifestFile = Path.Combine(tempInstallFolder, Path.GetFileNameWithoutExtension(file) + ".dnn");
                        var manifestWriter = new StreamWriter(manifestFile);
                        manifestWriter.Write(LegacyUtil.CreateSkinManifest(file, packageType, tempInstallFolder));
                        manifestWriter.Close();

                        installer = new Installer.Installer(tempInstallFolder, manifestFile, HttpContext.Current.Request.MapPath("."), true);

                        //Set the Repair flag to true for Batch Install
                        installer.InstallerInfo.RepairInstall = true;

                        success = installer.Install();
                    }
                    else if (Globals.Status != Globals.UpgradeStatus.None)
                    {
                        var message = string.Format(Localization.Localization.GetString("InstallPackageError", Localization.Localization.ExceptionsResourceFile), file, "Manifest file missing");
                        DnnInstallLogger.InstallLogError(message);
                    }
                }
                else
                {
                    //log the failure log when installer is invalid and not caught by mainfest file missing.
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
        /// Gets a ist of installable extensions sorted to ensure dependencies are installed first
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, PackageInfo> GetInstallPackages()
        {
            var packageTypes = new string[] { "Module", "Skin", "Container", "JavaScriptLibrary", "Language", "Provider", "AuthSystem", "Package" };
            var invalidPackages = new List<string>();

            var packages = new Dictionary<string, PackageInfo>();

            foreach (string packageType in packageTypes)
            {
                var installPackagePath = Globals.ApplicationMapPath + "\\Install\\" + packageType;
                if (Directory.Exists(installPackagePath))
                {
                    var files = Directory.GetFiles(installPackagePath);
                    if (files.Length > 0)
                    {
                        foreach (string file in files)
                        {
                            if (Path.GetExtension(file.ToLower()) == ".zip")
                            {
                                PackageController.ParsePackage(file, installPackagePath, packages, invalidPackages);
                                //HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, "Parsing - " + file.Replace(installPackagePath + @"\", "") + "<br/>");
                            }
                        }
                    }
                }
            }

            //Add packages with no dependency requirements
            var sortedPackages = packages.Where(p => p.Value.Dependencies.Count == 0).ToDictionary(p => p.Key, p => p.Value);

            int prevDependentCount = -1;

            var dependentPackages = packages.Where(p => p.Value.Dependencies.Count > 0).ToDictionary(p=> p.Key, p => p.Value);
            int dependentCount = dependentPackages.Count;
            //HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, "Start - Parsing Dependencies<br/>");
            while (dependentCount != prevDependentCount)
            {
                prevDependentCount = dependentCount;
                var addedPackages = new List<string>();
                foreach (var package in dependentPackages)
                {
                    //HtmlUtils.WriteFeedback(HttpContext.Current.Response, 4, "Parsing - " + package.Value.Name + "<br/>");
                    foreach (var dependency in package.Value.Dependencies)
                    {
                        if (sortedPackages.Count(p => p.Value.Name == dependency.PackageName && p.Value.Version >= dependency.Version) > 0)
                        {
                            //HtmlUtils.WriteFeedback(HttpContext.Current.Response, 4, "Dependency Resolved - " + package.Value.Name + "<br/>");
                            sortedPackages.Add(package.Key, package.Value);
                            addedPackages.Add(package.Key);
                        }
                    }
                }
                foreach (var packageKey in addedPackages)
                {
                    dependentPackages.Remove(packageKey);
                }
                dependentCount = dependentPackages.Count;
            }

            //Add any packages whose dependency cannot be resolved
            foreach (var package in dependentPackages)
            {
                sortedPackages.Add(package.Key, package.Value);
            }

            //HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, "End - Parsing Dependencies<br/>");

            //foreach (var package in sortedPackages)
            //{
            //    HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, "Installing - " + package.Key + "<br/>");
            //}
            return sortedPackages;
        }

        public static void InstallPackages(string packageType, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "InstallPackages:" + packageType);
            if (writeFeedback)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Installing Optional " + packageType + "s:<br>");
            }
            string installPackagePath = Globals.ApplicationMapPath + "\\Install\\" + packageType;
            if (Directory.Exists(installPackagePath))
            {
                foreach (string file in Directory.GetFiles(installPackagePath))
                {

                    if (Path.GetExtension(file.ToLower()) == ".zip" /*|| installLanguage */)
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
                    //Try and instantiate a 3.5 Class
                    if (Reflection.CreateType("System.Data.Linq.DataContext", true) != null)
                    {
                        isCurrent = true;
                    }
                    break;
                case "4.0":
                    //Look for requestValidationMode attribute
                    XmlDocument configFile = Config.Load();
                    XPathNavigator configNavigator = configFile.CreateNavigator().SelectSingleNode("//configuration/system.web/httpRuntime|//configuration/location/system.web/httpRuntime");
                    if (configNavigator != null && !string.IsNullOrEmpty(configNavigator.GetAttribute("requestValidationMode", "")))
                    {
                        isCurrent = true;
                    }
                    break;
            }
            return isCurrent;
        }

        public static void RemoveAdminPages(string tabPath)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "RemoveAdminPages:" + tabPath);
            var portalController = new PortalController();
            var tabController = new TabController();

            ArrayList portals = portalController.GetPortals();
            foreach (PortalInfo portal in portals)
            {
                int tabID = TabController.GetTabByTabPath(portal.PortalID, tabPath, Null.NullString);
                if ((tabID != Null.NullInteger))
                {
                    tabController.DeleteTab(tabID, portal.PortalID);
                }
            }
        }

        public static void RemoveHostPage(string pageName)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "RemoveHostPage:" + pageName);
            var controller = new TabController();
            TabInfo skinsTab = controller.GetTabByName(pageName, Null.NullInteger);
            if (skinsTab != null)
            {
                controller.DeleteTab(skinsTab.TabID, Null.NullInteger);
            }
        }

        public static void StartTimer()
        {
            //Start Upgrade Timer

            _startTime = DateTime.Now;
        }

        public static void TryUpgradeNETFramework()
        {
            var eventLogController = new EventLogController();
            switch (Globals.NETFrameworkVersion.ToString(2))
            {
                case "3.5":
                    if (!IsNETFrameworkCurrent("3.5"))
                    {
                        //Upgrade to .NET 3.5
                        string upgradeFile = string.Format("{0}\\Config\\Net35.config", Globals.InstallMapPath);
                        string message = UpdateConfig(upgradeFile, DotNetNukeContext.Current.Application.Version, ".NET 3.5 Upgrade");
                        if (string.IsNullOrEmpty(message))
                        {
                            //Remove old AJAX file
                            FileSystemUtils.DeleteFile(Path.Combine(Globals.ApplicationMapPath, "bin\\System.Web.Extensions.dll"));

                            //Log Upgrade

                            eventLogController.AddLog("UpgradeNet", "Upgraded Site to .NET 3.5", PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, EventLogController.EventLogType.HOST_ALERT);
                        }
                        else
                        {
                            //Log Failed Upgrade
                            eventLogController.AddLog("UpgradeNet", string.Format("Upgrade to .NET 3.5 failed. Error reported during attempt to update:{0}", message), PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, EventLogController.EventLogType.HOST_ALERT);
                        }
                    }
                    break;
                case "4.0":
                    if (!IsNETFrameworkCurrent("4.0"))
                    {
                        //Upgrade to .NET 4.0
                        string upgradeFile = string.Format("{0}\\Config\\Net40.config", Globals.InstallMapPath);
                        string strMessage = UpdateConfig(upgradeFile, DotNetNukeContext.Current.Application.Version, ".NET 4.0 Upgrade");
                        eventLogController.AddLog("UpgradeNet",
                                                  string.IsNullOrEmpty(strMessage)
                                                      ? "Upgraded Site to .NET 4.0"
                                                      : string.Format("Upgrade to .NET 4.0 failed. Error reported during attempt to update:{0}", strMessage),
                                                  PortalController.GetCurrentPortalSettings(),
                                                  UserController.GetCurrentUserInfo().UserID,
                                                  EventLogController.EventLogType.HOST_ALERT);
                    }
                    break;
            }
        }

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  UpgradeApplication - This overload is used for general application upgrade operations.
        ///</summary>
        ///<remarks>
        ///  Since it is not version specific and is invoked whenever the application is
        ///  restarted, the operations must be re-executable.
        ///</remarks>
        ///<history>
        ///  [cnurse]	11/6/2004	documented
        ///  [cnurse] 02/27/2007 made public so it can be called from Wizard
        ///</history>
        ///-----------------------------------------------------------------------------
        public static void UpgradeApplication()
        {
            try
            {
                //Remove UpdatePanel from Login Control - not neccessary in popup.
                var loginControl = ModuleControlController.GetModuleControlByControlKey("Login", -1);
                loginControl.SupportsPartialRendering = false;

                ModuleControlController.SaveModuleControl(loginControl, true);

                //Upgrade to .NET 3.5/4.0
                TryUpgradeNETFramework();

                //Update the version of the client resources - so the cache is cleared
                DataCache.ClearHostCache(false);
                HostController.Instance.IncrementCrmVersion(true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                var objEventLog = new EventLogController();
                var objEventLogInfo = new LogInfo();
                objEventLogInfo.AddProperty("Upgraded DotNetNuke", "General");
                objEventLogInfo.AddProperty("Warnings", "Error: " + ex.Message + Environment.NewLine);
                objEventLogInfo.LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString();
                objEventLogInfo.BypassBuffering = true;
                objEventLog.AddLog(objEventLogInfo);
                try
                {
                    Exceptions.Exceptions.LogException(ex);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }

            }

            //Remove any .txt and .config files that may exist in the Install folder
            foreach (string file in Directory.GetFiles(Globals.InstallMapPath + "Cleanup\\", "??.??.??.txt"))
            {
                FileSystemUtils.DeleteFile(file);
            }
            foreach (string file in Directory.GetFiles(Globals.InstallMapPath + "Config\\", "??.??.??.config"))
            {
                FileSystemUtils.DeleteFile(file);
            }
        }

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  UpgradeApplication - This overload is used for version specific application upgrade operations.
        ///</summary>
        ///<remarks>
        ///  This should be used for file system modifications or upgrade operations which
        ///  should only happen once. Database references are not recommended because future
        ///  versions of the application may result in code incompatibilties.
        ///</remarks>
        ///<history>
        ///  [cnurse]	11/6/2004	documented
        ///</history>
        ///-----------------------------------------------------------------------------
        public static string UpgradeApplication(string providerPath, Version version, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + Localization.Localization.GetString("ApplicationUpgrades", Localization.Localization.GlobalResourceFile) + ": " + version.ToString(3));
            string exceptions = "";
            if (writeFeedback)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, Localization.Localization.GetString("ApplicationUpgrades", Localization.Localization.GlobalResourceFile) + " : " + Globals.FormatVersion(version));
            }
            try
            {
                switch (version.ToString(3))
                {
                    case "3.2.3":
                        UpgradeToVersion323();
                        break;
                    case "4.4.0":
                        UpgradeToVersion440();
                        break;
                    case "4.7.0":
                        UpgradeToVersion470();
                        break;
                    case "4.8.2":
                        UpgradeToVersion482();
                        break;
                    case "5.0.0":
                        UpgradeToVersion500();
                        break;
                    case "5.0.1":
                        UpgradeToVersion501();
                        break;
                    case "5.1.0":
                        UpgradeToVersion510();
                        break;
                    case "5.1.1":
                        UpgradeToVersion511();
                        break;
                    case "5.1.3":
                        UpgradeToVersion513();
                        break;
                    case "5.2.0":
                        UpgradeToVersion520();
                        break;
                    case "5.2.1":
                        UpgradeToVersion521();
                        break;
                    case "5.3.0":
                        UpgradeToVersion530();
                        break;
                    case "5.4.0":
                        UpgradeToVersion540();
                        break;
                    case "5.4.3":
                        UpgradeToVersion543();
                        break;
                    case "5.5.0":
                        UpgradeToVersion550();
                        break;
                    case "5.6.0":
                        UpgradeToVersion560();
                        break;
                    case "5.6.2":
                        UpgradeToVersion562();
                        break;
                    case "6.0.0":
                        UpgradeToVersion600();
                        break;
                    case "6.0.1":
                        UpgradeToVersion601();
                        break;
                    case "6.0.2":
                        UpgradeToVersion602();
                        break;
                    case "6.1.0":
                        UpgradeToVersion610();
                        break;
                    case "6.1.2":
                        UpgradeToVersion612();
                        break;
                    case "6.1.3":
                        UpgradeToVersion613();
                        break;
                    case "6.2.0":
                        UpgradeToVersion620();
                        break;
                    case "6.2.1":
                        UpgradeToVersion621();
                        break;
                    case "6.2.3":
                        UpgradeToVersion623();
                        break;
                    case "6.2.4":
                        UpgradeToVersion624();
                        break;
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
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                exceptions += string.Format("Error: {0}{1}", ex.Message + ex.StackTrace, Environment.NewLine);
                // log the results
                if (string.IsNullOrEmpty(exceptions))
                {
                    DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogEnd", Localization.Localization.GlobalResourceFile) + Localization.Localization.GetString("ApplicationUpgrades", Localization.Localization.GlobalResourceFile) + ": " + version.ToString(3));
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
                HtmlUtils.WriteSuccessError(HttpContext.Current.Response, (string.IsNullOrEmpty(exceptions)));
            }

            return exceptions;
        }

        public static string UpdateConfig(string providerPath, Version version, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "UpdateConfig:" + Globals.FormatVersion(version));
            if (writeFeedback)
            {
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, string.Format("Updating Config Files: {0}", Globals.FormatVersion(version)));
            }
            string strExceptions = UpdateConfig(providerPath, Globals.InstallMapPath + "Config\\" + GetStringVersion(version) + ".config", version, "Core Upgrade");
            if (string.IsNullOrEmpty(strExceptions))
            {
                DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogEnd", Localization.Localization.GlobalResourceFile) + "UpdateConfig:" + Globals.FormatVersion(version));
            }
            else
            {
                DnnInstallLogger.InstallLogError(strExceptions);
            }

            if (writeFeedback)
            {
                HtmlUtils.WriteSuccessError(HttpContext.Current.Response, (string.IsNullOrEmpty(strExceptions)));
            }

            return strExceptions;
        }

        public static string UpdateConfig(string configFile, Version version, string reason)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "UpdateConfig:" + version.ToString(3));
            string exceptions = "";
            if (File.Exists(configFile))
            {
                //Create XmlMerge instance from config file source
                StreamReader stream = File.OpenText(configFile);
                try
                {
                    var merge = new XmlMerge(stream, version.ToString(3), reason);

                    //Process merge
                    merge.UpdateConfigs();
                }
                catch (Exception ex)
                {
                    exceptions += String.Format("Error: {0}{1}", ex.Message + ex.StackTrace, Environment.NewLine);
                    Exceptions.Exceptions.LogException(ex);
                }
                finally
                {
                    //Close stream
                    stream.Close();
                }
            }
            if (string.IsNullOrEmpty(exceptions))
            {
                DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogEnd", Localization.Localization.GlobalResourceFile) + "UpdateConfig:" + version.ToString(3));
            }
            else
            {
                DnnInstallLogger.InstallLogError(exceptions);
            }
            return exceptions;
        }

        public static string UpdateConfig(string providerPath, string configFile, Version version, string reason)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "UpdateConfig:" + version.ToString(3));
            string exceptions = "";
            if (File.Exists(configFile))
            {
                //Create XmlMerge instance from config file source
                StreamReader stream = File.OpenText(configFile);
                try
                {
                    var merge = new XmlMerge(stream, version.ToString(3), reason);

                    //Process merge
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
                    //Close stream
                    stream.Close();
                }
            }
            if (string.IsNullOrEmpty(exceptions))
            {
                DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogEnd", Localization.Localization.GlobalResourceFile) + "UpdateConfig:" + version.ToString(3));
            }
            else
            {
                DnnInstallLogger.InstallLogError(exceptions);
            }
            return exceptions;
        }


        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  UpgradeDNN manages the Upgrade of an exisiting DotNetNuke Application
        ///</summary>
        ///<remarks>
        ///</remarks>
        ///<param name = "providerPath">The path to the Data Provider</param>
        ///<param name = "dataBaseVersion">The current Database Version</param>
        ///<history>
        ///  [cnurse]	11/06/2004	created (Upgrade code extracted from AutoUpgrade)
        ///  [cnurse] 11/10/2004 version specific upgrades extracted to ExecuteScript
        ///  [cnurse] 01/20/2005 changed to Public so Upgrade can be manually controlled
        ///</history>
        ///-----------------------------------------------------------------------------
        public static void UpgradeDNN(string providerPath, Version dataBaseVersion)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "UpgradeDNN:" + Globals.FormatVersion(DotNetNukeContext.Current.Application.Version));
            HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Upgrading to Version: " + Globals.FormatVersion(DotNetNukeContext.Current.Application.Version) + "<br/>");

            //Process the Upgrade Script files
            var versions = new List<Version>();
            foreach (string scriptFile in GetUpgradeScripts(providerPath, dataBaseVersion))
            {
                versions.Add(new Version(GetFileNameWithoutExtension(scriptFile)));
                UpgradeVersion(scriptFile, true);
            }

            foreach (Version ver in versions)
            {
                //' perform version specific application upgrades
                UpgradeApplication(providerPath, ver, true);
            }

            foreach (Version ver in versions)
            {
                // delete files which are no longer used
                DeleteFiles(providerPath, ver, true);
            }
            foreach (Version ver in versions)
            {
                //execute config file updates
                UpdateConfig(providerPath, ver, true);
            }

            // perform general application upgrades
            HtmlUtils.WriteFeedback(HttpContext.Current.Response, 0, "Performing General Upgrades<br>");
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("GeneralUpgrades", Localization.Localization.GlobalResourceFile));
            UpgradeApplication();

            DataCache.ClearHostCache(true);
        }

        internal static string GetFileNameWithoutExtension(string scriptFile)
        {
            return Path.GetFileNameWithoutExtension(scriptFile);
        }

        public static string UpgradeIndicator(Version version, bool isLocal, bool isSecureConnection)
        {
            return UpgradeIndicator(version, DotNetNukeContext.Current.Application.Type, DotNetNukeContext.Current.Application.Name, "", isLocal, isSecureConnection);
        }

        public static string UpgradeIndicator(Version version, string packageType, string packageName, string culture, bool isLocal, bool isSecureConnection)
        {
            string url = "";
            if (Host.CheckUpgrade && version != new Version(0, 0, 0))
            {
                url = DotNetNukeContext.Current.Application.UpgradeUrl + "/update.aspx";
                if (UrlUtils.IsSecureConnectionOrSslOffload(HttpContext.Current.Request))
                {
                    url = url.Replace("http://", "https://");
                }
                url += "?core=" + Globals.FormatVersion(DotNetNukeContext.Current.Application.Version, "00", 3, "");
                url += "&version=" + Globals.FormatVersion(version, "00", 3, "");
                url += "&type=" + packageType;
                url += "&name=" + packageName;
                if (packageType.ToLowerInvariant() == "module")
                {
                    var moduleType = (from m in ModulesController.GetInstalledModules() where m.ModuleName == packageName select m).SingleOrDefault();
                    if (moduleType != null)
                    {
                        url += "&no=" + moduleType.Instances;
                    }
                }
                url += "&id=" + Host.GUID;
                if (packageType.ToUpper() == DotNetNukeContext.Current.Application.Type.ToUpper())
                {
                    if (!String.IsNullOrEmpty(HostController.Instance.GetString("NewsletterSubscribeEmail")))
                    {
                        url += "&email=" + HttpUtility.UrlEncode(HostController.Instance.GetString("NewsletterSubscribeEmail"));
                    }

                    var portals = new PortalController().GetPortals();
                    url += "&no=" + portals.Count;
                    url += "&os=" + Globals.FormatVersion(Globals.OperatingSystemVersion, "00", 2, "");
                    url += "&net=" + Globals.FormatVersion(Globals.NETFrameworkVersion, "00", 2, "");
                    url += "&db=" + Globals.FormatVersion(Globals.DatabaseEngineVersion, "00", 2, "");
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
            return UpgradeRedirect(DotNetNukeContext.Current.Application.Version, DotNetNukeContext.Current.Application.Type, DotNetNukeContext.Current.Application.Name, "");
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
                url += "?core=" + Globals.FormatVersion(DotNetNukeContext.Current.Application.Version, "00", 3, "");
                url += "&version=" + Globals.FormatVersion(version, "00", 3, "");
                url += "&type=" + packageType;
                url += "&name=" + packageName;
                if (!string.IsNullOrEmpty(culture))
                {
                    url += "&culture=" + culture;
                }
            }
            return url;
        }

        ///-----------------------------------------------------------------------------
        ///<summary>
        ///  UpgradeVersion upgrades a single version
        ///</summary>
        ///<remarks>
        ///</remarks>
        ///<param name = "scriptFile">The upgrade script file</param>
        ///<param name="writeFeedback">Write status to Response Stream?</param>
        ///<history>
        ///  [cnurse]	02/14/2007	created
        ///</history>
        ///-----------------------------------------------------------------------------
        public static string UpgradeVersion(string scriptFile, bool writeFeedback)
        {
            DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "UpgradeVersion:" + scriptFile);
            var version = new Version(GetFileNameWithoutExtension(scriptFile));
            string exceptions = Null.NullString;

            // verify script has not already been run
            if (!Globals.FindDatabaseVersion(version.Major, version.Minor, version.Build))
            {
                // execute script file (and version upgrades) for version
                exceptions = ExecuteScript(scriptFile, writeFeedback);

                // update the version
                Globals.UpdateDataBaseVersion(version);

                var eventLogController = new EventLogController();
                var eventLogInfo = new LogInfo();
                eventLogInfo.AddProperty("Upgraded DotNetNuke", "Version: " + Globals.FormatVersion(version));
                if (exceptions.Length > 0)
                {
                    eventLogInfo.AddProperty("Warnings", exceptions);
                }
                else
                {
                    eventLogInfo.AddProperty("No Warnings", "");
                }
                eventLogInfo.LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString();
                eventLogInfo.BypassBuffering = true;
                eventLogController.AddLog(eventLogInfo);
            }
            if (string.IsNullOrEmpty(exceptions))
            {
                DnnInstallLogger.InstallLogInfo(Localization.Localization.GetString("LogStart", Localization.Localization.GlobalResourceFile) + "UpgradeVersion:" + scriptFile);
            }
            else
            {
                DnnInstallLogger.InstallLogError(exceptions);
            }
            return exceptions;
        }

        protected static bool IsLanguageEnabled(int portalid, string code)
        {
            Locale enabledLanguage;
            return LocaleController.Instance.GetLocales(portalid).TryGetValue(code, out enabledLanguage);
        }

        public static string ActivateLicense()
        {
            var isLicensable = (File.Exists(HttpContext.Current.Server.MapPath("~\\bin\\DotNetNuke.Professional.dll")) || File.Exists(HttpContext.Current.Server.MapPath("~\\bin\\DotNetNuke.Enterprise.dll")));
            var activationResult = "";

            if (isLicensable)
            {
                var sku = File.Exists(HttpContext.Current.Server.MapPath("~\\bin\\DotNetNuke.Enterprise.dll")) ? "DNNENT" : "DNNPRO";
                HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, Localization.Localization.GetString("ActivatingLicense", Localization.Localization.GlobalResourceFile));

                var installConfig = InstallController.Instance.GetInstallConfig();
                var licenseConfig = (installConfig != null) ? installConfig.License : null;

                if (licenseConfig != null)
                {
                    dynamic licenseActivation = Reflection.CreateObject(Reflection.CreateType("DotNetNuke.Professional.LicenseActivation.ViewLicx"));
                    licenseActivation.AutoActivation(licenseConfig.AccountEmail, licenseConfig.InvoiceNumber, licenseConfig.WebServer, licenseConfig.LicenseType, sku);
                    activationResult = licenseActivation.LicenseResult;

                    //Log Event to Event Log
                    var objEventLog = new EventLogController();
                    objEventLog.AddLog("License Activation",
                                       "License Activated during install for: " + licenseConfig.AccountEmail + " | invoice: " + licenseConfig.InvoiceNumber,
                                       EventLogController.EventLogType.HOST_ALERT);
                }
            }

            return activationResult;
        }

        #endregion
    }
}