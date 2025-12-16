// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.InternalServices
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Web.Http;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Internal;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    using Microsoft.Extensions.DependencyInjection;

    [DnnAuthorize]
    public class ControlBarController : DnnApiController
    {
        private const string DefaultExtensionImage = "icon_extensions_32px.png";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ControlBarController));
        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly PersonalizationController personalizationController;
        private readonly Components.Controllers.IControlBarController controller;
        private Dictionary<string, string> nameDics;

        /// <summary>Initializes a new instance of the <see cref="ControlBarController"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        public ControlBarController(IBusinessControllerProvider businessControllerProvider)
            : this(businessControllerProvider, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ControlBarController"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="personalizationController">The personalization controller.</param>
        public ControlBarController(IBusinessControllerProvider businessControllerProvider, PersonalizationController personalizationController)
        {
            this.businessControllerProvider = businessControllerProvider;
            this.personalizationController = personalizationController ?? Globals.GetCurrentServiceProvider().GetRequiredService<PersonalizationController>();
            this.controller = Components.Controllers.ControlBarController.Instance;
        }

        [HttpGet]
        [DnnPageEditor]
        public HttpResponseMessage GetPortalDesktopModules(string category, int loadingStartIndex, int loadingPageSize, string searchTerm, string excludeCategories = "", bool sortBookmarks = false, string topModule = "")
        {
            if (string.IsNullOrEmpty(category))
            {
                category = "All";
            }

            var bookmarCategory = this.controller.GetBookmarkCategory(PortalSettings.Current.PortalId);
            var bookmarkedModules = this.controller.GetBookmarkedDesktopModules(PortalSettings.Current.PortalId, UserController.Instance.GetCurrentUserInfo().UserID, searchTerm);
            var bookmarkCategoryModules = this.controller.GetCategoryDesktopModules(this.PortalSettings.PortalId, bookmarCategory, searchTerm);

            var filteredList = bookmarCategory == category ? bookmarkCategoryModules.OrderBy(m => m.Key).Union(bookmarkedModules.OrderBy(m => m.Key)).Distinct()
                                            : this.controller.GetCategoryDesktopModules(this.PortalSettings.PortalId, category, searchTerm).OrderBy(m => m.Key);

            if (!string.IsNullOrEmpty(excludeCategories))
            {
                var excludeList = excludeCategories.ToLowerInvariant().Split(',');
                filteredList =
                    filteredList.Where(kvp =>
                        !excludeList.Contains(kvp.Value.DesktopModule.Category.ToLowerInvariant()));
            }

            if (sortBookmarks)
            {
                // sort bookmarked modules
                filteredList = bookmarkedModules.OrderBy(m => m.Key).Concat(filteredList.Except(bookmarkedModules));

                // move Html on top
                filteredList = filteredList.Where(m => m.Key.Equals(topModule, StringComparison.OrdinalIgnoreCase)).
                                Concat(filteredList.Except(filteredList.Where(m => m.Key.Equals(topModule, StringComparison.OrdinalIgnoreCase))));
            }

            filteredList = filteredList
                .Skip(loadingStartIndex)
                .Take(loadingPageSize);

            var result = filteredList.Select(kvp => new ModuleDefDTO
            {
                ModuleID = kvp.Value.DesktopModuleID,
                ModuleName = kvp.Key,
                ModuleImage = GetDeskTopModuleImage(kvp.Value.DesktopModuleID),
                Bookmarked = bookmarkedModules.Any(m => m.Key == kvp.Key),
                ExistsInBookmarkCategory = bookmarkCategoryModules.Any(m => m.Key == kvp.Key),
            }).ToList();
            return this.Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [DnnPageEditor]
        public HttpResponseMessage GetPageList(string portal)
        {
            var portalSettings = this.GetPortalSettings(portal);

            List<TabInfo> tabList = null;
            if (this.PortalSettings.PortalId == portalSettings.PortalId)
            {
                tabList = TabController.GetPortalTabs(portalSettings.PortalId, this.PortalSettings.ActiveTab.TabID, false, string.Empty, true, false, false, false, true);
            }
            else
            {
                var groups = PortalGroupController.Instance.GetPortalGroups().ToArray();

                var mygroup = (from @group in groups
                               select PortalGroupController.Instance.GetPortalsByGroup(@group.PortalGroupId)
                                  into portals
                               where portals.Any(x => x.PortalID == PortalSettings.Current.PortalId)
                               select portals.ToArray()).FirstOrDefault();

                if (mygroup != null && mygroup.Any(p => p.PortalID == portalSettings.PortalId))
                {
                    tabList = TabController.GetPortalTabs(portalSettings.PortalId, Null.NullInteger, false, string.Empty, true, false, false, false, false);
                }
                else
                {
                    // try to get pages not allowed
                    return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }

            List<PageDefDTO> result = new List<PageDefDTO>();
            foreach (var tab in tabList)
            {
                if (tab.PortalID == this.PortalSettings.PortalId || (GetModules(tab.TabID).Count > 0 && tab.TabID != portalSettings.AdminTabId && tab.ParentId != portalSettings.AdminTabId))
                {
                    result.Add(new PageDefDTO { TabID = tab.TabID, IndentedTabName = tab.IndentedTabName });
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [DnnPageEditor]
        public HttpResponseMessage GetTabModules(string tab)
        {
            int tabID;

            if (int.TryParse(tab, out tabID))
            {
                var result = new List<ModuleDefDTO>();
                if (tabID > 0)
                {
                    var pageModules = GetModules(tabID);

                    Dictionary<int, string> resultDict = pageModules.ToDictionary(module => module.ModuleID, module => module.ModuleTitle);
                    result.AddRange(from kvp in resultDict
                                    let imageUrl = GetTabModuleImage(tabID, kvp.Key)
                                    select new ModuleDefDTO { ModuleID = kvp.Key, ModuleName = kvp.Value, ModuleImage = imageUrl });
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, result);
            }

            return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnPageEditor]
        public HttpResponseMessage CopyPermissionsToChildren()
        {
            if (TabPermissionController.CanManagePage() && UserController.Instance.GetCurrentUserInfo().IsInRole("Administrators")
                                                        && this.ActiveTabHasChildren() && !this.PortalSettings.ActiveTab.IsSuperTab)
            {
                TabController.CopyPermissionsToChildren(this.PortalSettings.ActiveTab, this.PortalSettings.ActiveTab.TabPermissions);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }

            return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnPageEditor]
        public HttpResponseMessage AddModule(AddModuleDTO dto)
        {
            if (TabPermissionController.CanAddContentToPage() && this.CanAddModuleToPage())
            {
                int permissionType;
                try
                {
                    permissionType = int.Parse(dto.Visibility, CultureInfo.InvariantCulture);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                    permissionType = 0;
                }

                int positionId = -1;
                if (!string.IsNullOrEmpty(dto.Sort))
                {
                    try
                    {
                        var sortId = int.Parse(dto.Sort, CultureInfo.InvariantCulture);
                        if (sortId >= 0)
                        {
                            positionId = GetPaneModuleOrder(dto.Pane, sortId);
                        }
                    }
                    catch (Exception exc)
                    {
                        Logger.Error(exc);
                    }
                }

                if (positionId == -1)
                {
                    switch (dto.Position)
                    {
                        case "TOP":
                        case "0":
                            positionId = 0;
                            break;
                        case "BOTTOM":
                        case "-1":
                            positionId = -1;
                            break;
                    }
                }

                int moduleLstId;
                try
                {
                    moduleLstId = int.Parse(dto.Module, CultureInfo.InvariantCulture);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                    moduleLstId = -1;
                }

                try
                {
                    int tabModuleId = -1;
                    if (moduleLstId > -1)
                    {
                        if (dto.AddExistingModule == "true")
                        {
                            int pageId;
                            try
                            {
                                pageId = int.Parse(dto.Page, CultureInfo.InvariantCulture);
                            }
                            catch (Exception exc)
                            {
                                Logger.Error(exc);
                                pageId = -1;
                            }

                            if (pageId > -1)
                            {
                                tabModuleId = this.DoAddExistingModule(moduleLstId, pageId, dto.Pane, positionId, string.Empty, dto.CopyModule == "true");
                            }
                        }
                        else
                        {
                            tabModuleId = DoAddNewModule(string.Empty, moduleLstId, dto.Pane, positionId, permissionType, string.Empty);
                        }
                    }

                    return this.Request.CreateResponse(HttpStatusCode.OK, new { TabModuleID = tabModuleId });
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage ClearHostCache()
        {
            if (UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                DataCache.ClearCache();
                ClientResourceManager.ClearCache();
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }

            return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage RecycleApplicationPool()
        {
            if (UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                var log = new LogInfo { BypassBuffering = true, LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString() };
                log.AddProperty("Message", "UserRestart");
                LogController.Instance.AddLog(log);
                Config.Touch();
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }

            return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage SwitchSite(SwitchSiteDTO dto)
        {
            if (UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                try
                {
                    if (!string.IsNullOrEmpty(dto.Site))
                    {
                        int selectedPortalId = int.Parse(dto.Site, CultureInfo.InvariantCulture);
                        var portalAliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(selectedPortalId).ToList();

                        if (portalAliases.Count > 0 && (portalAliases[0] != null))
                        {
                            return this.Request.CreateResponse(HttpStatusCode.OK, new { RedirectURL = Globals.AddHTTP(((IPortalAliasInfo)portalAliases[0]).HttpAlias), });
                        }
                    }
                }
                catch (System.Threading.ThreadAbortException)
                {
                    // Do nothing we are not logging ThreadAbortExceptions caused by redirects
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SwitchLanguage(SwitchLanguageDTO dto)
        {
            try
            {
                if (this.PortalSettings.AllowUserUICulture && this.PortalSettings.ContentLocalizationEnabled)
                {
                    if (!string.IsNullOrEmpty(dto.Language))
                    {
                        var personalization = this.personalizationController.LoadProfile(this.UserInfo.UserID, this.PortalSettings.PortalId);
                        personalization.Profile["Usability:UICulture"] = dto.Language;
                        personalization.IsModified = true;
                        this.personalizationController.SaveProfile(personalization);
                        return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
                    }
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                // Do nothing we are not logging ThreadAbortxceptions caused by redirects
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnPageEditor]
        public HttpResponseMessage ToggleUserMode(UserModeDTO userMode)
        {
            if (userMode == null)
            {
                userMode = new UserModeDTO { UserMode = "VIEW" };
            }

            this.ToggleUserMode(userMode.UserMode);
            var response = this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });

            if (userMode.UserMode.Equals("VIEW", StringComparison.OrdinalIgnoreCase))
            {
                var cookie = this.Request.Headers.GetCookies("StayInEditMode").FirstOrDefault();
                if (cookie != null && !string.IsNullOrEmpty(cookie["StayInEditMode"].Value))
                {
                    var expireCookie = new CookieHeaderValue("StayInEditMode", string.Empty);
                    expireCookie.Expires = DateTimeOffset.Now.AddDays(-1);
                    expireCookie.Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/";
                    response.Headers.AddCookies(new List<CookieHeaderValue> { expireCookie });
                }
            }

            return response;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnPageEditor]
        public HttpResponseMessage SaveBookmark(BookmarkDTO bookmark)
        {
            if (string.IsNullOrEmpty(bookmark.Bookmark))
            {
                bookmark.Bookmark = string.Empty;
            }

            this.controller.SaveBookMark(this.PortalSettings.PortalId, this.UserInfo.UserID, bookmark.Title, bookmark.Bookmark);

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage LockInstance(LockingDTO lockingRequest)
        {
            HostController.Instance.Update("IsLocked", lockingRequest.Lock.ToString(), true);
            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage LockSite(LockingDTO lockingRequest)
        {
            PortalController.UpdatePortalSetting(this.PortalSettings.PortalId, "IsLocked", lockingRequest.Lock.ToString(), true);
            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public bool CanAddModuleToPage()
        {
            return true;

            // If we are not in an edit page
            // return (string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["mid"])) && (string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["ctl"]));
        }

        private static void SetCloneModuleContext(bool cloneModuleContext)
        {
            Thread.SetData(
                Thread.GetNamedDataSlot("CloneModuleContext"),
                cloneModuleContext ? bool.TrueString : bool.FalseString);
        }

        private static List<ModuleInfo> GetModules(int tabID)
        {
            var isRemote = TabController.Instance.GetTab(tabID, Null.NullInteger, false).PortalID != PortalSettings.Current.PortalId;
            var tabModules = ModuleController.Instance.GetTabModules(tabID);

            var pageModules = isRemote
                ? tabModules.Values.Where(m => ModuleSupportsSharing(m) && !m.IsDeleted).ToList()
                : tabModules.Values.Where(m => ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "MANAGE", m) && !m.IsDeleted).ToList();

            return pageModules;
        }

        private static bool ModuleSupportsSharing(ModuleInfo moduleInfo)
        {
            switch (moduleInfo.DesktopModule.Shareable)
            {
                case ModuleSharing.Supported:
                case ModuleSharing.Unknown:
                    return moduleInfo.IsShareable;
                default:
                    return false;
            }
        }

        private static string GetDeskTopModuleImage(int moduleId)
        {
            var portalDesktopModules = DesktopModuleController.GetDesktopModules(PortalSettings.Current.PortalId);
            var packages = PackageController.Instance.GetExtensionPackages(PortalSettings.Current.PortalId);

            string imageUrl =
                (from package in packages
                    join portMods in portalDesktopModules on package.PackageID equals portMods.Value.PackageID
                    where portMods.Value.DesktopModuleID == moduleId
                    select package.IconFile).FirstOrDefault();

            imageUrl = string.IsNullOrEmpty(imageUrl) ? Globals.ImagePath + DefaultExtensionImage : imageUrl;
            return System.Web.VirtualPathUtility.ToAbsolute(imageUrl);
        }

        private static string GetTabModuleImage(int tabId, int moduleId)
        {
            var tabModules = ModuleController.Instance.GetTabModules(tabId);
            var portalDesktopModules = DesktopModuleController.GetDesktopModules(PortalSettings.Current.PortalId);
            var moduleDefinitions = ModuleDefinitionController.GetModuleDefinitions();
            var packages = PackageController.Instance.GetExtensionPackages(PortalSettings.Current.PortalId);

            string imageUrl = (from package in packages
                join portMods in portalDesktopModules on package.PackageID equals portMods.Value.PackageID
                join modDefs in moduleDefinitions on portMods.Value.DesktopModuleID equals modDefs.Value.DesktopModuleID
                join tabMods in tabModules on modDefs.Value.DesktopModuleID equals tabMods.Value.DesktopModuleID
                where tabMods.Value.ModuleID == moduleId
                select package.IconFile).FirstOrDefault();

            imageUrl = string.IsNullOrEmpty(imageUrl) ? Globals.ImagePath + DefaultExtensionImage : imageUrl;
            return System.Web.VirtualPathUtility.ToAbsolute(imageUrl);
        }

        private static ModulePermissionInfo AddModulePermission(ModuleInfo objModule, PermissionInfo permission, int roleId, int userId, bool allowAccess)
        {
            var objModulePermission = new ModulePermissionInfo
            {
                ModuleID = objModule.ModuleID,
                PermissionID = permission.PermissionID,
                RoleID = roleId,
                UserID = userId,
                PermissionKey = permission.PermissionKey,
                AllowAccess = allowAccess,
            };

            // add the permission to the collection
            if (!objModule.ModulePermissions.Contains(objModulePermission))
            {
                objModule.ModulePermissions.Add(objModulePermission);
            }

            return objModulePermission;
        }

        private static int GetPaneModuleOrder(string pane, int sort)
        {
            var items = new List<int>();

            foreach (ModuleInfo m in PortalSettings.Current.ActiveTab.Modules)
            {
                // if user is allowed to view module and module is not deleted
                if (ModulePermissionController.CanViewModule(m) && !m.IsDeleted)
                {
                    // modules which are displayed on all tabs should not be displayed on the Admin or Super tabs
                    if (!m.AllTabs || !PortalSettings.Current.ActiveTab.IsSuperTab)
                    {
                        if (string.Equals(m.PaneName, pane, StringComparison.OrdinalIgnoreCase))
                        {
                            int moduleOrder = m.ModuleOrder;

                            while (items.Contains(moduleOrder) || moduleOrder == 0)
                            {
                                moduleOrder++;
                            }

                            items.Add(moduleOrder);
                        }
                    }
                }
            }

            items.Sort();

            if (items.Count > sort)
            {
                var itemOrder = items[sort];
                return itemOrder - 1;
            }
            else if (items.Count > 0)
            {
                return items.Last() + 1;
            }

            return 0;
        }

        private static int DoAddNewModule(string title, int desktopModuleId, string paneName, int position, int permissionType, string align)
        {
            try
            {
                if (!DesktopModuleController.GetDesktopModules(PortalSettings.Current.PortalId).TryGetValue(desktopModuleId, out _))
                {
                    throw new ArgumentException($"Could not find desktop module with given ID: {desktopModuleId}", nameof(desktopModuleId));
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            var tabModuleId = Null.NullInteger;
            foreach (ModuleDefinitionInfo objModuleDefinition in
                     ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModuleId).Values)
            {
                var objModule = new ModuleInfo();
                objModule.Initialize(PortalSettings.Current.ActiveTab.PortalID);

                objModule.PortalID = PortalSettings.Current.ActiveTab.PortalID;
                objModule.TabID = PortalSettings.Current.ActiveTab.TabID;
                objModule.ModuleOrder = position;
                objModule.ModuleTitle = string.IsNullOrEmpty(title) ? objModuleDefinition.FriendlyName : title;
                objModule.PaneName = paneName;
                objModule.ModuleDefID = objModuleDefinition.ModuleDefID;
                if (objModuleDefinition.DefaultCacheTime > 0)
                {
                    objModule.CacheTime = objModuleDefinition.DefaultCacheTime;
                    if (PortalSettings.Current.DefaultModuleId > Null.NullInteger && PortalSettings.Current.DefaultTabId > Null.NullInteger)
                    {
                        ModuleInfo defaultModule = ModuleController.Instance.GetModule(PortalSettings.Current.DefaultModuleId, PortalSettings.Current.DefaultTabId, true);
                        if (defaultModule != null)
                        {
                            objModule.CacheTime = defaultModule.CacheTime;
                        }
                    }
                }

                ModuleController.Instance.InitialModulePermission(objModule, objModule.TabID, permissionType);

                if (PortalSettings.Current.ContentLocalizationEnabled)
                {
                    Locale defaultLocale = LocaleController.Instance.GetDefaultLocale(PortalSettings.Current.PortalId);

                    // set the culture of the module to that of the tab
                    var tabInfo = TabController.Instance.GetTab(objModule.TabID, PortalSettings.Current.PortalId, false);
                    objModule.CultureCode = tabInfo != null ? tabInfo.CultureCode : defaultLocale.Code;
                }
                else
                {
                    objModule.CultureCode = Null.NullString;
                }

                objModule.AllTabs = false;
                objModule.Alignment = align;

                ModuleController.Instance.AddModule(objModule);

                if (tabModuleId == Null.NullInteger)
                {
                    tabModuleId = objModule.ModuleID;
                }

                // update the position to let later modules with add after previous one.
                position = ModuleController.Instance.GetTabModule(objModule.TabModuleID).ModuleOrder + 1;
            }

            return tabModuleId;
        }

        private void ToggleUserMode(string mode)
        {
            var personalization = this.personalizationController.LoadProfile(this.UserInfo.UserID, this.PortalSettings.PortalId);
            personalization.Profile["Usability:UserMode" + this.PortalSettings.PortalId] = mode.ToUpperInvariant();
            personalization.IsModified = true;
            this.personalizationController.SaveProfile(personalization);
        }

        private PortalSettings GetPortalSettings(string portal)
        {
            var portalSettings = PortalSettings.Current;

            try
            {
                if (!string.IsNullOrEmpty(portal))
                {
                    var selectedPortalId = int.Parse(portal, CultureInfo.InvariantCulture);
                    if (this.PortalSettings.PortalId != selectedPortalId)
                    {
                        portalSettings = new PortalSettings(selectedPortalId);
                    }
                }
            }
            catch (Exception)
            {
                portalSettings = PortalSettings.Current;
            }

            return portalSettings;
        }

        private bool ActiveTabHasChildren()
        {
            var children = TabController.GetTabsByParent(this.PortalSettings.ActiveTab.TabID, this.PortalSettings.ActiveTab.PortalID);

            if ((children == null) || children.Count < 1)
            {
                return false;
            }

            return true;
        }

        private int DoAddExistingModule(int moduleId, int tabId, string paneName, int position, string align, bool cloneModule)
        {
            ModuleInfo moduleInfo = ModuleController.Instance.GetModule(moduleId, tabId, false);

            int userID = -1;

            UserInfo user = UserController.Instance.GetCurrentUserInfo();
            if (user != null)
            {
                userID = user.UserID;
            }

            if (moduleInfo is { IsDeleted: false })
            {
                // Is this from a site other than our own? (i.e., is the user requesting "module sharing"?)
                var remote = moduleInfo.PortalID != PortalSettings.Current.PortalId;
                if (remote)
                {
                    switch (moduleInfo.DesktopModule.Shareable)
                    {
                        case ModuleSharing.Unsupported:
                            // Should never happen since the module should not be listed in the first place.
                            throw new SharingUnsupportedException($"Module '{moduleInfo.DesktopModule.FriendlyName}' does not support Shareable and should not be listed in Add Existing Module from a different source site");
                        case ModuleSharing.Supported:
                        case ModuleSharing.Unknown:
                            break;
                    }
                }

                if (!ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "MANAGE", moduleInfo))
                {
                    throw new SecurityException($"Module '{moduleInfo.ModuleID}' is not available in current context.");
                }

                // clone the module object ( to avoid creating an object reference to the data cache )
                ModuleInfo newModule = moduleInfo.Clone();

                newModule.UniqueId = Guid.NewGuid(); // Cloned Module requires a different uniqueID
                newModule.TabModuleID = Null.NullInteger;
                newModule.PortalID = PortalSettings.Current.PortalId;
                newModule.TabID = PortalSettings.Current.ActiveTab.TabID;
                newModule.ModuleOrder = position;
                newModule.PaneName = paneName;
                newModule.Alignment = align;

                if (cloneModule)
                {
                    newModule.ModuleID = Null.NullInteger;

                    // copy module settings and tab module settings
                    newModule.ModuleSettings.Clear();
                    foreach (var key in moduleInfo.ModuleSettings.Keys)
                    {
                        newModule.ModuleSettings.Add(key, moduleInfo.ModuleSettings[key]);
                    }

                    newModule.TabModuleSettings.Clear();
                    foreach (var key in moduleInfo.TabModuleSettings.Keys)
                    {
                        newModule.TabModuleSettings.Add(key, moduleInfo.TabModuleSettings[key]);
                    }

                    // reset the module id
                    newModule.ModuleID = ModuleController.Instance.AddModule(newModule);

                    if (!string.IsNullOrEmpty(newModule.DesktopModule.BusinessControllerClass))
                    {
                        var portable = this.businessControllerProvider.GetInstance<IPortable>(newModule);
                        if (portable is not null)
                        {
                            try
                            {
                                SetCloneModuleContext(true);
                                var content = portable.ExportModule(moduleId);
                                if (!string.IsNullOrEmpty(content))
                                {
                                    portable.ImportModule(
                                        newModule.ModuleID,
                                        content,
                                        newModule.DesktopModule.Version,
                                        userID);
                                }
                            }
                            finally
                            {
                                SetCloneModuleContext(false);
                            }
                        }
                    }
                }
                else
                {
                    // copy tab module settings
                    newModule.TabModuleSettings.Clear();
                    foreach (var key in moduleInfo.TabModuleSettings.Keys)
                    {
                        newModule.TabModuleSettings.Add(key, moduleInfo.TabModuleSettings[key]);
                    }

                    ModuleController.Instance.AddModule(newModule);
                }

                // if the tab of original module has custom stylesheet defined, then also copy the stylesheet
                // to the destination tab if its custom stylesheet is empty.
                var originalTab = TabController.Instance.GetTab(moduleInfo.TabID, moduleInfo.PortalID);
                var targetTab = PortalSettings.Current.ActiveTab;
                if (originalTab != null
                    && originalTab.TabSettings.ContainsKey("CustomStylesheet")
                    && !string.IsNullOrEmpty(originalTab.TabSettings["CustomStylesheet"].ToString())
                    && (!targetTab.TabSettings.ContainsKey("CustomStylesheet") ||
                        string.IsNullOrEmpty(targetTab.TabSettings["CustomStylesheet"].ToString())))
                {
                    TabController.Instance.UpdateTabSetting(targetTab.TabID, "CustomStylesheet", originalTab.TabSettings["CustomStylesheet"].ToString());
                }

                if (remote)
                {
                    // Ensure the Portal Admin has View rights
                    var permissionController = new PermissionController();
                    ArrayList arrSystemModuleViewPermissions = permissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", "VIEW");
                    AddModulePermission(
                        newModule,
                        (PermissionInfo)arrSystemModuleViewPermissions[0],
                        PortalSettings.Current.AdministratorRoleId,
                        Null.NullInteger,
                        true);

                    // Set PortalID correctly
                    newModule.OwnerPortalID = newModule.PortalID;
                    newModule.PortalID = PortalSettings.Current.PortalId;
                    ModulePermissionController.SaveModulePermissions(newModule);
                }

                // Add Event Log
                EventLogController.Instance.AddLog(newModule, PortalSettings.Current, userID, string.Empty, EventLogController.EventLogType.MODULE_CREATED);

                return newModule.ModuleID;
            }

            return -1;
        }

        private string GetModuleName(string moduleName)
        {
            if (this.nameDics == null)
            {
                this.nameDics = new Dictionary<string, string>
                {
                    { "SearchCrawlerAdmin", "SearchCrawler Admin" },
                    { "SearchCrawlerInput", "SearchCrawler Input" },
                    { "SearchCrawlerResults", "SearchCrawler Results" },
                };
            }

            return this.nameDics.TryGetValue(moduleName, out var name) ? name : moduleName;
        }

        public class ModuleDefDTO
        {
            public int ModuleID { get; set; }

            public string ModuleName { get; set; }

            public string ModuleImage { get; set; }

            public bool Bookmarked { get; set; }

            public bool ExistsInBookmarkCategory { get; set; }
        }

        public class PageDefDTO
        {
            public int TabID { get; set; }

            public string IndentedTabName { get; set; }
        }

        public class AddModuleDTO
        {
            public string Visibility { get; set; }

            public string Position { get; set; }

            public string Module { get; set; }

            public string Page { get; set; }

            public string Pane { get; set; }

            public string AddExistingModule { get; set; }

            public string CopyModule { get; set; }

            public string Sort { get; set; }
        }

        public class UserModeDTO
        {
            public string UserMode { get; set; }
        }

        public class SwitchSiteDTO
        {
            public string Site { get; set; }
        }

        public class SwitchLanguageDTO
        {
            public string Language { get; set; }
        }

        public class BookmarkDTO
        {
            public string Title { get; set; }

            public string Bookmark { get; set; }
        }

        public class LockingDTO
        {
            public bool Lock { get; set; }
        }
    }
}
