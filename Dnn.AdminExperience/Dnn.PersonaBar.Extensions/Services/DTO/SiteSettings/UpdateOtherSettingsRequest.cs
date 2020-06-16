
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using Newtonsoft.Json;

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class UpdateOtherSettingsRequest
    {
        public int? PortalId { get; set; }

        public string CultureCode { get; set; }

        public string AllowedExtensionsWhitelist { get; set; }
    }
}
