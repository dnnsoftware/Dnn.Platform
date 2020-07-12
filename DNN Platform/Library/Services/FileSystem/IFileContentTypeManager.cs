// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface IFileContentTypeManager
    {
        /// <summary>
        /// Gets get all content types dictionary.
        /// </summary>
        IDictionary<string, string> ContentTypes { get; }

        /// <summary>
        /// Gets the Content Type for the specified file extension.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <returns>The Content Type for the specified extension.</returns>
        string GetContentType(string extension);
    }
}
