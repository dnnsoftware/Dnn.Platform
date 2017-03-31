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
using Dnn.ExportImport.Dto;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Portal
{
    [JsonObject]
    [Serializable]
    [TableName("PortalLocalization")]
    [PrimaryKey("PortalID,CultureCode", AutoIncrement = false)]

    public class ExportPortalLocalization : BasicExportImportDto
    {
        [ColumnName("PortalID")]
        [JsonProperty(PropertyName = "PortalID")]
        public int PortalId { get; set; }

        public string CultureCode { get; set; }
        public string PortalName { get; set; }
        public string LogoFile { get; set; }
        public string FooterText { get; set; }
        public string Description { get; set; }
        public string KeyWords { get; set; }
        public string BackgroundFile { get; set; }
        public int? HomeTabId { get; set; }
        public int? LoginTabId { get; set; }
        public int? UserTabId { get; set; }
        public int? AdminTabId { get; set; }
        public int? SplashTabId { get; set; }

        [ColumnName("CreatedByUserID")]
        [JsonProperty(PropertyName = "CreatedByUserID")]
        public int? CreatedByUserId { get; set; }

        [IgnoreColumn]
        public string CreatedByUserName { get; set; } //This could be used to find "CreatedByUserId"
        public DateTime? CreatedOnDate { get; set; }

        [ColumnName("LastModifiedByUserID")]
        [JsonProperty(PropertyName = "LastModifiedByUserID")]
        public int? LastModifiedByUserId { get; set; }

        [IgnoreColumn]
        public string LastModifiedByUserName { get; set; } //This could be used to find "LastModifiedByUserId"
        public DateTime? LastModifiedOnDate { get; set; }
        public int? RegisterTabId { get; set; }
        public int? SearchTabId { get; set; }
        public int? Custom404TabId { get; set; }
        public int? Custom500TabId { get; set; }
    }
}