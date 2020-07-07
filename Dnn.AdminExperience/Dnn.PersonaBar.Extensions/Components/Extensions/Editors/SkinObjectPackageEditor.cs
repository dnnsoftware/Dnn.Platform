// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    using System;

    using Dnn.PersonaBar.Extensions.Components.Dto;
    using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer.Packages;

    public class SkinObjectPackageEditor : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SkinObjectPackageEditor));

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
                string value;
                var skinControl = SkinControlController.GetSkinControlByPackageID(packageSettings.PackageId);

                if (packageSettings.EditorActions.TryGetValue("controlKey", out value)
                    && !string.IsNullOrEmpty(value))
                {
                    skinControl.ControlKey = value;
                }
                if (packageSettings.EditorActions.TryGetValue("controlSrc", out value)
                    && !string.IsNullOrEmpty(value))
                {
                    skinControl.ControlSrc = value;
                }
                if (packageSettings.EditorActions.TryGetValue("supportsPartialRendering", out value)
                    && !string.IsNullOrEmpty(value))
                {
                    bool b;
                    bool.TryParse(value, out b);
                    skinControl.SupportsPartialRendering = b;
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
    }
}
