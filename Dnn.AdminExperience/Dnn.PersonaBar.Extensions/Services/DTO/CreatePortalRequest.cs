// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Sites.Services.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.Serialization;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class CreatePortalRequest
    {
        public string SiteTemplate { get; set; }

        public string SiteName { get; set; }

        public string SiteAlias { get; set; }

        public string SiteDescription { get; set; }

        public string SiteKeywords { get; set; }

        public bool IsChildSite { get; set; }

        public string HomeDirectory { get; set; }

        public int SiteGroupId { get; set; }

        public bool UseCurrentUserAsAdmin { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string PasswordConfirm { get; set; }

        public string Question { get; set; }

        public string Answer { get; set; }
    }
}
