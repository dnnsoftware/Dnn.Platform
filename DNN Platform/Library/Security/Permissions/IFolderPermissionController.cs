// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Security.Permissions
{
    public interface IFolderPermissionController
    {
        bool CanAddFolder(IFolderInfo folder);
        bool CanAdminFolder(IFolderInfo folder);
        bool CanViewFolder(IFolderInfo folder);
    }
}
