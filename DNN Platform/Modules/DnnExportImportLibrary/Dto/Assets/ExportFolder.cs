﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace Dnn.ExportImport.Dto.Assets
{
    public class ExportFolder : BasicExportImportDto
    {
        public int FolderId { get; set; }

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

        public string ParentFolderPath { get; set; }

        public bool IsVersioned { get; set; }

        public int? WorkflowId { get; set; }

        public string MappedPath { get; set; }

        public int? UserId { get; set; }

        public string Username { get; set; }
    }
}
