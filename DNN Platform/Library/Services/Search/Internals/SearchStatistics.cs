using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetNuke.Services.Search.Internals
{
    public class SearchStatistics
    {
        public int TotalActiveDocuments { get; set; }
        public int TotalDeletedDocuments { get; set; }
        public string IndexLocation { get; set; }
        public DateTime LastModifiedOn { get; set; }
        public long IndexDbSize { get; set; }
    }
}