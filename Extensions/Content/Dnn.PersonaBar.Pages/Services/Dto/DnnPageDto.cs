using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [JsonObject]
    public class DnnPageDto
    {
        public int TabId { get; set; }
        public string TabName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CultureCode { get; set; }
        public Guid DefaultLanguageGuid { get; set; }
        public bool IsTranslated { get; set; }
        public bool IsPublished { get; set; }
        public string Position { get; set; }
        public string Path { get; set; }
        public bool HasChildren { get; set; }
        public string PageUrl { get; set; }

        [JsonIgnore]
        public bool CanViewPage { get; set; }

        [JsonIgnore]
        public bool CanAdminPage { get; set; }

        [JsonIgnore]
        public string LocalResourceFile { get; set; }

        public bool TranslatedVisible => !Default && TabName != null;

        public bool PublishedVisible => !Default && IsTranslated;

        public bool Default => DefaultLanguageGuid == Null.NullGuid;

        public string LanguageStatus
        {
            get
            {
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();

                if (CultureCode == portalSettings.DefaultLanguage)
                    return Localization.GetString("Default.Text", LocalResourceFile);

                return IsLanguagePublished(portalSettings.PortalId, CultureCode)
                    ? ""
                    : Localization.GetString("NotActive.Text", LocalResourceFile);
            }
        }

        private static bool IsLanguagePublished(int portalId, string code)
        {
            var isPublished = Null.NullBoolean;
            Locale enabledLanguage;
            if (LocaleController.Instance.GetLocales(portalId).TryGetValue(code, out enabledLanguage))
            {
                isPublished = enabledLanguage.IsPublished;
            }
            return isPublished;
        }
    }
}