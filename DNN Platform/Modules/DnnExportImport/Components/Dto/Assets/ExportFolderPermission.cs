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

namespace Dnn.ExportImport.Components.Dto.Assets
{
    [JsonObject]
    [Serializable]
    [TableName("FolderPermission")]
    [PrimaryKey("FolderPermissionID")]
    public class ExportFolderPermission:BasicExportImportDto
    {
        [ColumnName("FolderPermissionID")]
        [JsonProperty(PropertyName = "FolderPermissionID")]
        public int FolderPermissionId { get; set; }

        [ColumnName("FolderID")]
        [JsonProperty(PropertyName = "FolderID")]
        public int FolderId { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string FolderPath { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        [ColumnName("PortalID")]
        [JsonProperty(PropertyName = "PortalID")]
        public string PortalId { get; set; }


        [ColumnName("PermissionID")]
        [JsonProperty(PropertyName = "PermissionID")]
        public int PermissionId { get; set; }

        public bool AllowAccess { get; set; }

        [ColumnName("RoleID")]
        [JsonProperty(PropertyName = "RoleID")]
        public int? RoleId { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string RoleName { get; set; }

        [ColumnName("UserID")]
        [JsonProperty(PropertyName = "UserID")]
        public int? UserId { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string Username { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string DisplayName { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string PermissionCode { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        [ColumnName("ModuleDefID")]
        [JsonProperty(PropertyName = "ModuleDefID")]
        public int ModuleDefId { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string PermissionKey { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string PermissionName { get; set; }

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

    }
}