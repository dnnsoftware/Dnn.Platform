// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using Dnn.EditBar.UI.Controllers;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
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

        private readonly IClientResourceController clientResourceController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;
        private readonly IPortalController portalController;
        private readonly IHostSettings hostSettings;
        private readonly IUserController userController;
        private readonly IHostSettingsService hostSettingsService;
        private readonly IServicesFramework servicesFramework;

        private bool supportAjax = true;

        /// <summary>Initializes a new instance of the <see cref="MvcContentEditorManager"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        public MvcContentEditorManager(IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger, IPortalController portalController, IHostSettings hostSettings, IUserController userController, IHostSettingsService hostSettingsService, IServicesFramework servicesFramework)
        {
            this.clientResourceController = clientResourceController;
            this.appStatus = appStatus;
            this.eventLogger = eventLogger;
            this.portalController = portalController;
            this.hostSettings = hostSettings;
            this.userController = userController;
            this.hostSettingsService = hostSettingsService;
            this.servicesFramework = servicesFramework;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public PortalSettings PortalSettings => PortalSettings.Current;

        public bool IsHostMenu
        {
            get
            {
                return Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID);
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

        public static void CreateManager(Controller controller, IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger, IPortalController portalController, IHostSettings hostSettings, IUserController userController, IHostSettingsService hostSettingsService, IServicesFramework servicesFramework)
        {
            if (hostSettings.DisableEditBar)
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
                    var manager = new MvcContentEditorManager(clientResourceController, appStatus, eventLogger, portalController, hostSettings, userController, hostSettingsService, servicesFramework);
                    manager.Context = controller.ControllerContext;
                    if (manager.OnInit())
                    {
                        manager.OnPreRender();
                    }
                }
            }
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
                CultureInfo.InvariantCulture,
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

        private bool HasTabPermission(string permissionKey)
        {
            var principal = Thread.CurrentPrincipal;
            if (!principal.Identity.IsAuthenticated)
            {
                return false;
            }

            bool isAdminUser = this.PortalSettings.UserInfo.IsSuperUser || PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName);
            if (isAdminUser)
            {
                return true;
            }

            return TabPermissionController.HasTabPermission(permissionKey);
        }

        private bool OnInit()
        {
            var user = this.PortalSettings.UserInfo;

            if (user.UserID > 0)
            {
                MvcClientAPI.RegisterClientVariable("dnn_current_userid", this.PortalSettings.UserInfo.UserID.ToString(CultureInfo.InvariantCulture), true);
            }

            if (Personalization.GetUserMode() != PortalSettings.Mode.Edit
                    || !this.IsPageEditor()
                    || Controllers.EditBarController.Instance.GetMenuItems().Count == 0)
            {
                return false;
            }

            this.RegisterClientResources();

            this.RegisterEditBarResources();

            return true;
        }

        private void OnPreRender()
        {
            this.RegisterInitScripts();
        }

        private void RegisterClientResources()
        {
            // register drop down list required resources
            this.clientResourceController.RegisterStylesheet("~/Resources/Shared/components/DropDownList/dnn.DropDownList.css", FileOrder.Css.ResourceCss);

            this.clientResourceController.RegisterStylesheet("~/Resources/Shared/scripts/jquery/dnn.jScrollBar.css", FileOrder.Css.ResourceCss);

            this.clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.extensions.js");
            this.clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.jquery.extensions.js");
            this.clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.DataStructures.js");
            this.clientResourceController.RegisterScript("~/Resources/Shared/scripts/jquery/jquery.mousewheel.js");
            this.clientResourceController.RegisterScript("~/Resources/Shared/scripts/jquery/dnn.jScrollBar.js");
            this.clientResourceController.RegisterScript("~/Resources/Shared/scripts/TreeView/dnn.TreeView.js");
            this.clientResourceController.RegisterScript("~/Resources/Shared/scripts/TreeView/dnn.DynamicTreeView.js");
            this.clientResourceController.RegisterScript("~/Resources/Shared/Components/DropDownList/dnn.DropDownList.js");

            this.clientResourceController.RegisterScript(Path.Combine(ControlFolder, "ContentEditorManager/Js/ModuleManager.js"));
            this.clientResourceController.RegisterScript(Path.Combine(ControlFolder, "ContentEditorManager/Js/ModuleDialog.js"));
            this.clientResourceController.RegisterScript(Path.Combine(ControlFolder, "ContentEditorManager/Js/ExistingModuleDialog.js"));
            this.clientResourceController.RegisterScript(Path.Combine(ControlFolder, "ContentEditorManager/Js/ModuleService.js"));
            this.clientResourceController.RegisterScript(Path.Combine(ControlFolder, "ContentEditorManager/Js/ContentEditor.js"));
            this.clientResourceController.CreateStylesheet(Path.Combine(ControlFolder, "ContentEditorManager/Styles/ContentEditor.css")).SetPriority(CssFileOrder).Register();
            ServicesFramework.Instance.RequestAjaxScriptSupport();

            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, CommonJs.DnnPlugins);

            // We need to add the Dnn JQuery plugins because the Edit Bar removes the Control Panel from the page
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, CommonJs.KnockoutMapping);

            this.clientResourceController.RegisterScript("~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js");
            this.clientResourceController.RegisterStylesheet("~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css");
        }

        private bool IsPageEditor()
        {
            return this.HasTabPermission("EDIT");
        }

        private List<List<string>> GetPaneClientIdCollection()
        {
            var panelClientIds = new List<List<string>>(this.PortalSettings.ActiveTab.Panes.Count);

            try
            {
                // var skinControl = this.Page.FindControl("SkinPlaceHolder").Controls[0];
                foreach (var pane in this.PortalSettings.ActiveTab.Panes.Cast<string>())
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

        private bool IsAdmin()
        {
            var user = this.PortalSettings.UserInfo;
            return user.IsSuperUser || PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName);
        }

        private void RegisterEditBarResources()
        {
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, CommonJs.jQuery);
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            MvcClientAPI.RegisterClientVariable("editbar_isAdmin", this.IsAdmin().ToString(), true);

            var settings = EditBarController.Instance.GetConfigurations(this.PortalSettings.PortalId);
            var settingsScript = "window.editBarSettings = " + JsonConvert.SerializeObject(settings) + ";";

            // this.Page.ClientScript.RegisterClientScriptBlock(this.Page.GetType(), "EditBarSettings", settingsScript, true);
            MvcClientAPI.RegisterStartupScript("EditBarSettings", settingsScript);

            this.clientResourceController.RegisterScript("~/DesktopModules/admin/Dnn.EditBar/scripts/editBarContainer.js");
            this.clientResourceController.RegisterStylesheet("~/DesktopModules/admin/Dnn.EditBar/css/editBarContainer.css");
        }

        private void RegisterInitScripts()
        {
            RegisterLocalResources();

            MvcClientAPI.RegisterClientVariable("cem_loginurl", Globals.LoginURL(HttpContext.Current.Request.RawUrl, false), true);
            var panes = string.Join(",", this.PortalSettings.ActiveTab.Panes.Cast<string>());
            var panesClientIds = GetPanesClientIds(this.GetPaneClientIdCollection());
            string script = $@"dnn.ContentEditorManager.init({{type: 'moduleManager', panes: dnn.panes.join(','), panesClientIds: dnn.panesClientIds.join(';'), supportAjax: {(this.SupportAjax ? "true" : "false")}}});";
            MvcClientAPI.RegisterStartupScript("ContentEditorManager", script);
        }
    }
}
