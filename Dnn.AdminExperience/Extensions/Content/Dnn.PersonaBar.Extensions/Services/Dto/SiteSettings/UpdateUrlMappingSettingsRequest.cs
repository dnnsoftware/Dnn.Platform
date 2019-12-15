// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class UpdateUrlMappingSettingsRequest
    {
        public int? PortalId { get; set; }

        public string CultureCode { get; set; }

        public string PortalAliasMapping { get; set; }

        public bool AutoAddPortalAlias { get; set; }
    }
}
