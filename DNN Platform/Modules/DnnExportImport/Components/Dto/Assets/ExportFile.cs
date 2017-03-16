using DotNetNuke.ComponentModel.DataAnnotations;
using System;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Assets
{
    [JsonObject]
    [Serializable]
    [TableName("Files")]
    [PrimaryKey("FileId")]
    public class ExportFile : BasicExportImportDto
    {
        public int FileId { get; set; }
        public int PortalId { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public int Size { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string ContentType { get; set; }

        [ColumnName("FolderID")]
        [JsonProperty(PropertyName = "FolderID")]
        public int FolderId { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string Folder { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public byte[] Content { get; set; }

        [ColumnName("CreatedByUserID")]
        [JsonProperty(PropertyName = "CreatedByUserID")]
        public int? CreatedByUserId { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string CreatedByUserName { get; set; } //This could be used to find "CreatedByUserId"
        public DateTime? CreatedOnDate { get; set; }

        [ColumnName("LastModifiedByUserID")]
        [JsonProperty(PropertyName = "LastModifiedByUserID")]
        public int? LastModifiedByUserId { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string LastModifiedByUserName { get; set; } //This could be used to find "LastModifiedByUserId"
        public DateTime? LastModifiedOnDate { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public Guid UniqueId { get; set; }
        public Guid VersionGuid { get; set; }

        [ColumnName("SHA1Hash")]
        [JsonProperty(PropertyName = "SHA1Hash")]
        public string Sha1Hash { get; set; }

        public DateTime LastModificationTime { get; set; }

        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public bool EnablePublishPeriod { get; set; }
        public DateTime? EndDate { get; set; }
        public int PublishedVersion { get; set; }

        [ColumnName("ContentItemID")]
        [JsonProperty(PropertyName = "ContentItemID")]
        public int? ContentItemId { get; set; }

        public bool HasBeenPublished { get; set; }
        public string Description { get; set; }


        [JsonIgnore]
        [IgnoreColumn]
        public int StorageLocation { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public bool IsCached { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        [ColumnName("FolderMappingID")]
        [JsonProperty(PropertyName = "FolderMappingID")]
        public int FolderMappingId { get; set; }

        public static void MapFile(ExportFile source, ExportFile target)
        {
            source.EnablePublishPeriod = target.EnablePublishPeriod;
            source.EndDate = target.EndDate;
            source.PublishedVersion = target.PublishedVersion;
            source.HasBeenPublished = target.HasBeenPublished;
            source.Description = target.Description;
            source.LastModificationTime = target.LastModificationTime;
            source.Sha1Hash = target.Sha1Hash;
            source.VersionGuid = target.VersionGuid;
            source.UniqueId = target.UniqueId;
            source.Size = target.Size;
            source.Extension = target.Extension;
            source.Width = target.Width;
            source.Height = target.Height;
            source.ContentType = target.ContentType;
            source.Content = target.Content;
        }
    }
}