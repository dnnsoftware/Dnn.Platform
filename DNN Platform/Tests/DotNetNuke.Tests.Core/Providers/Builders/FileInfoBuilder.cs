#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Tests.Utilities;

namespace DotNetNuke.Tests.Core.Providers.Builders
{
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
            fileId = Constants.FOLDER_ValidFileId;
            portalId = Constants.CONTENT_ValidPortalId;
            startDate = DateTime.Today;
            endDate = DateTime.MaxValue;
            enablePublishPeriod = false;
            contentItemId = Null.NullInteger;
            folderMappingID = Constants.FOLDER_ValidFolderMappingID;
            folderId = Constants.FOLDER_ValidFolderId;
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
                    FileId = fileId,
                    PortalId = portalId,
                    StartDate = startDate,
                    EnablePublishPeriod = enablePublishPeriod,
                    EndDate = endDate,
                    ContentItemID = contentItemId,
                    FolderMappingID = folderMappingID,
                    FolderId = folderId
                };
        }
    }
}
