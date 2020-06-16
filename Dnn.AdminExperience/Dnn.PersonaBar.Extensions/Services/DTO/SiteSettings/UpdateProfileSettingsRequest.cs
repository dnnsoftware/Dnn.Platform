﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    using Newtonsoft.Json;

    public class UpdateProfileSettingsRequest
    {
        public int? PortalId { get; set; }

        public bool RedirectOldProfileUrl { get; set; }

        public string VanityUrlPrefix { get; set; }

        public int ProfileDefaultVisibility { get; set; }

        public bool ProfileDisplayVisibility { get; set; }
    }
}
