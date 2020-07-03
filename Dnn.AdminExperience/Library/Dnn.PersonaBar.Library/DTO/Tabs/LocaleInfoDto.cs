// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.DTO.Tabs
{
    using Newtonsoft.Json;

    [JsonObject]
    public class LocaleInfoDto
    {
        public LocaleInfoDto(string cultureCode)
        {
            this.CultureCode = cultureCode;
            this.Icon = string.IsNullOrEmpty(cultureCode)
                ? "/images/Flags/none.gif"
                : $"/images/Flags/{cultureCode}.gif";
        }

        public string CultureCode { get; }

        public string Icon { get; }
    }
}
