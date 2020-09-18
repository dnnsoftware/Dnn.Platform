// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable InconsistentNaming
namespace Dnn.ExportImport.Dto.Pages
{
    using System;

    public class ExportTab : BasicExportImportDto
    {
        public int TabId { get; set; }

        public int TabOrder { get; set; }

        public string TabName { get; set; }

        public bool IsVisible { get; set; }

        public int? ParentId { get; set; }

        public string IconFile { get; set; }

        public bool DisableLink { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string KeyWords { get; set; }

        public bool IsDeleted { get; set; }

        public string Url { get; set; }

        public string SkinSrc { get; set; }

        public string ContainerSrc { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? RefreshInterval { get; set; }

        public string PageHeadText { get; set; }

        public bool IsSecure { get; set; }

        public bool PermanentRedirect { get; set; }

        public float SiteMapPriority { get; set; }

        public int? CreatedByUserID { get; set; }

        public DateTime? CreatedOnDate { get; set; }

        public int? LastModifiedByUserID { get; set; }

        public DateTime? LastModifiedOnDate { get; set; }

        public string IconFileLarge { get; set; }

        public string CultureCode { get; set; }

        public int? ContentItemID { get; set; } // FK

        public Guid UniqueId { get; set; }

        public Guid VersionGuid { get; set; }

        public Guid? DefaultLanguageGuid { get; set; }

        public Guid LocalizedVersionGuid { get; set; }

        public int Level { get; set; }

        public string TabPath { get; set; }

        public bool HasBeenPublished { get; set; }

        public bool IsSystem { get; set; }

        public int StateID { get; set; }

        public string CreatedByUserName { get; set; }

        public string LastModifiedByUserName { get; set; }

        public string Tags { get; set; }
    }
}
