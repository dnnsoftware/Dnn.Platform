﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Data
{
    using System.Collections.Generic;
    using System.Data;

    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Taxonomy;

    /// <summary>Persistent data of content with DataProvider instance.</summary>
    /// <remarks>It's better to use <see cref="Util.GetDataService"/> instead of create new instance directly.</remarks>
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
        private readonly DataProvider provider = DataProvider.Instance();

        /// <summary>Adds the content item.</summary>
        /// <param name="contentItem">The content item.</param>
        /// <param name="createdByUserId">The created by user id.</param>
        /// <returns>content item id.</returns>
        public int AddContentItem(ContentItem contentItem, int createdByUserId)
        {
            return this.provider.ExecuteScalar<int>(
                "AddContentItem",
                contentItem.Content,
                contentItem.ContentTypeId,
                contentItem.TabID,
                contentItem.ModuleID,
                contentItem.ContentKey,
                contentItem.Indexed,
                createdByUserId,
                this.provider.GetNull(contentItem.StateID));
        }

        /// <summary>Deletes the content item.</summary>
        /// <param name="contentItemId">The content item ID.</param>
        public void DeleteContentItem(int contentItemId)
        {
            this.provider.ExecuteNonQuery("DeleteContentItem", contentItemId);
        }

        /// <summary>Gets the content item.</summary>
        /// <param name="contentItemId">The content item id.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetContentItem(int contentItemId)
        {
            return this.provider.ExecuteReader("GetContentItem", contentItemId);
        }

        /// <summary>Gets the content items.</summary>
        /// <param name="contentTypeId">The Id of the Content Type.</param>
        /// <param name="tabId">The Id of the Tab.</param>
        /// <param name="moduleId">The Id of the Module.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetContentItems(int contentTypeId, int tabId, int moduleId)
        {
            return this.provider.ExecuteReader(
                "GetContentItems",
                this.provider.GetNull(contentTypeId),
                this.provider.GetNull(tabId),
                this.provider.GetNull(moduleId));
        }

        /// <summary>Gets the content items by term.</summary>
        /// <param name="term">The term.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetContentItemsByTerm(string term)
        {
            return this.provider.ExecuteReader("GetContentItemsByTerm", term);
        }

        /// <summary>Get a list of content items of the specified content type, <paramref name="contentTypeId"/>.</summary>
        /// <param name="contentTypeId">The type of content items you are searching for.</param>
        /// <returns>A data reader containing the content items.</returns>
        public IDataReader GetContentItemsByContentType(int contentTypeId)
        {
            return this.provider.ExecuteReader("GetContentItemsByContentType", contentTypeId);
        }

        /// <summary>Get a list of content items based on TabID (PageID).</summary>
        /// <param name="tabId">The TabID (or "Page ID") that the content items are associated with.</param>
        /// <returns>A data reader containing the content items.</returns>
        public IDataReader GetContentItemsByTabId(int tabId)
        {
            return this.provider.ExecuteReader("GetContentItemsByTabId", tabId);
        }

        /// <summary>Retrieve all content items associated with a articular Module ID, <paramref name="moduleId"/>.</summary>
        /// <param name="moduleId">The module ID.</param>
        /// <returns>A data reader containing the content items.</returns>
        public IDataReader GetContentItemsByModuleId(int moduleId)
        {
            return this.provider.ExecuteReader("GetContentItemsByModuleId", moduleId);
        }

        /// <summary>Retrieve a list of content items containing terms from the specified Vocabulary ID.</summary>
        /// <param name="vocabularyId">The vocabulary ID.</param>
        /// <returns>A data reader containing the content items.</returns>
        public IDataReader GetContentItemsByVocabularyId(int vocabularyId)
        {
            return this.provider.ExecuteReader("GetContentItemsByVocabularyId", vocabularyId);
        }

        /// <summary>Gets the un indexed content items.</summary>
        /// <returns>data reader.</returns>
        public IDataReader GetUnIndexedContentItems()
        {
            return this.provider.ExecuteReader("GetUnIndexedContentItems");
        }

        /// <summary>Updates the content item.</summary>
        /// <param name="contentItem">The content item.</param>
        /// <param name="createdByUserId">The created by user id.</param>
        public void UpdateContentItem(ContentItem contentItem, int createdByUserId)
        {
            this.provider.ExecuteNonQuery(
                "UpdateContentItem",
                contentItem.ContentItemId,
                contentItem.Content,
                contentItem.ContentTypeId,
                contentItem.TabID,
                contentItem.ModuleID,
                contentItem.ContentKey,
                contentItem.Indexed,
                createdByUserId,
                this.provider.GetNull(contentItem.StateID));
        }

        /// <summary>Adds the meta data.</summary>
        /// <param name="contentItem">The content item.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddMetaData(ContentItem contentItem, string name, string value)
        {
            this.provider.ExecuteNonQuery("AddMetaData", contentItem.ContentItemId, name, value);
        }

        /// <inheritdoc/>
        public void SynchronizeMetaData(ContentItem contentItem, IEnumerable<KeyValuePair<string, string>> added, IEnumerable<KeyValuePair<string, string>> deleted)
        {
#if false
            //TODO: fixing the original code requires adding new DataProvider methods. Reason:
            //      since we are calling DeleteMetaData/AddMetaData on their own connections,
            //      the transaction is useless and not going on the same connection as the other
            //      operations; we must find a better way of doing this transactional operation.
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
                this.DeleteMetaData(contentItem, item.Key, item.Value);
            }

            foreach (var item in added)
            {
                this.AddMetaData(contentItem, item.Key, item.Value);
            }
#endif
        }

        /// <summary>Deletes the meta data.</summary>
        /// <param name="contentItem">The content item.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void DeleteMetaData(ContentItem contentItem, string name, string value)
        {
            this.provider.ExecuteNonQuery("DeleteMetaData", contentItem.ContentItemId, name, value);
        }

        /// <summary>Gets the meta data.</summary>
        /// <param name="contentItemId">The content item id.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetMetaData(int contentItemId)
        {
            return this.provider.ExecuteReader("GetMetaData", contentItemId);
        }

        /// <summary>Adds the type of the content.</summary>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>content type id.</returns>
        public int AddContentType(ContentType contentType)
        {
            return this.provider.ExecuteScalar<int>("AddContentType", contentType.ContentType);
        }

        /// <inheritdoc/>
        public void DeleteContentType(ContentType contentType)
        {
            this.provider.ExecuteNonQuery("DeleteContentType", contentType.ContentTypeId);
        }

        /// <summary>Gets the content types.</summary>
        /// <returns>data reader.</returns>
        public IDataReader GetContentTypes()
        {
            return this.provider.ExecuteReader("GetContentTypes");
        }

        /// <summary>Updates the type of the content.</summary>
        /// <param name="contentType">Type of the content.</param>
        public void UpdateContentType(ContentType contentType)
        {
            this.provider.ExecuteNonQuery("UpdateContentType", contentType.ContentTypeId, contentType.ContentType);
        }

        /// <summary>Adds the type of the scope.</summary>
        /// <param name="scopeType">Type of the scope.</param>
        /// <returns>scope type id.</returns>
        public int AddScopeType(ScopeType scopeType)
        {
            return this.provider.ExecuteScalar<int>("AddScopeType", scopeType.ScopeType);
        }

        /// <summary>Deletes the type of the scope.</summary>
        /// <param name="scopeType">Type of the scope.</param>
        public void DeleteScopeType(ScopeType scopeType)
        {
            this.provider.ExecuteNonQuery("DeleteScopeType", scopeType.ScopeTypeId);
        }

        /// <summary>Gets the scope types.</summary>
        /// <returns>data reader.</returns>
        public IDataReader GetScopeTypes()
        {
            return this.provider.ExecuteReader("GetScopeTypes");
        }

        /// <summary>Updates the type of the scope.</summary>
        /// <param name="scopeType">Type of the scope.</param>
        public void UpdateScopeType(ScopeType scopeType)
        {
            this.provider.ExecuteNonQuery("UpdateScopeType", scopeType.ScopeTypeId, scopeType.ScopeType);
        }

        /// <summary>Adds the hierarchical term.</summary>
        /// <param name="term">The term.</param>
        /// <param name="createdByUserId">The created by user id.</param>
        /// <returns>term id.</returns>
        public int AddHeirarchicalTerm(Term term, int createdByUserId)
        {
            return this.provider.ExecuteScalar<int>("AddHeirarchicalTerm", term.VocabularyId, term.ParentTermId, term.Name, term.Description, term.Weight, createdByUserId);
        }

        /// <summary>Adds the simple term.</summary>
        /// <param name="term">The term.</param>
        /// <param name="createdByUserId">The created by user id.</param>
        /// <returns>term id.</returns>
        public int AddSimpleTerm(Term term, int createdByUserId)
        {
            return this.provider.ExecuteScalar<int>("AddSimpleTerm", term.VocabularyId, term.Name, term.Description, term.Weight, createdByUserId);
        }

        /// <inheritdoc/>
        public void AddTermToContent(Term term, ContentItem contentItem)
        {
            this.provider.ExecuteNonQuery("AddTermToContent", term.TermId, contentItem.ContentItemId);
        }

        /// <summary>Deletes the simple term.</summary>
        /// <param name="term">The term.</param>
        public void DeleteSimpleTerm(Term term)
        {
            this.provider.ExecuteNonQuery("DeleteSimpleTerm", term.TermId);
        }

        /// <summary>Deletes the hierarchical term.</summary>
        /// <param name="term">The term.</param>
        public void DeleteHeirarchicalTerm(Term term)
        {
            this.provider.ExecuteNonQuery("DeleteHeirarchicalTerm", term.TermId);
        }

        /// <summary>Gets the term.</summary>
        /// <param name="termId">The term id.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetTerm(int termId)
        {
            return this.provider.ExecuteReader("GetTerm", termId);
        }

        /// <summary>Retrieve term usage data for the specified Term ID, <paramref name="termId"/>.</summary>
        /// <param name="termId">The term ID.</param>
        /// <returns>A data reader with the term usage.</returns>
        public IDataReader GetTermUsage(int termId)
        {
            return this.provider.ExecuteReader("GetTermUsage", termId);
        }

        /// <summary>Gets the content of the terms by.</summary>
        /// <param name="contentItemId">The content item id.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetTermsByContent(int contentItemId)
        {
            return this.provider.ExecuteReader("GetTermsByContent", contentItemId);
        }

        /// <summary>Gets the terms by vocabulary.</summary>
        /// <param name="vocabularyId">The vocabulary id.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetTermsByVocabulary(int vocabularyId)
        {
            return this.provider.ExecuteReader("GetTermsByVocabulary", vocabularyId);
        }

        /// <summary>Removes the content of the terms from.</summary>
        /// <param name="contentItem">The content item.</param>
        public void RemoveTermsFromContent(ContentItem contentItem)
        {
            this.provider.ExecuteNonQuery("RemoveTermsFromContent", contentItem.ContentItemId);
        }

        /// <summary>Updates the hierarchical term.</summary>
        /// <param name="term">The term.</param>
        /// <param name="lastModifiedByUserId">The last modified by user id.</param>
        public void UpdateHeirarchicalTerm(Term term, int lastModifiedByUserId)
        {
            this.provider.ExecuteNonQuery("UpdateHeirarchicalTerm", term.TermId, term.VocabularyId, term.ParentTermId, term.Name, term.Description, term.Weight, lastModifiedByUserId);
        }

        /// <summary>Updates the simple term.</summary>
        /// <param name="term">The term.</param>
        /// <param name="lastModifiedByUserId">The last modified by user id.</param>
        public void UpdateSimpleTerm(Term term, int lastModifiedByUserId)
        {
            this.provider.ExecuteNonQuery("UpdateSimpleTerm", term.TermId, term.VocabularyId, term.Name, term.Description, term.Weight, lastModifiedByUserId);
        }

        /// <summary>Adds the vocabulary.</summary>
        /// <param name="vocabulary">The vocabulary.</param>
        /// <param name="createdByUserId">The created by user id.</param>
        /// <returns>Vocabulary id.</returns>
        public int AddVocabulary(Vocabulary vocabulary, int createdByUserId)
        {
            return this.provider.ExecuteScalar<int>(
                "AddVocabulary",
                vocabulary.Type,
                vocabulary.Name,
                vocabulary.Description,
                vocabulary.Weight,
                this.provider.GetNull(vocabulary.ScopeId),
                vocabulary.ScopeTypeId,
                createdByUserId);
        }

        /// <summary>Deletes the vocabulary.</summary>
        /// <param name="vocabulary">The vocabulary.</param>
        public void DeleteVocabulary(Vocabulary vocabulary)
        {
            this.provider.ExecuteNonQuery("DeleteVocabulary", vocabulary.VocabularyId);
        }

        /// <summary>Gets the vocabularies.</summary>
        /// <returns>data reader.</returns>
        public IDataReader GetVocabularies()
        {
            return this.provider.ExecuteReader("GetVocabularies");
        }

        /// <summary>Updates the vocabulary.</summary>
        /// <param name="vocabulary">The vocabulary.</param>
        /// <param name="lastModifiedByUserId">The last modified by user id.</param>
        public void UpdateVocabulary(Vocabulary vocabulary, int lastModifiedByUserId)
        {
            this.provider.ExecuteNonQuery(
                "UpdateVocabulary",
                vocabulary.VocabularyId,
                vocabulary.Type,
                vocabulary.Name,
                vocabulary.Description,
                vocabulary.Weight,
                vocabulary.ScopeId,
                vocabulary.ScopeTypeId,
                lastModifiedByUserId);
        }
    }
}
