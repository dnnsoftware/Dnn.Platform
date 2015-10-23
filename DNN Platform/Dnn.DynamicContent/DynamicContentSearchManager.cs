// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;

namespace Dnn.DynamicContent
{
    public class DynamicContentSearchManager : ServiceLocator<IDynamicContentSearchManager, DynamicContentSearchManager>, IDynamicContentSearchManager
    {
        #region Members
        private readonly IContentController _contentController;
        #endregion

        public DynamicContentSearchManager()
        {
            _contentController = ContentController.Instance;
        }

        public SearchDocument GetSearchDocument(DynamicContentItem dynamicContent)
        {
            Requires.NotNull(dynamicContent);
            Requires.NotNegative("Content Item Id", dynamicContent.ContentItemId);

            var moduleInfo = ModuleController.Instance.GetModule(dynamicContent.ModuleId, dynamicContent.TabId, false);
            if (moduleInfo == null)
            {
                return null;
            }

            var searchDoc = new SearchDocument
            {
                UniqueKey = dynamicContent.ContentItemId.ToString("D"),
                PortalId = dynamicContent.PortalId,
                SearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId,
                Title = moduleInfo.ModuleTitle,
                Description = string.Empty,
                Body = GenerateSearchContent(dynamicContent),
                ModuleId = dynamicContent.ModuleId,
                ModuleDefId = moduleInfo.ModuleDefID,
                TabId = dynamicContent.TabId,
                ModifiedTimeUtc = DateTime.UtcNow
            };

            var contentItem = _contentController.GetContentItem(dynamicContent.ContentItemId);

            if (contentItem.Terms != null && contentItem.Terms.Count > 0)
            {
                searchDoc.Tags = CollectHierarchicalTags(contentItem.Terms);
            }

            return searchDoc;        
        }

        private string GenerateSearchContent(DynamicContentItem dynamicContent)
        {
            var containedValues = dynamicContent.Content.Fields.Values.Select(f => f.GetStringValue());
            return string.Join(",", containedValues);
        }

        private List<string> CollectHierarchicalTags(List<Term> terms)
        {
            Func<List<Term>, List<string>, List<string>> collectTagsFunc = null;
            collectTagsFunc = (ts, tags) =>
            {
                if (ts == null || ts.Count <= 0)
                {
                    return tags;
                }
                foreach (var t in ts)
                {
                    tags.Add(t.Name);
                    tags.AddRange(collectTagsFunc(t.ChildTerms, new List<string>()));
                }
                return tags;
            };

            return collectTagsFunc(terms, new List<string>());
        }

        protected override Func<IDynamicContentSearchManager> GetFactory()
        {
            return () => new DynamicContentSearchManager();
        }
    }
}
