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
#region Usings

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;

#endregion

namespace DotNetNuke.UI.Containers
{
    public partial class LinkActions : ActionBase
    {
        protected string _itemSeparator = "";

        public string ItemSeparator
        {
            get
            {
                return _itemSeparator;
            }
            set
            {
                _itemSeparator = value;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                {
                    if (ActionRoot.Visible)
                    {
						//Is Root Menu visible?
                        if (Controls.Count > 0)
                        {
                            Controls.Clear();
                        }
                        var PreSpacer = new LiteralControl(ItemSeparator);
                        Controls.Add(PreSpacer);

                        //Add Menu Items
                        foreach (ModuleAction action in ActionRoot.Actions)
                        {
                            if (action.Title == "~")
                            {
								//not supported in this Action object
                            }
                            else
                            {
                                if (action.Visible)
                                {
                                    if ((ModuleControl.ModuleContext.EditMode && Globals.IsAdminControl() == false) ||
                                        (action.Secure != SecurityAccessLevel.Anonymous && action.Secure != SecurityAccessLevel.View))
                                    {
                                        var ModuleActionLink = new LinkButton();
                                        ModuleActionLink.Text = action.Title;
                                        ModuleActionLink.CssClass = "CommandButton";
                                        ModuleActionLink.ID = "lnk" + action.ID;

                                        ModuleActionLink.Click += LinkAction_Click;

                                        Controls.Add(ModuleActionLink);
                                        var Spacer = new LiteralControl(ItemSeparator);
                                        Controls.Add(Spacer);
                                    }
                                }
                            }
                        }
                    }
                }
				
                //Need to determine if this action list actually has any items.
                if (Controls.Count > 0)
                {
                    Visible = true;
                }
                else
                {
                    Visible = false;
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (String.IsNullOrEmpty(_itemSeparator))
            {
                _itemSeparator = "&nbsp;&nbsp;";
            }
        }

        private void LinkAction_Click(object sender, EventArgs e)
        {
            try
            {
                ProcessAction(((LinkButton) sender).ID.Substring(3));
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}