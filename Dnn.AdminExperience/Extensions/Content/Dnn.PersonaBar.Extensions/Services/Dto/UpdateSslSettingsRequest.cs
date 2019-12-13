// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Dnn.PersonaBar.Security.Services.Dto
{
    public class UpdateSslSettingsRequest
    {
        public bool SSLEnabled { get; set; }

        public bool SSLEnforced { get; set; }

        public string SSLURL { get; set; }

        public string STDURL { get; set; }

        public string SSLOffloadHeader { get; set; }
    }
}
