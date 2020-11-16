// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Seo.Services.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.Serialization;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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
