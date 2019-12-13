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
