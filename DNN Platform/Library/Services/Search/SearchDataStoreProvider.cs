// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;

    [Obsolete("Deprecated in DNN 7.1.  No longer used in the Search infrastructure.. Scheduled removal in v10.0.0.")]
    public abstract class SearchDataStoreProvider
    {
        // return the provider
        public static SearchDataStoreProvider Instance()
        {
            return ComponentFactory.GetComponent<SearchDataStoreProvider>();
        }

        public abstract void StoreSearchItems(SearchItemInfoCollection searchItems);

        public abstract SearchResultsInfoCollection GetSearchResults(int portalId, string criteria);

        public abstract SearchResultsInfoCollection GetSearchItems(int portalId, int tabId, int moduleId);
    }
}
