#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class UpdateSynonymsGroupRequest
    {
        public int? PortalId { get; set; }

        public int? SynonymsGroupID { get; set; }

        public string CultureCode { get; set; }

        public string SynonymsTags { get; set; }
    }
}
