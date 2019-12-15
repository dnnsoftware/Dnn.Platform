// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Library.DTO.Tabs
{
    [JsonObject]
    public class LocaleInfoDto
    {
        public LocaleInfoDto(string cultureCode)
        {
            CultureCode = cultureCode;
            Icon = string.IsNullOrEmpty(cultureCode)
                ? "/images/Flags/none.gif"
                : $"/images/Flags/{cultureCode}.gif";
        }

        public string CultureCode { get; }
        public string Icon { get; }
    }
}
