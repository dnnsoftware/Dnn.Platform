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

    [Serializable]
    [DataContract]
    public class UpdateThemeInfo
    {
        [DataMember(Name = "path")]
        public string Path { get; set; }

        [DataMember(Name = "token")]
        public string Token { get; set; }

        [DataMember(Name = "setting")]
        public string Setting { get; set; }

        [DataMember(Name = "value")]
        public string Value { get; set; }
    }
}
