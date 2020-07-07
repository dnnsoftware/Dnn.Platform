// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.IO;
    using System.Xml.XPath;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.EventQueue;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ModuleInstaller installs Module Components to a DotNetNuke site.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ModuleInstaller : ComponentInstallerBase
    {
        private DesktopModuleInfo _desktopModule;
        private EventMessage _eventMessage;
        private DesktopModuleInfo _installedDesktopModule;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List).
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public override string AllowableFiles
        {
            get
            {
                return "cshtml, vbhtml, ashx, aspx, ascx, vb, cs, resx, css, js, resources, config, vbproj, csproj, sln, htm, html, xml, psd, svc, asmx, xsl, xslt";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Commit method finalises the Install and commits any pending changes.
        /// </summary>
        /// <remarks>In the case of Modules this is not neccessary.</remarks>
        /// -----------------------------------------------------------------------------
        public override void Commit()
        {
            // Add CodeSubDirectory
            if (!string.IsNullOrEmpty(this._desktopModule.CodeSubDirectory))
            {
                Config.AddCodeSubDirectory(this._desktopModule.CodeSubDirectory);
            }

            if (this._desktopModule.SupportedFeatures == Null.NullInteger)
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
                oAppStartMessage.Attributes.Add("BusinessControllerClass", this._desktopModule.BusinessControllerClass);
                oAppStartMessage.Attributes.Add("desktopModuleID", this._desktopModule.DesktopModuleID.ToString());

                // send it to occur on next App_Start Event
                EventQueueController.SendMessage(oAppStartMessage, "Application_Start_FirstRequest");
            }

            // Add Event Message
            if (this._eventMessage != null)
            {
                if (!string.IsNullOrEmpty(this._eventMessage.Attributes["UpgradeVersionsList"]))
                {
                    this._eventMessage.Attributes.Set("desktopModuleID", this._desktopModule.DesktopModuleID.ToString());
                    EventQueueController.SendMessage(this._eventMessage, "Application_Start");
                }
            }

            // Add DesktopModule to all portals
            if (!this._desktopModule.IsPremium)
            {
                DesktopModuleController.AddDesktopModuleToPortals(this._desktopModule.DesktopModuleID);
            }

            // Add DesktopModule to all portals
            if (!string.IsNullOrEmpty(this._desktopModule.AdminPage))
            {
                foreach (PortalInfo portal in PortalController.Instance.GetPortals())
                {
                    bool createdNewPage = false, addedNewModule = false;
                    DesktopModuleController.AddDesktopModulePageToPortal(this._desktopModule, this._desktopModule.AdminPage, portal.PortalID, ref createdNewPage, ref addedNewModule);

                    if (createdNewPage)
                    {
                        this.Log.AddInfo(string.Format(Util.MODULE_AdminPageAdded, this._desktopModule.AdminPage, portal.PortalID));
                    }

                    if (addedNewModule)
                    {
                        this.Log.AddInfo(string.Format(Util.MODULE_AdminPagemoduleAdded, this._desktopModule.AdminPage, portal.PortalID));
                    }
                }
            }

            // Add host items
            if (this._desktopModule.Page != null && !string.IsNullOrEmpty(this._desktopModule.HostPage))
            {
                bool createdNewPage = false, addedNewModule = false;
                DesktopModuleController.AddDesktopModulePageToPortal(this._desktopModule, this._desktopModule.HostPage, Null.NullInteger, ref createdNewPage, ref addedNewModule);

                if (createdNewPage)
                {
                    this.Log.AddInfo(string.Format(Util.MODULE_HostPageAdded, this._desktopModule.HostPage));
                }

                if (addedNewModule)
                {
                    this.Log.AddInfo(string.Format(Util.MODULE_HostPagemoduleAdded, this._desktopModule.HostPage));
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Install method installs the Module component.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Install()
        {
            try
            {
                // Attempt to get the Desktop Module
                this._installedDesktopModule = DesktopModuleController.GetDesktopModuleByModuleName(this._desktopModule.ModuleName, this.Package.InstallerInfo.PortalID);

                if (this._installedDesktopModule != null)
                {
                    this._desktopModule.DesktopModuleID = this._installedDesktopModule.DesktopModuleID;

                    // save the module's category
                    this._desktopModule.Category = this._installedDesktopModule.Category;
                }

                // Clear ModuleControls and Module Definitions caches in case script has modifed the contents
                DataCache.RemoveCache(DataCache.ModuleDefinitionCacheKey);
                DataCache.RemoveCache(DataCache.ModuleControlsCacheKey);

                // Save DesktopModule and child objects to database
                this._desktopModule.PackageID = this.Package.PackageID;
                this._desktopModule.DesktopModuleID = DesktopModuleController.SaveDesktopModule(this._desktopModule, true, false);

                this.Completed = true;
                this.Log.AddInfo(string.Format(Util.MODULE_Registered, this._desktopModule.ModuleName));
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadManifest method reads the manifest file for the Module compoent.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void ReadManifest(XPathNavigator manifestNav)
        {
            // Load the Desktop Module from the manifest
            this._desktopModule = CBO.DeserializeObject<DesktopModuleInfo>(new StringReader(manifestNav.InnerXml));

            this._desktopModule.FriendlyName = this.Package.FriendlyName;
            this._desktopModule.Description = this.Package.Description;
            this._desktopModule.Version = Globals.FormatVersion(this.Package.Version, "00", 4, ".");
            this._desktopModule.CompatibleVersions = Null.NullString;
            this._desktopModule.Dependencies = Null.NullString;
            this._desktopModule.Permissions = Null.NullString;
            if (string.IsNullOrEmpty(this._desktopModule.BusinessControllerClass))
            {
                this._desktopModule.SupportedFeatures = 0;
            }

            this._eventMessage = this.ReadEventMessageNode(manifestNav);

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
                    ModuleDefinitionInfo moduleDefinition = this._desktopModule.ModuleDefinitions[friendlyName];
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Rollback method undoes the installation of the component in the event
        /// that one of the other components fails.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Rollback()
        {
            // If Temp Module exists then we need to update the DataStore with this
            if (this._installedDesktopModule == null)
            {
                // No Temp Module - Delete newly added module
                this.DeleteModule();
            }
            else
            {
                // Temp Module - Rollback to Temp
                DesktopModuleController.SaveDesktopModule(this._installedDesktopModule, true, false);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstall method uninstalls the Module component.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UnInstall()
        {
            this.DeleteModule();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The DeleteModule method deletes the Module from the data Store.
        /// </summary>
        /// -----------------------------------------------------------------------------
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
                    if ((this._desktopModule != null) && (!string.IsNullOrEmpty(this._desktopModule.CodeSubDirectory)))
                    {
                        Config.RemoveCodeSubDirectory(this._desktopModule.CodeSubDirectory);
                    }

                    var controller = new DesktopModuleController();

                    this.Log.AddInfo(string.Format(Util.MODULE_UnRegistered, tempDesktopModule.ModuleName));

                    // remove admin/host pages
                    if (!string.IsNullOrEmpty(tempDesktopModule.AdminPage))
                    {
                        string tabPath = "//Admin//" + tempDesktopModule.AdminPage;

                        var portals = PortalController.Instance.GetPortals();
                        foreach (PortalInfo portal in portals)
                        {
                            var tabID = TabController.GetTabByTabPath(portal.PortalID, tabPath, Null.NullString);

                            TabInfo temp = TabController.Instance.GetTab(tabID, portal.PortalID);
                            if (temp != null)
                            {
                                var mods = TabModulesController.Instance.GetTabModules(temp);
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
                                    this.Log.AddInfo(string.Format(Util.MODULE_AdminPageRemoved, tempDesktopModule.AdminPage, portal.PortalID));
                                    TabController.Instance.DeleteTab(tabID, portal.PortalID);
                                }

                                this.Log.AddInfo(string.Format(Util.MODULE_AdminPagemoduleRemoved, tempDesktopModule.AdminPage, portal.PortalID));
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(tempDesktopModule.HostPage))
                    {
                        Upgrade.Upgrade.RemoveHostPage(tempDesktopModule.HostPage);
                        this.Log.AddInfo(string.Format(Util.MODULE_HostPageRemoved, tempDesktopModule.HostPage));
                        this.Log.AddInfo(string.Format(Util.MODULE_HostPagemoduleRemoved, tempDesktopModule.HostPage));
                    }

                    controller.DeleteDesktopModule(tempDesktopModule);

                    // Remove all the tab versions related with the module.
                    foreach (var module in modules)
                    {
                        var moduleInfo = module as ModuleInfo;
                        if (moduleInfo != null)
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
