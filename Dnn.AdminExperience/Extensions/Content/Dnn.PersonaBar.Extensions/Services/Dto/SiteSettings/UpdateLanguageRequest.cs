#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class UpdateLanguageRequest
    {
        public int? PortalId { get; set; }

        public int? LanguageId { get; set; }

        public string Code { get; set; }

        public string Roles { get; set; }

        public bool Enabled { get; set; }

        public string Fallback { get; set; }
    }
}
