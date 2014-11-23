using System.Collections.Generic;

namespace DotNetNuke.Services.Search.Internals
{
    internal class LuceneResults
    {
        public IEnumerable<LuceneResult> Results { get; set; }
        public int TotalHits { get; set; }

        public LuceneResults()
        {
            Results = new List<LuceneResult>();
        }
    }
}
