#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class CreateLanguagePackRequest
    {
        public string PackType { get; set; }

        public int[] ModuleIds { get; set; }

        public string CultureCode { get; set; }

        public string FileName { get; set; }
    }
}
