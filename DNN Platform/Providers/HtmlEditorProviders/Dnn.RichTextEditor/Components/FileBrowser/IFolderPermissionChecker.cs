// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Components.FileBrowser
{
    using DotNetNuke.Services.FileSystem;

    /// <summary>Answers what the current user may do in a folder.</summary>
    public interface IFolderPermissionChecker
    {
        /// <summary>Gets a value indicating whether the user can see the folder and its contents (BROWSE or READ).</summary>
        /// <param name="folder">The folder.</param>
        /// <returns><see langword="true"/> if the folder can be browsed.</returns>
        bool CanBrowse(IFolderInfo folder);

        /// <summary>Gets a value indicating whether the user can open the folder's files (READ).</summary>
        /// <param name="folder">The folder.</param>
        /// <returns><see langword="true"/> if the folder's files can be viewed.</returns>
        bool CanView(IFolderInfo folder);

        /// <summary>Gets a value indicating whether the user can upload files to the folder (WRITE or ADD).</summary>
        /// <param name="folder">The folder.</param>
        /// <returns><see langword="true"/> if files can be added.</returns>
        bool CanAddFiles(IFolderInfo folder);

        /// <summary>Gets a value indicating whether the user can create subfolders in the folder (WRITE).</summary>
        /// <param name="folder">The folder.</param>
        /// <returns><see langword="true"/> if subfolders can be created.</returns>
        bool CanAddFolder(IFolderInfo folder);

        /// <summary>Gets a value indicating whether the user can delete files from the folder (WRITE).</summary>
        /// <param name="folder">The folder.</param>
        /// <returns><see langword="true"/> if files can be deleted.</returns>
        bool CanDeleteFiles(IFolderInfo folder);
    }
}
