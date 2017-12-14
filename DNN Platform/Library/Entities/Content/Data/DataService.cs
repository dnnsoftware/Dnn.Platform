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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Services.FileSystem;

#endregion

namespace DotNetNuke.Entities.Content.Data
{
	/// <summary>
	/// Persistent data of content with DataProvider instance.
	/// </summary>
	/// <remarks>
	/// It's better to use Util.GetDataService instead of create new instance directly.
	/// </remarks>
	/// <example>
	/// <code lang="C#">
	/// public ContentController() : this(Util.GetDataService())
    /// {
    /// }
    /// public ContentController(IDataService dataService)
    /// {
    ///     _dataService = dataService;
    /// }
	/// </code>
	/// </example>
    public class DataService : IDataService
    {
        private readonly DataProvider _provider = DataProvider.Instance();

        #region "ContentItem Methods"

		/// <summary>
		/// Adds the content item.
		/// </summary>
		/// <param name="contentItem">The content item.</param>
		/// <param name="createdByUserId">The created by user id.</param>
		/// <returns>content item id.</returns>
        public int AddContentItem(ContentItem contentItem, int createdByUserId)
        {
            return _provider.ExecuteScalar<int>("AddContentItem",
                                               contentItem.Content,
                                               contentItem.ContentTypeId,
                                               contentItem.TabID,
                                               contentItem.ModuleID,
                                               contentItem.ContentKey,
                                               contentItem.Indexed,
                                               createdByUserId,
                                               _provider.GetNull(contentItem.StateID));
        }

		/// <summary>
		/// Deletes the content item.
		/// </summary>
		/// <param name="contentItemId">The content item ID.</param>
        public void DeleteContentItem(int contentItemId)
        {
            _provider.ExecuteNonQuery("DeleteContentItem", contentItemId);
        }

		/// <summary>
		/// Gets the content item.
		/// </summary>
		/// <param name="contentItemId">The content item id.</param>
		/// <returns>data reader.</returns>
        public IDataReader GetContentItem(int contentItemId)
        {
            return _provider.ExecuteReader("GetContentItem", contentItemId);
        }

        /// <summary>
        /// Gets the content items.
        /// </summary>
        /// <param name="contentTypeId">The Id of the Content Type.</param>
        /// <param name="tabId">The Id of the Tab.</param>
        /// <param name="moduleId">The Id of the Module.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetContentItems(int contentTypeId, int tabId, int moduleId)
        {
            return _provider.ExecuteReader("GetContentItems", _provider.GetNull(contentTypeId), 
                                                            _provider.GetNull(tabId), 
                                                            _provider.GetNull(moduleId));
        }
     
		/// <summary>
		/// Gets the content items by term.
		/// </summary>
		/// <param name="term">The term.</param>
		/// <returns>data reader.</returns>
        public IDataReader GetContentItemsByTerm(string term)
        {
            return _provider.ExecuteReader("GetContentItemsByTerm", term);
        }

        /// <summary>
        /// Get a list of content items of the specified content type, <paramref name="contentTypeId"/>.
        /// </summary>
        /// <param name="contentTypeId">The type of content items you are searching for</param>
	    public IDataReader GetContentItemsByContentType(int contentTypeId)
        {
            return _provider.ExecuteReader("GetContentItemsByContentType", contentTypeId);
        }

        /// <summary>
        /// Get a list of content items based on TabID (PageID).
        /// </summary>
        /// <param name="tabId">The TabID (or "Page ID") that the content items are associated with</param>
        public IDataReader GetContentItemsByTabId(int tabId)
        {
            return _provider.ExecuteReader("GetContentItemsByTabId", tabId);
        }

        /// <summary>
        /// Retrieve all content items associated with a articular Module ID, <paramref name="moduleId"/>.
        /// </summary>
        public IDataReader GetContentItemsByModuleId(int moduleId)
        {
            return _provider.ExecuteReader("GetContentItemsByModuleId", moduleId);
        }

