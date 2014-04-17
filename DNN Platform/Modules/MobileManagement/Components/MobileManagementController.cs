#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// All Rights Reserved
#endregion
using System;
using System.Collections.Generic;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Upgrade;

namespace DotNetNuke.Modules.MobileManagement.Components
{
	public class MobileManagementController : IUpgradeable
    {
        #region IUpgradable Implementation

        public string UpgradeModule(string version)
        {
            switch(version)
            {
                case "06.01.05":
                    RemoveProVersion();
                    break;
            }
			return "Success";
        }

        private void RemoveProVersion()
        {
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                //Update Site Redirection management page
                var tabId = TabController.GetTabByTabPath(portal.PortalID, "//Admin//SiteRedirectionManagement", Null.NullString);
                TabInfo newTab;
                if(tabId == Null.NullInteger)
                {
                    newTab = Upgrade.AddAdminPage(portal,
                                                 "Site Redirection Management",
                                                 "Site Redirection Management.",
                                                 "~/desktopmodules/MobileManagement/images/MobileManagement_Standard_16x16.png",
                                                 "~/desktopmodules/MobileManagement/images/MobileManagement_Standard_32x32.png",
                                                 true);
                }
                else
                {
                    newTab = TabController.Instance.GetTab(tabId, portal.PortalID, true);
                    newTab.IconFile = "~/desktopmodules/MobileManagement/images/MobileManagement_Standard_16x16.png";
                    newTab.IconFileLarge = "~/desktopmodules/MobileManagement/images/MobileManagement_Standard_32x32.png";
                    TabController.Instance.UpdateTab(newTab);
                }

                //Remove Pro edition module
                int moduleID = Null.NullInteger;
                IDictionary<int, ModuleInfo> modules = ModuleController.Instance.GetTabModules(newTab.TabID);

                if (modules != null)
                {
                    foreach (ModuleInfo m in modules.Values)
                    {
                        if (m.DesktopModule.FriendlyName == "Site Redirection Management")
                        {
                            moduleID = m.ModuleID;
                            break;
                        }
                    }
                }

                if (moduleID != Null.NullInteger)
                {
                    ModuleController.Instance.DeleteTabModule(newTab.TabID, moduleID, false);
                }

                //Add community edition module
                ModuleDefinitionInfo mDef = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("DNN Site Redirection Management");
                if (mDef != null)
                {
                    Upgrade.AddModuleToPage(newTab, mDef.ModuleDefID, "Site Redirection Management", "~/desktopmodules/MobileManagement/images/MobileManagement_Standard_32x32.png", true);
                }
            }

            var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Professional.MobileManagement");
            if(package != null)
            {
                var installer = new Installer(package, Globals.ApplicationMapPath);
                installer.UnInstall(true);
            }
        }

        #endregion
    }
}