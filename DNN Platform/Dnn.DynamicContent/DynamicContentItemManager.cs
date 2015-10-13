// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;

namespace Dnn.DynamicContent
{
    public class DynamicContentItemManager : ServiceLocator<IDynamicContentItemManager, DynamicContentItemManager>, IDynamicContentItemManager
    {
        #region Fields
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DynamicContentItemManager));

        private Queue<SearchDocument> SearchDocumentsQueue;
        private readonly int MaxIndexingThreads;
        private int threadsCount = 0;

        #endregion

        protected override Func<IDynamicContentItemManager> GetFactory()
        {
            return () => new DynamicContentItemManager();
        }

        #region Constructors

        public DynamicContentItemManager()
        {
            SearchDocumentsQueue = new Queue<SearchDocument>();
            MaxIndexingThreads = Environment.ProcessorCount;
        }

        #endregion

        #region Public Methods

        public int AddContentItem(DynamicContentItem dynamicContent)
        {
            Requires.NotNull(dynamicContent);
            Requires.PropertyNotNull(dynamicContent, "ContentType");
            Requires.PropertyNotNegative(dynamicContent.ContentType, "ContentTypeId");
            Requires.PropertyNotNegative(dynamicContent, "ModuleId");

            var contentItem = new ContentItem
                                    {
                                        ContentTypeId = dynamicContent.ContentType.ContentTypeId,
                                        Content = dynamicContent.ToJson(),
                                        ModuleID = dynamicContent.ModuleId,
                                        TabID = dynamicContent.TabId,
                                        ContentKey = String.Empty
                                    };

            contentItem.ContentItemId = ContentController.Instance.AddContentItem(contentItem);

            SaveSearchDocument(dynamicContent, contentItem);

            return contentItem.ContentItemId;
        }

        public DynamicContentItem CreateContentItem(int portalId, int tabId, int moduleId, DynamicContentType contentType)
        {
            Requires.NotNegative("portalId", portalId);
            Requires.NotNegative("moduleId", moduleId);
            Requires.NotNegative("tabId", tabId);
            Requires.NotNull("contentType", contentType);

            if (contentType.FieldDefinitions.Count == 0)
            {
                throw new InvalidOperationException("The content type has no fields defined.");
            }
            return new DynamicContentItem(portalId, contentType) { ModuleId = moduleId, TabId = tabId };
        }

        public DynamicContentItem CreateContentItem(ContentItem contentItem)
        {
            Requires.NotNull("contentItem", contentItem);
            Requires.PropertyNotNegative(contentItem, "TabID");
            Requires.PropertyNotNegative(contentItem, "ModuleID");

            var module = ModuleController.Instance.GetModule(contentItem.ModuleID, contentItem.TabID, false);
            var dynamicContentItem = new DynamicContentItem(module.PortalID)
                                            {
                                                ContentItemId = contentItem.ContentItemId,
                                                ModuleId = contentItem.ModuleID,
                                                TabId = contentItem.TabID
                                            };

            dynamicContentItem.FromJson(contentItem.Content);

            return dynamicContentItem;
        }

        public void DeleteContentItem(DynamicContentItem dynamicContent)
        {
            Requires.NotNull(dynamicContent);
            Requires.PropertyNotNegative(dynamicContent, "ContentItemId");

            ContentController.Instance.DeleteContentItem(dynamicContent.ContentItemId);

            DeleteSearchDocument(dynamicContent);
        }

        public DynamicContentItem GetContentItem(int contentItemId)
        {
            Requires.NotNegative("contentItemId", contentItemId);

            var contentItem = ContentController.Instance.GetContentItem(contentItemId);
            DynamicContentItem dynamicContentItem = null;
            if (contentItem != null)
            {
                dynamicContentItem = CreateContentItem(contentItem);
            }

            return dynamicContentItem;
        }

        public IQueryable<DynamicContentItem> GetContentItems(int moduleId, int contentTypeId)
        {
            Requires.NotNegative("moduleId", moduleId);
            Requires.NotNegative("contentTypeId", contentTypeId);

            var contentItems = ContentController.Instance.GetContentItemsByModuleId(moduleId) .Where(c => c.ContentTypeId == contentTypeId);
            var dynamicContentItems = new List<DynamicContentItem>();
            foreach (var contentItem in contentItems)
            {
                dynamicContentItems.Add(CreateContentItem(contentItem));
            }

            return dynamicContentItems.AsQueryable();
        }

        public void UpdateContentItem(DynamicContentItem dynamicContent)
        {
            Requires.NotNull(dynamicContent);
            Requires.PropertyNotNegative(dynamicContent, "ContentItemId");
            Requires.PropertyNotNull(dynamicContent, "ContentType");
            Requires.PropertyNotNegative(dynamicContent.ContentType, "ContentTypeId");
            Requires.PropertyNotNegative(dynamicContent, "ModuleId");
            Requires.PropertyNotNegative(dynamicContent, "TabId");

            var contentItem = new ContentItem
                                {
                                    ContentItemId = dynamicContent.ContentItemId,
                                    ContentTypeId = dynamicContent.ContentType.ContentTypeId,
                                    Content = dynamicContent.ToJson(),
                                    ModuleID = dynamicContent.ModuleId,
                                    TabID = dynamicContent.TabId,
                                    ContentKey = String.Empty
                                };

            ContentController.Instance.UpdateContentItem(contentItem);

            SaveSearchDocument(dynamicContent, contentItem);
        }

        #endregion

        #region Private Methods

        private void SaveSearchDocument(DynamicContentItem dynamicContent, ContentItem contentItem)
        {
            var searchDoc = GenerateSearchDocument(dynamicContent, contentItem);
            if (searchDoc == null)
            {
                return;
            }

            if (threadsCount >= MaxIndexingThreads)
            {
                lock (SearchDocumentsQueue)
                {
                    SearchDocumentsQueue.Enqueue(searchDoc);
                }
            }
            else
            {
                threadsCount++;
                var processThread = new Thread(() => SaveSearchDocumentThread(searchDoc)) { IsBackground = true };
                processThread.Start();
            }
        }

        private void SaveSearchDocumentThread(SearchDocument searchDoc)
        {
            try
            {
                InternalSearchController.Instance.AddSearchDocument(searchDoc);

                while (SearchDocumentsQueue.Count > 0)
                {
                    lock (SearchDocumentsQueue)
                    {
                        searchDoc = SearchDocumentsQueue.Dequeue();
                    }
                    InternalSearchController.Instance.AddSearchDocument(searchDoc);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                threadsCount--;
            }
        } 

        private void DeleteSearchDocument(DynamicContentItem dynamicContent)
        {
            var searchDoc = new SearchDocument
            {
                UniqueKey = dynamicContent.ContentItemId.ToString("D"),
                ModuleId = dynamicContent.ModuleId,
                TabId = dynamicContent.TabId,
                SearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId
            };

            InternalSearchController.Instance.DeleteSearchDocument(searchDoc);
        }

        private SearchDocument GenerateSearchDocument(DynamicContentItem dynamicContent, ContentItem contentItem)
        {
            var moduleInfo = ModuleController.Instance.GetModule(contentItem.ModuleID, contentItem.TabID, false);
            if (moduleInfo == null)
            {
                return null;
            }

            var searchDoc = new SearchDocument
            {
                UniqueKey = contentItem.ContentItemId.ToString("D"),
                PortalId = dynamicContent.PortalId,
                SearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId,
                Title = moduleInfo.ModuleTitle,
                Description = string.Empty,
                Body = GenerateSearchContent(dynamicContent),
                ModuleId = contentItem.ModuleID,
                ModuleDefId = moduleInfo.ModuleDefID,
                TabId = contentItem.TabID,
                ModifiedTimeUtc = DateTime.UtcNow
            };

            if (contentItem.Terms != null && contentItem.Terms.Count > 0)
            {
                searchDoc.Tags = CollectHierarchicalTags(contentItem.Terms);
            }

            return searchDoc;
        }

        private string GenerateSearchContent(DynamicContentItem dynamicContent)
        {
            var contents = (from f in dynamicContent.Content.Fields.Values select f.Value).ToArray();
            return string.Join(" ", contents);
        }

        private List<string> CollectHierarchicalTags(List<Term> terms)
        {
            Func<List<Term>, List<string>, List<string>> collectTagsFunc = null;
            collectTagsFunc = (ts, tags) =>
            {
                if (ts != null && ts.Count > 0)
                {
                    foreach (var t in ts)
                    {
                        tags.Add(t.Name);
                        tags.AddRange(collectTagsFunc(t.ChildTerms, new List<string>()));
                    }
                }
                return tags;
            };

            return collectTagsFunc(terms, new List<string>());
        }

        #endregion
    }
}
