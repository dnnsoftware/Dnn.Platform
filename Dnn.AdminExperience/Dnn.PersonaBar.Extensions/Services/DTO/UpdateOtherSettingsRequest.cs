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

    public class UpdateOtherSettingsRequest
    {
        public bool ShowCriticalErrors { get; set; }

        public bool DebugMode { get; set; }

        public bool RememberCheckbox { get; set; }

        public int AutoAccountUnlockDuration { get; set; }

        public int AsyncTimeout { get; set; }

        public long MaxUploadSize { get; set; }

        public string AllowedExtensionWhitelist { get; set; }

        public string DefaultEndUserExtensionWhitelist { get; set; }
    }
}
