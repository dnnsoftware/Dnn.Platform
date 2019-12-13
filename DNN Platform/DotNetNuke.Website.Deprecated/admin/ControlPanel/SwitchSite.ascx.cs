#region Usings

using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Web.UI.WebControls;

#endregion

namespace DotNetNuke.UI.ControlPanel
{
    using System.Web.UI.WebControls;

    public partial class SwitchSite : UserControl, IDnnRibbonBarTool
    {
        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdSwitch.Click += CmdSwitchClick;

            try
            {
                if (Visible && !IsPostBack)
                {
                    LoadPortalsList();
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
                if ((!string.IsNullOrEmpty(SitesLst.SelectedValue)))
                {
                    int selectedPortalID = int.Parse(SitesLst.SelectedValue);
                    var portalAliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(selectedPortalID).ToList();

                    if ((portalAliases.Count > 0 && (portalAliases[0] != null)))
                    {
                        Response.Redirect(Globals.AddHTTP(((PortalAliasInfo) portalAliases[0]).HTTPAlias));
                    }
                }
            }
            catch(ThreadAbortException)
            {
              //Do nothing we are not logging ThreadAbortxceptions caused by redirects      
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }

        #endregion

        #region Properties

        public override bool Visible
        {
            get
            {
                if ((PortalSettings.Current.UserId == Null.NullInteger))
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

        #endregion

        #region Methods

        private void LoadPortalsList()
        {
            var portals = PortalController.Instance.GetPortals();

            SitesLst.ClearSelection();
            SitesLst.Items.Clear();

            SitesLst.DataSource = portals;
            SitesLst.DataTextField = "PortalName";
            SitesLst.DataValueField = "PortalID";
            SitesLst.DataBind();

            //SitesLst.Items.Insert(0, new ListItem(string.Empty));
            SitesLst.InsertItem(0, string.Empty, string.Empty);
        }

        #endregion
    }
}
