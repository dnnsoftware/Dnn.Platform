// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        public SearchDocument GetSearchDocument(ModuleInfo moduleInfo, DynamicContentItem dynamicContentItem)
        {
            Requires.NotNull(dynamicContentItem);
            
            var searchDoc = new SearchDocument
            {
                UniqueKey = dynamicContentItem.ContentItemId.ToString("D"),
                PortalId = dynamicContentItem.PortalId,
                SearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId,
                Title = moduleInfo.ModuleTitle,
                Description = string.Empty,
                Body = GenerateSearchContent(dynamicContentItem),
                ModuleId = dynamicContentItem.ModuleId,
                ModuleDefId = moduleInfo.ModuleDefID,
                TabId = dynamicContentItem.TabId,
                ModifiedTimeUtc = DateTime.UtcNow
            };

            searchDoc = PopulateSearchDocument(searchDoc, dynamicContentItem);
            var contentItem = _contentController.GetContentItem(dynamicContentItem.ContentItemId);

            if (contentItem.Terms != null && contentItem.Terms.Count > 0)
            {
                searchDoc.Tags = CollectHierarchicalTags(contentItem.Terms);
            }

            return searchDoc;        
        }

        private SearchDocument PopulateSearchDocument(SearchDocument searchDoc, DynamicContentItem dynamicContentItem)
        {
            foreach (var dynamicContentField in dynamicContentItem.Content.Fields)
            {
                if (dynamicContentField.Value.Definition.IsReferenceType)
                {
                    var value = dynamicContentField.Value.Value as DynamicContentPart;
                    PopulateComplexField(searchDoc, dynamicContentField.Key, value);
                }
                else
                {
                    searchDoc.Keywords.Add(dynamicContentField.Key, dynamicContentField.Value.GetStringValue());
                }
            }
            return searchDoc;
        }

        private void PopulateComplexField(SearchDocument searchDoc, string parentFieldName, DynamicContentPart fieldValue)
        {
            foreach (var dynamicContentField in fieldValue.Fields)
            {
                if (dynamicContentField.Value.Definition.IsReferenceType)
                {
                    var value = dynamicContentField.Value.Value as DynamicContentPart;
                    PopulateComplexField(searchDoc, parentFieldName+"/"+dynamicContentField.Key, value);
                }
                else
                {
                    searchDoc.Keywords.Add(parentFieldName+"/"+dynamicContentField.Key, dynamicContentField.Value.GetStringValue());
                }
            }
        }

        private string GenerateSearchContent(DynamicContentItem dynamicContent)
        {
            var containedValues = dynamicContent.Content.Fields.Values.Select(f => f.GetStringValue());
            return string.Join(", ", containedValues);
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
