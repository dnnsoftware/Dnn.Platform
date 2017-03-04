using System;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Users
{
    [JsonObject]
    [Serializable]
    [TableName("UserRelationships")]
    [PrimaryKey("UserRelationshipID")]
    public class UserRelationships : BasicExportImportDto // Do we import Relationships table as well?
    {
        [ColumnName("UserRelationshipID")]
        [JsonProperty(PropertyName = "UserRelationshipID")]
        public int UserRelationshipId { get; set; }

        [ColumnName("UserID")]
        [JsonProperty(PropertyName = "UserID")]
        public int UserId { get; set; }
        [IgnoreColumn]
        public string UserName { get; set; } //This could be used to find "UserId"


        [ColumnName("RelatedUserID")]
        [JsonProperty(PropertyName = "RelatedUserID")]
        public int RelatedUserId { get; set; }
        [IgnoreColumn]
        public string RelatedUserUserName { get; set; } //This could be used to find "RelatedUserId"


        [ColumnName("RelationshipID")]
        [JsonProperty(PropertyName = "RelationshipID")] // Do we import Relationships table as well?
        public int RelationshipId { get; set; }

        //[IgnoreColumn]
        //public string RelationshipName { get; set; } //This could be used to find "LastModifiedByUserId"

        public int Status { get; set; }

        [ColumnName("CreatedByUserID")]
        [JsonProperty(PropertyName = "CreatedByUserID")]
        public int CreatedByUserId { get; set; }

        [IgnoreColumn]
        public string CreatedByUserName { get; set; } //This could be used to find "CreatedByUserId"

        public DateTime CreatedOnDate { get; set; }

        [ColumnName("LastModifiedByUserID")]
        [JsonProperty(PropertyName = "LastModifiedByUserID")]
        public int LastModifiedByUserId { get; set; }

        [IgnoreColumn]
        public string LastModifiedByUserName { get; set; } //This could be used to find "LastModifiedByUserId"

        public DateTime? LastModifiedOnDate { get; set; }

    }
}