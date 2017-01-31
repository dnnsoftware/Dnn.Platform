using static DotNetNuke.Common.Globals;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class PortalInfoModel
    {
        public int PortalId;
        public string PortalName;
        public int CdfVersion;
        public string RegistrationMode;
        public string DefaultPortalAlias;
        public int PageCount;
        // command link
        public string __PageCount;
        public int UserCount;
        // command link
        public string __UserCount;
        public string SiteTheme;
        public string AdminTheme;
        public string Container;
        public string AdminContainer;

        public string Language;
        public static PortalInfoModel FromDnnPortalInfo(DotNetNuke.Entities.Portals.PortalSettings portal)
        {
            PortalInfoModel pim = new PortalInfoModel
            {
                PortalId = portal.PortalId,
                PortalName = portal.PortalName,
                CdfVersion = portal.CdfVersion,
                DefaultPortalAlias = portal.DefaultPortalAlias,
                PageCount = portal.Pages,
                __PageCount = "list-pages",
                UserCount = portal.Users,
                __UserCount = "list-users",
                SiteTheme = Utilities.FormatSkinName(portal.DefaultPortalSkin),
                AdminTheme = Utilities.FormatSkinName(portal.DefaultAdminSkin),
                Container = Utilities.FormatContainerName(portal.DefaultPortalContainer),
                AdminContainer = Utilities.FormatContainerName(portal.DefaultAdminContainer),
                Language = portal.DefaultLanguage
            };

            switch (portal.UserRegistration)
            {
                case (int)PortalRegistrationType.NoRegistration:
                    pim.RegistrationMode = "None";
                    break;
                case (int)PortalRegistrationType.PrivateRegistration:
                    pim.RegistrationMode = "Private";
                    break;
                case (int)PortalRegistrationType.PublicRegistration:
                    pim.RegistrationMode = "Public";
                    break;
                case (int)PortalRegistrationType.VerifiedRegistration:
                    pim.RegistrationMode = "Verified";
                    break;
                default:
                    pim.RegistrationMode = "Unknown";
                    break;
            }

            return pim;
        }

    }
}