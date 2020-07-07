// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    using Newtonsoft.Json;

    [JsonObject]
    public class LocalizationEntry
    {
        public string Name { get; set; }
        public string DefaultValue { get; set; }
        public string NewValue { get; set; }
    }
}
