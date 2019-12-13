// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Tests.Integration.Executers.Dto;
using NTestDataBuilder;

namespace DotNetNuke.Tests.Integration.Executers.Builders
{
    public class TabPermissionsBuilder : TestDataBuilder<TabPermissions, TabPermissionsBuilder>
    {
        public TabPermissionsBuilder()
        {
            WithTabId(Null.NullInteger);
        }

        public TabPermissionsBuilder WithTabId(int tabId) { Set(x => x.TabId, tabId); return this; }

        public TabPermissionsBuilder WithPermissionDefinitions(IList<Permission> permissionDefinitions) { Set(x => x.PermissionDefinitions, permissionDefinitions); return this; }

        public TabPermissionsBuilder WithRolePermissions(IList<RolePermission> rolePermissions) { Set(x => x.RolePermissions, rolePermissions); return this; }

        public TabPermissionsBuilder WithUserPermissions(IList<UserPermission> userPermissions) { Set(x => x.UserPermissions, userPermissions); return this; }

        protected override TabPermissions BuildObject()
        {
            return new TabPermissions
            {
                TabId = GetOrDefault(x => x.TabId),
                PermissionDefinitions = GetOrDefault(x => x.PermissionDefinitions),
                RolePermissions = GetOrDefault(x => x.RolePermissions),
                UserPermissions = GetOrDefault(x => x.UserPermissions),
            };
        }
    }
}
