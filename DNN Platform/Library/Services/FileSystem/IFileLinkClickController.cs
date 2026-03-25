// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System.Collections.Specialized;

    public interface IFileLinkClickController
    {
        /// <summary>Get the Link Click URL from a file.</summary>
        /// <param name="file">The specified file.</param>
        /// <returns>The Link Click URL.</returns>
        string GetFileLinkClick(IFileInfo file);

        /// <summary>Get the File ID value contained in a Link Click URL.</summary>
        /// <param name="queryParams">Query string parameters collection from a Link Click URL.</param>
        /// <returns>A File ID (or -1 if no File ID could be extracted from the query string parameters).</returns>
        int GetFileIdFromLinkClick(NameValueCollection queryParams);
    }
}
