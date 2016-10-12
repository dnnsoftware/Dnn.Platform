using DotNetNuke.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using DotNetNuke.Common.Utilities;

namespace Dnn.PersonaBar.Themes.Components.DTO
{
    [Serializable]
    [DataContract]
    public class ThemeInfo
    {
        [DataMember(Name = "packageName")]
        public string PackageName { get; set; }

        [DataMember(Name = "type")]
        public ThemeType Type { get; set; }

        [DataMember(Name = "path")]
        public string Path { get; set; }

        [DataMember(Name = "canDelete")]
        public bool CanDelete { get; set; } = true;

        [DataMember(Name = "level")]
        public ThemeLevel Level => Path.IndexOf(Globals.HostMapPath, StringComparison.InvariantCultureIgnoreCase) > Null.NullInteger
            ? ThemeLevel.Global
            : ThemeLevel.Site;
    }
}