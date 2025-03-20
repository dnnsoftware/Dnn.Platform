// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Security.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetNuke.Security.Permissions;

    public class PermissionTriStateModel
    {
        public PermissionInfo Permission { get; set; }

        public int RoleId { get; set; }

        public int UserId { get; set; }
    }
}
