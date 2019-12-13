using System;

namespace DotNetNuke.Services.Search
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke.Search.DataStore
    /// Class:      SearchCriteria
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SearchCriteria represents a search criterion
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Obsolete("Deprecated in DNN 7.1.  No longer used in the Search infrastructure.. Scheduled removal in v10.0.0.")]
    public class SearchCriteria
    {
        public string Criteria { get; set; }

        public bool MustExclude { get; set; }

        public bool MustInclude { get; set; }
    }
}
