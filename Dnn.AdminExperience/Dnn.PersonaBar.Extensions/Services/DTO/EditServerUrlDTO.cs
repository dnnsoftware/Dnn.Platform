﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Services.Dto
{
    using System.Runtime.Serialization;

    [DataContract]
    public class EditServerUrlDTO
    {
        [DataMember(Name = "serverId")]
        public int ServerId { get; set; }

        [DataMember(Name = "newUrl")]
        public string NewUrl { get; set; }
    }
}
