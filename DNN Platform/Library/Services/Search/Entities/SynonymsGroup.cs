using System;

namespace DotNetNuke.Services.Search.Entities
{
    [Serializable]
    public class SynonymsGroup
    {
        public int SynonymsGroupId { get; set; }
        public string SynonymsTags { get; set; }
        public int CreatedByUserId { get; set; }
        public int LastModifiedByUserId { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int PortalId { get; set; }
    }
}
