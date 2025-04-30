// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    /// <summary>A contract specifying the ability to read and adjust paths.</summary>
    public interface IPathUtils
    {
        /// <summary>Adds backslash to the specified source.</summary>
        /// <param name="source">The source string to be modified.</param>
        /// <returns>The original string plus a backslash.</returns>
        string AddTrailingSlash(string source);

        /// <summary>Formats the provided folder path by adding a slash if needed.</summary>
        /// <param name="folderPath">The folder path to format.</param>
        /// <returns>The formatted path.</returns>
        string FormatFolderPath(string folderPath);

        /// <summary>Gets the physical path for the specified relative path.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>The path.</returns>
        string GetPhysicalPath(int portalID, string relativePath);

        /// <summary>Gets the relative path for the specified physical path.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="physicalPath">The physical path.</param>
        /// <returns>The path.</returns>
        string GetRelativePath(int portalID, string physicalPath);

        /// <summary>Gets the physical root folder path for the specified portal.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <returns>The path.</returns>
        string GetRootFolderMapPath(int portalID);

        /// <summary>Checks if a folder is a default protected folder.</summary>
        /// <param name="folderPath">The folder path.</param>
        /// <returns>True if the folderPath is a default protected folder. False otherwise.</returns>
        bool IsDefaultProtectedPath(string folderPath);

        /// <summary>The MapPath method maps the specified relative or virtual path to the corresponding physical directory on the server.</summary>
        /// <param name="path">Specifies the relative or virtual path to map to a physical directory. If Path starts with either
        /// a forward (/) or backward slash (\), the MapPath method returns a path as if Path were a full, virtual path. If Path
        /// doesn't start with a slash, the MapPath method returns a path relative to the directory of the .asp file being processed.</param>
        /// <returns>The physical path.</returns>
        /// <remarks>
        /// If path is a null reference (Nothing in Visual Basic), then the MapPath method returns the full physical path
        /// of the directory that contains the current application.
        /// </remarks>
        string MapPath(string path);

        /// <summary>Removes the trailing slash or backslash from the specified source.</summary>
        /// <param name="source">The source string to be modified.</param>
        /// <returns>The original string minus the trailing slash.</returns>
        string RemoveTrailingSlash(string source);

        /// <summary>Strips the original path by removing starting 0 or 0\\.</summary>
        /// <param name="originalPath">The original path.</param>
        /// <returns>The stripped path.</returns>
        string StripFolderPath(string originalPath);
    }
}
