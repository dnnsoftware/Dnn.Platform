
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class UpdateSiteSettingsRequest
    {
        public int? PortalId { get; set; }

        public string CultureCode { get; set; }

        public string PortalName { get; set; }

        public string Description { get; set; }

        public string KeyWords { get; set; }

        public string FooterText { get; set; }

        public FileDto LogoFile { get; set; }

        public string TimeZone { get; set; }

        public FileDto FavIcon { get; set; }

        public string IconSet { get; set; }
    }
}
