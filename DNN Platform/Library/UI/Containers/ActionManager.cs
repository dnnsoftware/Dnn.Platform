// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;
    using System.Linq;
    using System.Web;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.WebControls;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.UI.Containers
    /// Class    : ActionManager
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ActionManager is a helper class that provides common Action Behaviours that can
    /// be used by any IActionControl implementation.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class ActionManager
    {
        private readonly PortalSettings PortalSettings = PortalController.Instance.GetCurrentPortalSettings();
        private readonly HttpRequest Request = HttpContext.Current.Request;
        private readonly HttpResponse Response = HttpContext.Current.Response;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionManager"/> class.
        /// Constructs a new ActionManager.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public ActionManager(IActionControl actionControl)
        {
            this.ActionControl = actionControl;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Action Control that is connected to this ActionManager instance.
        /// </summary>
        /// <returns>An IActionControl object.</returns>
        /// -----------------------------------------------------------------------------
        public IActionControl ActionControl { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ModuleInstanceContext instance that is connected to this ActionManager
        /// instance.
        /// </summary>
        /// <returns>A ModuleInstanceContext object.</returns>
        /// -----------------------------------------------------------------------------
        protected ModuleInstanceContext ModuleContext
        {
            get
            {
                return this.ActionControl.ModuleControl.ModuleContext;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DisplayControl determines whether the associated Action control should be
        /// displayed.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public bool DisplayControl(DNNNodeCollection objNodes)
        {
            if (objNodes != null && objNodes.Count > 0 && this.PortalSettings.UserMode != PortalSettings.Mode.View)
            {
                DNNNode objRootNode = objNodes[0];
                if (objRootNode.HasNodes && objRootNode.DNNNodes.Count == 0)
                {
                    // if has pending node then display control
                    return true;
                }
                else if (objRootNode.DNNNodes.Count > 0)
                {
                    // verify that at least one child is not a break
                    foreach (DNNNode childNode in objRootNode.DNNNodes)
                    {
                        if (!childNode.IsBreak)
                        {
                            // Found a child so make Visible
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetAction gets the action associated with the commandName.
        /// </summary>
        /// <param name="commandName">The command name.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public ModuleAction GetAction(string commandName)
        {
            return this.ActionControl.ModuleControl.ModuleContext.Actions.GetActionByCommandName(commandName);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetAction gets the action associated with the id.
        /// </summary>
        /// <param name="id">The Id.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public ModuleAction GetAction(int id)
        {
            return this.ActionControl.ModuleControl.ModuleContext.Actions.GetActionByID(id);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetClientScriptURL gets the client script to attach to the control's client
        /// side onclick event.
        /// </summary>
        /// <param name="action">The Action.</param>
        /// <param name="control">The Control.</param>
        /// -----------------------------------------------------------------------------
        public void GetClientScriptURL(ModuleAction action, WebControl control)
        {
            if (!string.IsNullOrEmpty(action.ClientScript))
            {
                string Script = action.ClientScript;
                int JSPos = Script.IndexOf("javascript:", StringComparison.InvariantCultureIgnoreCase);
                if (JSPos > -1)
                {
                    Script = Script.Substring(JSPos + 11);
                }

                string FormatScript = "javascript: return {0};";
                control.Attributes.Add("onClick", string.Format(FormatScript, Script));
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// IsVisible determines whether the action control is Visible.
        /// </summary>
        /// <param name="action">The Action.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public bool IsVisible(ModuleAction action)
        {
            bool _IsVisible = false;
            if (action.Visible && ModulePermissionController.HasModuleAccess(action.Secure, Null.NullString, this.ModuleContext.Configuration))
            {
                if ((this.ModuleContext.PortalSettings.UserMode == PortalSettings.Mode.Edit) || (action.Secure == SecurityAccessLevel.Anonymous || action.Secure == SecurityAccessLevel.View))
                {
                    _IsVisible = true;
                }
                else
                {
                    _IsVisible = false;
                }
            }
            else
            {
                _IsVisible = false;
            }

            return _IsVisible;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProcessAction processes the action.
        /// </summary>
        /// <param name="id">The Id of the Action.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public bool ProcessAction(string id)
        {
            bool bProcessed = true;
            int nid = 0;
            if (int.TryParse(id, out nid))
            {
                bProcessed = this.ProcessAction(this.ActionControl.ModuleControl.ModuleContext.Actions.GetActionByID(nid));
            }

            return bProcessed;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProcessAction processes the action.
        /// </summary>
        /// <param name="action">The Action.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public bool ProcessAction(ModuleAction action)
        {
            bool bProcessed = true;
            switch (action.CommandName)
            {
                case ModuleActionType.ModuleHelp:
                    this.DoAction(action);
                    break;
                case ModuleActionType.OnlineHelp:
                    this.DoAction(action);
                    break;
                case ModuleActionType.ModuleSettings:
                    this.DoAction(action);
                    break;
                case ModuleActionType.DeleteModule:
                    this.Delete(action);
                    break;
                case ModuleActionType.PrintModule:
                case ModuleActionType.SyndicateModule:
                    this.DoAction(action);
                    break;
                case ModuleActionType.ClearCache:
                    this.ClearCache(action);
                    break;
                case ModuleActionType.MovePane:
                    this.MoveToPane(action);
                    break;
                case ModuleActionType.MoveTop:
                case ModuleActionType.MoveUp:
                case ModuleActionType.MoveDown:
                case ModuleActionType.MoveBottom:
                    this.MoveUpDown(action);
                    break;
                case ModuleActionType.LocalizeModule:
                    this.Localize(action);
                    break;
                case ModuleActionType.DeLocalizeModule:
                    this.Localize(action);
                    break;
                case ModuleActionType.TranslateModule:
                    this.Translate(action);
                    break;
                case ModuleActionType.UnTranslateModule:
                    this.Translate(action);
                    break;
                default: // custom action
                    if (!string.IsNullOrEmpty(action.Url) && action.UseActionEvent == false)
                    {
                        this.DoAction(action);
                    }
                    else
                    {
                        bProcessed = false;
                    }

                    break;
            }

            return bProcessed;
        }

        private void ClearCache(ModuleAction Command)
        {
            // synchronize cache
            ModuleController.SynchronizeModule(this.ModuleContext.ModuleId);

            // Redirect to the same page to pick up changes
            this.Response.Redirect(this.Request.RawUrl, true);
        }

        private void Delete(ModuleAction Command)
        {
            var module = ModuleController.Instance.GetModule(int.Parse(Command.CommandArgument), this.ModuleContext.TabId, true);

            // Check if this is the owner instance of a shared module.
            var user = UserController.Instance.GetCurrentUserInfo();
            if (!module.IsShared)
            {
                foreach (ModuleInfo instance in ModuleController.Instance.GetTabModulesByModule(module.ModuleID))
                {
                    if (instance.IsShared)
                    {
                        // HARD Delete Shared Instance
                        ModuleController.Instance.DeleteTabModule(instance.TabID, instance.ModuleID, false);
                        EventLogController.Instance.AddLog(instance, this.PortalSettings, user.UserID, string.Empty, EventLogController.EventLogType.MODULE_DELETED);
                    }
                }
            }

            ModuleController.Instance.DeleteTabModule(this.ModuleContext.TabId, int.Parse(Command.CommandArgument), true);
            EventLogController.Instance.AddLog(module, this.PortalSettings, user.UserID, string.Empty, EventLogController.EventLogType.MODULE_SENT_TO_RECYCLE_BIN);

            // Redirect to the same page to pick up changes
            this.Response.Redirect(this.Request.RawUrl, true);
        }

        private void DoAction(ModuleAction Command)
        {
            if (Command.NewWindow)
            {
                UrlUtils.OpenNewWindow(this.ActionControl.ModuleControl.Control.Page, this.GetType(), Command.Url);
            }
            else
            {
                this.Response.Redirect(Command.Url, true);
            }
        }

        private void Localize(ModuleAction Command)
        {
            ModuleInfo sourceModule = ModuleController.Instance.GetModule(this.ModuleContext.ModuleId, this.ModuleContext.TabId, false);

            switch (Command.CommandName)
            {
                case ModuleActionType.LocalizeModule:
                    ModuleController.Instance.LocalizeModule(sourceModule, LocaleController.Instance.GetCurrentLocale(this.ModuleContext.PortalId));
                    break;
                case ModuleActionType.DeLocalizeModule:
                    ModuleController.Instance.DeLocalizeModule(sourceModule);
                    break;
            }

            // Redirect to the same page to pick up changes
            this.Response.Redirect(this.Request.RawUrl, true);
        }

        private void Translate(ModuleAction Command)
        {
            ModuleInfo sourceModule = ModuleController.Instance.GetModule(this.ModuleContext.ModuleId, this.ModuleContext.TabId, false);
            switch (Command.CommandName)
            {
                case ModuleActionType.TranslateModule:
                    ModuleController.Instance.UpdateTranslationStatus(sourceModule, true);
                    break;
                case ModuleActionType.UnTranslateModule:
                    ModuleController.Instance.UpdateTranslationStatus(sourceModule, false);
                    break;
            }

            // Redirect to the same page to pick up changes
            this.Response.Redirect(this.Request.RawUrl, true);
        }

        private void MoveToPane(ModuleAction Command)
        {
            ModuleController.Instance.UpdateModuleOrder(this.ModuleContext.TabId, this.ModuleContext.ModuleId, -1, Command.CommandArgument);
            ModuleController.Instance.UpdateTabModuleOrder(this.ModuleContext.TabId);

            // Redirect to the same page to pick up changes
            this.Response.Redirect(this.Request.RawUrl, true);
        }

        private void MoveUpDown(ModuleAction Command)
        {
            switch (Command.CommandName)
            {
                case ModuleActionType.MoveTop:
                    ModuleController.Instance.UpdateModuleOrder(this.ModuleContext.TabId, this.ModuleContext.ModuleId, 0, Command.CommandArgument);
                    break;
                case ModuleActionType.MoveUp:
                    ModuleController.Instance.UpdateModuleOrder(this.ModuleContext.TabId, this.ModuleContext.ModuleId, this.ModuleContext.Configuration.ModuleOrder - 3, Command.CommandArgument);
                    break;
                case ModuleActionType.MoveDown:
                    ModuleController.Instance.UpdateModuleOrder(this.ModuleContext.TabId, this.ModuleContext.ModuleId, this.ModuleContext.Configuration.ModuleOrder + 3, Command.CommandArgument);
                    break;
                case ModuleActionType.MoveBottom:
                    ModuleController.Instance.UpdateModuleOrder(this.ModuleContext.TabId, this.ModuleContext.ModuleId, (this.ModuleContext.Configuration.PaneModuleCount * 2) + 1, Command.CommandArgument);
                    break;
            }

            ModuleController.Instance.UpdateTabModuleOrder(this.ModuleContext.TabId);

            // Redirect to the same page to pick up changes
            this.Response.Redirect(this.Request.RawUrl, true);
        }
    }
}