        /// <summary>
        /// Retrieve a list of content items containg terms from the specified Vocabulary ID.
        /// </summary>
        public IDataReader GetContentItemsByVocabularyId(int vocabularyId)
        {
            return _provider.ExecuteReader("GetContentItemsByVocabularyId", vocabularyId);
        }

	    /// <summary>
		/// Gets the un indexed content items.
		/// </summary>
		/// <returns>data reader.</returns>
        public IDataReader GetUnIndexedContentItems()
        {
            return _provider.ExecuteReader("GetUnIndexedContentItems");
        }

		/// <summary>
		/// Updates the content item.
		/// </summary>
		/// <param name="contentItem">The content item.</param>
		/// <param name="createdByUserId">The created by user id.</param>
        public void UpdateContentItem(ContentItem contentItem, int createdByUserId)
        {
            _provider.ExecuteNonQuery("UpdateContentItem",
                                     contentItem.ContentItemId,
                                     contentItem.Content,
                                     contentItem.ContentTypeId,
                                     contentItem.TabID,
                                     contentItem.ModuleID,
                                     contentItem.ContentKey,
                                     contentItem.Indexed,
                                     createdByUserId,
                                     _provider.GetNull(contentItem.StateID));
        }

        #endregion

        #region "MetaData Methods"

		/// <summary>
		/// Adds the meta data.
		/// </summary>
		/// <param name="contentItem">The content item.</param>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
        public void AddMetaData(ContentItem contentItem, string name, string value)
        {
            _provider.ExecuteNonQuery("AddMetaData", contentItem.ContentItemId, name, value);
        }
        
        public void SynchronizeMetaData(ContentItem contentItem, IEnumerable<KeyValuePair<string,string>> added, IEnumerable<KeyValuePair<string,string>> deleted)
        {
#if false
            //TODO: fixing the original code requires adding new DataProvider methods. Reason:
            //      since we are calling DeleteMetaData/AddMetaData on their own connections,
            //      the transaction is useless and not going on the same connection as the other
            //      operations; we must find a better way of doing this transactionsl operation.
            using (var transaction = _provider.GetTransaction())
            {
                try
                {
                    var connection = transaction.Connection;
                    foreach (var item in deleted)
                    {
                        _provider.ExecuteNonQuery(connection, "DeleteMetaData", contentItem.ContentItemId, item.Key, item.Value);
                    }

                    foreach (var item in added)
                    {
                        _provider.ExecuteNonQuery(connection, "AddMetaData", contentItem, item.Key, item.Value);
                    }

                    _provider.CommitTransaction(transaction);
                }
                catch (Exception)
                {
                    _provider.RollbackTransaction(transaction);
                    throw;
                }
            }
#else
            foreach (var item in deleted)
            {
                DeleteMetaData(contentItem, item.Key, item.Value);
            }

            foreach (var item in added)
            {
                AddMetaData(contentItem, item.Key, item.Value);
            }
#endif
        }

		/// <summary>
		/// Deletes the meta data.
		/// </summary>
		/// <param name="contentItem">The content item.</param>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
        public void DeleteMetaData(ContentItem contentItem, string name, string value)
        {
            _provider.ExecuteNonQuery("DeleteMetaData", contentItem.ContentItemId, name, value);
        }

		/// <summary>
		/// Gets the meta data.
		/// </summary>
		/// <param name="contentItemId">The content item id.</param>
		/// <returns>data reader.</returns>
        public IDataReader GetMetaData(int contentItemId)
        {
            return _provider.ExecuteReader("GetMetaData", contentItemId);
        }

        #endregion

        #region "ContentType Methods"

        /// <summary>
        /// Adds the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>content type id.</returns>
        public int AddContentType(ContentType contentType)
        {
            return _provider.ExecuteScalar<int>("AddContentType", contentType.ContentType);
        }

        public void DeleteContentType(ContentType contentType)
        {
            _provider.ExecuteNonQuery("DeleteContentType", contentType.ContentTypeId);
        }

