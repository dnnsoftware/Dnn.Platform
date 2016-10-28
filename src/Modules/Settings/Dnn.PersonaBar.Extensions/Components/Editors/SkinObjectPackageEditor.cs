using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Extensions.Components.Dto;
using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
using DotNetNuke.Common;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Installer.Packages;

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    public class SkinObjectPackageEditor : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SkinObjectPackageEditor));

        #region IPackageEditor Implementation

        public PackageDetailDto GetPackageDetail(int portalId, PackageInfo package)
        {
            var skinControl = SkinControlController.GetSkinControlByPackageID(package.PackageID);
            var detail = new CommonPackageDetailDto(portalId, package);
            var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;

            detail.Settings.Add("controlKey", skinControl.ControlKey);
            detail.Settings.Add("controlSrc", skinControl.ControlSrc);
            detail.Settings.Add("supportsPartialRendering", skinControl.SupportsPartialRendering);
            detail.Settings.Add("readonly", !isHostUser);
            
            return detail;
        }

        public bool SavePackageSettings(PackageSettingsDto packageSettings, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var skinControl = SkinControlController.GetSkinControlByPackageID(packageSettings.PackageId);

                if (packageSettings.EditorActions.ContainsKey("controlKey")
                    && !string.IsNullOrEmpty(packageSettings.EditorActions["controlKey"]))
                {
                    skinControl.ControlKey = packageSettings.EditorActions["controlKey"];
                }
                if (packageSettings.EditorActions.ContainsKey("controlSrc")
                    && !string.IsNullOrEmpty(packageSettings.EditorActions["controlSrc"]))
                {
                    skinControl.ControlSrc = packageSettings.EditorActions["controlSrc"];
                }
                if (packageSettings.EditorActions.ContainsKey("supportsPartialRendering")
                    && !string.IsNullOrEmpty(packageSettings.EditorActions["supportsPartialRendering"]))
                {
                    skinControl.SupportsPartialRendering = bool.Parse(packageSettings.EditorActions["supportsPartialRendering"]);
                }

                SkinControlController.SaveSkinControl(skinControl);

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