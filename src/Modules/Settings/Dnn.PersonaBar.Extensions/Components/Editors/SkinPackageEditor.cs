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
            var skin = SkinController.GetSkinByPackageID(package.PackageID);
            var detail = new SkinPackageDetailDto(portalId, package);
            var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;

            detail.ReadOnly = !isHostUser;
            detail.Name = skin.SkinName;

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
    }
}