        /// <summary>
        /// Gets the content types.
        /// </summary>
        /// <returns>data reader.</returns>
        public IDataReader GetContentTypes()
        {
            return _provider.ExecuteReader("GetContentTypes");
        }

        /// <summary>
        /// Updates the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        public void UpdateContentType(ContentType contentType)
        {
            _provider.ExecuteNonQuery("UpdateContentType", contentType.ContentTypeId, contentType.ContentType);
        }

        #endregion

        #region "ScopeType Methods"

        /// <summary>
        /// Adds the type of the scope.
        /// </summary>
        /// <param name="scopeType">Type of the scope.</param>
        /// <returns>scope type id.</returns>
        public int AddScopeType(ScopeType scopeType)
        {
            return _provider.ExecuteScalar<int>("AddScopeType", scopeType.ScopeType);
        }

		/// <summary>
		/// Deletes the type of the scope.
		/// </summary>
		/// <param name="scopeType">Type of the scope.</param>
        public void DeleteScopeType(ScopeType scopeType)
        {
            _provider.ExecuteNonQuery("DeleteScopeType", scopeType.ScopeTypeId);
        }

		/// <summary>
		/// Gets the scope types.
		/// </summary>
		/// <returns>data reader.</returns>
        public IDataReader GetScopeTypes()
        {
            return _provider.ExecuteReader("GetScopeTypes");
        }

		/// <summary>
		/// Updates the type of the scope.
		/// </summary>
		/// <param name="scopeType">Type of the scope.</param>
        public void UpdateScopeType(ScopeType scopeType)
        {
            _provider.ExecuteNonQuery("UpdateScopeType", scopeType.ScopeTypeId, scopeType.ScopeType);
        }

        #endregion

        #region "Term Methods"

		/// <summary>
		/// Adds the heirarchical term.
		/// </summary>
		/// <param name="term">The term.</param>
		/// <param name="createdByUserId">The created by user id.</param>
		/// <returns>term id.</returns>
        public int AddHeirarchicalTerm(Term term, int createdByUserId)
        {
            return _provider.ExecuteScalar<int>("AddHeirarchicalTerm", term.VocabularyId, term.ParentTermId, term.Name, term.Description, term.Weight, createdByUserId);
        }

		/// <summary>
		/// Adds the simple term.
		/// </summary>
		/// <param name="term">The term.</param>
		/// <param name="createdByUserId">The created by user id.</param>
		/// <returns>term id.</returns>
        public int AddSimpleTerm(Term term, int createdByUserId)
        {
            return _provider.ExecuteScalar<int>("AddSimpleTerm", term.VocabularyId, term.Name, term.Description, term.Weight, createdByUserId);
        }

        public void AddTermToContent(Term term, ContentItem contentItem)
        {
            _provider.ExecuteNonQuery("AddTermToContent", term.TermId, contentItem.ContentItemId);
        }

		/// <summary>
		/// Deletes the simple term.
		/// </summary>
		/// <param name="term">The term.</param>
        public void DeleteSimpleTerm(Term term)
        {
            _provider.ExecuteNonQuery("DeleteSimpleTerm", term.TermId);
        }

		/// <summary>
		/// Deletes the heirarchical term.
		/// </summary>
		/// <param name="term">The term.</param>
        public void DeleteHeirarchicalTerm(Term term)
        {
            _provider.ExecuteNonQuery("DeleteHeirarchicalTerm", term.TermId);
        }

		/// <summary>
		/// Gets the term.
		/// </summary>
		/// <param name="termId">The term id.</param>
		/// <returns>data reader.</returns>
        public IDataReader GetTerm(int termId)
        {
            return _provider.ExecuteReader("GetTerm", termId);
        }

        /// <summary>
        /// Retrieve term usage data for the specified Term ID, <paramref name="termId"/>.
        /// </summary>
	    public IDataReader GetTermUsage(int termId)
        {
            return _provider.ExecuteReader("GetTermUsage", termId);
        }

