// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Modules
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.UI.Utilities;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>
    /// Provides context data for a particular instance of a module.
    /// </summary>
    public class ModuleInstanceContext
    {
        private readonly IModuleControl _moduleControl;
        private ModuleActionCollection _actions;
        private ModuleAction _moduleSpecificActions;
        private ModuleAction _moduleGenericActions;
        private ModuleAction _moduleMoveActions;
        private ModuleInfo _configuration;
        private bool? _isEditable;
        private int _nextActionId = -1;
        private Hashtable _settings;

        public ModuleInstanceContext()
        {
        }

        public ModuleInstanceContext(IModuleControl moduleControl)
        {
            this._moduleControl = moduleControl;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the EditMode property is used to determine whether the user is in the
        /// Administrator role.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool EditMode
        {
            get
            {
                return TabPermissionController.CanAdminPage();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether the module is Editable (in Admin mode).
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool IsEditable
        {
            get
            {
                // Perform tri-state switch check to avoid having to perform a security
                // role lookup on every property access (instead caching the result)
                if (!this._isEditable.HasValue)
                {
                    bool blnPreview = (this.PortalSettings.UserMode == PortalSettings.Mode.View) || this.PortalSettings.IsLocked;
                    if (Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID))
                    {
                        blnPreview = false;
                    }

                    bool blnHasModuleEditPermissions = false;
                    if (this._configuration != null)
                    {
                        blnHasModuleEditPermissions = ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "CONTENT", this.Configuration);
                    }

                    if (blnPreview == false && blnHasModuleEditPermissions)
                    {
                        this._isEditable = true;
                    }
                    else
                    {
                        this._isEditable = false;
                    }
                }

                return this._isEditable.Value;
            }
        }

        public bool IsHostMenu
        {
            get
            {
                return Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID);
            }
        }

        public PortalAliasInfo PortalAlias
        {
            get
            {
                return this.PortalSettings.PortalAlias;
            }
        }

        public int PortalId
        {
            get
            {
                return this.PortalSettings.PortalId;
            }
        }

        public PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the settings for this context.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public Hashtable Settings
        {
            get
            {
                if (this._settings == null)
                {
                    this._settings = new ModuleController().GetModuleSettings(this.ModuleId, this.TabId);

                    // add the TabModuleSettings to the ModuleSettings
                    Hashtable tabModuleSettings = new ModuleController().GetTabModuleSettings(this.TabModuleId, this.TabId);
                    foreach (string strKey in tabModuleSettings.Keys)
                    {
                        this._settings[strKey] = tabModuleSettings[strKey];
                    }
                }

                return this._settings;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the tab ID for this context.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int TabId
        {
            get
            {
                if (this._configuration != null)
                {
                    return Convert.ToInt32(this._configuration.TabID);
                }

                return Null.NullInteger;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Actions for this module context.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public ModuleActionCollection Actions
        {
            get
            {
                if (this._actions == null)
                {
                    this.LoadActions(HttpContext.Current.Request);
                }

                return this._actions;
            }

            set
            {
                this._actions = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Module Configuration (ModuleInfo) for this context.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public ModuleInfo Configuration
        {
            get
            {
                return this._configuration;
            }

            set
            {
                this._configuration = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the HelpUrl for this context.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string HelpURL { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the module ID for this context.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int ModuleId
        {
            get
            {
                if (this._configuration != null)
                {
                    return this._configuration.ModuleID;
                }

                return Null.NullInteger;
            }

            set
            {
                if (this._configuration != null)
                {
                    this._configuration.ModuleID = value;
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the tabnmodule ID for this context.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int TabModuleId
        {
            get
            {
                if (this._configuration != null)
                {
                    return Convert.ToInt32(this._configuration.TabModuleID);
                }

                return Null.NullInteger;
            }

            set
            {
                if (this._configuration != null)
                {
                    this._configuration.TabModuleID = value;
                }
            }
        }

        public string EditUrl()
        {
            return this.EditUrl(string.Empty, string.Empty, "Edit");
        }

        public string EditUrl(string controlKey)
        {
            return this.EditUrl(string.Empty, string.Empty, controlKey);
        }

        public string EditUrl(string keyName, string keyValue)
        {
            return this.EditUrl(keyName, keyValue, "Edit");
        }

        public string EditUrl(string keyName, string keyValue, string controlKey)
        {
            var parameters = new string[] { };
            return this.EditUrl(keyName, keyValue, controlKey, parameters);
        }

        public string EditUrl(string keyName, string keyValue, string controlKey, params string[] additionalParameters)
        {
            string key = controlKey;
            if (string.IsNullOrEmpty(key))
            {
                key = "Edit";
            }

            string moduleIdParam = string.Empty;
            if (this.Configuration != null)
            {
                moduleIdParam = string.Format("mid={0}", this.Configuration.ModuleID);
            }

            string[] parameters;
            if (!string.IsNullOrEmpty(keyName) && !string.IsNullOrEmpty(keyValue))
            {
                parameters = new string[2 + additionalParameters.Length];
                parameters[0] = moduleIdParam;
                parameters[1] = string.Format("{0}={1}", keyName, keyValue);
                Array.Copy(additionalParameters, 0, parameters, 2, additionalParameters.Length);
            }
            else
            {
                parameters = new string[1 + additionalParameters.Length];
                parameters[0] = moduleIdParam;
                Array.Copy(additionalParameters, 0, parameters, 1, additionalParameters.Length);
            }

            return this.NavigateUrl(this.PortalSettings.ActiveTab.TabID, key, false, parameters);
        }

        public string NavigateUrl(int tabID, string controlKey, bool pageRedirect, params string[] additionalParameters)
        {
            return this.NavigateUrl(tabID, controlKey, Globals.glbDefaultPage, pageRedirect, additionalParameters);
        }

        public string NavigateUrl(int tabID, string controlKey, string pageName, bool pageRedirect, params string[] additionalParameters)
        {
            var isSuperTab = TestableGlobals.Instance.IsHostTab(tabID);
            var settings = PortalController.Instance.GetCurrentPortalSettings();
            var language = Globals.GetCultureCode(tabID, isSuperTab, settings);
            var url = TestableGlobals.Instance.NavigateURL(tabID, isSuperTab, settings, controlKey, language, pageName, additionalParameters);

            // Making URLs call popups
            if (this.PortalSettings != null && this.PortalSettings.EnablePopUps)
            {
                if (!UIUtilities.IsLegacyUI(this.ModuleId, controlKey, this.PortalId) && url.Contains("ctl"))
                {
                    url = UrlUtils.PopUpUrl(url, null, this.PortalSettings, false, pageRedirect);
                }
            }

            return url;
        }

        public int GetNextActionID()
        {
            this._nextActionId += 1;
            return this._nextActionId;
        }

        private static string FilterUrl(HttpRequest request)
        {
            return request.RawUrl.Replace("\"", string.Empty);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetActionsCount gets the current number of actions.
        /// </summary>
        /// <param name="actions">The actions collection to count.</param>
        /// <param name="count">The current count.</param>
        /// -----------------------------------------------------------------------------
        private static int GetActionsCount(int count, ModuleActionCollection actions)
        {
            foreach (ModuleAction action in actions)
            {
                if (action.HasChildren())
                {
                    count += action.Actions.Count;

                    // Recursively call to see if this collection has any child actions that would affect the count
                    count = GetActionsCount(count, action.Actions);
                }
            }

            return count;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddHelpActions Adds the Help actions to the Action Menu.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void AddHelpActions()
        {
            var url = string.Empty;
            var showInNewWindow = false;
            if (!string.IsNullOrEmpty(this.Configuration.ModuleControl.HelpURL) && Host.EnableModuleOnLineHelp && this.PortalSettings.EnablePopUps)
            {
                var supportInPopup = this.SupportShowInPopup(this.Configuration.ModuleControl.HelpURL);
                if (supportInPopup)
                {
                    url = UrlUtils.PopUpUrl(this.Configuration.ModuleControl.HelpURL, this.PortalSettings, false, false, 550, 950);
                }
                else
                {
                    url = this.Configuration.ModuleControl.HelpURL;
                    showInNewWindow = true;
                }
            }
            else
            {
                url = this.NavigateUrl(this.TabId, "Help", false, "ctlid=" + this.Configuration.ModuleControlId, "moduleid=" + this.ModuleId);
            }

            var helpAction = new ModuleAction(this.GetNextActionID())
            {
                Title = Localization.GetString(ModuleActionType.ModuleHelp, Localization.GlobalResourceFile),
                CommandName = ModuleActionType.ModuleHelp,
                CommandArgument = string.Empty,
                Icon = "action_help.gif",
                Url = url,
                Secure = SecurityAccessLevel.Edit,
                Visible = true,
                NewWindow = showInNewWindow,
                UseActionEvent = true,
            };
            this._moduleGenericActions.Actions.Add(helpAction);
        }

        private void AddPrintAction()
        {
            var action = new ModuleAction(this.GetNextActionID())
            {
                Title = Localization.GetString(ModuleActionType.PrintModule, Localization.GlobalResourceFile),
                CommandName = ModuleActionType.PrintModule,
                CommandArgument = string.Empty,
                Icon = "action_print.gif",
                Url = this.NavigateUrl(
                                     this.TabId,
                                     string.Empty,
                                     false,
                                     "mid=" + this.ModuleId,
                                     "SkinSrc=" + Globals.QueryStringEncode("[G]" + SkinController.RootSkin + "/" + Globals.glbHostSkinFolder + "/" + "No Skin"),
                                     "ContainerSrc=" + Globals.QueryStringEncode("[G]" + SkinController.RootContainer + "/" + Globals.glbHostSkinFolder + "/" + "No Container"),
                                     "dnnprintmode=true"),
                Secure = SecurityAccessLevel.Anonymous,
                UseActionEvent = true,
                Visible = true,
                NewWindow = true,
            };
            this._moduleGenericActions.Actions.Add(action);
        }

        private void AddSyndicateAction()
        {
            var action = new ModuleAction(this.GetNextActionID())
            {
                Title = Localization.GetString(ModuleActionType.SyndicateModule, Localization.GlobalResourceFile),
                CommandName = ModuleActionType.SyndicateModule,
                CommandArgument = string.Empty,
                Icon = "action_rss.gif",
                Url = this.NavigateUrl(this.PortalSettings.ActiveTab.TabID, string.Empty, "RSS.aspx", false, "moduleid=" + this.ModuleId),
                Secure = SecurityAccessLevel.Anonymous,
                UseActionEvent = true,
                Visible = true,
                NewWindow = true,
            };
            this._moduleGenericActions.Actions.Add(action);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddMenuMoveActions Adds the Move actions to the Action Menu.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void AddMenuMoveActions()
        {
            // module movement
            this._moduleMoveActions = new ModuleAction(this.GetNextActionID(), Localization.GetString(ModuleActionType.MoveRoot, Localization.GlobalResourceFile), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, false);

            // move module up/down
            if (this.Configuration != null)
            {
                if ((this.Configuration.ModuleOrder != 0) && (this.Configuration.PaneModuleIndex > 0))
                {
                    this._moduleMoveActions.Actions.Add(
                        this.GetNextActionID(),
                        Localization.GetString(ModuleActionType.MoveTop, Localization.GlobalResourceFile),
                        ModuleActionType.MoveTop,
                        this.Configuration.PaneName,
                        "action_top.gif",
                        string.Empty,
                        false,
                        SecurityAccessLevel.View,
                        true,
                        false);
                    this._moduleMoveActions.Actions.Add(
                        this.GetNextActionID(),
                        Localization.GetString(ModuleActionType.MoveUp, Localization.GlobalResourceFile),
                        ModuleActionType.MoveUp,
                        this.Configuration.PaneName,
                        "action_up.gif",
                        string.Empty,
                        false,
                        SecurityAccessLevel.View,
                        true,
                        false);
                }

                if ((this.Configuration.ModuleOrder != 0) && (this.Configuration.PaneModuleIndex < (this.Configuration.PaneModuleCount - 1)))
                {
                    this._moduleMoveActions.Actions.Add(
                        this.GetNextActionID(),
                        Localization.GetString(ModuleActionType.MoveDown, Localization.GlobalResourceFile),
                        ModuleActionType.MoveDown,
                        this.Configuration.PaneName,
                        "action_down.gif",
                        string.Empty,
                        false,
                        SecurityAccessLevel.View,
                        true,
                        false);
                    this._moduleMoveActions.Actions.Add(
                        this.GetNextActionID(),
                        Localization.GetString(ModuleActionType.MoveBottom, Localization.GlobalResourceFile),
                        ModuleActionType.MoveBottom,
                        this.Configuration.PaneName,
                        "action_bottom.gif",
                        string.Empty,
                        false,
                        SecurityAccessLevel.View,
                        true,
                        false);
                }
            }

            // move module to pane
            foreach (object obj in this.PortalSettings.ActiveTab.Panes)
            {
                var pane = obj as string;
                if (!string.IsNullOrEmpty(pane) && this.Configuration != null && !this.Configuration.PaneName.Equals(pane, StringComparison.InvariantCultureIgnoreCase))
                {
                    this._moduleMoveActions.Actions.Add(
                        this.GetNextActionID(),
                        Localization.GetString(ModuleActionType.MovePane, Localization.GlobalResourceFile) + " " + pane,
                        ModuleActionType.MovePane,
                        pane,
                        "action_move.gif",
                        string.Empty,
                        false,
                        SecurityAccessLevel.View,
                        true,
                        false);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadActions loads the Actions collections.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void LoadActions(HttpRequest request)
        {
            this._actions = new ModuleActionCollection();
            if (this.PortalSettings.IsLocked)
            {
                return;
            }

            this._moduleGenericActions = new ModuleAction(this.GetNextActionID(), Localization.GetString("ModuleGenericActions.Action", Localization.GlobalResourceFile), string.Empty, string.Empty, string.Empty);
            int maxActionId = Null.NullInteger;

            // check if module Implements Entities.Modules.IActionable interface
            var actionable = this._moduleControl as IActionable;
            if (actionable != null)
            {
                this._moduleSpecificActions = new ModuleAction(this.GetNextActionID(), Localization.GetString("ModuleSpecificActions.Action", Localization.GlobalResourceFile), string.Empty, string.Empty, string.Empty);

                ModuleActionCollection moduleActions = actionable.ModuleActions;

                foreach (ModuleAction action in moduleActions)
                {
                    if (ModulePermissionController.HasModuleAccess(action.Secure, "CONTENT", this.Configuration))
                    {
                        if (string.IsNullOrEmpty(action.Icon))
                        {
                            action.Icon = "edit.gif";
                        }

                        if (action.ID > maxActionId)
                        {
                            maxActionId = action.ID;
                        }

                        this._moduleSpecificActions.Actions.Add(action);

                        if (!UIUtilities.IsLegacyUI(this.ModuleId, action.ControlKey, this.PortalId) && action.Url.Contains("ctl"))
                        {
                            action.ClientScript = UrlUtils.PopUpUrl(action.Url, this._moduleControl as Control, this.PortalSettings, true, false);
                        }
                    }
                }

                if (this._moduleSpecificActions.Actions.Count > 0)
                {
                    this._actions.Add(this._moduleSpecificActions);
                }
            }

            // Make sure the Next Action Id counter is correct
            int actionCount = GetActionsCount(this._actions.Count, this._actions);
            if (this._nextActionId < maxActionId)
            {
                this._nextActionId = maxActionId;
            }

            if (this._nextActionId < actionCount)
            {
                this._nextActionId = actionCount;
            }

            // Custom injection of Module Settings when shared as ViewOnly
            if (this.Configuration != null && (this.Configuration.IsShared && this.Configuration.IsShareableViewOnly)
                    && TabPermissionController.CanAddContentToPage())
            {
                this._moduleGenericActions.Actions.Add(
                    this.GetNextActionID(),
                    Localization.GetString("ModulePermissions.Action", Localization.GlobalResourceFile),
                    "ModulePermissions",
                    string.Empty,
                    "action_settings.gif",
                    this.NavigateUrl(this.TabId, "ModulePermissions", false, "ModuleId=" + this.ModuleId, "ReturnURL=" + FilterUrl(request)),
                    false,
                    SecurityAccessLevel.ViewPermissions,
                    true,
                    false);
            }
            else
            {
                if (!Globals.IsAdminControl() && ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Admin, "DELETE,MANAGE", this.Configuration))
                {
                    if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Admin, "MANAGE", this.Configuration))
                    {
                        this._moduleGenericActions.Actions.Add(
                            this.GetNextActionID(),
                            Localization.GetString(ModuleActionType.ModuleSettings, Localization.GlobalResourceFile),
                            ModuleActionType.ModuleSettings,
                            string.Empty,
                            "action_settings.gif",
                            this.NavigateUrl(this.TabId, "Module", false, "ModuleId=" + this.ModuleId, "ReturnURL=" + FilterUrl(request)),
                            false,
                            SecurityAccessLevel.Edit,
                            true,
                            false);
                    }
                }
            }

            if (!string.IsNullOrEmpty(this.Configuration.DesktopModule.BusinessControllerClass))
            {
                // check if module implements IPortable interface, and user has Admin permissions
                if (this.Configuration.DesktopModule.IsPortable)
                {
                    if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Admin, "EXPORT", this.Configuration))
                    {
                        this._moduleGenericActions.Actions.Add(
                            this.GetNextActionID(),
                            Localization.GetString(ModuleActionType.ExportModule, Localization.GlobalResourceFile),
                            ModuleActionType.ExportModule,
                            string.Empty,
                            "action_export.gif",
                            this.NavigateUrl(this.PortalSettings.ActiveTab.TabID, "ExportModule", false, "moduleid=" + this.ModuleId, "ReturnURL=" + FilterUrl(request)),

                            string.Empty,
                            false,
                            SecurityAccessLevel.View,
                            true,
                            false);
                    }

                    if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Admin, "IMPORT", this.Configuration))
                    {
                        this._moduleGenericActions.Actions.Add(
                            this.GetNextActionID(),
                            Localization.GetString(ModuleActionType.ImportModule, Localization.GlobalResourceFile),
                            ModuleActionType.ImportModule,
                            string.Empty,
                            "action_import.gif",
                            this.NavigateUrl(this.PortalSettings.ActiveTab.TabID, "ImportModule", false, "moduleid=" + this.ModuleId, "ReturnURL=" + FilterUrl(request)),
                            string.Empty,
                            false,
                            SecurityAccessLevel.View,
                            true,
                            false);
                    }
                }

                if (this.Configuration.DesktopModule.IsSearchable && this.Configuration.DisplaySyndicate)
                {
                    this.AddSyndicateAction();
                }
            }

            // help module actions available to content editors and administrators
            const string permisisonList = "CONTENT,DELETE,EDIT,EXPORT,IMPORT,MANAGE";
            if (ModulePermissionController.HasModulePermission(this.Configuration.ModulePermissions, permisisonList)
                    && request.QueryString["ctl"] != "Help"
                    && !Globals.IsAdminControl())
            {
                this.AddHelpActions();
            }

            // Add Print Action
            if (this.Configuration.DisplayPrint)
            {
                // print module action available to everyone
                this.AddPrintAction();
            }

            if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Host, "MANAGE", this.Configuration) && !Globals.IsAdminControl())
            {
                this._moduleGenericActions.Actions.Add(
                    this.GetNextActionID(),
                    Localization.GetString(ModuleActionType.ViewSource, Localization.GlobalResourceFile),
                    ModuleActionType.ViewSource,
                    string.Empty,
                    "action_source.gif",
                    this.NavigateUrl(this.TabId, "ViewSource", false, "ModuleId=" + this.ModuleId, "ctlid=" + this.Configuration.ModuleControlId, "ReturnURL=" + FilterUrl(request)),
                    false,
                    SecurityAccessLevel.Host,
                    true,
                    false);
            }

            if (!Globals.IsAdminControl() && ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Admin, "DELETE,MANAGE", this.Configuration))
            {
                if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Admin, "DELETE", this.Configuration))
                {
                    // Check if this is the owner instance of a shared module.
                    string confirmText = "confirm('" + ClientAPI.GetSafeJSString(Localization.GetString("DeleteModule.Confirm")) + "')";
                    if (!this.Configuration.IsShared)
                    {
                        var portal = PortalController.Instance.GetPortal(this.PortalSettings.PortalId);
                        if (PortalGroupController.Instance.IsModuleShared(this.Configuration.ModuleID, portal))
                        {
                            confirmText = "confirm('" + ClientAPI.GetSafeJSString(Localization.GetString("DeleteSharedModule.Confirm")) + "')";
                        }
                    }

                    this._moduleGenericActions.Actions.Add(
                        this.GetNextActionID(),
                        Localization.GetString(ModuleActionType.DeleteModule, Localization.GlobalResourceFile),
                        ModuleActionType.DeleteModule,
                        this.Configuration.ModuleID.ToString(),
                        "action_delete.gif",
                        string.Empty,
                        confirmText,
                        false,
                        SecurityAccessLevel.View,
                        true,
                        false);
                }

                if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Admin, "MANAGE", this.Configuration))
                {
                    this._moduleGenericActions.Actions.Add(
                        this.GetNextActionID(),
                        Localization.GetString(ModuleActionType.ClearCache, Localization.GlobalResourceFile),
                        ModuleActionType.ClearCache,
                        this.Configuration.ModuleID.ToString(),
                        "action_refresh.gif",
                        string.Empty,
                        false,
                        SecurityAccessLevel.View,
                        true,
                        false);
                }

                if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Admin, "MANAGE", this.Configuration))
                {
                    // module movement
                    this.AddMenuMoveActions();
                }
            }

            if (this._moduleGenericActions.Actions.Count > 0)
            {
                this._actions.Add(this._moduleGenericActions);
            }

            if (this._moduleMoveActions != null && this._moduleMoveActions.Actions.Count > 0)
            {
                this._actions.Add(this._moduleMoveActions);
            }

            foreach (ModuleAction action in this._moduleGenericActions.Actions)
            {
                if (!UIUtilities.IsLegacyUI(this.ModuleId, action.ControlKey, this.PortalId) && action.Url.Contains("ctl"))
                {
                    action.ClientScript = UrlUtils.PopUpUrl(action.Url, this._moduleControl as Control, this.PortalSettings, true, false);
                }
            }
        }

        private bool SupportShowInPopup(string url)
        {
            if (HttpContext.Current == null || !url.Contains("://"))
            {
                return true;
            }

            var isSecureConnection = UrlUtils.IsSecureConnectionOrSslOffload(HttpContext.Current.Request);
            return (isSecureConnection && url.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                   || (!isSecureConnection && url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
