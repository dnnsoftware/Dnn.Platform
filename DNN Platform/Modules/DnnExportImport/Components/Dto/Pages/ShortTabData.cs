using System;

namespace Dnn.ExportImport.Components.Dto.Pages
{
    public class ShortTabData
    {
        public int TabId { get; set; }
        public int? ParentId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
    }
}