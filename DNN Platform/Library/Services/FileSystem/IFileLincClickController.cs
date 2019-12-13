// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Specialized;

namespace DotNetNuke.Services.FileSystem
{
    public interface IFileLinkClickController
    {
        /// <summary>
        /// Get the Link Click Url from a file
        /// </summary>
        /// <param name="file">The specified file</param>
        /// <returns>The Link Click Url</returns>
        string GetFileLinkClick(IFileInfo file);

        /// <summary>
        /// Get the File Id value contained in a Link Click Url
        /// </summary>
        /// <param name="queryParams">Query string parameters collection from a Link Click url</param>
        /// <returns>A File Id (or -1 if no File Id could be extracted from the query string parameters)</returns>
        int GetFileIdFromLinkClick(NameValueCollection queryParams);
    }
}