	    /// <summary>
		/// Gets the content of the terms by.
		/// </summary>
		/// <param name="contentItemId">The content item id.</param>
		/// <returns>data reader.</returns>
        public IDataReader GetTermsByContent(int contentItemId)
        {
            return _provider.ExecuteReader("GetTermsByContent", contentItemId);
        }

		/// <summary>
		/// Gets the terms by vocabulary.
		/// </summary>
		/// <param name="vocabularyId">The vocabulary id.</param>
		/// <returns>data reader.</returns>
        public IDataReader GetTermsByVocabulary(int vocabularyId)
        {
            return _provider.ExecuteReader("GetTermsByVocabulary", vocabularyId);
        }

		/// <summary>
		/// Removes the content of the terms from.
		/// </summary>
		/// <param name="contentItem">The content item.</param>
        public void RemoveTermsFromContent(ContentItem contentItem)
        {
            _provider.ExecuteNonQuery("RemoveTermsFromContent", contentItem.ContentItemId);
        }

		/// <summary>
		/// Updates the heirarchical term.
		/// </summary>
		/// <param name="term">The term.</param>
		/// <param name="lastModifiedByUserId">The last modified by user id.</param>
        public void UpdateHeirarchicalTerm(Term term, int lastModifiedByUserId)
        {
            _provider.ExecuteNonQuery("UpdateHeirarchicalTerm", term.TermId, term.VocabularyId, term.ParentTermId, term.Name, term.Description, term.Weight, lastModifiedByUserId);
        }

		/// <summary>
		/// Updates the simple term.
		/// </summary>
		/// <param name="term">The term.</param>
		/// <param name="lastModifiedByUserId">The last modified by user id.</param>
        public void UpdateSimpleTerm(Term term, int lastModifiedByUserId)
        {
            _provider.ExecuteNonQuery("UpdateSimpleTerm", term.TermId, term.VocabularyId, term.Name, term.Description, term.Weight, lastModifiedByUserId);
        }

        #endregion

        #region "Vocabulary Methods"

		/// <summary>
		/// Adds the vocabulary.
		/// </summary>
		/// <param name="vocabulary">The vocabulary.</param>
		/// <param name="createdByUserId">The created by user id.</param>
		/// <returns>Vocabulary id.</returns>
        public int AddVocabulary(Vocabulary vocabulary, int createdByUserId)
        {
            return _provider.ExecuteScalar<int>("AddVocabulary",
                                               vocabulary.Type,
                                               vocabulary.Name,
                                               vocabulary.Description,
                                               vocabulary.Weight,
                                               _provider.GetNull(vocabulary.ScopeId),
                                               vocabulary.ScopeTypeId,
                                               createdByUserId);
        }

		/// <summary>
		/// Deletes the vocabulary.
		/// </summary>
		/// <param name="vocabulary">The vocabulary.</param>
        public void DeleteVocabulary(Vocabulary vocabulary)
        {
            _provider.ExecuteNonQuery("DeleteVocabulary", vocabulary.VocabularyId);
        }

		/// <summary>
		/// Gets the vocabularies.
		/// </summary>
		/// <returns>data reader.</returns>
        public IDataReader GetVocabularies()
        {
            return _provider.ExecuteReader("GetVocabularies");
        }

		/// <summary>
		/// Updates the vocabulary.
		/// </summary>
		/// <param name="vocabulary">The vocabulary.</param>
		/// <param name="lastModifiedByUserId">The last modified by user id.</param>
        public void UpdateVocabulary(Vocabulary vocabulary, int lastModifiedByUserId)
        {
            _provider.ExecuteNonQuery("UpdateVocabulary",
                                     vocabulary.VocabularyId,
                                     vocabulary.Type,
                                     vocabulary.Name,
                                     vocabulary.Description,
                                     vocabulary.Weight,
                                     vocabulary.ScopeId,
                                     vocabulary.ScopeTypeId,
                                     lastModifiedByUserId);
        }

	    #endregion
    }
}