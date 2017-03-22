using System;

namespace Dnn.ExportImport.Components.Dto.Pages
{
    public class ExportTabInfo
    {
        public int TabId { get; set; }
        public int ParentId { get; set; }
        public string TabName { get; set; }
        public string TabPath { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string CreatedByUserName { get; set; }
        public string LastModifiedByUserName { get; set; }
    }
}