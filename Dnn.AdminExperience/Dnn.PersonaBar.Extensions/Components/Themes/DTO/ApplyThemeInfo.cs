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
    public class ApplyThemeInfo
    {
        [DataMember(Name = "themeFile")]
        public ThemeFileInfo ThemeFile { get; set; }

        [DataMember(Name = "scope")]
        public ApplyThemeScope Scope { get; set; }
    }
}
