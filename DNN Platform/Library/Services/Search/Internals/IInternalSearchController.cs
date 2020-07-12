// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Services.Search.Entities;

    /// <summary>
    /// Internal Search Controller Interface.
    /// <remarks>This is an Internal interface and should not be used outside of Core.</remarks>
    /// </summary>
    public interface IInternalSearchController
    {
        /// <summary>
        /// Get a List of Search Content Source that participate in Search.
        /// </summary>
        /// <returns></returns>
        IEnumerable<SearchContentSource> GetSearchContentSourceList(int portalId);

        /// <summary>
        /// Returns current search indexs general information.
        /// </summary>
        /// <returns></returns>
        SearchStatistics GetSearchStatistics();

        /// <summary>
        /// Get Friendly Display Name for the Search Result.
        /// </summary>
        /// <remarks>SearchTypeId is used primarily to obtain this value. Multiple SearchTypeId can map to same Display Name,
        /// e.g. Tab, Module, Html/Module all map to Pages.
        /// For SearchTypeId=module, ModuleDefitionId is also used. Module's display name is used unless an entry is found in
        /// ~/DesktopModules/Admin/SearchResults/App_LocalResources/SearchableModules.resx for the Module_[MODULENAME].txt is found.</remarks>
        /// <returns></returns>
        string GetSearchDocumentTypeDisplayName(SearchResult searchResult);

        /// <summary>
        /// Add a Search Document to Search Index.
        /// </summary>
        void AddSearchDocument(SearchDocument searchDocument);

        /// <summary>
        /// Adds the collection of search documents to the search index.
        /// </summary>
        /// <remarks>
        /// The controller auto-commits at the end of this method.
        /// </remarks>
        /// <param name="searchDocumentList"></param>
        void AddSearchDocuments(IEnumerable<SearchDocument> searchDocumentList);

        /// <summary>
        /// Delete a Search Document from the Search Index.
        /// REQUIRES: searchDocument to have PortalId, UniqueKey, SearchTypeId properties set.
        /// </summary>
        /// <param name="searchDocument"></param>
        void DeleteSearchDocument(SearchDocument searchDocument);

        /// <summary>
        /// Delete all search documents related to a particula module.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="moduleId"></param>
        /// <param name="moduleDefId"></param>
        void DeleteSearchDocumentsByModule(int portalId, int moduleId, int moduleDefId);

        /// <summary>
        /// Deletes all documents of a specified portal and search type (used for re-index operation).
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="searchTypeId"></param>
        void DeleteAllDocuments(int portalId, int searchTypeId);

        /// <summary>
        /// Commits individually added/deleted documents to the search index.
        /// </summary>
        void Commit();

        /// <summary>
        /// Optimize the search index files by compacting and removing previously deleted search documents.
        /// <para>The call will return immediately and the operation runs on a background thread.</para>
        /// </summary>
        /// <remarks>
        /// This is a costly operation which consumes substantial CPU and I/O resources, therefore use it
        /// judiciously. If your site has a a single server that performs both indexing and searching, then
        /// you should consider running the optimize operation after hours or over the weekend so that it
        /// does not interfere with ongoing search activities.
        /// </remarks>
        /// <returns>True is optimization was scheduled to run in the background, false otherwise.</returns>
        bool OptimizeSearchIndex();
    }
}
