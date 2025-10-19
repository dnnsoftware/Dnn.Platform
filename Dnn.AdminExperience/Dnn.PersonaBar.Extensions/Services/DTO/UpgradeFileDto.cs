// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Services.Dto
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class UpgradeFileDto
    {
        [DataMember(Name = "version")]
        public Version Version { get; set; }

        [DataMember(Name = "fileName")]
        public string FileName { get; set; }

        [DataMember(Name = "isObsolete")]
        public bool IsObsolete { get; set; }
    }
}
