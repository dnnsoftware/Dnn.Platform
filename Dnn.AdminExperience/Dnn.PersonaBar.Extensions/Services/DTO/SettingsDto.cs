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
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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
