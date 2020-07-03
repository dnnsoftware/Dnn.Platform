﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    using System.Runtime.Serialization;

    [DataContract]
    public class CopyThemeRequest
    {
        [DataMember(Name = "pageId")]
        public int PageId { get; set; }

        [DataMember(Name = "theme")]
        public Theme Theme { get; set; }
    }
}
