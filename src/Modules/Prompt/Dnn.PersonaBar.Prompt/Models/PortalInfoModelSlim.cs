using static DotNetNuke.Common.Globals;

namespace Dnn.PersonaBar.Prompt.Models
{

    public class PortalInfoModelSlim
    {
        public int PortalId;
        public string PortalName;
        public string RegistrationMode;
        public string DefaultPortalAlias;
        public int PageCount;
        public int UserCount;
        public string Language;

        public static PortalInfoModelSlim FromDnnPortalInfo(DotNetNuke.Entities.Portals.PortalInfo portal)
        {
            PortalInfoModelSlim pim = new PortalInfoModelSlim
            {
                PortalId = portal.PortalID,
                PortalName = portal.PortalName,
                PageCount = portal.Pages,
                UserCount = portal.Users,
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