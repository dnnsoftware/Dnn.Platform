// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Services.FileSystem
{
    /// <summary>
    /// Represents the different options when downloading a file.
    /// </summary>
    public enum ContentDisposition
    {
        /// <summary>
        /// The browser will display a dialog to allow the user to save or view the document.
        /// </summary>
        Attachment,
        /// <summary>
        /// The document will be displayed automatically.
        /// </summary>
        Inline
    }
}
