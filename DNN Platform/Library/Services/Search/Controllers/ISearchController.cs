#region Usings

using System;

using DotNetNuke.Services.Search.Entities;

#endregion

namespace DotNetNuke.Services.Search.Controllers
{
    public interface ISearchController
    {
        #region Core Search APIs

        /// <summary>
        /// Get Search Result for the searchQuery at the Site Level
        /// </summary>
        /// <param name="searchQuery">SearchQuery object with various search criteria</param>
        /// <returns>SearchResults</returns>
        SearchResults SiteSearch(SearchQuery searchQuery);

        /// <summary>
        /// Get Search Result for the searchQuery at the Module Level
        /// </summary>
        /// <param name="searchQuery">SearchQuery object with various search criteria</param>
        /// <returns>SearchResults</returns>
        /// <remarks>SearchTypeIds provided in the searchQuery will be ignored</remarks>
        SearchResults ModuleSearch(SearchQuery searchQuery);

        #endregion
    }
}
