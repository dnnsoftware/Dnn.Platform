#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Data;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;

#endregion

namespace DotNetNuke.Entities.Content
{
    public class ContentController : IContentController
    {
        private readonly IDataService _dataService;

        #region Constructors

        public ContentController() : this(Util.GetDataService())
        {
        }

        public ContentController(IDataService dataService)
        {
            _dataService = dataService;
        }

        #endregion

        #region Public Methods

	    public int AddContentItem(ContentItem contentItem)
        {
            //Argument Contract
            Requires.NotNull("contentItem", contentItem);

            contentItem.ContentItemId = _dataService.AddContentItem(contentItem, UserController.GetCurrentUserInfo().UserID);

            SaveMetadataDelta(contentItem);

            return contentItem.ContentItemId;
        }

        public void DeleteContentItem(ContentItem contentItem)
        {
            //Argument Contract
            Requires.NotNull("contentItem", contentItem);
            Requires.PropertyNotNegative("contentItem", "ContentItemId", contentItem.ContentItemId);

            _dataService.DeleteContentItem(contentItem.ContentItemId);
        }

        public void DeleteContentItem(int contentItemId)
        {
            Requires.NotNegative("contentItemId", contentItemId);

            _dataService.DeleteContentItem(contentItemId);
        }
        
        public ContentItem GetContentItem(int contentItemId)
        {
            //Argument Contract
            Requires.NotNegative("contentItemId", contentItemId);

            return CBO.FillObject<ContentItem>(_dataService.GetContentItem(contentItemId));
        }

        public IQueryable<ContentItem> GetContentItems(int contentTypeId, int tabId, int moduleId)
        {
            return CBO.FillQueryable<ContentItem>(_dataService.GetContentItems(contentTypeId, tabId, moduleId));
        }

        public IQueryable<ContentItem> GetContentItemsByTerm(string term)
        {
            //Argument Contract
            Requires.NotNullOrEmpty("term", term);

            return CBO.FillQueryable<ContentItem>(_dataService.GetContentItemsByTerm(term));
        }

        public IQueryable<ContentItem> GetContentItemsByTerm(Term term)
	    {
	        return GetContentItemsByTerm(term.Name);
	    }

	    public IQueryable<ContentItem> GetContentItemsByContentType(int contentTypeId)
	    {
            return CBO.FillQueryable<ContentItem>(_dataService.GetContentItemsByContentType(contentTypeId));
	    }

        /// <summary>Get a list of content items by ContentType.</summary>
        public IQueryable<ContentItem> GetContentItemsByContentType(ContentType contentType)
        {
            return CBO.FillQueryable<ContentItem>(_dataService.GetContentItemsByContentType(contentType.ContentTypeId));
        }

	    public IQueryable<ContentItem> GetContentItemsByTerms(IList<Term> terms)
        {
            return GetContentItemsByTerms(terms.Select(t => t.Name).ToArray());
        }

	    public IQueryable<ContentItem> GetContentItemsByTerms(string[] terms)
        {
            var union = new List<ContentItem>();

            union = terms.Aggregate(union,
                (current, term) =>
                    !current.Any()
                        ? GetContentItemsByTerm(term).ToList()
                        : current.Intersect(GetContentItemsByTerm(term), new ContentItemEqualityComparer()).ToList());

            return union.AsQueryable();
        }

        public IQueryable<ContentItem> GetContentItemsByTabId(int tabId)
        {
            return CBO.FillQueryable<ContentItem>(_dataService.GetContentItemsByTabId(tabId));
        }

	    public IQueryable<ContentItem> GetContentItemsByVocabularyId(int vocabularyId)
        {
            return CBO.FillQueryable<ContentItem>(_dataService.GetContentItemsByVocabularyId(vocabularyId));
        }

        public IQueryable<ContentItem> GetUnIndexedContentItems()
        {
            return CBO.FillQueryable<ContentItem>(_dataService.GetUnIndexedContentItems());
        }

        public IQueryable<ContentItem> GetContentItemsByModuleId(int moduleId)
        {
            return CBO.FillQueryable<ContentItem>(_dataService.GetContentItemsByModuleId(moduleId));
        }

        public void UpdateContentItem(ContentItem contentItem)
        {
            //Argument Contract
            Requires.NotNull("contentItem", contentItem);
            Requires.PropertyNotNegative("contentItem", "ContentItemId", contentItem.ContentItemId);
            
            AttachmentController.SerializeAttachmentMetadata(contentItem);

            SaveMetadataDelta(contentItem);
            
            _dataService.UpdateContentItem(contentItem, UserController.GetCurrentUserInfo().UserID);
        }

        public void AddMetaData(ContentItem contentItem, string name, string value)
        {
            //Argument Contract
            Requires.NotNull("contentItem", contentItem);
            Requires.PropertyNotNegative("contentItem", "ContentItemId", contentItem.ContentItemId);
            Requires.NotNullOrEmpty("name", name);

            _dataService.AddMetaData(contentItem, name, value);
        }

        public void DeleteMetaData(ContentItem contentItem, string name, string value)
        {
            //Argument Contract
            Requires.NotNull("contentItem", contentItem);
            Requires.PropertyNotNegative("contentItem", "ContentItemId", contentItem.ContentItemId);
            Requires.NotNullOrEmpty("name", name);

            _dataService.DeleteMetaData(contentItem, name, value);
        }

        public void DeleteMetaData(ContentItem contentItem, string name)
        {
            if (contentItem.Metadata.AllKeys.Contains(name))
            {
                DeleteMetaData(contentItem, name, contentItem.Metadata[name]);
            }
        }

        public NameValueCollection GetMetaData(int contentItemId)
        {
            //Argument Contract
            Requires.NotNegative("contentItemId", contentItemId);

            var metadata = new NameValueCollection();

            using (var dr = _dataService.GetMetaData(contentItemId))
            {
                if (dr != null)
                {
                    while (dr.Read())
                    {
                        metadata.Add(dr.GetString(0), dr.GetString(1));
                    }
                }
            }

		    return metadata;
        }
        
        private void SaveMetadataDelta(ContentItem contentItem)
        {
            var persisted = GetMetaData(contentItem.ContentItemId);

            var lh = persisted.AllKeys.ToDictionary(x => x, x => persisted[x]);

            var rh = contentItem.Metadata.AllKeys.ToDictionary(x => x, x => contentItem.Metadata[x]);

            if (!MetaDataChanged(lh, rh))
            {
                return;
            }

            // Items in the database but missing from the parameter (have been deleted).
            var deleted = lh.Except(rh).ToArray();

            // Items included in the object but missing from the database (newly added).
            var added = rh.Except(lh).ToArray();

            _dataService.SynchronizeMetaData(contentItem, added, deleted);
        }

        private static bool MetaDataChanged(
            IEnumerable<KeyValuePair<string, string>> lh,
            IEnumerable<KeyValuePair<string, string>> rh)
        {
            return lh.SequenceEqual(rh, new NameValueEqualityComparer()) == false;
        }

        #endregion
    }
}