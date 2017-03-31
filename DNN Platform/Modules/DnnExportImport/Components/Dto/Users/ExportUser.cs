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
    [TableName("Users")]
    [PrimaryKey("UserID")]
    public class ExportUser : BasicExportImportDto
    {
        [IgnoreColumn]
        [JsonIgnore]
        public int Total { get; set; }
        [JsonIgnore]
        [IgnoreColumn]
        public int RowId { get; set; }
        [JsonIgnore]
        [IgnoreColumn]
        public int RowIdDesc { get; set; }

        [ColumnName("UserID")]
        [JsonProperty(PropertyName= "UserID")]
        public int UserId { get; set; }

        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsSuperUser { get; set; }
        public int AffiliateId { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public bool UpdatePassword { get; set; }

        [ColumnName("LastIPAddress")]
        [JsonProperty(PropertyName = "LastIPAddress")]
        public string LastIpAddress { get; set; }

        public bool IsDeleted { get; set; }

        [ColumnName("CreatedByUserID")]
        [JsonProperty(PropertyName= "CreatedByUserID")]
        public int CreatedByUserId { get; set; } //How do we insert this value?
        [IgnoreColumn]
        public string CreatedByUserName { get; set; }//This could be used to find "CreatedByUserId"
        public DateTime? CreatedOnDate { get; set; }

        [ColumnName("LastModifiedByUserID")]
        [JsonProperty(PropertyName= "LastModifiedByUserID")]
        public int LastModifiedByUserId { get; set; } //How do we insert this value?
        [IgnoreColumn]
        public string LastModifiedByUserName { get; set; }//This could be used to find "LastModifiedByUserId"
        public DateTime? LastModifiedOnDate { get; set; }
        public Guid? PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiration { get; set; }
        [IgnoreColumn]
        public string LowerEmail { get; set; }
    }
}
