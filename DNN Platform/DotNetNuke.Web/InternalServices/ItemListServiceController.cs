// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.Serialization;
    using System.Web.Http;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.DataStructures;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Common;

    [DnnAuthorize]
    public class ItemListServiceController : DnnApiController
    {
        private const string PortalPrefix = "P-";
        private const string RootKey = "Root";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ItemListServiceController));

        [HttpGet]
        public HttpResponseMessage GetPageDescendants(string parentId = null, int sortOrder = 0,
            string searchText = "", int portalId = -1, bool includeDisabled = false,
            bool includeAllTypes = false, bool includeActive = true, bool includeHostPages = false,
            string roles = "", bool disabledNotSelectable = false)
        {
            var response = new
            {
                Success = true,
                Items = this.GetPageDescendantsInternal(portalId, parentId, sortOrder, searchText,
                    includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable),
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage GetTreePathForPage(string itemId, int sortOrder = 0, int portalId = -1,
            bool includeDisabled = false, bool includeAllTypes = false,
            bool includeActive = true, bool includeHostPages = false, string roles = "")
        {
            var response = new
            {
                Success = true,
                Tree = this.GetTreePathForPageInternal(portalId, itemId, sortOrder, false,
                    includeDisabled, includeAllTypes, includeActive, includeHostPages, roles),
                IgnoreRoot = true,
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage SortPages(string treeAsJson, int sortOrder = 0, string searchText = "",
            int portalId = -1, bool includeDisabled = false, bool includeAllTypes = false,
            bool includeActive = true, bool includeHostPages = false, string roles = "")
        {
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? this.SortPagesInternal(portalId, treeAsJson, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles)
                            : this.SearchPagesInternal(portalId, searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles),
                IgnoreRoot = true,
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage SortPagesInPortalGroup(string treeAsJson, int sortOrder = 0,
            string searchText = "", bool includeDisabled = false, bool includeAllTypes = false,
            bool includeActive = true, bool includeHostPages = false, string roles = "")
        {
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? this.SortPagesInPortalGroupInternal(treeAsJson, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles)
                    : this.SearchPagesInPortalGroupInternal(treeAsJson, searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles),
                IgnoreRoot = true,
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage GetPages(int sortOrder = 0, int portalId = -1,
            bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = true,
            bool includeHostPages = false, string roles = "", bool disabledNotSelectable = false)
        {
            var response = new
            {
                Success = true,
                Tree = this.GetPagesInternal(portalId, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable),
                IgnoreRoot = true,
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage GetPagesInPortalGroup(int sortOrder = 0)
        {
            var response = new
            {
                Success = true,
                Tree = GetPagesInPortalGroupInternal(sortOrder),
                IgnoreRoot = true,
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage SearchPages(string searchText, int sortOrder = 0, int portalId = -1,
            bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = true,
            bool includeHostPages = false, string roles = "")
        {
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? this.GetPagesInternal(portalId, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, false)
                    : this.SearchPagesInternal(portalId, searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles),
                IgnoreRoot = true,
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage GetPageDescendantsInPortalGroup(string parentId = null, int sortOrder = 0,
            string searchText = "", bool includeDisabled = false, bool includeAllTypes = false,
            bool includeActive = true, bool includeHostPages = false, string roles = "")
        {
            var response = new
            {
                Success = true,
                Items = this.GetPageDescendantsInPortalGroupInternal(parentId, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles),
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage GetTreePathForPageInPortalGroup(string itemId, int sortOrder = 0,
            bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = true,
            bool includeHostPages = false, string roles = "")
        {
            var response = new
            {
                Success = true,
                Tree = this.GetTreePathForPageInternal(itemId, sortOrder, true, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles),
                IgnoreRoot = true,
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage SearchPagesInPortalGroup(string searchText, int sortOrder = 0,
            bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = true,
            bool includeHostPages = false, string roles = "")
        {
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? GetPagesInPortalGroupInternal(sortOrder)
                    : this.SearchPagesInPortalGroupInternal(searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles),
                IgnoreRoot = true,
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage GetFolderDescendants(string parentId = null, int sortOrder = 0, string searchText = "", string permission = null, int portalId = -1)
        {
            var response = new
            {
                Success = true,
                Items = this.GetFolderDescendantsInternal(portalId, parentId, sortOrder, searchText, permission),
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage GetFolders(int sortOrder = 0, string permission = null, int portalId = -1)
        {
            var response = new
            {
                Success = true,
                Tree = this.GetFoldersInternal(portalId, sortOrder, permission),
                IgnoreRoot = true,
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage SortFolders(string treeAsJson, int sortOrder = 0, string searchText = "", string permission = null, int portalId = -1)
        {
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? this.SortFoldersInternal(portalId, treeAsJson, sortOrder, permission) : this.SearchFoldersInternal(portalId, searchText, sortOrder, permission),
                IgnoreRoot = true,
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage GetTreePathForFolder(string itemId, int sortOrder = 0, string permission = null, int portalId = -1)
        {
            var response = new
            {
                Success = true,
                Tree = this.GetTreePathForFolderInternal(itemId, sortOrder, permission),
                IgnoreRoot = true,
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage SearchFolders(string searchText, int sortOrder = 0, string permission = null, int portalId = -1)
        {
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? this.GetFoldersInternal(portalId, sortOrder, permission) : this.SearchFoldersInternal(portalId, searchText, sortOrder, permission),
                IgnoreRoot = true,
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage GetFiles(int parentId, string filter, int sortOrder = 0, string permission = null, int portalId = -1)
        {
            var response = new
            {
                Success = true,
                Tree = this.GetFilesInternal(portalId, parentId, filter, string.Empty, sortOrder, permission),
                IgnoreRoot = true,
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage SortFiles(int parentId, string filter, int sortOrder = 0, string searchText = "", string permission = null, int portalId = -1)
        {
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? this.SortFilesInternal(portalId, parentId, filter, sortOrder, permission) : this.GetFilesInternal(portalId, parentId, filter, searchText, sortOrder, permission),
                IgnoreRoot = true,
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage SearchFiles(int parentId, string filter, string searchText, int sortOrder = 0, string permission = null, int portalId = -1)
        {
            var response = new
            {
                Success = true,
                Tree = this.GetFilesInternal(portalId, parentId, filter, searchText, sortOrder, permission),
                IgnoreRoot = true,
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage SearchUser(string q)
        {
            try
            {
                var portalId = PortalController.GetEffectivePortalId(this.PortalSettings.PortalId);
                const int numResults = 5;

                // GetUsersAdvancedSearch doesn't accept a comma or a single quote in the query so we have to remove them for now. See issue 20224.
                q = q.Replace(",", string.Empty).Replace("'", string.Empty);
                if (q.Length == 0)
                {
                    return this.Request.CreateResponse<SearchResult>(HttpStatusCode.OK, null);
                }

                var results = UserController.Instance.GetUsersBasicSearch(portalId, 0, numResults, "DisplayName", true, "DisplayName", q)
                    .Select(user => new SearchResult
                    {
                        id = user.UserID,
                        name = user.DisplayName,
                        iconfile = UserController.Instance.GetUserProfilePictureUrl(user.UserID, 32, 32),
                    }).ToList();

                return this.Request.CreateResponse(HttpStatusCode.OK, results.OrderBy(sr => sr.name));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetTerms(string q, bool includeSystem, bool includeTags)
        {
            var portalId = PortalSettings.Current.PortalId;

            var vocabRep = Util.GetVocabularyController();
            var termRep = Util.GetTermController();

            var terms = new ArrayList();
            var vocabularies = from v in vocabRep.GetVocabularies()
                where (v.ScopeType.ScopeType == "Application"
                       || (v.ScopeType.ScopeType == "Portal" && v.ScopeId == portalId))
                      && (!v.IsSystem || includeSystem)
                      && (v.Name != "Tags" || includeTags)
                select v;

            foreach (var v in vocabularies)
            {
                terms.AddRange(new[]
                {
                    from t in termRep.GetTermsByVocabulary(v.VocabularyId)
                    where string.IsNullOrEmpty(q) || t.Name.IndexOf(q, StringComparison.InvariantCultureIgnoreCase) > -1
                    select new { text = t.Name, value = t.TermId },
                });
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, terms);
        }

        private static NTree<ItemDto> GetPagesInPortalGroupInternal(int sortOrder)
        {
            var treeNode = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            var portals = GetPortalGroup(sortOrder);
            treeNode.Children = portals.Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            return treeNode;
        }

        private static IEnumerable<ItemDto> GetChildrenOf(IEnumerable<TabInfo> tabs, int parentId, IList<int> filterTabs = null)
        {
            return tabs.Where(tab => tab.ParentId == parentId).Select(tab => new ItemDto
            {
                Key = tab.TabID.ToString(CultureInfo.InvariantCulture),
                Value = tab.LocalizedTabName,
                HasChildren = tab.HasChildren,
                Selectable = filterTabs == null || filterTabs.Contains(tab.TabID),
            }).ToList();
        }

        private static IEnumerable<ItemDto> GetChildrenOf(IEnumerable<TabInfo> tabs, string parentId)
        {
            int id;
            id = int.TryParse(parentId, out id) ? id : Null.NullInteger;
            return GetChildrenOf(tabs, id);
        }

        private static void SortPagesRecursevely(IEnumerable<TabInfo> tabs, NTree<ItemDto> treeNode, NTree<ItemIdDto> openedNode, int sortOrder)
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
                    var treeNodeChild = treeNode.Children.Find(child => string.Equals(child.Data.Key, openedNodeChild.Data.Id, StringComparison.InvariantCultureIgnoreCase));
                    if (treeNodeChild == null)
                    {
                        continue;
                    }

                    SortPagesRecursevely(tabs, treeNodeChild, openedNodeChild, sortOrder);
                }
            }
        }

        private static IEnumerable<ItemDto> GetPortalGroup(int sortOrder)
        {
            var mygroup = GetMyPortalGroup();
            var portals = mygroup.Select(p => new ItemDto
            {
                Key = PortalPrefix + p.PortalID.ToString(CultureInfo.InvariantCulture),
                Value = p.PortalName,
                HasChildren = true,
                Selectable = false,
            }).ToList();
            return ApplySort(portals, sortOrder);
        }

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

        private NTree<ItemDto> GetPagesInternal(int portalId, int sortOrder, bool includeDisabled = false,
            bool includeAllTypes = false, bool includeActive = false, bool includeHostPages = false,
            string roles = "", bool disabledNotSelectable = false)
        {
            if (portalId == -1)
            {
                portalId = this.GetActivePortalId();
            }

            var tabs = this.GetPortalPages(portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            var sortedTree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            if (tabs == null)
            {
                return sortedTree;
            }

            var filterTabs = this.FilterTabsByRole(tabs, roles, disabledNotSelectable);
            var children = ApplySort(GetChildrenOf(tabs, Null.NullInteger, filterTabs), sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            sortedTree.Children = children;
            return sortedTree;
        }

        private IEnumerable<ItemDto> GetPageDescendantsInPortalGroupInternal(string parentId, int sortOrder,
            string searchText, bool includeDisabled = false, bool includeAllTypes = false,
            bool includeActive = true, bool includeHostPages = false, string roles = "")
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

                if (!string.IsNullOrEmpty(searchText))
                {
                    return this.SearchPagesInternal(portalId, searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles).Children.Select(node => node.Data);
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

            return this.GetPageDescendantsInternal(portalId, parentIdAsInt, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
        }

        private IEnumerable<ItemDto> GetPageDescendantsInternal(int portalId, string parentId, int sortOrder,
            string searchText, bool includeDisabled = false, bool includeAllTypes = false,
            bool includeActive = true, bool includeHostPages = false, string roles = "", bool disabledNotSelectable = false)
        {
            int id;
            id = int.TryParse(parentId, out id) ? id : Null.NullInteger;
            return this.GetPageDescendantsInternal(portalId, id, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable);
        }

        private IEnumerable<ItemDto> GetPageDescendantsInternal(int portalId, int parentId, int sortOrder,
            string searchText, bool includeDisabled = false, bool includeAllTypes = false,
            bool includeActive = true, bool includeHostPages = false, string roles = "", bool disabledNotSelectable = false)
        {
            List<TabInfo> tabs;

            if (portalId == -1)
            {
                portalId = this.GetActivePortalId(parentId);
            }
            else
            {
                if (!this.IsPortalIdValid(portalId))
                {
                    return new List<ItemDto>();
                }
            }

            Func<TabInfo, bool> searchFunc;
            if (string.IsNullOrEmpty(searchText))
            {
                searchFunc = page => true;
            }
            else
            {
                searchFunc = page => page.LocalizedTabName.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) > -1;
            }

            if (portalId > -1)
            {
                tabs = TabController.GetPortalTabs(portalId, includeActive ? Null.NullInteger : this.PortalSettings.ActiveTab.TabID, false, null, true, false, includeAllTypes, true, false)
                    .Where(tab => searchFunc(tab)
                                  && tab.ParentId == parentId
                                  && (includeDisabled || !tab.DisableLink)
                                  && (includeAllTypes || tab.TabType == TabType.Normal)
                                  && !tab.IsSystem)
                    .OrderBy(tab => tab.TabOrder)
                    .ToList();

                if (this.PortalSettings.UserInfo.IsSuperUser && includeHostPages)
                {
                    tabs.AddRange(TabController.Instance.GetTabsByPortal(-1).AsList()
                        .Where(tab => searchFunc(tab) && tab.ParentId == parentId && !tab.IsDeleted && !tab.DisableLink && !tab.IsSystem)
                        .OrderBy(tab => tab.TabOrder)
                        .ToList());
                }
            }
            else
            {
                if (this.PortalSettings.UserInfo.IsSuperUser)
                {
                    tabs = TabController.Instance.GetTabsByPortal(-1).AsList()
                        .Where(tab => searchFunc(tab) && tab.ParentId == parentId && !tab.IsDeleted && !tab.DisableLink && !tab.IsSystem)
                        .OrderBy(tab => tab.TabOrder)
                        .ToList();
                }
                else
                {
                    return new List<ItemDto>();
                }
            }

            var filterTabs = this.FilterTabsByRole(tabs, roles, disabledNotSelectable);

            var pages = tabs.Select(tab => new ItemDto
            {
                Key = tab.TabID.ToString(CultureInfo.InvariantCulture),
                Value = tab.LocalizedTabName,
                HasChildren = tab.HasChildren,
                Selectable = filterTabs.Contains(tab.TabID),
            });

            return ApplySort(pages, sortOrder);
        }

        private NTree<ItemDto> SearchPagesInternal(int portalId, string searchText, int sortOrder,
            bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = true,
            bool includeHostPages = false, string roles = "", bool disabledNotSelectable = false)
        {
            var tree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };

            List<TabInfo> tabs;
            if (portalId == -1)
            {
                portalId = this.GetActivePortalId();
            }
            else
            {
                if (!this.IsPortalIdValid(portalId))
                {
                    return tree;
                }
            }

            Func<TabInfo, bool> searchFunc;
            if (string.IsNullOrEmpty(searchText))
            {
                searchFunc = page => true;
            }
            else
            {
                searchFunc = page => page.LocalizedTabName.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) > -1;
            }

            if (portalId > -1)
            {
                tabs = TabController.Instance.GetTabsByPortal(portalId).Where(tab =>
                        (includeActive || tab.Value.TabID != this.PortalSettings.ActiveTab.TabID)
                        && (includeDisabled || !tab.Value.DisableLink)
                        && (includeAllTypes || tab.Value.TabType == TabType.Normal)
                        && searchFunc(tab.Value)
                        && !tab.Value.IsSystem)
                    .OrderBy(tab => tab.Value.TabOrder)
                    .Select(tab => tab.Value)
                    .ToList();

                if (this.PortalSettings.UserInfo.IsSuperUser && includeHostPages)
                {
                    tabs.AddRange(TabController.Instance.GetTabsByPortal(-1).Where(tab => !tab.Value.DisableLink && searchFunc(tab.Value) && !tab.Value.IsSystem)
                        .OrderBy(tab => tab.Value.TabOrder)
                        .Select(tab => tab.Value)
                        .ToList());
                }
            }
            else
            {
                if (this.PortalSettings.UserInfo.IsSuperUser)
                {
                    tabs = TabController.Instance.GetTabsByPortal(-1).Where(tab => !tab.Value.DisableLink && searchFunc(tab.Value) && !tab.Value.IsSystem)
                        .OrderBy(tab => tab.Value.TabOrder)
                        .Select(tab => tab.Value)
                        .ToList();
                }
                else
                {
                    return tree;
                }
            }

            var filterTabs = this.FilterTabsByRole(tabs, roles, disabledNotSelectable);

            var pages = tabs.Select(tab => new ItemDto
            {
                Key = tab.TabID.ToString(CultureInfo.InvariantCulture),
                Value = tab.LocalizedTabName,
                HasChildren = false,
                Selectable = filterTabs.Contains(tab.TabID),
            });

            tree.Children = ApplySort(pages, sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            return tree;
        }

        private List<TabInfo> GetPortalPages(int portalId, bool includeDisabled = false,
            bool includeAllTypes = false, bool includeActive = false, bool includeHostPages = false,
            string roles = "")
        {
            List<TabInfo> tabs = null;
            if (portalId == -1)
            {
                portalId = this.GetActivePortalId();
            }
            else
            {
                if (!this.IsPortalIdValid(portalId))
                {
                    return null;
                }
            }

            if (portalId > -1)
            {
                tabs = TabController.GetPortalTabs(portalId, includeActive ? Null.NullInteger : this.PortalSettings.ActiveTab.TabID, false, null, true, false, includeAllTypes, true, false)
                    .Where(t => (!t.DisableLink || includeDisabled) && !t.IsSystem)
                    .ToList();

                if (this.PortalSettings.UserInfo.IsSuperUser && includeHostPages)
                {
                    tabs.AddRange(TabController.Instance.GetTabsByPortal(-1).AsList().Where(t => !t.IsDeleted && !t.DisableLink && !t.IsSystem).ToList());
                }
            }
            else
            {
                if (this.PortalSettings.UserInfo.IsSuperUser)
                {
                    tabs = TabController.Instance.GetTabsByPortal(-1).AsList().Where(t => !t.IsDeleted && !t.DisableLink && !t.IsSystem).ToList();
                }
            }

            return tabs;
        }

        private NTree<ItemDto> SortPagesInternal(int portalId, string treeAsJson, int sortOrder,
            bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = false,
            bool includeHostPages = false, string roles = "")
        {
            var tree = DotNetNuke.Common.Utilities.Json.Deserialize<NTree<ItemIdDto>>(treeAsJson);
            return this.SortPagesInternal(portalId, tree, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
        }

        private NTree<ItemDto> SortPagesInternal(int portalId, NTree<ItemIdDto> openedNodesTree, int sortOrder,
            bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = false,
            bool includeHostPages = false, string roles = "")
        {
            var pages = this.GetPortalPages(portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            var sortedTree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            if (pages == null)
            {
                return sortedTree;
            }

            SortPagesRecursevely(pages, sortedTree, openedNodesTree, sortOrder);
            return sortedTree;
        }

        private NTree<ItemDto> SearchPagesInPortalGroupInternal(string searchText, int sortOrder,
            bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = false,
            bool includeHostPages = false, string roles = "")
        {
            var treeNode = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            var portals = GetPortalGroup(sortOrder);
            treeNode.Children = portals.Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            foreach (var child in treeNode.Children)
            {
                int portalId;
                if (int.TryParse(child.Data.Key.Replace(PortalPrefix, string.Empty), out portalId))
                {
                    var pageTree = this.SearchPagesInternal(portalId, searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
                    child.Children = pageTree.Children;
                }
            }

            return treeNode;
        }

        private NTree<ItemDto> SearchPagesInPortalGroupInternal(string treeAsJson, string searchText,
            int sortOrder, bool includeDisabled = false, bool includeAllTypes = false,
            bool includeActive = false, bool includeHostPages = false, string roles = "")
        {
            var treeNode = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            var openedNode = DotNetNuke.Common.Utilities.Json.Deserialize<NTree<ItemIdDto>>(treeAsJson);
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
                var treeNodeChild = treeNode.Children.Find(child => string.Equals(child.Data.Key, portalIdString, StringComparison.InvariantCultureIgnoreCase));
                if (treeNodeChild == null)
                {
                    continue;
                }

                int portalId;
                if (int.TryParse(treeNodeChild.Data.Key.Replace(PortalPrefix, string.Empty), out portalId))
                {
                    var pageTree = this.SearchPagesInternal(portalId, searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
                    treeNodeChild.Children = pageTree.Children;
                }
            }

            return treeNode;
        }

        private NTree<ItemDto> SortPagesInPortalGroupInternal(string treeAsJson, int sortOrder,
            bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = false,
            bool includeHostPages = false, string roles = "")
        {
            var tree = DotNetNuke.Common.Utilities.Json.Deserialize<NTree<ItemIdDto>>(treeAsJson);
            return this.SortPagesInPortalGroupInternal(tree, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
        }

        private NTree<ItemDto> SortPagesInPortalGroupInternal(NTree<ItemIdDto> openedNode, int sortOrder,
            bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = false,
            bool includeHostPages = false, string roles = "")
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
                    var treeNodeChild = treeNode.Children.Find(child => string.Equals(child.Data.Key, portalIdString, StringComparison.InvariantCultureIgnoreCase));
                    if (treeNodeChild == null)
                    {
                        continue;
                    }

                    int portalId;
                    if (!int.TryParse(portalIdString.Replace(PortalPrefix, string.Empty), out portalId))
                    {
                        portalId = -1;
                    }

                    var treeOfPages = this.SortPagesInternal(portalId, openedNodeChild, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
                    treeNodeChild.Children = treeOfPages.Children;
                }
            }

            return treeNode;
        }

        private NTree<ItemDto> GetTreePathForPageInternal(int portalId, string itemId, int sortOrder,
            bool includePortalTree = false, bool includeDisabled = false, bool includeAllTypes = false,
            bool includeActive = false, bool includeHostPages = false, string roles = "")
        {
            int itemIdAsInt;
            if (string.IsNullOrEmpty(itemId) || !int.TryParse(itemId, out itemIdAsInt))
            {
                itemIdAsInt = Null.NullInteger;
            }

            return this.GetTreePathForPageInternal(portalId, itemIdAsInt, sortOrder, includePortalTree, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
        }

        private NTree<ItemDto> GetTreePathForPageInternal(string itemId, int sortOrder,
            bool includePortalTree = false, bool includeDisabled = false, bool includeAllTypes = false,
            bool includeActive = false, bool includeHostPages = false, string roles = "")
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

            return this.GetTreePathForPageInternal(portalId, itemIdAsInt, sortOrder, includePortalTree, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
        }

        private NTree<ItemDto> GetTreePathForPageInternal(int portalId, int selectedItemId,
            int sortOrder, bool includePortalTree = false, bool includeDisabled = false,
            bool includeAllTypes = false, bool includeActive = false, bool includeHostPages = false,
            string roles = "", bool disabledNotSelectable = false)
        {
            var tree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };

            if (selectedItemId <= 0)
            {
                return tree;
            }

            var pages = this.GetPortalPages(portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);

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
                },
            };

            var parentId = page.ParentId;
            var parentTab = parentId > 0 ? pages.SingleOrDefault(t => t.TabID == parentId) : null;
            var filterTabs = this.FilterTabsByRole(pages, roles, disabledNotSelectable);
            while (parentTab != null)
            {
                // load all sibiling
                var siblingTabs = GetChildrenOf(pages, parentId, filterTabs);
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
                        Selectable = true,
                    },
                    Children = siblingTabsTree,
                };

                parentId = parentTab.ParentId;
                parentTab = parentId > 0 ? pages.SingleOrDefault(t => t.TabID == parentId) : null;
            }

            // retain root pages
            var rootTabs = GetChildrenOf(pages, Null.NullInteger, filterTabs);
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
                        },
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

        private List<int> FilterTabsByRole(IList<TabInfo> tabs, string roles, bool disabledNotSelectable)
        {
            var filterTabs = new List<int>();
            if (!string.IsNullOrEmpty(roles))
            {
                var roleList = roles.Split(';').Select(int.Parse);

                filterTabs.AddRange(
                    tabs.Where(
                            t =>
                                t.TabPermissions.Cast<TabPermissionInfo>()
                                    .Any(p => roleList.Contains(p.RoleID) && p.UserID == Null.NullInteger && p.PermissionKey == "VIEW" && p.AllowAccess)).ToList()
                        .Where(t => !disabledNotSelectable || !t.DisableLink)
                        .Select(t => t.TabID));
            }
            else
            {
                filterTabs.AddRange(tabs.Where(t => !disabledNotSelectable || !t.DisableLink).Select(t => t.TabID));
            }

            return filterTabs;
        }

        private NTree<ItemDto> GetFoldersInternal(int portalId, int sortOrder, string permissions)
        {
            var tree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            var children = ApplySort(this.GetFolderDescendantsInternal(portalId, -1, sortOrder, string.Empty, permissions), sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            tree.Children = children;
            foreach (var child in tree.Children)
            {
                children = ApplySort(this.GetFolderDescendantsInternal(portalId, child.Data.Key, sortOrder, string.Empty, permissions), sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
                child.Children = children;
            }

            return tree;
        }

        private NTree<ItemDto> SortFoldersInternal(int portalId, string treeAsJson, int sortOrder, string permissions)
        {
            var tree = DotNetNuke.Common.Utilities.Json.Deserialize<NTree<ItemIdDto>>(treeAsJson);
            return this.SortFoldersInternal(portalId, tree, sortOrder, permissions);
        }

        private NTree<ItemDto> SortFoldersInternal(int portalId, NTree<ItemIdDto> openedNodesTree, int sortOrder, string permissions)
        {
            var sortedTree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            this.SortFoldersRecursevely(portalId, sortedTree, openedNodesTree, sortOrder, permissions);
            return sortedTree;
        }

        private void SortFoldersRecursevely(int portalId, NTree<ItemDto> treeNode, NTree<ItemIdDto> openedNode, int sortOrder, string permissions)
        {
            if (openedNode == null)
            {
                return;
            }

            var children = ApplySort(this.GetFolderDescendantsInternal(portalId, openedNode.Data.Id, sortOrder, string.Empty, permissions), sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            treeNode.Children = children;
            if (openedNode.HasChildren())
            {
                foreach (var openedNodeChild in openedNode.Children)
                {
                    var treeNodeChild = treeNode.Children.Find(child => string.Equals(child.Data.Key, openedNodeChild.Data.Id, StringComparison.InvariantCultureIgnoreCase));
                    if (treeNodeChild == null)
                    {
                        continue;
                    }

                    this.SortFoldersRecursevely(portalId, treeNodeChild, openedNodeChild, sortOrder, permissions);
                }
            }
        }

        private IEnumerable<ItemDto> GetFolderDescendantsInternal(int portalId, string parentId, int sortOrder, string searchText, string permission)
        {
            int id;
            id = int.TryParse(parentId, out id) ? id : Null.NullInteger;
            return this.GetFolderDescendantsInternal(portalId, id, sortOrder, searchText, permission);
        }

        private IEnumerable<ItemDto> GetFolderDescendantsInternal(int portalId, int parentId, int sortOrder, string searchText, string permission)
        {
            if (portalId > -1)
            {
                if (!this.IsPortalIdValid(portalId))
                {
                    return new List<ItemDto>();
                }
            }
            else
            {
                portalId = this.GetActivePortalId();
            }

            var parentFolder = parentId > -1 ? FolderManager.Instance.GetFolder(parentId) : FolderManager.Instance.GetFolder(portalId, string.Empty);

            if (parentFolder == null)
            {
                return new List<ItemDto>();
            }

            var hasPermission = string.IsNullOrEmpty(permission) ?
                (this.HasPermission(parentFolder, "BROWSE") || this.HasPermission(parentFolder, "READ")) :
                this.HasPermission(parentFolder, permission.ToUpper());
            if (!hasPermission)
            {
                return new List<ItemDto>();
            }

            if (parentId < 1)
            {
                return new List<ItemDto>
                {
                    new ItemDto
                    {
                        Key = parentFolder.FolderID.ToString(CultureInfo.InvariantCulture),
                        Value = portalId == -1 ? DynamicSharedConstants.HostRootFolder : DynamicSharedConstants.RootFolder,
                        HasChildren = this.HasChildren(parentFolder, permission),
                        Selectable = true
                    },
                };
            }

            var childrenFolders = this.GetFolderDescendants(parentFolder, searchText, permission);

            var folders = childrenFolders.Select(folder => new ItemDto
            {
                Key = folder.FolderID.ToString(CultureInfo.InvariantCulture),
                Value = folder.FolderName,
                HasChildren = this.HasChildren(folder, permission),
                Selectable = true,
            });

            return ApplySort(folders, sortOrder);
        }

        private NTree<ItemDto> SearchFoldersInternal(int portalId, string searchText, int sortOrder, string permission)
        {
            var tree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };

            if (portalId > -1)
            {
                if (!this.IsPortalIdValid(portalId))
                {
                    return tree;
                }
            }
            else
            {
                portalId = this.GetActivePortalId();
            }

            var allFolders = this.GetPortalFolders(portalId, searchText, permission);
            var folders = allFolders.Select(f => new ItemDto
            {
                Key = f.FolderID.ToString(CultureInfo.InvariantCulture),
                Value = f.FolderName,
                HasChildren = false,
                Selectable = true,
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
                (this.HasPermission(folder, "BROWSE") || this.HasPermission(folder, "READ")) :
                this.HasPermission(folder, permission.ToUpper());
            if (!hasPermission)
            {
                return new NTree<ItemDto>();
            }

            var selfTree = new NTree<ItemDto>
            {
                Data = new ItemDto
                {
                    Key = folder.FolderID.ToString(CultureInfo.InvariantCulture),
                    Value = folder.FolderName,
                    HasChildren = this.HasChildren(folder, permission),
                    Selectable = true
                },
            };
            var parentId = folder.ParentID;
            var parentFolder = parentId > 0 ? FolderManager.Instance.GetFolder(parentId) : null;

            while (parentFolder != null)
            {
                // load all sibling
                var siblingFolders = this.GetFolderDescendants(parentFolder, string.Empty, permission)
                    .Select(folderInfo => new ItemDto
                    {
                        Key = folderInfo.FolderID.ToString(CultureInfo.InvariantCulture),
                        Value = folderInfo.FolderName,
                        HasChildren = this.HasChildren(folderInfo, permission),
                        Selectable = true,
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
                        Selectable = true,
                    },
                    Children = siblingFoldersTree,
                };

                parentId = parentFolder.ParentID;
                parentFolder = parentId > 0 ? FolderManager.Instance.GetFolder(parentId) : null;
            }

            selfTree.Data.Value = DynamicSharedConstants.RootFolder;

            tree.Children.Add(selfTree);
            return tree;
        }

        private bool HasPermission(IFolderInfo folder, string permissionKey)
        {
            var hasPermision = this.PortalSettings.UserInfo.IsSuperUser;

            if (!hasPermision && folder != null)
            {
                hasPermision = FolderPermissionController.HasFolderPermission(folder.FolderPermissions, permissionKey);
            }

            return hasPermision;
        }

        private IEnumerable<IFolderInfo> GetFolderDescendants(IFolderInfo parentFolder, string searchText, string permission)
        {
            Func<IFolderInfo, bool> searchFunc;
            if (string.IsNullOrEmpty(searchText))
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
                    (this.HasPermission(folder, "BROWSE") || this.HasPermission(folder, "READ")) :
                    this.HasPermission(folder, permission)) && searchFunc(folder));
        }

        private IEnumerable<IFolderInfo> GetPortalFolders(int portalId, string searchText, string permission)
        {
            if (portalId == -1)
            {
                portalId = this.GetActivePortalId();
            }

            Func<IFolderInfo, bool> searchFunc;
            if (string.IsNullOrEmpty(searchText))
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
                    (this.HasPermission(folder, "BROWSE") || this.HasPermission(folder, "READ")) :
                    this.HasPermission(folder, permission)) && searchFunc(folder));
        }

        private bool HasChildren(IFolderInfo parentFolder, string permission)
        {
            permission = string.IsNullOrEmpty(permission) ? null : permission.ToUpper();
            return FolderManager.Instance.GetFolders(parentFolder).Any(folder =>
                (string.IsNullOrEmpty(permission) ?
                    (this.HasPermission(folder, "BROWSE") || this.HasPermission(folder, "READ")) :
                    this.HasPermission(folder, permission)));
        }

        private NTree<ItemDto> GetFilesInternal(int portalId, int parentId, string filter, string searchText, int sortOrder, string permissions)
        {
            var tree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            var children = this.GetFileItemsDto(portalId, parentId, filter, searchText, permissions, sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            tree.Children = children;
            return tree;
        }

        private NTree<ItemDto> SortFilesInternal(int portalId, int parentId, string filter, int sortOrder, string permissions)
        {
            var sortedTree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            var children = this.GetFileItemsDto(portalId, parentId, filter, string.Empty, permissions, sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            sortedTree.Children = children;
            return sortedTree;
        }

        private IEnumerable<ItemDto> GetFileItemsDto(int portalId, int parentId, string filter, string searchText, string permission, int sortOrder)
        {
            if (portalId > -1)
            {
                if (!this.IsPortalIdValid(portalId))
                {
                    return new List<ItemDto>();
                }
            }
            else
            {
                portalId = this.GetActivePortalId();
            }

            var parentFolder = parentId > -1 ? FolderManager.Instance.GetFolder(parentId) : FolderManager.Instance.GetFolder(portalId, string.Empty);

            if (parentFolder == null)
            {
                return new List<ItemDto>();
            }

            var hasPermission = string.IsNullOrEmpty(permission) ?
                (this.HasPermission(parentFolder, "BROWSE") || this.HasPermission(parentFolder, "READ")) :
                this.HasPermission(parentFolder, permission.ToUpper());
            if (!hasPermission)
            {
                return new List<ItemDto>();
            }

            if (parentId < 1)
            {
                return new List<ItemDto>();
            }

            var files = this.GetFiles(parentFolder, filter, searchText);

            var filesDto = files.Select(f => new ItemDto
            {
                Key = f.FileId.ToString(CultureInfo.InvariantCulture),
                Value = f.FileName,
                HasChildren = false,
                Selectable = true,
            }).ToList();

            var sortedList = ApplySort(filesDto, sortOrder);

            return sortedList;
        }

        private IEnumerable<IFileInfo> GetFiles(IFolderInfo parentFolder, string filter, string searchText)
        {
            Func<IFileInfo, bool> searchFunc;
            var filterList = string.IsNullOrEmpty(filter) ? null : filter.ToLowerInvariant().Split(',').ToList();
            if (string.IsNullOrEmpty(searchText))
            {
                searchFunc = f => filterList == null || filterList.Contains(f.Extension.ToLowerInvariant());
            }
            else
            {
                searchFunc = f => f.FileName.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) > -1
                                  && (filterList == null || filterList.Contains(f.Extension.ToLowerInvariant()));
            }

            return FolderManager.Instance.GetFiles(parentFolder).Where(f => searchFunc(f));
        }

        private bool IsPortalIdValid(int portalId)
        {
            if (this.UserInfo.IsSuperUser)
            {
                return true;
            }

            if (this.PortalSettings.PortalId == portalId)
            {
                return true;
            }

            var isAdminUser = PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName);
            if (!isAdminUser)
            {
                return false;
            }

            var mygroup = GetMyPortalGroup();
            return mygroup != null && mygroup.Any(p => p.PortalID == portalId);
        }

        private int GetActivePortalId(int pageId)
        {
            var page = TabController.Instance.GetTab(pageId, Null.NullInteger, false);
            var portalId = page.PortalID;

            if (portalId == Null.NullInteger)
            {
                portalId = this.GetActivePortalId();
            }

            return portalId;
        }

        private int GetActivePortalId()
        {
            var portalId = -1;
            if (!TabController.CurrentPage.IsSuperTab)
            {
                portalId = this.PortalSettings.PortalId;
            }

            return portalId;
        }

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

        /// <summary>
        /// This class stores a single search result needed by jQuery Tokeninput.
        /// </summary>
        private class SearchResult
        {
            // ReSharper disable InconsistentNaming
            // ReSharper disable NotAccessedField.Local
            public int id;
            public string name;
            public string iconfile;

            // ReSharper restore NotAccessedField.Local
            // ReSharper restore InconsistentNaming
        }
    }
}
