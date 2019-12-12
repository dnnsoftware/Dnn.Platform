// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Newtonsoft.Json;

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    [JsonObject]
    public class LanguageTabDto
    {
        public int PageId { get; set; }
        public string PageName { get; set; }
        public string ViewUrl { get; set; }
    }
}
