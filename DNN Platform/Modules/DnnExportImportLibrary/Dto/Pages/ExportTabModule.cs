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

// ReSharper disable InconsistentNaming

namespace Dnn.ExportImport.Dto.Pages
{
    public class ExportTabModule : BasicExportImportDto
    {
        public int TabModuleID { get; set; }
        public int TabID { get; set; }
        public int ModuleID { get; set; }
        public string PaneName { get; set; }
        public int ModuleOrder { get; set; }
        public int CacheTime { get; set; }
        public string Alignment { get; set; }
        public string Color { get; set; }
        public string Border { get; set; }
        public string IconFile { get; set; }
        public int Visibility { get; set; }
        public string ContainerSrc { get; set; }
        public bool DisplayTitle { get; set; }
        public bool DisplayPrint { get; set; }
        public bool DisplaySyndicate { get; set; }
        public bool IsWebSlice { get; set; }
        public string WebSliceTitle { get; set; }
        public DateTime? WebSliceExpiryDate { get; set; }
        public int? WebSliceTTL { get; set; }
        public int? CreatedByUserID { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public int? LastModifiedByUserID { get; set; }
        public DateTime? LastModifiedOnDate { get; set; }
        public bool IsDeleted { get; set; }
        public string CacheMethod { get; set; }
        public string ModuleTitle { get; set; }
        public string Header { get; set; }
        public string Footer { get; set; }
        public string CultureCode { get; set; }
        public Guid UniqueId { get; set; }
        public Guid VersionGuid { get; set; }
        public Guid? DefaultLanguageGuid { get; set; }
        public Guid LocalizedVersionGuid { get; set; }
        public bool InheritViewPermissions { get; set; }
        public bool IsShareable { get; set; }
        public bool IsShareableViewOnly { get; set; }

        public string FriendlyName { get; set; } // this is ModuleDefinition.FriendlyName
        public string CreatedByUserName { get; set; }
        public string LastModifiedByUserName { get; set; }
    }
}