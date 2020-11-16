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

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;

    [Serializable]
    [DataContract]
    public class ThemeInfo
    {
        [DataMember(Name = "level")]
        public ThemeLevel Level => ThemesController.GetThemeLevel(this.Path);

        [DataMember(Name = "packageName")]
        public string PackageName { get; set; }

        [DataMember(Name = "type")]
        public ThemeType Type { get; set; }

        [DataMember(Name = "path")]
        public string Path { get; set; }

        [DataMember(Name = "defaultThemeFile")]
        public string DefaultThemeFile { get; set; }

        [DataMember(Name = "thumbnail")]
        public string Thumbnail { get; set; }

        [DataMember(Name = "canDelete")]
        public bool CanDelete { get; set; } = true;
    }
}
