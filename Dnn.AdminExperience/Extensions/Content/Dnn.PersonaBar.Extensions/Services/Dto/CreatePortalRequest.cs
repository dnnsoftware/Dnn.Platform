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

namespace Dnn.PersonaBar.Sites.Services.Dto
{
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
