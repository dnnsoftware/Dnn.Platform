#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class UpdateLanguageSettingsRequest
    {
        public int? PortalId { get; set; }

        public string CultureCode { get; set; }

        public bool EnableBrowserLanguage { get; set; }

        public bool AllowUserUICulture { get; set; }

        public string SiteDefaultLanguage { get; set; }

        public bool EnableUrlLanguage { get; set; }

        public string LanguageDisplayMode { get; set; }

        public bool AllowContentLocalization { get; set; }
    }
}
