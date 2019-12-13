using System;

namespace Dnn.ExportImport.Dto.Users
{
    public class ExportUserProfile : BasicExportImportDto
    {
        public int ProfileId { get; set; }

        public int UserId { get; set; }

        public int PropertyDefinitionId { get; set; }

        public string PropertyName { get; set; }

        public string PropertyValue { get; set; }
        public string PropertyText { get; set; }
        public int Visibility { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public string ExtendedVisibility { get; set; }
    }
}
