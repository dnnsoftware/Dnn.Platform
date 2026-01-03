// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using Dnn.EditBar.UI.Controllers;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.Client.ResourceManager;
    using DotNetNuke.Web.MvcPipeline.UI.Utilities;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>Content Editor Manager.</summary>
    public class MvcContentEditorManager
    {
        public const string ControlFolder = "~/DesktopModules/admin/Dnn.EditBar/Resources";
        private const int CssFileOrder = 40;

        private bool supportAjax = true;

        public static PortalSettings PortalSettings => PortalController.Instance.GetCurrentPortalSettings();

        public static bool IsHostMenu
        {
            get
            {
                return Globals.IsHostTab(PortalSettings.ActiveTab.TabID);
            }
        }

        public ControllerContext Context { get; set; }

        public Skin Skin { get; set; }

        private static string LocalResourcesFile => Path.Combine(ControlFolder, "ContentEditorManager/App_LocalResources/SharedResources.resx");

        private bool SupportAjax
        {
            get
            {
                return this.supportAjax;
            }

            set
            {
                this.supportAjax = value;
            }
        }

        public static void CreateManager(Controller controller)
        {
            if (Host.DisableEditBar)
            {
                return;
            }

            var request = controller.Request;
            var isSpecialPageMode = request.QueryString["dnnprintmode"] == "true" || request.QueryString["popUp"] == "true";
            if (isSpecialPageMode
                    || Globals.IsAdminControl())
            {
                return;
            }

            if (!Globals.IsAdminControl())
            {
                if (PortalSettings.Current.UserId > 0)
                {
                    var manager = new MvcContentEditorManager();
                    manager.Context = controller.ControllerContext;
                    if (manager.OnInit())
                    {
                        manager.OnPreRender();
                    }
                }
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

#pragma warning disable CA1822 // Mark members as static
        protected bool OnInit()
#pragma warning restore CA1822 // Mark members as static
        {
            var user = PortalSettings.UserInfo;

            if (user.UserID > 0)
            {
                MvcClientAPI.RegisterClientVariable("dnn_current_userid", PortalSettings.UserInfo.UserID.ToString(), true);
            }

            if (Personalization.GetUserMode() != PortalSettings.Mode.Edit
                    || !IsPageEditor()
                    || Controllers.EditBarController.Instance.GetMenuItems().Count == 0)
            {
                return false;
            }

            RegisterClientResources();

            RegisterEditBarResources();

            return true;
        }

        protected void OnPreRender()
        {
            this.RegisterInitScripts();
        }

        private static void RegisterClientResources()
        {
            DotNetNuke.Web.Client.ClientResourceManagement.ClientResourceManager.EnableAsyncPostBackHandler();

            // register drop down list required resources
            var controller = GetClientResourcesController();
            controller.RegisterStylesheet("~/Resources/Shared/components/DropDownList/dnn.DropDownList.css", FileOrder.Css.ResourceCss);

            controller.RegisterStylesheet("~/Resources/Shared/scripts/jquery/dnn.jScrollBar.css", FileOrder.Css.ResourceCss);

            controller.RegisterScript("~/Resources/Shared/scripts/dnn.extensions.js");
            controller.RegisterScript("~/Resources/Shared/scripts/dnn.jquery.extensions.js");
            controller.RegisterScript("~/Resources/Shared/scripts/dnn.DataStructures.js");
            controller.RegisterScript("~/Resources/Shared/scripts/jquery/jquery.mousewheel.js");
            controller.RegisterScript("~/Resources/Shared/scripts/jquery/dnn.jScrollBar.js");
            controller.RegisterScript("~/Resources/Shared/scripts/TreeView/dnn.TreeView.js");
            controller.RegisterScript("~/Resources/Shared/scripts/TreeView/dnn.DynamicTreeView.js");
            controller.RegisterScript("~/Resources/Shared/Components/DropDownList/dnn.DropDownList.js");

            controller.RegisterScript(Path.Combine(ControlFolder, "ContentEditorManager/Js/ModuleManager.js"));
            controller.RegisterScript(Path.Combine(ControlFolder, "ContentEditorManager/Js/ModuleDialog.js"));
            controller.RegisterScript(Path.Combine(ControlFolder, "ContentEditorManager/Js/ExistingModuleDialog.js"));
            controller.RegisterScript(Path.Combine(ControlFolder, "ContentEditorManager/Js/ModuleService.js"));
            controller.RegisterScript(Path.Combine(ControlFolder, "ContentEditorManager/Js/ContentEditor.js"));
            controller.CreateStylesheet(Path.Combine(ControlFolder, "ContentEditorManager/Styles/ContentEditor.css")).SetPriority(CssFileOrder).Register();
            ServicesFramework.Instance.RequestAjaxScriptSupport();

            JavaScript.RequestRegistration(CommonJs.DnnPlugins);

            // We need to add the Dnn JQuery plugins because the Edit Bar removes the Control Panel from the page
            JavaScript.RequestRegistration(CommonJs.KnockoutMapping);

            controller.RegisterScript("~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js");
            controller.RegisterStylesheet("~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css");
        }

        private static bool IsPageEditor()
        {
            return HasTabPermission("EDIT");
        }

        private static List<List<string>> GetPaneClientIdCollection()
        {
            var panelClientIds = new List<List<string>>(PortalSettings.ActiveTab.Panes.Count);

            try
            {
                // var skinControl = this.Page.FindControl("SkinPlaceHolder").Controls[0];
                foreach (var pane in PortalSettings.ActiveTab.Panes.Cast<string>())
                {
                    var foundControls = new List<Control>();

                    // FindControlRecursive(skinControl, pane, foundControls);
                    panelClientIds.Add((from control in foundControls select control.ClientID).ToList());
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            return panelClientIds;
        }

        private static string GetPanesClientIds(IEnumerable<IEnumerable<string>> panelCliendIdCollection)
        {
            return string.Join(";", panelCliendIdCollection.Select(x => string.Join(",", x)));
        }

        private static void RegisterLocalResources()
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
                Localization.GetSafeJSString("Category_All.Text", LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_clearButtonTooltip.Text", LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_loadingResultText.Text", LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_resultsText.Text", LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_searchButtonTooltip.Text", LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_searchInputPlaceHolder.Text", LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_selectedItemCollapseTooltip.Text", LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_selectedItemExpandTooltip.Text", LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_selectItemDefaultText.Text", LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_sortAscendingButtonTitle.Text", LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_sortAscendingButtonTooltip.Text", LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_sortDescendingButtonTooltip.Text", LocalResourcesFile),
                Localization.GetSafeJSString("pagePicker_unsortedOrderButtonTooltip.Text", LocalResourcesFile),
                Localization.GetSafeJSString("Site.Text", LocalResourcesFile),
                Localization.GetSafeJSString("Page.Text", LocalResourcesFile),
                Localization.GetSafeJSString("AddExistingModule.Text", LocalResourcesFile),
                Localization.GetSafeJSString("MakeCopy.Text", LocalResourcesFile));

            MvcClientAPI.RegisterStartupScript("ContentEditorManagerResources", script);
        }

        private static bool IsAdmin()
        {
            var user = PortalSettings.UserInfo;
            return user.IsSuperUser || PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName);
        }

        private static IClientResourceController GetClientResourcesController()
        {
            var serviceProvider = DotNetNuke.Common.Globals.GetCurrentServiceProvider();
            return serviceProvider.GetRequiredService<IClientResourceController>();
        }

        private static void RegisterEditBarResources()
        {
            JavaScript.RequestRegistration(CommonJs.jQuery);
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            MvcClientAPI.RegisterClientVariable("editbar_isAdmin", IsAdmin().ToString(), true);

            var settings = EditBarController.Instance.GetConfigurations(PortalSettings.PortalId);
            var settingsScript = "window.editBarSettings = " + JsonConvert.SerializeObject(settings) + ";";

            // this.Page.ClientScript.RegisterClientScriptBlock(this.Page.GetType(), "EditBarSettings", settingsScript, true);
            MvcClientAPI.RegisterStartupScript("EditBarSettings", settingsScript);

            var controller = GetClientResourcesController();
            controller.RegisterScript("~/DesktopModules/admin/Dnn.EditBar/scripts/editBarContainer.js");
            controller.RegisterStylesheet("~/DesktopModules/admin/Dnn.EditBar/css/editBarContainer.css");
        }

        private void RegisterInitScripts()
        {
            RegisterLocalResources();

            MvcClientAPI.RegisterClientVariable("cem_loginurl", Globals.LoginURL(HttpContext.Current.Request.RawUrl, false), true);
            var panes = string.Join(",", PortalSettings.ActiveTab.Panes.Cast<string>());
            var panesClientIds = GetPanesClientIds(GetPaneClientIdCollection());
            string script = $@"dnn.ContentEditorManager.init({{type: 'moduleManager', panes: dnn.panes.join(','), panesClientIds: dnn.panesClientIds.join(';'), supportAjax: {(this.SupportAjax ? "true" : "false")}}});";
            MvcClientAPI.RegisterStartupScript("ContentEditorManager", script);
        }
    }
}
