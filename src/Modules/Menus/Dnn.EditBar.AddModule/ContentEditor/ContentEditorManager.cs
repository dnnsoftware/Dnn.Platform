#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Dnn.EditBar.AddModule.Helpers;
using Dnn.EditBar.Library;
using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Tabs.TabVersions;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Personalization;
using DotNetNuke.UI;
using DotNetNuke.UI.Containers;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.UI.WebControls;
using Globals = DotNetNuke.Common.Globals;

namespace Dnn.EditBar.AddModule.ContentEditor
{
    /// <summary>
    /// Content Editor Manager
    /// </summary>
    public class ContentEditorManager : UserControlBase
    {
        #region Fields
        private const int CssFileOrder = 40;
        public const string ControlFolder = "~/admin/Dnn.EditBar/Resources";
        private bool _supportAjax = true;
        #endregion

        #region Properties

        public Skin Skin { get; set; }

        private string LocalResourcesFile
        {
            get { return Path.Combine(ControlFolder, "ContentEditorManager/App_LocalResources/SharedResources.resx"); }
        }

        private bool SupportAjax
        {
            get
            {
                return ScriptManager.GetCurrent(Page) != null && _supportAjax;
            }
            set
            {
                _supportAjax = value;
            }
        }

        #endregion

        #region Static Methods

        internal static ContentEditorManager GetCurrent(Page page)
        {
            if (page.Items.Contains("ContentEditorManager"))
            {
                return page.Items["ContentEditorManager"] as ContentEditorManager;
            }

            return null;
        }

        #endregion

        #region Override Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (GetCurrent(Page) != null)
            {
                throw new Exception("Instance has already initialized");
            }

            var user = PortalSettings.UserInfo;

            var hasContentEditorRole = user.UserID > 0;

            if (hasContentEditorRole)
            {
                ClientAPI.RegisterClientVariable(Page, "dnn_current_userid", PortalSettings.UserInfo.UserID.ToString(), true);
            }

            var controlPanelVisible = true;
            if (!user.IsSuperUser && !PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName))
            {
                if (hasContentEditorRole)
                {
                    controlPanelVisible = false;
                }
                else
                {
                    controlPanelVisible = PageSecurityHelper.IsPageAdmin() || PageSecurityHelper.IsModuleAdmin(PortalSettings);
                }
            }

            var isSpecialPageMode = Request.QueryString["dnnprintmode"] == "true" || Request.QueryString["popUp"] == "true";
            if (isSpecialPageMode
                || PortalSettings.UserMode != PortalSettings.Mode.Edit)
            {
                Parent.Controls.Remove(this);
                return;
            }

            RegisterClientResources();

            Page.Items.Add("ContentEditorManager", this);

            //if there is pending work cookie, then reset it
            CheckPendingData();

            //if there is callback data cookie, then process the module for drag.
            CheckCallbackData();

