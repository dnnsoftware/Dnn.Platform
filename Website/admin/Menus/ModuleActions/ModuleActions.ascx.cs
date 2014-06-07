#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Containers;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;

// ReSharper disable CheckNamespace
namespace DotNetNuke.Admin.Containers
// ReSharper restore CheckNamespace
{
    public partial class ModuleActions : ActionBase
    {
        private List<int> validIDs = new List<int>();
        
        protected string AdminActionsJSON { get; set; }

        protected string AdminText
        {
            get { return Localization.GetString("ModuleGenericActions.Action", Localization.GlobalResourceFile); }
        }

        protected string CustomActionsJSON { get; set; }

        protected string CustomText
        {
            get { return Localization.GetString("ModuleSpecificActions.Action", Localization.GlobalResourceFile); }
        }

        protected string MoveText
        {
            get { return Localization.GetString(ModuleActionType.MoveRoot, Localization.GlobalResourceFile); }
        }

        protected string Panes { get; set; }

        protected bool SupportsMove { get; set; }

        protected bool IsShared { get; set; }

        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, Localization.GlobalResourceFile);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ID = "ModuleActions";

            actionButton.Click += actionButton_Click;

            ClientResourceManager.RegisterStyleSheet(Page, "~/admin/menus/ModuleActions/ModuleActions.css", FileOrder.Css.ModuleCss);
            ClientResourceManager.RegisterScript(Page, "~/admin/menus/ModuleActions/ModuleActions.js");

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
        }

        void actionButton_Click(object sender, EventArgs e)
        {
            ProcessAction(Request.Params["__EVENTARGUMENT"]);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            AdminActionsJSON = "[]";
            CustomActionsJSON = "[]";
            Panes = "[]";
            try
            {
                {
                    if (ActionRoot.Visible)
                    {
                        //Add Menu Items
                        foreach (ModuleAction rootAction in ActionRoot.Actions)
                        {
                            //Process Children
                            var actions = new List<ModuleAction>();
                            foreach (ModuleAction action in rootAction.Actions)
                            {
                                if (action.Visible)
                                {
                                    if ((EditMode && Globals.IsAdminControl() == false) ||
                                        (action.Secure != SecurityAccessLevel.Anonymous && action.Secure != SecurityAccessLevel.View))
                                    {
                                        if (!action.Icon.Contains("://")
                                                && !action.Icon.StartsWith("/")
                                                && !action.Icon.StartsWith("~/"))
                                        {
                                            action.Icon = "~/images/" + action.Icon;
                                        }
                                        if (action.Icon.StartsWith("~/"))
                                        {
                                            action.Icon = Globals.ResolveUrl(action.Icon);
                                        }

                                        actions.Add(action);

                                        if(String.IsNullOrEmpty(action.Url))
                                        {
                                            validIDs.Add(action.ID);
                                        }
                                    }
                                }

                            }

                            var oSerializer = new JavaScriptSerializer();
                            if (rootAction.Title == Localization.GetString("ModuleGenericActions.Action", Localization.GlobalResourceFile))
                            {
                                AdminActionsJSON = oSerializer.Serialize(actions);
                            }
                            else
                            {
                                if (rootAction.Title == Localization.GetString("ModuleSpecificActions.Action", Localization.GlobalResourceFile))
                                {
                                    CustomActionsJSON = oSerializer.Serialize(actions);
                                }
                                else
                                {
                                    SupportsMove = (actions.Count > 0);
                                    Panes = oSerializer.Serialize(PortalSettings.ActiveTab.Panes);
                                }
                            }
                        }
                        IsShared = PortalGroupController.Instance.IsModuleShared(ModuleContext.ModuleId, PortalController.Instance.GetPortal(PortalSettings.PortalId));
                    }
                }

            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            foreach(int id in validIDs)
            {
                Page.ClientScript.RegisterForEventValidation(actionButton.UniqueID, id.ToString());
            }
        }
    }    
}
