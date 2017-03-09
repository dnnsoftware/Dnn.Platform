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
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto
{
    [JsonObject]
    public class ExportDto
    {
        /// <summary>
        /// Specifies the version of the exportes schema.
        /// </summary>
        public string SchemaVersion { get; set; } = "1.0.0";

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
        /// Include item properties in the export. This is used for some items 
        /// as including files metadata.
        /// </summary>
        public bool IncludeProperties { get; set; }

        /// <summary>
        /// Whether to include items permissions in the exported file.
        /// </summary>
        public bool IncludePermissions { get; set; }

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
        /// Specifies what to do when there is a collision during the import process.
        /// </summary>
        public CollisionResolution CollisionResolution { get; set; }

        /// <summary>
        /// Items' last modified on or created on for the items which need to be exported.
        /// This time format should be local time with offset in order to bae sure to export
        /// items properly and reduce the possibility of export issues.
        /// </summary>
        public DateTimeOffset ExportTime { get; set; }

        /// <summary>
        ///  The pages to be exported.
        /// </summary>
        public PageToExport[] Pages { get; set; }
    }

    /// <summary>
    ///  Spercifies page to be exported.
    /// </summary>
    [JsonObject]
    public class PageToExport
    {
        public int TabId { get; set; }
        public PageToExport[] Children { get; set; }
    }

    /// <summary>
    /// Specifies what to do when there is a collision during the import process.
    /// </summary>
    public enum CollisionResolution
    {
        /// <summary>
        /// Ignore the imported item and continue.
        /// </summary>
        Ignore,

        /// <summary>
        /// Overwrites the existing item upon importing.
        /// </summary>
        Overwrite,

        /// <summary>
        /// Duplicate the items into a new item upon importing.
        /// </summary>
        Duplicate
    }
}