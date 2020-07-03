// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Builders
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Tests.Utilities;

    internal class FileInfoBuilder
    {
        private int fileId;
        private int portalId;
        private int folderId;
        private int contentItemId;
        private DateTime startDate;
        private DateTime endDate;
        private bool enablePublishPeriod;
        private int folderMappingID;

        internal FileInfoBuilder()
        {
            this.fileId = Constants.FOLDER_ValidFileId;
            this.portalId = Constants.CONTENT_ValidPortalId;
            this.startDate = DateTime.Today;
            this.endDate = DateTime.MaxValue;
            this.enablePublishPeriod = false;
            this.contentItemId = Null.NullInteger;
            this.folderMappingID = Constants.FOLDER_ValidFolderMappingID;
            this.folderId = Constants.FOLDER_ValidFolderId;
        }

        internal FileInfoBuilder WithFileId(int fileId)
        {
            this.fileId = fileId;
            return this;
        }

        internal FileInfoBuilder WithContentItemId(int contentItemId)
        {
            this.contentItemId = contentItemId;
            return this;
        }

        internal FileInfoBuilder WithStartDate(DateTime startDate)
        {
            this.startDate = startDate;
            return this;
        }

        internal FileInfoBuilder WithEndDate(DateTime endDate)
        {
            this.endDate = endDate;
            return this;
        }

        internal FileInfoBuilder WithEnablePublishPeriod(bool enablePublishPeriod)
        {
            this.enablePublishPeriod = enablePublishPeriod;
            return this;
        }

        internal FileInfo Build()
        {
            return new FileInfo
            {
                FileId = this.fileId,
                PortalId = this.portalId,
                StartDate = this.startDate,
                EnablePublishPeriod = this.enablePublishPeriod,
                EndDate = this.endDate,
                ContentItemID = this.contentItemId,
                FolderMappingID = this.folderMappingID,
                FolderId = this.folderId,
            };
        }
    }
}
