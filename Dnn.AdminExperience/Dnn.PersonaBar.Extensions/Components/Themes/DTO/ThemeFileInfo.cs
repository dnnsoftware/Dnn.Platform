// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Themes.Components.DTO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;
    using System.Web;

    [Serializable]
    [DataContract]
    public class ThemeFileInfo
    {
        [DataMember(Name = "themeName")]
        public string ThemeName { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "type")]
        public ThemeType Type { get; set; }

        [DataMember(Name = "path")]
        public string Path { get; set; }

        [DataMember(Name = "thumbnail")]
        public string Thumbnail { get; set; }

        [DataMember(Name = "canDelete")]
        public bool CanDelete { get; set; } = true;

        [DataMember(Name = "level")]
        public ThemeLevel Level { get; set; }
    }
}
