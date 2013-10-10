#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.Http;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.DataStructures;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Common;

namespace DotNetNuke.Web.InternalServices
{
    [DnnAuthorize]
    public class ItemListServiceController : DnnApiController
    {
        private const string PortalPrefix = "P-";
        private const string RootKey = "Root";

        #region Dtos

        [DataContract]
        public class ItemDto
        {
            [DataMember(Name = "key")]
            public string Key { get; set; }

            [DataMember(Name = "value")]
            public string Value { get; set; }

            [DataMember(Name = "hasChildren")]
            public bool HasChildren { get; set; }

            [DataMember(Name = "selectable")]
            public bool Selectable { get; set; }
        }

        [DataContract]
        public class ItemIdDto
        {
            [DataMember(Name = "id")]
            public string Id { get; set; }
        }

        #endregion

        #region Web Method

        [HttpGet]
		public HttpResponseMessage GetPageDescendants(string parentId = null, int sortOrder = 0, string searchText = "", int portalId = -1, bool includeDisabled = false, bool includeAllTypes = false)
        {
            var response = new
            {
                Success = true,
                Items = GetPageDescendantsInternal(portalId, parentId, sortOrder, searchText, includeDisabled, includeAllTypes)
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
		public HttpResponseMessage GetTreePathForPage(string itemId, int sortOrder = 0, int portalId = -1, bool includeDisabled = false, bool includeAllTypes = false)
        {
            var response = new
            {
                Success = true,
                Tree = GetTreePathForPageInternal(portalId, itemId, sortOrder, includeDisabled, includeAllTypes),
                IgnoreRoot = true
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
		public HttpResponseMessage SortPages(string treeAsJson, int sortOrder = 0, string searchText = "", int portalId = -1, bool includeDisabled = false, bool includeAllTypes = false)
        {
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? SortPagesInternal(portalId, treeAsJson, sortOrder, includeDisabled, includeAllTypes) 
							: SearchPagesInternal(portalId, searchText, sortOrder, includeDisabled, includeAllTypes),
                IgnoreRoot = true
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
		public HttpResponseMessage SortPagesInPortalGroup(string treeAsJson, int sortOrder = 0, string searchText = "", bool includeDisabled = false, bool includeAllTypes = false)
        {
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? SortPagesInPortalGroupInternal(treeAsJson, sortOrder, includeDisabled, includeAllTypes)
						: SearchPagesInPortalGroupInternal(treeAsJson, searchText, sortOrder, includeDisabled, includeAllTypes),
                IgnoreRoot = true
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
		public HttpResponseMessage GetPages(int sortOrder = 0, int portalId = -1, bool includeDisabled = false, bool includeAllTypes = false)
        {
            var response = new
            {
                Success = true,
				Tree = GetPagesInternal(portalId, sortOrder, includeDisabled, includeAllTypes),
                IgnoreRoot = true
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
		public HttpResponseMessage GetPagesInPortalGroup(int sortOrder = 0)
        {
            var response = new
            {
                Success = true,
                Tree = GetPagesInPortalGroupInternal(sortOrder),
                IgnoreRoot = true
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
		public HttpResponseMessage SearchPages(string searchText, int sortOrder = 0, int portalId = -1, bool includeDisabled = false, bool includeAllTypes = false)
        {
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? GetPagesInternal(portalId, sortOrder, includeDisabled, includeAllTypes)
							: SearchPagesInternal(portalId, searchText, sortOrder, includeDisabled, includeAllTypes),
                IgnoreRoot = true
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage GetFolderDescendants(string parentId = null, int sortOrder = 0, string searchText = "", string permission = null, int portalId = -1)
        {
            var response = new
            {
                Success = true,
                Items = GetFolderDescendantsInternal(portalId, parentId, sortOrder, searchText, permission)
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage GetFolders(int sortOrder = 0, string permission = null, int portalId = -1)
        {
            var response = new
            {
                Success = true,
                Tree = GetFoldersInternal(portalId, sortOrder, permission),
                IgnoreRoot = true
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage SortFolders(string treeAsJson, int sortOrder = 0, string searchText = "", string permission = null, int portalId = -1)
        {
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? SortFoldersInternal(portalId, treeAsJson, sortOrder, permission) : SearchFoldersInternal(portalId, searchText, sortOrder, permission),
                IgnoreRoot = true
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage GetTreePathForFolder(string itemId, int sortOrder = 0, string permission = null, int portalId = -1)
        {
            var response = new
            {
                Success = true,
                Tree = GetTreePathForFolderInternal(itemId, sortOrder, permission),
                IgnoreRoot = true
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage SearchFolders(string searchText, int sortOrder = 0, string permission = null, int portalId = -1)
        {
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? GetFoldersInternal(portalId, sortOrder, permission) : SearchFoldersInternal(portalId, searchText, sortOrder, permission),
                IgnoreRoot = true
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
		public HttpResponseMessage GetPageDescendantsInPortalGroup(string parentId = null, int sortOrder = 0, string searchText = "", bool includeDisabled = false, bool includeAllTypes = false)
        {
            var response = new
            {
                Success = true,
                Items = GetPageDescendantsInPortalGroupInternal(parentId, sortOrder, searchText, includeDisabled , includeAllTypes)
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
		public HttpResponseMessage GetTreePathForPageInPortalGroup(string itemId, int sortOrder = 0, bool includeDisabled = false, bool includeAllTypes = false)
        {
            var response = new
            {
                Success = true,
                Tree = GetTreePathForPageInternal(itemId, sortOrder, true, includeDisabled, includeAllTypes),
                IgnoreRoot = true
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
		public HttpResponseMessage SearchPagesInPortalGroup(string searchText, int sortOrder = 0, bool includeDisabled = false, bool includeAllTypes = false)
        {
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? GetPagesInPortalGroupInternal(sortOrder)
						: SearchPagesInPortalGroupInternal(searchText, sortOrder, includeDisabled, includeAllTypes),
                IgnoreRoot = true
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        #endregion

        #region Pages List

		private NTree<ItemDto> GetPagesInternal(int portalId, int sortOrder, bool includeDisabled = false, bool includeAllTypes = false)
        {
            if (portalId == -1)
            {
                portalId = GetActivePortalId();
            }
			var tabs = GetPortalPages(portalId, includeDisabled, includeAllTypes);
            var sortedTree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            if (tabs == null)
            {
                return sortedTree;
            }
            var children = ApplySort(GetChildrenOf(tabs, Null.NullInteger), sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            sortedTree.Children = children;
            return sortedTree;
        }

        private static NTree<ItemDto> GetPagesInPortalGroupInternal(int sortOrder)
        {
            var treeNode = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            var portals = GetPortalGroup(sortOrder);
            treeNode.Children = portals.Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            return treeNode;
        }

		private IEnumerable<ItemDto> GetPageDescendantsInPortalGroupInternal(string parentId, int sortOrder, string searchText, bool includeDisabled = false, bool includeAllTypes = false)
        {
            if (string.IsNullOrEmpty(parentId))
            {
                return null;
            }
            int portalId;
            int parentIdAsInt;
            if (parentId.StartsWith(PortalPrefix))
            {
                parentIdAsInt = -1;
                if (!int.TryParse(parentId.Replace(PortalPrefix, string.Empty), out portalId))
                {
                    portalId = -1;
                }
                if (!String.IsNullOrEmpty(searchText))
                {
                    return SearchPagesInternal(portalId, searchText, sortOrder, includeDisabled, includeAllTypes).Children.Select(node => node.Data);
                }
            }
            else
            {
                portalId = -1;
                if (!int.TryParse(parentId, out parentIdAsInt))
                {
                    parentIdAsInt = -1;
                }
            }
            return GetPageDescendantsInternal(portalId, parentIdAsInt, sortOrder, searchText, includeDisabled, includeAllTypes);
        }

		private IEnumerable<ItemDto> GetPageDescendantsInternal(int portalId, string parentId, int sortOrder, string searchText, bool includeDisabled = false, bool includeAllTypes = false)
        {
            int id;
            id = int.TryParse(parentId, out id) ? id : Null.NullInteger;
            return GetPageDescendantsInternal(portalId, id, sortOrder, searchText, includeDisabled , includeAllTypes);
        }

		private IEnumerable<ItemDto> GetPageDescendantsInternal(int portalId, int parentId, int sortOrder, string searchText, bool includeDisabled = false, bool includeAllTypes = false)
        {
            List<TabInfo> tabs;

            if (portalId == -1)
            {
                portalId = GetActivePortalId(parentId);
            }
            else
            {
                if (!IsPortalIdValid(portalId)) return new List<ItemDto>();
            }

            Func<TabInfo, bool> searchFunc;
            if (String.IsNullOrEmpty(searchText))
            {
                searchFunc = page => true;
            }
            else
            {
                searchFunc = page => page.LocalizedTabName.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) > -1;
            }

            if (portalId > -1)
            {
                var includeHiddenTabs = PortalSettings.UserInfo.IsSuperUser || PortalSettings.UserInfo.IsInRole("Administrators");
                tabs =
                    TabController.GetPortalTabs(portalId, Null.NullInteger, false, null, includeHiddenTabs, false, false, true, false)
                                 .Where(tab => searchFunc(tab) && tab.ParentId == parentId && (includeDisabled || !tab.DisableLink) && (includeAllTypes || tab.TabType == TabType.Normal))
                                 .OrderBy(tab => tab.TabOrder)
                                 .ToList();
            }
            else
            {
                if (PortalSettings.UserInfo.IsSuperUser)
                {
                    var tabController = new TabController();

                    tabs = tabController.GetTabsByPortal(-1).AsList()
                        .Where(tab => searchFunc(tab) && tab.ParentId == parentId && !tab.IsDeleted && !tab.DisableLink)
                        .OrderBy(tab => tab.TabOrder)
                        .ToList();
                }
                else
                {
                    return new List<ItemDto>();
                }
            }

            var pages = tabs.Select(tab => new ItemDto
            {
                Key = tab.TabID.ToString(CultureInfo.InvariantCulture),
                Value = tab.LocalizedTabName,
                HasChildren = tab.HasChildren,
                Selectable = true
            });

            return ApplySort(pages, sortOrder);
        }

		private NTree<ItemDto> SearchPagesInternal(int portalId, string searchText, int sortOrder, bool includeDisabled = false, bool includeAllTypes = false)
        {
            var tree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };

            List<TabInfo> tabs;
            if (portalId == -1)
            {
                portalId = GetActivePortalId();
            }
            else
            {
                if (!IsPortalIdValid(portalId))
                {
                    return tree;
                }
            }

            Func<TabInfo, bool> searchFunc;
            if (String.IsNullOrEmpty(searchText))
            {
                searchFunc = page => true;
            }
            else
            {
                searchFunc = page => page.LocalizedTabName.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) > -1;
            }

            var tabController = new TabController();
            var allTabs = tabController.GetAllTabs().ToArray().Select(tab => (TabInfo)tab).ToList();

            if (portalId > -1)
            {
                var includeHiddenTabs = PortalSettings.UserInfo.IsSuperUser || PortalSettings.UserInfo.IsInRole("Administrators");

                if (!includeHiddenTabs) allTabs = allTabs.Where(t => t.IsVisible).ToList();
                tabs = allTabs.Where(tab => tab.PortalID == portalId && (includeDisabled || !tab.DisableLink) && (includeAllTypes || tab.TabType == TabType.Normal) && searchFunc(tab))
                    .OrderBy(tab => tab.TabOrder)
                    .ToList();
            }
            else
            {
                if (PortalSettings.UserInfo.IsSuperUser)
                {
                    tabs = allTabs.Where(tab => tab.PortalID == -1 && !tab.DisableLink && searchFunc(tab))
                    .OrderBy(tab => tab.TabOrder)
                    .ToList();
                }
                else
                {
                    return tree;
                }
            }

            var pages = tabs.Select(tab => new ItemDto
            {
                Key = tab.TabID.ToString(CultureInfo.InvariantCulture),
                Value = tab.LocalizedTabName,
                HasChildren = false,
                Selectable = true
            });

            tree.Children = ApplySort(pages, sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            return tree;
        }

		private List<TabInfo> GetPortalPages(int portalId, bool includeDisabled = false, bool includeAllTypes = false)
        {
            List<TabInfo> tabs = null;
            if (portalId == -1)
            {
                portalId = GetActivePortalId();
            }
            else
            {
                if (!IsPortalIdValid(portalId))
                {
                    return null;
                }
            }

            if (portalId > -1)
            {
                var includeHiddenTabs = PortalSettings.UserInfo.IsSuperUser || PortalSettings.UserInfo.IsInRole("Administrators");
				tabs = TabController.GetPortalTabs(portalId, Null.NullInteger, false, null, includeHiddenTabs, false, includeAllTypes, true, false)
					.Where(t => !t.DisableLink || includeDisabled)
                    .ToList();
            }
            else
            {
                if (PortalSettings.UserInfo.IsSuperUser)
                {
                    var tabController = new TabController();
                    tabs = tabController.GetTabsByPortal(-1).AsList().Where(t => !t.IsDeleted && !t.DisableLink).ToList();
                }
            }
            return tabs;
        }

        private static IEnumerable<ItemDto> GetChildrenOf(IEnumerable<TabInfo> tabs, int parentId)
        {
            return tabs.Where(tab => tab.ParentId == parentId).Select(tab => new ItemDto
            {
                Key = tab.TabID.ToString(CultureInfo.InvariantCulture),
                Value = tab.LocalizedTabName,
                HasChildren = tab.HasChildren,
                Selectable = true
            }).ToList();
        }

        private static IEnumerable<ItemDto> GetChildrenOf(IEnumerable<TabInfo> tabs, string parentId)
        {
            int id;
            id = int.TryParse(parentId, out id) ? id : Null.NullInteger;
            return GetChildrenOf(tabs, id);
        }

		private NTree<ItemDto> SortPagesInternal(int portalId, string treeAsJson, int sortOrder, bool includeDisabled = false, bool includeAllTypes = false)
        {
            var tree = Json.Deserialize<NTree<ItemIdDto>>(treeAsJson);
            return SortPagesInternal(portalId, tree, sortOrder, includeDisabled, includeAllTypes);
        }

		private NTree<ItemDto> SortPagesInternal(int portalId, NTree<ItemIdDto> openedNodesTree, int sortOrder, bool includeDisabled = false, bool includeAllTypes = false)
        {
			var pages = GetPortalPages(portalId, includeDisabled, includeAllTypes);
            var sortedTree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            if (pages == null)
            {
                return sortedTree;
            }
            SortPagesRecursevely(pages, sortedTree, openedNodesTree, sortOrder);
            return sortedTree;
        }

		private NTree<ItemDto> SearchPagesInPortalGroupInternal(string searchText, int sortOrder, bool includeDisabled = false, bool includeAllTypes = false)
        {
            var treeNode = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            var portals = GetPortalGroup(sortOrder);
            treeNode.Children = portals.Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            foreach (var child in treeNode.Children)
            {
                int portalId;
                if (int.TryParse(child.Data.Key.Replace(PortalPrefix, string.Empty), out portalId))
                {
                    var pageTree = SearchPagesInternal(portalId, searchText, sortOrder, includeDisabled, includeAllTypes);
                    child.Children = pageTree.Children;
                }
            }
            return treeNode;
        }

		private NTree<ItemDto> SearchPagesInPortalGroupInternal(string treeAsJson, string searchText, int sortOrder, bool includeDisabled = false, bool includeAllTypes = false)
        {
            var treeNode = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            var openedNode = Json.Deserialize<NTree<ItemIdDto>>(treeAsJson);
            if (openedNode == null)
            {
                return treeNode;
            }

            var portals = GetPortalGroup(sortOrder);
            treeNode.Children = portals.Select(dto => new NTree<ItemDto> { Data = dto }).ToList();

            if (!openedNode.HasChildren())
            {
                return treeNode;
            }
            foreach (var openedNodeChild in openedNode.Children)
            {
                var portalIdString = openedNodeChild.Data.Id;
                var treeNodeChild = treeNode.Children.Find(child => String.Equals(child.Data.Key, portalIdString, StringComparison.InvariantCultureIgnoreCase));
                if (treeNodeChild == null)
                {
                    continue;
                }
                int portalId;
                if (int.TryParse(treeNodeChild.Data.Key.Replace(PortalPrefix, string.Empty), out portalId))
                {
                    var pageTree = SearchPagesInternal(portalId, searchText, sortOrder, includeDisabled, includeAllTypes);
                    treeNodeChild.Children = pageTree.Children;
                }
            }
            return treeNode;
        }

		private NTree<ItemDto> SortPagesInPortalGroupInternal(string treeAsJson, int sortOrder, bool includeDisabled = false, bool includeAllTypes = false)
        {
            var tree = Json.Deserialize<NTree<ItemIdDto>>(treeAsJson);
			return SortPagesInPortalGroupInternal(tree, sortOrder, includeDisabled, includeAllTypes);
        }

		private NTree<ItemDto> SortPagesInPortalGroupInternal(NTree<ItemIdDto> openedNode, int sortOrder, bool includeDisabled = false, bool includeAllTypes = false)
        {
            var treeNode = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            if (openedNode == null)
            {
                return treeNode;
            }
            var portals = GetPortalGroup(sortOrder);
            treeNode.Children = portals.Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            if (openedNode.HasChildren())
            {
                foreach (var openedNodeChild in openedNode.Children)
                {
                    var portalIdString = openedNodeChild.Data.Id;
                    var treeNodeChild = treeNode.Children.Find(child => String.Equals(child.Data.Key, portalIdString, StringComparison.InvariantCultureIgnoreCase));
                    if (treeNodeChild == null)
                    {
                        continue;
                    }
                    int portalId;
                    if (!int.TryParse(portalIdString.Replace(PortalPrefix, string.Empty), out portalId))
                    {
                        portalId = -1;
                    }
                    var treeOfPages = SortPagesInternal(portalId, openedNodeChild, sortOrder, includeDisabled, includeAllTypes);
                    treeNodeChild.Children = treeOfPages.Children;
                }
            }
            return treeNode;
        }

		private static void SortPagesRecursevely(IList<TabInfo> tabs, NTree<ItemDto> treeNode, NTree<ItemIdDto> openedNode, int sortOrder)
        {
            if (openedNode == null)
            {
                return;
            }
            var children = ApplySort(GetChildrenOf(tabs, openedNode.Data.Id), sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            treeNode.Children = children;
            if (openedNode.HasChildren())
            {
                foreach (var openedNodeChild in openedNode.Children)
                {
                    var treeNodeChild = treeNode.Children.Find(child => String.Equals(child.Data.Key, openedNodeChild.Data.Id, StringComparison.InvariantCultureIgnoreCase));
                    if (treeNodeChild == null)
                    {
                        continue;
                    }
                    SortPagesRecursevely(tabs, treeNodeChild, openedNodeChild, sortOrder);
                }
            }
        }

		private NTree<ItemDto> GetTreePathForPageInternal(int portalId, string itemId, int sortOrder, bool includePortalTree = false, bool includeDisabled = false, bool includeAllTypes = false)
        {
            int itemIdAsInt;
            if (string.IsNullOrEmpty(itemId) || !int.TryParse(itemId, out itemIdAsInt))
            {
                itemIdAsInt = Null.NullInteger;
            }
            return GetTreePathForPageInternal(portalId, itemIdAsInt, sortOrder, includePortalTree, includeDisabled, includeAllTypes);
        }

		private NTree<ItemDto> GetTreePathForPageInternal(string itemId, int sortOrder, bool includePortalTree = false, bool includeDisabled = false, bool includeAllTypes = false)
        {
            var tree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            int itemIdAsInt;
            if (string.IsNullOrEmpty(itemId) || !int.TryParse(itemId, out itemIdAsInt))
            {
                return tree;
            }
            var portals = PortalController.GetPortalDictionary();
            int portalId;
            if (portals.ContainsKey(itemIdAsInt))
            {
                portalId = portals[itemIdAsInt];
            }
            else
            {
                return tree;
            }
            return GetTreePathForPageInternal(portalId, itemIdAsInt, sortOrder, includePortalTree, includeDisabled, includeAllTypes);
        }

		private NTree<ItemDto> GetTreePathForPageInternal(int portalId, int selectedItemId, int sortOrder, bool includePortalTree = false, bool includeDisabled = false, bool includeAllTypes = false)
        {
            var tree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };

            if (selectedItemId <= 0)
            {
                return tree;
            }

            var pages = GetPortalPages(portalId, includeDisabled, includeAllTypes);

            if (pages == null)
            {
                return tree;
            }

            var page = pages.SingleOrDefault(pageInfo => pageInfo.TabID == selectedItemId);

            if (page == null)
            {
                return tree;
            }

            var selfTree = new NTree<ItemDto>
            {
                Data = new ItemDto
                {
                    Key = page.TabID.ToString(CultureInfo.InvariantCulture),
                    Value = page.LocalizedTabName,
                    HasChildren = page.HasChildren,
                    Selectable = true
                }
            };

            var parentId = page.ParentId;
            var parentTab = parentId > 0 ? pages.SingleOrDefault(t => t.TabID == parentId) : null;

            while (parentTab != null)
            {
                // load all sibiling
                var siblingTabs = GetChildrenOf(pages, parentId);
                siblingTabs = ApplySort(siblingTabs, sortOrder);
                var siblingTabsTree = siblingTabs.Select(t => new NTree<ItemDto> { Data = t }).ToList();

                // attach the tree
                if (selfTree.Children != null)
                {
                    foreach (var node in siblingTabsTree)
                    {
                        if (node.Data.Key == selfTree.Data.Key)
                        {
                            node.Children = selfTree.Children;
                            break;
                        }
                    }
                }

                selfTree = new NTree<ItemDto>
                {
                    Data = new ItemDto
                    {
                        Key = parentId.ToString(CultureInfo.InvariantCulture),
                        Value = parentTab.LocalizedTabName,
                        HasChildren = true,
                        Selectable = true
                    },
                    Children = siblingTabsTree
                };

                parentId = parentTab.ParentId;
                parentTab = parentId > 0 ? pages.SingleOrDefault(t => t.TabID == parentId) : null;
            }

            // retain root pages
            var rootTabs = GetChildrenOf(pages, Null.NullInteger);
            rootTabs = ApplySort(rootTabs, sortOrder);
            var rootTree = rootTabs.Select(dto => new NTree<ItemDto> { Data = dto }).ToList();

            foreach (var node in rootTree)
            {
                if (node.Data.Key == selfTree.Data.Key)
                {
                    node.Children = selfTree.Children;
                    break;
                }
            }

            if (includePortalTree)
            {
                var myGroup = GetMyPortalGroup();
                var portalTree = myGroup.Select(
                    portal => new NTree<ItemDto>
                    {
                        Data = new ItemDto
                        {
                            Key = PortalPrefix + portal.PortalID.ToString(CultureInfo.InvariantCulture),
                            Value = portal.PortalName,
                            HasChildren = true,
                            Selectable = false
                        }
                    }).ToList();

                foreach (var node in portalTree)
                {
                    if (node.Data.Key == PortalPrefix + portalId.ToString(CultureInfo.InvariantCulture))
                    {
                        node.Children = rootTree;
                        break;
                    }
                }
                rootTree = portalTree;
            }
            tree.Children = rootTree;

            return tree;
        }

        private static IEnumerable<ItemDto> GetPortalGroup(int sortOrder)
        {
            var mygroup = GetMyPortalGroup();
            var portals = mygroup.Select(p => new ItemDto
            {
                Key = PortalPrefix + p.PortalID.ToString(CultureInfo.InvariantCulture),
                Value = p.PortalName,
                HasChildren = true,
                Selectable = false
            }).ToList();
            return ApplySort(portals, sortOrder);
        }

        #endregion

        #region Folders List

        private NTree<ItemDto> GetFoldersInternal(int portalId, int sortOrder, string permissions)
        {
            var tree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            var children = ApplySort(GetFolderDescendantsInternal(portalId, -1, sortOrder, string.Empty, permissions), sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            tree.Children = children;
            foreach (var child in tree.Children)
            {
                children = ApplySort(GetFolderDescendantsInternal(portalId, child.Data.Key, sortOrder, string.Empty, permissions), sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
                child.Children = children;
            }
            return tree;
        }

        private NTree<ItemDto> SortFoldersInternal(int portalId, string treeAsJson, int sortOrder, string permissions)
        {
            var tree = Json.Deserialize<NTree<ItemIdDto>>(treeAsJson);
            return SortFoldersInternal(portalId, tree, sortOrder, permissions);
        }

        private NTree<ItemDto> SortFoldersInternal(int portalId, NTree<ItemIdDto> openedNodesTree, int sortOrder, string permissions)
        {
            var sortedTree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            SortFoldersRecursevely(portalId, sortedTree, openedNodesTree, sortOrder, permissions);
            return sortedTree;
        }

        private void SortFoldersRecursevely(int portalId, NTree<ItemDto> treeNode, NTree<ItemIdDto> openedNode, int sortOrder, string permissions)
        {
            if (openedNode == null)
            {
                return;
            }
            var children = ApplySort(GetFolderDescendantsInternal(portalId, openedNode.Data.Id, sortOrder, string.Empty, permissions), sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            treeNode.Children = children;
            if (openedNode.HasChildren())
            {
                foreach (var openedNodeChild in openedNode.Children)
                {
                    var treeNodeChild = treeNode.Children.Find(child => String.Equals(child.Data.Key, openedNodeChild.Data.Id, StringComparison.InvariantCultureIgnoreCase));
                    if (treeNodeChild == null)
                    {
                        continue;
                    }
                    SortFoldersRecursevely(portalId, treeNodeChild, openedNodeChild, sortOrder, permissions);
                }
            }
        }

        private IEnumerable<ItemDto> GetFolderDescendantsInternal(int portalId, string parentId, int sortOrder, string searchText, string permission)
        {
            int id;
            id = int.TryParse(parentId, out id) ? id : Null.NullInteger;
            return GetFolderDescendantsInternal(portalId, id, sortOrder, searchText, permission);
        }

        private IEnumerable<ItemDto> GetFolderDescendantsInternal(int portalId, int parentId, int sortOrder, string searchText, string permission)
        {
            if (portalId > -1)
            {
                if (!IsPortalIdValid(portalId)) return new List<ItemDto>();
            }
            else
            {
                portalId = GetActivePortalId();
            }

            var parentFolder = parentId > -1 ? FolderManager.Instance.GetFolder(parentId) : FolderManager.Instance.GetFolder(portalId, "");

            if (parentFolder == null)
            {
                return new List<ItemDto>();
            }

            var hasPermission = string.IsNullOrEmpty(permission) ?
                (HasPermission(parentFolder, "BROWSE") || HasPermission(parentFolder, "READ")) :
                HasPermission(parentFolder, permission.ToUpper());
            if (!hasPermission) return new List<ItemDto>();

            if (parentId < 1) return new List<ItemDto> { new ItemDto
                {
                    Key = parentFolder.FolderID.ToString(CultureInfo.InvariantCulture), 
                    Value = portalId == -1 ? SharedConstants.HostRootFolder : SharedConstants.RootFolder,
                    HasChildren = HasChildren(parentFolder, permission),
                    Selectable = true
                } };

            var childrenFolders = GetFolderDescendants(parentFolder, searchText, permission);

            var folders = childrenFolders.Select(folder => new ItemDto
            {
                Key = folder.FolderID.ToString(CultureInfo.InvariantCulture),
                Value = folder.FolderName,
                HasChildren = HasChildren(folder, permission),
                Selectable = true
            });

            return ApplySort(folders, sortOrder);
        }

        private NTree<ItemDto> SearchFoldersInternal(int portalId, string searchText, int sortOrder, string permission)
        {
            var tree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };

            if (portalId > -1)
            {
                if (!IsPortalIdValid(portalId))
                {
                    return tree;
                }
            }
            else
            {
                portalId = GetActivePortalId();
            }

            var allFolders = GetPortalFolders(portalId, searchText, permission);
            var folders = allFolders.Select(f => new ItemDto
            {
                Key = f.FolderID.ToString(CultureInfo.InvariantCulture),
                Value = f.FolderName,
                HasChildren = false,
                Selectable = true
            });
            tree.Children = ApplySort(folders, sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            return tree;
        }

        private NTree<ItemDto> GetTreePathForFolderInternal(string selectedItemId, int sortOrder, string permission)
        {
            var tree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };

            int itemId;
            if (string.IsNullOrEmpty(selectedItemId) || !int.TryParse(selectedItemId, out itemId))
            {
                return tree;
            }

            if (itemId <= 0)
            {
                return tree;
            }

            var folder = FolderManager.Instance.GetFolder(itemId);
            if (folder == null)
            {
                return tree;
            }

            var hasPermission = string.IsNullOrEmpty(permission) ?
                (HasPermission(folder, "BROWSE") || HasPermission(folder, "READ")) :
                HasPermission(folder, permission.ToUpper());
            if (!hasPermission) return new NTree<ItemDto>();

            var selfTree = new NTree<ItemDto>
            {
                Data = new ItemDto
                {
                    Key = folder.FolderID.ToString(CultureInfo.InvariantCulture),
                    Value = folder.FolderName,
                    HasChildren = HasChildren(folder, permission),
                    Selectable = true
                }
            };
            var parentId = folder.ParentID;
            var parentFolder = parentId > 0 ? FolderManager.Instance.GetFolder(parentId) : null;

            while (parentFolder != null)
            {
                // load all sibling
                var siblingFolders = GetFolderDescendants(parentFolder, string.Empty, permission)
                    .Select(folderInfo => new ItemDto
                    {
                        Key = folderInfo.FolderID.ToString(CultureInfo.InvariantCulture),
                        Value = folderInfo.FolderName,
                        HasChildren = HasChildren(folderInfo, permission),
                        Selectable = true
                    }).ToList();
                siblingFolders = ApplySort(siblingFolders, sortOrder).ToList();

                var siblingFoldersTree = siblingFolders.Select(f => new NTree<ItemDto> { Data = f }).ToList();

                // attach the tree
                if (selfTree.Children != null)
                {
                    foreach (var node in siblingFoldersTree)
                    {
                        if (node.Data.Key == selfTree.Data.Key)
                        {
                            node.Children = selfTree.Children;
                            break;
                        }
                    }
                }

                selfTree = new NTree<ItemDto>
                {
                    Data = new ItemDto
                    {
                        Key = parentId.ToString(CultureInfo.InvariantCulture),
                        Value = parentFolder.FolderName,
                        HasChildren = true,
                        Selectable = true
                    },
                    Children = siblingFoldersTree
                };

                parentId = parentFolder.ParentID;
                parentFolder = parentId > 0 ? FolderManager.Instance.GetFolder(parentId) : null;
            }
            selfTree.Data.Value = SharedConstants.RootFolder;

            tree.Children.Add(selfTree);
            return tree;
        }

        private bool HasPermission(IFolderInfo folder, string permissionKey)
        {
            var hasPermision = PortalSettings.UserInfo.IsSuperUser;

            if (!hasPermision && folder != null)
            {
                hasPermision = FolderPermissionController.HasFolderPermission(folder.FolderPermissions, permissionKey);
            }

            return hasPermision;
        }

        private IEnumerable<IFolderInfo> GetFolderDescendants(IFolderInfo parentFolder, string searchText, string permission)
        {
            Func<IFolderInfo, bool> searchFunc;
            if (String.IsNullOrEmpty(searchText))
            {
                searchFunc = folder => true;
            }
            else
            {
                searchFunc = folder => folder.FolderName.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) > -1;
            }
            permission = string.IsNullOrEmpty(permission) ? null : permission.ToUpper();
            return FolderManager.Instance.GetFolders(parentFolder).Where(folder =>
                (string.IsNullOrEmpty(permission) ?
                    (HasPermission(folder, "BROWSE") || HasPermission(folder, "READ")) :
                    (HasPermission(folder, permission))
                ) && searchFunc(folder));
        }

        private IEnumerable<IFolderInfo> GetPortalFolders(int portalId, string searchText, string permission)
        {
            if (portalId == -1)
            {
                portalId = GetActivePortalId();
            }
            Func<IFolderInfo, bool> searchFunc;
            if (String.IsNullOrEmpty(searchText))
            {
                searchFunc = folder => true;
            }
            else
            {
                searchFunc = folder => folder.FolderName.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) > -1;
            }
            permission = string.IsNullOrEmpty(permission) ? null : permission.ToUpper();
            return FolderManager.Instance.GetFolders(portalId).Where(folder =>
                (string.IsNullOrEmpty(permission) ?
                    (HasPermission(folder, "BROWSE") || HasPermission(folder, "READ")) :
                    (HasPermission(folder, permission))
                ) && searchFunc(folder));
        }

        private bool HasChildren(IFolderInfo parentFolder, string permission)
        {
            permission = string.IsNullOrEmpty(permission) ? null : permission.ToUpper();
            return FolderManager.Instance.GetFolders(parentFolder).Any(folder =>
                (string.IsNullOrEmpty(permission) ?
                    (HasPermission(folder, "BROWSE") || HasPermission(folder, "READ")) :
                    (HasPermission(folder, permission))
                )
            );
        }

        #endregion

        #region Sort

        private static IEnumerable<ItemDto> ApplySort(IEnumerable<ItemDto> items, int sortOrder)
        {
            switch (sortOrder)
            {
                case 1: // sort by a-z
                    return items.OrderBy(item => item.Value).ToList();
                case 2: // sort by z-a
                    return items.OrderByDescending(item => item.Value).ToList();
                default: // no sort
                    return items;
            }
        }

        #endregion

        #region check portal permission

        private static IEnumerable<PortalInfo> GetMyPortalGroup()
        {
            var groups = PortalGroupController.Instance.GetPortalGroups().ToArray();
            var mygroup = (from @group in groups
                           select PortalGroupController.Instance.GetPortalsByGroup(@group.PortalGroupId)
                               into portals
                               where portals.Any(x => x.PortalID == PortalSettings.Current.PortalId)
                               select portals.ToArray()).FirstOrDefault();
            return mygroup;
        }

        private bool IsPortalIdValid(int portalId)
        {
            if (UserInfo.IsSuperUser) return true;
            var isAdminUser = PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName);
            if (!isAdminUser) return false;

            if (PortalSettings.PortalId == portalId) return true;

            var mygroup = GetMyPortalGroup();
            return (mygroup != null && mygroup.Any(p => p.PortalID == portalId));
        }

        private int GetActivePortalId(int pageId)
        {
            var page = new TabController().GetTab(pageId, Null.NullInteger, false);
            var portalId = page.PortalID;

            if (portalId == Null.NullInteger)
            {
                portalId = GetActivePortalId();
            }
            return portalId;
        }

        private int GetActivePortalId()
        {
            var portalId = -1;
            if (!TabController.CurrentPage.IsSuperTab)
                portalId = PortalSettings.PortalId;

            return portalId;
        }

        #endregion

    }
}
