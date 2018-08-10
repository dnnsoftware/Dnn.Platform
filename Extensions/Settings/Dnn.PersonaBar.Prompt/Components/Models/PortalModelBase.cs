using DotNetNuke.Common;

namespace Dnn.PersonaBar.Prompt.Components.Models
{

    public class PortalModelBase
    {
        public int PortalId { get; set; }
        public string PortalName { get; set; }
        public string RegistrationMode { get; set; }
        public string DefaultPortalAlias { get; set; }
        public int PageCount { get; set; }
        public int UserCount { get; set; }
        public string Language { get; set; }

        #region Constructors
        public PortalModelBase()
        {
        }

        public PortalModelBase(DotNetNuke.Entities.Portals.PortalInfo portal)
        {
            PortalId = portal.PortalID;
            PortalName = portal.PortalName;
            PageCount = portal.Pages;
            UserCount = portal.Users;
            Language = portal.DefaultLanguage;

            switch (portal.UserRegistration)
            {
                case (int)Globals.PortalRegistrationType.NoRegistration:
                    RegistrationMode = "None";
                    break;
                case (int)Globals.PortalRegistrationType.PrivateRegistration:
                    RegistrationMode = "Private";
                    break;
                case (int)Globals.PortalRegistrationType.PublicRegistration:
                    RegistrationMode = "Public";
                    break;
                case (int)Globals.PortalRegistrationType.VerifiedRegistration:
                    RegistrationMode = "Verified";
                    break;
                default:
                    RegistrationMode = "Unknown";
                    break;
            }
        }
        #endregion

    }
}