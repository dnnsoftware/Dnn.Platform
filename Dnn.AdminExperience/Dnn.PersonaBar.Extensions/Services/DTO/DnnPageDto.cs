// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;
    using Newtonsoft.Json;

    [JsonObject]
    public class DnnPageDto
    {
        public bool TranslatedVisible => !this.Default && this.TabName != null;

        public bool PublishedVisible => !this.Default && this.IsTranslated;

        public bool Default => this.DefaultLanguageGuid == Null.NullGuid;

        public string LanguageStatus
        {
            get
            {
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();

                if (this.CultureCode == portalSettings.DefaultLanguage)
                    return Localization.GetString("Default.Text", this.LocalResourceFile);

                return IsLanguagePublished(portalSettings.PortalId, this.CultureCode)
                    ? ""
                    : Localization.GetString("NotActive.Text", this.LocalResourceFile);
            }
        }

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
        public bool IsSpecial { get; set; }

        [JsonIgnore]
        public bool CanViewPage { get; set; }

        [JsonIgnore]
        public bool CanAdminPage { get; set; }

        [JsonIgnore]
        public string LocalResourceFile { get; set; }

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
