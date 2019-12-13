#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class UpdateSiteAliasRequest
    {
        public int? PortalId { get; set; }

        public int? PortalAliasID { get; set; }

        public string HTTPAlias { get; set; }

        public string BrowserType { get; set; }

        public string Skin { get; set; }

        public string CultureCode { get; set; }

        public bool IsPrimary { get; set; }
    }
}
