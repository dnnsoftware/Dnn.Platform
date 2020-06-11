// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.UI.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.UI.WebControls
    /// Class	 : ActionLink
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ActionLink provides a button for a single action.
    /// </summary>
    /// <remarks>
    /// ActionBase inherits from HyperLink
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ActionLink : HyperLink
    {
        #region Private Members

        public ActionLink()
        {
            this.RequireEditMode = false;
            this.Security = "Edit";
            this.ControlKey = "";
            this.Title = "";
        }

        #endregion

        #region Public Properties

        public string Title { get; set; }

        public string ControlKey { get; set; }

        public string KeyName { get; set; }

        public string KeyValue { get; set; }

        public string Security { get; set; }

        public bool RequireEditMode { get; set; }

        public IModuleControl ModuleControl { get; set; }

        #endregion

        #region Private Methods

        private bool IsVisible(SecurityAccessLevel security)
        {
            bool isVisible = false;
            if (ModulePermissionController.HasModuleAccess(security, Null.NullString, this.ModuleControl.ModuleContext.Configuration))
            {
                if ((this.RequireEditMode != true || this.ModuleControl.ModuleContext.PortalSettings.UserMode == PortalSettings.Mode.Edit) || (security == SecurityAccessLevel.Anonymous || security == SecurityAccessLevel.View))
                {
                    isVisible = true;
                }
            }

            return isVisible;
        }

        #endregion

        #region Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateChildControls builds the control tree
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void CreateChildControls()
        {
            //Call base class method to ensure Control Tree is built
            base.CreateChildControls();

            //Set Causes Validation and Enables ViewState to false
            this.EnableViewState = false;
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnPreRender runs when just before the Render phase of the Page Lifecycle
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (this.Visible && this.IsVisible((SecurityAccessLevel)Enum.Parse(typeof(SecurityAccessLevel), this.Security)))
            {
                this.Text = this.Title;
                this.NavigateUrl = this.ControlKey != "" 
                                ? this.ModuleControl.ModuleContext.EditUrl(this.KeyName, this.KeyValue, this.ControlKey) 
                                : this.ModuleControl.ModuleContext.EditUrl(this.Title);

                if (this.CssClass == "")
                {
                    this.CssClass = "dnnPrimaryAction";
                }
            }
            else
            {
                this.Visible = false;
            }
        }

        #endregion
    }
}
