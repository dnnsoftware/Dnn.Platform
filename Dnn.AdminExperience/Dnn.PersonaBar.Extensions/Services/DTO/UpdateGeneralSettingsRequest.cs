// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Dnn.PersonaBar.Seo.Services.Dto
{
    public class UpdateGeneralSettingsRequest
    {
        public bool EnableSystemGeneratedUrls { get; set; }

        public string ReplaceSpaceWith { get; set; }

        public bool ForceLowerCase { get; set; }

        public bool AutoAsciiConvert { get; set; }

        public string DeletedTabHandlingType { get; set; }

        public bool RedirectUnfriendly { get; set; }

        public bool RedirectWrongCase { get; set; }

        public bool ForcePortalDefaultLanguage { get; set; }
    }
}
