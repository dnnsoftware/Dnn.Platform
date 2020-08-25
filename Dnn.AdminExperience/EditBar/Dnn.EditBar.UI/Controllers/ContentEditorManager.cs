// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Collections;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI;
    using DotNetNuke.UI.Containers;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.UI.WebControls;
    using Newtonsoft.Json;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>
    /// Content Editor Manager.
    /// </summary>
    public class ContentEditorManager : UserControlBase
    {
        public const string ControlFolder = "~/DesktopModules/admin/Dnn.EditBar/Resources";
        private const int CssFileOrder = 40;
        private bool _supportAjax = true;

        public Skin Skin { get; set; }

        private string LocalResourcesFile
        {
            get { return Path.Combine(ControlFolder, "ContentEditorManager/App_LocalResources/SharedResources.resx"); }
        }

        private bool SupportAjax
        {
            get
            {
                return ScriptManager.GetCurrent(this.Page) != null && this._supportAjax;
            }

            set
            {
                this._supportAjax = value;
            }
        }

        public static bool HasTabPermission(string permissionKey)
        {
            var principal = Thread.CurrentPrincipal;
            if (!principal.Identity.IsAuthenticated)
            {
                return false;
            }

            var currentPortal = PortalController.Instance.GetCurrentPortalSettings();

            bool isAdminUser = currentPortal.UserInfo.IsSuperUser || PortalSecurity.IsInRole(currentPortal.AdministratorRoleName);
            if (isAdminUser)
            {
                return true;
            }

            return TabPermissionController.HasTabPermission(permissionKey);
        }

        internal static ContentEditorManager GetCurrent(Page page)
        {
            if (page.Items.Contains("ContentEditorManager"))
            {
                return page.Items["ContentEditorManager"] as ContentEditorManager;
            }

            return null;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (GetCurrent(this.Page) != null)
            {
                throw new Exception("Instance has already initialized");
            }

            this.AutoSetUserMode();

            var user = this.PortalSettings.UserInfo;

            if (user.UserID > 0)
            {
                ClientAPI.RegisterClientVariable(this.Page, "dnn_current_userid", this.PortalSettings.UserInfo.UserID.ToString(), true);
            }

            if (this.PortalSettings.UserMode != PortalSettings.Mode.Edit
                    || !this.IsPageEditor()
                    || EditBarController.Instance.GetMenuItems().Count == 0)
            {
                this.Parent.Controls.Remove(this);
                return;
            }

            this.RegisterClientResources();

            this.RegisterEditBarResources();

            this.Page.Items.Add("ContentEditorManager", this);

            // if there is pending work cookie, then reset it
            this.CheckPendingData();

            // if there is callback data cookie, then process the module for drag.
            this.CheckCallbackData();

            this.EnsureChildControls();
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            this.RemoveEmptyPaneClass();
            this.RegisterInitScripts();
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            foreach (string paneId in this.PortalSettings.ActiveTab.Panes)
            {
                var pane = this.Skin.FindControl(paneId) as HtmlContainerControl;
                if (pane == null)
                {
                    continue;
                }

                // create update panel
                var updatePanel = new UpdatePanel
                {
                    UpdateMode = UpdatePanelUpdateMode.Conditional,
                    ID = pane.ID + "_SyncPanel",
                    ChildrenAsTriggers = true,
                };

                try
                {
                    // find update panels in pane and fire the unload event for a known issue: CONTENT-4039
                    var updatePanels = this.GetUpdatePanelsInPane(pane);
                    updatePanels.ForEach(p => p.Unload += this.UpdatePanelUnloadEvent);
                    updatePanel.Unload += this.UpdatePanelUnloadEvent;

                    var paneIndex = pane.Parent.Controls.IndexOf(pane);
                    pane.Parent.Controls.AddAt(paneIndex, updatePanel);

                    var templateContainer = updatePanel.ContentTemplateContainer;
                    templateContainer.Controls.Add(pane);
                }
                catch (Exception)
                {
                    this.SupportAjax = false;
                    return;
                }

                updatePanel.Attributes.Add("class", $"DnnAjaxPanel {pane.Attributes["class"]}");
                pane.Attributes["class"] = string.Empty;

                var scriptManager = ScriptManager.GetCurrent(this.Page);
                if (scriptManager != null && scriptManager.IsInAsyncPostBack
                        && updatePanel.ClientID == this.Request.Form["__EVENTTARGET"]
                        && !string.IsNullOrEmpty(this.Request.Form["__EVENTARGUMENT"])
                        && this.Request.Form["__EVENTARGUMENT"].ToLowerInvariant() != "undefined"
                        && this.Request.Form["__EVENTARGUMENT"].ToLowerInvariant().StartsWith("module-"))
                {
                    var moduleId = Convert.ToInt32(this.Request.Form["__EVENTARGUMENT"].Substring(7));

                    var moduleContainer = this.FindModuleContainer(moduleId);
                    if (moduleContainer != null)
                    {
                        var moduleControl = this.FindModuleControl(moduleId);
                        var moduleInfo = this.FindModuleInfo(moduleId);

                        if (moduleControl != null && moduleInfo != null && moduleContainer.Parent is HtmlContainerControl)
                        {
                            ((HtmlContainerControl)moduleContainer.Parent).Attributes["data-module-title"] = moduleInfo.ModuleTitle;

                            if (this.HaveContentLayoutModuleOnPage())
                            {
                                this.Page.Items[typeof(ProxyPage)] = moduleControl;
                            }
                            else
                            {
                                moduleControl.Page = new ProxyPage(this.Page);
                            }

                            this.ProcessDragTipShown(moduleContainer);
                        }
                    }
                }
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            var scripts = ScriptManager.GetCurrent(this.Page).GetRegisteredStartupScripts()
                .Where(s => s.Control is ProxyPage).ToList();
            foreach (var script in scripts)
            {
                ScriptManager.RegisterStartupScript(this.Page, script.Type, script.Key, script.Script, script.AddScriptTags);
            }

            base.Render(writer);
        }

        private static void FindControlRecursive(Control rootControl, string controlId, ICollection<Control> foundControls)
        {
            if (rootControl.ID == controlId)
            {
                foundControls.Add(rootControl);
            }

            foreach (Control subControl in rootControl.Controls)
            {
                FindControlRecursive(subControl, controlId, foundControls);
            }
        }

        private void RegisterClientResources()
        {
            ClientResourceManager.EnableAsyncPostBackHandler();

            // register drop down list required resources
            ClientResourceManager.RegisterStyleSheet(this.Page, "~/Resources/Shared/components/DropDownList/dnn.DropDownList.css", FileOrder.Css.ResourceCss);
            ClientResourceManager.RegisterStyleSheet(this.Page, "~/Resources/Shared/scripts/jquery/dnn.jScrollBar.css", FileOrder.Css.ResourceCss);

            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/dnn.extensions.js");
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/dnn.jquery.extensions.js");
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/dnn.DataStructures.js");
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/jquery/jquery.mousewheel.js");
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/jquery/dnn.jScrollBar.js");
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/TreeView/dnn.TreeView.js");
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/TreeView/dnn.DynamicTreeView.js");
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/Components/DropDownList/dnn.DropDownList.js");

            ClientResourceManager.RegisterScript(this.Page, Path.Combine(ControlFolder, "ContentEditorManager/Js/ModuleManager.js"));
            ClientResourceManager.RegisterScript(this.Page, Path.Combine(ControlFolder, "ContentEditorManager/Js/ModuleDialog.js"));
            ClientResourceManager.RegisterScript(this.Page, Path.Combine(ControlFolder, "ContentEditorManager/Js/ExistingModuleDialog.js"));
            ClientResourceManager.RegisterScript(this.Page, Path.Combine(ControlFolder, "ContentEditorManager/Js/ModuleService.js"));
            ClientResourceManager.RegisterScript(this.Page, Path.Combine(ControlFolder, "ContentEditorManager/Js/ContentEditor.js"));
            ClientResourceManager.RegisterStyleSheet(
                this.Page,
                Path.Combine(ControlFolder, "ContentEditorManager/Styles/ContentEditor.css"), CssFileOrder);
            ServicesFramework.Instance.RequestAjaxScriptSupport();

            JavaScript.RequestRegistration(CommonJs.DnnPlugins);

            // We need to add the Dnn JQuery plugins because the Edit Bar removes the Control Panel from the page
            JavaScript.RequestRegistration(CommonJs.KnockoutMapping);

            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js");
            ClientResourceManager.RegisterStyleSheet(
                this.Page,
                "~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css");
        }

        private void RegisterEditBarResources()
        {
            JavaScript.RequestRegistration(CommonJs.jQuery);
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            ClientAPI.RegisterClientVariable(this.Page, "editbar_isAdmin", this.IsAdmin().ToString(), true);

            var settings = EditBarController.Instance.GetConfigurations(this.PortalSettings.PortalId);
            var settingsScript = "window.editBarSettings = " + JsonConvert.SerializeObject(settings) + ";";
            this.Page.ClientScript.RegisterClientScriptBlock(this.Page.GetType(), "EditBarSettings", settingsScript, true);

            ClientResourceManager.RegisterScript(this.Page, "~/DesktopModules/admin/Dnn.EditBar/scripts/editBarContainer.js");

            ClientResourceManager.RegisterStyleSheet(this.Page, "~/DesktopModules/admin/Dnn.EditBar/css/editBarContainer.css");
        }

        private bool IsPageEditor()
        {
            return HasTabPermission("EDIT");
        }

        private IEnumerable<IEnumerable<string>> GetPaneClientIdCollection()
        {
            var panelClientIds = new List<List<string>>(this.PortalSettings.ActiveTab.Panes.Count);

            try
            {
                var skinControl = this.Page.FindControl("SkinPlaceHolder").Controls[0];

                foreach (var pane in this.PortalSettings.ActiveTab.Panes.Cast<string>())
                {
                    var foundControls = new List<Control>();
                    FindControlRecursive(skinControl, pane, foundControls);
                    panelClientIds.Add((from control in foundControls select control.ClientID).ToList());
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            return panelClientIds;
        }

        /// <summary>
        /// remove default empty pane class as some skin have special style on it
        /// and it may caught style issues with content editor feature, then we use a
        /// new style to cover it.
        /// </summary>
        private void RemoveEmptyPaneClass()
        {
            foreach (string paneId in this.PortalSettings.ActiveTab.Panes)
            {
                var paneControl = this.Skin.FindControl(paneId) as HtmlContainerControl;
                if (paneControl != null
                        && !string.IsNullOrEmpty(paneControl.Attributes["class"])
                        && paneControl.Attributes["class"].Contains("DNNEmptyPane"))
                {
                    paneControl.Attributes["class"] = $"{paneControl.Attributes["class"]} EditBarEmptyPane";
                }
            }
        }

        private void RegisterInitScripts()
        {
            this.RegisterLocalResources();

            ClientAPI.RegisterClientVariable(this.Page, "cem_loginurl", Globals.LoginURL(HttpContext.Current.Request.RawUrl, false), true);

            var panes = string.Join(",", this.PortalSettings.ActiveTab.Panes.Cast<string>());
            var panesClientIds = this.GetPanesClientIds(this.GetPaneClientIdCollection());
            const string scriptFormat = @"dnn.ContentEditorManager.init({{type: 'moduleManager', panes: '{0}', panesClientIds: '{2}', supportAjax: {1}}});";
            var script = string.Format(
                scriptFormat,
                panes,
                this.SupportAjax ? "true" : "false",
                panesClientIds);

            if (ScriptManager.GetCurrent(this.Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "ContentEditorManager", script, true);
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "ContentEditorManager", script, true);
            }
        }

        private string GetPanesClientIds(IEnumerable<IEnumerable<string>> panelCliendIdCollection)
        {
            return string.Join(";", panelCliendIdCollection.Select(x => string.Join(",", x)));
        }

        private void RegisterLocalResources()
        {
            const string scriptFormat = @"dnn.ContentEditorManagerResources = {{
                                                                                    title: '{0}',
                                                                                    nomodules: '{1}',
                                                                                    dragtip: '{2}',
                                                                                    pendingsave: '{3}',
                                                                                    confirmTitle: '{4}',
                                                                                    confirmYes: '{5}',
                                                                                    confirmNo: '{6}',
                                                                                    cancelConfirm: '{7}',
                                                                                    deleteModuleConfirm: '{8}',
                                                                                    cancel: '{9}',
                                                                                    searchPlaceHolder: '{10}',
                                                                                    categoryRecommended: '{11}',
                                                                                    categoryAll: '{12}',
                                                                                    pagePicker_clearButtonTooltip: '{13}',
                                                                                    pagePicker_loadingResultText: '{14}',
                                                                                    pagePicker_resultsText: '{15}',
                                                                                    pagePicker_searchButtonTooltip: '{16}',
                                                                                    pagePicker_searchInputPlaceHolder: '{17}',
                                                                                    pagePicker_selectedItemCollapseTooltip: '{18}',
                                                                                    pagePicker_selectedItemExpandTooltip: '{19}',
                                                                                    pagePicker_selectItemDefaultText: '{20}',
                                                                                    pagePicker_sortAscendingButtonTitle: '{21}',
                                                                                    pagePicker_sortAscendingButtonTooltip: '{22}',
                                                                                    pagePicker_sortDescendingButtonTooltip: '{23}',
                                                                                    pagePicker_unsortedOrderButtonTooltip: '{24}',
                                                                                    site: '{25}',
                                                                                    page: '{26}',
                                                                                    addExistingModule: '{27}',
                                                                                    makeCopy: '{28}'
                                                                                }};";

            var script = string.Format(
                scriptFormat,
                Localization.GetSafeJSString("AddModule.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("NoModules.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("DragTip.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("PendingSave.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("ConfirmTitle.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("ConfirmYes.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("ConfirmNo.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("CancelConfirm.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("DeleteModuleConfirm.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("Cancel.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("SearchPlaceHolder.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("Category_Recommended.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("Category_All.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_clearButtonTooltip.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_loadingResultText.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_resultsText.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_searchButtonTooltip.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_searchInputPlaceHolder.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_selectedItemCollapseTooltip.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_selectedItemExpandTooltip.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_selectItemDefaultText.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_sortAscendingButtonTitle.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_sortAscendingButtonTooltip.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_sortDescendingButtonTooltip.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_unsortedOrderButtonTooltip.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("Site.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("Page.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("AddExistingModule.Text", this.LocalResourcesFile),
                Localization.GetSafeJSString("MakeCopy.Text", this.LocalResourcesFile));

            if (ScriptManager.GetCurrent(this.Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "ContentEditorManagerResources", script, true);
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "ContentEditorManagerResources", script, true);
            }
        }

        private void CheckPendingData()
        {
            if (this.Request.Cookies["cem_pending"] != null)
            {
                var cookie = this.Request.Cookies["cem_pending"];
                var pendingData = cookie.Value;
                if (!string.IsNullOrEmpty(pendingData))
                {
                    var tabId = this.PortalSettings.ActiveTab.TabID;
                    int moduleId;
                    if (pendingData.StartsWith("module-")
                        && int.TryParse(pendingData.Substring(7), out moduleId))
                    {
                        var module = ModuleController.Instance.GetModule(moduleId, tabId, false);
                        if (module != null)
                        {
                            this.RemoveTabModule(tabId, moduleId);

                            // remove related modules
                            ModuleController.Instance.GetTabModules(tabId).Values
                                .Where(m => m.CreatedOnDate > module.CreatedOnDate && m.CreatedByUserID == module.CreatedByUserID)
                                .ForEach(m =>
                                {
                                    this.RemoveTabModule(tabId, m.ModuleID);
                                });
                        }
                    }
                }

                cookie.Expires = DateTime.Now.AddDays(-1);
                this.Response.Cookies.Add(cookie);
            }
        }

        private void RemoveTabModule(int tabId, int moduleId)
        {
            ModuleController.Instance.DeleteTabModule(tabId, moduleId, false);

            // remove that module control
            var moduleControl = ControlUtilities.FindFirstDescendent<Container>(
                this.Skin,
                c => c.ID == "ctr" + moduleId);

            if (moduleControl != null)
            {
                moduleControl.Parent.Parent.Controls.Remove(moduleControl.Parent);
            }
        }

        private void CheckCallbackData()
        {
            if (this.Request.Cookies["CEM_CallbackData"] != null)
            {
                var cookie = this.Request.Cookies["CEM_CallbackData"];
                var callbackData = cookie.Value;
                if (!string.IsNullOrEmpty(callbackData) && callbackData.StartsWith("module-"))
                {
                    var moduleId = Convert.ToInt32(callbackData.Substring(7));

                    var moduleContainer = this.FindModuleContainer(moduleId);
                    var moduleInfo = this.FindModuleInfo(moduleId);
                    if (moduleContainer != null && moduleInfo != null && moduleContainer.Parent is HtmlContainerControl)
                    {
                        ((HtmlContainerControl)moduleContainer.Parent).Attributes["data-module-title"] = moduleInfo.ModuleTitle;
                        this.ProcessDragTipShown(moduleContainer);
                    }
                }
            }
        }

        private void ProcessDragTipShown(Container moduleContainer)
        {
            var dragTipShown = Convert.ToString(Personalization.GetProfile("Usability", "DragTipShown" + this.PortalSettings.PortalId));
            if (string.IsNullOrEmpty(dragTipShown) && moduleContainer.Parent is HtmlContainerControl && this.Request.Cookies["noFloat"] == null)
            {
                Personalization.SetProfile("Usability", "DragTipShown" + this.PortalSettings.PortalId, "true");
                ((HtmlContainerControl)moduleContainer.Parent).Attributes["class"] += " dragtip";
            }
        }

        private Container FindModuleContainer(int moduleId)
        {
            return ControlUtilities.FindFirstDescendent<Container>(this.Skin, c => c.ID == "ctr" + moduleId);
        }

        private Control FindModuleControl(int moduleId)
        {
            var moduleContainer = this.FindModuleContainer(moduleId);
            if (moduleContainer != null)
            {
                var moduleInfo = this.FindModuleInfo(moduleId);
                if (moduleInfo != null)
                {
                    var controlId = Path.GetFileNameWithoutExtension(moduleInfo.ModuleControl.ControlSrc);
                    return ControlUtilities.FindFirstDescendent<Control>(moduleContainer, c => c.ID == controlId);
                }
            }

            return null;
        }

        private ModuleInfo FindModuleInfo(int moduleId)
        {
            return this.PortalSettings.ActiveTab.Modules.Cast<ModuleInfo>()
                        .FirstOrDefault(m => m.ModuleID == moduleId);
        }

        private List<UpdatePanel> GetUpdatePanelsInPane(Control parent)
        {
            var panels = new List<UpdatePanel>();
            if (parent is UpdatePanel)
            {
                panels.Add(parent as UpdatePanel);
            }
            else if (parent != null && !this.IsListControl(parent))
            {
                foreach (Control childControl in parent.Controls)
                {
                    panels.AddRange(this.GetUpdatePanelsInPane(childControl));
                }
            }

            return panels;
        }

        private bool IsListControl(Control control)
        {
            return control is DataBoundControl || control is Repeater || control is DataGrid;
        }

        private void UpdatePanelUnloadEvent(object sender, EventArgs e)
        {
            try
            {
                var methodInfo = typeof(ScriptManager).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                            .First(i => i.Name.Equals("System.Web.UI.IScriptManagerInternal.RegisterUpdatePanel"));
                methodInfo.Invoke(
                    ScriptManager.GetCurrent(this.Page),
                    new[] { sender });
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }

        private bool HaveContentLayoutModuleOnPage()
        {
            var moduleDefinition =
                ModuleDefinitionController.GetModuleDefinitions().Values
                    .FirstOrDefault(m => m.DefinitionName == "Content Layout");
            if (moduleDefinition != null)
            {
                return this.PortalSettings.ActiveTab.Modules.Cast<ModuleInfo>().Any(m => m.ModuleDefID == moduleDefinition.ModuleDefID);
            }

            return false;
        }

        private void SetLastPageHistory(string pageId)
        {
            this.Response.Cookies.Add(new HttpCookie("LastPageId", pageId) { Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/" });
        }

        private string GetLastPageHistory()
        {
            var cookie = this.Request.Cookies["LastPageId"];
            if (cookie != null)
            {
                return cookie.Value;
            }

            return "NEW";
        }

        private void SetUserMode(string userMode)
        {
            Personalization.SetProfile("Usability", "UserMode" + this.PortalSettings.PortalId, userMode.ToUpper());
        }

        private void AutoSetUserMode()
        {
            int tabId = this.PortalSettings.ActiveTab.TabID;
            int portalId = PortalSettings.Current.PortalId;
            string pageId = string.Format("{0}:{1}", portalId, tabId);

            HttpCookie cookie = this.Request.Cookies["StayInEditMode"];
            if (cookie != null && cookie.Value == "YES")
            {
                if (PortalSettings.Current.UserMode != PortalSettings.Mode.Edit)
                {
                    this.SetUserMode("EDIT");
                    this.SetLastPageHistory(pageId);
                    this.Response.Redirect(this.Request.RawUrl, true);
                }

                return;
            }

            string lastPageId = this.GetLastPageHistory();
            var isShowAsCustomError = this.Request.QueryString.AllKeys.Contains("aspxerrorpath");

            if (lastPageId != pageId && !isShowAsCustomError)
            {
                // navigate between pages
                if (PortalSettings.Current.UserMode != PortalSettings.Mode.View)
                {
                    this.SetUserMode("VIEW");
                    this.SetLastPageHistory(pageId);
                    this.Response.Redirect(this.Request.RawUrl, true);
                }
            }

            if (!isShowAsCustomError)
            {
                this.SetLastPageHistory(pageId);
            }
        }

        private bool IsAdmin()
        {
            var user = this.PortalSettings.UserInfo;
            return user.IsSuperUser || PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName);
        }

        public class ProxyPage : CDefault
        {
            private readonly Page _originalPage;

            public ProxyPage(Page originalPage)
            {
                this._originalPage = originalPage;

                this.SetField("_header", originalPage.Header);
                this.SetField("_request", originalPage.Request);
                this.SetField("_response", originalPage.Response);
                this.SetField("_context", HttpContext.Current);
                this.SetField("_application", HttpContext.Current.Application);
                this.SetField("_cache", HttpContext.Current.Cache);

                this.CopyProperty(originalPage, "TemplateControlVirtualDirectory");

                foreach (var key in originalPage.Items.Keys)
                {
                    this.Items.Add(key, originalPage.Items[key]);
                }
            }

            public override Control FindControl(string id)
            {
                return this._originalPage.FindControl(id);
            }

            private void SetField(string name, object value)
            {
                var field = typeof(Page).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(this, value);
                }
            }

            private void CopyProperty(Page original, string propertyName)
            {
                var property = typeof(Page).GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null)
                {
                    var value = property.GetValue(original, new object[] { });

                    property.SetValue(this, value, new object[] { });
                }
            }
        }
    }
}
