#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Data.SqlTypes;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;

#endregion

namespace DotNetNuke.Services.Search
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke.Search.Index
    /// Class:      ModuleIndexer
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ModuleIndexer is an implementation of the abstract IndexingProvider
    /// class
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ModuleIndexer : IndexingProvider
    {
        #region Private Fields

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ModuleIndexer));
        private static readonly int ModuleSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId;

		private readonly IDictionary<int, IEnumerable<ModuleIndexInfo>> _searchModules;

        #endregion

		#region Constructors

	    public ModuleIndexer() : this(false)
	    {
	    }

		public ModuleIndexer(bool needSearchModules)
		{
			_searchModules = new Dictionary<int, IEnumerable<ModuleIndexInfo>>();

			if (needSearchModules)
			{
				var portals = PortalController.Instance.GetPortals();
				foreach (var portal in portals.Cast<PortalInfo>())
				{
					_searchModules.Add(portal.PortalID, GetModulesForIndex(portal.PortalID));
				}

				_searchModules.Add(Null.NullInteger, GetModulesForIndex(Null.NullInteger));
			}
		}

		#endregion

		#region Public Methods

		/// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns the number of indexed SearchDocuments for the portal.
        /// </summary>
        /// <remarks>This replaces "GetSearchIndexItems" as a newer implementation of search.</remarks>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public override int IndexSearchDocuments(int portalId,
            ScheduleHistoryItem schedule, DateTime startDateLocal, Action<IEnumerable<SearchDocument>> indexer)
        {
            Requires.NotNull("indexer", indexer);
            const int saveThreshold = 1024 * 2;
            var totalIndexed = 0;
            startDateLocal = GetLocalTimeOfLastIndexedItem(portalId, schedule.ScheduleID, startDateLocal);
            var searchDocuments = new List<SearchDocument>();
			var searchModuleCollection = _searchModules.ContainsKey(portalId)
                ? _searchModules[portalId].Where(m => m.SupportSearch).Select(m => m.ModuleInfo)
                : GetSearchModules(portalId);

            //Some modules update LastContentModifiedOnDate (e.g. Html module) when their content changes.
            //We won't be calling into such modules if LastContentModifiedOnDate is prior to startDate.
            //LastContentModifiedOnDate remains MinValue for modules that don't update this property
            var modulesInDateRange = searchModuleCollection.Where(module =>
                !((SqlDateTime.MinValue.Value < module.LastContentModifiedOnDate && module.LastContentModifiedOnDate < startDateLocal)))
                .OrderBy(m => m.LastContentModifiedOnDate).ThenBy(m => m.ModuleID).ToArray();

            if (modulesInDateRange.Any())
            {
                foreach (var module in modulesInDateRange)
                {
                    try
                    {
                        var controller = Reflection.CreateObject(module.DesktopModule.BusinessControllerClass, module.DesktopModule.BusinessControllerClass);
                        var contentInfo = new SearchContentModuleInfo {ModSearchBaseControllerType= (ModuleSearchBase) controller, ModInfo = module};
                        var searchItems = contentInfo.ModSearchBaseControllerType.GetModifiedSearchDocuments(module, startDateLocal.ToUniversalTime());

                        if (searchItems != null && searchItems.Count > 0)
                        {
                            AddModuleMetaData(searchItems, module);
                            searchDocuments.AddRange(searchItems);

                            if (Logger.IsTraceEnabled)
                            {
                                Logger.TraceFormat("ModuleIndexer: {0} search documents found for module [{1} mid:{2}]",
                                    searchItems.Count, module.DesktopModule.ModuleName, module.ModuleID);
                            }

                            if (searchDocuments.Count >= saveThreshold)
                            {
                                totalIndexed += IndexCollectedDocs(indexer, searchDocuments, portalId, schedule);
                                searchDocuments.Clear();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Exceptions.Exceptions.LogException(ex);
                    }
                }

                if (searchDocuments.Count > 0)
                {
                    totalIndexed += IndexCollectedDocs(indexer, searchDocuments, portalId, schedule);
                }
            }

            return totalIndexed;
        }

        private static void AddModuleMetaData(IEnumerable<SearchDocument> searchItems, ModuleInfo module)
        {
            foreach (var searchItem in searchItems)
            {
                searchItem.ModuleDefId = module.ModuleDefID;
                searchItem.ModuleId = module.ModuleID;
                if (string.IsNullOrEmpty(searchItem.CultureCode))
                {
                    searchItem.CultureCode = module.CultureCode;
                }

                if (Null.IsNull(searchItem.ModifiedTimeUtc))
                {
                    searchItem.ModifiedTimeUtc = module.LastContentModifiedOnDate.ToUniversalTime();
                }
            }
        }

        private int IndexCollectedDocs(
            Action<IEnumerable<SearchDocument>> indexer, ICollection<SearchDocument> searchDocuments, int portalId, ScheduleHistoryItem schedule)
        {
            indexer.Invoke(searchDocuments);
            var total = searchDocuments.Count;
            SetLocalTimeOfLastIndexedItem(portalId, schedule.ScheduleID, schedule.StartDate);
            return total;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns a collection of SearchDocuments containing module metadata (title, header, footer...) of Searchable Modules.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public List<SearchDocument> GetModuleMetaData(int portalId, DateTime startDate)
        {
            var searchDocuments = new List<SearchDocument>();
			var searchModuleCollection = _searchModules.ContainsKey(portalId) ? 
											_searchModules[portalId].Select(m => m.ModuleInfo) : GetSearchModules(portalId, true);
            foreach (ModuleInfo module in searchModuleCollection)
            {
                try
                {
                    if (module.LastModifiedOnDate > startDate && module.LastModifiedOnDate < DateTime.Now)
                    {
                        var searchDoc = new SearchDocument
                        {
                            SearchTypeId = ModuleSearchTypeId,
                            UniqueKey = Constants.ModuleMetaDataPrefixTag + module.ModuleID,
                            ModuleDefId = module.ModuleDefID,
                            ModuleId = module.ModuleID,
                            Title = module.ModuleTitle,
                            PortalId = portalId,
                            CultureCode = module.CultureCode,
                            ModifiedTimeUtc = module.LastModifiedOnDate.ToUniversalTime(),
                            Body = module.Header + " " + module.Footer
                        };

                        if (module.Terms != null && module.Terms.Count > 0)
                        {
                            searchDoc.Tags = module.Terms.Select(t => t.Name);
                        }

                        Logger.Trace("ModuleIndexer: Search document for metaData found for module [" + module.DesktopModule.ModuleName + " mid:" + module.ModuleID + "]");

                        searchDocuments.Add(searchDoc);
                    }
                }
                catch (Exception ex)
                {
                    Exceptions.Exceptions.LogException(ex);
                }
            }

            return searchDocuments;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Converts a SearchItemInfo into a SearchDocument.
        /// 
        /// SearchItemInfo object was used in the old version of search.
        /// </summary>
        /// <param name="searchItem"></param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        #pragma warning disable 0618
        public SearchDocument ConvertSearchItemInfoToSearchDocument(SearchItemInfo searchItem)
        {
            var module = ModuleController.Instance.GetModule(searchItem.ModuleId, Null.NullInteger, true);

            var searchDoc = new SearchDocument
            {
                // Assigns as a Search key the SearchItems' GUID, if not it creates a dummy guid.
                UniqueKey = (searchItem.SearchKey.Trim() != string.Empty) ? searchItem.SearchKey : Guid.NewGuid().ToString(),
                QueryString = searchItem.GUID,
                Title = searchItem.Title,
                Body = searchItem.Content,
                Description = searchItem.Description,
                ModifiedTimeUtc = searchItem.PubDate,
                AuthorUserId = searchItem.Author,
                TabId = searchItem.TabId,
                PortalId = module.PortalID,
                SearchTypeId = ModuleSearchTypeId,
                CultureCode = module.CultureCode,
                //Add Module MetaData
                ModuleDefId = module.ModuleDefID,
                ModuleId = module.ModuleID
            };

            return searchDoc;
        }
        #pragma warning restore 0618

        #endregion

        #region Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of modules that are listed as "Searchable" from the module definition and check if they
        /// implement ModuleSearchBase -- which is a newer implementation of search that replaces ISearchable
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        protected IEnumerable<ModuleInfo> GetSearchModules(int portalId)
        {
            return GetSearchModules(portalId, false);
        }

		protected IEnumerable<ModuleInfo> GetSearchModules(int portalId, bool allModules)
		{
			return from mii in GetModulesForIndex(portalId)
				where allModules || mii.SupportSearch
				select mii.ModuleInfo;
		}


        #endregion

        #region Obsolete Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LEGACY: Deprecated in DNN 7.1. Use 'IndexSearchDocuments' instead.
        /// Used for Legacy Search (ISearchable) 
        /// 
        /// GetSearchIndexItems gets the SearchInfo Items for the Portal
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// -----------------------------------------------------------------------------
        [Obsolete("Legacy Search (ISearchable) -- Deprecated in DNN 7.1. Use 'IndexSearchDocuments' instead.")]
        public override SearchItemInfoCollection GetSearchIndexItems(int portalId)
        {
            var searchItems = new SearchItemInfoCollection();
            var searchCollection = GetModuleList(portalId);
            foreach (SearchContentModuleInfo scModInfo in searchCollection)
            {
                try
                {
                    var myCollection = scModInfo.ModControllerType.GetSearchItems(scModInfo.ModInfo);
                    if (myCollection != null)
                    {
                        foreach (SearchItemInfo searchItem in myCollection)
                        {
                            searchItem.TabId = scModInfo.ModInfo.TabID;
                        }

                        Logger.Trace("ModuleIndexer: " + myCollection.Count + " search documents found for module [" + scModInfo.ModInfo.DesktopModule.ModuleName + " mid:" + scModInfo.ModInfo.ModuleID + "]");

                        searchItems.AddRange(myCollection);
                    }
                }
                catch (Exception ex)
                {
                    Exceptions.Exceptions.LogException(ex);
                }
            }
            return searchItems;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LEGACY: Deprecated in DNN 7.1. Use 'GetSearchModules' instead.
        /// Used for Legacy Search (ISearchable) 
        /// 
        /// GetModuleList gets a collection of SearchContentModuleInfo Items for the Portal
        /// </summary>
        /// <remarks>
        /// Parses the Modules of the Portal, determining whetehr they are searchable.
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// -----------------------------------------------------------------------------
        [Obsolete("Legacy Search (ISearchable) -- Deprecated in DNN 7.1. Use 'GetSearchModules' instead.")]
        protected SearchContentModuleInfoCollection GetModuleList(int portalId)
        {
            var results = new SearchContentModuleInfoCollection();
            var arrModules = ModuleController.Instance.GetSearchModules(portalId);
            var businessControllers = new Hashtable();
            var htModules = new Hashtable();
            
            foreach (var module in arrModules.Cast<ModuleInfo>().Where(module => !htModules.ContainsKey(module.ModuleID)))
            {
                try
                {
                    //Check if the business controller is in the Hashtable
                    var controller = businessControllers[module.DesktopModule.BusinessControllerClass];
                    if (!String.IsNullOrEmpty(module.DesktopModule.BusinessControllerClass))
                    {
                        //If nothing create a new instance
                        if (controller == null)
                        {
                            //Add to hashtable
                            controller = Reflection.CreateObject(module.DesktopModule.BusinessControllerClass, module.DesktopModule.BusinessControllerClass);                              
                            businessControllers.Add(module.DesktopModule.BusinessControllerClass, controller);
                        }                            
                        //Double-Check that module supports ISearchable

                        //Check if module inherits from ModuleSearchBase                        
                        if (controller is ISearchable && !(controller is ModuleSearchBase))
                        {
                            var contentInfo = new SearchContentModuleInfo {ModControllerType = (ISearchable) controller, ModInfo = module};
                            results.Add(contentInfo);
                        }                           
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    ThrowLogError(module, ex);
                }
                finally
                {
                    htModules.Add(module.ModuleID, module.ModuleID);
                }
            }
            return results;
        }

        #endregion

        #region Private Methods

        private static void ThrowLogError(ModuleInfo module, Exception ex)
        {
            try
            {
                var message = string.Format(
                        Localization.Localization.GetExceptionMessage("ErrorCreatingBusinessControllerClass",
                            "Error Creating BusinessControllerClass '{0}' of module({1}) id=({2}) in tab({3}) and portal({4})"),
                        module.DesktopModule.BusinessControllerClass,
                        module.DesktopModule.ModuleName,
                        module.ModuleID,
                        module.TabID,
                        module.PortalID);
                throw new Exception(message, ex);
            }
            catch (Exception ex1)
            {
                Exceptions.Exceptions.LogException(ex1);
            }
        }

        private IEnumerable<ModuleIndexInfo> GetModulesForIndex(int portalId)
        {
            var businessControllers = new Hashtable();
            var searchModuleIds = new HashSet<int>();
			var searchModules = new List<ModuleIndexInfo>();
            //Only get modules that are set to be Indexed.
            var modules = ModuleController.Instance.GetSearchModules(portalId).Cast<ModuleInfo>().Where(m => m.TabModuleSettings["AllowIndex"] == null || bool.Parse(m.TabModuleSettings["AllowIndex"].ToString()));

            foreach (var module in modules.Where(module => !searchModuleIds.Contains(module.ModuleID)))
            {
                try
                {
                    var tab = TabController.Instance.GetTab(module.TabID, portalId, false);
                    //Only index modules on tabs that are set to be Indexed.
                    if (tab.TabSettings["AllowIndex"] == null || (tab.TabSettings["AllowIndex"] != null && bool.Parse(tab.TabSettings["AllowIndex"].ToString())))
                    {
                        //Check if the business controller is in the Hashtable                        
                        var controller = businessControllers[module.DesktopModule.BusinessControllerClass];
                        if (!String.IsNullOrEmpty(module.DesktopModule.BusinessControllerClass))
                        {
                            //If nothing create a new instance
                            if (controller == null)
                            {
                                //Add to hashtable
                                controller = Reflection.CreateObject(module.DesktopModule.BusinessControllerClass, module.DesktopModule.BusinessControllerClass);
                                businessControllers.Add(module.DesktopModule.BusinessControllerClass, controller);
                            }
                        }

                        searchModules.Add(new ModuleIndexInfo{ModuleInfo = module, SupportSearch = controller is ModuleSearchBase});
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    ThrowLogError(module, ex);
                }
                finally
                {
                    searchModuleIds.Add(module.ModuleID);
                }
            }
            return searchModules;
        }

        #endregion

    }
}
