// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    using Newtonsoft.Json;

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
