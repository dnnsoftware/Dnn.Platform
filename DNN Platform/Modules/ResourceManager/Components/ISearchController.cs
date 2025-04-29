// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Components;

using System.Collections.Generic;

using DotNetNuke.Services.FileSystem;

/// <summary>Provides search functionality for the resource manager.</summary>
public interface ISearchController
{
    /// <summary>Performs the Advanced File Search.</summary>
    /// <param name="moduleId">The id of the Module.</param>
    /// <param name="folder">Folder that defines the context of the search.</param>
    /// <param name="recursive">A value indicating whether to include subfolders in search.</param>
    /// <param name="search">Search query.</param>
    /// <param name="pageIndex">The page index to get.</param>
    /// <param name="pageSize">The size of each page.</param>
    /// <param name="sorting">A string that defines the sort.</param>
    /// <param name="moduleMode">Module Mode.</param>
    /// <param name="totalCount">Returns the total count (out parameter).</param>
    /// <returns>Result set of the file search page.</returns>
    IList<IFileInfo> SearchFolderContent(int moduleId, IFolderInfo folder, bool recursive, string search, int pageIndex, int pageSize, string sorting, int moduleMode, out int totalCount);
}
