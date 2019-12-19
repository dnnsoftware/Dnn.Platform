// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Dnn.PersonaBar.Themes.Components.DTO
{
    [DataContract]
    public class ApplyThemeInfo
    {
        [DataMember(Name = "themeFile")]
        public ThemeFileInfo ThemeFile { get; set; }

        [DataMember(Name = "scope")]
        public ApplyThemeScope Scope { get; set; }
    }
}
