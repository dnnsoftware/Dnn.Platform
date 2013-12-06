#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;

#endregion

namespace DotNetNuke.Services.Search
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke.Search.DataStore
    /// Class:      SearchDataStoreController
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SearchDataStoreController is the Business Controller class for SearchDataStore
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    ///		[cnurse]	11/15/2004	documented
    /// </history>
    /// -----------------------------------------------------------------------------
    [Obsolete("Deprecated in DNN 7.1.  No longer used in the Search infrastructure.")]
    public class SearchDataStoreController
    {
        public static int AddSearchItem(SearchItemInfo item)
        {
            return DataProvider.Instance().AddSearchItem(item.Title, item.Description, item.Author, item.PubDate, item.ModuleId, item.SearchKey, item.GUID, item.ImageFileId);
        }

        public static void DeleteSearchItem(int SearchItemId)
        {
            DataProvider.Instance().DeleteSearchItem(SearchItemId);
        }

        public static void DeleteSearchItemWords(int SearchItemId)
        {
            DataProvider.Instance().DeleteSearchItemWords(SearchItemId);
        }

        [Obsolete("Deprecated in DNN 7.1.2  Implementation changed to return empty result set")]
        public static SearchItemInfo GetSearchItem(int ModuleId, string SearchKey)
        {
            var empty=new SearchItemInfo();
            return empty;
        }

        [Obsolete("Deprecated in DNN 7.1.2  Implementation changed to return empty result set")]
        public static Dictionary<string, SearchItemInfo> GetSearchItems(int ModuleId)
        {
            var empty = new Dictionary<string, SearchItemInfo>();
            return empty;
        }

        [Obsolete("Deprecated in DNN 7.1.2  Implementation changed to return empty result set")]
        public static ArrayList GetSearchItems(int PortalId, int TabId, int ModuleId)
        {
            var empty = new ArrayList();
            return empty;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetSearchResults gets the search results for a single word
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="PortalID">A Id of the Portal</param>
        /// <param name="Word">The word</param>
        /// <history>
        ///		[cnurse]	11/15/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [Obsolete("Deprecated in DNN 7.1.2  Implementation changed to return empty result set")]
        public static SearchResultsInfoCollection GetSearchResults(int PortalID, string Word)
        {
            var empty = new SearchResultsInfoCollection();
            return empty;
        }

        [Obsolete("Deprecated in DNN 7.1.2  Implementation changed to return empty result set")]
        public static SearchResultsInfoCollection GetSearchResults(int PortalId, int TabId, int ModuleId)
        {
            var empty = new SearchResultsInfoCollection();
            return empty;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetSearchSettings gets the search settings for a single module
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="ModuleId">The Id of the Module</param>
        /// <history>
        ///		[cnurse]	11/15/2004	created
        /// </history>
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
                        dicSearchSettings[dr.GetString(0)] = "";
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

        public static void UpdateSearchItem(SearchItemInfo item)
        {
            DataProvider.Instance().UpdateSearchItem(item.SearchItemId,
                                                     item.Title,
                                                     item.Description,
                                                     item.Author,
                                                     item.PubDate,
                                                     item.ModuleId,
                                                     item.SearchKey,
                                                     item.GUID,
                                                     item.HitCount,
                                                     item.ImageFileId);
        }
    }
}
