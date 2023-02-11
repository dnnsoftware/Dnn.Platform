// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;

    /// <summary>The SearchDataStoreController is the Business Controller class for SearchDataStore.</summary>
    [Obsolete("Deprecated in DNN 7.1.  No longer used in the Search infrastructure.. Scheduled removal in v10.0.0.")]
    public class SearchDataStoreController
    {
        [Obsolete("Deprecated in DNN 7.2.2  Implementation changed to return a NullInteger value. Scheduled removal in v10.0.0.")]
        public static int AddSearchItem(SearchItemInfo item)
        {
            return Null.NullInteger;
        }

        [Obsolete("Deprecated in DNN 7.2.2  Implementation changed to do nothing. Scheduled removal in v10.0.0.")]
        public static void DeleteSearchItem(int searchItemId)
        {
        }

        [Obsolete("Deprecated in DNN 7.2.2  Implementation changed to do nothing. Scheduled removal in v10.0.0.")]
        public static void DeleteSearchItemWords(int searchItemId)
        {
        }

        /// <returns>An empty <see cref="SearchItemInfo"/>.</returns>
        [Obsolete("Deprecated in DNN 7.1.2  Implementation changed to return empty result set. Scheduled removal in v10.0.0.")]
        public static SearchItemInfo GetSearchItem(int moduleId, string searchKey)
        {
            var empty = new SearchItemInfo();
            return empty;
        }

        /// <returns>An empty <see cref="Dictionary{TKey,TValue}"/>.</returns>
        [Obsolete("Deprecated in DNN 7.1.2  Implementation changed to return empty result set. Scheduled removal in v10.0.0.")]
        public static Dictionary<string, SearchItemInfo> GetSearchItems(int moduleId)
        {
            var empty = new Dictionary<string, SearchItemInfo>();
            return empty;
        }

        /// <returns>An empty <see cref="ArrayList"/>.</returns>
        [Obsolete("Deprecated in DNN 7.1.2  Implementation changed to return empty result set. Scheduled removal in v10.0.0.")]
        public static ArrayList GetSearchItems(int portalId, int tabId, int moduleId)
        {
            var empty = new ArrayList();
            return empty;
        }

        /// <summary>GetSearchResults gets the search results for a single word.</summary>
        /// <param name="portalID">A Id of the Portal.</param>
        /// <param name="word">The word.</param>
        /// <returns>An empty <see cref="SearchResultsInfoCollection"/>.</returns>
        [Obsolete("Deprecated in DNN 7.1.2  Implementation changed to return empty result set. Scheduled removal in v10.0.0.")]
        public static SearchResultsInfoCollection GetSearchResults(int portalID, string word)
        {
            var empty = new SearchResultsInfoCollection();
            return empty;
        }

        /// <returns>An empty <see cref="SearchResultsInfoCollection"/>.</returns>
        [Obsolete("Deprecated in DNN 7.1.2  Implementation changed to return empty result set. Scheduled removal in v10.0.0.")]
        public static SearchResultsInfoCollection GetSearchResults(int portalId, int tabId, int moduleId)
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

        [Obsolete("Deprecated in DNN 7.2.2  Implementation changed to do nothing. Scheduled removal in v10.0.0.")]
        public static void UpdateSearchItem(SearchItemInfo item)
        {
        }
    }
}
