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
    public class UpdateOtherSettingsRequest
    {
        public bool DisplayCopyright { get; set; }

        public bool ShowCriticalErrors { get; set; }

        public bool DebugMode { get; set; }

        public bool RememberCheckbox { get; set; }

        public int AutoAccountUnlockDuration { get; set; }

        public int AsyncTimeout { get; set; }

        public long MaxUploadSize { get; set; }

        public string AllowedExtensionWhitelist { get; set; }
    }
}
