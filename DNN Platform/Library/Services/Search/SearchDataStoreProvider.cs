// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search;

using DotNetNuke.ComponentModel;
using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(7, 1, 0, "No longer used in the Search infrastructure", RemovalVersion = 10)]
public abstract partial class SearchDataStoreProvider
{
    // return the provider
    public static SearchDataStoreProvider Instance()
    {
        return ComponentFactory.GetComponent<SearchDataStoreProvider>();
    }

    /// <summary>StoreSearchItems adds the Search Item to the Data Store.</summary>
    /// <param name="searchItems">A Collection of SearchItems.</param>
    public abstract void StoreSearchItems(SearchItemInfoCollection searchItems);

    /// <summary>GetSearchResults gets the search results for a passed in criteria string.</summary>
    /// <param name="portalId">A Id of the Portal.</param>
    /// <param name="criteria">The criteria string.</param>
    /// <returns>A <see cref="SearchResultsInfoCollection"/>.</returns>
    public abstract SearchResultsInfoCollection GetSearchResults(int portalId, string criteria);

    /// <summary>GetSearchItems gets a collection of Search Items for a Module/Tab/Portal.</summary>
    /// <param name="portalId">A Id of the Portal.</param>
    /// <param name="tabId">A Id of the Tab.</param>
    /// <param name="moduleId">A Id of the Module.</param>
    /// <returns>A <see cref="SearchResultsInfoCollection"/>.</returns>
    public abstract SearchResultsInfoCollection GetSearchItems(int portalId, int tabId, int moduleId);
}
