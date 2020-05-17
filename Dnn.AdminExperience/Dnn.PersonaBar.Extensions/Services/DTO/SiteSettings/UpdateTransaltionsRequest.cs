// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    [JsonObject]
    public class UpdateTransaltionsRequest
    {
        public int? PortalId { get; set; }
        public string Mode { get; set; }
        public string Locale { get; set; }
        public string ResourceFile { get; set; }
        public IList<LocalizationEntry> Entries { get; set; }
    }
}
