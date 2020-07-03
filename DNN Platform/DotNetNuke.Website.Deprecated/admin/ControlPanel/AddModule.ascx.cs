// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.ControlPanel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Web;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.UI;
    using DotNetNuke.Web.UI.WebControls;
    using DotNetNuke.Web.UI.WebControls.Extensions;
    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;
    using PortalInfo = DotNetNuke.Entities.Portals.PortalInfo;
    using Reflection = DotNetNuke.Framework.Reflection;

    public partial class AddModule : UserControlBase, IDnnRibbonBarTool
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AddModule));
        private readonly INavigationManager _navigationManager;
        private bool _enabled = true;

        public AddModule()
        {
            this._navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public override bool Visible
        {
            get
            {
                return base.Visible && TabPermissionController.CanAddContentToPage();
            }

            set
            {
                base.Visible = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return this._enabled && this.CanAddModuleToPage();
            }

            set
            {
                this._enabled = value;
            }
        }

        public string ToolName
        {
            get
            {
                return "QuickAddModule";
            }

            set
            {
                throw new NotSupportedException("Set ToolName not supported");
            }
        }

        /// <summary>Gets the currently-selected module.</summary>
        protected DesktopModuleInfo SelectedModule
        {
            get
            {
                if (this.AddExistingModule.Checked)
                {
                    var tabId = -1;
                    if (!string.IsNullOrEmpty(this.PageLst.SelectedValue))
                    {
                        tabId = int.Parse(this.PageLst.SelectedValue);
                    }

                    if (tabId < 0)
                    {
                        tabId = PortalSettings.Current.ActiveTab.TabID;
                    }

                    if (!string.IsNullOrEmpty(this.ModuleLst.SelectedValue))
                    {
                        var moduleId = int.Parse(this.ModuleLst.SelectedValue);
                        if (moduleId >= 0)
                        {
                            return ModuleController.Instance.GetModule(moduleId, tabId, false).DesktopModule;
                        }
                    }
                }
                else
                {
                    var portalId = -1;

                    if (this.SiteListPanel.Visible)
                    {
                        portalId = int.Parse(this.SiteList.SelectedValue);
                    }

                    if (portalId < 0)
                    {
                        portalId = PortalSettings.Current.PortalId;
                    }

                    if (!string.IsNullOrEmpty(this.ModuleLst.SelectedValue))
                    {
                        var moduleId = int.Parse(this.ModuleLst.SelectedValue);
                        if (moduleId >= 0)
                        {
                            return DesktopModuleController.GetDesktopModule(moduleId, portalId);
                        }
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets return the <see cref="PortalSettings"/> for the selected portal (from the Site list), unless
        /// the site list is not visible or there are no other sites in our site group, in which case
        /// it returns the PortalSettings for the current portal.
        /// </summary>
        private PortalSettings SelectedPortalSettings
        {
            get
            {
                var portalSettings = PortalSettings.Current;

                try
                {
                    if (this.SiteListPanel.Visible && this.SiteList.SelectedItem != null)
                    {
                        if (!string.IsNullOrEmpty(this.SiteList.SelectedItem.Value))
                        {
                            var selectedPortalId = int.Parse(this.SiteList.SelectedItem.Value);
                            if (this.PortalSettings.PortalId != selectedPortalId)
                            {
                                portalSettings = new PortalSettings(int.Parse(this.SiteList.SelectedItem.Value));
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    portalSettings = PortalSettings.Current;
                }

                return portalSettings;
            }
        }

        private string LocalResourceFile
        {
            get
            {
                return string.Format("{0}/{1}/{2}.ascx.resx", this.TemplateSourceDirectory, Localization.LocalResourceDirectory, this.GetType().BaseType.Name);
            }
        }

        public bool CanAddModuleToPage()
        {
            if (HttpContext.Current == null)
            {
                return false;
            }

            // If we are not in an edit page
            return string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["mid"]) && string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["ctl"]);
        }

        protected void AddNewOrExisting_OnClick(object sender, EventArgs e)
        {
            this.LoadAllLists();
        }

        protected void PaneLstSelectedIndexChanged(object sender, EventArgs e)
        {
            this.LoadPositionList();
            this.LoadPaneModulesList();
        }

        protected void PageLstSelectedIndexChanged(object sender, EventArgs e)
        {
            this.LoadModuleList();
        }

        protected void PositionLstSelectedIndexChanged(object sender, EventArgs e)
        {
            this.PaneModulesLst.Enabled = this.PositionLst.SelectedValue == "ABOVE" || this.PositionLst.SelectedValue == "BELOW";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Is there more than one site in this group?
            var multipleSites = this.GetCurrentPortalsGroup().Count() > 1;
            ClientAPI.RegisterClientVariable(this.Page, "moduleSharing", multipleSites.ToString().ToLowerInvariant(), true);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            this.cmdAddModule.Click += this.CmdAddModuleClick;
            this.AddNewModule.CheckedChanged += this.AddNewOrExisting_OnClick;
            this.AddExistingModule.CheckedChanged += this.AddNewOrExisting_OnClick;
            this.SiteList.SelectedIndexChanged += this.SiteList_SelectedIndexChanged;
            this.CategoryList.SelectedIndexChanged += this.CategoryListSelectedIndexChanged;
            this.PageLst.SelectedIndexChanged += this.PageLstSelectedIndexChanged;
            this.PaneLst.SelectedIndexChanged += this.PaneLstSelectedIndexChanged;
            this.PositionLst.SelectedIndexChanged += this.PositionLstSelectedIndexChanged;

            try
            {
                if (this.Visible)
                {
                    this.cmdAddModule.Enabled = this.Enabled;
                    this.AddExistingModule.Enabled = this.Enabled;
                    this.AddNewModule.Enabled = this.Enabled;
                    this.Title.Enabled = this.Enabled;
                    this.PageLst.Enabled = this.Enabled;
                    this.ModuleLst.Enabled = this.Enabled;
                    this.VisibilityLst.Enabled = this.Enabled;
                    this.PaneLst.Enabled = this.Enabled;
                    this.PositionLst.Enabled = this.Enabled;
                    this.PaneModulesLst.Enabled = this.Enabled;

                    UserInfo objUser = UserController.Instance.GetCurrentUserInfo();
                    if (objUser != null)
                    {
                        if (objUser.IsSuperUser)
                        {
                            var objModule = ModuleController.Instance.GetModuleByDefinition(-1, "Extensions");
                            if (objModule != null)
                            {
                                var strURL = this._navigationManager.NavigateURL(objModule.TabID, true);
                                this.hlMoreExtensions.NavigateUrl = strURL + "#moreExtensions";
                            }
                            else
                            {
                                this.hlMoreExtensions.Enabled = false;
                            }

                            this.hlMoreExtensions.Text = this.GetString("hlMoreExtensions");
                            this.hlMoreExtensions.Visible = true;
                        }
                    }
                }

                if (!this.IsPostBack && this.Visible && this.Enabled)
                {
                    this.AddNewModule.Checked = true;
                    this.LoadAllLists();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void CmdAddModuleClick(object sender, EventArgs e)
        {
            if (TabPermissionController.CanAddContentToPage() && this.CanAddModuleToPage())
            {
                int permissionType;
                try
                {
                    permissionType = int.Parse(this.VisibilityLst.SelectedValue);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    permissionType = 0;
                }

                int position = -1;
                switch (this.PositionLst.SelectedValue)
                {
                    case "TOP":
                        position = 0;
                        break;
                    case "ABOVE":
                        if (!string.IsNullOrEmpty(this.PaneModulesLst.SelectedValue))
                        {
                            try
                            {
                                position = int.Parse(this.PaneModulesLst.SelectedValue) - 1;
                            }
                            catch (Exception exc)
                            {
                                Logger.Error(exc);

                                position = -1;
                            }
                        }
                        else
                        {
                            position = 0;
                        }

                        break;
                    case "BELOW":
                        if (!string.IsNullOrEmpty(this.PaneModulesLst.SelectedValue))
                        {
                            try
                            {
                                position = int.Parse(this.PaneModulesLst.SelectedValue) + 1;
                            }
                            catch (Exception exc)
                            {
                                Logger.Error(exc);

                                position = -1;
                            }
                        }
                        else
                        {
                            position = -1;
                        }

                        break;
                    case "BOTTOM":
                        position = -1;
                        break;
                }

                int moduleLstID;
                try
                {
                    moduleLstID = int.Parse(this.ModuleLst.SelectedValue);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    moduleLstID = -1;
                }

                if (moduleLstID > -1)
                {
                    if (this.AddExistingModule.Checked)
                    {
                        int pageID;
                        try
                        {
                            pageID = int.Parse(this.PageLst.SelectedValue);
                        }
                        catch (Exception exc)
                        {
                            Logger.Error(exc);

                            pageID = -1;
                        }

                        if (pageID > -1)
                        {
                            this.DoAddExistingModule(moduleLstID, pageID, this.PaneLst.SelectedValue, position, string.Empty, this.chkCopyModule.Checked);
                        }
                    }
                    else
                    {
                        DoAddNewModule(this.Title.Text, moduleLstID, this.PaneLst.SelectedValue, position, permissionType, string.Empty);
                    }
                }

                // set view mode to edit after add module.
                if (this.PortalSettings.UserMode != PortalSettings.Mode.Edit)
                {
                    Personalization.SetProfile("Usability", "UserMode" + this.PortalSettings.PortalId, "EDIT");
                }

                this.Response.Redirect(this.Request.RawUrl, true);
            }
        }

        protected string GetString(string key)
        {
            return Localization.GetString(key, this.LocalResourceFile);
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

        private static void SetCloneModuleContext(bool cloneModuleContext)
        {
            Thread.SetData(
                Thread.GetNamedDataSlot("CloneModuleContext"),
                cloneModuleContext ? bool.TrueString : bool.FalseString);
        }

        private static void DoAddNewModule(string title, int desktopModuleId, string paneName, int position, int permissionType, string align)
        {
            try
            {
                DesktopModuleInfo desktopModule;
                if (!DesktopModuleController.GetDesktopModules(PortalSettings.Current.PortalId).TryGetValue(desktopModuleId, out desktopModule))
                {
                    throw new ArgumentException("desktopModuleId");
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

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

                    // check whether original tab is exists, if true then set culture code to default language,
                    // otherwise set culture code to current.
                    if (TabController.Instance.GetTabByCulture(objModule.TabID, PortalSettings.Current.PortalId, defaultLocale) != null)
                    {
                        objModule.CultureCode = defaultLocale.Code;
                    }
                    else
                    {
                        objModule.CultureCode = PortalSettings.Current.CultureCode;
                    }
                }
                else
                {
                    objModule.CultureCode = Null.NullString;
                }

                objModule.AllTabs = false;
                objModule.Alignment = align;

                ModuleController.Instance.AddModule(objModule);
            }
        }

        private static bool GetIsPortable(string moduleID, string tabID)
        {
            bool isPortable = false;
            int parsedModuleID;
            int parsedTabID;

            bool validModuleID = int.TryParse(moduleID, out parsedModuleID);
            bool validTabID = int.TryParse(tabID, out parsedTabID);

            if (validModuleID && validTabID)
            {
                ModuleInfo moduleInfo = ModuleController.Instance.GetModule(parsedModuleID, parsedTabID, false);
                if (moduleInfo != null)
                {
                    DesktopModuleInfo moduleDesktopInfo = moduleInfo.DesktopModule;
                    if (moduleDesktopInfo != null)
                    {
                        isPortable = moduleDesktopInfo.IsPortable;
                    }
                }
            }

            return isPortable;
        }

        private void CmdConfirmAddModuleClick(object sender, EventArgs e)
        {
            this.CmdAddModuleClick(sender, e);
        }

        private void SiteList_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.LoadModuleList();
            this.LoadPageList();
        }

        private void CategoryListSelectedIndexChanged(object sender, EventArgs e)
        {
            this.LoadModuleList();
        }

        private void DoAddExistingModule(int moduleId, int tabId, string paneName, int position, string align, bool cloneModule)
        {
            ModuleInfo moduleInfo = ModuleController.Instance.GetModule(moduleId, tabId, false);

            int userID = -1;
            if (this.Request.IsAuthenticated)
            {
                UserInfo user = UserController.Instance.GetCurrentUserInfo();
                if (user != null)
                {
                    userID = user.UserID;
                }
            }

            if (moduleInfo != null)
            {
                // Is this from a site other than our own? (i.e., is the user requesting "module sharing"?)
                var remote = moduleInfo.PortalID != PortalSettings.Current.PortalId;
                if (remote)
                {
                    switch (moduleInfo.DesktopModule.Shareable)
                    {
                        case ModuleSharing.Unsupported:
                            // Should never happen since the module should not be listed in the first place.
                            throw new ApplicationException(string.Format(
                                "Module '{0}' does not support Shareable and should not be listed in Add Existing Module from a different source site",
                                moduleInfo.DesktopModule.FriendlyName));
                        case ModuleSharing.Supported:
                            break;
                        default:
                        case ModuleSharing.Unknown:
                            break;
                    }
                }

                // clone the module object ( to avoid creating an object reference to the data cache )
                ModuleInfo newModule = moduleInfo.Clone();

                newModule.UniqueId = Guid.NewGuid(); // Cloned Module requires a different uniqueID
                newModule.TabModuleID = Null.NullInteger;
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
                        object objObject = Reflection.CreateObject(newModule.DesktopModule.BusinessControllerClass, newModule.DesktopModule.BusinessControllerClass);
                        if (objObject is IPortable)
                        {
                            try
                            {
                                SetCloneModuleContext(true);
                                string content = Convert.ToString(((IPortable)objObject).ExportModule(moduleId));
                                if (!string.IsNullOrEmpty(content))
                                {
                                    ((IPortable)objObject).ImportModule(newModule.ModuleID, content, newModule.DesktopModule.Version, userID);
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
            }
        }

        private IEnumerable<PortalInfo> GetCurrentPortalsGroup()
        {
            var groups = PortalGroupController.Instance.GetPortalGroups().ToArray();

            var result = (from @group in groups
                          select PortalGroupController.Instance.GetPortalsByGroup(@group.PortalGroupId)
                              into portals
                          where portals.Any(x => x.PortalID == PortalSettings.Current.PortalId)
                          select portals.ToArray()).FirstOrDefault();

            // Are we in a group of one?
            if (result == null || result.Length == 0)
            {
                result = new[] { PortalController.Instance.GetPortal(PortalSettings.Current.PortalId) };
            }

            return result;
        }

        private void LoadAllLists()
        {
            this.LoadSiteList();
            this.LoadCategoryList();
            this.LoadPageList();
            this.LoadModuleList();
            this.LoadVisibilityList();
            this.LoadPaneList();
            this.LoadPositionList();
            this.LoadPaneModulesList();
        }

        private void LoadCategoryList()
        {
            this.CategoryListPanel.Visible = !this.AddExistingModule.Checked;

            ITermController termController = Util.GetTermController();
            this.CategoryList.DataSource = termController.GetTermsByVocabulary("Module_Categories").OrderBy(t => t.Weight).Where(t => t.Name != "< None >").ToList();
            this.CategoryList.DataBind();

            // CategoryList.Items.Add(new ListItem(Localization.GetString("AllCategories", LocalResourceFile), "All"));
            this.CategoryList.AddItem(Localization.GetString("AllCategories", this.LocalResourceFile), "All");
            if (!this.IsPostBack)
            {
                this.CategoryList.Select("Common", false);
            }
        }

        private void LoadModuleList()
        {
            if (this.AddExistingModule.Checked)
            {
                // Get list of modules for the selected tab
                if (!string.IsNullOrEmpty(this.PageLst.SelectedValue))
                {
                    var tabId = int.Parse(this.PageLst.SelectedValue);
                    if (tabId >= 0)
                    {
                        this.ModuleLst.BindTabModulesByTabID(tabId);
                    }

                    if (this.ModuleLst.ItemCount > 0)
                    {
                        this.chkCopyModule.Visible = true;
                        this.SetCopyModuleMessage(GetIsPortable(this.ModuleLst.SelectedValue, this.PageLst.SelectedValue));
                    }
                }
            }
            else
            {
                this.ModuleLst.Filter = this.CategoryList.SelectedValue == "All"
                                        ? (kvp => true)
                                         : (Func<KeyValuePair<string, PortalDesktopModuleInfo>, bool>)(kvp => kvp.Value.DesktopModule.Category == this.CategoryList.SelectedValue);
                this.ModuleLst.BindAllPortalDesktopModules();
            }

            this.ModuleLst.Enabled = this.ModuleLst.ItemCount > 0;
        }

        private void LoadPageList()
        {
            this.PageListPanel.Visible = this.AddExistingModule.Checked;
            this.TitlePanel.Enabled = !this.AddExistingModule.Checked;
            this.chkCopyModule.Visible = this.AddExistingModule.Checked;

            if (this.AddExistingModule.Checked)
            {
                this.chkCopyModule.Text = Localization.GetString("CopyModuleDefault.Text", this.LocalResourceFile);
            }

            var portalSettings = this.SelectedPortalSettings;

            this.PageLst.Items.Clear();

            if (this.PageListPanel.Visible)
            {
                this.PageLst.DataValueField = "TabID";
                this.PageLst.DataTextField = "IndentedTabName";
                if (this.PortalSettings.PortalId == this.SelectedPortalSettings.PortalId)
                {
                    this.PageLst.DataSource = TabController.GetPortalTabs(portalSettings.PortalId, portalSettings.ActiveTab.TabID, true, string.Empty, true, false, false, false, true);
                }
                else
                {
                    this.PageLst.DataSource = TabController.GetPortalTabs(portalSettings.PortalId, Null.NullInteger, true, string.Empty, true, false, false, false, true);
                }

                this.PageLst.DataBind();
            }
        }

        private void LoadPaneList()
        {
            this.PaneLst.Items.Clear();
            this.PaneLst.DataSource = PortalSettings.Current.ActiveTab.Panes;
            this.PaneLst.DataBind();
            if (PortalSettings.Current.ActiveTab.Panes.Contains(Globals.glbDefaultPane))
            {
                this.PaneLst.SelectedValue = Globals.glbDefaultPane;
            }
        }

        private void LoadPaneModulesList()
        {
            var items = new Dictionary<string, string> { { string.Empty, string.Empty } };

            foreach (ModuleInfo m in PortalSettings.Current.ActiveTab.Modules)
            {
                // if user is allowed to view module and module is not deleted
                if (ModulePermissionController.CanViewModule(m) && !m.IsDeleted)
                {
                    // modules which are displayed on all tabs should not be displayed on the Admin or Super tabs
                    if (!m.AllTabs || !PortalSettings.Current.ActiveTab.IsSuperTab)
                    {
                        if (m.PaneName == this.PaneLst.SelectedValue)
                        {
                            int moduleOrder = m.ModuleOrder;

                            while (items.ContainsKey(moduleOrder.ToString()) || moduleOrder == 0)
                            {
                                moduleOrder++;
                            }

                            items.Add(moduleOrder.ToString(), m.ModuleTitle);
                        }
                    }
                }
            }

            this.PaneModulesLst.Enabled = true;
            this.PaneModulesLst.Items.Clear();
            this.PaneModulesLst.DataValueField = "key";
            this.PaneModulesLst.DataTextField = "value";
            this.PaneModulesLst.DataSource = items;
            this.PaneModulesLst.DataBind();

            if (this.PaneModulesLst.Items.Count <= 1)
            {
                var listItem = this.PositionLst.FindItemByValue("ABOVE");
                if (listItem != null)
                {
                    this.PositionLst.Items.Remove(listItem);
                }

                listItem = this.PositionLst.FindItemByValue("BELOW");
                if (listItem != null)
                {
                    this.PositionLst.Items.Remove(listItem);
                }

                this.PaneModulesLst.Enabled = false;
            }

            if (this.PositionLst.SelectedValue == "TOP" || this.PositionLst.SelectedValue == "BOTTOM")
            {
                this.PaneModulesLst.Enabled = false;
            }
        }

        private void LoadPositionList()
        {
            var items = new Dictionary<string, string>
                            {
                                { "TOP", this.GetString("Top") },
                                { "ABOVE", this.GetString("Above") },
                                { "BELOW", this.GetString("Below") },
                                { "BOTTOM", this.GetString("Bottom") },
                            };

            this.PositionLst.Items.Clear();
            this.PositionLst.DataValueField = "key";
            this.PositionLst.DataTextField = "value";
            this.PositionLst.DataSource = items;
            this.PositionLst.DataBind();
            this.PositionLst.SelectedValue = "BOTTOM";
        }

        private void LoadSiteList()
        {
            // Is there more than one site in this group?
            var multipleSites = this.GetCurrentPortalsGroup().Count() > 1;

            this.SiteListPanel.Visible = multipleSites && this.AddExistingModule.Checked;

            if (this.SiteListPanel.Visible)
            {
                // Get a list of portals in this SiteGroup.
                var portals = PortalController.Instance.GetPortals().Cast<PortalInfo>().ToArray();

                this.SiteList.DataSource = portals.Select(
                    x => new { Value = x.PortalID, Name = x.PortalName, GroupID = x.PortalGroupID }).ToList();
                this.SiteList.DataTextField = "Name";
                this.SiteList.DataValueField = "Value";
                this.SiteList.DataBind();
            }
        }

        private void LoadVisibilityList()
        {
            this.VisibilityLst.Enabled = !this.AddExistingModule.Checked;
            if (this.VisibilityLst.Enabled)
            {
                var items = new Dictionary<string, string> { { "0", this.GetString("PermissionView") }, { "1", this.GetString("PermissionEdit") } };

                this.VisibilityLst.Items.Clear();
                this.VisibilityLst.DataValueField = "key";
                this.VisibilityLst.DataTextField = "value";
                this.VisibilityLst.DataSource = items;
                this.VisibilityLst.DataBind();
            }
        }

        private void SetCopyModuleMessage(bool isPortable)
        {
            if (isPortable)
            {
                this.chkCopyModule.Text = Localization.GetString("CopyModuleWcontent", this.LocalResourceFile);
                this.chkCopyModule.ToolTip = Localization.GetString("CopyModuleWcontent.ToolTip", this.LocalResourceFile);
            }
            else
            {
                this.chkCopyModule.Text = Localization.GetString("CopyModuleWOcontent", this.LocalResourceFile);
                this.chkCopyModule.ToolTip = Localization.GetString("CopyModuleWOcontent.ToolTip", this.LocalResourceFile);
            }
        }
    }
}
