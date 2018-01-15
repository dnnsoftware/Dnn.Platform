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

        #region Supoort for BaseEntityInfo on inherited classes

        int CreatedByUserID { get; }

        DateTime CreatedOnDate { get; }

        int LastModifiedByUserID { get; }

        DateTime LastModifiedOnDate { get; }

        UserInfo CreatedByUser(int portalId);

        UserInfo LastModifiedByUser(int portalId);

        #endregion
    }
}
