#region Copyright
// DotNetNuke� - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

#region "Usings"

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

#endregion

namespace DotNetNuke.Modules.PreviewProfileManagement.Components
{
	/// <summary>
	/// Business controller of device profile management.
	/// </summary>
	public class PreviewProfileManagementController : IUpgradeable
    {
        #region IUpgradable Implementation

		/// <summary>
		/// IUpgradable.UpgradeModule.
		/// </summary>
		/// <param name="version">upgrade in version.</param>
		/// <returns></returns>
        public string UpgradeModule(string version)
        {
            switch (version)
            {
                case "06.02.00":
                    RemoveProVersion();
                    break;
            }
			return "Success";
        }

        private void RemoveProVersion()
        {
            //update the tab module to use CE version
            TabInfo newTab;

            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                //Update Site Redirection management page
                var tabId = TabController.GetTabByTabPath(portal.PortalID, "//Admin//DevicePreviewManagement", Null.NullString);
                if (tabId == Null.NullInteger)
                {
                    newTab = Upgrade.AddAdminPage(portal,
                                                 "Device Preview Management",
                                                 "Device Preview Management.",
                                                 "~/desktopmodules/DevicePreviewManagement/images/DevicePreview_Standard_16X16.png",
                                                 "~/desktopmodules/DevicePreviewManagement/images/DevicePreview_Standard_32X32.png",
                                                 true);
                }
                else
                {
                    newTab = TabController.Instance.GetTab(tabId, portal.PortalID, true);
                    newTab.IconFile = "~/desktopmodules/DevicePreviewManagement/images/DevicePreview_Standard_16X16.png";
                    newTab.IconFileLarge = "~/desktopmodules/DevicePreviewManagement/images/DevicePreview_Standard_32X32.png";
                    TabController.Instance.UpdateTab(newTab);
                }

                //Remove Pro edition module
                int moduleID = Null.NullInteger;
                IDictionary<int, ModuleInfo> modules = ModuleController.Instance.GetTabModules(newTab.TabID);

                if (modules != null)
                {
                    foreach (ModuleInfo m in modules.Values)
                    {
                        if (m.DesktopModule.FriendlyName == "Device Preview Management")
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
                ModuleDefinitionInfo mDef = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("DNN Device Preview Management");
                if (mDef != null)
                {
                    Upgrade.AddModuleToPage(newTab, mDef.ModuleDefID, "Device Preview Management", "~/desktopmodules/DevicePreviewManagement/images/DevicePreview_Standard_32X32.png", true);
                }

                //reset default devices created flag
                string defaultPreviewProfiles;
                var settings = PortalController.Instance.GetPortalSettings(portal.PortalID);
                if (settings.TryGetValue("DefPreviewProfiles_Created", out defaultPreviewProfiles) && defaultPreviewProfiles == "DNNCORP.CE")
                {
                    PortalController.DeletePortalSetting(portal.PortalID, "DefPreviewProfiles_Created");
                }
            }

            var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Professional.PreviewProfileManagement");
            if (package != null)
            {
                var installer = new Installer(package, Globals.ApplicationMapPath);
                installer.UnInstall(true);
            }
        }

        #endregion
    }
}