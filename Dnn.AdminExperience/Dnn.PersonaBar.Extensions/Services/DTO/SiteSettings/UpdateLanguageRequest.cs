// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    using Newtonsoft.Json;

    public class UpdateLanguageRequest
    {
        public int? PortalId { get; set; }

        public int? LanguageId { get; set; }

        public string Code { get; set; }

        public string Roles { get; set; }

        public bool Enabled { get; set; }

        public string Fallback { get; set; }
    }
}
