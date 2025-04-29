// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Internal;

using DotNetNuke.Common.Utilities;

/// <summary>An abstraction of <see cref="FileSystemUtils"/> to enable unit testing.</summary>
internal interface IFileSystemUtils
{
    /// <summary>Deletes all empty folders beneath a given root folder and the root folder itself as well if empty.</summary>
    /// <param name="path">The root folder path.</param>
    void DeleteEmptyFoldersRecursive(string path);
}
