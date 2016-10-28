using System;
using Dnn.PersonaBar.Extensions.Components.Dto;
using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Installer.Packages;

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    public class SkinObjectPackageEditor : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SkinObjectPackageEditor));

        #region IPackageEditor Implementation

        public PackageInfoDto GetPackageDetail(int portalId, PackageInfo package)
        {
            var skinControl = SkinControlController.GetSkinControlByPackageID(package.PackageID);
            var detail = new SkinObjectPackageDetailDto(portalId, package);
            var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;

            detail.ControlKey = skinControl.ControlKey;
            detail.ControlSrc = skinControl.ControlSrc;
            detail.SupportsPartialRendering = skinControl.SupportsPartialRendering;
            detail.ReadOnly |= !isHostUser;
            
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
    }
}