            EnsureChildControls();
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            RemoveEmptyPaneClass();
            RegisterInitScripts();
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            foreach (string paneId in PortalSettings.ActiveTab.Panes)
            {
                var pane = Skin.FindControl(paneId) as HtmlContainerControl;
                if (pane == null)
                {
                    continue;
                }

                var ajaxPanel = new DnnAjaxPanel
                {
                    ID = pane.ID + "_SyncPanel",
                    RestoreOriginalRenderDelegate = false,

                };

                try
                {
                    if (!Page.IsPostBack)
                    {
                        //find update panels in pane and fire the unload event for a known issue: CONTENT-4039
                        var updatePanels = GetUpdatePanelsInPane(pane);
                        updatePanels.ForEach(p => p.Unload += UpdatePanelUnloadEvent);
                    }

                    var paneIndex = pane.Parent.Controls.IndexOf(pane);
                    pane.Parent.Controls.AddAt(paneIndex, ajaxPanel);
                    ajaxPanel.Controls.Add(pane);
                }
                catch (Exception ex)
                {
                    SupportAjax = false;
                    return;
                }

                ajaxPanel.CssClass = pane.Attributes["class"];
                pane.Attributes["class"] = string.Empty;

                var scriptManager = ScriptManager.GetCurrent(Page);
                if (scriptManager != null && scriptManager.IsInAsyncPostBack
                        && ajaxPanel.UniqueID == Request.Form["__EVENTTARGET"]
                        && !string.IsNullOrEmpty(Request.Form["__EVENTARGUMENT"])
                        && Request.Form["__EVENTARGUMENT"].ToLowerInvariant() != "undefined"
                        && Request.Form["__EVENTARGUMENT"].ToLowerInvariant().StartsWith("module-"))
                {
                    var moduleId = Convert.ToInt32(Request.Form["__EVENTARGUMENT"].Substring(7));

                    var moduleContainer = FindModuleContainer(moduleId);
                    if (moduleContainer != null)
                    {
                        var moduleControl = FindModuleControl(moduleId);
                        var moduleInfo = FindModuleInfo(moduleId);

                        if (moduleControl != null && moduleInfo != null && moduleContainer.Parent is HtmlContainerControl)
                        {
                            ((HtmlContainerControl) moduleContainer.Parent).Attributes["data-module-title"] = moduleInfo.ModuleTitle;
                            
                            if (HaveContentLayoutModuleOnPage())
                            {
                                Page.Items[typeof(ProxyPage)] = moduleControl;
                            }
                            else
                            {
                                moduleControl.Page = new ProxyPage(Page);
                            }

                            ProcessDragTipShown(moduleContainer);
                        }
                    }
                }
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            var scripts = ScriptManager.GetCurrent(Page).GetRegisteredStartupScripts()
                .Where(s => s.Control is ProxyPage).ToList();
            foreach (var script in scripts)
            {
                ScriptManager.RegisterStartupScript(Page, script.Type, script.Key, script.Script, script.AddScriptTags);
            }
            base.Render(writer);
        }

        #endregion

        #region Private Methods

        private void RegisterClientResources()
        {
            ClientResourceManager.EnableAsyncPostBackHandler();
            ClientResourceManager.RegisterScript(Page, Path.Combine(ControlFolder, "ContentEditorManager/Js/ModuleManager.js"));
            ClientResourceManager.RegisterScript(Page, Path.Combine(ControlFolder, "ContentEditorManager/Js/ModuleDialog.js"));
            ClientResourceManager.RegisterScript(Page, Path.Combine(ControlFolder, "ContentEditorManager/Js/ModuleService.js"));
            ClientResourceManager.RegisterScript(Page, Path.Combine(ControlFolder, "ContentEditorManager/Js/ContentEditor.js"));
            ClientResourceManager.RegisterStyleSheet(Page,
                Path.Combine(ControlFolder, "ContentEditorManager/Styles/ContentEditor.css"), CssFileOrder);
            ServicesFramework.Instance.RequestAjaxScriptSupport();

            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            //We need to add the Dnn JQuery plugins because the Edit Bar removes the Control Panel from the page
            JavaScript.RequestRegistration(CommonJs.KnockoutMapping);
            //ClientResourceManager.RegisterScript(Page,
            //    Constants.SharedAssetsPath + "Javascripts/knockout.validation.min.js", 110);
            //ClientResourceManager.RegisterScript(Page,
            //    Constants.SharedAssetsPath + "Javascripts/dnn.jquery.notify.js");

            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js");
            ClientResourceManager.RegisterStyleSheet(Page,
                "~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css");
        }

