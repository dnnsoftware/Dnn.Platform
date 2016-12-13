#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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