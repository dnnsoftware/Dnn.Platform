// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Executers.Builders
{
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Tests.Integration.Executers.Dto;
    using NTestDataBuilder;

    public class TabPermissionsBuilder : TestDataBuilder<TabPermissions, TabPermissionsBuilder>
    {
        public TabPermissionsBuilder()
        {
            this.WithTabId(Null.NullInteger);
        }

        public TabPermissionsBuilder WithTabId(int tabId)
        {
            this.Set(x => x.TabId, tabId);
            return this;
        }

        public TabPermissionsBuilder WithPermissionDefinitions(IList<Permission> permissionDefinitions)
        {
            this.Set(x => x.PermissionDefinitions, permissionDefinitions);
            return this;
        }

        public TabPermissionsBuilder WithRolePermissions(IList<RolePermission> rolePermissions)
        {
            this.Set(x => x.RolePermissions, rolePermissions);
            return this;
        }

        public TabPermissionsBuilder WithUserPermissions(IList<UserPermission> userPermissions)
        {
            this.Set(x => x.UserPermissions, userPermissions);
            return this;
        }

        protected override TabPermissions BuildObject()
        {
            return new TabPermissions
            {
                TabId = this.GetOrDefault(x => x.TabId),
                PermissionDefinitions = this.GetOrDefault(x => x.PermissionDefinitions),
                RolePermissions = this.GetOrDefault(x => x.RolePermissions),
                UserPermissions = this.GetOrDefault(x => x.UserPermissions),
            };
        }
    }
}
