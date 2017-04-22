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
using Dnn.ExportImport.Components.Common;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto
{
    [JsonObject]
    public class ExportDto
    {
        /// <summary>
        /// Specifies the version of the exportes schema.
        /// </summary>
        public string SchemaVersion { get; set; } = Constants.CurrentSchemaVersion;

        /// <summary>
        /// ID of portal to export items from.
        /// </summary>
        public int PortalId { get; set; }

        /// <summary>
        /// SKU of the product from which the export is done.
        /// </summary>
        public string ProductSku { get; set; }

        /// <summary>
        /// Version of the product from which the export is done.
        /// </summary>
        public string ProductVersion { get; set; }

        /// <summary>
        /// Name of export job.
        /// </summary>
        public string ExportName { get; set; }

        /// <summary>
        /// Description of export job.
        /// </summary>
        public string ExportDescription { get; set; }

        /// <summary>
        /// Names of items to export
        /// </summary>
        /// <example>["Content", "Assets", "Users"]</example>
        public string[] ItemsToExport { get; set; }

        /// <summary>
        /// Whether to include deleted items in the export.
        /// Note that these will be deleted on the imported site.
        /// </summary>
        public bool IncludeDeletions { get; set; }

        /// <summary>
        ///  Whether to include the content items in the exported file.
        /// This applies to items such as pages/tabs and the content of their modules.
        /// </summary>
        public bool IncludeContent { get; set; }

        /// <summary>
        /// Whether to include item files in the exported file.
        /// This applies for user files and content files.
        /// In case the folder's and files were included in the 
        /// <see cref="ItemsToExport"/> then, this flag will be set to false.
        /// </summary>
        public bool IncludeFiles { get; set; }

        /// <summary>
        /// Whether to include users in the export file or not.
        /// </summary>
        public bool IncludeUsers { get; set; }

        /// <summary>
        /// Whether to include vocabularies in the export file or not.
        /// </summary>
        public bool IncludeVocabularies { get; set; }

        /// <summary>
        /// Whether to include page templates in export file or not.
        /// </summary>
        public bool IncludeTemplates { get; set; }

        /// <summary>
        /// Whether to include profile properties in exported file. 
        /// When this flag is enabled only then userprofile would be exported.
        /// </summary>
        public bool IncludeProperfileProperties { get; set; }

        /// <summary>
        /// Whether to include modules packages in exported file.
        /// </summary>
        public bool IncludeExtensions { get; set; }

        /// <summary>
        /// Whether to include roles or not in export file.
        /// If this flag is disabled, User Roles won't be exported.
        /// If this flag is disabled, Assets permissions won't be exported.
        /// </summary>
        public bool IncludeRoles { get; set; }

        /// <summary>
        /// Whether to incldue permissions with each entity in export file or not.
        /// </summary>
        public bool IncludePermissions { get; set; }

        /// <summary>
        /// Export mode. Differential or Complete.
        /// </summary>
        public ExportMode ExportMode { get; set; }

        /// <summary>
        /// Items' last modified on or created on for the items which need to be exported.
        /// This time format should be local time with offset in order to bae sure to export
        /// items properly and reduce the possibility of export issues.
        /// </summary>
        [JsonProperty("FromDate")]
        public DateTime? FromDateUtc { get; set; }

        /// <summary>
        /// Date when job was created. 
        /// NOTE: This will be set internally only by the engine and not by the UI
        /// </summary>
        [JsonProperty("ToDate")]
        public DateTime ToDateUtc { get; set; }

        [JsonIgnore]
        public DateTime? FromDate => FromDateUtc;
        [JsonIgnore]
        public DateTime ToDate => ToDateUtc;

        /// <summary>
        /// The pages to be exported. These are the ID's (plus other information)
        /// of all checked items but not their children when a parent is checked.
        /// If the 'TabId=-1' is included in the list, it means all site pages.
        /// </summary>
        public PageToExport[] Pages { get; set; }

        /// <summary>
        /// Whether to run the job immediately or not.
        /// </summary>
        public bool RunNow { get; set; }
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

    public enum TriCheckedState
    {
        Checked,
        UnChecked,
        Partial,
    }
}