#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class UpdateIgnoreWordsRequest
    {
        public int? PortalId { get; set; }

        public int StopWordsId { get; set; }

        public string CultureCode { get; set; }

        public string StopWords { get; set; }
    }
}
