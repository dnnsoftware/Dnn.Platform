// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Sites.Components.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Web;

    [DataContract]
    public class HttpAliasDto
    {
        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "link")]
        public string Link { get; set; }
    }
}
