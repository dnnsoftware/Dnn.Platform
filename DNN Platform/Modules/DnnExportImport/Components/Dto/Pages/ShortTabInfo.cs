using System;

namespace Dnn.ExportImport.Components.Dto.Pages
{
    public class ShortTabInfo
    {
        public int TabId { get; set; }
        public int ParentId { get; set; }
        public string TabName { get; set; }
        public string TabPath { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
    }
}