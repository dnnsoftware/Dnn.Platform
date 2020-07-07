// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.ControlPanel
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Threading;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Web.UI.WebControls;

    public partial class SwitchSite : UserControl, IDnnRibbonBarTool
    {
        public override bool Visible
        {
            get
            {
                if (PortalSettings.Current.UserId == Null.NullInteger)
                {
                    return false;
                }

                return PortalSettings.Current.UserInfo.IsSuperUser && base.Visible;
            }

            set
            {
                base.Visible = value;
            }
        }

        public string ToolName
        {
            get
            {
                return "QuickSwitchSite";
            }

            set
            {
                throw new NotSupportedException("Set ToolName not supported");
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdSwitch.Click += this.CmdSwitchClick;

            try
            {
                if (this.Visible && !this.IsPostBack)
                {
                    this.LoadPortalsList();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void CmdSwitchClick(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(this.SitesLst.SelectedValue))
                {
                    int selectedPortalID = int.Parse(this.SitesLst.SelectedValue);
                    var portalAliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(selectedPortalID).ToList();

                    if (portalAliases.Count > 0 && (portalAliases[0] != null))
                    {
                        this.Response.Redirect(Globals.AddHTTP(((PortalAliasInfo)portalAliases[0]).HTTPAlias));
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Do nothing we are not logging ThreadAbortxceptions caused by redirects
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }

        private void LoadPortalsList()
        {
            var portals = PortalController.Instance.GetPortals();

            this.SitesLst.ClearSelection();
            this.SitesLst.Items.Clear();

            this.SitesLst.DataSource = portals;
            this.SitesLst.DataTextField = "PortalName";
            this.SitesLst.DataValueField = "PortalID";
            this.SitesLst.DataBind();

            // SitesLst.Items.Insert(0, new ListItem(string.Empty));
            this.SitesLst.InsertItem(0, string.Empty, string.Empty);
        }
    }
}
