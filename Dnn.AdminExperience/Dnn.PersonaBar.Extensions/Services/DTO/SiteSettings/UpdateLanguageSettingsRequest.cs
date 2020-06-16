// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    using Newtonsoft.Json;

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
