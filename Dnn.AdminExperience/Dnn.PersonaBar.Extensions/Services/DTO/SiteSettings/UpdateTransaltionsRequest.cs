// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
