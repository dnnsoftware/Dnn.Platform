// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Executers.Dto
{
    using System.Collections.Generic;

    public class TabPermissions
    {
        public int TabId { get; set; }

        public IList<Permission> PermissionDefinitions { get; set; }

        public IList<RolePermission> RolePermissions { get; set; }

        public IList<UserPermission> UserPermissions { get; set; }
    }
}
