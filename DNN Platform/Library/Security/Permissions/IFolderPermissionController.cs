// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
