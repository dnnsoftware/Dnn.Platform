#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class UpdateDefaultPagesSettingsRequest
    {
        public int? PortalId { get; set; }

        public string CultureCode { get; set; }

        public int SplashTabId { get; set; }

        public int HomeTabId { get; set; }

        public int LoginTabId { get; set; }

        public int RegisterTabId { get; set; }

        public int UserTabId { get; set; }

        public int SearchTabId { get; set; }

        public int Custom404TabId { get; set; }

        public int Custom500TabId { get; set; }

        public int TermsTabId { get; set; }

        public int PrivacyTabId { get; set; }

        public string PageHeadText { get; set; }
    }
}
