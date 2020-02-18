// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [JsonObject]
    public class TranslatorsComment
    {
        public int TabId { get; set; }
        public string Text { get; set; }
    }
}
