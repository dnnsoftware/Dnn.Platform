#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class UpdateProfileSettingsRequest
    {
        public int? PortalId { get; set; }

        public bool RedirectOldProfileUrl { get; set; }

        public string VanityUrlPrefix { get; set; }

        public int ProfileDefaultVisibility { get; set; }

        public bool ProfileDisplayVisibility { get; set; }
    }
}
