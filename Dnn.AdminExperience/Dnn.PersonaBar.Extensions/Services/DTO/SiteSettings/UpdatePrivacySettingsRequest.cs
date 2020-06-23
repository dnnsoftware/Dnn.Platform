// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    using Newtonsoft.Json;

    public class UpdatePrivacySettingsRequest
    {
        public int? PortalId { get; set; }

        public string CultureCode { get; set; }

        public bool ShowCookieConsent { get; set; }

        public string CookieMoreLink { get; set; }

        public bool CheckUpgrade { get; set; }

        public bool DnnImprovementProgram { get; set; }

        public bool DataConsentActive { get; set; }

        public int DataConsentConsentRedirect { get; set; }

        public int DataConsentUserDeleteAction { get; set; }

        public int DataConsentDelay { get; set; }

        public string DataConsentDelayMeasurement { get; set; }
    }
}
