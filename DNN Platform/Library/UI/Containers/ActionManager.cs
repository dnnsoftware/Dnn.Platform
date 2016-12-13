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

#endregion

namespace DotNetNuke.UI.Containers
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.UI.Containers
    /// Class	 : ActionManager
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ActionManager is a helper class that provides common Action Behaviours that can
    /// be used by any IActionControl implementation
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class ActionManager
    {
		#region Private Members

        private readonly PortalSettings PortalSettings = PortalController.Instance.GetCurrentPortalSettings();
        private readonly HttpRequest Request = HttpContext.Current.Request;
        private readonly HttpResponse Response = HttpContext.Current.Response;

		#endregion

		#region Constructors

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new ActionManager
        /// </summary>
        /// -----------------------------------------------------------------------------
        public ActionManager(IActionControl actionControl)
        {
            ActionControl = actionControl;
        }
		
		#endregion

		#region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Action Control that is connected to this ActionManager instance
        /// </summary>
        /// <returns>An IActionControl object</returns>
        /// -----------------------------------------------------------------------------
        public IActionControl ActionControl { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ModuleInstanceContext instance that is connected to this ActionManager 
        /// instance
        /// </summary>
        /// <returns>A ModuleInstanceContext object</returns>
        /// -----------------------------------------------------------------------------
        protected ModuleInstanceContext ModuleContext
        {
            get
            {
                return ActionControl.ModuleControl.ModuleContext;
            }
        }

		#endregion

		#region Private Methods

        private void ClearCache(ModuleAction Command)
        {
			//synchronize cache
            ModuleController.SynchronizeModule(ModuleContext.ModuleId);

            //Redirect to the same page to pick up changes
            Response.Redirect(Request.RawUrl, true);
        }

        private void Delete(ModuleAction Command)
        {
            var module = ModuleController.Instance.GetModule(int.Parse(Command.CommandArgument), ModuleContext.TabId, true);

            //Check if this is the owner instance of a shared module.
            var user = UserController.Instance.GetCurrentUserInfo();
            if (!module.IsShared)
            {
                foreach (ModuleInfo instance in ModuleController.Instance.GetTabModulesByModule(module.ModuleID))
                {
                    if(instance.IsShared)
                    {
                        //HARD Delete Shared Instance
                        ModuleController.Instance.DeleteTabModule(instance.TabID, instance.ModuleID, false);
                        EventLogController.Instance.AddLog(instance, PortalSettings, user.UserID, "", EventLogController.EventLogType.MODULE_DELETED);
                    }
                }
            }

            ModuleController.Instance.DeleteTabModule(ModuleContext.TabId, int.Parse(Command.CommandArgument), true);
            EventLogController.Instance.AddLog(module, PortalSettings, user.UserID, "", EventLogController.EventLogType.MODULE_SENT_TO_RECYCLE_BIN);

            //Redirect to the same page to pick up changes
            Response.Redirect(Request.RawUrl, true);
        }

        private void DoAction(ModuleAction Command)
        {
            if (Command.NewWindow)
            {
                UrlUtils.OpenNewWindow(ActionControl.ModuleControl.Control.Page, GetType(), Command.Url);
            }
            else
            {
                Response.Redirect(Command.Url, true);
            }
        }

        private void Localize(ModuleAction Command)
        {
            ModuleInfo sourceModule = ModuleController.Instance.GetModule(ModuleContext.ModuleId, ModuleContext.TabId, false);

            switch (Command.CommandName)
            {
                case ModuleActionType.LocalizeModule:
                    ModuleController.Instance.LocalizeModule(sourceModule, LocaleController.Instance.GetCurrentLocale(ModuleContext.PortalId));
                    break;
                case ModuleActionType.DeLocalizeModule:
                    ModuleController.Instance.DeLocalizeModule(sourceModule);
                    break;
            }

            // Redirect to the same page to pick up changes
            Response.Redirect(Request.RawUrl, true);
        }

        private void Translate(ModuleAction Command)
        {
            ModuleInfo sourceModule = ModuleController.Instance.GetModule(ModuleContext.ModuleId, ModuleContext.TabId, false);
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
            Response.Redirect(Request.RawUrl, true);
        }

        private void MoveToPane(ModuleAction Command)
        {
            ModuleController.Instance.UpdateModuleOrder(ModuleContext.TabId, ModuleContext.ModuleId, -1, Command.CommandArgument);
            ModuleController.Instance.UpdateTabModuleOrder(ModuleContext.TabId);

            //Redirect to the same page to pick up changes
            Response.Redirect(Request.RawUrl, true);
        }

        private void MoveUpDown(ModuleAction Command)
        {
            switch (Command.CommandName)
            {
                case ModuleActionType.MoveTop:
                    ModuleController.Instance.UpdateModuleOrder(ModuleContext.TabId, ModuleContext.ModuleId, 0, Command.CommandArgument);
                    break;
                case ModuleActionType.MoveUp:
                    ModuleController.Instance.UpdateModuleOrder(ModuleContext.TabId, ModuleContext.ModuleId, ModuleContext.Configuration.ModuleOrder - 3, Command.CommandArgument);
                    break;
                case ModuleActionType.MoveDown:
                    ModuleController.Instance.UpdateModuleOrder(ModuleContext.TabId, ModuleContext.ModuleId, ModuleContext.Configuration.ModuleOrder + 3, Command.CommandArgument);
                    break;
                case ModuleActionType.MoveBottom:
                    ModuleController.Instance.UpdateModuleOrder(ModuleContext.TabId, ModuleContext.ModuleId, (ModuleContext.Configuration.PaneModuleCount * 2) + 1, Command.CommandArgument);
                    break;
            }
            ModuleController.Instance.UpdateTabModuleOrder(ModuleContext.TabId);

            //Redirect to the same page to pick up changes
            Response.Redirect(Request.RawUrl, true);
        }

		#endregion

		#region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DisplayControl determines whether the associated Action control should be 
        /// displayed
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool DisplayControl(DNNNodeCollection objNodes)
        {
            if (objNodes != null && objNodes.Count > 0 && PortalSettings.UserMode != PortalSettings.Mode.View)
            {
                DNNNode objRootNode = objNodes[0];
                if (objRootNode.HasNodes && objRootNode.DNNNodes.Count == 0)
                {
					//if has pending node then display control
                    return true;
                }
                else if (objRootNode.DNNNodes.Count > 0)
                {
					//verify that at least one child is not a break
                    foreach (DNNNode childNode in objRootNode.DNNNodes)
                    {
                        if (!childNode.IsBreak)
                        {
							//Found a child so make Visible
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetAction gets the action associated with the commandName
        /// </summary>
        /// <param name="commandName">The command name</param>
        /// -----------------------------------------------------------------------------
        public ModuleAction GetAction(string commandName)
        {
            return ActionControl.ModuleControl.ModuleContext.Actions.GetActionByCommandName(commandName);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetAction gets the action associated with the id
        /// </summary>
        /// <param name="id">The Id</param>
        /// -----------------------------------------------------------------------------
        public ModuleAction GetAction(int id)
        {
            return ActionControl.ModuleControl.ModuleContext.Actions.GetActionByID(id);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetClientScriptURL gets the client script to attach to the control's client 
        /// side onclick event
        /// </summary>
        /// <param name="action">The Action</param>
        /// <param name="control">The Control</param>
        /// -----------------------------------------------------------------------------
        public void GetClientScriptURL(ModuleAction action, WebControl control)
        {
            if (!String.IsNullOrEmpty(action.ClientScript))
            {
                string Script = action.ClientScript;
                int JSPos = Script.ToLower().IndexOf("javascript:");
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
        /// IsVisible determines whether the action control is Visible
        /// </summary>
        /// <param name="action">The Action</param>
        /// -----------------------------------------------------------------------------
        public bool IsVisible(ModuleAction action)
        {
            bool _IsVisible = false;
            if (action.Visible && ModulePermissionController.HasModuleAccess(action.Secure, Null.NullString, ModuleContext.Configuration))
            {
                if ((ModuleContext.PortalSettings.UserMode == PortalSettings.Mode.Edit) || (action.Secure == SecurityAccessLevel.Anonymous || action.Secure == SecurityAccessLevel.View))
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
        /// ProcessAction processes the action
        /// </summary>
        /// <param name="id">The Id of the Action</param>
        /// -----------------------------------------------------------------------------
        public bool ProcessAction(string id)
        {
            bool bProcessed = true;
            int nid = 0;
            if (Int32.TryParse(id, out nid))
            {
                bProcessed = ProcessAction(ActionControl.ModuleControl.ModuleContext.Actions.GetActionByID(nid));
            }
            return bProcessed;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProcessAction processes the action
        /// </summary>
        /// <param name="action">The Action</param>
        /// -----------------------------------------------------------------------------
        public bool ProcessAction(ModuleAction action)
        {
            bool bProcessed = true;
            switch (action.CommandName)
            {
                case ModuleActionType.ModuleHelp:
                    DoAction(action);
                    break;
                case ModuleActionType.OnlineHelp:
                    DoAction(action);
                    break;
                case ModuleActionType.ModuleSettings:
                    DoAction(action);
                    break;
                case ModuleActionType.DeleteModule:
                    Delete(action);
                    break;
                case ModuleActionType.PrintModule:
                case ModuleActionType.SyndicateModule:
                    DoAction(action);
                    break;
                case ModuleActionType.ClearCache:
                    ClearCache(action);
                    break;
                case ModuleActionType.MovePane:
                    MoveToPane(action);
                    break;
                case ModuleActionType.MoveTop:
                case ModuleActionType.MoveUp:
                case ModuleActionType.MoveDown:
                case ModuleActionType.MoveBottom:
                    MoveUpDown(action);
                    break;
                case ModuleActionType.LocalizeModule:
                    Localize(action);
                    break;
                case ModuleActionType.DeLocalizeModule:
                    Localize(action);
                    break;
                case ModuleActionType.TranslateModule:
                    Translate(action);
                    break;
                case ModuleActionType.UnTranslateModule:
                    Translate(action);
                    break;
                default: //custom action
                    if (!String.IsNullOrEmpty(action.Url) && action.UseActionEvent == false)
                    {
                        DoAction(action);
                    }
                    else
                    {
                        bProcessed = false;
                    }
                    break;
            }
            return bProcessed;
        }
		
		#endregion
    }
}
