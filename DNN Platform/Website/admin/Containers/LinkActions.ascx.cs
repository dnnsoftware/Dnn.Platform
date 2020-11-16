// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;

    public partial class LinkActions : ActionBase
    {
        protected string _itemSeparator = string.Empty;

        public string ItemSeparator
        {
            get
            {
                return this._itemSeparator;
            }

            set
            {
                this._itemSeparator = value;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                {
                    if (this.ActionRoot.Visible)
                    {
                        // Is Root Menu visible?
                        if (this.Controls.Count > 0)
                        {
                            this.Controls.Clear();
                        }

                        var PreSpacer = new LiteralControl(this.ItemSeparator);
                        this.Controls.Add(PreSpacer);

                        // Add Menu Items
                        foreach (ModuleAction action in this.ActionRoot.Actions)
                        {
                            if (action.Title == "~")
                            {
                                // not supported in this Action object
                            }
                            else
                            {
                                if (action.Visible)
                                {
                                    if ((this.ModuleControl.ModuleContext.EditMode && Globals.IsAdminControl() == false) ||
                                        (action.Secure != SecurityAccessLevel.Anonymous && action.Secure != SecurityAccessLevel.View))
                                    {
                                        var ModuleActionLink = new LinkButton();
                                        ModuleActionLink.Text = action.Title;
                                        ModuleActionLink.CssClass = "CommandButton";
                                        ModuleActionLink.ID = "lnk" + action.ID;

                                        ModuleActionLink.Click += this.LinkAction_Click;

                                        this.Controls.Add(ModuleActionLink);
                                        var Spacer = new LiteralControl(this.ItemSeparator);
                                        this.Controls.Add(Spacer);
                                    }
                                }
                            }
                        }
                    }
                }

                // Need to determine if this action list actually has any items.
                if (this.Controls.Count > 0)
                {
                    this.Visible = true;
                }
                else
                {
                    this.Visible = false;
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (string.IsNullOrEmpty(this._itemSeparator))
            {
                this._itemSeparator = "&nbsp;&nbsp;";
            }
        }

        private void LinkAction_Click(object sender, EventArgs e)
        {
            try
            {
                this.ProcessAction(((LinkButton)sender).ID.Substring(3));
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
