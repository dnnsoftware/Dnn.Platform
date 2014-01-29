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
using System.Collections;
using System.Linq;
using System.Threading;
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Portals.Internal;
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
                    var portalAliases = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(selectedPortalID).ToList();

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
            var portalCtrl = new PortalController();
            ArrayList portals = portalCtrl.GetPortals();

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