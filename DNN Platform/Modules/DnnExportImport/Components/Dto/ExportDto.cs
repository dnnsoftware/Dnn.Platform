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
        /// Items' last modified on or created on for the items which need to be exported.
        /// This time format should be local time with offset in order to bae sure to export
        /// items properly and reduce the possibility of export issues.
        /// </summary>
        public DateTimeOffset? FromDate { get; set; }

        /// <summary>
        /// Date when job was created. 
        /// NOTE: This will be set internally only by the engine and not by the UI
        /// </summary>
        public DateTime ToDate { get; set; }

        /// <summary>
        /// The pages to be exported (list of TabId values). These are the ID's
        /// of all checked items but not their children when a parent is checked.
        /// If the value '-1' is included in the list, it means all site pages.
        /// </summary>
        public int[] Pages { get; set; }
    }
}