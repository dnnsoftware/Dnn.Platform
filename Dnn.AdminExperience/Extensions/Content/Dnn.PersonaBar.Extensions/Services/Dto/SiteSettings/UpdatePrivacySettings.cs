#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class UpdatePrivacySettingsRequest
    {
        public int? PortalId { get; set; }

        public string CultureCode { get; set; }

        public bool ShowCookieConsent { get; set; }

        public string CookieMoreLink { get; set; }

        public bool CheckUpgrade { get; set; }

        public bool DnnImprovementProgram { get; set; }

        public bool DisplayCopyright { get; set; }

        public bool DataConsentActive { get; set; }

        public int DataConsentConsentRedirect { get; set; }

        public int DataConsentUserDeleteAction { get; set; }

        public int DataConsentDelay { get; set; }

        public string DataConsentDelayMeasurement { get; set; }
    }
}
