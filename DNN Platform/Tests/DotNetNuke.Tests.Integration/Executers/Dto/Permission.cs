// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Executers.Dto
{
    public class Permission
    {
        public int PermissionId { get; set; }

        public string PermissionName { get; set; }

        public bool FullControl { get; set; }

        public bool View { get; set; }

        public bool AllowAccess { get; set; }
    }
}
