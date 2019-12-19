// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
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
