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
using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
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
    /// <history>
    ///		[cnurse]	11/15/2004	documented
    ///     [vnguyen]   04/16/2013  updated with methods for an Updated Search
    /// </history>
    /// -----------------------------------------------------------------------------
    public class ModuleIndexer : IndexingProvider
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ModuleIndexer));
        private static readonly int ModuleSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId;
        
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns the collection of SearchDocuments for the portal.
        /// This replaces "GetSearchIndexItems" as a newer implementation of search.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        /// <history>
        ///     [vnguyen]   04/16/2013  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override IEnumerable<SearchDocument> GetSearchDocuments(int portalId, DateTime startDate)
        {
            var searchDocuments = new List<SearchDocument>();
            var searchModuleCollection = GetSearchModules(portalId);

            foreach (var module in searchModuleCollection)
            {
                try
                {
                    //Some modules update LastContentModifiedOnDate (e.g. Html module) when their content changes.
                    //We won't be calling into such modules if LastContentModifiedOnDate is prior to startDate
                    //LastContentModifiedOnDate remains minvalue for modules that don't update this property
                    if (module.LastContentModifiedOnDate != DateTime.MinValue && module.LastContentModifiedOnDate < startDate)
                    {
                        continue;
                    }

                    var controller =  Reflection.CreateObject(module.DesktopModule.BusinessControllerClass, module.DesktopModule.BusinessControllerClass);
                    var contentInfo = new SearchContentModuleInfo {ModSearchBaseControllerType= (ModuleSearchBase) controller, ModInfo = module};
                    var searchItems = contentInfo.ModSearchBaseControllerType.GetModifiedSearchDocuments(module, startDate);

                    if (searchItems != null)
                    {
                        //Add Module MetaData
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

                        Logger.Trace("ModuleIndexer: " + searchItems.Count + " search documents found for module [" + module.DesktopModule.ModuleName + " mid:" + module.ModuleID + "]");

                        searchDocuments.AddRange(searchItems);
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
        /// Returns a collection of SearchDocuments containing module metadata (title, header, footer...) of Searchable Modules.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        /// <history>
        ///     [vnguyen]   05/17/2013  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public List<SearchDocument> GetModuleMetaData(int portalId, DateTime startDate)
        {
            var searchDocuments = new List<SearchDocument>();
            var searchModuleCollection = GetSearchModules(portalId);
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
                            ModifiedTimeUtc = module.LastModifiedOnDate,
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
        /// <history>
        ///     [vnguyen]   05/16/2013  created
        /// </history>
        /// -----------------------------------------------------------------------------
        #pragma warning disable 0618
        public SearchDocument ConvertSearchItemInfoToSearchDocument(SearchItemInfo searchItem)
        {
            var moduleController = new ModuleController();
            var module = moduleController.GetModule(searchItem.ModuleId);

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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of modules that are listed as "Searchable" from the module definition and check if they
        /// implement ModuleSearchBase -- which is a newer implementation of search that replaces ISearchable
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns></returns>
        /// <history>
        ///     [vnguyen]   04/16/2013  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected IEnumerable<ModuleInfo> GetSearchModules(int portalId)
        {
            var tabController = new TabController();
            var moduleController = new ModuleController();
            var businessControllers = new Hashtable();
            var searchModuleIds = new HashSet<int>();
            var searchModules = new List<ModuleInfo>();
            //Only get modules that are set to be Indexed.
            var modules = moduleController.GetSearchModules(portalId).Cast<ModuleInfo>().Where(m => m.TabModuleSettings["AllowIndex"] == null || bool.Parse(m.TabModuleSettings["AllowIndex"].ToString()));
            
            foreach (var module in modules.Where(module => !searchModuleIds.Contains(module.ModuleID)))
            {
                try
                {
                    var tab = tabController.GetTab(module.TabID, portalId, false);
                    //Only index modules on tabs that are set to be Indexed.
                    if(tab.TabSettings["AllowIndex"] == null || (tab.TabSettings["AllowIndex"]!=null && bool.Parse(tab.TabSettings["AllowIndex"].ToString())))
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
                            //Check if module inherits from ModuleSearchBase
                            if (controller is ModuleSearchBase) searchModules.Add(module);
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
                    searchModuleIds.Add(module.ModuleID);
                }
            }
            return searchModules;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LEGACY: Depricated in DNN 7.1. Use 'GetSearchDocuments' instead.
        /// Used for Legacy Search (ISearchable) 
        /// 
        /// GetSearchIndexItems gets the SearchInfo Items for the Portal
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <history>
        ///		[cnurse]	11/15/2004	documented
        ///     [vnguyen]   09/07/2010  Modified: Included logic to add TabId to searchItems
        /// </history>
        /// -----------------------------------------------------------------------------
        [Obsolete("Legacy Search (ISearchable) -- Depricated in DNN 7.1. Use 'GetSearchDocuments' instead.")]
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
        /// LEGACY: Depricated in DNN 7.1. Use 'GetSearchModules' instead.
        /// Used for Legacy Search (ISearchable) 
        /// 
        /// GetModuleList gets a collection of SearchContentModuleInfo Items for the Portal
        /// </summary>
        /// <remarks>
        /// Parses the Modules of the Portal, determining whetehr they are searchable.
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <history>
        ///		[cnurse]	11/15/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [Obsolete("Legacy Search (ISearchable) -- Depricated in DNN 7.1. Use 'GetSearchModules' instead.")]
        protected SearchContentModuleInfoCollection GetModuleList(int portalId)
        {
            var results = new SearchContentModuleInfoCollection();
            var objModules = new ModuleController();
            var arrModules = objModules.GetSearchModules(portalId);
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

        private static void ThrowLogError(ModuleInfo module, Exception ex)
        {
            try
            {
                var message = string.Format("Error Creating BusinessControllerClass '{0}' of module({1}) id=({2}) in tab({3}) and portal({4}) ",
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

    }
}