// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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

        public bool DataConsentActive { get; set; }

        public int DataConsentConsentRedirect { get; set; }

        public int DataConsentUserDeleteAction { get; set; }

        public int DataConsentDelay { get; set; }

        public string DataConsentDelayMeasurement { get; set; }
    }
}
