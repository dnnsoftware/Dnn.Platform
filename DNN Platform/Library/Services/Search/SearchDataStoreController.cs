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

    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke.Search.DataStore
    /// Class:      SearchDataStoreController
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SearchDataStoreController is the Business Controller class for SearchDataStore.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Obsolete("Deprecated in DNN 7.1.  No longer used in the Search infrastructure.. Scheduled removal in v10.0.0.")]
    public class SearchDataStoreController
    {
        [Obsolete("Deprecated in DNN 7.2.2  Implementation changed to return a NullInteger value. Scheduled removal in v10.0.0.")]
        public static int AddSearchItem(SearchItemInfo item)
        {
            return Null.NullInteger;
        }

        [Obsolete("Deprecated in DNN 7.2.2  Implementation changed to do nothing. Scheduled removal in v10.0.0.")]
        public static void DeleteSearchItem(int SearchItemId)
        {
        }

        [Obsolete("Deprecated in DNN 7.2.2  Implementation changed to do nothing. Scheduled removal in v10.0.0.")]
        public static void DeleteSearchItemWords(int SearchItemId)
        {
        }

        [Obsolete("Deprecated in DNN 7.1.2  Implementation changed to return empty result set. Scheduled removal in v10.0.0.")]
        public static SearchItemInfo GetSearchItem(int ModuleId, string SearchKey)
        {
            var empty = new SearchItemInfo();
            return empty;
        }

        [Obsolete("Deprecated in DNN 7.1.2  Implementation changed to return empty result set. Scheduled removal in v10.0.0.")]
        public static Dictionary<string, SearchItemInfo> GetSearchItems(int ModuleId)
        {
            var empty = new Dictionary<string, SearchItemInfo>();
            return empty;
        }

        [Obsolete("Deprecated in DNN 7.1.2  Implementation changed to return empty result set. Scheduled removal in v10.0.0.")]
        public static ArrayList GetSearchItems(int PortalId, int TabId, int ModuleId)
        {
            var empty = new ArrayList();
            return empty;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetSearchResults gets the search results for a single word.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="PortalID">A Id of the Portal.</param>
        /// <param name="Word">The word.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        [Obsolete("Deprecated in DNN 7.1.2  Implementation changed to return empty result set. Scheduled removal in v10.0.0.")]
        public static SearchResultsInfoCollection GetSearchResults(int PortalID, string Word)
        {
            var empty = new SearchResultsInfoCollection();
            return empty;
        }

        [Obsolete("Deprecated in DNN 7.1.2  Implementation changed to return empty result set. Scheduled removal in v10.0.0.")]
        public static SearchResultsInfoCollection GetSearchResults(int PortalId, int TabId, int ModuleId)
        {
            var empty = new SearchResultsInfoCollection();
            return empty;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetSearchSettings gets the search settings for a single module.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="ModuleId">The Id of the Module.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static Dictionary<string, string> GetSearchSettings(int ModuleId)
        {
            var dicSearchSettings = new Dictionary<string, string>();
            IDataReader dr = null;
            try
            {
                dr = DataProvider.Instance().GetSearchSettings(ModuleId);
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
