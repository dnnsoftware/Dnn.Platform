// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
        Inline,
    }
}
