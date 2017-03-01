using System;

namespace Dnn.ExportImport.Components.Dto.Users
{
    public class AspnetUsers : BasicExportImportDto
    {
        public Guid ApplicationId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string LoweredUserName { get; set; }
        public string MobileAlias { get; set; }
        public bool IsAnonymous { get; set; }
        public DateTime? LastActivityDate { get; set; }
    }
}
