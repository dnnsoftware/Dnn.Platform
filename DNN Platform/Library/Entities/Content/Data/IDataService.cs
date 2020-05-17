// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections.Generic;
using System.Data;

using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Services.FileSystem;

#endregion

namespace DotNetNuke.Entities.Content.Data
{
	/// <summary>
	/// Interface of DataService.
	/// </summary>
	/// <seealso cref="DataService"/>
    public interface IDataService
    {
        //Content Item Methods
        int AddContentItem(ContentItem contentItem, int createdByUserId);

        void DeleteContentItem(int contentItemId);

        IDataReader GetContentItem(int contentItemId);

	    IDataReader GetContentItems(int contentTypeId, int tabId, int moduleId);

        IDataReader GetContentItemsByTerm(string term);

        IDataReader GetContentItemsByContentType(int contentTypeId);

        IDataReader GetContentItemsByModuleId(int moduleId);

	    IDataReader GetContentItemsByTabId(int tabId);

	    IDataReader GetContentItemsByVocabularyId(int vocabularyId);

        IDataReader GetUnIndexedContentItems();

        void UpdateContentItem(ContentItem contentItem, int lastModifiedByUserId);

        //Content MetaData Methods
        void AddMetaData(ContentItem contentItem, string name, string value);

        void DeleteMetaData(ContentItem contentItem, string name, string value);
        
        IDataReader GetMetaData(int contentItemId);

	    void SynchronizeMetaData(ContentItem contentItem,
	                             IEnumerable<KeyValuePair<string, string>> added,
	                             IEnumerable<KeyValuePair<string, string>> deleted);

        //ContentType Methods
        int AddContentType(ContentType contentType);

        void DeleteContentType(ContentType contentType);

        IDataReader GetContentTypes();

        void UpdateContentType(ContentType contentType);
        
        //ScopeType Methods
        int AddScopeType(ScopeType scopeType);

        void DeleteScopeType(ScopeType scopeType);

        IDataReader GetScopeTypes();

        void UpdateScopeType(ScopeType scopeType);

        //Term Methods
        int AddHeirarchicalTerm(Term term, int createdByUserId);

        int AddSimpleTerm(Term term, int createdByUserId);

        void AddTermToContent(Term term, ContentItem contentItem);

        void DeleteSimpleTerm(Term term);

        void DeleteHeirarchicalTerm(Term term);

        IDataReader GetTerm(int termId);

	    IDataReader GetTermUsage(int termId);

        IDataReader GetTermsByContent(int contentItemId);

        IDataReader GetTermsByVocabulary(int vocabularyId);

        void RemoveTermsFromContent(ContentItem contentItem);

        void UpdateHeirarchicalTerm(Term term, int lastModifiedByUserId);

        void UpdateSimpleTerm(Term term, int lastModifiedByUserId);

        //Vocabulary Methods
        int AddVocabulary(Vocabulary vocabulary, int createdByUserId);

        void DeleteVocabulary(Vocabulary vocabulary);

        IDataReader GetVocabularies();

        void UpdateVocabulary(Vocabulary vocabulary, int lastModifiedByUserId);
    }
}
