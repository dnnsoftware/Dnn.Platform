// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Components.FileBrowser
{
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem;

    /// <summary>Checks the current user's folder permissions through <see cref="FolderPermissionController"/>.</summary>
    public class FolderPermissionChecker : IFolderPermissionChecker
    {
        /// <inheritdoc/>
        public bool CanBrowse(IFolderInfo folder)
        {
            return FolderPermissionController.CanBrowseFolder(folder as FolderInfo);
        }

        /// <inheritdoc/>
        public bool CanView(IFolderInfo folder)
        {
            return FolderPermissionController.CanViewFolder(folder as FolderInfo);
        }

        /// <inheritdoc/>
        public bool CanAddFiles(IFolderInfo folder)
        {
            // Same rule as the core file upload service: WRITE or ADD.
            return FolderPermissionController.CanAddFolder(folder as FolderInfo)
                || (folder != null && FolderPermissionController.HasFolderPermission(folder.FolderPermissions, "ADD"));
        }

        /// <inheritdoc/>
        public bool CanAddFolder(IFolderInfo folder)
        {
            return FolderPermissionController.CanAddFolder(folder as FolderInfo);
        }

        /// <inheritdoc/>
        public bool CanDeleteFiles(IFolderInfo folder)
        {
            return FolderPermissionController.CanDeleteFolder(folder as FolderInfo);
        }
    }
}
