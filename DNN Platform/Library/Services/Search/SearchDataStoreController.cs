// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Internal.SourceGenerators;

/// <summary>The SearchDataStoreController is the Business Controller class for SearchDataStore.</summary>
[DnnDeprecated(7, 1, 0, "No longer used in the Search infrastructure", RemovalVersion = 10)]
public partial class SearchDataStoreController
{
    [DnnDeprecated(7, 2, 2, "Implementation changed to return a NullInteger value", RemovalVersion = 10)]
    public static partial int AddSearchItem(SearchItemInfo item)
    {
        return Null.NullInteger;
    }

    [DnnDeprecated(7, 2, 2, "Implementation changed to do nothing", RemovalVersion = 10)]
    public static partial void DeleteSearchItem(int searchItemId)
    {
    }

    [DnnDeprecated(7, 2, 2, "Implementation changed to do nothing", RemovalVersion = 10)]
    public static partial void DeleteSearchItemWords(int searchItemId)
    {
    }

    /// <returns>An empty <see cref="SearchItemInfo"/>.</returns>
    [DnnDeprecated(7, 2, 2, "Implementation changed to return empty result set", RemovalVersion = 10)]
    public static partial SearchItemInfo GetSearchItem(int moduleId, string searchKey)
    {
        var empty = new SearchItemInfo();
        return empty;
    }

    /// <returns>An empty <see cref="Dictionary{TKey,TValue}"/>.</returns>
    [DnnDeprecated(7, 2, 2, "Implementation changed to return empty result set", RemovalVersion = 10)]
    public static partial Dictionary<string, SearchItemInfo> GetSearchItems(int moduleId)
    {
        var empty = new Dictionary<string, SearchItemInfo>();
        return empty;
    }

    /// <returns>An empty <see cref="ArrayList"/>.</returns>
    [DnnDeprecated(7, 2, 2, "Implementation changed to return empty result set", RemovalVersion = 10)]
    public static partial ArrayList GetSearchItems(int portalId, int tabId, int moduleId)
    {
        var empty = new ArrayList();
        return empty;
    }

    /// <summary>GetSearchResults gets the search results for a single word.</summary>
    /// <param name="portalID">A Id of the Portal.</param>
    /// <param name="word">The word.</param>
    /// <returns>An empty <see cref="SearchResultsInfoCollection"/>.</returns>
    [DnnDeprecated(7, 2, 2, "Implementation changed to return empty result set", RemovalVersion = 10)]
    public static partial SearchResultsInfoCollection GetSearchResults(int portalID, string word)
    {
        var empty = new SearchResultsInfoCollection();
        return empty;
    }

    /// <returns>An empty <see cref="SearchResultsInfoCollection"/>.</returns>
    [DnnDeprecated(7, 2, 2, "Implementation changed to return empty result set", RemovalVersion = 10)]
    public static partial SearchResultsInfoCollection GetSearchResults(int portalId, int tabId, int moduleId)
    {
        var empty = new SearchResultsInfoCollection();
        return empty;
    }

    /// <summary>GetSearchSettings gets the search settings for a single module.</summary>
    /// <param name="moduleId">The Id of the Module.</param>
    /// <returns>A <see cref="Dictionary{TKey,TValue}"/> of settings (<c>NULL</c> values are coerced to <see cref="string.Empty"/>).</returns>
    public static Dictionary<string, string> GetSearchSettings(int moduleId)
    {
        var dicSearchSettings = new Dictionary<string, string>();
        IDataReader dr = null;
        try
        {
            dr = DataProvider.Instance().GetSearchSettings(moduleId);
            while (dr.Read())
            {
                if (!dr.IsDBNull(1))
                {
                    dicSearchSettings[dr.GetString(0)] = dr.GetString(1);
                }
                else
                {
                    dicSearchSettings[dr.GetString(0)] = string.Empty;
                }
            }
        }
        catch (Exception ex)
        {
            Exceptions.Exceptions.LogException(ex);
        }
        finally
        {
            CBO.CloseDataReader(dr, true);
        }

        return dicSearchSettings;
    }

    [DnnDeprecated(7, 2, 2, "Implementation changed to do nothing", RemovalVersion = 10)]
    public static partial void UpdateSearchItem(SearchItemInfo item)
    {
    }
}
