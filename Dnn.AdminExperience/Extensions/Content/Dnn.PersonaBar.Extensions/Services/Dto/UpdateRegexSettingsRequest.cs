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

namespace Dnn.PersonaBar.Seo.Services.Dto
{
    public class UpdateRegexSettingsRequest
    {
        public string IgnoreRegex { get; set; }

        public string DoNotRewriteRegex { get; set; }

        public string UseSiteUrlsRegex { get; set; }

        public string DoNotRedirectRegex { get; set; }

        public string DoNotRedirectSecureRegex { get; set; }

        public string ForceLowerCaseRegex { get; set; }

        public string NoFriendlyUrlRegex { get; set; }

        public string DoNotIncludeInPathRegex { get; set; }

        public string ValidExtensionlessUrlsRegex { get; set; }

        public string RegexMatch { get; set; }
    }
}
