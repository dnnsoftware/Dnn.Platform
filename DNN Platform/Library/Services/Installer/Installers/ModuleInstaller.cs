// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.EventQueue;

    /// <summary>The ModuleInstaller installs Module Components to a DotNetNuke site.</summary>
    public class ModuleInstaller : ComponentInstallerBase
    {
        private DesktopModuleInfo desktopModule;
        private EventMessage eventMessage;
        private DesktopModuleInfo installedDesktopModule;

        /// <summary>Gets a list of allowable file extensions (in addition to the Host's List).</summary>
        /// <value>A String.</value>
        public override string AllowableFiles
        {
            get
            {
                return "cshtml, vbhtml, ashx, aspx, ascx, vb, cs, resx, css, js, resources, config, vbproj, csproj, sln, htm, html, xml, psd, svc, asmx, xsl, xslt";
            }
        }

        /// <summary>The Commit method finalises the Install and commits any pending changes.</summary>
        /// <remarks>In the case of Modules this is not neccessary.</remarks>
        public override void Commit()
        {
            // Add CodeSubDirectory
            if (!string.IsNullOrEmpty(this.desktopModule.CodeSubDirectory))
            {
                Config.AddCodeSubDirectory(this.desktopModule.CodeSubDirectory);
            }

            if (this.desktopModule.SupportedFeatures == Null.NullInteger)
            {
                // Set an Event Message so the features are loaded by reflection on restart
                var oAppStartMessage = new EventMessage
                {
                    Priority = MessagePriority.High,
                    ExpirationDate = DateTime.Now.AddYears(-1),
                    SentDate = DateTime.Now,
                    Body = string.Empty,
                    ProcessorType = "DotNetNuke.Entities.Modules.EventMessageProcessor, DotNetNuke",
                    ProcessorCommand = "UpdateSupportedFeatures",
                };

                // Add custom Attributes for this message
                oAppStartMessage.Attributes.Add("BusinessControllerClass", this.desktopModule.BusinessControllerClass);
                oAppStartMessage.Attributes.Add("desktopModuleID", this.desktopModule.DesktopModuleID.ToString(CultureInfo.InvariantCulture));

                // send it to occur on next App_Start Event
                EventQueueController.SendMessage(oAppStartMessage, "Application_Start_FirstRequest");
            }

            // Add Event Message
            if (this.eventMessage != null)
            {
                if (!string.IsNullOrEmpty(this.eventMessage.Attributes["UpgradeVersionsList"]))
                {
                    this.eventMessage.Attributes.Set("desktopModuleID", this.desktopModule.DesktopModuleID.ToString(CultureInfo.InvariantCulture));
                    EventQueueController.SendMessage(this.eventMessage, "Application_Start");
                }
            }

            // Add DesktopModule to all portals
            if (!this.desktopModule.IsPremium)
            {
                DesktopModuleController.AddDesktopModuleToPortals(this.desktopModule.DesktopModuleID);
            }

            // Add DesktopModule to all portals
            if (!string.IsNullOrEmpty(this.desktopModule.AdminPage))
            {
                foreach (IPortalInfo portal in PortalController.Instance.GetPortals())
                {
                    bool createdNewPage = false, addedNewModule = false;
                    DesktopModuleController.AddDesktopModulePageToPortal(this.desktopModule, this.desktopModule.AdminPage, portal.PortalId, ref createdNewPage, ref addedNewModule);

                    if (createdNewPage)
                    {
                        this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.MODULE_AdminPageAdded, this.desktopModule.AdminPage, portal.PortalId));
                    }

                    if (addedNewModule)
                    {
                        this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.MODULE_AdminPagemoduleAdded, this.desktopModule.AdminPage, portal.PortalId));
                    }
                }
            }

            // Add host items
            if (this.desktopModule.Page != null && !string.IsNullOrEmpty(this.desktopModule.HostPage))
            {
                bool createdNewPage = false, addedNewModule = false;
                DesktopModuleController.AddDesktopModulePageToPortal(this.desktopModule, this.desktopModule.HostPage, Null.NullInteger, ref createdNewPage, ref addedNewModule);

                if (createdNewPage)
                {
                    this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.MODULE_HostPageAdded, this.desktopModule.HostPage));
                }

                if (addedNewModule)
                {
                    this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.MODULE_HostPagemoduleAdded, this.desktopModule.HostPage));
                }
            }
        }

        /// <summary>The Install method installs the Module component.</summary>
        public override void Install()
        {
            try
            {
                // Attempt to get the Desktop Module
                this.installedDesktopModule = DesktopModuleController.GetDesktopModuleByModuleName(this.desktopModule.ModuleName, this.Package.InstallerInfo.PortalID);

                if (this.installedDesktopModule != null)
                {
                    this.desktopModule.DesktopModuleID = this.installedDesktopModule.DesktopModuleID;

                    // save the module's category
                    this.desktopModule.Category = this.installedDesktopModule.Category;
                }

                // Clear ModuleControls and Module Definitions caches in case script has modified the contents
                DataCache.RemoveCache(DataCache.ModuleDefinitionCacheKey);
                DataCache.RemoveCache(DataCache.ModuleControlsCacheKey);

                // Save DesktopModule and child objects to database
                this.desktopModule.PackageID = this.Package.PackageID;
                this.desktopModule.DesktopModuleID = DesktopModuleController.SaveDesktopModule(this.desktopModule, true, false);

                this.Completed = true;
                this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.MODULE_Registered, this.desktopModule.ModuleName));
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
        }

        /// <summary>The ReadManifest method reads the manifest file for the Module component.</summary>
        /// <param name="manifestNav">The XPath navigator for the Module section of the manifest.</param>
        public override void ReadManifest(XPathNavigator manifestNav)
        {
            // Load the Desktop Module from the manifest
            this.desktopModule = CBO.DeserializeObject<DesktopModuleInfo>(new StringReader(manifestNav.InnerXml));

            // Allow a <component type="Module"> (i.e. a DesktopModule) to have its own friendlyname / description.
            // This allows multiple DesktopModules in one Package, allowing large MVC packages which share one assembly
            // but have many functions.
            if (this.desktopModule.FriendlyName == null || this.desktopModule.FriendlyName.Trim().Length == 0)
            {
                this.desktopModule.FriendlyName = this.Package.FriendlyName;
            }

            if (this.desktopModule.Description == null || this.desktopModule.Description.Trim().Length == 0)
            {
                this.desktopModule.Description = this.Package.Description;
            }

            this.desktopModule.Version = Globals.FormatVersion(this.Package.Version, "00", 4, ".");
            this.desktopModule.CompatibleVersions = Null.NullString;
            this.desktopModule.Dependencies = Null.NullString;
            this.desktopModule.Permissions = Null.NullString;
            if (string.IsNullOrEmpty(this.desktopModule.BusinessControllerClass))
            {
                this.desktopModule.SupportedFeatures = 0;
            }

            this.eventMessage = this.ReadEventMessageNode(manifestNav);

            // Load permissions (to add)
            foreach (XPathNavigator moduleDefinitionNav in manifestNav.Select("desktopModule/moduleDefinitions/moduleDefinition"))
            {
                string friendlyName = Util.ReadElement(moduleDefinitionNav, "friendlyName");
                foreach (XPathNavigator permissionNav in moduleDefinitionNav.Select("permissions/permission"))
                {
                    var permission = new PermissionInfo();
                    permission.PermissionCode = Util.ReadAttribute(permissionNav, "code");
                    permission.PermissionKey = Util.ReadAttribute(permissionNav, "key");
                    permission.PermissionName = Util.ReadAttribute(permissionNav, "name");
                    ModuleDefinitionInfo moduleDefinition = this.desktopModule.ModuleDefinitions[friendlyName];
                    if (moduleDefinition != null)
                    {
                        moduleDefinition.Permissions.Add(permission.PermissionKey, permission);
                    }
                }
            }

            if (this.Log.Valid)
            {
                this.Log.AddInfo(Util.MODULE_ReadSuccess);
            }
        }

        /// <summary>
        /// The Rollback method undoes the installation of the component in the event
        /// that one of the other components fails.
        /// </summary>
        public override void Rollback()
        {
            // If Temp Module exists then we need to update the DataStore with this
            if (this.installedDesktopModule == null)
            {
                // No Temp Module - Delete newly added module
                this.DeleteModule();
            }
            else
            {
                // Temp Module - Rollback to Temp
                DesktopModuleController.SaveDesktopModule(this.installedDesktopModule, true, false);
            }
        }

        /// <summary>The UnInstall method uninstalls the Module component.</summary>
        public override void UnInstall()
        {
            this.DeleteModule();
        }

        /// <summary>The DeleteModule method deletes the Module from the data Store.</summary>
        private void DeleteModule()
        {
            try
            {
                // Attempt to get the Desktop Module
                DesktopModuleInfo tempDesktopModule = DesktopModuleController.GetDesktopModuleByPackageID(this.Package.PackageID);
                if (tempDesktopModule != null)
                {
                    var modules = ModuleController.Instance.GetModulesByDesktopModuleId(tempDesktopModule.DesktopModuleID);

                    // Remove CodeSubDirectory
                    if ((this.desktopModule != null) && (!string.IsNullOrEmpty(this.desktopModule.CodeSubDirectory)))
                    {
                        Config.RemoveCodeSubDirectory(this.desktopModule.CodeSubDirectory);
                    }

                    var controller = new DesktopModuleController();

                    this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.MODULE_UnRegistered, tempDesktopModule.ModuleName));

                    // remove admin/host pages
                    if (!string.IsNullOrEmpty(tempDesktopModule.AdminPage))
                    {
                        foreach (IPortalInfo portal in PortalController.Instance.GetPortals())
                        {
                            var adminTabId = TabController.GetTabByTabPath(portal.PortalId, "//Admin", Null.NullString);
                            if (adminTabId == Null.NullInteger)
                            {
                                continue;
                            }

                            var tabPath = Globals.GenerateTabPath(adminTabId, tempDesktopModule.AdminPage);
                            var moduleAdminTabId = TabController.GetTabByTabPath(portal.PortalId, tabPath, Null.NullString);

                            TabInfo moduleAdminTab = TabController.Instance.GetTab(moduleAdminTabId, portal.PortalId);
                            if (moduleAdminTab != null)
                            {
                                var mods = TabModulesController.Instance.GetTabModules(moduleAdminTab);
                                bool noOtherTabModule = true;
                                foreach (ModuleInfo mod in mods)
                                {
                                    if (mod.DesktopModuleID != tempDesktopModule.DesktopModuleID)
                                    {
                                        noOtherTabModule = false;
                                    }
                                }

                                if (noOtherTabModule)
                                {
                                    this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.MODULE_AdminPageRemoved, tempDesktopModule.AdminPage, portal.PortalId));
                                    TabController.Instance.DeleteTab(moduleAdminTabId, portal.PortalId);
                                }

                                this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.MODULE_AdminPagemoduleRemoved, tempDesktopModule.AdminPage, portal.PortalId));
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(tempDesktopModule.HostPage))
                    {
                        Upgrade.Upgrade.RemoveHostPage(tempDesktopModule.HostPage);
                        this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.MODULE_HostPageRemoved, tempDesktopModule.HostPage));
                        this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.MODULE_HostPagemoduleRemoved, tempDesktopModule.HostPage));
                    }

                    controller.DeleteDesktopModule(tempDesktopModule);

                    // Remove all the tab versions related with the module.
                    foreach (var module in modules)
                    {
                        if (module is ModuleInfo moduleInfo)
                        {
                            TabVersionController.Instance.DeleteTabVersionDetailByModule(moduleInfo.ModuleID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
        }
    }
}
