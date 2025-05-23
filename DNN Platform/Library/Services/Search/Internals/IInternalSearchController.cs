﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System.Collections.Generic;

    using DotNetNuke.Services.Search.Entities;

    /// <summary>Internal Search Controller Interface.</summary>
    /// <remarks>This is an Internal interface and should not be used outside of Core.</remarks>
    public interface IInternalSearchController
    {
        /// <summary>Get a List of Search Content Source that participate in Search.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A sequence of <see cref="SearchContentSource"/> instances.</returns>
        IEnumerable<SearchContentSource> GetSearchContentSourceList(int portalId);

        /// <summary>Returns current search indexes general information.</summary>
        /// <returns>A <see cref="SearchStatistics"/> instance or <see langword="null"/>.</returns>
        SearchStatistics GetSearchStatistics();

        /// <summary>Get Friendly Display Name for the Search Result.</summary>
        /// <remarks>SearchTypeId is used primarily to obtain this value. Multiple SearchTypeId can map to same Display Name,
        /// e.g. Tab, Module, Html/Module all map to Pages.
        /// For SearchTypeId=module, ModuleDefinitionId is also used. Module's display name is used unless an entry is found in
        /// ~/DesktopModules/Admin/SearchResults/App_LocalResources/SearchableModules.resx for the Module_[MODULENAME].txt is found.</remarks>
        /// <param name="searchResult">The search result.</param>
        /// <returns>The display name or <see cref="string.Empty"/>.</returns>
        string GetSearchDocumentTypeDisplayName(SearchResult searchResult);

        /// <summary>Add a Search Document to Search Index.</summary>
        /// <param name="searchDocument">The document to add.</param>
        void AddSearchDocument(SearchDocument searchDocument);

        /// <summary>Adds the collection of search documents to the search index.</summary>
        /// <remarks>The controller auto-commits at the end of this method.</remarks>
        /// <param name="searchDocumentList">The search documents to all.</param>
        void AddSearchDocuments(IEnumerable<SearchDocument> searchDocumentList);

        /// <summary>Delete a Search Document from the Search Index. REQUIRES: searchDocument to have PortalId, UniqueKey, SearchTypeId properties set.</summary>
        /// <param name="searchDocument">The search document to delete.</param>
        void DeleteSearchDocument(SearchDocument searchDocument);

        /// <summary>Delete all search documents related to a particular module.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="moduleDefId">The module definition ID.</param>
        void DeleteSearchDocumentsByModule(int portalId, int moduleId, int moduleDefId);

        /// <summary>Deletes all documents of a specified portal and search type (used for re-index operation).</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="searchTypeId">The search type ID.</param>
        void DeleteAllDocuments(int portalId, int searchTypeId);

        /// <summary>Commits individually added/deleted documents to the search index.</summary>
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
