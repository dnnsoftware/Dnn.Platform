// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals;
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

    using Microsoft.Extensions.DependencyInjection;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>The ModuleIndexer is an implementation of the abstract <see cref="IndexingProviderBase"/> class.</summary>
    public class ModuleIndexer : IndexingProviderBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleIndexer));
        private static readonly int ModuleSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId;

        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly Dictionary<int, IEnumerable<ModuleIndexInfo>> searchModules;

        /// <summary>Initializes a new instance of the <see cref="ModuleIndexer"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public ModuleIndexer()
            : this(false, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModuleIndexer"/> class.</summary>
        /// <param name="needSearchModules">Whether to pre-populate the collection of search modules.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public ModuleIndexer(bool needSearchModules)
            : this(needSearchModules, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModuleIndexer"/> class.</summary>
        /// <param name="needSearchModules">Whether to pre-populate the collection of search modules.</param>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        public ModuleIndexer(bool needSearchModules, IBusinessControllerProvider businessControllerProvider)
        {
            this.businessControllerProvider = businessControllerProvider ?? Globals.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
            this.searchModules = new Dictionary<int, IEnumerable<ModuleIndexInfo>>();

            if (needSearchModules)
            {
                var portals = PortalController.Instance.GetPortals();
                foreach (var portal in portals.Cast<IPortalInfo>())
                {
                    this.searchModules.Add(portal.PortalId, GetModulesForIndex(portal.PortalId));
                }

                this.searchModules.Add(Null.NullInteger, GetModulesForIndex(Null.NullInteger));
            }
        }

        /// <inheritdoc />
        public override int IndexSearchDocuments(int portalId, ScheduleHistoryItem schedule, DateTime startDateLocal, Action<IEnumerable<SearchDocument>> indexer)
        {
            Requires.NotNull("indexer", indexer);
            const int saveThreshold = 1024 * 2;
            var totalIndexed = 0;
            startDateLocal = this.GetLocalTimeOfLastIndexedItem(portalId, schedule.ScheduleID, startDateLocal);
            var searchDocuments = new List<SearchDocument>();
            var searchModuleCollection = this.searchModules.TryGetValue(portalId, out var indexes)
                ? indexes.Where(m => m.SupportSearch).Select(m => m.ModuleInfo)
                : this.GetSearchModules(portalId);

            // Some modules update LastContentModifiedOnDate (e.g. Html module) when their content changes.
            // We won't be calling into such modules if LastContentModifiedOnDate is prior to startDate.
            // LastContentModifiedOnDate remains MinValue for modules that don't update this property
            var modulesInDateRange = searchModuleCollection.Where(module =>
                !(SqlDateTime.MinValue.Value < module.LastContentModifiedOnDate && module.LastContentModifiedOnDate < startDateLocal))
                .OrderBy(m => m.LastContentModifiedOnDate).ThenBy(m => m.ModuleID).ToArray();

            if (modulesInDateRange.Length != 0)
            {
                foreach (var module in modulesInDateRange)
                {
                    try
                    {
                        var controller = this.businessControllerProvider.GetInstance<ModuleSearchBase>(module);
                        var contentInfo = new SearchContentModuleInfo { ModSearchBaseControllerType = controller, ModInfo = module };
                        var searchItems = contentInfo.ModSearchBaseControllerType.GetModifiedSearchDocuments(module, startDateLocal.ToUniversalTime());

                        if (searchItems is { Count: > 0, })
                        {
                            AddModuleMetaData(searchItems, module);
                            searchDocuments.AddRange(searchItems);

                            if (Logger.IsTraceEnabled)
                            {
                                Logger.TraceFormat(
                                    "ModuleIndexer: {0} search documents found for module [{1} mid:{2}]",
                                    searchItems.Count,
                                    module.DesktopModule.ModuleName,
                                    module.ModuleID);
                            }

                            if (searchDocuments.Count >= saveThreshold)
                            {
                                totalIndexed += this.IndexCollectedDocs(indexer, searchDocuments, portalId, schedule);
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
                    totalIndexed += this.IndexCollectedDocs(indexer, searchDocuments, portalId, schedule);
                }
            }

            return totalIndexed;
        }

        /// <summary>Returns a collection of SearchDocuments containing module metadata (title, header, footer...) of Searchable Modules.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="startDate">The date after which to look for changes.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="SearchDocument"/> instances.</returns>
        public List<SearchDocument> GetModuleMetaData(int portalId, DateTime startDate)
        {
            var searchDocuments = new List<SearchDocument>();
            var searchModuleCollection = this.searchModules.TryGetValue(portalId, out var indexes)
                ? indexes.Select(m => m.ModuleInfo)
                : this.GetSearchModules(portalId, true);
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
                            Body = module.Header + " " + module.Footer,
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

        /// <summary>Gets a list of modules that are listed as "Searchable" from the module definition and check if they implement <see cref="ModuleSearchBase"/> -- which is a newer implementation of search that replaces <see cref="ISearchable"/>.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A sequence of <see cref="ModuleInfo"/> instances.</returns>
        protected IEnumerable<ModuleInfo> GetSearchModules(int portalId)
        {
            return this.GetSearchModules(portalId, false);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected IEnumerable<ModuleInfo> GetSearchModules(int portalId, bool allModules)
        {
            return from mii in GetModulesForIndex(portalId)
                   where allModules || mii.SupportSearch
                   select mii.ModuleInfo;
        }

        private static void ThrowLogError(ModuleInfo module, Exception ex)
        {
            try
            {
                var message = string.Format(
                        Localization.GetExceptionMessage(
                            "ErrorCreatingBusinessControllerClass",
                            "Error Creating BusinessControllerClass '{0}' of module({1}) id=({2}) in tab({3}) and portal({4})"),
                        module.DesktopModule.BusinessControllerClass,
                        module.DesktopModule.ModuleName,
                        module.ModuleID,
                        module.TabID,
                        module.PortalID);
                throw new BusinessControllerClassException(message, ex);
            }
            catch (Exception ex1)
            {
                Exceptions.Exceptions.LogException(ex1);
            }
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

        private static List<ModuleIndexInfo> GetModulesForIndex(int portalId)
        {
            var businessControllers = new Dictionary<string, bool>();
            var searchModuleIds = new HashSet<int>();
            var searchModules = new List<ModuleIndexInfo>();

            // Only get modules that are set to be Indexed.
            var modules = ModuleController.Instance.GetSearchModules(portalId).Cast<ModuleInfo>().Where(m => m.TabModuleSettings["AllowIndex"] == null || bool.Parse(m.TabModuleSettings["AllowIndex"].ToString()));

            foreach (var module in modules.Where(module => !searchModuleIds.Contains(module.ModuleID)))
            {
                try
                {
                    var tab = TabController.Instance.GetTab(module.TabID, portalId, false);

                    // Only index modules on tabs that are set to be Indexed.
                    if (tab.TabSettings["AllowIndex"] == null || (tab.TabSettings["AllowIndex"] != null && bool.Parse(tab.TabSettings["AllowIndex"].ToString())))
                    {
                        // Check if the business controller is in the Hashtable
                        if (!businessControllers.TryGetValue(module.DesktopModule.BusinessControllerClass, out var supportsSearch))
                        {
                            var controllerType = Reflection.CreateType(module.DesktopModule.BusinessControllerClass);
                            supportsSearch = typeof(ModuleSearchBase).IsAssignableFrom(controllerType);
                            businessControllers.Add(module.DesktopModule.BusinessControllerClass, supportsSearch);
                        }

                        searchModules.Add(new ModuleIndexInfo { ModuleInfo = module, SupportSearch = supportsSearch });
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

        private int IndexCollectedDocs(
            Action<IEnumerable<SearchDocument>> indexer, List<SearchDocument> searchDocuments, int portalId, ScheduleHistoryItem schedule)
        {
            indexer.Invoke(searchDocuments);
            var total = searchDocuments.Count;
            this.SetLocalTimeOfLastIndexedItem(portalId, schedule.ScheduleID, schedule.StartDate);
            return total;
        }
#pragma warning restore 0618
    }
}
