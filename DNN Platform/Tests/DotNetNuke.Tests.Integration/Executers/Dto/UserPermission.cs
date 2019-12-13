// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;

namespace DotNetNuke.Tests.Integration.Executers.Dto
{
    public class UserPermission
    {
        public int UserId { get; set; }

        public string DisplayName { get; set; }

        public IList<Permission> Permissions { get; set; }
    }
}
