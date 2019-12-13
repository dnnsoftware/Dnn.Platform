#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class UpdateUrlMappingSettingsRequest
    {
        public int? PortalId { get; set; }

        public string CultureCode { get; set; }

        public string PortalAliasMapping { get; set; }

        public bool AutoAddPortalAlias { get; set; }
    }
}
