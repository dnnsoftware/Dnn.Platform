﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace Dnn.ExportImport.Dto.Assets
{
    public class ExportFile : BasicExportImportDto
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public int Size { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string ContentType { get; set; }

        public int FolderId { get; set; }

        public string Folder { get; set; }

        public byte[] Content { get; set; }

        public int? CreatedByUserId { get; set; }

        public string CreatedByUserName { get; set; } //This could be used to find "CreatedByUserId"
        public DateTime? CreatedOnDate { get; set; }

        public int? LastModifiedByUserId { get; set; }
        public string LastModifiedByUserName { get; set; } //This could be used to find "LastModifiedByUserId"
        public DateTime? LastModifiedOnDate { get; set; }

        public Guid UniqueId { get; set; }
        public Guid VersionGuid { get; set; }

        public string Sha1Hash { get; set; }

        public DateTime LastModificationTime { get; set; }

        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public bool EnablePublishPeriod { get; set; }
        public DateTime? EndDate { get; set; }
        public int PublishedVersion { get; set; }

        public int? ContentItemId { get; set; }

        public bool HasBeenPublished { get; set; }
        public string Description { get; set; }


        public int StorageLocation { get; set; }

        public bool IsCached { get; set; }

        public int FolderMappingId { get; set; }
    }
}
