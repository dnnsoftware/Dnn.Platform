using System;

namespace Dnn.ExportImport.Dto.Assets
{
    public class ExportFolderMapping: BasicExportImportDto
    {
        public int FolderMappingId { get; set; }

        public string MappingName { get; set; }
        public string FolderProviderType { get; set; }
        public int? Priority { get; set; }

        public int? CreatedByUserId { get; set; }

        public string CreatedByUserName { get; set; } //This could be used to find "CreatedByUserId"
        public DateTime? CreatedOnDate { get; set; }

        public int? LastModifiedByUserId { get; set; }

        public string LastModifiedByUserName { get; set; } //This could be used to find "LastModifiedByUserId"
        public DateTime? LastModifiedOnDate { get; set; }
    }
}
