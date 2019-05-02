#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Search.Internals;

#endregion

namespace DotNetNuke.Services.Search
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke.Search.DataStore
    /// Class:      SearchDataStore
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SearchDataStore is an implementation of the abstract SearchDataStoreProvider
    /// class
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Obsolete("Deprecated in DNN 7.1.  No longer used in the Search infrastructure.. Scheduled removal in v10.0.0.")]
    public class SearchDataStore : SearchDataStoreProvider
    {
		#region Private Methods
        
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetCommonWords gets a list of the Common Words for the locale
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="locale">The locale string</param>
        /// <returns>A hashtable of common words</returns>
        /// -----------------------------------------------------------------------------
        private Hashtable GetCommonWords(string locale)
        {
            string strCacheKey = "CommonWords" + locale;
            var objWords = (Hashtable) DataCache.GetCache(strCacheKey);
            if (objWords == null)
            {
                objWords = new Hashtable();
                IDataReader drWords = DataProvider.Instance().GetSearchCommonWordsByLocale(locale);
                try
                {
                    while (drWords.Read())
                    {
                        objWords.Add(drWords["CommonWord"].ToString(), drWords["CommonWord"].ToString());
                    }
                }
                finally
                {
                    drWords.Close();
                    drWords.Dispose();
                }
                DataCache.SetCache(strCacheKey, objWords);
            }
            return objWords;
        }

        #endregion

		#region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetSearchItems gets a collection of Search Items for a Module/Tab/Portal
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">A Id of the Portal</param>
        /// <param name="tabId">A Id of the Tab</param>
        /// <param name="moduleId">A Id of the Module</param>
        /// -----------------------------------------------------------------------------
        public override SearchResultsInfoCollection GetSearchItems(int portalId, int tabId, int moduleId)
        {
            return SearchDataStoreController.GetSearchResults(portalId, tabId, moduleId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetSearchResults gets the search results for a passed in criteria string
        /// </summary>
        /// <remarks>
        /// </remarks>
		/// <param name="portalId">A Id of the Portal</param>
		/// <param name="criteria">The criteria string</param>
        /// -----------------------------------------------------------------------------
        public override SearchResultsInfoCollection GetSearchResults(int portalId, string criteria)
        {
            bool hasExcluded = Null.NullBoolean;
            bool hasMandatory = Null.NullBoolean;

            var portal = PortalController.Instance.GetPortal(portalId);

            //Get the Settings for this Portal
            var portalSettings = new PortalSettings(portal);

            //We will assume that the content is in the locale of the Portal
            Hashtable commonWords = GetCommonWords(portalSettings.DefaultLanguage);

            //clean criteria
            criteria = criteria.ToLowerInvariant();

            //split search criteria into words
            var searchWords = new SearchCriteriaCollection(criteria);

            var searchResults = new Dictionary<string, SearchResultsInfoCollection>();

            //dicResults is a Dictionary(Of SearchItemID, Dictionary(Of TabID, SearchResultsInfo)
            var dicResults = new Dictionary<int, Dictionary<int, SearchResultsInfo>>();

            //iterate through search criteria words
            foreach (SearchCriteria criterion in searchWords)
            {
                if (commonWords.ContainsKey(criterion.Criteria) == false || portalSettings.SearchIncludeCommon)
                {
                    if (!searchResults.ContainsKey(criterion.Criteria))
                    {
                        searchResults.Add(criterion.Criteria, SearchDataStoreController.GetSearchResults(portalId, criterion.Criteria));
                    }
                    if (searchResults.ContainsKey(criterion.Criteria))
                    {
                        foreach (SearchResultsInfo result in searchResults[criterion.Criteria])
                        {
							//Add results to dicResults
                            if (!criterion.MustExclude)
                            {
                                if (dicResults.ContainsKey(result.SearchItemID))
                                {
                                    //The Dictionary exists for this SearchItemID already so look in the TabId keyed Sub-Dictionary
                                    Dictionary<int, SearchResultsInfo> dic = dicResults[result.SearchItemID];
                                    if (dic.ContainsKey(result.TabId))
                                    {
                                        //The sub-Dictionary contains the item already so update the relevance
                                        SearchResultsInfo searchResult = dic[result.TabId];
                                        searchResult.Relevance += result.Relevance;
                                    }
                                    else
                                    {
										//Add Entry to Sub-Dictionary
                                        dic.Add(result.TabId, result);
                                    }
                                }
                                else
                                {
									//Create new TabId keyed Dictionary
                                    var dic = new Dictionary<int, SearchResultsInfo>();
                                    dic.Add(result.TabId, result);

                                    //Add new Dictionary to SearchResults
                                    dicResults.Add(result.SearchItemID, dic);
                                }
                            }
                        }
                    }
                }
            }
            foreach (SearchCriteria criterion in searchWords)
            {
                var mandatoryResults = new Dictionary<int, bool>();
                var excludedResults = new Dictionary<int, bool>();
                if (searchResults.ContainsKey(criterion.Criteria))
                {
                    foreach (SearchResultsInfo result in searchResults[criterion.Criteria])
                    {
                        if (criterion.MustInclude)
                        {
							//Add to mandatory results lookup
                            mandatoryResults[result.SearchItemID] = true;
                            hasMandatory = true;
                        }
                        else if (criterion.MustExclude)
                        {
							//Add to exclude results lookup
                            excludedResults[result.SearchItemID] = true;
                            hasExcluded = true;
                        }
                    }
                }
                foreach (KeyValuePair<int, Dictionary<int, SearchResultsInfo>> kvpResults in dicResults)
                {
                    //The key of this collection is the SearchItemID,  Check if the value of this collection should be processed
                    if (hasMandatory && (!mandatoryResults.ContainsKey(kvpResults.Key)))
                    {
                        //1. If mandatoryResults exist then only process if in mandatoryResults Collection
                        foreach (SearchResultsInfo result in kvpResults.Value.Values)
                        {
                            result.Delete = true;
                        }
                    }
                    else if (hasExcluded && (excludedResults.ContainsKey(kvpResults.Key)))
                    {
                        //2. Do not process results in the excludedResults Collection
                        foreach (SearchResultsInfo result in kvpResults.Value.Values)
                        {
                            result.Delete = true;
                        }
                    }
                }
            }
			
            //Process results against permissions and mandatory and excluded results
            var results = new SearchResultsInfoCollection();
            foreach (KeyValuePair<int, Dictionary<int, SearchResultsInfo>> kvpResults in dicResults)
            {
                foreach (SearchResultsInfo result in kvpResults.Value.Values)
                {
                    if (!result.Delete)
                    {
						//Check If authorised to View Tab
                        TabInfo objTab = TabController.Instance.GetTab(result.TabId, portalId, false);
                        if (TabPermissionController.CanViewPage(objTab))
                        {
							//Check If authorised to View Module
                            ModuleInfo objModule = ModuleController.Instance.GetModule(result.ModuleId, result.TabId, false);
                            if (ModulePermissionController.CanViewModule(objModule))
                            {
                                results.Add(result);
                            }
                        }
                    }
                }
            }
			
            //Return Search Results Collection
            return results;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// StoreSearchItems adds the Search Item to the Data Store
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="searchItems">A Collection of SearchItems</param>
        public override void StoreSearchItems(SearchItemInfoCollection searchItems)
        {
            var indexer = new ModuleIndexer();
            
            var modulesDic = new Dictionary<int, string>();
            foreach (SearchItemInfo item in searchItems)
            {                
                if (!modulesDic.ContainsKey(item.ModuleId))
                {
                    var module = ModuleController.Instance.GetModule(item.ModuleId, Null.NullInteger, true);
                    modulesDic.Add(item.ModuleId, module.CultureCode);
                    
                    //Remove all indexed items for this module
                    InternalSearchController.Instance.DeleteSearchDocumentsByModule(module.PortalID, module.ModuleID, module.ModuleDefID);
                }
            }
          
            //Process the SearchItems by Module to reduce Database hits
            foreach (var kvp in modulesDic)
            {
                //Get the Module's SearchItems
                var moduleSearchItems = searchItems.ModuleItems(kvp.Key);
                
                //Convert SearchItemInfo objects to SearchDocument objects
                var searchDocuments = (from SearchItemInfo item in moduleSearchItems select indexer.ConvertSearchItemInfoToSearchDocument(item)).ToList();
                
                //Index
                InternalSearchController.Instance.AddSearchDocuments(searchDocuments);                
            }
        } 

        #endregion

    }
}
