#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
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

namespace Dnn.ExportImport.Dto.Assets
{
    public class ExportFolder : BasicExportImportDto
    {
        public int FolderId { get; set; }

        public int PortalId { get; set; }

        public string FolderPath { get; set; }
        public int StorageLocation { get; set; }
        public bool IsProtected { get; set; }
        public bool IsCached { get; set; }
        public DateTime? LastUpdated { get; set; }

        public int? CreatedByUserId { get; set; }

        public string CreatedByUserName { get; set; } //This could be used to find "CreatedByUserId"
        public DateTime? CreatedOnDate { get; set; }

        public int? LastModifiedByUserId { get; set; }

        public string LastModifiedByUserName { get; set; } //This could be used to find "LastModifiedByUserId"
        public DateTime? LastModifiedOnDate { get; set; }
        public Guid UniqueId { get; set; }
        public Guid VersionGuid { get; set; }
        public int FolderMappingId { get; set; }

        public string FolderMappingName { get; set; }

        public int? ParentId { get; set; }

        public bool IsVersioned { get; set; }

        public int? WorkflowId { get; set; }

        public string MappedPath { get; set; }

        public int? UserId { get; set; }

        public string Username { get; set; }
    }
}