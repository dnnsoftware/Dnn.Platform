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

namespace Dnn.ExportImport.Components.Dto.Users
{
    [JsonObject]
    [Serializable]
    [TableName("UserRoles")]
    [PrimaryKey("UserRoleID")]
    public class ExportUserRole : BasicExportImportDto
    {
        [JsonProperty(PropertyName = "UserRoleID")]
        [ColumnName("UserRoleID")]
        public int UserRoleId { get; set; }

        [JsonProperty(PropertyName = "UserID")]
        [ColumnName("UserID")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "RoleID")]
        [ColumnName("RoleID")]
        public int RoleId { get; set; }

        [IgnoreColumn]
        [JsonIgnore]
        public string RoleName { get; set; }

        public DateTime? ExpiryDate { get; set; }
        public bool IsTrialUsed { get; set; }
        public DateTime? EffectiveDate { get; set; }

        [JsonProperty(PropertyName = "CreatedByUserID")]
        [ColumnName("CreatedByUserID")]
        public int CreatedByUserId { get; set; }

        [IgnoreColumn]
        [JsonIgnore]
        public string CreatedByUserName { get; set; } //This could be used to find "CreatedByUserId"
        public DateTime CreatedOnDate { get; set; }

        [JsonProperty(PropertyName = "LastModifiedByUserID")]
        [ColumnName("LastModifiedByUserID")]
        public int LastModifiedByUserId { get; set; }

        [IgnoreColumn]
        [JsonIgnore]
        public string LastModifiedByUserName { get; set; } //This could be used to find "LastModifiedByUserId"
        public DateTime? LastModifiedOnDate { get; set; }
        public int Status { get; set; }
        public bool IsOwner { get; set; }
    }
}