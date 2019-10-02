using System.IO;
using Dnn.PersonaBar.Extensions.Components.Dto;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Extensions.Components
{
    public interface IInstallController
    {
        InstallResultDto InstallPackage(PortalSettings portalSettings, UserInfo user, string legacySkin, string fileName,
            Stream stream, bool isPortalPackage = false);

        ParseResultDto ParsePackage(PortalSettings portalSettings, UserInfo user, string fileName, Stream stream);
    }
}