        private IEnumerable<IEnumerable<string>> GetPaneClientIdCollection()
        {
            var panelClientIds = new List<List<string>>(PortalSettings.ActiveTab.Panes.Count);

            try
            {
                var skinControl = Page.FindControl("SkinPlaceHolder").Controls[0];

                foreach (var pane in PortalSettings.ActiveTab.Panes.Cast<string>())
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

        private static void FindControlRecursive(Control rootControl, string controlId, ICollection<Control> foundControls)
        {

            if (rootControl.ID == controlId)
            {
                foundControls.Add(rootControl);
            }

            foreach(Control subControl in rootControl.Controls)
            {
                FindControlRecursive(subControl, controlId, foundControls);
            }
        }

        /// <summary>
        /// remove default empty pane class as some skin have special style on it
        /// and it may caught style issues with content editor feature, then we use a
        /// new style to cover it.
        /// </summary>
        private void RemoveEmptyPaneClass()
        {
            foreach (string paneId in PortalSettings.ActiveTab.Panes)
            {
                var paneControl = Skin.FindControl(paneId) as HtmlContainerControl;
                if (paneControl != null
                        && !string.IsNullOrEmpty(paneControl.Attributes["class"])
                        && paneControl.Attributes["class"].Contains("DNNEmptyPane"))
                {
                    paneControl.Attributes["class"] =
                        paneControl.Attributes["class"].Replace("DNNEmptyPane", "EvoqEmptyPane");
                }
            }
        }

        private void RegisterInitScripts()
        {
            RegisterLocalResources();

            ClientAPI.RegisterClientVariable(Page, "cem_loginurl", Globals.LoginURL(HttpContext.Current.Request.RawUrl, false), true);

            var panes = string.Join(",", PortalSettings.ActiveTab.Panes.Cast<string>());
            var panesClientIds = GetPanesClientIds(GetPaneClientIdCollection());
            const string scriptFormat = @"dnn.ContentEditorManager.init({{type: 'moduleManager', panes: '{0}', panesClientIds: '{2}', supportAjax: {1}}});";
            var script = string.Format(scriptFormat,
                                            panes,
                                            SupportAjax ? "true" : "false",
                                            panesClientIds);

            if (ScriptManager.GetCurrent(Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(Page, GetType(), "ContentEditorManager", script, true);
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(GetType(), "ContentEditorManager", script, true);
            }
        }

        private string GetPanesClientIds(IEnumerable<IEnumerable<string>> panelCliendIdCollection)
        {
            return string.Join(";", panelCliendIdCollection.Select(x => String.Join(",", x)));
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
                                                                                    categoryAll: '{12}'
                                                                                }};";

            var script = string.Format(scriptFormat,
                Localization.GetSafeJSString("AddModule.Text", LocalResourcesFile),
                Localization.GetSafeJSString("NoModules.Text", LocalResourcesFile),
                Localization.GetSafeJSString("DragTip.Text", LocalResourcesFile),
                Localization.GetSafeJSString("PendingSave.Text", LocalResourcesFile),
                Localization.GetSafeJSString("ConfirmTitle.Text", LocalResourcesFile),
                Localization.GetSafeJSString("ConfirmYes.Text", LocalResourcesFile),
                Localization.GetSafeJSString("ConfirmNo.Text", LocalResourcesFile),
                Localization.GetSafeJSString("CancelConfirm.Text", LocalResourcesFile),
                Localization.GetSafeJSString("DeleteModuleConfirm.Text", LocalResourcesFile),
                Localization.GetSafeJSString("Cancel.Text", LocalResourcesFile),
                Localization.GetSafeJSString("SearchPlaceHolder.Text", LocalResourcesFile),
                Localization.GetSafeJSString("Category_Recommended.Text", LocalResourcesFile),
                Localization.GetSafeJSString("Category_All.Text", LocalResourcesFile)
                );

            if (ScriptManager.GetCurrent(Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(Page, GetType(), "ContentEditorManagerResources", script, true);
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(GetType(), "ContentEditorManagerResources", script, true);
            }
        }

        private void CheckPendingData()
        {
            if (Request.Cookies["cem_pending"] != null)
            {
                var cookie = Request.Cookies["cem_pending"];
                var pendingData = cookie.Value;
                if (!string.IsNullOrEmpty(pendingData))
                {
                    var tabId = PortalSettings.ActiveTab.TabID;
                    int moduleId;
                    if (pendingData.StartsWith("module-")
                            && Int32.TryParse(pendingData.Substring(7), out moduleId)
                            && ModuleController.Instance.GetModule(moduleId, tabId, false) != null)
                    {
                        RemoveTabModule(tabId, moduleId);

                        //remove related modules
                        ModuleController.Instance.GetTabModules(tabId).Values
                            .Where(m => m.ModuleID > moduleId)
                            .ForEach(m =>
                            {
                                RemoveTabModule(tabId, m.ModuleID);
                            });
                    }
                }

                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }
        }

        private void RemoveTabModule(int tabId, int moduleId)
        {
            ModuleController.Instance.DeleteTabModule(tabId, moduleId, false);

            //remove that module control
            var moduleControl = ControlUtilities.FindFirstDescendent<Container>(Skin,
                c => c.ID == "ctr" + moduleId);

            if (moduleControl != null)
            {
                moduleControl.Parent.Parent.Controls.Remove(moduleControl.Parent);
            }
        }

        private void CheckCallbackData()
        {
            if (Request.Cookies["CEM_CallbackData"] != null)
            {
                var cookie = Request.Cookies["CEM_CallbackData"];
                var callbackData = cookie.Value;
                if (!string.IsNullOrEmpty(callbackData) && callbackData.StartsWith("module-"))
                {
                    var moduleId = Convert.ToInt32(callbackData.Substring(7));

                    var moduleContainer = FindModuleContainer(moduleId);
                    var moduleInfo = FindModuleInfo(moduleId);
                    if (moduleContainer != null && moduleInfo != null && moduleContainer.Parent is HtmlContainerControl)
                    {
                        ((HtmlContainerControl) moduleContainer.Parent).Attributes["data-module-title"] = moduleInfo.ModuleTitle;
                        ProcessDragTipShown(moduleContainer);
                    }
                }
            }
        }

        private void ProcessDragTipShown(Container moduleContainer)
        {
            var dragTipShown = Convert.ToString(Personalization.GetProfile("Usability", "DragTipShown" + PortalSettings.PortalId));
            if (string.IsNullOrEmpty(dragTipShown) && moduleContainer.Parent is HtmlContainerControl && Request.Cookies["noFloat"] == null)
            {
                Personalization.SetProfile("Usability", "DragTipShown" + PortalSettings.PortalId, "true");
                ((HtmlContainerControl) moduleContainer.Parent).Attributes["class"] += " dragtip";
            }
        }

        private Container FindModuleContainer(int moduleId)
        {
            return ControlUtilities.FindFirstDescendent<Container>(Skin, c => c.ID == "ctr" + moduleId);
        }

        private Control FindModuleControl(int moduleId)
        {
            var moduleContainer = FindModuleContainer(moduleId);
            if (moduleContainer != null)
            {
                var moduleInfo = FindModuleInfo(moduleId);
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
            return PortalSettings.ActiveTab.Modules.Cast<ModuleInfo>()
                        .FirstOrDefault(m => m.ModuleID == moduleId);
        }

        private List<UpdatePanel> GetUpdatePanelsInPane(Control parent)
        {
            var panels = new List<UpdatePanel>();
            if (parent is UpdatePanel)
            {
                panels.Add(parent as UpdatePanel);
            }
            else
            {
                foreach (Control childControl in parent.Controls)
                {
                    panels.AddRange(GetUpdatePanelsInPane(childControl));
                }
            }

            return panels;
        }

        private void UpdatePanelUnloadEvent(object sender, EventArgs e)
        {
            try
            {
                var methodInfo = typeof(ScriptManager).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                            .First(i => i.Name.Equals("System.Web.UI.IScriptManagerInternal.RegisterUpdatePanel"));
                methodInfo.Invoke(ScriptManager.GetCurrent(Page),
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
                return PortalSettings.ActiveTab.Modules.Cast<ModuleInfo>().Any(m => m.ModuleDefID == moduleDefinition.ModuleDefID);
            }

            return false;
        }

        #endregion

        #region Helper Classes


        public class ProxyPage : CDefault
        {
            private readonly Page _originalPage;
            public ProxyPage(Page originalPage)
            {
                _originalPage = originalPage;

                SetField("_header", originalPage.Header);
                SetField("_request", originalPage.Request);
                SetField("_response", originalPage.Response);
                SetField("_context", HttpContext.Current);
                SetField("_application", HttpContext.Current.Application);
                SetField("_cache", HttpContext.Current.Cache);

                CopyProperty(originalPage, "TemplateControlVirtualDirectory");

                foreach (var key in originalPage.Items.Keys)
                {
                    Items.Add(key, originalPage.Items[key]);
                }
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
                    var value = property.GetValue(original, new object[]{});

                    property.SetValue(this, value, new object[] { });
                }
            }

            public override Control FindControl(string id)
            {
                return _originalPage.FindControl(id);
            }
        }

        #endregion
    }
}