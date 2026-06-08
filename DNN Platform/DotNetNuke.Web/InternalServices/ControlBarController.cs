// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.InternalServices;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Logging;
using DotNetNuke.Abstractions.Modules;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Abstractions.Security.Permissions;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
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

using Microsoft.Extensions.DependencyInjection;

/// <summary>A web API for the control bar.</summary>
/// <param name="businessControllerProvider">The business controller provider.</param>
/// <param name="personalizationController">The personalization controller.</param>
/// <param name="appStatus">The application status.</param>
/// <param name="portalAliasService">The portal alias service.</param>
/// <param name="hostSettingsService">The host settings service.</param>
/// <param name="portalController">The portal controller.</param>
/// <param name="permissionDefinitionService">The permission definition service.</param>
/// <param name="eventLogger">The event logger.</param>
/// <param name="hostSettings">The host settings.</param>
[DnnAuthorize]
public class ControlBarController(IBusinessControllerProvider businessControllerProvider, PersonalizationController personalizationController, IApplicationStatusInfo appStatus, IPortalAliasService portalAliasService, IHostSettingsService hostSettingsService, IPortalController portalController, IPermissionDefinitionService permissionDefinitionService, IEventLogger eventLogger, IHostSettings hostSettings)
    : DnnApiController
{
    private const string DefaultExtensionImage = "icon_extensions_32px.png";
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ControlBarController));
    private readonly IBusinessControllerProvider businessControllerProvider = businessControllerProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<IBusinessControllerProvider>();
    private readonly PersonalizationController personalizationController = personalizationController ?? Globals.GetCurrentServiceProvider().GetRequiredService<PersonalizationController>();
    private readonly IApplicationStatusInfo appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
    private readonly IPortalAliasService portalAliasService = portalAliasService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>();
    private readonly IHostSettingsService hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
    private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
    private readonly IPermissionDefinitionService permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
    private readonly IEventLogger eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
    private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
    private readonly Components.Controllers.IControlBarController controller = Components.Controllers.ControlBarController.Instance;

    /// <summary>Initializes a new instance of the <see cref="ControlBarController"/> class.</summary>
    /// <param name="businessControllerProvider">The business controller provider.</param>
    [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
    public ControlBarController(IBusinessControllerProvider businessControllerProvider)
        : this(businessControllerProvider, null, null, null, null, null, null, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ControlBarController"/> class.</summary>
    /// <param name="businessControllerProvider">The business controller provider.</param>
    /// <param name="personalizationController">The personalization controller.</param>
    [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
    public ControlBarController(IBusinessControllerProvider businessControllerProvider, PersonalizationController personalizationController)
        : this(businessControllerProvider, personalizationController, null, null, null, null, null, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ControlBarController"/> class.</summary>
    /// <param name="businessControllerProvider">The business controller provider.</param>
    /// <param name="personalizationController">The personalization controller.</param>
    /// <param name="appStatus">The application status.</param>
    /// <param name="portalAliasService">The portal alias service.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="permissionDefinitionService">The permission definition service.</param>
    /// <param name="eventLogger">The event logger.</param>
    [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
    public ControlBarController(IBusinessControllerProvider businessControllerProvider, PersonalizationController personalizationController, IApplicationStatusInfo appStatus, IPortalAliasService portalAliasService, IHostSettingsService hostSettingsService, IPortalController portalController, IPermissionDefinitionService permissionDefinitionService, IEventLogger eventLogger)
        : this(businessControllerProvider, personalizationController, appStatus, portalAliasService, hostSettingsService, portalController, permissionDefinitionService, eventLogger, null)
    {
    }

    /// <summary>Gets the desktop modules available to the portal.</summary>
    /// <param name="category">The module category (<c>"All"</c> if <see langword="null"/> or <see cref="string.Empty"/>).</param>
    /// <param name="loadingStartIndex">The index.</param>
    /// <param name="loadingPageSize">The page size.</param>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="excludeCategories">A comma-delimited list of categories to exclude.</param>
    /// <param name="sortBookmarks">Whether to sort bookmarked modules to the top.</param>
    /// <param name="topModule">The friendly name of a module to display first in the list (only if <paramref name="sortBookmarks"/> is <see langword="true"/>).</param>
    /// <returns>A response containing a list of <see cref="ModuleDefDTO"/> objects.</returns>
    [HttpGet]
    [DnnPageEditor]
    public HttpResponseMessage GetPortalDesktopModules(string category, int loadingStartIndex, int loadingPageSize, string searchTerm, string excludeCategories = "", bool sortBookmarks = false, string topModule = "")
    {
        if (string.IsNullOrEmpty(category))
        {
            category = "All";
        }

        var bookmarkCategory = this.controller.GetBookmarkCategory(PortalSettings.Current.PortalId);
        var bookmarkedModules = this.controller.GetBookmarkedDesktopModules(PortalSettings.Current.PortalId, UserController.Instance.GetCurrentUserInfo().UserID, searchTerm);
        var bookmarkCategoryModules = this.controller.GetCategoryDesktopModules(this.PortalSettings.PortalId, bookmarkCategory, searchTerm);

        var filteredList = bookmarkCategory == category
            ? bookmarkCategoryModules.OrderBy(m => m.Key).Union(bookmarkedModules.OrderBy(m => m.Key)).Distinct()
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
            ModuleImage = GetDeskTopModuleImage(this.hostSettings, kvp.Value.DesktopModuleID),
            Bookmarked = bookmarkedModules.Any(m => m.Key == kvp.Key),
            ExistsInBookmarkCategory = bookmarkCategoryModules.Any(m => m.Key == kvp.Key),
        }).ToList();
        return this.Request.CreateResponse(HttpStatusCode.OK, result);
    }

    /// <summary>Gets the pages for a portal.</summary>
    /// <param name="portal">The portal ID, or <see langword="null"/> or <see cref="string.Empty"/> for the current portal.</param>
    /// <returns>A response with a list of <see cref="PageDefDTO"/> objects.</returns>
    [HttpGet]
    [DnnPageEditor]
    public HttpResponseMessage GetPageList(string portal)
    {
        var portalSettings = this.GetPortalSettings(portal);

        List<TabInfo> tabList;
        if (this.PortalSettings.PortalId == portalSettings.PortalId)
        {
            tabList = TabController.GetPortalTabs(this.hostSettings, this.appStatus, portalSettings.PortalId, this.PortalSettings.ActiveTab.TabID, false, string.Empty, true, false, false, false, true);
        }
        else
        {
            var groups = PortalGroupController.Instance.GetPortalGroups().ToArray();

            var myGroup = (
                    from @group in groups
                    select PortalGroupController.Instance.GetPortalsByGroup(@group.PortalGroupId).Cast<IPortalInfo>() into portals
                    where portals.Any(x => x.PortalId == PortalSettings.Current.PortalId)
                    select portals.ToArray())
                .FirstOrDefault();

            if (myGroup != null && myGroup.Any(p => p.PortalId == portalSettings.PortalId))
            {
                tabList = TabController.GetPortalTabs(this.hostSettings, this.appStatus, portalSettings.PortalId, Null.NullInteger, false, string.Empty, true, false, false, false, false);
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

    /// <summary>Gets the modules to a page.</summary>
    /// <param name="tab">The tab ID.</param>
    /// <returns>A response with a list of <see cref="ModuleDefDTO"/> objects.</returns>
    [HttpGet]
    [DnnPageEditor]
    public HttpResponseMessage GetTabModules(string tab)
    {
        if (int.TryParse(tab, out var tabId))
        {
            var result = new List<ModuleDefDTO>();
            if (tabId > 0)
            {
                var pageModules = GetModules(tabId);

                Dictionary<int, string> resultDict = pageModules.ToDictionary(module => module.ModuleID, module => module.ModuleTitle);
                result.AddRange(from kvp in resultDict
                    let imageUrl = GetTabModuleImage(this.hostSettings, tabId, kvp.Key)
                    select new ModuleDefDTO { ModuleID = kvp.Key, ModuleName = kvp.Value, ModuleImage = imageUrl });
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, result);
        }

        return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
    }

    /// <summary>Copy permissions from the active page to its descendants.</summary>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DnnPageEditor]
    public HttpResponseMessage CopyPermissionsToChildren()
    {
        if (TabPermissionController.CanManagePage() && UserController.Instance.GetCurrentUserInfo().IsInRole("Administrators")
                                                    && this.ActiveTabHasChildren() && !this.PortalSettings.ActiveTab.IsSuperTab)
        {
            TabController.CopyPermissionsToChildren(this.eventLogger, this.PortalSettings.ActiveTab, this.PortalSettings.ActiveTab.TabPermissions);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
        }

        return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
    }

    /// <summary>Add a module to a page.</summary>
    /// <param name="dto">Information about the module to add.</param>
    /// <returns>A response with an object containing the tab-module ID of the new instance.</returns>
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
                        tabModuleId = DoAddNewModule(this.hostSettings, string.Empty, moduleLstId, dto.Pane, positionId, permissionType, string.Empty);
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

    /// <summary>Clears the host cache.</summary>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequireHost]
    public HttpResponseMessage ClearHostCache()
    {
        if (UserController.Instance.GetCurrentUserInfo().IsSuperUser)
        {
            DataCache.ClearCache();
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
        }

        return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
    }

    /// <summary>Recycles the application pool.</summary>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequireHost]
    public HttpResponseMessage RecycleApplicationPool()
    {
        if (UserController.Instance.GetCurrentUserInfo().IsSuperUser)
        {
            var log = new LogInfo { BypassBuffering = true, LogTypeKey = nameof(EventLogType.HOST_ALERT) };
            log.AddProperty("Message", "UserRestart");
            LogController.Instance.AddLog(log);
            Config.Touch(this.appStatus);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
        }

        return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
    }

    /// <summary>Switches to a different portal/site.</summary>
    /// <param name="dto">Information about the site to switch to.</param>
    /// <returns>A response with an object containing a <c>RedirectURL</c> field.</returns>
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
                    var portalAliases = this.portalAliasService.GetPortalAliasesByPortalId(selectedPortalId).ToList();

                    if (portalAliases.Count > 0 && portalAliases[0] != null)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.OK, new { RedirectURL = Globals.AddHTTP(portalAliases[0].HttpAlias), });
                    }
                }
            }
            catch (ThreadAbortException)
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

    /// <summary>Updates the user's preferred language.</summary>
    /// <param name="dto">Information about the language switch.</param>
    /// <returns>A response indicating success.</returns>
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
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
                }
            }
        }
        catch (ThreadAbortException)
        {
            // Do nothing we are not logging ThreadAbortExceptions caused by redirects
        }
        catch (Exception ex)
        {
            Exceptions.LogException(ex);
        }

        return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
    }

    /// <summary>Toggle between view and edit mode.</summary>
    /// <param name="userMode">The user mode to switch to.</param>
    /// <returns>A response indicating success.</returns>
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
        var response = this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });

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

    /// <summary>Saves a bookmark for a user.</summary>
    /// <param name="bookmark">The bookmark to save.</param>
    /// <returns>A response indicating success.</returns>
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

        return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
    }

    /// <summary>Locks or unlocks the instance.</summary>
    /// <param name="lockingRequest">The lock request.</param>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequireHost]
    public HttpResponseMessage LockInstance(LockingDTO lockingRequest)
    {
        this.hostSettingsService.Update("IsLocked", lockingRequest.Lock.ToString(), true);
        return this.Request.CreateResponse(HttpStatusCode.OK);
    }

    /// <summary>Locks or unlocks the current site.</summary>
    /// <param name="lockingRequest">The lock request.</param>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequireHost]
    public HttpResponseMessage LockSite(LockingDTO lockingRequest)
    {
        PortalController.UpdatePortalSetting(this.portalController, this.PortalSettings.PortalId, "IsLocked", lockingRequest.Lock.ToString(), true);
        return this.Request.CreateResponse(HttpStatusCode.OK);
    }

    /// <summary>Gets a value indicating whether the current user can add a module to the current page.</summary>
    /// <returns><see langword="true"/>.</returns>
    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
    public bool CanAddModuleToPage()
    {
        return true;

        // If we are not in an edit page
        ////return (string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["mid"])) && (string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["ctl"]));
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

    private static string GetDeskTopModuleImage(IHostSettings hostSettings, int moduleId)
    {
        var portalDesktopModules = DesktopModuleController.GetDesktopModules(hostSettings, PortalSettings.Current.PortalId);
        var packages = PackageController.Instance.GetExtensionPackages(PortalSettings.Current.PortalId);

        string imageUrl =
            (from package in packages
                join portMods in portalDesktopModules on package.PackageID equals portMods.Value.PackageID
                where portMods.Value.DesktopModuleID == moduleId
                select package.IconFile).FirstOrDefault();

        imageUrl = string.IsNullOrEmpty(imageUrl) ? Globals.ImagePath + DefaultExtensionImage : imageUrl;
        return System.Web.VirtualPathUtility.ToAbsolute(imageUrl);
    }

    private static string GetTabModuleImage(IHostSettings hostSettings, int tabId, int moduleId)
    {
        var tabModules = ModuleController.Instance.GetTabModules(tabId);
        var portalDesktopModules = DesktopModuleController.GetDesktopModules(hostSettings, PortalSettings.Current.PortalId);
        var moduleDefinitions = ModuleDefinitionController.GetModuleDefinitions(hostSettings);
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

    private static void AddModulePermission(ModuleInfo objModule, IPermissionDefinitionInfo permission, int roleId, int userId, bool allowAccess)
    {
        IPermissionInfo objModulePermission = new ModulePermissionInfo
        {
            ModuleID = objModule.ModuleID,
            PermissionKey = permission.PermissionKey,
            AllowAccess = allowAccess,
        };
        objModulePermission.PermissionId = permission.PermissionId;
        objModulePermission.RoleId = roleId;
        objModulePermission.UserId = userId;

        // add the permission to the collection
        if (!objModule.ModulePermissions.Contains(objModulePermission))
        {
            objModule.ModulePermissions.Add((ModulePermissionInfo)objModulePermission);
        }
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

    private static int DoAddNewModule(IHostSettings hostSettings, string title, int desktopModuleId, string paneName, int position, int permissionType, string align)
    {
        try
        {
            if (!DesktopModuleController.GetDesktopModules(hostSettings, PortalSettings.Current.PortalId).TryGetValue(desktopModuleId, out _))
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
                var arrSystemModuleViewPermissions = this.permissionDefinitionService.GetDefinitionsByCodeAndKey("SYSTEM_MODULE_DEFINITION", "VIEW");
                AddModulePermission(
                    newModule,
                    arrSystemModuleViewPermissions.First(),
                    PortalSettings.Current.AdministratorRoleId,
                    Null.NullInteger,
                    true);

                // Set PortalID correctly
                newModule.OwnerPortalID = newModule.PortalID;
                newModule.PortalID = PortalSettings.Current.PortalId;
                ModulePermissionController.SaveModulePermissions(newModule);
            }

            // Add Event Log
            this.eventLogger.AddLog(newModule, PortalSettings.Current, userID, string.Empty, EventLogType.MODULE_CREATED);

            return newModule.ModuleID;
        }

        return -1;
    }

    /// <summary>A data transfer object with information about a module definition.</summary>
    public class ModuleDefDTO
    {
        /// <summary>Gets or sets the module ID.</summary>
        public int ModuleID { get; set; }

        /// <summary>Gets or sets the module name.</summary>
        public string ModuleName { get; set; }

        /// <summary>Gets or sets the path to the module's image.</summary>
        public string ModuleImage { get; set; }

        /// <summary>Gets or sets a value indicating whether the module is bookmarked.</summary>
        public bool Bookmarked { get; set; }

        /// <summary>Gets or sets a value indicating whether the module is in a bookmarked category.</summary>
        public bool ExistsInBookmarkCategory { get; set; }
    }

    /// <summary>A data transfer object with information about a page.</summary>
    public class PageDefDTO
    {
        /// <summary>Gets or sets the page's ID.</summary>
        public int TabID { get; set; }

        /// <summary>Gets or sets the page's indented name.</summary>
        public string IndentedTabName { get; set; }
    }

    /// <summary>A data transfer object with information about adding a module to a page.</summary>
    public class AddModuleDTO
    {
        /// <summary>Gets or sets the visibility of the module.</summary>
        public string Visibility { get; set; }

        /// <summary>Gets or sets the position of the module.</summary>
        public string Position { get; set; }

        /// <summary>Gets or sets the ID of an existing module.</summary>
        public string Module { get; set; }

        /// <summary>Gets or sets the ID of the page.</summary>
        public string Page { get; set; }

        /// <summary>Gets or sets the pane name.</summary>
        public string Pane { get; set; }

        /// <summary>Gets or sets a value indicating whether to add an existing module instead of a new module.</summary>
        public string AddExistingModule { get; set; }

        /// <summary>Gets or sets a value indicating whether to copy an existing module instead of making a shared reference.</summary>
        public string CopyModule { get; set; }

        /// <summary>Gets or sets the sort of the module.</summary>
        public string Sort { get; set; }
    }

    /// <summary>A data transfer object with information about the user mode.</summary>
    public class UserModeDTO
    {
        /// <summary>Gets or sets the user mode.</summary>
        public string UserMode { get; set; }
    }

    /// <summary>A data transfer object with information about the site to switch to.</summary>
    public class SwitchSiteDTO
    {
        /// <summary>Gets or sets the portal ID.</summary>
        public string Site { get; set; }
    }

    /// <summary>A data transfer object with information about the language to switch to.</summary>
    public class SwitchLanguageDTO
    {
        /// <summary>Gets or sets the language code.</summary>
        public string Language { get; set; }
    }

    /// <summary>A data transfer object with information about a bookmark to add.</summary>
    public class BookmarkDTO
    {
        /// <summary>Gets or sets the bookmark title.</summary>
        public string Title { get; set; }

        /// <summary>Gets or sets the bookmark value.</summary>
        public string Bookmark { get; set; }
    }

    /// <summary>A data transfer object with information about a lock/unlock request.</summary>
    public class LockingDTO
    {
        /// <summary>Gets or sets a value indicating whether to lock or unlock the site or instance.</summary>
        public bool Lock { get; set; }
    }
}
