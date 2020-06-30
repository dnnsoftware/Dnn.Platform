// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Permissions
{
    using DotNetNuke.Services.FileSystem;

    public interface IFolderPermissionController
    {
        bool CanAddFolder(IFolderInfo folder);

        bool CanAdminFolder(IFolderInfo folder);

        bool CanViewFolder(IFolderInfo folder);
    }
}
