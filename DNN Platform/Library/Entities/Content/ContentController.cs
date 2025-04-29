// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Data;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;

public class ContentController : ServiceLocator<IContentController, ContentController>, IContentController
{
    private readonly IDataService dataService;

    /// <summary>Initializes a new instance of the <see cref="ContentController"/> class.</summary>
    public ContentController()
        : this(Util.GetDataService())
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ContentController"/> class.</summary>
    /// <param name="dataService">The data service.</param>
    public ContentController(IDataService dataService)
    {
        this.dataService = dataService;
    }

    /// <inheritdoc/>
    public int AddContentItem(ContentItem contentItem)
    {
        // Argument Contract
        Requires.NotNull("contentItem", contentItem);
        var currentUser = UserController.Instance.GetCurrentUserInfo();
        var createdByUserId = currentUser.UserID;
        if (contentItem.CreatedByUserID >= 0 && contentItem.CreatedByUserID != currentUser.UserID)
        {
            createdByUserId = contentItem.CreatedByUserID;
        }

        contentItem.ContentItemId = this.dataService.AddContentItem(contentItem, createdByUserId);
        contentItem.CreatedByUserID = createdByUserId;
        contentItem.LastModifiedByUserID = currentUser.UserID;

        this.SaveMetadataDelta(contentItem);

        UpdateContentItemsCache(contentItem);

        return contentItem.ContentItemId;
    }

    /// <inheritdoc/>
    public void DeleteContentItem(ContentItem contentItem)
    {
        // Argument Contract
        Requires.NotNull("contentItem", contentItem);
        Requires.PropertyNotNegative("contentItem", "ContentItemId", contentItem.ContentItemId);

        var searrchDoc = new SearchDocumentToDelete
        {
            UniqueKey = contentItem.ContentItemId.ToString("D"),
            ModuleId = contentItem.ModuleID,
            TabId = contentItem.TabID,
            SearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId,
        };
        DotNetNuke.Data.DataProvider.Instance().AddSearchDeletedItems(searrchDoc);

        this.dataService.DeleteContentItem(contentItem.ContentItemId);

        UpdateContentItemsCache(contentItem, false);
    }

    /// <inheritdoc/>
    public void DeleteContentItem(int contentItemId)
    {
        var contentItem = this.GetContentItem(contentItemId);
        this.DeleteContentItem(contentItem);
    }

    /// <inheritdoc/>
    public ContentItem GetContentItem(int contentItemId)
    {
        // Argument Contract
        Requires.NotNegative("contentItemId", contentItemId);

        return CBO.GetCachedObject<ContentItem>(
            new CacheItemArgs(GetContentItemCacheKey(contentItemId), DataCache.ContentItemsCacheTimeOut, DataCache.ContentItemsCachePriority),
            c => CBO.FillObject<ContentItem>(this.dataService.GetContentItem(contentItemId)));
    }

    public IQueryable<ContentItem> GetContentItems(int contentTypeId, int tabId, int moduleId)
    {
        return CBO.FillQueryable<ContentItem>(this.dataService.GetContentItems(contentTypeId, tabId, moduleId));
    }

    /// <inheritdoc/>
    public IQueryable<ContentItem> GetContentItemsByTerm(string term)
    {
        // Argument Contract
        Requires.NotNullOrEmpty("term", term);

        return CBO.FillQueryable<ContentItem>(this.dataService.GetContentItemsByTerm(term));
    }

    /// <inheritdoc/>
    public IQueryable<ContentItem> GetContentItemsByTerm(Term term)
    {
        return this.GetContentItemsByTerm(term.Name);
    }

    /// <inheritdoc/>
    public IQueryable<ContentItem> GetContentItemsByContentType(int contentTypeId)
    {
        return CBO.FillQueryable<ContentItem>(this.dataService.GetContentItemsByContentType(contentTypeId));
    }

    /// <inheritdoc />
    public IQueryable<ContentItem> GetContentItemsByContentType(ContentType contentType)
    {
        return this.GetContentItemsByContentType(contentType.ContentTypeId);
    }

    /// <inheritdoc/>
    public IQueryable<ContentItem> GetContentItemsByTerms(IList<Term> terms)
    {
        return this.GetContentItemsByTerms(terms.Select(t => t.Name).ToArray());
    }

    /// <inheritdoc/>
    public IQueryable<ContentItem> GetContentItemsByTerms(string[] terms)
    {
        var union = new List<ContentItem>();

        union = terms.Aggregate(
            union,
            (current, term) =>
                !current.Any()
                    ? this.GetContentItemsByTerm(term).ToList()
                    : current.Intersect(this.GetContentItemsByTerm(term), new ContentItemEqualityComparer()).ToList());

        return union.AsQueryable();
    }

