// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Security.Services.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.Serialization;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Entities.Host;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class UpdateSslSettingsRequest
    {
        public bool SSLEnabled { get; set; }

        public bool SSLEnforced { get; set; }

        public string SSLURL { get; set; }

        public string STDURL { get; set; }

        public string SSLOffloadHeader { get; set; }
    }
}
