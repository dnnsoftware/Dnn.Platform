#region Copyright
//
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
#endregion

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