    /// <inheritdoc/>
    public IQueryable<ContentItem> GetContentItemsByTabId(int tabId)
    {
        return CBO.FillQueryable<ContentItem>(this.dataService.GetContentItemsByTabId(tabId));
    }

    /// <inheritdoc/>
    public IQueryable<ContentItem> GetContentItemsByVocabularyId(int vocabularyId)
    {
        return CBO.FillQueryable<ContentItem>(this.dataService.GetContentItemsByVocabularyId(vocabularyId));
    }

    /// <inheritdoc/>
    public IQueryable<ContentItem> GetUnIndexedContentItems()
    {
        return CBO.FillQueryable<ContentItem>(this.dataService.GetUnIndexedContentItems());
    }

    /// <inheritdoc/>
    public IQueryable<ContentItem> GetContentItemsByModuleId(int moduleId)
    {
        return CBO.FillQueryable<ContentItem>(this.dataService.GetContentItemsByModuleId(moduleId));
    }

    /// <inheritdoc/>
    public void UpdateContentItem(ContentItem contentItem)
    {
        // Argument Contract
        Requires.NotNull("contentItem", contentItem);
        Requires.PropertyNotNegative("contentItem", "ContentItemId", contentItem.ContentItemId);

        AttachmentController.SerializeAttachmentMetadata(contentItem);

        this.SaveMetadataDelta(contentItem);

        var userId = UserController.Instance.GetCurrentUserInfo().UserID;
        this.dataService.UpdateContentItem(contentItem, userId);
        contentItem.LastModifiedByUserID = userId;

        UpdateContentItemsCache(contentItem);
    }

    /// <inheritdoc/>
    public void AddMetaData(ContentItem contentItem, string name, string value)
    {
        // Argument Contract
        Requires.NotNull("contentItem", contentItem);
        Requires.PropertyNotNegative("contentItem", "ContentItemId", contentItem.ContentItemId);
        Requires.NotNullOrEmpty("name", name);

        this.dataService.AddMetaData(contentItem, name, value);

        UpdateContentItemsCache(contentItem, false);
    }

    /// <inheritdoc/>
    public void DeleteMetaData(ContentItem contentItem, string name, string value)
    {
        // Argument Contract
        Requires.NotNull("contentItem", contentItem);
        Requires.PropertyNotNegative("contentItem", "ContentItemId", contentItem.ContentItemId);
        Requires.NotNullOrEmpty("name", name);

        this.dataService.DeleteMetaData(contentItem, name, value);

        UpdateContentItemsCache(contentItem, false);
    }

    /// <inheritdoc/>
    public void DeleteMetaData(ContentItem contentItem, string name)
    {
        if (contentItem.Metadata.AllKeys.Contains(name))
        {
            this.DeleteMetaData(contentItem, name, contentItem.Metadata[name]);
        }
    }

    /// <inheritdoc/>
    public NameValueCollection GetMetaData(int contentItemId)
    {
        // Argument Contract
        Requires.NotNegative("contentItemId", contentItemId);

        var metadata = new NameValueCollection();

        using (var dr = this.dataService.GetMetaData(contentItemId))
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

    /// <inheritdoc/>
    protected override Func<IContentController> GetFactory()
    {
        return () => new ContentController();
    }

    private static bool MetaDataChanged(
        IEnumerable<KeyValuePair<string, string>> lh,
        IEnumerable<KeyValuePair<string, string>> rh)
    {
        return lh.SequenceEqual(rh, new NameValueEqualityComparer()) == false;
    }

    private static void UpdateContentItemsCache(ContentItem contentItem, bool readdItem = true)
    {
        DataCache.RemoveCache(GetContentItemCacheKey(contentItem.ContentItemId)); // remove first to synch web-farm servers
        if (readdItem)
        {
            CBO.Instance.GetCachedObject<ContentItem>(
                new CacheItemArgs(
                    GetContentItemCacheKey(contentItem.ContentItemId),
                    DataCache.ContentItemsCacheTimeOut,
                    DataCache.ContentItemsCachePriority),
                c => contentItem,
                false);
        }
    }

    private static string GetContentItemCacheKey(int contetnItemId)
    {
        return string.Format(DataCache.ContentItemsCacheKey, contetnItemId);
    }

    private void SaveMetadataDelta(ContentItem contentItem)
    {
        var persisted = this.GetMetaData(contentItem.ContentItemId);

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

        this.dataService.SynchronizeMetaData(contentItem, added, deleted);

        UpdateContentItemsCache(contentItem, false);
    }
}
