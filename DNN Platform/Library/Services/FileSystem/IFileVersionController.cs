#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

using System.Collections.Generic;
using System.IO;

namespace DotNetNuke.Services.FileSystem
{
    /// <summary>
    /// Do not implement.  This interface is meant for reference and unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface IFileVersionController
    {
        /// <summary>
        /// Add a new version of the file
        /// </summary>
        /// <param name="file">The file to add a version to</param>
        /// <param name="userId">The user who is performing the operation</param>
        /// <param name="published">Indicates if the new version should be the published version</param>
        /// <param name="removeOldestVersions">Remove the oldest versions if # > MaxVersions</param>
        /// <param name="content">Version content</param>
        /// <returns>The name of the file where the content should be stored</returns>
        string AddFileVersion(IFileInfo file, int userId, bool published = true, bool removeOldestVersions = true, Stream content = null);

        /// <summary>
        /// Changes the published version of a file
        /// </summary>
        /// <param name="file">The file to change its published version</param>
        /// <param name="version">the version to change to</param>
        void SetPublishedVersion(IFileInfo file, int version);

        /// <summary>
        /// Deletes a version of a file.
        /// If the version to delete is the published version, the previous version gets published
        /// </summary>
        /// <param name="file">The file to delete the version from</param>
        /// <param name="version">The number of the version to delete</param>
        /// <returns>The new published version</returns>
        int DeleteFileVersion(IFileInfo file, int version);

        /// <summary>
        /// Gets the physical file with a specific version of a file
        /// </summary>
        /// <param name="file">The file to get the version from</param>
        /// <param name="version">The number of the version to retrieve</param>
        /// <returns>The version of the file</returns>
        FileVersionInfo GetFileVersion(IFileInfo file, int version);

        /// <summary>
        /// Deletes all the unpublished versions of a file
        /// </summary>
        /// <param name="file">The file with versions</param>
        /// <param name="resetPublishedVersionNumber">If True reset to 1 the PublishedVersion Property of the FileInfo</param>
        void DeleteAllUnpublishedVersions(IFileInfo file, bool resetPublishedVersionNumber);

        /// <summary>
        /// Returns all the versions of a file
        /// </summary>
        /// <param name="file">The file with versions</param>
        /// <returns>Collection of file versions</returns>
        IEnumerable<FileVersionInfo> GetFileVersions(IFileInfo file);

        /// <summary>
        /// This method returns true if FileVersion is enabled in the Folder, false otherwise 
        /// </summary>
        /// <param name="folderId">Folder Id</param>
        /// <returns>true if FileVersion is enabled in the Folder, false otherwise </returns>
        bool IsFolderVersioned(int folderId);

        /// <summary>
        /// This method returns true if FileVersion is enabled in the Folder, false otherwise 
        /// </summary>
        /// <param name="folder">FolderInfo object</param>
        /// <returns>true if FileVersion is enabled in the Folder, false otherwise </returns>
        bool IsFolderVersioned(IFolderInfo folder);

        /// <summary>
        /// This method returns true if FileVersion is enabled in the portal, false otherwise 
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <returns>true if FileVersion is enabled in the portal, false otherwise </returns>
        bool IsFileVersionEnabled(int portalId);

        /// <summary>
        /// This method returns the max number of versions for a portal
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <returns>Max file versions</returns>
        int MaxFileVersions(int portalId);

        /// <summary>
        /// Rollbacks a file to the specified version
        /// </summary>
        /// <param name="file">The file to perform the rollback</param>        
        /// <param name="version">The version to rollback to</param>
        /// <param name="userId">The user who is performing the operation</param>
        void RollbackFileVersion(IFileInfo file, int version, int userId);

        /// <summary>
        /// Get the content of a specific version file
        /// </summary>
        /// <param name="file">The file to get the version</param>
        /// <param name="version">The version to obtain the content</param>
        /// <returns>The Stream with the file content</returns>
        Stream GetVersionContent(IFileInfo file, int version);

        /// <summary>
        /// Get all the non-published versions in a Folder
        /// </summary>
        /// <param name="folderId">Folder Id</param>
        /// <returns>Collection of file versions</returns>
        IEnumerable<FileVersionInfo> GetFileVersionsInFolder(int folderId);
    }
}
