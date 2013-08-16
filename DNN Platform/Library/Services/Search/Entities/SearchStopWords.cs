using System;

namespace DotNetNuke.Services.Search.Entities
{
    [Serializable]
    public class SearchStopWords
    {
        public int StopWordsId { get; set; }
        public string StopWords { get; set; }
        public int CreatedByUserId { get; set; }
        public int LastModifiedByUserId { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int PortalId { get; set; }
        public string CultureCode { get; set; }
    }
}
