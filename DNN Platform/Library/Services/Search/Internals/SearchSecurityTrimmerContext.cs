using DotNetNuke.Services.Search.Entities;
using Lucene.Net.Search;

namespace DotNetNuke.Services.Search.Internals
{
    internal class SearchSecurityTrimmerContext
    {
        public IndexSearcher Searcher { get; set; }
        public SecurityCheckerDelegate SecurityChecker { get; set; }
        public LuceneQuery LuceneQuery { get; set; }
        public SearchQuery SearchQuery { get; set; }
    }
}
