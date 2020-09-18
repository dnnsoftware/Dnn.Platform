// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    using Newtonsoft.Json;

    public class UpdateSiteAliasRequest
    {
        public int? PortalId { get; set; }

        public int? PortalAliasID { get; set; }

        public string HTTPAlias { get; set; }

        public string BrowserType { get; set; }

        public string Skin { get; set; }

        public string CultureCode { get; set; }

        public bool IsPrimary { get; set; }
    }
}
