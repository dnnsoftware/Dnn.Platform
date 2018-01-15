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
using System.Collections.Generic;

using DotNetNuke.Services.Search.Entities;

#endregion

namespace DotNetNuke.Services.Search.Internals
{
    /// <summary>
    /// Internal Search Controller Interface.
    /// <remarks>This is an Internal interface and should not be used outside of Core.</remarks>
    /// </summary>
    public interface IInternalSearchController 
    {
        /// <summary>
        /// Get a List of Search Content Source that participate in Search
        /// </summary>
        IEnumerable<SearchContentSource> GetSearchContentSourceList(int portalId);

        /// <summary>
        /// Returns current search indexs general information
        /// </summary>
        /// <returns></returns>
        SearchStatistics GetSearchStatistics();

        /// <summary>
        /// Get Friendly Display Name for the Search Result
        /// </summary>
        /// <remarks>SearchTypeId is used primarily to obtain this value. Multiple SearchTypeId can map to same Display Name, 
        /// e.g. Tab, Module, Html/Module all map to Pages.
        /// For SearchTypeId=module, ModuleDefitionId is also used. Module's display name is used unless an entry is found in 
        /// ~/DesktopModules/Admin/SearchResults/App_LocalResources/SearchableModules.resx for the Module_[MODULENAME].txt is found.</remarks>
        string GetSearchDocumentTypeDisplayName(SearchResult searchResult);

        #region Core Search Indexing APIs

        /// <summary>
        /// Add a Search Document to Search Index
        /// </summary>
        void AddSearchDocument(SearchDocument searchDocument);

        /// <summary>
        /// Adds the collection of search documents to the search index
        /// </summary>
        /// <remarks>
        /// The controller auto-commits at the end of this method
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
        /// Delete all search documents related to a particula module
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="moduleId"></param>
        /// <param name="moduleDefId"></param>
        void DeleteSearchDocumentsByModule(int portalId, int moduleId, int moduleDefId);

        /// <summary>
        /// Deletes all documents of a specified portal and search type (used for re-index operation)
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="searchTypeId"></param>
        void DeleteAllDocuments(int portalId, int searchTypeId);

        /// <summary>
        /// Commits individually added/deleted documents to the search index
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

        #endregion
    }
}