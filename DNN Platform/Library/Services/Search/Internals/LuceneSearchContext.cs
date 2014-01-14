using DotNetNuke.Services.Search.Entities;

namespace DotNetNuke.Services.Search.Internals
{
    internal class LuceneSearchContext
    {
        public LuceneQuery LuceneQuery { get; set; }
        public SearchQuery SearchQuery { get; set; }
        public SecurityCheckerDelegate SecurityCheckerDelegate { get; set; }
    }
}
