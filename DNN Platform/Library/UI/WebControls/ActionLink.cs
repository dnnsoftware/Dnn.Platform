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
            RequireEditMode = false;
            Security = "Edit";
            ControlKey = "";
            Title = "";
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
            if (ModulePermissionController.HasModuleAccess(security, Null.NullString, ModuleControl.ModuleContext.Configuration))
            {
                if ((RequireEditMode != true || ModuleControl.ModuleContext.PortalSettings.UserMode == PortalSettings.Mode.Edit) || (security == SecurityAccessLevel.Anonymous || security == SecurityAccessLevel.View))
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
            EnableViewState = false;
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnPreRender runs when just before the Render phase of the Page Lifecycle
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Visible && IsVisible((SecurityAccessLevel)Enum.Parse(typeof(SecurityAccessLevel), Security)))
            {
                Text = Title;
                NavigateUrl = ControlKey != "" 
                                ? ModuleControl.ModuleContext.EditUrl(KeyName, KeyValue, ControlKey) 
                                : ModuleControl.ModuleContext.EditUrl(Title);

                if (CssClass == "")
                {
                    CssClass = "dnnPrimaryAction";
                }
            }
            else
            {
                Visible = false;
            }
        }

        #endregion
    }
}
