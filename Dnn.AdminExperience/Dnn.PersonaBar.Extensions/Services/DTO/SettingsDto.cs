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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Dnn.PersonaBar.Security.Services.Dto
{
    public class SettingsDto
    {
        public string SettingName { get; set; }

        public string SettingValue { get; set; }

        public string Type { get; set; }

        public int PortalId { get; set; }

        public int TabId { get; set; }

        public int ModuleId { get; set; }

        public int LastModifiedByUserId { get; set; }

        public string LastModifiedOnDate { get; set; }
    }
}
