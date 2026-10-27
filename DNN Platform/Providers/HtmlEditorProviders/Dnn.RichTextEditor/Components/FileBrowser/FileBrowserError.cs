// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Components.FileBrowser
{
    /// <summary>Why a file browser operation failed. The names double as localization keys.</summary>
    public enum FileBrowserError
    {
        /// <summary>The operation succeeded.</summary>
        None = 0,

        /// <summary>The folder does not exist or cannot be browsed by the user.</summary>
        FolderNotFound,

        /// <summary>The file does not exist.</summary>
        FileNotFound,

        /// <summary>The user lacks the folder permission the operation needs.</summary>
        PermissionDenied,

        /// <summary>The file or folder name is not allowed.</summary>
        InvalidName,

        /// <summary>A folder with that name already exists.</summary>
        AlreadyExists,

        /// <summary>The file type is not on the host's allowed file extension list.</summary>
        ExtensionNotAllowed,

        /// <summary>The file could not be saved, for example because the site's disk quota is used up.</summary>
        SaveFailed,
    }
}
