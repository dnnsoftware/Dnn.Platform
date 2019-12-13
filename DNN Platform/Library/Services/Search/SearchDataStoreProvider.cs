#region Usings

using System;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.ComponentModel;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;

#endregion

namespace DotNetNuke.Services.Search
{
    [Obsolete("Deprecated in DNN 7.1.  No longer used in the Search infrastructure.. Scheduled removal in v10.0.0.")]
    public abstract class SearchDataStoreProvider
    {
		#region "Shared/Static Methods"

        //return the provider
        public static SearchDataStoreProvider Instance()
        {
            return ComponentFactory.GetComponent<SearchDataStoreProvider>();
        }
		
		#endregion

		#region "Abstract Methods"

        public abstract void StoreSearchItems(SearchItemInfoCollection searchItems);

        public abstract SearchResultsInfoCollection GetSearchResults(int portalId, string criteria);

        public abstract SearchResultsInfoCollection GetSearchItems(int portalId, int tabId, int moduleId);
		
		#endregion
    }
}
