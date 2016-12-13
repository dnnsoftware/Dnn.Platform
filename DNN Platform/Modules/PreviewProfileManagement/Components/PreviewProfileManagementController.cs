#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
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