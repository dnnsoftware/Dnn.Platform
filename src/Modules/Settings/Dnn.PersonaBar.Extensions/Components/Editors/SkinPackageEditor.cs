using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Extensions.Components.Dto;
using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
using Dnn.PersonaBar.Library.Helper;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.UI;
using DotNetNuke.UI.Skins;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    public class SkinPackageEditor : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SkinPackageEditor));

        #region IPackageEditor Implementation

        public PackageDetailDto GetPackageDetail(int portalId, PackageInfo package)
        {
            var skin = SkinController.GetSkinByPackageID(package.PackageID);
            var detail = new CommonPackageDetailDto(portalId, package);
            var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;

            detail.Settings.Add("readonly", !isHostUser);
            detail.Settings.Add("name", skin.SkinName);

            return detail;
        }

        public bool SavePackageSettings(PackageSettingsDto packageSettings, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;

                if(isHostUser && packageSettings.EditorActions.ContainsKey("name")
                    && !string.IsNullOrEmpty(packageSettings.EditorActions["name"]))
                {
                    var skin = SkinController.GetSkinByPackageID(packageSettings.PackageId);
                    skin.SkinName = packageSettings.EditorActions["name"];
                    SkinController.UpdateSkinPackage(skin);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                errorMessage = ex.Message;
                return false;
            }
        }

        #endregion

        #region Private Methods

        private string GetSettingUrl(int portalId, int authSystemPackageId)
        {
            var module = ModuleController.Instance.GetModulesByDefinition(portalId, "Extensions")
                .Cast<ModuleInfo>().FirstOrDefault();
            if (module == null)
            {
                return string.Empty;
            }

            var tabId = TabController.Instance.GetTabsByModuleID(module.ModuleID).Keys.FirstOrDefault();
            if (tabId <= 0)
            {
                return string.Empty;
            }
            //ctl/Edit/mid/345/packageid/52
            return Globals.NavigateURL(tabId, PortalSettings.Current, "Edit", 
                                            "mid=" + module.ModuleID, 
                                            "packageid=" + authSystemPackageId,
                                            "popUp=true",
                                            "mode=settings");
        }

        #endregion
    }
}