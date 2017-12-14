#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
#region Usings

using System;
using System.Collections;
using System.Linq;
using System.Web;

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
using System.Web.UI;

#endregion

namespace DotNetNuke.UI.Modules
{
    using DotNetNuke.Common.Internal;

    /// <summary>
    /// Provides context data for a particular instance of a module
    /// </summary>
    public class ModuleInstanceContext
    {
        #region Private Members

        private readonly IModuleControl _moduleControl;
        private ModuleActionCollection _actions;
        private ModuleAction _moduleSpecificActions;
        private ModuleAction _moduleGenericActions;
        private ModuleAction _moduleMoveActions;
        private ModuleInfo _configuration;
        private bool? _isEditable;
        private int _nextActionId = -1;
        private Hashtable _settings;

        #endregion

        #region Constructors

        public ModuleInstanceContext()
        {
        }

        public ModuleInstanceContext(IModuleControl moduleControl)
        {
            _moduleControl = moduleControl;
        }

        #endregion

        #region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Actions for this module context
        /// </summary>
        /// -----------------------------------------------------------------------------
        public ModuleActionCollection Actions
        {
            get
            {
                if (_actions == null)
                {
                    LoadActions(HttpContext.Current.Request);
                }
                return _actions;
            }
            set
            {
                _actions = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Module Configuration (ModuleInfo) for this context
        /// </summary>
        /// -----------------------------------------------------------------------------
        public ModuleInfo Configuration
        {
            get
            {
                return _configuration;
            }
            set
            {
                _configuration = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The EditMode property is used to determine whether the user is in the
        /// Administrator role
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
        /// Gets and sets the HelpUrl for this context
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string HelpURL { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the module is Editable (in Admin mode)
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool IsEditable
        {
            get
            {
                //Perform tri-state switch check to avoid having to perform a security
                //role lookup on every property access (instead caching the result)
                if (!_isEditable.HasValue)
                {
                    bool blnPreview = (PortalSettings.UserMode == PortalSettings.Mode.View);
                    if (Globals.IsHostTab(PortalSettings.ActiveTab.TabID))
                    {
                        blnPreview = false;
                    }
                    bool blnHasModuleEditPermissions = false;
                    if (_configuration != null)
                    {
                        blnHasModuleEditPermissions = ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "CONTENT", Configuration);
                    }
                    if (blnPreview == false && blnHasModuleEditPermissions)
                    {
                        _isEditable = true;
                    }
                    else
                    {
                        _isEditable = false;
                    }
                }
                return _isEditable.Value;
            }
        }

        public bool IsHostMenu
        {
            get
            {
                return Globals.IsHostTab(PortalSettings.ActiveTab.TabID);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the module ID for this context
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int ModuleId
        {
            get
            {
                if (_configuration != null)
                {
                    return _configuration.ModuleID;
                }

                return Null.NullInteger;
            }
            set
            {
                if (_configuration != null)
                {
                    _configuration.ModuleID = value;
                }
            }
        }

        public PortalAliasInfo PortalAlias
        {
            get
            {
                return PortalSettings.PortalAlias;
            }
        }

        public int PortalId
        {
            get
            {
                return PortalSettings.PortalId;
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
        /// Gets the settings for this context
        /// </summary>
        /// -----------------------------------------------------------------------------
        public Hashtable Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new ModuleController().GetModuleSettings(ModuleId, TabId);

                    //add the TabModuleSettings to the ModuleSettings
                    Hashtable tabModuleSettings = new ModuleController().GetTabModuleSettings(TabModuleId, TabId);
                    foreach (string strKey in tabModuleSettings.Keys)
                    {
                        _settings[strKey] = tabModuleSettings[strKey];
                    }
                }
                return _settings;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the tab ID for this context
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int TabId
        {
            get
            {
                if (_configuration != null)
                {
                    return Convert.ToInt32(_configuration.TabID);
                }

                return Null.NullInteger;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the tabnmodule ID for this context
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int TabModuleId
        {
            get
            {
                if (_configuration != null)
                {
                    return Convert.ToInt32(_configuration.TabModuleID);
                }

                return Null.NullInteger;
            }
            set
            {
                if (_configuration != null)
                {
                    _configuration.TabModuleID = value;
                }
            }
        }

        #endregion

        #region Private Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddHelpActions Adds the Help actions to the Action Menu
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void AddHelpActions()
        {
            var url = string.Empty;
            var showInNewWindow = false;
            if (!string.IsNullOrEmpty(Configuration.ModuleControl.HelpURL) && Host.EnableModuleOnLineHelp && PortalSettings.EnablePopUps)
            {
                var supportInPopup = SupportShowInPopup(Configuration.ModuleControl.HelpURL);
                if (supportInPopup)
                {
                    url = UrlUtils.PopUpUrl(Configuration.ModuleControl.HelpURL, PortalSettings, false, false, 550, 950);
                }
                else
                {
                    url = Configuration.ModuleControl.HelpURL;
                    showInNewWindow = true;
                }
            }
            else
            {
                url = NavigateUrl(TabId, "Help", false, "ctlid=" + Configuration.ModuleControlId, "moduleid=" + ModuleId);
            }

            var helpAction = new ModuleAction(GetNextActionID())
                                 {
                                     Title = Localization.GetString(ModuleActionType.ModuleHelp, Localization.GlobalResourceFile),
                                     CommandName = ModuleActionType.ModuleHelp,
                                     CommandArgument = "",
                                     Icon = "action_help.gif",
                                     Url = url,
                                     Secure = SecurityAccessLevel.Edit,
                                     Visible = true,
                                     NewWindow = showInNewWindow,
                                     UseActionEvent = true
                                 };
            _moduleGenericActions.Actions.Add(helpAction);
        }

        private void AddPrintAction()
        {
            var action = new ModuleAction(GetNextActionID())
                             {
                                 Title = Localization.GetString(ModuleActionType.PrintModule, Localization.GlobalResourceFile),
                                 CommandName = ModuleActionType.PrintModule,
                                 CommandArgument = "",
                                 Icon = "action_print.gif",
                                 Url = NavigateUrl(TabId,
                                                 "",
                                                 false,
                                                 "mid=" + ModuleId,
                                                 "SkinSrc=" + Globals.QueryStringEncode("[G]" + SkinController.RootSkin + "/" + Globals.glbHostSkinFolder + "/" + "No Skin"),
                                                 "ContainerSrc=" + Globals.QueryStringEncode("[G]" + SkinController.RootContainer + "/" + Globals.glbHostSkinFolder + "/" + "No Container"),
                                                 "dnnprintmode=true"),
                                 Secure = SecurityAccessLevel.Anonymous,
                                 UseActionEvent = true,
                                 Visible = true,
                                 NewWindow = true
                             };
            _moduleGenericActions.Actions.Add(action);
        }

        private void AddSyndicateAction()
        {
            var action = new ModuleAction(GetNextActionID())
                             {
                                 Title = Localization.GetString(ModuleActionType.SyndicateModule, Localization.GlobalResourceFile),
                                 CommandName = ModuleActionType.SyndicateModule,
                                 CommandArgument = "",
                                 Icon = "action_rss.gif",
                                 Url = NavigateUrl(PortalSettings.ActiveTab.TabID, "", "RSS.aspx", false, "moduleid=" + ModuleId),
                                 Secure = SecurityAccessLevel.Anonymous,
                                 UseActionEvent = true,
                                 Visible = true,
                                 NewWindow = true
                             };
            _moduleGenericActions.Actions.Add(action);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddMenuMoveActions Adds the Move actions to the Action Menu
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void AddMenuMoveActions()
        {
            //module movement
            _moduleMoveActions = new ModuleAction(GetNextActionID(), Localization.GetString(ModuleActionType.MoveRoot, Localization.GlobalResourceFile), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, false);

            //move module up/down
            if (Configuration != null)
            {
                if ((Configuration.ModuleOrder != 0) && (Configuration.PaneModuleIndex > 0))
                {
                    _moduleMoveActions.Actions.Add(GetNextActionID(),
                                               Localization.GetString(ModuleActionType.MoveTop, Localization.GlobalResourceFile),
                                               ModuleActionType.MoveTop,
                                               Configuration.PaneName,
                                               "action_top.gif",
                                               "",
                                               false,
                                               SecurityAccessLevel.View,
                                               true,
                                               false);
                    _moduleMoveActions.Actions.Add(GetNextActionID(),
                                               Localization.GetString(ModuleActionType.MoveUp, Localization.GlobalResourceFile),
                                               ModuleActionType.MoveUp,
                                               Configuration.PaneName,
                                               "action_up.gif",
                                               "",
                                               false,
                                               SecurityAccessLevel.View,
                                               true,
                                               false);
                }
                if ((Configuration.ModuleOrder != 0) && (Configuration.PaneModuleIndex < (Configuration.PaneModuleCount - 1)))
                {
                    _moduleMoveActions.Actions.Add(GetNextActionID(),
                                               Localization.GetString(ModuleActionType.MoveDown, Localization.GlobalResourceFile),
                                               ModuleActionType.MoveDown,
                                               Configuration.PaneName,
                                               "action_down.gif",
                                               "",
                                               false,
                                               SecurityAccessLevel.View,
                                               true,
                                               false);
                    _moduleMoveActions.Actions.Add(GetNextActionID(),
                                               Localization.GetString(ModuleActionType.MoveBottom, Localization.GlobalResourceFile),
                                               ModuleActionType.MoveBottom,
                                               Configuration.PaneName,
                                               "action_bottom.gif",
                                               "",
                                               false,
                                               SecurityAccessLevel.View,
                                               true,
                                               false);
                }
            }

            //move module to pane
            foreach (object obj in PortalSettings.ActiveTab.Panes)
            {
                var pane = obj as string;
                if (!string.IsNullOrEmpty(pane) && Configuration != null && !Configuration.PaneName.Equals(pane, StringComparison.InvariantCultureIgnoreCase))
                {
                    _moduleMoveActions.Actions.Add(GetNextActionID(),
                                               Localization.GetString(ModuleActionType.MovePane, Localization.GlobalResourceFile) + " " + pane,
                                               ModuleActionType.MovePane,
                                               pane,
                                               "action_move.gif",
                                               "",
                                               false,
                                               SecurityAccessLevel.View,
                                               true,
                                               false);
                }
            }

        }

        private static string FilterUrl(HttpRequest request)
        {
            return request.RawUrl.Replace("\"", "");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetActionsCount gets the current number of actions
        /// </summary>
        /// <param name="actions">The actions collection to count.</param>
        /// <param name="count">The current count</param>
        /// -----------------------------------------------------------------------------
        private static int GetActionsCount(int count, ModuleActionCollection actions)
        {
            foreach (ModuleAction action in actions)
            {
                if (action.HasChildren())
                {
                    count += action.Actions.Count;

                    //Recursively call to see if this collection has any child actions that would affect the count
                    count = GetActionsCount(count, action.Actions);
                }
            }
            return count;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadActions loads the Actions collections
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void LoadActions(HttpRequest request)
        {
            _actions = new ModuleActionCollection();
            _moduleGenericActions = new ModuleAction(GetNextActionID(), Localization.GetString("ModuleGenericActions.Action", Localization.GlobalResourceFile), string.Empty, string.Empty, string.Empty);
            int maxActionId = Null.NullInteger;

            //check if module Implements Entities.Modules.IActionable interface
            var actionable = _moduleControl as IActionable;
            if (actionable != null)
            {
                _moduleSpecificActions = new ModuleAction(GetNextActionID(), Localization.GetString("ModuleSpecificActions.Action", Localization.GlobalResourceFile), string.Empty, string.Empty, string.Empty);

                ModuleActionCollection moduleActions = actionable.ModuleActions;

                foreach (ModuleAction action in moduleActions)
                {
                    if (ModulePermissionController.HasModuleAccess(action.Secure, "CONTENT", Configuration))
                    {
                        if (String.IsNullOrEmpty(action.Icon))
                        {
                            action.Icon = "edit.gif";
                        }
                        if (action.ID > maxActionId)
                        {
                            maxActionId = action.ID;
                        }
                        _moduleSpecificActions.Actions.Add(action);

                        if (!UIUtilities.IsLegacyUI(ModuleId, action.ControlKey, PortalId) && action.Url.Contains("ctl"))
                        {
                            action.ClientScript = UrlUtils.PopUpUrl(action.Url, _moduleControl as Control, PortalSettings, true, false);
                        }
                    }
                }
                if (_moduleSpecificActions.Actions.Count > 0)
                {
                    _actions.Add(_moduleSpecificActions);
                }
            }

            //Make sure the Next Action Id counter is correct
            int actionCount = GetActionsCount(_actions.Count, _actions);
            if (_nextActionId < maxActionId)
            {
                _nextActionId = maxActionId;
            }
            if (_nextActionId < actionCount)
            {
                _nextActionId = actionCount;
            }

            //Custom injection of Module Settings when shared as ViewOnly
            if (Configuration != null && (Configuration.IsShared && Configuration.IsShareableViewOnly)
                    && TabPermissionController.CanAddContentToPage())
            {
                _moduleGenericActions.Actions.Add(GetNextActionID(),
                             Localization.GetString("ModulePermissions.Action", Localization.GlobalResourceFile),
                             "ModulePermissions",
                             "",
                             "action_settings.gif",
                             NavigateUrl(TabId, "ModulePermissions", false, "ModuleId=" + ModuleId, "ReturnURL=" + FilterUrl(request)),
                             false,
                             SecurityAccessLevel.ViewPermissions,
                             true,
                             false);
            }
            else
            {
                if (!Globals.IsAdminControl() && ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Admin, "DELETE,MANAGE", Configuration))
                {
                    if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Admin, "MANAGE", Configuration))
                    {
                        _moduleGenericActions.Actions.Add(GetNextActionID(),
                                                          Localization.GetString(ModuleActionType.ModuleSettings, Localization.GlobalResourceFile),
                                                          ModuleActionType.ModuleSettings,
                                                          "",
                                                          "action_settings.gif",
                                                          NavigateUrl(TabId, "Module", false, "ModuleId=" + ModuleId, "ReturnURL=" + FilterUrl(request)),
                                                          false,
                                                          SecurityAccessLevel.Edit,
                                                          true,
                                                          false);
                    }
                }
            }

            if (!string.IsNullOrEmpty(Configuration.DesktopModule.BusinessControllerClass))
            {
                //check if module implements IPortable interface, and user has Admin permissions
                if (Configuration.DesktopModule.IsPortable)
                {
                    if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Admin, "EXPORT", Configuration))
                    {
                        _moduleGenericActions.Actions.Add(GetNextActionID(),
                                     Localization.GetString(ModuleActionType.ExportModule, Localization.GlobalResourceFile),
                                     ModuleActionType.ExportModule,
                                     "",
                                     "action_export.gif",
                                     NavigateUrl(PortalSettings.ActiveTab.TabID, "ExportModule", false, "moduleid=" + ModuleId, "ReturnURL=" + FilterUrl(request)),

                                     "",
                                     false,
                                     SecurityAccessLevel.View,
                                     true,
                                     false);
                    }
                    if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Admin, "IMPORT", Configuration))
                    {
                        _moduleGenericActions.Actions.Add(GetNextActionID(),
                                     Localization.GetString(ModuleActionType.ImportModule, Localization.GlobalResourceFile),
                                     ModuleActionType.ImportModule,
                                     "",
                                     "action_import.gif",
                                     NavigateUrl(PortalSettings.ActiveTab.TabID, "ImportModule", false, "moduleid=" + ModuleId, "ReturnURL=" + FilterUrl(request)),
                                     "",
                                     false,
                                     SecurityAccessLevel.View,
                                     true,
                                     false);
                    }
                }
                if (Configuration.DesktopModule.IsSearchable && Configuration.DisplaySyndicate)
                {
                    AddSyndicateAction();
                }
            }

            //help module actions available to content editors and administrators
            const string permisisonList = "CONTENT,DELETE,EDIT,EXPORT,IMPORT,MANAGE";
            if (ModulePermissionController.HasModulePermission(Configuration.ModulePermissions, permisisonList) 
                    && request.QueryString["ctl"] != "Help"
                    && !Globals.IsAdminControl())
            {
                AddHelpActions();
            }

            //Add Print Action
            if (Configuration.DisplayPrint)
            {
                //print module action available to everyone
                AddPrintAction();
            }

            if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Host, "MANAGE", Configuration) && !Globals.IsAdminControl())
            {
                _moduleGenericActions.Actions.Add(GetNextActionID(),
                             Localization.GetString(ModuleActionType.ViewSource, Localization.GlobalResourceFile),
                             ModuleActionType.ViewSource,
                             "",
                             "action_source.gif",
                             NavigateUrl(TabId, "ViewSource", false, "ModuleId=" + ModuleId, "ctlid=" + Configuration.ModuleControlId, "ReturnURL=" + FilterUrl(request)),
                             false,
                             SecurityAccessLevel.Host,
                             true,
                             false);
            }



            if (!Globals.IsAdminControl() && ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Admin, "DELETE,MANAGE", Configuration))
            {
                if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Admin, "DELETE", Configuration))
                {
                    //Check if this is the owner instance of a shared module.
                    string confirmText = "confirm('" + ClientAPI.GetSafeJSString(Localization.GetString("DeleteModule.Confirm")) + "')";
                    if (!Configuration.IsShared)
                    {
                        var portal = PortalController.Instance.GetPortal(PortalSettings.PortalId);
                        if (PortalGroupController.Instance.IsModuleShared(Configuration.ModuleID, portal))
                        {
                            confirmText = "confirm('" + ClientAPI.GetSafeJSString(Localization.GetString("DeleteSharedModule.Confirm")) + "')";
                        }
                    }

                    _moduleGenericActions.Actions.Add(GetNextActionID(),
                                 Localization.GetString(ModuleActionType.DeleteModule, Localization.GlobalResourceFile),
                                 ModuleActionType.DeleteModule,
                                 Configuration.ModuleID.ToString(),
                                 "action_delete.gif",
                                 "",
                                 confirmText,
                                 false,
                                 SecurityAccessLevel.View,
                                 true,
                                 false);
                }
                if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Admin, "MANAGE", Configuration))
                {
                    _moduleGenericActions.Actions.Add(GetNextActionID(),
                                 Localization.GetString(ModuleActionType.ClearCache, Localization.GlobalResourceFile),
                                 ModuleActionType.ClearCache,
                                 Configuration.ModuleID.ToString(),
                                 "action_refresh.gif",
                                 "",
                                 false,
                                 SecurityAccessLevel.View,
                                 true,
                                 false);
                }

