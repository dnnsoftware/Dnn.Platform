using Dnn.PersonaBar.Extensions.Components.Dto;
using DotNetNuke.Services.Installer.Packages;

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    public interface IPackageEditor
    {
        PackageInfoDto GetPackageDetail(int portalId, PackageInfo package);

        bool SavePackageSettings(PackageSettingsDto packageSettings, out string errorMessage);
    }
}