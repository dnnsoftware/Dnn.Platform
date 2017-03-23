using System;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Dnn.ExportImport.Components.Dto.Pages
{
    [JsonObject]
    public class ExportTabAliasSkin : BasicExportImportDto
    {
        public int TabAliasSkinId { get; set; }
        public int TabId { get; set; }
        public int PortalAliasId { get; set; }
        public string SkinSrc { get; set; }
        public int? CreatedByUserID { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public int? LastModifiedByUserID { get; set; }
        public DateTime? LastModifiedOnDate { get; set; }

        public string HTTPAlias { get; set; }
        public string CreatedByUserName { get; set; }
        public string LastModifiedByUserName { get; set; }
    }
}