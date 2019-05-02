#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.IO;
using System.Xml.XPath;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.EventQueue;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Tabs.TabVersions;

#endregion

namespace DotNetNuke.Services.Installer.Installers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ModuleInstaller installs Module Components to a DotNetNuke site
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ModuleInstaller : ComponentInstallerBase
    {
		#region Private Properties

        private DesktopModuleInfo _desktopModule;
        private EventMessage _eventMessage;
        private DesktopModuleInfo _installedDesktopModule;

        #endregion

        #region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List)
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public override string AllowableFiles
        {
            get
            {
				return "cshtml, vbhtml, ashx, aspx, ascx, vb, cs, resx, css, js, resources, config, vbproj, csproj, sln, htm, html, xml, psd, svc, asmx, xsl, xslt";
            }
        }
		
		#endregion

		#region Private Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The DeleteModule method deletes the Module from the data Store.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void DeleteModule()
        {
            try
            {
				//Attempt to get the Desktop Module
                DesktopModuleInfo tempDesktopModule = DesktopModuleController.GetDesktopModuleByPackageID(Package.PackageID);
                if (tempDesktopModule != null)
                {
                    var modules = ModuleController.Instance.GetModulesByDesktopModuleId(tempDesktopModule.DesktopModuleID);
                    //Remove CodeSubDirectory
                    if ((_desktopModule != null) && (!string.IsNullOrEmpty(_desktopModule.CodeSubDirectory)))
                    {
                        Config.RemoveCodeSubDirectory(_desktopModule.CodeSubDirectory);
                    }
                    var controller = new DesktopModuleController();
                    

                    Log.AddInfo(string.Format(Util.MODULE_UnRegistered, tempDesktopModule.ModuleName));
                    //remove admin/host pages
                    if (!String.IsNullOrEmpty(tempDesktopModule.AdminPage))
                    {
                        string tabPath = "//Admin//" + tempDesktopModule.AdminPage;
                        
                        var portals = PortalController.Instance.GetPortals();
                        foreach (PortalInfo portal in portals)
                        {
                            var tabID = TabController.GetTabByTabPath(portal.PortalID, tabPath, Null.NullString);
                            
                            TabInfo temp = TabController.Instance.GetTab(tabID, portal.PortalID);
                            if ((temp != null))
                            {
                               
                                var mods = TabModulesController.Instance.GetTabModules((temp));
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
                                    Log.AddInfo(string.Format(Util.MODULE_AdminPageRemoved, tempDesktopModule.AdminPage, portal.PortalID));
                                    TabController.Instance.DeleteTab(tabID, portal.PortalID);
                                }
                                Log.AddInfo(string.Format(Util.MODULE_AdminPagemoduleRemoved, tempDesktopModule.AdminPage, portal.PortalID));
                            }
                        }
                        
                    }
                    if (!String.IsNullOrEmpty(tempDesktopModule.HostPage))
                    {
                        Upgrade.Upgrade.RemoveHostPage(tempDesktopModule.HostPage);
                        Log.AddInfo(string.Format(Util.MODULE_HostPageRemoved, tempDesktopModule.HostPage));
                        Log.AddInfo(string.Format(Util.MODULE_HostPagemoduleRemoved, tempDesktopModule.HostPage));
                    }

                    controller.DeleteDesktopModule(tempDesktopModule);
                    //Remove all the tab versions related with the module.
                    foreach (var module in modules)
                    {
                        var moduleInfo = module as ModuleInfo;
                        if (moduleInfo != null)
                            TabVersionController.Instance.DeleteTabVersionDetailByModule(moduleInfo.ModuleID);
                    }
                }

            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
            }
        }
		
		#endregion

		#region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Commit method finalises the Install and commits any pending changes.
        /// </summary>
        /// <remarks>In the case of Modules this is not neccessary</remarks>
        /// -----------------------------------------------------------------------------
        public override void Commit()
        {
			//Add CodeSubDirectory
            if (!string.IsNullOrEmpty(_desktopModule.CodeSubDirectory))
            {
                Config.AddCodeSubDirectory(_desktopModule.CodeSubDirectory);
            }
            if (_desktopModule.SupportedFeatures == Null.NullInteger)
            {
                //Set an Event Message so the features are loaded by reflection on restart
                var oAppStartMessage = new EventMessage
                                        {
                                            Priority = MessagePriority.High,
                                            ExpirationDate = DateTime.Now.AddYears(-1),
                                            SentDate = DateTime.Now,
                                            Body = "",
                                            ProcessorType = "DotNetNuke.Entities.Modules.EventMessageProcessor, DotNetNuke",
                                            ProcessorCommand = "UpdateSupportedFeatures"
                                        };

                //Add custom Attributes for this message
                oAppStartMessage.Attributes.Add("BusinessControllerClass", _desktopModule.BusinessControllerClass);
                oAppStartMessage.Attributes.Add("desktopModuleID", _desktopModule.DesktopModuleID.ToString());

                //send it to occur on next App_Start Event
                EventQueueController.SendMessage(oAppStartMessage, "Application_Start_FirstRequest");
            }
			
			//Add Event Message
            if (_eventMessage != null)
            {
                if (!String.IsNullOrEmpty(_eventMessage.Attributes["UpgradeVersionsList"]))
                {
                    _eventMessage.Attributes.Set("desktopModuleID", _desktopModule.DesktopModuleID.ToString());
                    EventQueueController.SendMessage(_eventMessage, "Application_Start");
                }
            }
            
			//Add DesktopModule to all portals
			if (!_desktopModule.IsPremium)
            {
                DesktopModuleController.AddDesktopModuleToPortals(_desktopModule.DesktopModuleID);
            }

            //Add DesktopModule to all portals
            if (!String.IsNullOrEmpty(_desktopModule.AdminPage))
            {
                foreach (PortalInfo portal in PortalController.Instance.GetPortals())
                {

                    bool createdNewPage = false, addedNewModule = false;
                    DesktopModuleController.AddDesktopModulePageToPortal(_desktopModule, _desktopModule.AdminPage, portal.PortalID, ref createdNewPage, ref addedNewModule);

                    if (createdNewPage)
                    {
                        Log.AddInfo(string.Format(Util.MODULE_AdminPageAdded, _desktopModule.AdminPage, portal.PortalID));
                    }

                    if (addedNewModule)
                    {
                        Log.AddInfo(string.Format(Util.MODULE_AdminPagemoduleAdded, _desktopModule.AdminPage,portal.PortalID));
                    }
                }
               
            }

            //Add host items
            if (_desktopModule.Page != null && !String.IsNullOrEmpty(_desktopModule.HostPage))
            {
                bool createdNewPage = false, addedNewModule = false;
                DesktopModuleController.AddDesktopModulePageToPortal(_desktopModule, _desktopModule.HostPage, Null.NullInteger, ref createdNewPage, ref addedNewModule);

                if (createdNewPage)
                {
                    Log.AddInfo(string.Format(Util.MODULE_HostPageAdded, _desktopModule.HostPage));
                }

                if (addedNewModule)
                {
                    Log.AddInfo(string.Format(Util.MODULE_HostPagemoduleAdded, _desktopModule.HostPage));
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Install method installs the Module component
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Install()
        {
            try
            {
				//Attempt to get the Desktop Module
                _installedDesktopModule = DesktopModuleController.GetDesktopModuleByModuleName(_desktopModule.ModuleName, Package.InstallerInfo.PortalID);

                if (_installedDesktopModule != null)
                {
                    _desktopModule.DesktopModuleID = _installedDesktopModule.DesktopModuleID;
					//save the module's category
                	_desktopModule.Category = _installedDesktopModule.Category;
                }
				
                //Clear ModuleControls and Module Definitions caches in case script has modifed the contents
                DataCache.RemoveCache(DataCache.ModuleDefinitionCacheKey);
                DataCache.RemoveCache(DataCache.ModuleControlsCacheKey);

                //Save DesktopModule and child objects to database
                _desktopModule.PackageID = Package.PackageID;
                _desktopModule.DesktopModuleID = DesktopModuleController.SaveDesktopModule(_desktopModule, true, false);

                Completed = true;
                Log.AddInfo(string.Format(Util.MODULE_Registered, _desktopModule.ModuleName));
            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadManifest method reads the manifest file for the Module compoent.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void ReadManifest(XPathNavigator manifestNav)
        {
            //Load the Desktop Module from the manifest
            _desktopModule = CBO.DeserializeObject<DesktopModuleInfo>(new StringReader(manifestNav.InnerXml));

            _desktopModule.FriendlyName = Package.FriendlyName;
            _desktopModule.Description = Package.Description;
            _desktopModule.Version = Globals.FormatVersion(Package.Version, "00", 4, ".");
            _desktopModule.CompatibleVersions = Null.NullString;
            _desktopModule.Dependencies = Null.NullString;
            _desktopModule.Permissions = Null.NullString;
            if (string.IsNullOrEmpty(_desktopModule.BusinessControllerClass))
            {
                _desktopModule.SupportedFeatures = 0;
            }

            _eventMessage = ReadEventMessageNode(manifestNav);
			
            //Load permissions (to add)
            foreach (XPathNavigator moduleDefinitionNav in manifestNav.Select("desktopModule/moduleDefinitions/moduleDefinition"))
            {
                string friendlyName = Util.ReadElement(moduleDefinitionNav, "friendlyName");
                foreach (XPathNavigator permissionNav in moduleDefinitionNav.Select("permissions/permission"))
                {
                    var permission = new PermissionInfo();
                    permission.PermissionCode = Util.ReadAttribute(permissionNav, "code");
                    permission.PermissionKey = Util.ReadAttribute(permissionNav, "key");
                    permission.PermissionName = Util.ReadAttribute(permissionNav, "name");
                    ModuleDefinitionInfo moduleDefinition = _desktopModule.ModuleDefinitions[friendlyName];
                    if (moduleDefinition != null)
                    {
                        moduleDefinition.Permissions.Add(permission.PermissionKey, permission);
                    }
                }
            }
            if (Log.Valid)
            {
                Log.AddInfo(Util.MODULE_ReadSuccess);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Rollback method undoes the installation of the component in the event 
        /// that one of the other components fails
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Rollback()
        {
			//If Temp Module exists then we need to update the DataStore with this 
            if (_installedDesktopModule == null)
            {
				//No Temp Module - Delete newly added module
                DeleteModule();
            }
            else
            {
				//Temp Module - Rollback to Temp
                DesktopModuleController.SaveDesktopModule(_installedDesktopModule, true, false);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstall method uninstalls the Module component
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UnInstall()
        {
            DeleteModule();
        }
		
		#endregion
    }
}
