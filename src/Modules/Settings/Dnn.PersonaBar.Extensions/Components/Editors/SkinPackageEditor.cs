using System;
using Dnn.PersonaBar.Extensions.Components.Dto;
using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.UI.Skins;

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    public class SkinPackageEditor : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SkinPackageEditor));

        #region IPackageEditor Implementation

        public PackageInfoDto GetPackageDetail(int portalId, PackageInfo package)
        {
            var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
            var skin = SkinController.GetSkinByPackageID(package.PackageID);
            var detail = new SkinPackageDetailDto(portalId, package)
            {
                ThemePackageName = skin.SkinName,
                ReadOnly = !isHostUser,
            };

            return detail;
        }

        public bool SavePackageSettings(PackageSettingsDto packageSettings, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;

                if (isHostUser)
                {
                    string value;
                    var skin = SkinController.GetSkinByPackageID(packageSettings.PackageId);

                    if (packageSettings.EditorActions.TryGetValue("themePackageName", out value)
                        && !string.IsNullOrEmpty(value))
                    {
                        skin.SkinName = value;
                        SkinController.UpdateSkinPackage(skin);
                    }
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
    }
}