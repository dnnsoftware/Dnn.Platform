// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Dto
{
    using System;

    using Dnn.ExportImport.Components.Common;
    using Newtonsoft.Json;

    public enum TriCheckedState
    {
#if false
        // for schema 1.0.0
        Checked = 0,
        UnChecked = 1,
        Partial = 2,
#else
        UnChecked = 0,
        Checked = 1,
        CheckedWithAllChildren = 2,
#endif

    }

    [JsonObject]
    public class ExportDto
    {
        [JsonIgnore]
        public DateTime? FromDate => this.FromDateUtc;

        [JsonIgnore]
        public DateTime ToDate => this.ToDateUtc;

        public int Id { get; set; }

        /// <summary>
        /// Gets or sets specifies the version of the exportes schema.
        /// </summary>
        public string SchemaVersion { get; set; } = Constants.CurrentSchemaVersion;

        /// <summary>
        /// Gets or sets iD of portal to export items from.
        /// </summary>
        public int PortalId { get; set; }

        /// <summary>
        /// Gets or sets sKU of the product from which the export is done.
        /// </summary>
        public string ProductSku { get; set; }

        /// <summary>
        /// Gets or sets version of the product from which the export is done.
        /// </summary>
        public string ProductVersion { get; set; }

        /// <summary>
        /// Gets or sets name of export job.
        /// </summary>
        public string ExportName { get; set; }

        /// <summary>
        /// Gets or sets description of export job.
        /// </summary>
        public string ExportDescription { get; set; }

        /// <summary>
        /// Gets or sets names of items to export.
        /// </summary>
        /// <example>["Content", "Assets", "Users"].</example>
        public string[] ItemsToExport { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether to include deleted items in the export.
        /// Note that these will be deleted on the imported site.
        /// </summary>
        public bool IncludeDeletions { get; set; }

        /// <summary>
        ///  Gets or sets a value indicating whether whether to include the content items in the exported file.
        /// This applies to items such as pages/tabs and the content of their modules.
        /// </summary>
        public bool IncludeContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether to include item files in the exported file.
        /// This applies for user files and content files.
        /// In case the folder's and files were included in the
        /// <see cref="ItemsToExport"/> then, this flag will be set to false.
        /// </summary>
        public bool IncludeFiles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether to include users in the export file or not.
        /// </summary>
        public bool IncludeUsers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether to include vocabularies in the export file or not.
        /// </summary>
        public bool IncludeVocabularies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether to include page templates in export file or not.
        /// </summary>
        public bool IncludeTemplates { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether to include profile properties in exported file.
        /// When this flag is enabled only then userprofile would be exported.
        /// </summary>
        public bool IncludeProperfileProperties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether to include modules packages in exported file.
        /// </summary>
        public bool IncludeExtensions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether to include roles or not in export file.
        /// If this flag is disabled, User Roles won't be exported.
        /// If this flag is disabled, Assets permissions won't be exported.
        /// </summary>
        public bool IncludeRoles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether to incldue permissions with each entity in export file or not.
        /// </summary>
        public bool IncludePermissions { get; set; }

        /// <summary>
        /// Gets or sets export mode. Differential or Complete.
        /// </summary>
        public ExportMode ExportMode { get; set; }

        /// <summary>
        /// Gets or sets items' last modified on or created on for the items which need to be exported.
        /// This time format should be local time with offset in order to bae sure to export
        /// items properly and reduce the possibility of export issues.
        /// </summary>
        [JsonProperty("FromDate")]
        public DateTime? FromDateUtc { get; set; }

        /// <summary>
        /// Gets or sets date when job was created.
        /// NOTE: This will be set internally only by the engine and not by the UI.
        /// </summary>
        [JsonProperty("ToDate")]
        public DateTime ToDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the pages to be exported. These are the ID's (plus other information)
        /// of all checked items but not their children when a parent is checked.
        /// If the 'TabId=-1' is included in the list, it means all site pages.
        /// </summary>
        public PageToExport[] Pages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether to run the job immediately or not.
        /// </summary>
        public bool RunNow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether used to determine if the DB file needs cleanup before starting import or not.
        /// </summary>
        public bool IsDirty { get; set; }
    }

    /// <summary>
    ///  Spercifies page to be exported.
    /// </summary>
    [JsonObject]
    public class PageToExport
    {
        public int TabId { get; set; }

        public int ParentTabId { get; set; }

        public TriCheckedState CheckedState { get; set; }
    }
}
