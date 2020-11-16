// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Themes.Components.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Web;

    [DataContract]
    public class ApplyDefaultThemeInfo
    {
        [DataMember(Name = "themeName")]
        public string ThemeName { get; set; }

        [DataMember(Name = "level")]
        public ThemeLevel Level { get; set; }
    }
}
