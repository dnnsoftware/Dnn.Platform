// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Class    : Containers.Icon
    ///
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Contains the attributes of an Icon.
    /// These are read into the PortalModuleBase collection as attributes for the icons within the module controls.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public partial class PrintModule : ActionBase
    {
        public string PrintIcon { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                foreach (ModuleAction action in this.Actions)
                {
                    this.DisplayAction(action);
                }

                // set visibility
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

        private void DisplayAction(ModuleAction action)
        {
            if (action.CommandName == ModuleActionType.PrintModule)
            {
                if (action.Visible)
                {
                    if ((this.PortalSettings.UserMode == PortalSettings.Mode.Edit) || (action.Secure == SecurityAccessLevel.Anonymous || action.Secure == SecurityAccessLevel.View))
                    {
                        if (this.ModuleContext.Configuration.DisplayPrint)
                        {
                            var ModuleActionIcon = new ImageButton();
                            if (!string.IsNullOrEmpty(this.PrintIcon))
                            {
                                ModuleActionIcon.ImageUrl = this.ModuleContext.Configuration.ContainerPath.Substring(0, this.ModuleContext.Configuration.ContainerPath.LastIndexOf("/") + 1) + this.PrintIcon;
                            }
                            else
                            {
                                ModuleActionIcon.ImageUrl = "~/images/" + action.Icon;
                            }

                            ModuleActionIcon.ToolTip = action.Title;
                            ModuleActionIcon.ID = "ico" + action.ID;
                            ModuleActionIcon.CausesValidation = false;

                            ModuleActionIcon.Click += this.IconAction_Click;

                            this.Controls.Add(ModuleActionIcon);
                        }
                    }
                }
            }

            foreach (ModuleAction subAction in action.Actions)
            {
                this.DisplayAction(subAction);
            }
        }

        private void IconAction_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                this.ProcessAction(((ImageButton)sender).ID.Substring(3));
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
