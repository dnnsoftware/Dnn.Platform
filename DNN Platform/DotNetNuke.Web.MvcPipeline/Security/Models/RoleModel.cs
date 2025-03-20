// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Security.Models
{
    using System.Collections.Generic;

    public class RoleModel
    {
        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public Dictionary<string, PermissionModel> Permissions { get; set; }
    }
}