                if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Admin, "MANAGE", Configuration))
                {
                    //module movement
                    AddMenuMoveActions();
                }
            }

            if (_moduleGenericActions.Actions.Count > 0)
            {
                _actions.Add(_moduleGenericActions);
            }

            if (_moduleMoveActions != null && _moduleMoveActions.Actions.Count > 0)
            {
                _actions.Add(_moduleMoveActions);
            }

            foreach (ModuleAction action in _moduleGenericActions.Actions)
            {
                if (!UIUtilities.IsLegacyUI(ModuleId, action.ControlKey, PortalId) && action.Url.Contains("ctl"))
                {
                    action.ClientScript = UrlUtils.PopUpUrl(action.Url, _moduleControl as Control, PortalSettings, true, false);
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

        #endregion

        #region Public Methods

        public string EditUrl()
        {
            return EditUrl("", "", "Edit");
        }

        public string EditUrl(string controlKey)
        {
            return EditUrl("", "", controlKey);
        }

        public string EditUrl(string keyName, string keyValue)
        {
            return EditUrl(keyName, keyValue, "Edit");
        }

        public string EditUrl(string keyName, string keyValue, string controlKey)
        {
            var parameters = new string[] { };
            return EditUrl(keyName, keyValue, controlKey, parameters);
        }

        public string EditUrl(string keyName, string keyValue, string controlKey, params string[] additionalParameters)
        {
            string key = controlKey;
            if (string.IsNullOrEmpty(key))
            {
                key = "Edit";
            }
            string moduleIdParam = string.Empty;
            if (Configuration != null)
            {
                moduleIdParam = string.Format("mid={0}", Configuration.ModuleID);
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

            return NavigateUrl(PortalSettings.ActiveTab.TabID, key, false, parameters);
        }

        public string NavigateUrl(int tabID, string controlKey, bool pageRedirect, params string[] additionalParameters)
        {
            return NavigateUrl(tabID, controlKey, Globals.glbDefaultPage, pageRedirect, additionalParameters);
        }

        public string NavigateUrl(int tabID, string controlKey, string pageName, bool pageRedirect, params string[] additionalParameters)
        {
            var isSuperTab = TestableGlobals.Instance.IsHostTab(tabID);
            var settings = PortalController.Instance.GetCurrentPortalSettings();
            var language = Globals.GetCultureCode(tabID, isSuperTab, settings);
            var url = TestableGlobals.Instance.NavigateURL(tabID, isSuperTab, settings, controlKey, language, pageName, additionalParameters);

            // Making URLs call popups
            if (PortalSettings != null && PortalSettings.EnablePopUps)
            {
                if (!UIUtilities.IsLegacyUI(ModuleId, controlKey, PortalId) && (url.Contains("ctl")))
                {
                    url = UrlUtils.PopUpUrl(url, null, PortalSettings, false, pageRedirect);
                }
            }
            return url;
        }

        public int GetNextActionID()
        {
            _nextActionId += 1;
            return _nextActionId;
        }

        #endregion
    }
}
