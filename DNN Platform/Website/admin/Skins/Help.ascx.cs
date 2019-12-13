#region Usings

using System;

using DotNetNuke.Entities.Host;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <returns></returns>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    public partial class Help : SkinObjectBase
    {
        public string CssClass { get; set; }

        private void InitializeComponent()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (!String.IsNullOrEmpty(CssClass))
                {
                    hypHelp.CssClass = CssClass;
                }
                if (Request.IsAuthenticated)
                {
                    if (TabPermissionController.CanAdminPage())
                    {
                        hypHelp.Text = Localization.GetString("Help");
                        hypHelp.NavigateUrl = "mailto:" + Host.HostEmail + "?subject=" + PortalSettings.PortalName + " Support Request";
                        hypHelp.Visible = true;
                    }
                    else
                    {
                        hypHelp.Text = Localization.GetString("Help");
                        hypHelp.NavigateUrl = "mailto:" + PortalSettings.Email + "?subject=" + PortalSettings.PortalName + " Support Request";
                        hypHelp.Visible = true;
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
