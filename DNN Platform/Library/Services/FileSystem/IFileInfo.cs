// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.IO;

using DotNetNuke.Entities.Users;

namespace DotNetNuke.Services.FileSystem
{
    public interface IFileInfo
    {
        string ContentType { get; set; }

        string Extension { get; set; }

        int FileId { get; set; }

        string FileName { get; set; }

        string Folder { get; set; }

        int FolderId { get; set; }

        int Height { get; set; }

        bool IsCached { get; set; }

        string PhysicalPath { get; }

        int PortalId { get; set; }

        string RelativePath { get; }

        string SHA1Hash { get; set; }

        int Size { get; set; }

        int StorageLocation { get; set; }

        Guid UniqueId { get; set; }

        Guid VersionGuid { get; set; }

        int Width { get; set; }

        FileAttributes? FileAttributes { get; }

        bool SupportsFileAttributes { get; }

        DateTime LastModificationTime { get; set; }

        int FolderMappingID { get; set; }

        /// <summary>
        /// Gets or sets a metadata field with an optional title associated to the file
        /// </summary>
        string Title { get; set; }

        string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether publish period is enabled for the file
        /// </summary>
        bool EnablePublishPeriod { get; set; }

        /// <summary>
        /// Gets or sets the date on which the file starts to be published
        /// </summary>
        DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the date on which the file ends to be published
        /// </summary>
        DateTime EndDate { get; set; }

        /// <summary>
        /// Gets a value indicating whether the file is enabled,
        /// considering if the publish period is active and if the current date is within the publish period
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Gets or sets a reference to ContentItem, to use in Workflows
        /// </summary>
        int ContentItemID { get; set; }

        /// <summary>
        /// Gets or sets the published version number of the file
        /// </summary>
        int PublishedVersion { get; set; }

        /// <summary>
        /// Gets a flag which says whether the file has ever been published
        /// </summary>
        bool HasBeenPublished { get; }

        int CreatedByUserID { get; }

        DateTime CreatedOnDate { get; }

        int LastModifiedByUserID { get; }

        DateTime LastModifiedOnDate { get; }

        UserInfo CreatedByUser(int portalId);

        UserInfo LastModifiedByUser(int portalId);
    }
}
