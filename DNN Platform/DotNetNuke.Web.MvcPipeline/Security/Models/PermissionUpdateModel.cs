// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Security.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;

    public class PermissionUpdateModel
    {
        public int PermissionId { get; set; }

        public bool IsRolePermission { get; set; }

        public int RoleId { get; set; }

        public int UserId { get; set; }

        public string PermissionKey { get; set; }

        public bool AllowAccess { get; set; }
    }
}
